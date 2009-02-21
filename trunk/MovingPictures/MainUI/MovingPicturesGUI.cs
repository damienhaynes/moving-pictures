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
using MediaPortal.Plugins.MovingPictures.MainUI.Filters;
using MediaPortal.Profile;
using MediaPortal.Util;
using MediaPortal.Ripper;
using NLog;
using System.IO;
using System.Diagnostics;


namespace MediaPortal.Plugins.MovingPictures.MainUI {
    public class MovingPicturesGUI : GUIWindow {
        public enum DiskInsertedAction { PLAY, DETAILS, NOTHING }

        #region Private Variables

        private static Logger logger = LogManager.GetCurrentClassLogger();

        MovieBrowser browser;
        RemoteNumpadFilter remoteFilter;
        WatchedFlagFilter watchedFilter;

        MovingPicturesSkinSettings skinSettings;

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

        private bool waitingForMedia = false;
        private string waitingForMediaSerial;

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

        [SkinControl(13)]
        protected GUIAnimation workingAnimation = null;

        #endregion

        public MovingPicturesGUI() {
            g_Player.PlayBackEnded += new g_Player.EndedHandler(OnPlayBackEnded);
            g_Player.PlayBackStopped += new g_Player.StoppedHandler(OnPlayBackStoppedOrChanged);
            MediaPortal.Util.Utils.OnStartExternal += new MediaPortal.Util.Utils.UtilEventHandler(Utils_OnStartExternal);
            MediaPortal.Util.Utils.OnStopExternal += new MediaPortal.Util.Utils.UtilEventHandler(Utils_OnStopExternal);
            MovingPicturesCore.Importer.Progress += new MovieImporter.ImportProgressHandler(Importer_Progress);

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
                diskInsertedAction = (DiskInsertedAction) Enum.Parse(typeof(DiskInsertedAction), MovingPicturesCore.Settings.DiskInsertionBehavior);
            } catch {
                diskInsertedAction = DiskInsertedAction.DETAILS;
            }

            // setup the image resources for cover and backdrop display
            int artworkDelay = MovingPicturesCore.Settings.ArtworkLoadingDelay;

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

