using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MediaPortal.GUI.Library;
using MediaPortal.Plugins.MovingPictures.ConfigScreen;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using System.Threading;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;
using System.Collections;
using System.Xml;
using System.Timers;
using MediaPortal.Plugins.MovingPictures.DataProviders;
using Cornerstone.Database.Tables;
using Cornerstone.Database;
using Cornerstone.Database.CustomTypes;
using MediaPortal.Util;

namespace MediaPortal.Plugins.MovingPictures {
    public class MovingPicturesGUI : GUIWindow {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public enum ViewMode { LIST, SMALLICON, LARGEICON, FILMSTRIP, FANART_FILMSTRIP, DETAILS }

        #region Private Variables

        Dictionary<string, string> defines;
        Dictionary<string, bool> loggedProperties;

        private bool currentlyPlaying = false;
        private DBMovieInfo currentMovie;
        private int currentPart = 1;

        private bool loaded = false;

        private System.Timers.Timer updateArtworkTimer;

        private List<GUIListItem> _listItems; // List Source
        private string _listFilterString; // List Filter String

        #endregion

        #region GUI Controls

        [SkinControl(50)]
        protected GUIFacadeControl movieBrowser = null;

        [SkinControl(1)]
        protected GUIImage movieBackdrop = null;

        [SkinControl(2)]
        protected GUIButtonControl cycleViewButton = null;

        [SkinControl(3)]
        protected GUIButtonControl viewMenuButton = null;

        [SkinControl(4)]
        protected GUIButtonControl filterButton = null;

        [SkinControl(5)]
        protected GUIButtonControl settingsButton = null;

        [SkinControl(6)]
        protected GUIButtonControl playButton = null;

        [SkinControl(7)]
        protected GUIButtonControl textToggleButton = null;

        #endregion

        // Defines the current view mode. Reassign to switch modes.        
        public ViewMode CurrentView {
            get {
                return currentView;
            }

            set {
                // update the state variables
                if (currentView != value)
                    previousView = currentView;
                currentView = value;

                // if we are leaving details view, set focus back on the facade
                if (previousView == ViewMode.DETAILS) {
                    ClearFocus();
                    movieBrowser.Focus = true;
                    movieBrowser.Visible = true;
                }

                switch (currentView) {
                    case ViewMode.LIST:
                        movieBrowser.View = GUIFacadeControl.ViewMode.List;
                        break;
                    case ViewMode.SMALLICON:
                        movieBrowser.View = GUIFacadeControl.ViewMode.SmallIcons;
                        break;
                    case ViewMode.LARGEICON:
                        movieBrowser.View = GUIFacadeControl.ViewMode.LargeIcons;
                        break;
                    case ViewMode.FILMSTRIP:
                        movieBrowser.View = GUIFacadeControl.ViewMode.Filmstrip;
                        break;
                    case ViewMode.DETAILS:
                        ClearFocus();

                        movieBrowser.Visible = false;
                        movieBrowser.ListView.Visible = false;
                        movieBrowser.ThumbnailView.Visible = false;
                        movieBrowser.FilmstripView.Visible = false;

                        playButton.Focus = true;
                        break;
                }

                if (movieBrowser.SelectedListItem != null)
                    SelectedMovie = movieBrowser.SelectedListItem.TVTag as DBMovieInfo;

                UpdateArtwork();
            }
        }
        private ViewMode currentView;
        private ViewMode previousView;

        // The currenty selected movie.
        public DBMovieInfo SelectedMovie {
            get {
                return selectedMovie;
            }

            set {
                if (selectedMovie == value)
                    return;

                if (selectedMovie != null)
                    logger.Debug("SelectedMovie changed: " + selectedMovie.Title);

                // load new data
                selectedMovie = value;
                selectedIndex = movieBrowser.SelectedListItemIndex;
                PublishDetails(SelectedMovie, "SelectedMovie");

                // start the timer for new artwork loading
                updateArtworkTimer.Stop();
                updateArtworkTimer.Start();
            }
        }
        private DBMovieInfo selectedMovie;

        public int SelectedIndex {
            get {
                return selectedIndex;
            }
        }
        private int selectedIndex = 0;

        public MovingPicturesGUI() {
            selectedMovie = null;

            g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayBackStarted);
            g_Player.PlayBackEnded += new g_Player.EndedHandler(OnPlayBackEnded);
            g_Player.PlayBackStopped += new g_Player.StoppedHandler(OnPlayBackStopped);

            // setup the timer for delayed artwork loading
            int artworkDelay = (int)MovingPicturesCore.SettingsManager["gui_artwork_delay"].Value;
            updateArtworkTimer = new System.Timers.Timer();
            updateArtworkTimer.Elapsed += new ElapsedEventHandler(OnUpdateArtworkTimerElapsed);
            updateArtworkTimer.Interval = artworkDelay;
            updateArtworkTimer.AutoReset = false;

            _listItems = new List<GUIListItem>();
            _listFilterString = string.Empty;
            loggedProperties = new Dictionary<string, bool>();
        }

        ~MovingPicturesGUI() {
        }

