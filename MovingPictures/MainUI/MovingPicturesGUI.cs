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
using System.Windows.Media.Animation;


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

        private bool loaded = false;

        private ImageSwapper backdrop;
        private AsyncImageResource cover = null;

        private DiskInsertedAction diskInsertedAction;
        Dictionary<string, string> recentInsertedDiskSerials;

        private bool dialogActive = false;

        private DBMovieInfo awaitingUserRatingMovie;
        private MoviePlayer moviePlayer;
        double _lastCommandTime = 0;
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

        [SkinControl(14)]
        protected GUIButtonControl sortMenuButton = null;

        #endregion

        public MovingPicturesGUI() {
            MovingPicturesCore.Importer.Progress += new MovieImporter.ImportProgressHandler(Importer_Progress);

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

            // instantiate player
            moviePlayer = new MoviePlayer(this);
            moviePlayer.MovieEnded += new MoviePlayerEvent(onMovieEnded);
            moviePlayer.MovieStopped += new MoviePlayerEvent(onMovieStopped);
        }

        ~MovingPicturesGUI() {
        }

        private void UpdateArtwork() {
            if (browser.SelectedMovie == null)
                return;

            // update resources with new files
            cover.Filename = browser.SelectedMovie.CoverFullPath;
            backdrop.Filename = browser.SelectedMovie.BackdropFullPath;

            if (browser.CurrentView != BrowserViewMode.DETAILS)
                browser.facade.SelectedListItem.RefreshCoverArt();
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
            if (sortMenuButton != null) sortMenuButton.Focus = false;
        }

        public void ShowMessage(string heading, string lines) {
            string line1 = null, line2 = null, line3 = null, line4 = null;
            string[] linesArray = lines.Split(new string[] { "\\n" }, StringSplitOptions.None);

            if (linesArray.Length >= 1) line1 = linesArray[0];
            if (linesArray.Length >= 2) line2 = linesArray[1];
            if (linesArray.Length >= 3) line3 = linesArray[2];
            if (linesArray.Length >= 4) line4 = linesArray[3];

            ShowMessage(heading, line1, line2, line3, line4);
        }

        public void ShowMessage(string heading, string line1, string line2, string line3, string line4) {
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
        public bool ShowCustomYesNo(string heading, string lines, string yesLabel, string noLabel, bool defaultYes) {
            dialogActive = true;
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
                dialogActive = false;
                return dialog.IsConfirmed;
            }
            finally {
                dialogActive = false;

                // set the standard yes/no dialog back to it's original state (yes/no buttons)
                if (dialog != null) {
                    dialog.ClearAll();
                }
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
            // TODO: Why is this like this?
            if (browser.CurrentView != BrowserViewMode.DETAILS) {
                browser.JumpToBeginningOfList();
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

            if (awaitingUserRatingMovie != null) {
                GetUserRating(awaitingUserRatingMovie);
                awaitingUserRatingMovie = null;
            }

            SetProperty("#MovingPictures.Settings.HomeScreenName", MovingPicturesCore.Settings.HomeScreenName);
        }

        protected override void OnPageDestroy(int new_windowId) {
            // Enable autoplay again when we are leaving the plugin
            // But only when we are not playing something
            if (!moviePlayer.IsPlaying)                
                enableNativeAutoplay();

            base.OnPageDestroy(new_windowId);
        }

        // Disable MediaPortal's AutoPlay

        /// <summary>
        /// Disable MediaPortal AutoPlay
        /// </summary>
        private void disableNativeAutoplay() {
            logger.Info("Disabling native autoplay.");
            AutoPlay.StopListening();
        }

        /// <summary>
        /// Enable MediaPortal AutoPlay
        /// </summary>
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
                        case MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM:
                            if (control == facade) {
                                if (clickToDetails)
                                    browser.CurrentView = BrowserViewMode.DETAILS;
                                else
                                    playSelectedMovie();
                            }
                            break;
                        case MediaPortal.GUI.Library.Action.ActionType.ACTION_SHOW_INFO:
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

                // a click on the sort menu button
                case 14:
                    showSortContext();
                    break;
            }

            base.OnClicked(controlId, control, actionType);
        }

        public override void OnAction(MediaPortal.GUI.Library.Action action) {
            switch (action.wID) {
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_PARENT_DIR:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_HOME:
                    GUIWindowManager.ShowPreviousWindow();
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_PREVIOUS_MENU:
                    if (browser.CurrentView == BrowserViewMode.DETAILS)
                        // return to the facade screen
                        browser.CurrentView = browser.PreviousView;
                    else if (remoteFilter.Active)
                        // if a remote filter is active remove it
                        remoteFilter.Clear();
                    else {
                        // show previous screen (exit the plug-in
                        GUIWindowManager.ShowPreviousWindow();
                    }
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_PLAY:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_MUSIC_PLAY:
                    // don't be confused, this in some cases is the generic PLAY action
                    playSelectedMovie();
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_KEY_PRESSED:
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
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_UP:
                    int _loopDelay = 100; // wait at the last item this amount of msec until loop to the last item

                    // if the user is holding the up key, don't go past index 1
                    // we need to do this ourselves because the GUIListControl is hard coded to stop at index 0
                    // index 0 is a group header in our case, so we want to stop at 1 instead
                    if (!(browser.CurrentView == BrowserViewMode.LIST
                        && MovingPicturesCore.Settings.AllowGrouping
                        && browser.Facade.SelectedListItemIndex == 1
                        && AnimationTimer.TickCount - _lastCommandTime < _loopDelay
                        )) {
                        base.OnAction(action);
                    }
                    _lastCommandTime = AnimationTimer.TickCount;
                    break;

                case MediaPortal.GUI.Library.Action.ActionType.ACTION_PAGE_UP:
                    lock (browser) {
                        base.OnAction(action);

                        if ((browser.CurrentView == BrowserViewMode.LIST
                            && MovingPicturesCore.Settings.AllowGrouping
                            && browser.Facade.SelectedListItemIndex == 0
                            )) {
                            browser.Facade.SelectedListItemIndex = 1;
                        }
                    }
                    break;
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

            GUIListItem movieOptionsItem = new GUIListItem(Translation.MovieOptions + " ...");
            movieOptionsItem.ItemId = 4;
            dialog.Add(movieOptionsItem);

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
                case 4:
                    showDetailsContext();
                    break;
            }
        }

        private void showSortContext() {

            IDialogbox dialog = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dialog.Reset();
            dialog.SetHeading("Moving Pictures - " + Translation.SortBy);

            foreach (int value in Enum.GetValues(typeof(SortingFields))) {
                SortingFields field = (SortingFields)Enum.Parse(typeof(SortingFields), value.ToString());
                string menuCaption = Sort.GetFriendlySortName(field);
                GUIListItem listItem = new GUIListItem(menuCaption);
                listItem.ItemId = value;

                if (field == browser.CurrentSortField) {
                    if (browser.CurrentSortDirection == SortingDirections.Ascending)
                        listItem.Label2 = Translation.DownAbbreviation;
                    else
                        listItem.Label2 = Translation.UpAbbreviation;
                }
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
            browser.JumpToBeginningOfList();
            SetProperty("#MovingPictures.Sort.Field", Sort.GetFriendlySortName(browser.CurrentSortField));
            SetProperty("#MovingPictures.Sort.Direction", browser.CurrentSortDirection.ToString());
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
            GUIListItem rateItem = new GUIListItem();

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

            if (browser.SelectedMovie.ActiveUserSettings.WatchedCount > 0) {
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

            rateItem = new GUIListItem(Translation.Rate);
            rateItem.ItemId = currID;
            dialog.Add(rateItem);
            currID++;

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
                browser.SelectedMovie.ActiveUserSettings.WatchedCount = 0;
                browser.SelectedMovie.ActiveUserSettings.Commit();
                UpdateMovieDetails();
                browser.UpdateListColors(browser.SelectedMovie);
                browser.ReapplyFilters();
            }
            else if (dialog.SelectedId == watchedItem.ItemId) {
                browser.SelectedMovie.ActiveUserSettings.WatchedCount = 1;
                browser.SelectedMovie.ActiveUserSettings.Commit();
                UpdateMovieDetails();
                browser.UpdateListColors(browser.SelectedMovie);
                browser.ReapplyFilters();
            }
            else if (dialog.SelectedId == deleteItem.ItemId) {
                deleteMovie();
            }
            else if (dialog.SelectedId == rateItem.ItemId) {
                if (GetUserRating(browser.SelectedMovie)) {
                    UpdateMovieDetails();
                }
            }
        }

        private bool GetUserRating(DBMovieInfo movie) {
            GUIGeneralRating ratingDlg = (GUIGeneralRating)GUIWindowManager.GetWindow(GUIGeneralRating.ID);
            ratingDlg.Reset();
			ratingDlg.SetHeading(Translation.RateHeading);
            ratingDlg.SetLine(1, String.Format(Translation.SelectYourRating, browser.SelectedMovie.Title));
            DBUserMovieSettings userMovieSettings = movie.ActiveUserSettings;			
            ratingDlg.Rating = userMovieSettings.UserRating.GetValueOrDefault(3);
			ratingDlg.DisplayStars = GUIGeneralRating.StarDisplay.FIVE_STARS;
			SetRatingDescriptions(ratingDlg);

            try {
                ratingDlg.DoModal(GetID);
            }
            catch (ArgumentNullException) {
                ShowMessage(Translation.Error, Translation.SkinDoesNotSupportRatingDialog);
                return false;
            }
            if (ratingDlg.IsSubmitted) {
                userMovieSettings.UserRating = ratingDlg.Rating;
                userMovieSettings.Commit();
            }
            return ratingDlg.IsSubmitted;
        }

		private void SetRatingDescriptions(GUIGeneralRating ratingDlg) {
			ratingDlg.FiveStarRateOneDesc = Translation.RateFiveStarOne;
			ratingDlg.FiveStarRateTwoDesc = Translation.RateFiveStarTwo;
			ratingDlg.FiveStarRateThreeDesc = Translation.RateFiveStarThree;
			ratingDlg.FiveStarRateFourDesc = Translation.RateFiveStarFour;
			ratingDlg.FiveStarRateFiveDesc = Translation.RateFiveStarFive;
			return;
		}

        private void deleteMovie() {
            DBLocalMedia firstFile = browser.SelectedMovie.LocalMedia[0];

            // if the file is available and read only, or known to be stored on optical media, prompt to ignore.
            if ((firstFile.IsAvailable && firstFile.File.IsReadOnly) || firstFile.ImportPath.IsOpticalDrive) {
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
                foreach (DBLocalMedia lm in browser.SelectedMovie.LocalMedia) {
                    lm.UpdateMediaInfo();
                    lm.Commit();
                }
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

            // Play movie
            moviePlayer.Play(browser.SelectedMovie);
         }

        private void onMovieStopped(DBMovieInfo movie) {
            // If we or stopping in another windows enable native auto-play again
            // This will most of the time be the fullscreen playback window, 
            // if we would re-enter the plugin, autoplay will be disabled again.
            if (GetID != GUIWindowManager.ActiveWindow)
                enableNativeAutoplay();
        }           

        private void onMovieEnded(DBMovieInfo movie) {
            
            // Rating
            if (MovingPicturesCore.Settings.AutoPromptForRating)
                awaitingUserRatingMovie = movie;
            
            // Reapply Filters
            browser.UpdateListColors(movie);
            browser.ReapplyFilters();

            // if we are on the details page for the movie just marked as watched and we are filtering
            // go back to facade since this movie is no longer selectable. later need to tweak to allow 
            // movies filtered out to be displayed in details anyway.
            if (movie == browser.SelectedMovie && browser.CurrentView == BrowserViewMode.DETAILS && watchedFilter.Active)
                browser.CurrentView = browser.PreviousView;

            onMovieStopped(movie);
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
                    moviePlayer.Play(movie, part);
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
            // and there's no active dialog waiting for user input
            if (GUIWindowManager.ActiveWindow != GetID && !moviePlayer.IsPlaying && !dialogActive)
                return;

            logger.Debug("OnVolumeInserted: Volume={0}, Serial={1}", volume, serial);

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
                    
                    // For DVDs we have to make sure this is the correct
                    // media file, a simple availability check will point out 
                    // if we should requery the database using the discid
                    if (!localMedia.IsAvailable)
                        localMedia = DBLocalMedia.GetDVD(moviePath, Utility.GetDiscIdString(Directory.GetParent(moviePath).FullName));   
                    
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
            // if we are playing something from this volume stop it
            if (moviePlayer.IsPlaying)
                if (moviePlayer.CurrentMedia.DriveLetter == volume)
                    moviePlayer.Stop();

            logger.Debug("OnVolumeRemoved" + volume);
        }

        #endregion

        #region Skin and Property Settings

        private void UpdateMovieDetails() {
            if (browser.SelectedMovie == null)
                return;

            PublishDetails(browser.SelectedMovie, "SelectedMovie");
            PublishDetails(browser.SelectedMovie.ActiveUserSettings, "UserMovieSettings");
            PublishDetails(browser.SelectedMovie.LocalMedia[0], "LocalMedia", true);

            int selectedIndex = browser.Facade.SelectedListItemIndex;
            for (int i = 0; i < browser.Facade.SelectedListItemIndex; i++) {
                if (browser.facade.ListView.ListItems[i].TVTag == null)
                    selectedIndex--;
            }
            selectedIndex++; // make this one-based
            SetProperty("#MovingPictures.SelectedIndex", selectedIndex.ToString());

            SetProperty("#MovingPictures.LocalMedia.Subtitles", browser.SelectedMovie.LocalMedia[0].HasSubtitles ? "subtitles" : "nosubtitles", true);

            if (selectedMovieWatchedIndicator != null)
                if (browser.SelectedMovie.ActiveUserSettings.WatchedCount > 0)
                    selectedMovieWatchedIndicator.Visible = true;
                else
                    selectedMovieWatchedIndicator.Visible = false;

            UpdateArtwork();
        }

        public void SetProperty(string property, string value) {
            SetProperty(property, value, false);
        }

        public void SetProperty(string property, string value, bool forceLogging) {
            if (property == null)
                return;

            try {
                lock (loggedProperties) {
                    if (!loggedProperties.ContainsKey(property) || forceLogging) {
                        logger.Debug(property + " = \"" + value + "\"");
                        loggedProperties[property] = true;
                    }
                }
            }
            catch (Exception e) {
                if (e is ThreadAbortException)
                    throw e;

                logger.Warn("Internal .NET error from dictionary class!");
            }

            // If the value is empty always add a space
            // otherwise the property will keep 
            // displaying it's previous value
            if (String.IsNullOrEmpty(value))
                value = " ";

            GUIPropertyManager.SetProperty(property, value);
        }

        // this does standard object publishing for any database object.
        private void PublishDetails(DatabaseTable obj, string prefix) {
            PublishDetails(obj, prefix, false);
        }

        private void PublishDetails(DatabaseTable obj, string prefix, bool forceLogging) {
            if (obj == null)
                return;

            int maxStringListElements = MovingPicturesCore.Settings.MaxElementsToDisplay;

            Type tableType = obj.GetType();
            foreach (DBField currField in DBField.GetFieldList(tableType)) {
                string propertyStr;
                string valueStr;

                // for string lists, lets do some nice formating
                object value = currField.GetValue(obj);
                if (value == null) {
                    propertyStr = "#MovingPictures." + prefix + "." + currField.FieldName;
                    SetProperty(propertyStr, "", forceLogging);
                }

                else if (value.GetType() == typeof(StringList)) {

                    // make sure we dont go overboard with listing elements :P
                    StringList valueStrList = (StringList)value;
                    int max = maxStringListElements;
                    if (max > valueStrList.Count)
                        max = valueStrList.Count;

                    // add the coma seperated string
                    propertyStr = "#MovingPictures." + prefix + "." + currField.FieldName;
                    valueStr = valueStrList.ToPrettyString(max);
                    SetProperty(propertyStr, valueStr, forceLogging);

                    // add each value individually
                    for (int i = 0; i < max; i++) {
                        // note, the "extra" in the middle is needed due to a bug in skin parser
                        propertyStr = "#MovingPictures." + prefix + ".extra." + currField.FieldName + "." + (i + 1);
                        valueStr = valueStrList[i];
                        SetProperty(propertyStr, valueStr, forceLogging);
                    }

                    // for floats we need to make sure we use english style printing or imagelist controls
                    // will break. 
                }
                else if (value.GetType() == typeof(float)) {
                    propertyStr = "#MovingPictures." + prefix + "." + currField.FieldName;
                    valueStr = ((float)currField.GetValue(obj)).ToString(CultureInfo.CreateSpecificCulture("en-US"));
                    SetProperty(propertyStr, valueStr, forceLogging);

                    // vanilla publication
                }
                else {
                    propertyStr = "#MovingPictures." + prefix + "." + currField.FieldName;
                    valueStr = currField.GetValue(obj).ToString().Trim();
                    SetProperty(propertyStr, valueStr, forceLogging);
                }
            }

            PublishBonusDetails(obj, prefix, forceLogging);
        }

        // publishing for special fields not specifically in the database
        private void PublishBonusDetails(DatabaseTable obj, string prefix) {
            PublishBonusDetails(obj, prefix, false);
        }

        private void PublishBonusDetails(DatabaseTable obj, string prefix, bool forceLogging) {
            string propertyStr;
            string valueStr;

            if (obj is DBMovieInfo) {
                DBMovieInfo movie = (DBMovieInfo)obj;
                int minValue;
                int hourValue;

                // hour component of runtime
                propertyStr = "#MovingPictures." + prefix + ".extra.runtime.hour";
                hourValue = (movie.Runtime / 60);
                SetProperty(propertyStr, hourValue.ToString(), forceLogging);

                // minute component of runtime
                propertyStr = "#MovingPictures." + prefix + ".extra.runtime.minute";
                minValue = (movie.Runtime % 60);
                SetProperty(propertyStr, minValue.ToString(), forceLogging);

                // give short runtime string 0:00
                propertyStr = "#MovingPictures." + prefix + ".extra.runtime.short";
                valueStr = string.Format("{0}:{1:00}", hourValue, minValue);
                SetProperty(propertyStr, valueStr, forceLogging);

                // give pretty runtime string
                propertyStr = "#MovingPictures." + prefix + ".extra.runtime.en.pretty";
                valueStr = string.Empty;
                if (hourValue > 0)
                    valueStr = string.Format("{0} hour{1}", hourValue, hourValue != 1 ? "s" : string.Empty);
                if (minValue > 0)
                    valueStr = valueStr + string.Format(", {0} minute{1}", minValue, minValue != 1 ? "s" : string.Empty);
                SetProperty(propertyStr, valueStr, forceLogging);
            }

            // convert mediainfo duration from milliseconds to hour & minute values
            if (obj is DBLocalMedia) {
                DBLocalMedia movie = (DBLocalMedia)obj;
                int minValue;
                int hourValue;

                // hour component of duration
                propertyStr = "#MovingPictures." + prefix + ".extra.runtime.hour";
                hourValue = (movie.Duration / 3600000);
                SetProperty(propertyStr, hourValue.ToString(), forceLogging);

                // minute component of duration
                propertyStr = "#MovingPictures." + prefix + ".extra.runtime.minute";
                minValue = ((movie.Duration / 60000) % 60);
                SetProperty(propertyStr, minValue.ToString(), forceLogging);

                // give short runtime string 0:00
                propertyStr = "#MovingPictures." + prefix + ".extra.runtime.short";
                valueStr = string.Format("{0}:{1:00}", hourValue, minValue);
                SetProperty(propertyStr, valueStr, forceLogging);

                // give pretty runtime string
                propertyStr = "#MovingPictures." + prefix + ".extra.runtime.en.pretty";
                valueStr = string.Empty;
                if (hourValue > 0)
                    valueStr = string.Format("{0} hour{1}", hourValue, hourValue != 1 ? "s" : string.Empty);
                if (minValue > 0)
                    valueStr = valueStr + string.Format(", {0} minute{1}", minValue, minValue != 1 ? "s" : string.Empty);
                SetProperty(propertyStr, valueStr, forceLogging);
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