        // set the backdrop visibility based on the skin settings
        private void SetBackdropVisibility() {
            if (movieBackdropControl == null)
                return;

            backdrop.Active = skinSettings.UseBackdrop(browser.CurrentView);
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

        private void ShowMessage(string heading, string lines) {
            string line1 = null, line2 = null, line3 = null, line4 = null;
            string[] linesArray = lines.Split(new string[] { "\\n" }, StringSplitOptions.None);

            if (linesArray.Length >= 1) line1 = linesArray[0];
            if (linesArray.Length >= 2) line2 = linesArray[1];
            if (linesArray.Length >= 3) line3 = linesArray[2];
            if (linesArray.Length >= 4) line4 = linesArray[3];

            ShowMessage(heading, line1, line2, line3, line4);
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

        /// <summary>
        /// Displays a yes/no dialog with custom labels for the buttons
        /// This method may become obsolete in the future if media portal adds more dialogs
        /// </summary>
        /// <returns>True if yes was clicked, False if no was clicked</returns>
        private bool ShowCustomYesNo(string heading, string lines, string yesLabel, string noLabel, bool defaultYes) {
            GUIDialogYesNo dialog = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            try {
                dialog.Reset();
                dialog.SetHeading(heading);
                string[] linesArray = lines.Split(new string[] { "\\n" }, StringSplitOptions.None);
                if (linesArray.Length > 0) dialog.SetLine(1, linesArray[0]);
                if (linesArray.Length > 1) dialog.SetLine(2, linesArray[1]);
                if (linesArray.Length > 2) dialog.SetLine(3, linesArray[2]);
                if (linesArray.Length > 3) dialog.SetLine(4, linesArray[3]);
                dialog.SetDefaultToYes(defaultYes);

                foreach (System.Windows.UIElement item in dialog.Children) {
                    if (item is GUIButtonControl) {
                        GUIButtonControl btn = (GUIButtonControl)item;
                        if (btn.GetID == 11 && !String.IsNullOrEmpty(yesLabel)) // Yes button
                            btn.Label = yesLabel;
                        else if (btn.GetID == 10 && !String.IsNullOrEmpty(noLabel)) // No button
                            btn.Label = noLabel;
                    }
                }
                dialog.DoModal(GetID);

                return dialog.IsConfirmed;
            }
            finally {
                // set the standard yes/no dialog back to it's original state (yes/no buttons)
                if (dialog != null)
                    dialog.ClearAll();
            }
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
            else if (remoteFilteringIndicator != null)
                remoteFilteringIndicator.Visible = false;
        }

        private void OnBrowserSelectionChanged(DBMovieInfo movie) {
            UpdateMovieDetails();
        }

        private void OnBrowserViewChanged(BrowserViewMode previousView, BrowserViewMode currentView) {
            logger.Debug("OnBrowserViewChanged Started");
            
            if (currentView == BrowserViewMode.DETAILS) {
                ClearFocus();
                playButton.Focus = true;
            }

            UpdateMovieDetails();
            SetBackdropVisibility();
            UpdateArtwork();

            logger.Debug("OnBrowserViewChanged Ended");
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
            MovingPicturesCore.Initialize();

            // start the background importer
            if (MovingPicturesCore.Settings.EnableImporterInGUI)
                MovingPicturesCore.Importer.Start();

            // load skin based settings from skin file
            skinSettings = new MovingPicturesSkinSettings(_windowXmlFileName);

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
                dialog.SetHeading(Translation.ProblemLoadingSkinFile);
                dialog.DoModal(GetID);
                GUIWindowManager.ShowPreviousWindow();
                return;
            }

            // if the user hasn't defined any import paths they need to goto the config screen
            if (DBImportPath.GetAll().Count == 0) {
                ShowMessage(Translation.NoImportPathsHeading, Translation.NoImportPathsBody);
                GUIWindowManager.ShowPreviousWindow();
                return;
            }

            if (browser == null) {
                browser = new MovieBrowser(skinSettings);

                // add available filters to browser
                remoteFilter = new RemoteNumpadFilter();
                browser.ActiveFilters.Add(remoteFilter);

                watchedFilter = new WatchedFlagFilter();
                browser.ActiveFilters.Add(watchedFilter);

                // if option is set, turn on the watched movies filter by default
                if (MovingPicturesCore.Settings.ShowUnwatchedOnStartup)
                    watchedFilter.Active = true;

                // give the browser a delegate to the method to clear focus from all existing controls
                browser.ClearFocusAction = new MovieBrowser.ClearFocusDelegate(ClearFocus);

                // setup browser events
                browser.SelectionChanged += new MovieBrowser.SelectionChangedDelegate(OnBrowserSelectionChanged);
                browser.ContentsChanged += new MovieBrowser.ContentsChangedDelegate(OnBrowserContentsChanged);
                browser.ViewChanged +=new MovieBrowser.ViewChangedDelegate(OnBrowserViewChanged);

                SetProperty("#MovingPictures.Sort.Field", Sort.GetFriendlySortName(browser.CurrentSortField));
                SetProperty("#MovingPictures.Sort.Direction", browser.CurrentSortDirection.ToString());
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

            // first time setup tasks
            if (!loaded) {
                loaded = true;

                // Listen to the DeviceManager for external media activity (i.e. disks inserted)
                logger.Debug("Listening for device changes.");
                DeviceManager.OnVolumeInserted += new DeviceManager.DeviceManagerEvent(OnVolumeInserted);
                DeviceManager.OnVolumeRemoved += new DeviceManager.DeviceManagerEvent(OnVolumeRemoved);

                browser.CurrentView = browser.DefaultView;
            }

            // if we have loaded before, lets update the view to match our previous settings
            else
                browser.ReapplyView();
            

            // if we are not in details view (maybe we just came back from playing a movie)
            // set the first item in the list as selected.
            if (browser.CurrentView != BrowserViewMode.DETAILS) {
                facade.SelectedListItemIndex = 0;
                browser.SyncFromFacade();
            }
            
            setWorkingAnimationStatus(false);

            // (re)link our backdrop image controls to the backdrop image swapper
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
                    bool clickToDetails = MovingPicturesCore.Settings.ClickShowsDetails;

                    switch (actionType) {
                        case Action.ActionType.ACTION_SELECT_ITEM:
                            if (control == facade) {
                                if (clickToDetails)
                                    browser.CurrentView = BrowserViewMode.DETAILS;
                                else
                                    playSelectedMovie();
                            }
                            break;
                        case Action.ActionType.ACTION_SHOW_INFO:
                            if (control == facade) {
                                browser.CurrentView = BrowserViewMode.DETAILS;
                            }
                            break;
                    }
                    break;

                // a click on the rotate view button
                case 2:
                    browser.CycleView();
                    break;

                // a click on the view menu button
                case 3:
                    showChangeViewContext();
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
                    if (browser.CurrentView == BrowserViewMode.DETAILS)
                        // return to the facade screen
                        browser.CurrentView = browser.PreviousView;
                    else if (remoteFilter.Active)
                        // if a remote filter is active remove it
                        remoteFilter.Clear();
                    else {
                        // show previous screen (exit the plug-in
                        currentlyPlaying = false;
                        GUIWindowManager.ShowPreviousWindow();
                    }
                    break;
                case Action.ActionType.ACTION_PLAY:
                case Action.ActionType.ACTION_MUSIC_PLAY:
                    // don't be confused, this in some cases is the generic PLAY action
                    playSelectedMovie();
                    break;
                case Action.ActionType.ACTION_KEY_PRESSED:
                    // if remote filtering is active, try to route the keypress through the filter
                    bool remoteFilterEnabled = MovingPicturesCore.Settings.UseRemoteControlFiltering;
                    if (remoteFilterEnabled && browser.CurrentView != BrowserViewMode.DETAILS) {
                        // try to update the filter
                        bool changedFilter = false;
                        if (action.m_key != null)
                            changedFilter = remoteFilter.KeyPress((char)action.m_key.KeyChar);
                        
                        // if update failed (an incorrect key press?) use standard key processing
                        if (!changedFilter)
                            base.OnAction(action);
                    }
                    // basic keypress functionality
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
            switch (browser.CurrentView) {
                case BrowserViewMode.DETAILS:
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
            dialog.SetHeading("Moving Pictures");  // not translated because it's a proper noun

            GUIListItem watchItem = new GUIListItem(watchedFilter.Active ? Translation.ShowAllMovies : Translation.ShowOnlyUnwatchedMovies);
            watchItem.ItemId = 1;
            dialog.Add(watchItem);

            GUIListItem sortItem = new GUIListItem(Translation.SortBy + " ...");
            sortItem.ItemId = 2;
            dialog.Add(sortItem);

            GUIListItem viewItem = new GUIListItem(Translation.ChangeView + " ...");
            viewItem.ItemId = 3;
            dialog.Add(viewItem);

            dialog.DoModal(GUIWindowManager.ActiveWindow);
            switch (dialog.SelectedId) {
                case 1:
                    watchedFilter.Active = !watchedFilter.Active;
                    break;
                case 2:
                    showSortContext();
                    break;
                case 3:
                    showChangeViewContext();
                    break;
            }

        }

        private void showSortContext() {

            IDialogbox dialog = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dialog.Reset();
            dialog.SetHeading("Moving Pictures - " + Translation.SortBy);

            foreach (int value in Enum.GetValues(typeof(SortingFields))) {
                string menuCaption = Sort.GetFriendlySortName(
                    (SortingFields)Enum.Parse(typeof(SortingFields), value.ToString()));
                GUIListItem listItem = new GUIListItem(menuCaption);
                listItem.ItemId = value;
                dialog.Add(listItem);
            }

            dialog.DoModal(GUIWindowManager.ActiveWindow);
            if (dialog.SelectedId <= 0) return;  // user canceled out of menu

            SortingFields newSortField =
                (SortingFields)Enum.Parse(typeof(SortingFields), dialog.SelectedId.ToString());

            if (browser.CurrentSortField == newSortField) {
                // toggle sort direction
                if (browser.CurrentSortDirection == SortingDirections.Ascending)
                    browser.CurrentSortDirection = SortingDirections.Descending;
                else
                    browser.CurrentSortDirection = SortingDirections.Ascending;
            }
            else {
                browser.CurrentSortField = newSortField;
                browser.CurrentSortDirection = Sort.GetLastSortDirection(newSortField);
            }

            browser.ReloadFacade();
            SetProperty("#MovingPictures.Sort.Field", Sort.GetFriendlySortName(browser.CurrentSortField));
            SetProperty("#MovingPictures.Sort.Direction", browser.CurrentSortDirection.ToString());
            browser.Facade.SelectedListItemIndex = 0;
        }

        private void showChangeViewContext() {
            IDialogbox dialog = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dialog.Reset();
            dialog.SetHeading("Moving Pictures - " + Translation.ChangeView);

            int currID = 1;
            GUIListItem listItem = new GUIListItem(Translation.ListView);
            if (skinSettings.ListViewAvailable) {
                listItem.ItemId = currID++;
                dialog.Add(listItem);
            }

            GUIListItem thumbItem = new GUIListItem(Translation.ThumbnailView);
            if (skinSettings.IconViewAvailable) {
                thumbItem.ItemId = currID++;
                dialog.Add(thumbItem);
            }

            GUIListItem largeThumbItem = new GUIListItem(Translation.LargeThumbnailView);
            if (skinSettings.LargeIconViewAvailable) {
                largeThumbItem.ItemId = currID++;
                dialog.Add(largeThumbItem);
            }

            GUIListItem filmItem = new GUIListItem(Translation.FilmstripView);
            if (skinSettings.FilmstripViewAvailable) {
                filmItem.ItemId = currID++;
                dialog.Add(filmItem);
            }

            dialog.DoModal(GUIWindowManager.ActiveWindow);

            if (dialog.SelectedId == listItem.ItemId) {
                browser.CurrentView = BrowserViewMode.LIST;
            }
            else if (dialog.SelectedId == thumbItem.ItemId) {
                browser.CurrentView = BrowserViewMode.SMALLICON;
            }
            else if (dialog.SelectedId == largeThumbItem.ItemId) {
                browser.CurrentView = BrowserViewMode.LARGEICON;
            }
            else if (dialog.SelectedId == filmItem.ItemId) {
                browser.CurrentView = BrowserViewMode.FILMSTRIP;
            } 
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
            GUIListItem deleteItem = new GUIListItem();

            int currID = 1;

            detailsItem = new GUIListItem(Translation.UpdateDetailsFromOnline);
            detailsItem.ItemId = currID;
            dialog.Add(detailsItem);
            currID++;

            if (browser.SelectedMovie.AlternateCovers.Count > 1) {
                cycleArtItem = new GUIListItem(Translation.CycleCoverArt);
                cycleArtItem.ItemId = currID;
                dialog.Add(cycleArtItem);
                currID++;
            }

            if (browser.SelectedMovie.CoverFullPath.Trim().Length == 0 ||
                browser.SelectedMovie.BackdropFullPath.Trim().Length == 0) {

                retrieveArtItem = new GUIListItem(Translation.CheckForMissingArtwork);
                retrieveArtItem.ItemId = currID;
                dialog.Add(retrieveArtItem);
                currID++;
            }

            if (browser.SelectedMovie.UserSettings[0].Watched > 0) {
                unwatchedItem = new GUIListItem(Translation.MarkAsUnwatched);
                unwatchedItem.ItemId = currID;
                dialog.Add(unwatchedItem);
                currID++;
            }
            else {
                watchedItem = new GUIListItem(Translation.MarkAsWatched);
                watchedItem.ItemId = currID;
                dialog.Add(watchedItem);
                currID++;
            }

            if (MovingPicturesCore.Settings.AllowDelete) {
                deleteItem = new GUIListItem(Translation.DeleteMovie);
                deleteItem.ItemId = currID;
                dialog.Add(deleteItem);
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
                UpdateMovieDetails();
            }
            else if (dialog.SelectedId == watchedItem.ItemId) {
                browser.SelectedMovie.UserSettings[0].Watched = 1;

                browser.SelectedMovie.UserSettings[0].Commit();
                browser.ReapplyFilters();
                browser.ReloadFacade();
                UpdateMovieDetails();
            }
            else if (dialog.SelectedId == deleteItem.ItemId) {
                deleteMovie();
            }
        }

        private void deleteMovie() {
            DBLocalMedia firstFile = browser.SelectedMovie.LocalMedia[0];

            // if the file is available and read only, or known to be stored on optical media, prompt to ignore.
            if ((firstFile.IsAvailable && firstFile.File.IsReadOnly) || DeviceManager.GetVolumeInfo(firstFile.DriveLetter).DriveInfo.DriveType == DriveType.CDRom) {
                bool bIgnore = ShowCustomYesNo("Moving Pictures", Translation.CannotDeleteReadOnly, null, null, false);

                if (bIgnore) {
                    browser.SelectedMovie.DeleteAndIgnore();
                    if (browser.CurrentView == BrowserViewMode.DETAILS)
                        // return to the facade screen
                        browser.CurrentView = browser.PreviousView;
                }
                return;
            }


            // if the file is offline display an error dialog
            if (!firstFile.IsAvailable) {
                ShowMessage("Moving Pictures", String.Format(Translation.CannotDeleteOffline, browser.SelectedMovie.Title));
                return;
            }

            // if the file is available and not read only, confirm delete.
            string sDoYouWant = String.Format(Translation.DoYouWantToDelete, browser.SelectedMovie.Title);
            bool bConfirm = ShowCustomYesNo("Moving Pictures", sDoYouWant, null, null, false);

            if (bConfirm) {
                bool deleteSuccesful = browser.SelectedMovie.DeleteFiles();

                if (deleteSuccesful) {
                    if (browser.CurrentView == BrowserViewMode.DETAILS)
                        // return to the facade screen
                        browser.CurrentView = browser.PreviousView;
                }
                else {
                    ShowMessage("Moving Pictures", Translation.DeleteFailed, null, null, null);
                }
            }
        }

        // From online, updates the details of the currently selected movie.
        private void updateDetails() {
            bool bConfirm = ShowCustomYesNo(Translation.UpdateMovieDetailsHeader, Translation.UpdateMovieDetailsBody, null, null, false);

            if (bConfirm && browser.SelectedMovie != null) {
                MovingPicturesCore.DataProviderManager.Update(browser.SelectedMovie);
                browser.SelectedMovie.Commit();
                UpdateMovieDetails();
            }
        }

        // retrieves from online artwork for the currently selected movie
        // if and only if no artwork currently exists.
        private void retrieveMissingArt() {
            if (browser.SelectedMovie == null)
                return;

            if (browser.SelectedMovie.CoverFullPath.Trim().Length == 0) {
                MovingPicturesCore.DataProviderManager.GetArtwork(browser.SelectedMovie);
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
            logger.Debug("playSelectedMovie()");
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
            logger.Debug("playMovie()");
            
            if (movie == null || requestedPart > movie.LocalMedia.Count || requestedPart < 1) 
                return;

            logger.Debug(" movie: {0}, requestedPart: {1}", movie.Title, requestedPart);
            for (int i = 0; i < movie.LocalMedia.Count; i++) {
                logger.Debug("LocalMedia[{0}] = {1}  Duration = {2}", i, movie.LocalMedia[i].FullPath, movie.LocalMedia[i].Duration);
            }


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
                if (!resume && movie.LocalMedia.Count > 1 && (DaemonTools.IsImageFile(firstExtension) || firstExtension.ToLower() == ".ifo")) {
                    GUIDialogFileStacking dlg = (GUIDialogFileStacking)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_FILESTACKING);
                    if (null != dlg) {
                        dlg.SetNumberOfFiles(movie.LocalMedia.Count);
                        dlg.DoModal(GUIWindowManager.ActiveWindow);
                        part = dlg.SelectedFile;
                        if (part < 1) return;
                    }
                }
            }


            DBLocalMedia mediaToPlay = movie.LocalMedia[part - 1];

            // If the media is missing, this loop will ask the user to insert it.
            // This loop can exit in 2 ways:
            // 1. the user clicked "retry" and the media is now available
            // 2. the user clicked "cancel" and we break out of the method
            while (!mediaToPlay.IsAvailable) {

                // the waiting for variables are set so that
                // we can auto play in the OnVolumeInserted event handler.
                waitingForMedia = true;
                waitingForMediaSerial = mediaToPlay.VolumeSerial;


                string bodyString = String.Format(Translation.MediaNotAvailableBody, mediaToPlay.MediaLabel);

                bool retry = ShowCustomYesNo(Translation.MediaNotAvailableHeader, bodyString,
                    Translation.Retry, Translation.Cancel, true);

                waitingForMedia = false;
                waitingForMediaSerial = "";

                // if the user clicked cancel, return.
                if (!retry) return;                
            }


            if (mediaToPlay.IsRemoved) {
                ShowMessage("Error", Translation.MediaIsMissing);
                return;
            }

            currentlyPlayingMovie = movie;
            currentlyPlayingPart = part;

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
                    logger.Debug("jumping to resume point");
                    if (g_Player.IsDVD) 
                        g_Player.Player.SetResumeState(movie.UserSettings[0].ResumeData.Data);
                    else {
                        logger.Debug("ResumeTime = {0}", movie.UserSettings[0].ResumeTime);
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
                    string custom_intro = MovingPicturesCore.Settings.CustomIntroLocation;

                    // Check if the custom intro is specified by user and exists
                    if (custom_intro.Length > 0 && File.Exists(custom_intro)) {
                        logger.Debug("Playing Custom Intro: {0}", custom_intro);

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
            if (movie.UserSettings == null || movie.UserSettings.Count == 0 || movie.UserSettings[0].ResumeTime <= 30)
                return false;

            logger.Debug("PromptUserToResume {0} ResumeTime {1} ResumePart {2}", movie.Title, movie.UserSettings[0].ResumeTime, movie.UserSettings[0].ResumePart);

            // figure out the resume time to display to the user
            int displayTime = movie.UserSettings[0].ResumeTime;
            if (movie.LocalMedia.Count > 1) {
                for (int i = 0; i < movie.UserSettings[0].ResumePart - 1; i++) {
                    if (movie.LocalMedia[i].Duration > 0)
                        displayTime += movie.LocalMedia[i].Duration;
                }
            }

            string sbody = movie.Title + "\n" + Translation.ResumeFrom + " " + Util.Utils.SecondsToHMSString(displayTime);
            bool bResume = ShowCustomYesNo(Translation.ResumeFromLast, sbody, null, null, true);

            if (bResume)
                return true;

            return false;
        }

        // start playback of a regular file
        private void playFile(string media) {
            logger.Debug("playFile " + media);
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
                    } else {
                       // Show a dialog to the user that explains how to configure the alternate playback
                       string ext = (videoFormat == VideoDiscFormat.Bluray) ? ".M2TS" : ".EVO" ;
                       logger.Info("HD Playback: extension '{0}' is missing from the mediaportal configuration.", ext);
                       ShowMessage(Translation.PlaybackFailedHeader, String.Format(Translation.PlaybackFailed, ext));
                       return;
                    }
                }

                logger.Info("HD Playback: Internal, Media={0}", media);
            }

            GUIGraphicsContext.IsFullScreenVideo = true;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
            bool success = g_Player.Play(media, g_Player.MediaType.Video);
            currentlyPlaying = success;
        }

        // start playback of an image file (ISO)
        private void playImage(string media) {
            logger.Debug("playImage " + media);
            string drive;

            // Check if the current image is already mounted
            if (!DaemonTools.IsMounted(media)) {
                // if not try to mount the image
                logger.Info("Mounting image...");
                if (!DaemonTools.Mount(media, out drive)) {
                    ShowMessage(Translation.Error, Translation.FailedMountingImage);
                    logger.Error("Mounting image failed.");
                    return;
                }
            }
            else {
                logger.Info("Image was already mounted.");
                drive = DaemonTools.GetVirtualDrive();
            }

            logger.Info("Image mounted: Drive={0}", drive);

            // This line will list the complete file structure of the image
            // Output will only show when the log is set to DEBUG.
            // Purpose of method is troubleshoot different image structures.
            Utility.LogDirectoryStructure(drive);
            
            // See if we can find a known entry path for a video disc format
            string discPath = Utility.GetVideoDiscPath(drive);

            // If we didn't find any just pass the driveletter
            if (discPath == null)
                discPath = drive;

            // Play the file/path
            playFile(discPath);
        }

        // This method launches an external HD player controlled by Moving Pictures
        // Eventually when Mediaportal has a native solution for HD video disc formats
        // this will be not needed anymore.
        private void LaunchHDPlayer(string videoPath) {
            logger.Info("HD Playback: Launching external player.");

            // First check if the user supplied executable for the external player is valid
            string execPath = MovingPicturesCore.Settings.ExternalPlayerExecutable;
            if (!File.Exists(execPath)) {
                // if it's not show a dialog explaining the error
                ShowMessage("Error", Translation.MissingExternalPlayerExe);
                logger.Info("HD Playback: The external player executable '{0}' is missing.", execPath);
                // do nothing
                return;
            }
            
            // process the argument string and replace the 'filename' variable
            string arguments = MovingPicturesCore.Settings.ExternalPlayerArguements;
            string videoRoot = Utility.GetMovieBaseDirectory(new FileInfo(videoPath).Directory).FullName;
            string filename = Utility.IsDriveRoot(videoRoot) ? videoRoot : videoPath;
            arguments = arguments.Replace("%filename%", filename);

            logger.Debug("External player command: {0} {1}", execPath, arguments);
            
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

                // hide mediaportal and suspend rendering to save resources for the external player
                GUIGraphicsContext.BlankScreen = true;
                GUIGraphicsContext.form.Hide();
                GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.SUSPENDING;                               

                currentlyPlaying = true;                
                logger.Info("HD Playback: External player started.");
            }
            catch (Exception e) {
                logger.ErrorException("HD Playback: Could not start the external player process.", e);
            }
        }

