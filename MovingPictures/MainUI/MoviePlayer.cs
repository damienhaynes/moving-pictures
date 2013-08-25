using System;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Threading;
using Cornerstone.Database.CustomTypes;
using Cornerstone.Tools;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Util;
using MediaPortal.InputDevices;
using NLog;
using System.Collections.Generic;
using MediaPortal.Plugins.MovingPictures.BackgroundProcesses;

namespace MediaPortal.Plugins.MovingPictures.MainUI {

    public delegate void MoviePlayerEvent(DBMovieInfo movie);

    public class MoviePlayer {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        public enum MoviePlayerState { Idle, Processing, Playing }

        #region Private variables

        private MovingPicturesGUI _gui;
        private bool donePlayingCustomIntros = false;
        private int customIntrosPlayed = 0;
        private bool mountedPlayback = false;
        private bool listenToExternalPlayerEvents = false;
        private DBLocalMedia queuedMedia;
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
            // stop the mediainfo background process if it is running
            MovingPicturesCore.ProcessManager.CancelProcess(typeof(MediaInfoUpdateProcess));

            // stop the internal player if it's running
            if (g_Player.Playing)
                g_Player.Stop();

            // set player state working
            _playerState = MoviePlayerState.Processing;
            
            // queue the local media object in case we first need to play the custom intro
            // we can get back to it later.
            queuedMedia = movie.LocalMedia[part-1];
            
            // try playing our custom intro (if present). If successful quit, as we need to
            // wait for the intro to finish.
            bool success = playCustomIntro();
            if (success) return;

            // Start movie
            playMovie(movie, part);
        }

