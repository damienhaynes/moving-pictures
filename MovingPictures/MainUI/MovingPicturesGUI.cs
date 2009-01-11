using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Xml;
using Cornerstone.Database;
using Cornerstone.Database.CustomTypes;
using Cornerstone.Database.Tables;
using Cornerstone.MP;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Plugins.MovingPictures.DataProviders;
using MediaPortal.Plugins.MovingPictures.MainUI.MovieBrowser;
using MediaPortal.Profile;
using MediaPortal.Util;
using MediaPortal.Ripper;
using NLog;
using System.IO;

namespace MediaPortal.Plugins.MovingPictures {
    public class MovingPicturesGUI : GUIWindow {
        public enum ViewMode { LIST, SMALLICON, LARGEICON, FILMSTRIP, DETAILS }

        public enum DiskInsertedAction { PLAY, DETAILS, NOTHING }

        #region Private Variables

        private static Logger logger = LogManager.GetCurrentClassLogger();

        MovieBrowser browser;
        RemoteNumpadFilter remoteFilter;
        WatchedFlagFilter watchedFilter;

        Dictionary<string, string> defines;
        Dictionary<string, bool> loggedProperties;

        private bool currentlyPlaying = false;
        private DBMovieInfo currentlyPlayingMovie;
        private int currentlyPlayingPart = 1;

        private bool loaded = false;

        private bool customIntroPlayed = false;

        private ImageSwapper backdrop;
        private AsyncImageResource cover = null;

        private DiskInsertedAction diskInsertedAction;
        Dictionary<string, string> recentInsertedDiskSerials;

        #endregion

        #region GUI Controls

        [SkinControl(50)]
        protected GUIFacadeControl facade = null;

        [SkinControl(1)]
        protected GUIImage movieBackdropControl = null;

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

        [SkinControl(8)]
        protected GUILabelControl watchedFilteringIndicator = null;

        [SkinControl(9)]
        protected GUILabelControl selectedMovieWatchedIndicator = null;

        [SkinControl(10)]
        protected GUILabelControl remoteFilteringIndicator = null;

        [SkinControl(11)]
        protected GUIImage movieBackdropControl2 = null;

        [SkinControl(12)]
        protected GUIImage loadingImage = null;

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
                    facade.Focus = true;
                    facade.Visible = true;
                }

                switch (currentView) {
                    case ViewMode.LIST:
                        facade.View = GUIFacadeControl.ViewMode.List;
                        break;
                    case ViewMode.SMALLICON:
                        facade.View = GUIFacadeControl.ViewMode.SmallIcons;
                        break;
                    case ViewMode.LARGEICON:
                        facade.View = GUIFacadeControl.ViewMode.LargeIcons;
                        break;
                    case ViewMode.FILMSTRIP:
                        facade.View = GUIFacadeControl.ViewMode.Filmstrip;
                        break;
                    case ViewMode.DETAILS:
                        ClearFocus();

                        facade.Visible = false;
                        facade.ListView.Visible = false;
                        facade.ThumbnailView.Visible = false;
                        facade.FilmstripView.Visible = false;

                        playButton.Focus = true;
                        break;
                }

                if (facade.SelectedListItem != null)
                    updateMovieDetails(facade.SelectedListItem.TVTag as DBMovieInfo);