        private void OnHDPlayerExited(object obj, EventArgs e) {
            // show mediaportal and start rendering
            GUIGraphicsContext.BlankScreen = false;
            GUIGraphicsContext.form.Show();
            GUIGraphicsContext.ResetLastActivity();
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GETFOCUS, 0, 0, 0, 0, 0, null);
            GUIWindowManager.SendThreadMessage(msg);
            GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;
            currentlyPlaying = false;
            logger.Info("HD Playback: The external player has exited.");
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

            if (type != g_Player.MediaType.Video || !currentlyPlaying)
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

        private void Utils_OnStartExternal(System.Diagnostics.Process proc, bool waitForExit) {
            if (currentlyPlayingMovie != null) {
                logger.Info("OnStartExternal");
                currentlyPlaying = true;
            }
        }

        private void Utils_OnStopExternal(System.Diagnostics.Process proc, bool waitForExit) {
            if (!currentlyPlaying)
                return;

            logger.Info("OnStopExternal");
            if (currentlyPlayingPart < currentlyPlayingMovie.LocalMedia.Count) {
                string sBody = String.Format(Translation.ContinueToNextPartBody, (currentlyPlayingPart + 1)) + "\n" + currentlyPlayingMovie.Title;
                bool bContinue = ShowCustomYesNo(Translation.ContinueToNextPartHeader, sBody, null, null, true);

                if (bContinue) {
                    logger.Debug("Goto next part");
                    currentlyPlayingPart++;
                    playMovie(currentlyPlayingMovie, currentlyPlayingPart);
                    return;
                }
            }

            currentlyPlaying = false;
            currentlyPlayingMovie = null;
            currentlyPlayingPart = 0;
            
        }