        public void Stop() {
            if (g_Player.Playing)
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
                if (!_resumeActive && movie.LocalMedia.Count > 1 && (movie.LocalMedia[0].IsImageFile || movie.LocalMedia[0].IsVideoDisc)) {
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
            MediaState mediaState = mediaToPlay.State;
            while (mediaState != MediaState.Online) {
                switch (mediaState) {
                    case MediaState.Removed:
                        _gui.ShowMessage("Error", Translation.MediaIsMissing);
                        resetPlayer();
                        return; 
                    case MediaState.Offline:
                        string bodyString = String.Format(Translation.MediaNotAvailableBody, mediaToPlay.MediaLabel);
                        // Special debug line to troubleshoot availability issues
                        logger.Debug("Media not available: Path={0}, DriveType={1}, Serial={2}, ExpectedSerial={3}",
                            mediaToPlay.FullPath, mediaToPlay.ImportPath.GetDriveType().ToString(),
                            mediaToPlay.ImportPath.GetVolumeSerial(), mediaToPlay.VolumeSerial);

                        // Prompt user to enter media
                        if (!_gui.ShowCustomYesNo(Translation.MediaNotAvailableHeader, bodyString, Translation.Retry, Translation.Cancel, true)) {
                            // user cancelled so exit
                            resetPlayer();
                            return;
                        }
                        break;
                    case MediaState.NotMounted:
                        // Mount this media
                        MountResult result = mediaToPlay.Mount();
                        while (result == MountResult.Pending) {
                            if (_gui.ShowCustomYesNo(Translation.VirtualDriveHeader, Translation.VirtualDriveMessage, Translation.Retry, Translation.Cancel, true)) {
                                // User has chosen to retry
                                // We stay in the mount loop
                                result = mediaToPlay.Mount();
                            }
                            else {
                                // Exit the player
                                resetPlayer();
                                return;
                            }
                        }

                        // If the mounting failed (can not be solved within the loop) show error and return
                        if (result == MountResult.Failed) {
                            _gui.ShowMessage(Translation.Error, Translation.FailedMountingImage);
                            // Exit the player
                            resetPlayer();
                            return;
                        }
                        
                        // Mounting was succesfull, break the mount loop
                        break;
                }

                // Check mediaState again
                mediaState = mediaToPlay.State;
            }
            
            // Get the path to the playable video.
            string videoPath = mediaToPlay.GetVideoPath();

            // If the media is an image, it will be mounted by this point so
            // we flag the mounted playback variable
            mountedPlayback = mediaToPlay.IsImageFile;

            // if we do not have MediaInfo but have the AutoRetrieveMediaInfo setting toggled
            // get the media info
            if (!mediaToPlay.HasMediaInfo && MovingPicturesCore.Settings.AutoRetrieveMediaInfo) {
                mediaToPlay.UpdateMediaInfo();
                mediaToPlay.Commit();
            }

            // store the current media object so we can request it later
            queuedMedia = mediaToPlay;

            // start playback
            logger.Info("Playing: Movie='{0}' FullPath='{1}', VideoPath='{2}', Mounted={3})", movie.Title, mediaToPlay.FullPath, videoPath, mountedPlayback.ToString());
            playFile(videoPath, mediaToPlay.VideoFormat);
        }

        private bool playCustomIntro() {
            // Check if we have already played a custom intro
            if (!donePlayingCustomIntros && MovingPicturesCore.Settings.CustomIntroCount > 0) {
                DBMovieInfo queuedMovie = queuedMedia.AttachedMovies[0];
                // Only play custom intro for we are not resuming
                if (queuedMovie.UserSettings == null || queuedMovie.UserSettings.Count == 0 || queuedMovie.ActiveUserSettings.ResumeTime < 30) {
                    string custom_intro = MovingPicturesCore.Settings.CustomIntroLocation;
                    custom_intro = queuedMovie.ParseWildcards(custom_intro);
                    custom_intro = getCustomIntroFile(custom_intro);
                    if (custom_intro == null) return false;

                    // Check if the custom intro is specified by user and exists
                    if (custom_intro.Length > 0 && File.Exists(custom_intro)) {
                        logger.Debug("Playing Custom Intro: {0}", custom_intro);

                        // we set this variable before we start the actual playback
                        // because playFile to account for the blocking nature of the
                        // mediaportal external player logic
                        customIntrosPlayed++;
                        if (customIntrosPlayed >= MovingPicturesCore.Settings.CustomIntroCount)
                            donePlayingCustomIntros = true;

                        // start playback
                        playFile(custom_intro);
                        return true;
                    }
                    else if (custom_intro.Length > 0) {
                        logger.Warn("Unable to find custom intro file: " + custom_intro);
                    }
                }
            }

            return false;
        }

        private string getCustomIntroFile(string path) {
            try {
                if (File.Exists(path)) return path;
                if (Directory.Exists(path)) {
                    Random r = new Random();
                    List<FileInfo> videos = VideoUtility.GetVideoFilesRecursive(new DirectoryInfo(path));
                    if (videos.Count == 0) return null;
                    return videos[r.Next(videos.Count)].FullName;
                }
            }
            catch (Exception e) {
                logger.Error("Unexpected error searching for custom intro: " + e.Message);
            }

            return null;
        }

        // Start playback of a file (detects format first)
        public void playFile(string media) {
            VideoFormat videoFormat = VideoUtility.GetVideoFormat(media);
            if (videoFormat != VideoFormat.NotSupported) {
                playFile(media, videoFormat);
            }
            else {
                logger.Warn("'{0}' is not a playable video file.", media);
                resetPlayer();
            }
        }

        // start playback of a file (using known format)
        private void playFile(string media, VideoFormat videoFormat) {
            logger.Debug("Processing media for playback: File={0}, VideoFormat={1}", media, videoFormat);
                        
            // HD Playback
            if (videoFormat == VideoFormat.Bluray || videoFormat == VideoFormat.HDDVD) {

                // Take proper action according to playback setting
                bool hdExternal = MovingPicturesCore.Settings.UseExternalPlayer;

                // Launch external player if user has configured it for HD playback.
                if (hdExternal) {
                    LaunchHDPlayer(media);
                    logger.Info("HD Playback: Internal, Media={0}", media);
                    return;
                }
            }
            
            // We start listening to external player events
            listenToExternalPlayerEvents = true;
            
            // Play the file using the mediaportal player
            bool success = g_Player.Play(media.Trim());

            // We stop listening to external player events
            listenToExternalPlayerEvents = false;

            // if the playback started and we are still playing go full screen (internal player)
            if (success && g_Player.Playing)
                g_Player.ShowFullScreenWindow();
            else if (!success) {
                // if the playback did not happen, reset the player
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
            string fps = ((int)(queuedMedia.VideoFrameRate + 0.5f)).ToString();
            arguments = arguments.Replace("%filename%", filename);
            arguments = arguments.Replace("%fps%", fps);
            arguments = arguments.Replace("%root%", videoRoot);

            logger.Debug("External Player: Video='{0}', Root={1}, FPS={2}, ExecCommandLine={3} {4}", filename, videoRoot, fps, execPath, arguments);

            // Set Refresh Rate Based On FPS if needed
            if (MovingPicturesCore.Settings.UseDynamicRefreshRateChangerWithExternalPlayer) {
                double framerate = double.Parse(queuedMedia.VideoFrameRate.ToString(NumberFormatInfo.InvariantInfo), NumberFormatInfo.InvariantInfo);
                logger.Info("Requesting new refresh rate: FPS={0}", framerate.ToString());
                RefreshRateChanger.SetRefreshRateBasedOnFPS(framerate, filename, RefreshRateChanger.MediaType.Video);
                if (RefreshRateChanger.RefreshRateChangePending) {
                    TimeSpan ts = DateTime.Now - RefreshRateChanger.RefreshRateChangeExecutionTime;
                    if (ts.TotalSeconds > RefreshRateChanger.WAIT_FOR_REFRESHRATE_RESET_MAX) {
                        logger.Info("Refresh rate change failed. Please check your mediaportal log and configuration", RefreshRateChanger.WAIT_FOR_REFRESHRATE_RESET_MAX);
                        RefreshRateChanger.ResetRefreshRateState();
                    } 
                }
            }

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
                onMediaStarted(queuedMedia);
            }
            catch (Exception e) {
                logger.ErrorException("HD Playback: Could not start the external player process.", e);
                resetPlayer();
            }
        }

        private void OnHDPlayerExited(object obj, EventArgs e) {
            
            // Restore refresh rate if it was changed
            if (MovingPicturesCore.Settings.UseDynamicRefreshRateChangerWithExternalPlayer && RefreshRateChanger.RefreshRateChangePending)
                RefreshRateChanger.AdaptRefreshRate();

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
                updateMediaDuration(queuedMedia);

                // get the movie
                DBMovieInfo movie = queuedMedia.AttachedMovies[0];

                // and jump to our resume position if necessary
                if (_resumeActive) {
                  if (g_Player.IsDVD && !movie.LocalMedia[0].IsBluray) {
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
                onMediaStarted(queuedMedia);
            }
        }

        private void onPlayBackStoppedOrChanged(g_Player.MediaType type, int timeMovieStopped, string filename) {
            if (type != g_Player.MediaType.Video || _playerState != MoviePlayerState.Playing)
                return;

            if (customIntrosPlayed > 0) return;

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
                onMediaStarted(queuedMedia);
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
            if (donePlayingCustomIntros) {

                // Set custom intro played back to false
                donePlayingCustomIntros = false;
                customIntrosPlayed = 0;

                // If a custom intro was just played, we need to play the selected movie
                playMovie(queuedMedia.AttachedMovies[0], queuedMedia.Part);
                return true;
            }

            if (customIntrosPlayed > 0) {
                playCustomIntro();
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
            if (previousMovie != CurrentMovie) {
                if (MovingPicturesCore.Settings.FollwitEnabled)
                    MovingPicturesCore.Follwit.CurrentlyWatching(localMedia.AttachedMovies[0], true);
                if (MovieStarted != null) MovieStarted(CurrentMovie);
            }
        }

        private void onMovieStopped(DBMovieInfo movie) {
            // reset player
            resetPlayer();

            if (MovingPicturesCore.Settings.FollwitEnabled) 
                MovingPicturesCore.Follwit.CurrentlyWatching(movie, false);

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

            if (MovingPicturesCore.Settings.FollwitEnabled) 
                MovingPicturesCore.Follwit.CurrentlyWatching(movie, false);

            // invoke event
            if (MovieEnded != null)
                MovieEnded(movie);           
        }

        /// <summary>
        /// Resets player variables
        /// </summary>
        private void resetPlayer() {

            // If we have an image mounted, unmount it
            if (mountedPlayback) {
                queuedMedia.UnMount();
                mountedPlayback = false;
            }
            
            // reset player variables

            if (GUIGraphicsContext.IsFullScreenVideo)
                GUIGraphicsContext.IsFullScreenVideo = false;

            activeMedia = null;
            queuedMedia = null;
            _playerState = MoviePlayerState.Idle;
            _resumeActive = false;
            listenToExternalPlayerEvents = false;
            donePlayingCustomIntros = false;
            customIntrosPlayed = 0;
            ClearPlayProperties();

            logger.Debug("Reset.");
        }

        #endregion

        #region Movie Update Methods

        // store the duration of the file if it is not set
        private void updateMediaDuration(DBLocalMedia localMedia) {
            if (localMedia.Duration == 0) {
                logger.Debug("UpdateMediaDuration: LocalMedia={0}, Format={1}, Duration={2}", localMedia.FullPath, localMedia.VideoFormat, g_Player.Player.Duration.ToString(NumberFormatInfo.InvariantInfo));
                localMedia.Duration = ((int)g_Player.Player.Duration) * 1000;
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

        /// <summary>
        /// Publish all video specific properties to skin
        /// http://wiki.team-mediaportal.com/1_MEDIAPORTAL_1/18_Contribute/7_Skins/Skin_Architecture/Current_File_Tags
        /// </summary>
        private void SetPlayProperties() {
            _gui.SetProperty("#Play.Current.Title", CurrentMovie.Title);
            _gui.SetProperty("#Play.Current.Plot", CurrentMovie.Summary);
            _gui.SetProperty("#Play.Current.PlotOutline", CurrentMovie.Summary);
            _gui.SetProperty("#Play.Current.Thumb", CurrentMovie.CoverThumbFullPath);
            _gui.SetProperty("#Play.Current.Year", CurrentMovie.Year.ToString());
            _gui.SetProperty("#Play.Current.File", CurrentMovie.LocalMedia[0].File.FullName);
            _gui.SetProperty("#Play.Current.DVDLabel", CurrentMovie.LocalMedia[0].MediaLabel);
            _gui.SetProperty("#Play.Current.IMDBNumber", CurrentMovie.ImdbID);
            _gui.SetProperty("#Play.Current.Rating", CurrentMovie.Score.ToString());
            _gui.SetProperty("#Play.Current.Votes", CurrentMovie.Popularity.ToString());
            _gui.SetProperty("#Play.Current.Runtime", CurrentMovie.ActualRuntime != 0 ? CurrentMovie.ActualRuntime.ToString() : CurrentMovie.Runtime.ToString());
            _gui.SetProperty("#Play.Current.MPAARating", CurrentMovie.Certification);
            _gui.SetProperty("#Play.Current.IsWatched", (CurrentMovie.UserSettings[0].WatchedCount > 0).ToString().ToLowerInvariant());
            _gui.SetProperty("#Play.Current.TagLine", CurrentMovie.Tagline);
            _gui.SetProperty("#Play.Current.Director", CurrentMovie.Directors.ToPrettyString(1));
            _gui.SetProperty("#Play.Current.Genre", CurrentMovie.Genres.ToPrettyString(1));
            _gui.SetProperty("#Play.Current.Credits", CurrentMovie.Writers.ToPrettyString(MovingPicturesCore.Settings.MaxElementsToDisplay));
            _gui.SetProperty("#Play.Current.Cast", CurrentMovie.Actors.ToPrettyString(MovingPicturesCore.Settings.MaxElementsToDisplay));
        }

        private void ClearPlayProperties() {
            _gui.SetProperty("#Play.Current.Title", string.Empty);
            _gui.SetProperty("#Play.Current.Plot", string.Empty);
            _gui.SetProperty("#Play.Current.PlotOutline", string.Empty);
            _gui.SetProperty("#Play.Current.Thumb", string.Empty);
            _gui.SetProperty("#Play.Current.Year", string.Empty);
            _gui.SetProperty("#Play.Current.File", string.Empty);
            _gui.SetProperty("#Play.Current.DVDLabel", string.Empty);
            _gui.SetProperty("#Play.Current.IMDBNumber", string.Empty);
            _gui.SetProperty("#Play.Current.Rating", string.Empty);
            _gui.SetProperty("#Play.Current.Votes", string.Empty);
            _gui.SetProperty("#Play.Current.Runtime", string.Empty);
            _gui.SetProperty("#Play.Current.MPAARating", string.Empty);
            _gui.SetProperty("#Play.Current.IsWatched", string.Empty);
            _gui.SetProperty("#Play.Current.TagLine", string.Empty);
            _gui.SetProperty("#Play.Current.Director", string.Empty);
            _gui.SetProperty("#Play.Current.Genre", string.Empty);
            _gui.SetProperty("#Play.Current.Credits", string.Empty);
            _gui.SetProperty("#Play.Current.Cast", string.Empty);
        }

        // Updates the movie metadata on the playback screen (for when the user clicks info). 
        // The delay is necessary because Player tries to use metadata from the MyVideos database.
        // We want to update this after that happens so the correct info is there.
        private void UpdatePlaybackInfo() {
            Thread.Sleep(2000);
            if (CurrentMovie != null) {
                SetPlayProperties();
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