        // this timer creates a delay for loading background art. it waits a user defined
        // period of time (usually something like 200ms, THEN updates the backdrop. If the
        // user switches again *before* the timer expires, the timer is reset. THis allows
        // quick traversal on the GUI
        private void OnUpdateArtworkTimerElapsed(object sender, ElapsedEventArgs e) {
            UpdateArtwork();
        }

        private void UpdateArtwork() {
            if (SelectedMovie == null)
                return;

            string oldBackdrop = GUIPropertyManager.GetProperty("#MovingPictures.Backdrop");
            string oldCover = GUIPropertyManager.GetProperty("#MovingPictures.Coverart");

            // update backdrop and cover art
            SetProperty("#MovingPictures.Backdrop", SelectedMovie.BackdropFullPath.Trim());
            SetProperty("#MovingPictures.Coverart", SelectedMovie.CoverFullPath.Trim());

            // clear out previous textures for backdrop and cover art
            GUITextureManager.ReleaseTexture(oldBackdrop);
            GUITextureManager.ReleaseTexture(oldCover);

            // grab the skin supplied setting for backdrop visibility
            bool backdropActive = true;
            switch (CurrentView) {
                case ViewMode.FILMSTRIP:
                    backdropActive = defines["#filmstrip.backdrop.used"].Equals("true");
                    break;
                case ViewMode.LARGEICON:
                    backdropActive = defines["#largeicons.backdrop.used"].Equals("true");
                    break;
                case ViewMode.SMALLICON:
                    backdropActive = defines["#smallicons.backdrop.used"].Equals("true");
                    break;
                case ViewMode.LIST:
                    backdropActive = defines["#list.backdrop.used"].Equals("true");
                    break;
                case ViewMode.DETAILS:
                    backdropActive = defines["#details.backdrop.used"].Equals("true");
                    break;
            }

            // set backdrop visibility
            if (backdropActive && SelectedMovie != null &&
                SelectedMovie.BackdropFullPath.Trim().Length != 0)

                movieBackdrop.Visible = true;
            else
                movieBackdrop.Visible = false;
        }

        private bool IsViewAvailable(ViewMode view) {
            switch (view) {
                case ViewMode.FILMSTRIP:
                    logger.Debug("FILMSTRIP: " + defines["#filmstrip.available"].Equals("true"));
                    return defines["#filmstrip.available"].Equals("true");
                case ViewMode.LARGEICON:
                    logger.Debug("LARGEICON: " + defines["#largeicons.available"].Equals("true"));
                    return defines["#largeicons.available"].Equals("true");
                case ViewMode.SMALLICON:
                    logger.Debug("SMALLICON: " + defines["#smallicons.available"].Equals("true"));
                    return defines["#smallicons.available"].Equals("true");
                case ViewMode.LIST:
                    logger.Debug("LIST: " + defines["#list.available"].Equals("true"));
                    return defines["#list.available"].Equals("true");
                case ViewMode.FANART_FILMSTRIP:
                    logger.Debug("FANART_FILMSTRIP: false");
                    return false;
                case ViewMode.DETAILS:
                    logger.Debug("DETAILS: " + defines["#filmstrip.available"].Equals("true"));
                    return true;
                default:
                    logger.Debug("DEFAULT: false");
                    return false;
            }
        }

        private void ClearFocus() {
            if (movieBrowser != null) {
                movieBrowser.Focus = false;
                if (movieBrowser.ListView != null) movieBrowser.ListView.Focus = false;
                if (movieBrowser.ThumbnailView != null) movieBrowser.ThumbnailView.Focus = false;
                if (movieBrowser.FilmstripView != null) movieBrowser.FilmstripView.Focus = false;
            }

            if (cycleViewButton != null) cycleViewButton.Focus = false;
            if (viewMenuButton != null) viewMenuButton.Focus = false;
            if (filterButton != null) filterButton.Focus = false;
            if (settingsButton != null) settingsButton.Focus = false;
            if (playButton != null) playButton.Focus = false;
            if (textToggleButton != null) textToggleButton.Focus = false;
        }

        private void ShowMessage(string heading, string line1, string line2, string line3, string line4) {
            GUIDialogOK dialog = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
            dialog.Reset();
            dialog.SetHeading(heading);
            if (line1 != null) dialog.SetLine(1, line1);
            if (line2 != null) dialog.SetLine(2, line2);
            if (line3 != null) dialog.SetLine(3, line3);
            if (line4 != null) dialog.SetLine(4, line4);
            dialog.DoModal(GetID);
        }

        #region GUIWindow Methods

        public override int GetID {
            get {
                return 96742;
            }
        }

        public override bool Init() {
            bool success;

            // initialize the moving pictures core services
            success = Load(GUIGraphicsContext.Skin + @"\movingpictures.xml");
            success = success && MovingPicturesCore.Initialize();

            // start the background importer
            MovingPicturesCore.Importer.Start();

            // grab any <define> tags from the skin for later use
            LoadDefinesFromSkin();

            // setup listeners for new or removed movies
            MovingPicturesCore.DatabaseManager.ObjectDeleted += new DatabaseManager.ObjectAffectedDelegate(OnMovieDeleted);
            MovingPicturesCore.DatabaseManager.ObjectInserted += new DatabaseManager.ObjectAffectedDelegate(OnMovieAdded);

            return success;
        }