        private void OnPlayBackStoppedOrChanged(g_Player.MediaType type, int timeMovieStopped, string filename) {
            if (type != g_Player.MediaType.Video || !currentlyPlaying)
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

            int requiredWatchedPercent = MovingPicturesCore.Settings.MinimumWatchPercentage;
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

        private void setWorkingAnimationStatus(bool visible) {
            try {
                if (workingAnimation != null) {
                    if (visible)
                        workingAnimation.AllocResources();
                    else
                        workingAnimation.FreeResources();
                    workingAnimation.Visible = visible;
                }
            }
            catch (Exception) {
            }
        }

        private void Importer_Progress(int percentDone, int taskCount, int taskTotal, string taskDescription) {
            setWorkingAnimationStatus(percentDone < 100);
        }

        private void updateMovieWatchedCounter(DBMovieInfo movie) {
            if (movie == null)
                return;

            // get the user settings for the default profile (for now)
            DBUserMovieSettings userSetting = movie.UserSettings[0];
            userSetting.Watched++; // increment watch counter
            userSetting.Commit();
            DBWatchedHistory.AddWatchedHistory(movie, userSetting.User);

            browser.ReapplyFilters();

            // if we are on the details page for the movie just marked as watched and we are filtering
            // go back to facade since this movie is no longer selectable. later need to tweak to allow 
            // movies filtered out to be displayed in details anyway.
            if (movie == browser.SelectedMovie && browser.CurrentView == BrowserViewMode.DETAILS && watchedFilter.Active)
                browser.CurrentView = browser.PreviousView;
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
                    browser.CurrentView = BrowserViewMode.DETAILS;
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

            logger.Debug("OnVolumeInserted: Volume={0}, Serial={1}", volume, serial);

            if (GUIWindowManager.ActiveWindow == GetID) {

                // Clear recent disc information
                logger.Debug("Resetting Recent Disc Information.");
                recentInsertedDiskSerials.Clear();


                if (waitingForMedia) {
                    if (waitingForMediaSerial == serial) {
                        // Correct volume inserted.  Starting playback.
                        waitingForMedia = false;
                        waitingForMediaSerial = "";
                        playSelectedMovie();

                        // Why is the following needed?  For some reason, playSelectedMovie() isn't 
                        // playing in full screen.  I have to manually switch to fullscreen here. - Z6
                        GUIGraphicsContext.IsFullScreenVideo = true;
                        return;
                    }
                }

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
                if (currentlyPlayingMovie.LocalMedia[currentlyPlayingPart].DriveLetter == volume)
                    g_Player.Stop();

            logger.Debug("OnVolumeRemoved" + volume);
        }

        #endregion

        #region Skin and Property Settings

        private void UpdateMovieDetails() {
            if (browser.SelectedMovie == null)
                return;

            if (browser.SelectedMovie == null)
                return;

            PublishDetails(browser.SelectedMovie, "SelectedMovie");

            if (selectedMovieWatchedIndicator != null)
                if (browser.SelectedMovie.UserSettings[0].Watched > 0)
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
            if (obj == null)
                return;

            int maxStringListElements = MovingPicturesCore.Settings.MaxElementsToDisplay;

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

        #endregion

    }
}
