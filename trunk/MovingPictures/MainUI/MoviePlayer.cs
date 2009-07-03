using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Cornerstone.Database.CustomTypes;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Util;
using MediaPortal.InputDevices;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.MainUI {

    public delegate void MoviePlayerEvent(DBMovieInfo movie);

    public class MoviePlayer {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        public enum MoviePlayerState { Idle, Processing, Playing }

        #region Private variables

        private MovingPicturesGUI _gui;
        private bool customIntroPlayed = false;
        private bool mountedPlayback = false;
        private bool listenToExternalPlayerEvents = true;
        private DBLocalMedia _queuedMedia;
        private int _activePart;
        private bool _resumeActive = false;

        #endregion

        #region Events

        public event MoviePlayerEvent MovieStarted;
        public event MoviePlayerEvent MovieStopped;
        public event MoviePlayerEvent MovieEnded;

        #endregion

        #region Ctor

        public MoviePlayer(MovingPicturesGUI gui) {
            _gui = gui;

            // external player handlers
            Util.Utils.OnStartExternal += new Util.Utils.UtilEventHandler(onStartExternal);
            Util.Utils.OnStopExternal += new Util.Utils.UtilEventHandler(onStopExternal);

            // default player handlers
            g_Player.PlayBackStarted += new g_Player.StartedHandler(onPlaybackStarted);
            g_Player.PlayBackEnded += new g_Player.EndedHandler(onPlayBackEnded);
            g_Player.PlayBackStopped += new g_Player.StoppedHandler(onPlayBackStoppedOrChanged);
            
            try {
                // This is a handler added in RC4 - if we are using an older mediaportal version
                // this would throw an exception.
                g_Player.PlayBackChanged += new g_Player.ChangedHandler(onPlayBackStoppedOrChanged);
            }
            catch (Exception) {
                logger.Warn("Running MediaPortal 1.0 RC3 or earlier. Unexpected behavior may occur when starting playback of a new movie without stopping previous movie. Please upgrade for better performance.");
            }

            logger.Info("Movie Player initialized.");
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Get a value indicating that a movie is being played
        /// </summary>
        
        public bool IsPlaying {
            get {
                return (_playerState != MoviePlayerState.Idle);
            }
        } 

        public MoviePlayerState State {
            get {
                return _playerState;
            }
        } private MoviePlayerState _playerState = MoviePlayerState.Idle;

        /// <summary>
        /// Gets the currently playing movie
        /// </summary>
        public DBMovieInfo CurrentMovie {
            get {
                if (_activeMovie != null && _playerState == MoviePlayerState.Playing)
                    return _activeMovie;

                return null;
            }
        } private DBMovieInfo _activeMovie;

        /// <summary>
        /// Gets the currently playing local media object
        /// </summary>
        public DBLocalMedia CurrentMedia {
            get {
                if (_activeMedia != null && _playerState == MoviePlayerState.Playing)
                    return _activeMedia;

                return null;
            }
        }

        private DBLocalMedia activeMedia {
            get {
                return _activeMedia;
            }
            set {
                _activeMedia = value;
                if (_activeMedia != null) {
                    _activeMovie = _activeMedia.AttachedMovies[0];
                    _activePart = _activeMedia.Part;
                }
                else {
                    _activeMovie = null;
                    _activePart = 0;
                }
            }
        } private DBLocalMedia _activeMedia;

        #endregion

        #region Public Methods

        public void Play(DBMovieInfo movie) {
            Play(movie, 1);
        }

        public void Play(DBMovieInfo movie, int part) {
            // set player state working
            _playerState = MoviePlayerState.Processing;
            
            // queue the local media object in case we first need to play the custom intro
            // we can get back to it later.
            _queuedMedia = movie.LocalMedia[part-1];
            
            // try playing our custom intro (if present). If successful quit, as we need to
            // wait for the intro to finish.
            bool success = playCustomIntro();
            if (success) return;

            // Start movie
            playMovie(movie, part);
        }

        public void Stop() {
            if (g_Player.Player.Playing)
                g_Player.Stop();
            
            resetPlayer();
        }

        #endregion

        #region Playback logic

        private void playMovie(DBMovieInfo movie, int requestedPart) {
            logger.Debug("playMovie()");
            _playerState = MoviePlayerState.Processing;

            if (movie == null || requestedPart > movie.LocalMedia.Count || requestedPart < 1) {
                resetPlayer();
                return;
            }

            logger.Debug("Request: Movie='{0}', Part={1}", movie.Title, requestedPart);
            for (int i = 0; i < movie.LocalMedia.Count; i++) {
                logger.Debug("LocalMedia[{0}] = {1}  Duration = {2}", i, movie.LocalMedia[i].FullPath, movie.LocalMedia[i].Duration);
            }

            int part = requestedPart;

            // if this is a request to start the movie from the begining, check if we should resume
            // or prompt the user for disk selection
            if (requestedPart == 1) {
                // check if we should be resuming, and if not, clear resume data
                _resumeActive = PromptUserToResume(movie);
                if (_resumeActive)
                    part = movie.ActiveUserSettings.ResumePart;
                else
                    clearMovieResumeState(movie);

                // if we have a multi-part movie composed of disk images and we are not resuming 
                // ask which part the user wants to play
                string firstExtension = movie.LocalMedia[0].File.Extension;
                if (!_resumeActive && movie.LocalMedia.Count > 1 && (DaemonTools.IsImageFile(firstExtension) || movie.LocalMedia[0].IsVideoDisc)) {
                    GUIDialogFileStacking dlg = (GUIDialogFileStacking)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_FILESTACKING);
                    if (null != dlg) {
                        dlg.SetNumberOfFiles(movie.LocalMedia.Count);
                        dlg.DoModal(GUIWindowManager.ActiveWindow);
                        part = dlg.SelectedFile;
                        if (part < 1) {
                            resetPlayer();
                            return;
                        }
                    }
                }
            }

            DBLocalMedia mediaToPlay = movie.LocalMedia[part - 1];
            string videoPath;
            MediaStatus mediaStatus = mediaToPlay.GetVideoPath(out videoPath);
            while (mediaStatus != MediaStatus.Online) {
                bool exitPlayer = false;
                switch (mediaStatus) {
                    case MediaStatus.Removed:
                        _gui.ShowMessage("Error", Translation.MediaIsMissing);
                        exitPlayer = true;
                        break;
                    case MediaStatus.Offline:
                        string bodyString = String.Format(Translation.MediaNotAvailableBody, mediaToPlay.MediaLabel);
                        exitPlayer = _gui.ShowCustomYesNo(Translation.MediaNotAvailableHeader, bodyString,
                        Translation.Retry, Translation.Cancel, true);
                        // Special debug line to troubleshoot availability issues
                        logger.Debug("Media not available: Path={0}, DriveType={1}, Serial={2}, ExpectedSerial={3}",
                            mediaToPlay.FullPath, mediaToPlay.ImportPath.GetDriveType().ToString(),
                            mediaToPlay.ImportPath.GetDiskSerial(), mediaToPlay.VolumeSerial);

                        break;
                    case MediaStatus.MountError:
                        _gui.ShowMessage(Translation.Error, Translation.FailedMountingImage);
                        exitPlayer = true;
                        break;
                    case MediaStatus.MountDriveNotReady:
                        exitPlayer = !_gui.ShowCustomYesNo(Translation.VirtualDriveHeader, Translation.VirtualDriveMessage, Translation.Retry, Translation.Cancel, true);
                        break;
                    default:
                        exitPlayer = true;
                        break;
                }

                // when needed exit the playback logic
                if (exitPlayer) {
                    resetPlayer();
                    return;
                }
            }            

            // Flag that we are playing back mounted media.
            // note: if the media is an image and we reach this logic
            // it should be mounted properly.
            if (mediaToPlay.IsImageFile)
                mountedPlayback = true;
            else
                mountedPlayback = false;

            // if we do not have MediaInfo but have the AutoRetrieveMediaInfo setting toggled
            // get the media info
            if (!mediaToPlay.HasMediaInfo && MovingPicturesCore.Settings.AutoRetrieveMediaInfo) {
                mediaToPlay.UpdateMediaInfo();
                mediaToPlay.Commit();
            }

            // store the current media object so we can request it later
            _queuedMedia = mediaToPlay;

            // start playback
            logger.Info("Playing: Movie='{0}' FullPath='{1}', VideoPath='{2}', Mounted={3})", movie.Title, mediaToPlay.FullPath, videoPath, mountedPlayback.ToString());
            playFile(videoPath);            
        }

        private bool playCustomIntro() {
            // Check if we have already played a custom intro
            if (!customIntroPlayed) {
                DBMovieInfo queuedMovie = _queuedMedia.AttachedMovies[0];
                // Only play custom intro for we are not resuming
                if (queuedMovie.UserSettings == null || queuedMovie.UserSettings.Count == 0 || queuedMovie.ActiveUserSettings.ResumeTime == 0) {
                    string custom_intro = MovingPicturesCore.Settings.CustomIntroLocation;

                    // Check if the custom intro is specified by user and exists
                    if (custom_intro.Length > 0 && File.Exists(custom_intro)) {
                        logger.Debug("Playing Custom Intro: {0}", custom_intro);

                        // we set this variable before we start the actual playback
                        // because playFile to account for the blocking nature of the
                        // mediaportal external player logic
                        customIntroPlayed = true;

                        // start playback
                        playFile(custom_intro);
                        return true;
                    }
                }
            }

            return false;
        }

        // start playback of a video file
        private void playFile(string media) {
            logger.Debug("Processing media for playback: File={0}", media);
            VideoDiscFormat videoFormat = Utility.GetVideoDiscFormat(media);
                        
            // HD Playback
            if (videoFormat == VideoDiscFormat.Bluray || videoFormat == VideoDiscFormat.HDDVD) {

                // Take proper action according to playback setting
                bool hdExternal = MovingPicturesCore.Settings.UseExternalPlayer;

                // Launch external player if user has configured it for HD playback.
                if (hdExternal) {
                    LaunchHDPlayer(media);
                    return;
                }

                // Alternate playback HD content (without menu)
                string newMedia = Utility.GetMainFeatureStreamFromVideoDisc(media, videoFormat);
                if (newMedia != null) {
                    // Check if the stream extension is in the mediaportal extension list.
                    if (Utility.IsMediaPortalVideoFile(newMedia)) {
                        media = newMedia;
                    }
                    else {
                        // Show a dialog to the user that explains how to configure the alternate playback
                        string ext = (videoFormat == VideoDiscFormat.Bluray) ? ".M2TS" : ".EVO";
                        logger.Info("HD Playback: extension '{0}' is missing from the mediaportal configuration.", ext);
                        _gui.ShowMessage(Translation.PlaybackFailedHeader, String.Format(Translation.PlaybackFailed, ext));
                        resetPlayer();
                        return;
                    }
                }

                logger.Info("HD Playback: Internal, Media={0}", media);
            }

            // Todo: do these two lines still make sense?
            GUIGraphicsContext.IsFullScreenVideo = true;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
            
            // We start listening to external player events
            listenToExternalPlayerEvents = true;
            
            // Play the file using the mediaportal player
            bool success = g_Player.Play(media);

            // We stop listening to external player events
            listenToExternalPlayerEvents = false;

            // if the playback did not happen, reset the player
            if (!success) {
                logger.Info("Playback failed: Media={0}", media);
                resetPlayer();
            }
        }

        #endregion

        #region External HD Player

        // This method launches an external HD player controlled by Moving Pictures
        // Eventually when Mediaportal has a native solution for HD video disc formats
        // this will be not needed anymore.
        private void LaunchHDPlayer(string videoPath) {
            logger.Info("HD Playback: Launching external player.");

            // First check if the user supplied executable for the external player is valid
            string execPath = MovingPicturesCore.Settings.ExternalPlayerExecutable;
            if (!File.Exists(execPath)) {
                // if it's not show a dialog explaining the error
                _gui.ShowMessage("Error", Translation.MissingExternalPlayerExe);
                logger.Info("HD Playback: The external player executable '{0}' is missing.", execPath);
                // do nothing
                resetPlayer();
                return;
            }

            // process the argument string and replace the 'filename' variable
            string arguments = MovingPicturesCore.Settings.ExternalPlayerArguements;
            string videoRoot = Utility.GetMovieBaseDirectory(new FileInfo(videoPath).Directory).FullName;
            string filename = Utility.IsDriveRoot(videoRoot) ? videoRoot : videoPath;
            string fps = ((int)(_queuedMedia.VideoFrameRate + 0.5f)).ToString();
            arguments = arguments.Replace("%filename%", filename);
            arguments = arguments.Replace("%fps%", fps);

            logger.Debug("External Player: Video='{0}', FPS={1}, ExecCommandLine={2} {3}", filename, fps, execPath, arguments);

            // Setup the external player process
            ProcessStartInfo processinfo = new ProcessStartInfo();
            processinfo.FileName = execPath;
            processinfo.Arguments = arguments;

            Process hdPlayer = new Process();
            hdPlayer.StartInfo = processinfo;
            hdPlayer.Exited += OnHDPlayerExited;
            hdPlayer.EnableRaisingEvents = true;

            try {
                // start external player process
                hdPlayer.Start();
                
                // disable mediaportal input devices
                InputDevices.InputDevices.Stop();

                // hide mediaportal and suspend rendering to save resources for the external player
                GUIGraphicsContext.BlankScreen = true;
                GUIGraphicsContext.form.Hide();
                GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.SUSPENDING;

                logger.Info("HD Playback: External player started.");
                onMediaStarted(_queuedMedia);
            }
            catch (Exception e) {
                logger.ErrorException("HD Playback: Could not start the external player process.", e);
                resetPlayer();
            }
        }

        private void OnHDPlayerExited(object obj, EventArgs e) {
            
            // enable mediaportal input devices
            InputDevices.InputDevices.Init();

            // show mediaportal and start rendering
            GUIGraphicsContext.BlankScreen = false;
            GUIGraphicsContext.form.Show();
            GUIGraphicsContext.ResetLastActivity();
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GETFOCUS, 0, 0, 0, 0, 0, null);
            GUIWindowManager.SendThreadMessage(msg);
            GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;
            logger.Info("HD Playback: The external player has exited.");

            // call the logic for when an external player exits 
            onExternalExit();
        }

        #endregion

        #region Internal Player Event Handlers

        private void onPlaybackStarted(g_Player.MediaType type, string filename) {
            if (_playerState == MoviePlayerState.Processing && g_Player.Player.Playing) {
                logger.Info("Playback Started: Internal, File={0}", filename);

                // get the duration of the media 
                updateMediaDuration(_queuedMedia);

                // get the movie
                DBMovieInfo movie = _queuedMedia.AttachedMovies[0];

                // and jump to our resume position if necessary
                if (_resumeActive) {
                    if (g_Player.IsDVD) {
                        logger.Debug("Resume: DVD state.");
                        g_Player.Player.SetResumeState(movie.ActiveUserSettings.ResumeData.Data);
                    }
                    else {
                        logger.Debug("Resume: Time={0}", movie.ActiveUserSettings.ResumeTime);
                        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SEEK_POSITION, 0, 0, 0, 0, 0, null);
                        msg.Param1 = movie.ActiveUserSettings.ResumeTime;
                        GUIGraphicsContext.SendMessage(msg);
                    }
                    // deactivate resume
                    _resumeActive = false;
                }

                // Trigger Movie started
                onMediaStarted(_queuedMedia);
            }
        }

        private void onPlayBackStoppedOrChanged(g_Player.MediaType type, int timeMovieStopped, string filename) {
            if (type != g_Player.MediaType.Video || _playerState != MoviePlayerState.Playing)
                return;

            logger.Debug("OnPlayBackStoppedOrChanged: File={0}, Movie={1}, Part={2}, TimeStopped={3}", filename, _activeMovie.Title, _activePart, timeMovieStopped);

            // Because we can't get duration for DVD's at start like with normal files
            // we are getting the duration when the DVD is stopped. If the duration of 
            // feature is an hour or more it's probably the main feature and we will update
            // the database. 
            if (g_Player.IsDVD && (g_Player.Player.Duration >= 3600)) {
                DBLocalMedia playingFile = _activeMovie.LocalMedia[_activePart - 1];
                updateMediaDuration(playingFile);
            }

            int requiredWatchedPercent = MovingPicturesCore.Settings.MinimumWatchPercentage;
            int watchedPercentage = _activeMovie.GetPercentage(_activePart, timeMovieStopped);

            logger.Debug("Watched: Percentage=" + watchedPercentage + ", Required=" + requiredWatchedPercent);

            // if enough of the movie has been watched
            if (watchedPercentage >= requiredWatchedPercent) {
                // run movie ended logic
                onMovieEnded(_activeMovie);
            }
            // otherwise, store resume data.
            else {
                byte[] resumeData = null;
                g_Player.Player.GetResumeState(out resumeData);
                updateMovieResumeState(_activeMovie, _activePart, timeMovieStopped, resumeData);
                // run movie stopped logic
                onMovieStopped(_activeMovie);
            }            
        }

        private void onPlayBackEnded(g_Player.MediaType type, string filename) {
            if (type != g_Player.MediaType.Video || _playerState != MoviePlayerState.Playing)
                return;

            if (handleCustomIntroEnded())
                return;

            logger.Debug("OnPlayBackEnded filename={0} currentMovie={1} currentPart={2}", filename, _activeMovie.Title, _activePart);
            if (_activeMovie.LocalMedia.Count >= (_activePart + 1)) {
                logger.Debug("Goto next part");
                _activePart++;
                playMovie(_activeMovie, _activePart);
            }
            else {
                onMovieEnded(_activeMovie);
            }
        }

        #endregion

        #region External Player Event Handlers

        private void onStartExternal(Process proc, bool waitForExit) {
            // If we were listening for external player events
            if (_playerState == MoviePlayerState.Processing && listenToExternalPlayerEvents) {
                logger.Info("Playback Started: External");
                onMediaStarted(_queuedMedia);
            }
        }

        private void onStopExternal(Process proc, bool waitForExit) {
            if (_playerState != MoviePlayerState.Playing || !listenToExternalPlayerEvents)
                return;

            logger.Debug("Handling: OnStopExternal()");
            
            // call the logic for when an external player exits
            onExternalExit();
        }

        #endregion

        #region Player Events

        private bool handleCustomIntroEnded() {
            if (customIntroPlayed) {

                // Set custom intro played back to false
                customIntroPlayed = false;

                // If a custom intro was just played, we need to play the selected movie
                playMovie(_queuedMedia.AttachedMovies[0], _queuedMedia.Part);
                return true;
            }

            return false;
        }

        private void onExternalExit() {
            if (handleCustomIntroEnded())
                return;

            if (CurrentMovie == null)
                return;

            if (_activePart < _activeMovie.LocalMedia.Count) {
                string sBody = String.Format(Translation.ContinueToNextPartBody, (_activePart + 1)) + "\n" + _activeMovie.Title;
                bool bContinue = _gui.ShowCustomYesNo(Translation.ContinueToNextPartHeader, sBody, null, null, true);

                if (bContinue) {
                    logger.Debug("Goto next part");
                    _activePart++;
                    playMovie(_activeMovie, _activePart);
                }
                else {
                    // movie stopped
                    onMovieStopped(_activeMovie);
                }
            }
            else {
                // movie ended
                onMovieEnded(_activeMovie);
            }
        }

        private void onMediaStarted(DBLocalMedia localMedia) {
           // set playback active
           _playerState = MoviePlayerState.Playing;
           
           DBMovieInfo previousMovie = CurrentMovie;
           activeMedia = localMedia;

           // Update OSD (delayed)
           Thread newThread = new Thread(new ThreadStart(UpdatePlaybackInfo));
           newThread.Start();

           // only invoke movie started event if we were not playing this movie before
           if ((previousMovie != CurrentMovie) && (MovieStarted != null))
               MovieStarted(CurrentMovie);
        }

        private void onMovieStopped(DBMovieInfo movie) {
            // reset player
            resetPlayer();

            // invoke event
            if (MovieStopped != null)
                MovieStopped(movie);
        }

        private void onMovieEnded(DBMovieInfo movie) {
            // update watched counter
            updateMovieWatchedCounter(movie);

            // clear resume state
            clearMovieResumeState(movie);

            // reset player
            resetPlayer();

            // invoke event
            if (MovieEnded != null)
                MovieEnded(movie);           
        }

        /// <summary>
        /// Resets player variables
        /// </summary>
        private void resetPlayer() {
           
            // reset player variables
            activeMedia = null;
            _queuedMedia = null;
            _playerState = MoviePlayerState.Idle;
            _resumeActive = false;
            listenToExternalPlayerEvents = false;
            customIntroPlayed = false;

            // If we have an image mounted, unmount it
            if (mountedPlayback) {
                DaemonTools.UnMount();
                mountedPlayback = false;
            }
            
            logger.Debug("Reset.");
        }

        #endregion

        #region Movie Update Methods

        // store the duration of the file if it is not set
        private void updateMediaDuration(DBLocalMedia localMedia) {
            if (localMedia.Duration == 0) {
                localMedia.Duration = (int)g_Player.Player.Duration;
                localMedia.Commit();
            }
        }

        private void updateMovieWatchedCounter(DBMovieInfo movie) {
            if (movie == null)
                return;

            // get the user settings for the default profile (for now)
            DBUserMovieSettings userSetting = movie.ActiveUserSettings;
            userSetting.WatchedCount++; // increment watch counter
            userSetting.Commit();
            DBWatchedHistory.AddWatchedHistory(movie, userSetting.User);
        }

        private void clearMovieResumeState(DBMovieInfo movie) {
            updateMovieResumeState(movie, 0, 0, null);
        }

        private void updateMovieResumeState(DBMovieInfo movie, int part, int timePlayed, byte[] resumeData) {
            if (movie.UserSettings.Count == 0)
                return;

            // get the user settings for the default profile (for now)
            DBUserMovieSettings userSetting = movie.ActiveUserSettings;

            if (timePlayed > 0) {
                // set part and time data 
                userSetting.ResumePart = part;
                userSetting.ResumeTime = timePlayed;
                userSetting.ResumeData = new ByteArray(resumeData);
                logger.Debug("Updating movie resume state.");
            }
            else {
                // clear the resume settings
                userSetting.ResumePart = 0;
                userSetting.ResumeTime = 0;
                userSetting.ResumeData = null;
                logger.Debug("Clearing movie resume state.");
            }
            // save the changes to the user setting for this movie
            userSetting.Commit();
        }

        #endregion

        #region GUI/OSD

        // Updates the movie metadata on the playback screen (for when the user clicks info). 
        // The delay is necessary because Player tries to use metadata from the MyVideos database.
        // We want to update this after that happens so the correct info is there.
        private void UpdatePlaybackInfo() {
            Thread.Sleep(2000);
            if (CurrentMovie != null) {
                _gui.SetProperty("#Play.Current.Title", CurrentMovie.Title);
                _gui.SetProperty("#Play.Current.Plot", CurrentMovie.Summary);
                _gui.SetProperty("#Play.Current.Thumb", CurrentMovie.CoverThumbFullPath);
                _gui.SetProperty("#Play.Current.Year", CurrentMovie.Year.ToString());

                if (CurrentMovie.Genres.Count > 0)
                    _gui.SetProperty("#Play.Current.Genre", CurrentMovie.Genres[0]);
                else
                    _gui.SetProperty("#Play.Current.Genre", "");

            }
        }

        private bool PromptUserToResume(DBMovieInfo movie) {
            if (movie.UserSettings == null || movie.UserSettings.Count == 0 || (movie.ActiveUserSettings.ResumePart < 2 && movie.ActiveUserSettings.ResumeTime <= 30))
                return false;

            logger.Debug("Resume Prompt: Movie='{0}', ResumePart={1}, ResumeTime={2}", movie.Title, movie.ActiveUserSettings.ResumePart, movie.ActiveUserSettings.ResumeTime);

            // figure out the resume time to display to the user
            int displayTime = movie.ActiveUserSettings.ResumeTime;
            if (movie.LocalMedia.Count > 1) {
                for (int i = 0; i < movie.ActiveUserSettings.ResumePart - 1; i++) {
                    if (movie.LocalMedia[i].Duration > 0)
                        displayTime += (movie.LocalMedia[i].Duration / 1000); // convert milliseconds to seconds
                }
            }

            string sbody = movie.Title + "\n" + Translation.ResumeFrom + " " + Util.Utils.SecondsToHMSString(displayTime);
            bool bResume = _gui.ShowCustomYesNo(Translation.ResumeFromLast, sbody, null, null, true);

            if (bResume)
                return true;

            return false;
        }

        #endregion

    }
}