                SetBackdropVisibility();
                UpdateArtwork();
            }
        }
        private ViewMode currentView;
        private ViewMode previousView;

        public MovingPicturesGUI() {
            g_Player.PlayBackEnded += new g_Player.EndedHandler(OnPlayBackEnded);
            g_Player.PlayBackStopped += new g_Player.StoppedHandler(OnPlayBackStoppedOrChanged);

            // This is a handler added in RC4 - if we are using an older mediaportal version
            // this would throw an exception.
            try {
                g_Player.PlayBackChanged += new g_Player.ChangedHandler(OnPlayBackStoppedOrChanged);
            }
            catch (Exception) {
                logger.Error("Running MediaPortal 1.0 RC3 or earlier. Unexpected behavior may occur when starting playback of a new movie without stopping previous movie. Please upgrade for better performance.");
            }

            // Get Moving Pictures specific autoplay setting
            try {
                diskInsertedAction = (DiskInsertedAction) Enum.Parse(typeof(DiskInsertedAction), MovingPicturesCore.SettingsManager["on_disc_loaded"].StringValue);
            } catch {
                diskInsertedAction = DiskInsertedAction.DETAILS;
            }

            // setup the image resources for cover and backdrop display
            int artworkDelay = (int)MovingPicturesCore.SettingsManager["gui_artwork_delay"].Value;

            backdrop = new ImageSwapper();
            backdrop.ImageResource.Delay = artworkDelay;
            backdrop.PropertyOne = "#MovingPictures.Backdrop";

            cover = new AsyncImageResource();
            cover.Property = "#MovingPictures.Coverart";
            cover.Delay = artworkDelay;

            // used to prevent overzealous logging of skin properties
            loggedProperties = new Dictionary<string, bool>();
        }

        ~MovingPicturesGUI() {
        }

        private void UpdateArtwork() {
            if (browser.SelectedMovie == null)
                return;

            // update resources with new files
            cover.Filename = browser.SelectedMovie.CoverFullPath;
            backdrop.Filename = browser.SelectedMovie.BackdropFullPath;
        }

        private void SetBackdropVisibility() {
            if (movieBackdropControl == null)
                return;

            // grab the skin supplied setting for backdrop visibility
            bool backdropActive = true;
            try {
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
            }
            catch (KeyNotFoundException) {
                backdropActive = true;
            }

            // set backdrop visibility
            backdrop.Active = backdropActive;
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
                case ViewMode.DETAILS:
                    logger.Debug("DETAILS: " + defines["#filmstrip.available"].Equals("true"));
                    return true;
                default:
                    logger.Debug("DEFAULT: false");
                    return false;
            }
        }

        private void ClearFocus() {
            if (facade != null) {
                facade.Focus = false;
                if (facade.ListView != null) facade.ListView.Focus = false;
                if (facade.ThumbnailView != null) facade.ThumbnailView.Focus = false;
                if (facade.FilmstripView != null) facade.FilmstripView.Focus = false;
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

        private void OnBrowserContentsChanged() {
            // update properties
            PublishViewDetails();

            // set the global watched indicator
            if (watchedFilteringIndicator != null && watchedFilter.Active != watchedFilteringIndicator.Visible)
                watchedFilteringIndicator.Visible = watchedFilter.Active;

            // set the label for the remoteFiltering indicator
            if (remoteFilteringIndicator != null && remoteFilter.Active) {
                SetProperty("#MovingPictures.Filter.Label", remoteFilter.Text);
                remoteFilteringIndicator.Visible = true;
            }
            else
                remoteFilteringIndicator.Visible = false;
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

            logger.Info("GUI Initialization Complete");
            return success;
        }

        public override void DeInit() {
            base.DeInit();
            MovingPicturesCore.Shutdown();
        }

        protected override void OnPageLoad() {
            // if the component didn't load properly we probably have a bad skin file
            if (facade == null) {
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
                dialog.SetLine(1, "It doesn't look like you have");
                dialog.SetLine(2, "defined any import paths. You");
                dialog.SetLine(3, "should close MediaPortal and");
                dialog.SetLine(4, "launch the MediaPortal");
                dialog.SetLine(5, "Configuration Screen to");
                dialog.SetLine(6, "configure Moving Pictures.");
                dialog.DoModal(GetID);
                GUIWindowManager.ShowPreviousWindow();
                return;
            }

            if (browser == null) {
                browser = new MovieBrowser();

                remoteFilter = new RemoteNumpadFilter();
                browser.ActiveFilters.Add(remoteFilter);

                watchedFilter = new WatchedFlagFilter();
                browser.ActiveFilters.Add(watchedFilter);

                // if option is set, turn on the watched movies filter by default
                bool startWithWatchedFilterOn = (bool)MovingPicturesCore.SettingsManager["start_watched_filter_on"].Value;
                if (startWithWatchedFilterOn)
                    watchedFilter.Active = true;

                browser.SelectionChanged += new MovieBrowser.SelectionChangedDelegate(updateMovieDetails);
                browser.ContentsChanged += new MovieBrowser.ContentsChangedDelegate(OnBrowserContentsChanged);

            }

            // start the device monitor
            if (!MovingPicturesCore.DeviceManager.MonitorStarted) {
                MovingPicturesCore.DeviceManager.Handle = GUIGraphicsContext.form.Handle;
                MovingPicturesCore.DeviceManager.StartMonitor();
                
                // Listen to the DeviceManager
                logger.Debug("Listening for device changes.");
                MovingPicturesCore.DeviceManager.OnVolumeInserted += new DeviceManager.DeviceManagerEvent(OnVolumeInserted);
                MovingPicturesCore.DeviceManager.OnVolumeRemoved += new DeviceManager.DeviceManagerEvent(OnVolumeRemoved);

            }

            if (recentInsertedDiskSerials == null) {              
                // Also listen to new movies added as part of the autoplay/details functionality
                if (diskInsertedAction != DiskInsertedAction.NOTHING) {
                    recentInsertedDiskSerials = new Dictionary<string, string>();
                    MovingPicturesCore.DatabaseManager.ObjectInserted += new DatabaseManager.ObjectAffectedDelegate(OnMovieAdded);
                    logger.Debug("Autoplay/details is now listening for movie additions");
                }
            }

            OnBrowserContentsChanged();

            browser.Facade = facade;
            facade.Focus = true;

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

            // if we have loaded before, lets update the view to match our previous settings
            else {
                ViewMode tmp = previousView;
                CurrentView = CurrentView;
                previousView = tmp;
            }

            // (re)link our backdrio image controls to the backdrop image swapper
            backdrop.GUIImageOne = movieBackdropControl;
            backdrop.GUIImageTwo = movieBackdropControl2;
            backdrop.LoadingImage = loadingImage;


            // load fanart and coverart
            UpdateArtwork();

            // Take control and disable MediaPortal AutoPlay when the plugin has focus
            disableNativeAutoplay();
        }

        protected override void OnPageDestroy(int new_windowId) {
            // Enable autoplay again when we are leaving the plugin
            // But only when the new window is NOT the fullscreen window.
            if (new_windowId != (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO)
                enableNativeAutoplay();

            base.OnPageDestroy(new_windowId);
        }

        // Disable MediaPortal's AutoPlay
        private void disableNativeAutoplay() {
            logger.Info("Disabling native autoplay.");
            AutoPlay.StopListening();
        }

        // Enable MediaPortal's AutoPlay
        private void enableNativeAutoplay() {
            if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING) {
                logger.Info("Re-enabling native autoplay.");
                AutoPlay.StartListening();
            }
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
                            if (control == facade) {
                                if (clickToDetails)
                                    CurrentView = ViewMode.DETAILS;
                                else
                                    playSelectedMovie();
                            }
                            break;
                        case Action.ActionType.ACTION_SHOW_INFO:
                            if (control == facade)
                            {
                                CurrentView = ViewMode.DETAILS;
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
                        // return to the facade screen
                        CurrentView = previousView;
                    else if (remoteFilter.Active)
                        // if a remote filter is active remove it
                        remoteFilter.Clear();
                    else {
                        // show previous screen (exit the plug-in
                        currentlyPlaying = false;
                        GUIWindowManager.ShowPreviousWindow();
                    }
                    break;
                case Action.ActionType.ACTION_MUSIC_PLAY:
                    // don't be confused, this is the generic PLAY action
                    playSelectedMovie();
                    break;
                case Action.ActionType.ACTION_KEY_PRESSED:
                    // List Filter
                    bool remoteFilterEnabled = (bool)MovingPicturesCore.SettingsManager["enable_rc_filter"].Value;
                    if ((action.m_key != null) && (CurrentView != ViewMode.DETAILS) && remoteFilterEnabled) {
                        if (!remoteFilter.KeyPress((char)action.m_key.KeyChar))
                            base.OnAction(action);
                    }
                    else {
                        base.OnAction(action);
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
            // WARNING/TODO
            // If any items are added to this menu conditionally, the current setup will break.
            // the method structure needs to be changed to mimic the showDetailsContext() method
            // if ANY conditional items are added.

            IDialogbox dialog = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dialog.Reset();
            dialog.SetHeading("Moving Pictures");

            GUIListItem watchItem = new GUIListItem(watchedFilter.Active ? "Show All Movies" : "Show Only Unwatched Movies");
            watchItem.ItemId = 1;
            dialog.Add(watchItem);

            GUIListItem sortItem = new GUIListItem("Sort By");
            sortItem.ItemId = 2;
            dialog.Add(sortItem);

            GUIListItem listItem = new GUIListItem("List View");
            listItem.ItemId = 3;
            dialog.Add(listItem);

            GUIListItem thumbItem = new GUIListItem("Thumbnail View");
            thumbItem.ItemId = 4;
            dialog.Add(thumbItem);

            GUIListItem largeThumbItem = new GUIListItem("Large Thumbnail View");
            largeThumbItem.ItemId = 5;
            dialog.Add(largeThumbItem);

            GUIListItem filmItem = new GUIListItem("Filmstrip View");
            filmItem.ItemId = 6;
            dialog.Add(filmItem);

            dialog.DoModal(GUIWindowManager.ActiveWindow);
            switch (dialog.SelectedId) {
                case 1:
                    watchedFilter.Active = !watchedFilter.Active;
                    break;

                case 2:
                    showSortContext();
                    break;
                case 3:
                    CurrentView = ViewMode.LIST;
                    break;
                case 4:
                    CurrentView = ViewMode.SMALLICON;
                    break;
                case 5:
                    CurrentView = ViewMode.LARGEICON;
                    break;
                case 6:
                    CurrentView = ViewMode.FILMSTRIP;
                    break;
            }

        }

        private void showSortContext() {

            IDialogbox dialog = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dialog.Reset();
            dialog.SetHeading("Moving Pictures - Sort By");

            GUIListItem titleItem = new GUIListItem("Title");
            titleItem.ItemId = 1;
            dialog.Add(titleItem);

            GUIListItem dateAddedItem = new GUIListItem("Date Added");
            dateAddedItem.ItemId = 2;
            dialog.Add(dateAddedItem);

            GUIListItem yearItem = new GUIListItem("Year");
            yearItem.ItemId = 3;
            dialog.Add(yearItem);

            GUIListItem certificationItem = new GUIListItem("Certification");
            certificationItem.ItemId = 4;
            dialog.Add(certificationItem);

            GUIListItem languageItem = new GUIListItem("Language");
            languageItem.ItemId = 5;
            dialog.Add(languageItem);

            GUIListItem scoreItem = new GUIListItem("Score");
            scoreItem.ItemId = 6;
            dialog.Add(scoreItem);

            GUIListItem userScoreItem = new GUIListItem("User Score");
            userScoreItem.ItemId = 7;
            dialog.Add(userScoreItem);

            GUIListItem popularityItem = new GUIListItem("Popularity");
            popularityItem.ItemId = 8;
            dialog.Add(popularityItem);

            GUIListItem runtimeItem = new GUIListItem("Runtime");
            runtimeItem.ItemId = 9;
            dialog.Add(runtimeItem);

            GUIListItem filePathItem = new GUIListItem("File Path");
            filePathItem.ItemId = 10;
            dialog.Add(filePathItem);

            dialog.DoModal(GUIWindowManager.ActiveWindow);

            GUIListItemMovieComparer.SortingFields newSortField = GUIListItemMovieComparer.SortingFields.Title;
            GUIListItemMovieComparer.SortingDirections defaultSortDirection = GUIListItemMovieComparer.SortingDirections.Ascending;

            switch (dialog.SelectedId) {
                case 1:
                    newSortField = GUIListItemMovieComparer.SortingFields.Title;
                    break;

                case 2:
                    newSortField = GUIListItemMovieComparer.SortingFields.DateAdded;
                    defaultSortDirection = GUIListItemMovieComparer.SortingDirections.Descending;
                    break;

                case 3:
                    newSortField = GUIListItemMovieComparer.SortingFields.Year;
                    break;

                case 4:
                    newSortField = GUIListItemMovieComparer.SortingFields.Certification;
                    break;

                case 5:
                    newSortField = GUIListItemMovieComparer.SortingFields.Language;
                    break;

                case 6:
                    newSortField = GUIListItemMovieComparer.SortingFields.Score;
                    break;

                case 7:
                    newSortField = GUIListItemMovieComparer.SortingFields.UserScore;
                    break;

                case 8:
                    newSortField = GUIListItemMovieComparer.SortingFields.Popularity;
                    break;

                case 9:
                    newSortField = GUIListItemMovieComparer.SortingFields.Runtime;
                    break;

                case 10:
                    newSortField = GUIListItemMovieComparer.SortingFields.FilePath;
                    break;

                default:
                    // do nothing
                    return;
                    break;
            }

            if (browser.SortField == newSortField) {
                // toggle sort direction
                if (browser.SortDirection == GUIListItemMovieComparer.SortingDirections.Ascending)
                    browser.SortDirection = GUIListItemMovieComparer.SortingDirections.Descending;
                else
                    browser.SortDirection = GUIListItemMovieComparer.SortingDirections.Ascending;
            }
            else {
                browser.SortField = newSortField;
                browser.SortDirection = defaultSortDirection;
            }

            browser.ReloadFacade();
        }

        private void showDetailsContext() {
            IDialogbox dialog = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dialog.Reset();
            dialog.SetHeading("Moving Pictures");

            // initialize list items
            GUIListItem detailsItem = new GUIListItem();
            GUIListItem cycleArtItem = new GUIListItem();
            GUIListItem retrieveArtItem = new GUIListItem();
            GUIListItem unwatchedItem = new GUIListItem();
            GUIListItem watchedItem = new GUIListItem();

            int currID = 1;

            detailsItem = new GUIListItem("Update Details from Online");
            detailsItem.ItemId = currID;
            dialog.Add(detailsItem);
            currID++;

            if (browser.SelectedMovie.AlternateCovers.Count > 1) {
                cycleArtItem = new GUIListItem("Cycle Cover-Art");
                cycleArtItem.ItemId = currID;
                dialog.Add(cycleArtItem);
                currID++;
            }

            if (browser.SelectedMovie.CoverFullPath.Trim().Length == 0 ||
                browser.SelectedMovie.BackdropFullPath.Trim().Length == 0) {

                retrieveArtItem = new GUIListItem("Check for Missing Artwork Online");
                retrieveArtItem.ItemId = currID;
                dialog.Add(retrieveArtItem);
                currID++;
            }

            if (browser.SelectedMovie.UserSettings[0].Watched > 0) {
                unwatchedItem = new GUIListItem("Mark as Unwatched");
                unwatchedItem.ItemId = currID;
                dialog.Add(unwatchedItem);
                currID++;
            }
            else {
                watchedItem = new GUIListItem("Mark as Watched");
                watchedItem.ItemId = currID;
                dialog.Add(watchedItem);
                currID++;
            }


            dialog.DoModal(GUIWindowManager.ActiveWindow);
            if (dialog.SelectedId == detailsItem.ItemId) {
                updateDetails();
            }
            else if (dialog.SelectedId == cycleArtItem.ItemId) {
                browser.SelectedMovie.NextCover();
                browser.SelectedMovie.Commit();

                // update the new cover art in the facade
                browser.facade.SelectedListItem.IconImage = browser.SelectedMovie.CoverThumbFullPath.Trim();
                browser.facade.SelectedListItem.IconImageBig = browser.SelectedMovie.CoverThumbFullPath.Trim();

                UpdateArtwork();
            }
            else if (dialog.SelectedId == retrieveArtItem.ItemId) {
                logger.Info("Updating artwork for " + browser.SelectedMovie.Title);
                retrieveMissingArt();
            }
            else if (dialog.SelectedId == unwatchedItem.ItemId) {
                browser.SelectedMovie.UserSettings[0].Watched = 0;

                browser.SelectedMovie.UserSettings[0].Commit();
                browser.ReapplyFilters();
                browser.ReloadFacade();
            }
            else if (dialog.SelectedId == watchedItem.ItemId) {
                browser.SelectedMovie.UserSettings[0].Watched = 1;

                browser.SelectedMovie.UserSettings[0].Commit();
                browser.ReapplyFilters();
                browser.ReloadFacade();
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

            if (dialog.IsConfirmed && browser.SelectedMovie != null) {
                MovingPicturesCore.DataProviderManager.Update(browser.SelectedMovie);
                browser.SelectedMovie.Commit();
                PublishDetails(browser.SelectedMovie, "SelectedMovie");
            }


        }

        // retrieves from online artwork for the currently selected movie
        // if and only if no artwork currently exists.
        private void retrieveMissingArt() {
            if (browser.SelectedMovie == null)
                return;

            if (browser.SelectedMovie.CoverFullPath.Trim().Length == 0) {
                MovingPicturesCore.DataProviderManager.GetArtwork(browser.SelectedMovie);
                browser.SelectedMovie.UnloadArtwork();
                browser.SelectedMovie.Commit();
            }

            if (browser.SelectedMovie.BackdropFullPath.Trim().Length == 0) {
                new LocalProvider().GetBackdrop(browser.SelectedMovie);
                MovingPicturesCore.DataProviderManager.GetBackdrop(browser.SelectedMovie);
                browser.SelectedMovie.Commit();
            }

            UpdateArtwork();
        }

        #endregion

        #region Playback Methods

        private void playSelectedMovie() {
            // make sure we have a valid movie selected
            if (browser.SelectedMovie == null) {
                logger.Error("Tried to play when there is no selected movie!");
                return;
            }

            // try playing our custom intro (if present). If successful quit, as we need to
            // wait for the intro to finish.
            bool success = playCustomIntro();
            if (success) return;
            
            playMovie(browser.SelectedMovie, 1);
         }

        private void playMovie(DBMovieInfo movie, int requestedPart) {
            if (movie == null || requestedPart > movie.LocalMedia.Count || requestedPart < 1) 
                return;

            int part = requestedPart;
            bool resume = false;

            // if this is a request to start the movie from the begining, check if we should resume
            // or prompt the user for disk selection
            if (requestedPart == 1) {
                // check if we should be resuming, and if not, clear resume data
                resume = PromptUserToResume(movie);
                if (resume)
                    part = movie.UserSettings[0].ResumePart;
                else
                    clearMovieResumeState(movie);

                // if we have a multi-part movie composed of disk images and we are not resuming 
                // ask which part the user wants to play
                string firstExtension = movie.LocalMedia[0].File.Extension;
                if (!resume && movie.LocalMedia.Count > 1 && (DaemonTools.IsImageFile(firstExtension) || firstExtension == ".ifo")) {
                    GUIDialogFileStacking dlg = (GUIDialogFileStacking)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_FILESTACKING);
                    if (null != dlg) {
                        dlg.SetNumberOfFiles(movie.LocalMedia.Count);
                        dlg.DoModal(GUIWindowManager.ActiveWindow);
                        part = dlg.SelectedFile;
                        if (part < 1) return;
                    }
                }
            }


            // if the media is missing, alert the user and quit.
            DBLocalMedia mediaToPlay = movie.LocalMedia[part - 1];
            if (!mediaToPlay.IsAvailable) {
                ShowMessage("Media Not Available",
                            "The media for the Movie you have selected is not",
                            "currently available. Please insert or connect media ",
                            "labeled: " + mediaToPlay.MediaLabel, null);
                return;
            }
            else if (mediaToPlay.IsRemoved) {
                ShowMessage("Error",
                            "The media for the Movie you have selected is missing!",
                            "Very sorry but something has gone wrong...", null, null);
                return;
            }


            // start playback
            logger.Info("Playing {0} ({1})", movie.Title, mediaToPlay.FullPath);
            string filename = mediaToPlay.FullPath;
            string extension = mediaToPlay.File.Extension;
            if (DaemonTools.IsImageFile(extension))
                playImage(filename);
            else
                playFile(filename);


            // if playback started
            if (currentlyPlaying) {
                // set class level variables to track what is playing
                currentlyPlayingMovie = movie;
                currentlyPlayingPart = part;

                Thread newThread = new Thread(new ThreadStart(UpdatePlaybackInfo));
                newThread.Start();

                // grab the duration of this file
                updateMediaDuration(mediaToPlay);

                // and jump to our resume position if necessary
                if (resume && g_Player.Playing) {  
                    if (g_Player.IsDVD) 
                        g_Player.Player.SetResumeState(movie.UserSettings[0].ResumeData.Data);
                    else {
                        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SEEK_POSITION, 0, 0, 0, 0, 0, null);
                        msg.Param1 = movie.UserSettings[0].ResumeTime;
                        GUIGraphicsContext.SendMessage(msg);
                    }
                }
            }
        }

        private bool playCustomIntro() {
            // Check if we have already played a custom intro
            if (!customIntroPlayed) {
                // Only play custom intro for we are not resuming
                if (browser.SelectedMovie.UserSettings == null || browser.SelectedMovie.UserSettings.Count == 0 || browser.SelectedMovie.UserSettings[0].ResumeTime == 0) {
                    string custom_intro = (string)MovingPicturesCore.SettingsManager["custom_intro_location"].Value;

                    // Check if the custom intro is specified by user and exists
                    if (custom_intro.Length > 0 && File.Exists(custom_intro)) {
                        logger.Debug("Playing Custom Into: {0}", custom_intro);

                        // start playback
                        playFile(custom_intro);

                        // if playback started
                        if (currentlyPlaying) {
                            // set class level variables to track what is playing
                            customIntroPlayed = true;

                            Thread newThread = new Thread(new ThreadStart(UpdatePlaybackInfo));
                            newThread.Start();

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool PromptUserToResume(DBMovieInfo movie) {
            if (movie.UserSettings == null || movie.UserSettings.Count == 0 || movie.UserSettings[0].ResumeTime == 0)
                return false;

            // figure out the resume time to display to the user
            int displayTime = movie.UserSettings[0].ResumeTime;
            if (movie.LocalMedia.Count > 1) {
                for (int i = 0; i < movie.UserSettings[0].ResumePart - 1; i++) {
                    if (movie.LocalMedia[i].Duration > 0)
                        displayTime += movie.LocalMedia[i].Duration;
                }
            }

            GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (null == dlgYesNo) return false;
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); 
            dlgYesNo.SetLine(1, movie.Title);
            dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936) + " " + MediaPortal.Util.Utils.SecondsToHMSString(displayTime));
            dlgYesNo.SetDefaultToYes(true);
            dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

            if (dlgYesNo.IsConfirmed)
                return true;

            return false;
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

            // Check if the current image is already mounted
            if (!DaemonTools.IsMounted(media)) {
                // if not try to mount the image
                logger.Info("Trying to mount image.");
                if (!DaemonTools.Mount(media, out drive)) {
                    ShowMessage("Error", "Sorry, failed mounting DVD Image...", null, null, null);
                    return;
                }
                // We call this method to let the (un)mount events be handled by mediaportal
                // before we start playback. Not doing so would result in 
                // the DVD being stopped right after we start it.
                // todo: investigate if this is still needed with the autoplay listener stopped
                GUIWindowManager.Process();
            }
            else {
                logger.Info("DVD Image already mounted.");
                drive = DaemonTools.GetVirtualDrive();
            }

            // Try to grab a known video disc format
            string discPath = Utility.GetVideoDiscPath(drive);
            if (discPath == null) {
                ShowMessage("Error", "The image file does not contain a valid video disc format", null, null, null);
                return;
            }

            // play the file
            playFile(discPath);
        }

        // store the duration of the file if it is not set
        private void updateMediaDuration(DBLocalMedia localMedia) {
            if (localMedia.Duration == 0) {
                localMedia.Duration = (int)g_Player.Player.Duration;
                localMedia.Commit();
            }
        }

        private void OnPlayBackEnded(g_Player.MediaType type, string filename) {
            logger.Debug("OnPlayBackEnded");

            if (customIntroPlayed) {
                // If a custom intro was just played, we need to play the selected movie
                playSelectedMovie();

                // Set custom intro played back to false so it will play again on next movie selection
                customIntroPlayed = false;
                return;
            }

            if (type != g_Player.MediaType.Video || currentlyPlayingMovie == null)
                return;

            logger.Debug("OnPlayBackEnded filename={0} currentMovie={1} currentPart={2}", filename, currentlyPlayingMovie.Title, currentlyPlayingPart);
            clearMovieResumeState(currentlyPlayingMovie);
            if (currentlyPlayingMovie.LocalMedia.Count >= (currentlyPlayingPart + 1)) {
                logger.Debug("Goto next part");
                currentlyPlayingPart++;
                playMovie(currentlyPlayingMovie, currentlyPlayingPart);
            }
            else {
                updateMovieWatchedCounter(currentlyPlayingMovie);
                currentlyPlayingPart = 0;
                currentlyPlaying = false;
                currentlyPlayingMovie = null;
            }

            // If we or stopping in another windows enable native auto-play again
            // This will most of the time be the fullscreen playback window, 
            // if we would re-enter the plugin, autoplay be disabled again.
            if (GetID != GUIWindowManager.ActiveWindow)
                enableNativeAutoplay();
        }

        private void OnPlayBackStoppedOrChanged(g_Player.MediaType type, int timeMovieStopped, string filename) {
            logger.Debug("OnPlayBackStoppedOrChanged");
            if (type != g_Player.MediaType.Video || currentlyPlayingMovie == null)
                return;

            logger.Debug("OnPlayBackStoppedOrChanged: filename={1} currentMovie={1} currentPart={2} timeMovieStopped={3} ", filename, currentlyPlayingMovie.Title, currentlyPlayingPart, timeMovieStopped);

            // Because we can't get duration for DVD's at start like with normal files
            // we are getting the duration when the DVD is stopped. If the duration of 
            // feature is an hour or more it's probably the main feature and we will update
            // the database. 
            if (g_Player.IsDVD && (g_Player.Player.Duration >= 3600)) {
                DBLocalMedia playingFile = currentlyPlayingMovie.LocalMedia[currentlyPlayingPart - 1];
                updateMediaDuration(playingFile);
            }

            int requiredWatchedPercent = (int)MovingPicturesCore.SettingsManager["gui_watch_percentage"].Value;
            logger.Debug("Percentage: " + currentlyPlayingMovie.GetPercentage(currentlyPlayingPart, timeMovieStopped) + " Required: " + requiredWatchedPercent);

            // if enough of the movie has been watched, hit the watched flag
            if (currentlyPlayingMovie.GetPercentage(currentlyPlayingPart, timeMovieStopped) >= requiredWatchedPercent) {
                updateMovieWatchedCounter(currentlyPlayingMovie);
                clearMovieResumeState(currentlyPlayingMovie);
            }
            // otherwise, store resume data.
            else {
                byte[] resumeData = null;
                g_Player.Player.GetResumeState(out resumeData);
                updateMovieResumeState(currentlyPlayingMovie, currentlyPlayingPart, timeMovieStopped, resumeData);
            }

            currentlyPlayingPart = 0;
            currentlyPlaying = false;
            currentlyPlayingMovie = null;
            
            // If we or stopping in another windows enable native auto-play again
            // This will most of the time be the fullscreen playback window, 
            // if we would re-enter the plugin, autoplay be disabled again.
            if (GetID != GUIWindowManager.ActiveWindow)
                enableNativeAutoplay();
        }

        private void updateMovieWatchedCounter(DBMovieInfo movie) {
            if (movie == null)
                return;

            // get the user settings for the default profile (for now)
            DBUserMovieSettings userSetting = movie.UserSettings[0];
            userSetting.Watched++; // increment watch counter
            userSetting.Commit();

            browser.ReapplyFilters();

            // if we are on the details page for the movie just marked as watched and we are filtering
            // go back to facade since this movie is no longer selectable. later need to tweak to allow 
            // movies filtered out to be displayed in details anyway.
            if (movie == browser.SelectedMovie && CurrentView == ViewMode.DETAILS && watchedFilter.Active)
                CurrentView = previousView;
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

        #region DeviceManager Methods

        /// <summary>
        /// Handles action to be taken after a disk has been inserted and added to the database.
        /// </summary>
        private void HandleInsertedDisk(DBMovieInfo movie, int part) {
            switch (diskInsertedAction) {
                case DiskInsertedAction.DETAILS:
                    logger.Info("HandleInsertedDisk: Showing details for video disc: '{0}'", movie.Title);
                    browser.SelectedMovie = movie;
                    CurrentView = ViewMode.DETAILS;
                    break;
                case DiskInsertedAction.PLAY:
                    logger.Info("HandleInsertedDisk: Starting playback for video disc: '{0}'", movie.Title);
                    playMovie(movie, part);
                    break;
            }
        }

        /// <summary>
        /// Listens for newly added movies from the database manager. Used for triggering delayed 
        /// actions when a disk is inserted.
        /// </summary>
        /// <param name="obj"></param>
        private void OnMovieAdded(DatabaseTable obj) {
            // if not a movie, not a recently inserted disk, or if we aren't in Moving Pics, quit
            if (obj.GetType() != typeof(DBMovieInfo) || recentInsertedDiskSerials.Count == 0 || GUIWindowManager.ActiveWindow != GetID)
                return;

            DBMovieInfo movie = (DBMovieInfo)obj;

            if (movie.LocalMedia.Count == 0)
                return;

            string path = movie.LocalMedia[0].FullPath.ToLower();
            logger.Debug("OnMovieAdded: " + movie.Title);

            // Check if this path was recently inserted
            if (recentInsertedDiskSerials.ContainsKey(path))
                // Double check the serial (just in case)
                if (movie.LocalMedia[0].VolumeSerial == recentInsertedDiskSerials[path]) {
                    recentInsertedDiskSerials.Clear();
                    HandleInsertedDisk(movie, 1);
                    return;
                }
        }

        private void OnVolumeInserted(string volume, string serial) {
            // only respond when the plugin (or it's playback) is active
            if (GUIWindowManager.ActiveWindow != GetID && !currentlyPlaying)
                return;
            
            logger.Debug("onVolumeInserted: " + volume);

            if (GUIWindowManager.ActiveWindow == GetID) {

                // Clear recent disc information
                logger.Debug("Resetting Recent Disc Information.");
                recentInsertedDiskSerials.Clear();

                // DVD / Blu-ray 
                // Try to grab a valid video path from the disc
                string moviePath = Utility.GetVideoDiscPath(volume);
                DBMovieInfo movie = null;
                int part = 1;

                // if we have a video path a video disk was inserted.
                if (moviePath != null) {
                    logger.Info("Video Disc Detected.");

                    // Try to grab the movie from our DB
                    DBLocalMedia localMedia = DBLocalMedia.Get(moviePath, serial);
                    if (localMedia.ID != null) {
                        if (localMedia.AttachedMovies.Count > 0) {
                            // movie already exists in the database, so lets handle it
                            movie = localMedia.AttachedMovies[0];
                            part = localMedia.Part;
                            HandleInsertedDisk(movie, part);
                        }
                    }
                    else {
                        // This is a new disk, so lets keep track of it, in case it's added
                        // to the database by the importer.
                        // @todo: maybe just store a new DBLocalMedia object?
                        logger.Debug("Adding Recent Disc Information: {0} ({1})", moviePath.ToLower(), serial);
                        recentInsertedDiskSerials.Add(moviePath.ToLower(), serial);
                        return;
                    }
                }
            }

        }

        private void OnVolumeRemoved(string volume, string serial) {
            // only respond when the plugin (or it's playback) is active
            if (GUIWindowManager.ActiveWindow != GetID && !currentlyPlaying)
                return;
                
            // if we are playing something from this volume stop it
            if (currentlyPlaying)
                if (currentlyPlayingMovie.LocalMedia[currentlyPlayingPart].Volume == volume)
                    g_Player.Stop();

            logger.Debug("OnVolumeRemoved" + volume);
        }

        #endregion

        #region Skin and Property Settings
        
        private void updateMovieDetails(DBMovieInfo movie) {
            PublishDetails(movie, "SelectedMovie");

            if (selectedMovieWatchedIndicator != null)
                if (movie.UserSettings[0].Watched > 0)
                    selectedMovieWatchedIndicator.Visible = true;
                else
                    selectedMovieWatchedIndicator.Visible = false;

            UpdateArtwork();
        }

        private void SetProperty(string property, string value) {
            if (property == null)
                return;

            if (!loggedProperties.ContainsKey(property)) {
                logger.Debug(property + " = \"" + value + "\"");
                loggedProperties[property] = true;
            }

            // If the value is empty always add a space
            // otherwise the property will keep 
            // displaying it's previous value
            if (String.IsNullOrEmpty(value))
                value = " ";

            GUIPropertyManager.SetProperty(property, value);
        }

        // Updates the movie metadata on the playback screen (for when the user clicks info). 
        // The delay is neccisary because Player tries to use metadata from the MyVideos database.
        // We want to update this after that happens so the correct info is there.
        private void UpdatePlaybackInfo() {
            Thread.Sleep(2000);
            if (currentlyPlayingMovie != null && currentlyPlaying) {
                SetProperty("#Play.Current.Title", currentlyPlayingMovie.Title);
                SetProperty("#Play.Current.Plot", currentlyPlayingMovie.Summary);
                SetProperty("#Play.Current.Thumb", currentlyPlayingMovie.CoverThumbFullPath);
                SetProperty("#Play.Current.Year", currentlyPlayingMovie.Year.ToString());

                if (currentlyPlayingMovie.Genres.Count > 0)
                    SetProperty("#Play.Current.Genre", currentlyPlayingMovie.Genres[0]);
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

                    // for floats we need to make sure we use english style printing or imagelist controls
                    // will break. 
                }
                else if (value.GetType() == typeof(float)) {
                    propertyStr = "#MovingPictures." + prefix + "." + currField.FieldName;
                    valueStr = ((float)currField.GetValue(obj)).ToString(CultureInfo.CreateSpecificCulture("en-US"));
                    SetProperty(propertyStr, valueStr);

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

        // all details relating to the current view and filtering status
        private void PublishViewDetails() {
            string propertyStr;
            string valueStr;

            // total number of movies.
            propertyStr = "#MovingPictures.general.totalmoviecount";
            valueStr = browser.AllMovies.Count.ToString();
            SetProperty(propertyStr, valueStr);

            // number of movies in list based on filter
            propertyStr = "#MovingPictures.general.filteredmoviecount";
            valueStr = browser.FilteredMovies.Count.ToString();
            SetProperty(propertyStr, valueStr);
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

    }
}