        public override void DeInit() {
            base.DeInit();
            MovingPicturesCore.Shutdown();
        }

        protected override void OnPageLoad() {
            // if the component didn't load properly we probably have a bad skin file
            if (movieBrowser == null) {
                GUIDialogOK dialog = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                dialog.Reset();
                dialog.SetHeading("Sorry, there was a problem loading skin file...");
                dialog.DoModal(GetID);
                GUIWindowManager.ShowPreviousWindow();
                return;
            }

            // if the user hasn't defined any import paths they need to goto the config screen
            if (DBImportPath.GetAll().Count == 0) {
                GUIDialogOK dialog = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                dialog.Reset();
                dialog.SetHeading("No Import Paths!");
                dialog.SetLine(1, "It doesn't look like you have defined any");
                dialog.SetLine(2, "import paths in the configuration screen.");
                dialog.SetLine(3, "You shold close MediaPortal and launch the");
                dialog.SetLine(4, "MediaPortal Configuration Screen to configure");
                dialog.SetLine(5, "Moving Pictures.");
                dialog.DoModal(GetID);
                GUIWindowManager.ShowPreviousWindow();
                return;
            }


            // create our movie list
            populateMovieListFromDatabase();

            // add movies to facade
            updateMovieBrowser();
            movieBrowser.Focus = true;

            // if this is our first time loading, we need to setup our default view data
            if (!loaded) {
                loaded = true;

                // set the default view for the facade
                string defaultView = ((string)MovingPicturesCore.SettingsManager["default_view"].Value).Trim().ToLower();
                if (defaultView.Equals("list")) {
                    CurrentView = ViewMode.LIST;
                }
                else if (defaultView.Equals("thumbs")) {
                    CurrentView = ViewMode.SMALLICON;
                }
                else if (defaultView.Equals("largethumbs")) {
                    CurrentView = ViewMode.LARGEICON;
                }
                else if (defaultView.Equals("filmstrip")) {
                    CurrentView = ViewMode.FILMSTRIP;
                }
                else {
                    CurrentView = ViewMode.LIST;
                    logger.Warn("The DEFAULT_VIEW setting contains an invalid value. Defaulting to List View.");
                }
            }

            // if we have loaded before, lets update the facade selection to match our actual selection
            else {
                ReloadSelection();
                ViewMode tmp = previousView;
                CurrentView = CurrentView;
                previousView = tmp;
            }

            // load fanart and coverart
            UpdateArtwork();
        }

        protected override void OnPageDestroy(int new_windowId) {
            base.OnPageDestroy(new_windowId);
        }

        protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType) {
            switch (controlId) {
                // a click from the facade
                case 50:
                    bool clickToDetails = (bool)MovingPicturesCore.SettingsManager["click_to_details"].Value;

                    switch (actionType) {
                        case Action.ActionType.ACTION_PLAY:
                            playSelectedMovie();
                            break;
                        case Action.ActionType.ACTION_SELECT_ITEM:
                            if (control == movieBrowser) {
                                if (clickToDetails)
                                    CurrentView = ViewMode.DETAILS;
                                else
                                    playSelectedMovie();
                            }
                            break;
                    }
                    break;

                // a click on the rotate view button
                case 2:
                    cycleView();
                    break;

                // a click on the view menu button
                case 3:
                    showMainContext();
                    break;

                // a click on the play button
                case 6:
                    playSelectedMovie();
                    break;
            }

            base.OnClicked(controlId, control, actionType);
        }

        public override void OnAction(Action action) {
            switch (action.wID) {
                case Action.ActionType.ACTION_PARENT_DIR:
                case Action.ActionType.ACTION_HOME:
                    currentlyPlaying = false;
                    GUIWindowManager.ShowPreviousWindow();
                    break;
                case Action.ActionType.ACTION_PREVIOUS_MENU:
                    if (CurrentView == ViewMode.DETAILS)
                        CurrentView = previousView;
                    else {
                        // if a filter is active remove it
                        if (!String.IsNullOrEmpty(_listFilterString)) {
                            _listFilterString = string.Empty;
                            // update the movie browser
                            updateMovieBrowser();
                        }
                        else {
                            // show previous screen
                            currentlyPlaying = false;
                            GUIWindowManager.ShowPreviousWindow();
                        }
                    }
                    break;
                case Action.ActionType.ACTION_MUSIC_PLAY:
                    // yes, the generic "play" action is called.... ACTION_MUSIC_PLAY...
                    playSelectedMovie();
                    break;
                case Action.ActionType.ACTION_KEY_PRESSED:
                    base.OnAction(action);
                
                    // List Filter
                    if ((action.m_key != null) && (CurrentView != ViewMode.DETAILS)) {
                        if (((action.m_key.KeyChar >= '0') && (action.m_key.KeyChar <= '9')) ||
                          action.m_key.KeyChar == '*' || action.m_key.KeyChar == '(' ||
                          action.m_key.KeyChar == '#' || action.m_key.KeyChar == '§') {

                            if (action.m_key.KeyChar == '*') {
                                // reset the list filter string
                                _listFilterString = string.Empty;
                            }
                            else {
                                // add the numeric code to the list filter string
                                Char key = (char)action.m_key.KeyChar;
                                _listFilterString += NumPadEncode(key.ToString());
                            }

                            // update the movie browser
                            updateMovieBrowser();
                        }
                    }
                    break;
                case Action.ActionType.ACTION_MOVE_RIGHT:
                case Action.ActionType.ACTION_MOVE_LEFT:
                case Action.ActionType.ACTION_MOVE_UP:
                default:
                    base.OnAction(action);
                    break;
            }
        }

        public override bool OnMessage(GUIMessage message) {
            return base.OnMessage(message);
        }

        #endregion

        #region Context Menu Methods

        protected override void OnShowContextMenu() {
            base.OnShowContextMenu();
            switch (CurrentView) {
                case ViewMode.DETAILS:
                    showDetailsContext();
                    break;
                default:
                    showMainContext();
                    break;
            }

            base.OnShowContextMenu();
        }

        private void showMainContext() {
            IDialogbox dialog = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dialog.Reset();
            dialog.SetHeading("Moving Pictures");

            GUIListItem listItem = new GUIListItem("List View");
            listItem.ItemId = 1;
            dialog.Add(listItem);

            GUIListItem thumbItem = new GUIListItem("Thumbnail View");
            thumbItem.ItemId = 2;
            dialog.Add(thumbItem);

            GUIListItem largeThumbItem = new GUIListItem("Large Thumbnail View");
            largeThumbItem.ItemId = 3;
            dialog.Add(largeThumbItem);

            GUIListItem filmItem = new GUIListItem("Filmstrip View");
            filmItem.ItemId = 4;
            dialog.Add(filmItem);

            dialog.DoModal(GUIWindowManager.ActiveWindow);
            switch (dialog.SelectedId) {
                case 1:
                    CurrentView = ViewMode.LIST;
                    break;
                case 2:
                    CurrentView = ViewMode.SMALLICON;
                    break;
                case 3:
                    CurrentView = ViewMode.LARGEICON;
                    break;
                case 4:
                    CurrentView = ViewMode.FILMSTRIP;
                    break;
            }

        }

        private void showDetailsContext() {
            IDialogbox dialog = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dialog.Reset();
            dialog.SetHeading("Moving Pictures");

            GUIListItem detailsItem = new GUIListItem("Update Details from Online");
            detailsItem.ItemId = 1;
            dialog.Add(detailsItem);

            if (SelectedMovie.AlternateCovers.Count > 1) {
                GUIListItem cycleArtItem = new GUIListItem("Cycle Cover-Art");
                cycleArtItem.ItemId = 2;
                dialog.Add(cycleArtItem);
            }

            if (selectedMovie.CoverFullPath.Trim().Length == 0 ||
                selectedMovie.BackdropFullPath.Trim().Length == 0) {
                GUIListItem retrieveArtItem = new GUIListItem("Check for Missing Artwork Online");
                retrieveArtItem.ItemId = 3;
                dialog.Add(retrieveArtItem);
            }

            dialog.DoModal(GUIWindowManager.ActiveWindow);
            switch (dialog.SelectedId) {
                case 1:
                    updateDetails();
                    break;
                case 2:
                    SelectedMovie.NextCover();
                    SelectedMovie.Commit();
                    UpdateArtwork();
                    break;
                case 3:
                    retrieveMissingArt();
                    break;
            }
        }

        // rotates the current view to the next available
        private void cycleView() {
            if (CurrentView == ViewMode.DETAILS)
                return;

            ViewMode newView = CurrentView;

            do {
                // rotate view until one is available
                if (newView == ViewMode.FILMSTRIP)
                    newView = ViewMode.LIST;
                else
                    newView++;

                logger.Debug("trying " + newView.ToString());

            } while (!IsViewAvailable(newView));

            previousView = CurrentView;
            CurrentView = newView;
        }

        // From online, updates the details of the currently selected movie.
        private void updateDetails() {
            GUIDialogYesNo dialog = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (dialog == null)
                return;

            dialog.Title = "Update Movie Details";
            dialog.SetLine(1, "You are about to refresh all movie metadata, overwriting");
            dialog.SetLine(2, "any custom modifications to this film. Do you want");
            dialog.SetLine(3, "to continue?");
            dialog.SetDefaultToYes(false);
            dialog.DoModal(GetID);

            if (dialog.IsConfirmed && SelectedMovie != null) {
                MovingPicturesCore.DataProviderManager.Update(SelectedMovie);
                SelectedMovie.Commit();
                PublishDetails(SelectedMovie, "SelectedMovie");
            }


        }

        // retrieves from online artwork for the currently selected movie
        // if and only if no artwork currently exists.
        private void retrieveMissingArt() {
            if (SelectedMovie == null)
                return;

            if (SelectedMovie.CoverFullPath.Trim().Length == 0) {
                MovingPicturesCore.DataProviderManager.GetArtwork(SelectedMovie);
                SelectedMovie.UnloadArtwork();
                SelectedMovie.Commit();
            }

            if (SelectedMovie.BackdropFullPath.Trim().Length == 0) {
                new LocalProvider().GetBackdrop(SelectedMovie);
                MovingPicturesCore.DataProviderManager.GetBackdrop(SelectedMovie);
                SelectedMovie.Commit();
            }

            UpdateArtwork();
        }

        #endregion

        #region Playback Methods

        private void playSelectedMovie() {
            if (SelectedMovie == null)
                SelectedMovie = movieBrowser.SelectedListItem.TVTag as DBMovieInfo;

            int part = 1;
            int parts = SelectedMovie.LocalMedia.Count;
            int resumeTime = 0;
            int resumePart = 0;
            byte[] resumeData = null;

            // Get User Settings for this movie
            if (SelectedMovie.UserSettings.Count > 0) {
                resumeTime = SelectedMovie.UserSettings[0].ResumeTime;
                resumePart = SelectedMovie.UserSettings[0].ResumePart;
                resumeData = SelectedMovie.UserSettings[0].ResumeData;
            }

            // If we have resume data ask the user if he wants to resume
            // todo: customize the dialog
            if (resumeTime > 0) {
                GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
                if (null == dlgYesNo) return;
                dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
                dlgYesNo.SetLine(1, SelectedMovie.Title);
                dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936) + " " + MediaPortal.Util.Utils.SecondsToHMSString(resumeTime));
                dlgYesNo.SetDefaultToYes(true);
                dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

                if (!dlgYesNo.IsConfirmed) resumeTime = 0; // reset resume time
                if (resumeTime > 0) part = resumePart; // on resume set part also
            }

            // if we have a multi-part movie and we are not resuming
            // ask which part the user wants to play
            // todo: customize the dialog
            if (parts > 1 && resumeTime == 0) {
                GUIDialogFileStacking dlg = (GUIDialogFileStacking)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_FILESTACKING);
                if (null != dlg) {
                    dlg.SetNumberOfFiles(parts);
                    dlg.DoModal(GUIWindowManager.ActiveWindow);
                    part = dlg.SelectedFile;
                    if (part < 1) return;
                }
            }

            // Start playing the movie with defined parameters
            playMovie(SelectedMovie, part, resumeTime, resumeData);
        }

        private void playMovie(DBMovieInfo movie, int part) {
            playMovie(movie, part, 0, null);
        }

        private void playMovie(DBMovieInfo movie, int part, int resumeTime) {
            playMovie(movie, part, resumeTime, null);
        }

        private void playMovie(DBMovieInfo movie, int part, int resumeTime, byte[] resumeData) {
            if (movie == null)
                return;

            DBLocalMedia localMediaToPlay = movie.LocalMedia[part - 1];

            // check for removable
            if (!localMediaToPlay.File.Exists && localMediaToPlay.ImportPath.Removable) {
                ShowMessage("Removable Media Not Available",
                            "The media for the Movie you have selected is not",
                            "currently available. Please insert or connect this",
                            "media source and try again.", null);
                return;
            }
            else if (!localMediaToPlay.File.Exists) {
                ShowMessage("Error",
                            "The media for the Movie you have selected is missing!",
                            "Very sorry but something has gone wrong...", null, null);
                return;
            }

            // grab media info
            string media = movie.LocalMedia[part - 1].FullPath;
            string ext = movie.LocalMedia[part - 1].File.Extension;

            // check if the current media is an image file
            if (DaemonTools.IsImageFile(ext))
                playImage(media);
            else
                playFile(media);

            // set the currently playing part if playback was successful
            if (currentlyPlaying) {
                currentMovie = movie;
                currentPart = part;

                // use resume data if needed
                if (resumeTime > 0 && g_Player.Playing) {
                    if (g_Player.IsDVD) {
                        // Resume DVD 
                        g_Player.Player.SetResumeState(resumeData);
                    }
                    else {
                        // Resume Other
                        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SEEK_POSITION, 0, 0, 0, 0, 0, null);
                        msg.Param1 = (int)resumeTime;
                        GUIGraphicsContext.SendMessage(msg);
                    }
                }

            }
        }

        // start playback of a regular file
        private void playFile(string media) {
            GUIGraphicsContext.IsFullScreenVideo = true;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);

            bool success = g_Player.Play(media, g_Player.MediaType.Video);
            currentlyPlaying = success;
        }

        // start playback of an image file (ISO)
        private void playImage(string media) {
            string drive;
            bool alreadyMounted;

            // Check if the current image is already mounted
            if (!DaemonTools.IsMounted(media)) {
                // if not try to mount the image
                logger.Info("Trying to mount image.");
                alreadyMounted = false;
                if (!DaemonTools.Mount(media, out drive)) {
                    ShowMessage("Error", "Sorry, failed mounting DVD Image...", null, null, null);
                    return;
                }
            }
            else {
                logger.Info("DVD Image already mounted.");
                drive = DaemonTools.GetVirtualDrive();
                alreadyMounted = true;
            }

            // try to grab a DVD IFO file off the newly mounted drive, and check if it exists
            string ifoPath = drive + @"\VIDEO_TS\VIDEO_TS.IFO";
            if (!System.IO.File.Exists(media)) {
                ShowMessage("Error", "The image file does not contain a DVD!", null, null, null);
                return;
            }

            // if this image was already mounted, autoplay doesn't work, so manually launch the DVD
            if (alreadyMounted)
                playFile(ifoPath);

            // if we have not already mounted and we let the auto play system start the movie
            // this data could be wrong because the user could chose not to play via the prmpt.
            // this should be handled in the future. possible bug.
            currentlyPlaying = true;
            return;
        }

        private void OnPlayBackStarted(g_Player.MediaType type, string filename) {
            if (currentMovie != null && currentlyPlaying) {
                logger.Info("OnPlayBackStarted filename={0} currentMovie={1}", filename, currentMovie.Title);
                Thread newThread = new Thread(new ThreadStart(UpdatePlaybackInfo));
                newThread.Start();
            }
        }

        private void OnPlayBackEnded(g_Player.MediaType type, string filename) {
            if (type != g_Player.MediaType.Video || currentMovie == null)
                return;

            logger.Info("OnPlayBackEnded filename={0} currentMovie={1} currentPart={2}", filename, currentMovie.Title, currentPart);
            clearMovieResumeState(currentMovie);
            if (currentMovie.LocalMedia.Count > 1 && currentMovie.LocalMedia.Count <= currentPart + 1) {
                logger.Debug("Goto next part");
                currentPart++;
                playMovie(currentMovie, currentPart);
            }
            else {
                updateMovieWatchedCounter(currentMovie);
                currentPart = 0;
                currentlyPlaying = false;
                currentMovie = null;
            }
        }

        private void OnPlayBackStopped(g_Player.MediaType type, int timeMovieStopped, string filename) {
            if (type != g_Player.MediaType.Video || currentMovie == null)
                return;

            logger.Info("OnPlayBackStopped filename={0} currentMovie={1} currentPart={2} timeMovieStopped={3} ", filename, currentMovie.Title, currentPart, timeMovieStopped);

            byte[] resumeData = null;
            g_Player.Player.GetResumeState(out resumeData);

            updateMovieResumeState(currentMovie, currentPart, timeMovieStopped, resumeData);
        }

        private void updateMovieWatchedCounter(DBMovieInfo movie) {
            if (movie == null)
                return;

            // get the user settings for the default profile (for now)
            DBUserMovieSettings userSetting = movie.UserSettings[0];
            userSetting.Watched++; // increment watch counter
            userSetting.Commit();
        }

        private void clearMovieResumeState(DBMovieInfo movie) {
            updateMovieResumeState(movie, 0, 0, null);
        }

        private void updateMovieResumeState(DBMovieInfo movie, int part, int timePlayed, byte[] resumeData) {
            // @TODO: maybe this function should not be in the GUI code  

            if (movie.UserSettings.Count == 0)
                return;

            // get the user settings for the default profile (for now)
            DBUserMovieSettings userSetting = movie.UserSettings[0];

            if (timePlayed > 0) {
                // set part and time data 
                userSetting.ResumePart = part;
                userSetting.ResumeTime = timePlayed;
                userSetting.ResumeData = resumeData;
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

        #region Facade / Movie Browser Management Methods

        private void OnItemSelected(GUIListItem item, GUIControl parent) {
            // if this is not a message from the facade, exit
            if (parent != movieBrowser && parent != movieBrowser.FilmstripView &&
                parent != movieBrowser.ThumbnailView && parent != movieBrowser.ListView)
                return;

            // if we already are on this movie, exit
            if (SelectedMovie == item.TVTag)
                return;

            SelectedMovie = item.TVTag as DBMovieInfo;
        }

        // This will reselect the currently selected movie on the facade. Generally this
        // is only neccisary when the plugin has been exited and reentered.
        private void ReloadSelection() {
            //Choose old value depending on type of view
            if (movieBrowser.PlayListView != null)
                movieBrowser.PlayListView.SelectedListItemIndex = SelectedIndex;

            if (movieBrowser.ListView != null)
                movieBrowser.ListView.SelectedListItemIndex = SelectedIndex;

            if (movieBrowser.AlbumListView != null)
                movieBrowser.AlbumListView.SelectedListItemIndex = SelectedIndex;

            if (movieBrowser.ThumbnailView != null)
                movieBrowser.ThumbnailView.SelectedListItemIndex = SelectedIndex;

            if (movieBrowser.FilmstripView != null) {
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, this.GetID, 0, movieBrowser.FilmstripView.GetID, SelectedIndex, 0, null);
                OnMessage(msg);
            }
        }

        private void OnMovieAdded(DatabaseTable obj) {
            // if this is not a movie object, break
            if (obj.GetType() != typeof(DBMovieInfo) || movieBrowser == null)
                return;

            Predicate<GUIListItem> predicate = delegate(GUIListItem item) {
                return item.ItemId == ((DBMovieInfo)obj).ID.Value;
            };

            // if this item is already in the list exit. This should never really happen though...  
            if (_listItems.Exists(predicate)) {
                logger.Warn("Received two \"added\" messages for " + (DBMovieInfo)obj);
                return;
            }

            logger.Info("Adding " + ((DBMovieInfo)obj).Title + " to movie list.");

            // add movie to the list (and sort)
            addMovieToList((DBMovieInfo)obj, true);
            // update the movie browser to reflect the changes
            updateMovieBrowser();
        }

        private void OnMovieDeleted(DatabaseTable obj) {
            // if this is not a movie object, break
            if (obj.GetType() != typeof(DBMovieInfo) || movieBrowser == null)
                return;

            logger.Info("Removing " + ((DBMovieInfo)obj).Title + " from list.");

            // remove movie from list
            DBMovieInfo movie = (DBMovieInfo)obj;
            removeMovieFromList(movie);
            // update the movie browser to reflect the changes
            updateMovieBrowser();
        }

        private void populateMovieListFromDatabase() {
            // clear the list
            _listItems.Clear();

            // populate the list
            List<DBMovieInfo> movies = DBMovieInfo.GetAll();
            foreach (DBMovieInfo currMovie in movies)
                addMovieToList(currMovie);

            // sort the list
            _listItems.Sort(new GUIListItemMovieComparer());
        }

        private void addMovieToList(DBMovieInfo newMovie) {
            addMovieToList(newMovie, false);
        }

        private void addMovieToList(DBMovieInfo newMovie, bool sortList) {
            if (newMovie == null)
                return;

            // create a new list object
            GUIListItem currItem = new GUIListItem();
            currItem.ItemId = newMovie.ID.Value;
            currItem.Label = newMovie.Title;
            currItem.IconImage = newMovie.CoverThumbFullPath.Trim();
            currItem.IconImageBig = newMovie.CoverThumbFullPath.Trim();
            currItem.TVTag = newMovie;
            currItem.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(OnItemSelected);

            // add the listitem
            _listItems.Add(currItem);

            // sort list if requested
            if (sortList)
                _listItems.Sort(new GUIListItemMovieComparer());
        }

        private void removeMovieFromList(DBMovieInfo movie) {
            if (movie == null)
                return;

            Predicate<GUIListItem> predicate = delegate(GUIListItem item) {
                DBMovieInfo movieItem = (DBMovieInfo)item.TVTag;
                return movieItem.ID == null;
            };

            // remove the item(s) from the list
            _listItems.RemoveAll(predicate);
        }

        // fills the movieBrowser (facade) with the current listitems
        private void updateMovieBrowser() {

            // If the listFilterString contains characters
            // filter the facade using the current filter string
            if (!String.IsNullOrEmpty(_listFilterString)) {
                logger.Debug("List Filter Active: '{0}'", _listFilterString);

                // This is the "contains" predicate
                // @todo: StartsWith, EndsWith etc.. ?
                Predicate<GUIListItem> predicate = delegate(GUIListItem item) {
                    return (NumPadEncode(item.Label).Contains(_listFilterString));
                };
                // Filter the list with the specified critera
                List<GUIListItem> filteredList = _listItems.FindAll(predicate);
                // fill movieBrowser (facade) with the filtered list.
                updateMovieBrowser(filteredList);

            }
            else {
                // fill movieBrowser (facade) with all items.
                updateMovieBrowser(_listItems);
            }
        }

        // fills the movieBrowser (facade) with the given listitems
        private void updateMovieBrowser(List<GUIListItem> items) {
            // Clear the movie browser control (facade)
            GUIControl.ClearControl(GetID, movieBrowser.GetID);

            // Add the items
            foreach (GUIListItem item in items)
                movieBrowser.Add(item);

            // if our selection has changed, update it
            if (SelectedMovie != movieBrowser.SelectedListItem.TVTag)
                SelectedMovie = movieBrowser.SelectedListItem.TVTag as DBMovieInfo;
        }
        
        #endregion

        #region Skin and Property Settings

        private void SetProperty(string property, string value) {
            if (!loggedProperties.ContainsKey(property)) {
                logger.Debug(property + " = \"" + value + "\"");
                loggedProperties[property] = true;
            }

            // If the value is empty always add a space
            // otherwise the property will keep 
            // displaying it's previous value
            if (String.IsNullOrEmpty(value))
                value = "";

            GUIPropertyManager.SetProperty(property, value);
        }

        // Updates the movie metadata on the playback screen (for when the user clicks info). 
        // The delay is neccisary because Player tries to use metadata from the MyVideos database.
        // We want to update this after that happens so the correct info is there.
        private void UpdatePlaybackInfo() {
            Thread.Sleep(2000);
            if (currentMovie != null) {
                SetProperty("#Play.Current.Title", currentMovie.Title);
                SetProperty("#Play.Current.Plot", currentMovie.Summary);
                SetProperty("#Play.Current.Thumb", currentMovie.CoverThumbFullPath);
                SetProperty("#Play.Current.Year", currentMovie.Year.ToString());

                if (currentMovie.Genres.Count > 0)
                    SetProperty("#Play.Current.Genre", currentMovie.Genres[0]);
                else
                    SetProperty("#Play.Current.Genre", "");

            }
        }

        // this does standard object publishing for any database object.
        private void PublishDetails(DatabaseTable obj, string prefix) {
            int maxStringListElements = (int)MovingPicturesCore.SettingsManager["max_string_list_items"].Value;

            Type tableType = obj.GetType();
            foreach (DBField currField in DBField.GetFieldList(tableType)) {
                string propertyStr;
                string valueStr;

                // for string lists, lets do some nice formating
                object value = currField.GetValue(obj);
                if (value.GetType() == typeof(StringList)) {

                    // make sure we dont go overboard with listing elements :P
                    StringList valueStrList = (StringList)value;
                    int max = maxStringListElements;
                    if (max > valueStrList.Count)
                        max = valueStrList.Count;

                    // add the coma seperated string
                    propertyStr = "#MovingPictures." + prefix + "." + currField.FieldName;
                    valueStr = valueStrList.ToPrettyString(max);
                    SetProperty(propertyStr, valueStr);

                    // add each value individually
                    for (int i = 0; i < max; i++) {
                        // note, the "extra" in the middle is needed due to a bug in skin parser
                        propertyStr = "#MovingPictures." + prefix + ".extra." + currField.FieldName + "." + (i + 1);
                        valueStr = valueStrList[i];
                        SetProperty(propertyStr, valueStr);
                    }

                    // vanilla publication
                }
                else {
                    propertyStr = "#MovingPictures." + prefix + "." + currField.FieldName;
                    valueStr = currField.GetValue(obj).ToString().Trim();
                    SetProperty(propertyStr, valueStr);
                }
            }

            PublishBonusDetails(obj, prefix);
        }

        // publishing for special fields not specifically in the database
        private void PublishBonusDetails(DatabaseTable obj, string prefix) {
            string propertyStr;
            string valueStr;

            if (obj is DBMovieInfo) {
                DBMovieInfo movie = (DBMovieInfo)obj;
                int minValue;
                int hourValue;

                // hour component of runtime
                propertyStr = "#MovingPictures." + prefix + ".extra.runtime.hour";
                hourValue = (movie.Runtime / 60);
                SetProperty(propertyStr, hourValue.ToString());

                // minute component of runtime
                propertyStr = "#MovingPictures." + prefix + ".extra.runtime.minute";
                minValue = (movie.Runtime % 60);
                SetProperty(propertyStr, minValue.ToString());

                // give short runtime string 0:00
                propertyStr = "#MovingPictures." + prefix + ".extra.runtime.short";
                valueStr = string.Format("{0}:{1:00}", hourValue, minValue);
                SetProperty(propertyStr, valueStr);

                // give pretty runtime string
                propertyStr = "#MovingPictures." + prefix + ".extra.runtime.en.pretty";
                valueStr = string.Empty;
                if (hourValue > 0)
                    valueStr = string.Format("{0} hour{1}", hourValue, hourValue != 1 ? "s" : string.Empty);
                if (minValue > 0)
                    valueStr = valueStr + string.Format(", {0} minute{1}", minValue, minValue != 1 ? "s" : string.Empty);
                SetProperty(propertyStr, valueStr);
            }
        }

        // Grabs the <define> tags from the skin for skin parameters from skinner.
        private void LoadDefinesFromSkin() {
            try {
                // Load the XML file
                XmlDocument doc = new XmlDocument();
                logger.Info("Loading defines from skin.");
                doc.Load(_windowXmlFileName);

                // parse out the define tags and store them
                defines = new Dictionary<string, string>();
                foreach (XmlNode node in doc.SelectNodes("/window/define")) {
                    string[] tokens = node.InnerText.Split(':');

                    if (tokens.Length < 2)
                        continue;

                    defines[tokens[0]] = tokens[1];
                    logger.Debug("Loaded define from skin: " + tokens[0] + ": " + tokens[1]);
                }
            }
            catch (Exception e) {
                logger.ErrorException("Unexpected error loading <define> tags from skin file.", e);
            }
        }

        #endregion

        public static string NumPadEncode(string input) {
            string rtn = input.Trim();
            rtn = Regex.Replace(rtn, @"[abc]", "2", RegexOptions.IgnoreCase);
            rtn = Regex.Replace(rtn, @"[def]", "3", RegexOptions.IgnoreCase);
            rtn = Regex.Replace(rtn, @"[ghi]", "4", RegexOptions.IgnoreCase);
            rtn = Regex.Replace(rtn, @"[jkl]", "5", RegexOptions.IgnoreCase);
            rtn = Regex.Replace(rtn, @"[mno]", "6", RegexOptions.IgnoreCase);
            rtn = Regex.Replace(rtn, @"[pqrs]", "7", RegexOptions.IgnoreCase);
            rtn = Regex.Replace(rtn, @"[tuv]", "8", RegexOptions.IgnoreCase);
            rtn = Regex.Replace(rtn, @"[wxyz]", "9", RegexOptions.IgnoreCase);
            rtn = Regex.Replace(rtn, @"[\s]", "0", RegexOptions.IgnoreCase);
            return rtn;
        }

    }


    public class GUIListItemMovieComparer : IComparer<GUIListItem> {
        public int Compare(GUIListItem x, GUIListItem y) {
            try {
                DBMovieInfo movieX = (DBMovieInfo)x.TVTag;
                DBMovieInfo movieY = (DBMovieInfo)y.TVTag;

                return movieX.SortBy.CompareTo(movieY.SortBy);
            }
            catch {
                return 0;
            }
        }
    }
}
