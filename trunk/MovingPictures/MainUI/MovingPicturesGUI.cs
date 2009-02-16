﻿using System;
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
        private bool ShowCustomYesNo(string heading, string line1, string line2, string line3, string line4, string yesLabel, string noLabel, bool defaultYes) {
            GUIDialogYesNo dialog = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            try {
                dialog.Reset();
                dialog.SetHeading(heading);
                if (!String.IsNullOrEmpty(line1)) dialog.SetLine(1, line1);
                if (!String.IsNullOrEmpty(line2)) dialog.SetLine(2, line2);
                if (!String.IsNullOrEmpty(line3)) dialog.SetLine(3, line3);
                if (!String.IsNullOrEmpty(line4)) dialog.SetLine(4, line4);
                dialog.SetDefaultToYes(defaultYes);


                foreach (System.Windows.UIElement item in dialog.Children) {
                    if (item is GUIButtonControl) {
                        GUIButtonControl btn = (GUIButtonControl)item;
                        if (btn.GetID == 11) // Yes button
                            btn.Label = yesLabel;
                        else if (btn.GetID == 10) // No button
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
            else
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
            if ((bool)MovingPicturesCore.SettingsManager["importer_gui_enabled"].Value)
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
                browser = new MovieBrowser(skinSettings);

                // add available filters to browser
                remoteFilter = new RemoteNumpadFilter();
                browser.ActiveFilters.Add(remoteFilter);

                watchedFilter = new WatchedFlagFilter();
                browser.ActiveFilters.Add(watchedFilter);

                // if option is set, turn on the watched movies filter by default
                bool startWithWatchedFilterOn = (bool)MovingPicturesCore.SettingsManager["start_watched_filter_on"].Value;
                if (startWithWatchedFilterOn)
                    watchedFilter.Active = true;

                // give the browser a delegate to the method to clear focus from all existing controls
                browser.ClearFocusAction = new MovieBrowser.ClearFocusDelegate(ClearFocus);

                // setup browser events
                browser.SelectionChanged += new MovieBrowser.SelectionChangedDelegate(OnBrowserSelectionChanged);
                browser.ContentsChanged += new MovieBrowser.ContentsChangedDelegate(OnBrowserContentsChanged);
                browser.ViewChanged +=new MovieBrowser.ViewChangedDelegate(OnBrowserViewChanged);

                SetProperty("#MovingPictures.Sort.Field", GUIListItemMovieComparer.GetFriendlySortName(browser.CurrentSortField));
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
                    bool clickToDetails = (bool)MovingPicturesCore.SettingsManager["click_to_details"].Value;

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
                    bool remoteFilterEnabled = (bool)MovingPicturesCore.SettingsManager["enable_rc_filter"].Value;
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
            dialog.SetHeading("Moving Pictures");

            GUIListItem watchItem = new GUIListItem(watchedFilter.Active ? "Show All Movies" : "Show Only Unwatched Movies");
            watchItem.ItemId = 1;
            dialog.Add(watchItem);

            GUIListItem sortItem = new GUIListItem("Sort By ...");
            sortItem.ItemId = 2;
            dialog.Add(sortItem);

            GUIListItem viewItem = new GUIListItem("Change View ...");
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
            dialog.SetHeading("Moving Pictures - Sort By");

            foreach (int value in Enum.GetValues(typeof(GUIListItemMovieComparer.SortingFields))) {
                string menuCaption = GUIListItemMovieComparer.GetFriendlySortName(
                    (GUIListItemMovieComparer.SortingFields)Enum.Parse(typeof(GUIListItemMovieComparer.SortingFields), value.ToString()));
                GUIListItem listItem = new GUIListItem(menuCaption);
                listItem.ItemId = value;
                dialog.Add(listItem);
            }

            dialog.DoModal(GUIWindowManager.ActiveWindow);

            GUIListItemMovieComparer.SortingFields newSortField = GUIListItemMovieComparer.SortingFields.Title;
            GUIListItemMovieComparer.SortingDirections defaultSortDirection = GUIListItemMovieComparer.SortingDirections.Ascending;
            
            if (dialog.SelectedId <= 0) return;  // user canceled out of menu
            
            newSortField = (GUIListItemMovieComparer.SortingFields)Enum.Parse(typeof(GUIListItemMovieComparer.SortingFields), dialog.SelectedId.ToString());
            if (newSortField == GUIListItemMovieComparer.SortingFields.DateAdded)
                defaultSortDirection = GUIListItemMovieComparer.SortingDirections.Descending;

            if (browser.CurrentSortField == newSortField) {
                // toggle sort direction
                if (browser.CurrentSortDirection == GUIListItemMovieComparer.SortingDirections.Ascending)
                    browser.CurrentSortDirection = GUIListItemMovieComparer.SortingDirections.Descending;
                else
                    browser.CurrentSortDirection = GUIListItemMovieComparer.SortingDirections.Ascending;
            }
            else {
                browser.CurrentSortField = newSortField;
                browser.CurrentSortDirection = defaultSortDirection;
            }

            browser.ReloadFacade();
            SetProperty("#MovingPictures.Sort.Field", GUIListItemMovieComparer.GetFriendlySortName(browser.CurrentSortField));
            SetProperty("#MovingPictures.Sort.Direction", browser.CurrentSortDirection.ToString());
            browser.Facade.SelectedListItemIndex = 0;
        }

        private void showChangeViewContext() {
            IDialogbox dialog = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dialog.Reset();
            dialog.SetHeading("Moving Pictures - Change View");

            int currID = 1;
            GUIListItem listItem = new GUIListItem("List View");
            if (skinSettings.ListViewAvailable) {
                listItem.ItemId = currID++;
                dialog.Add(listItem);
            }

            GUIListItem thumbItem = new GUIListItem("Thumbnail View");
            if (skinSettings.IconViewAvailable) {
                thumbItem.ItemId = currID++;
                dialog.Add(thumbItem);
            }

            GUIListItem largeThumbItem = new GUIListItem("Large Thumbnail View");
            if (skinSettings.LargeIconViewAvailable) {
                largeThumbItem.ItemId = currID++;
                dialog.Add(largeThumbItem);
            }

            GUIListItem filmItem = new GUIListItem("Filmstrip View");
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

            bool deleteEnabled = (bool)MovingPicturesCore.SettingsManager["enable_delete_movie"].Value;

            if (deleteEnabled) {
                deleteItem = new GUIListItem("Delete Movie");
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
            }
            else if (dialog.SelectedId == watchedItem.ItemId) {
                browser.SelectedMovie.UserSettings[0].Watched = 1;

                browser.SelectedMovie.UserSettings[0].Commit();
                browser.ReapplyFilters();
                browser.ReloadFacade();
            }
            else if (dialog.SelectedId == deleteItem.ItemId) {
                deleteMovie();
            }
        }

        private void deleteMovie() {
            DBLocalMedia firstFile = browser.SelectedMovie.LocalMedia[0];

            // if the file is available and read only, or known to be stored on optical media, prompt to ignore.
            if ((firstFile.IsAvailable && firstFile.File.IsReadOnly) || DeviceManager.GetVolumeInfo(firstFile.DriveLetter).DriveInfo.DriveType == DriveType.CDRom) {
                GUIDialogYesNo ignoreDialog = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
                ignoreDialog.Reset();
                ignoreDialog.SetHeading("Moving Pictures");
                ignoreDialog.SetLine(1, "Cannot delete a read-only movie.");
                ignoreDialog.SetLine(2, "Would you like Moving Pictures to ignore this movie?");
                ignoreDialog.SetDefaultToYes(false);
                ignoreDialog.DoModal(GUIWindowManager.ActiveWindow);

                if (ignoreDialog.IsConfirmed) {
                    browser.SelectedMovie.DeleteAndIgnore();
                    if (browser.CurrentView == BrowserViewMode.DETAILS)
                        // return to the facade screen
                        browser.CurrentView = browser.PreviousView;
                }
                return;
            }


            // if the file is offline display an error dialog
            if (!firstFile.IsAvailable) {
                ShowMessage("Moving Pictures", "Not able to delete " + browser.SelectedMovie.Title, " because the file is offline", null, null);
                return;
            }

            // if the file is available and not read only, confirm delete.
            GUIDialogYesNo deleteDialog = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            deleteDialog.Reset();
            deleteDialog.SetHeading("Moving Pictures");
            deleteDialog.SetLine(1, "Do you want to permanently delete");
            deleteDialog.SetLine(2, browser.SelectedMovie.Title);
            deleteDialog.SetLine(3, "from your hard drive?");
            deleteDialog.SetDefaultToYes(false);

            deleteDialog.DoModal(GUIWindowManager.ActiveWindow);

            if (deleteDialog.IsConfirmed) {
                bool deleteSuccesful = browser.SelectedMovie.DeleteFiles();

                if (deleteSuccesful) {
                    if (browser.CurrentView == BrowserViewMode.DETAILS)
                        // return to the facade screen
                        browser.CurrentView = browser.PreviousView;
                }
                else {
                    ShowMessage("Moving Pictures", "Delete failed", null, null, null);
                }
            }
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
            logger.Debug("1. part = {0}", part);
            bool resume = false;

            // if this is a request to start the movie from the begining, check if we should resume
            // or prompt the user for disk selection
            if (requestedPart == 1) {
                // check if we should be resuming, and if not, clear resume data
                resume = PromptUserToResume(movie);
                logger.Debug("resume = {0}", resume);
                if (resume)
                    part = movie.UserSettings[0].ResumePart;
                else
                    clearMovieResumeState(movie);

                logger.Debug("2. part = {0}", part);
                // if we have a multi-part movie composed of disk images and we are not resuming 
                // ask which part the user wants to play
                string firstExtension = movie.LocalMedia[0].File.Extension;
                if (!resume && movie.LocalMedia.Count > 1 && (DaemonTools.IsImageFile(firstExtension) || firstExtension.ToLower() == ".ifo")) {
                    logger.Debug("Displaying filestacking dialog");
                    GUIDialogFileStacking dlg = (GUIDialogFileStacking)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_FILESTACKING);
                    if (null != dlg) {
                        dlg.SetNumberOfFiles(movie.LocalMedia.Count);
                        dlg.DoModal(GUIWindowManager.ActiveWindow);
                        part = dlg.SelectedFile;
                        if (part < 1) return;
                        logger.Debug("3. part = {0}", part);
                    }
                }
            }


            DBLocalMedia mediaToPlay = movie.LocalMedia[part - 1];
            logger.Debug("4. part = {0} mediaToPlay= {1}", part, mediaToPlay.FullPath);

            // If the media is missing, this loop will ask the user to insert it.
            // This loop can exit in 2 ways:
            // 1. the user clicked "retry" and the media is now available
            // 2. the user clicked "cancel" and we break out of the method
            while (!mediaToPlay.IsAvailable) {

                // the waiting for variables are set so that
                // we can auto play in the OnVolumeInserted event handler.
                waitingForMedia = true;
                waitingForMediaSerial = mediaToPlay.VolumeSerial;

                bool retry = ShowCustomYesNo("Media Not Available",
                                            "The media for the movie you have selected is not",
                                            "currently available. Please insert or connect media",
                                            "labeled: " + mediaToPlay.MediaLabel,
                                            null, "Retry", GUILocalizeStrings.Get(222), true);

                waitingForMedia = false;
                waitingForMediaSerial = "";

                if (retry) {
                    // user clicked Retry,
                    // do nothing and let the while loop
                    // test it's condition again
                }
                else {
                    // user clicked Cancel, break out of this method.
                    return;
                }
            }


            if (mediaToPlay.IsRemoved) {
                ShowMessage("Error",
                            "The media for the Movie you have selected is missing!",
                            "Very sorry but something has gone wrong...", null, null);
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

            GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (null == dlgYesNo) return false;
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); 
            dlgYesNo.SetLine(1, movie.Title);
            dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936) + " " + MediaPortal.Util.Utils.SecondsToHMSString(displayTime));
            dlgYesNo.SetDefaultToYes(true);
            dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
            logger.Debug("PromptUserToResume: User chose " + dlgYesNo.IsConfirmed.ToString());
            if (dlgYesNo.IsConfirmed)
                return true;

            return false;
        }

        // start playback of a regular file
        private void playFile(string media) {
            logger.Debug("playFile " + media);
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
                ShowMessage("Error", "Either the image file does not contain", 
                                     "a valid video disc format, or your Daemon", 
                                     "Tools MediaPortal configuration is incorrect.", null);
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
            logger.Info("OnStartExternal");
            currentlyPlaying = true;
        }

        private void Utils_OnStopExternal(System.Diagnostics.Process proc, bool waitForExit) {
            logger.Info("OnStopExternal");
            if (currentlyPlayingPart < currentlyPlayingMovie.LocalMedia.Count) {
                GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
                dlgYesNo.SetHeading("Continue to next part?");
                dlgYesNo.SetLine(1, String.Format("Do you wish to continue with part {0}?", (currentlyPlayingPart + 1)));
                dlgYesNo.SetLine(2, currentlyPlayingMovie.Title);
                dlgYesNo.SetDefaultToYes(true);
                dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
                if (dlgYesNo.IsConfirmed) {
                    logger.Debug("Goto next part");
                    currentlyPlayingPart++;
                    playMovie(currentlyPlayingMovie, currentlyPlayingPart);
                }
                else {
                    currentlyPlaying = false;
                    currentlyPlayingMovie = null;
                    currentlyPlayingPart = 0;
                }
                return;
            }
            
        }

        private void OnPlayBackStoppedOrChanged(g_Player.MediaType type, int timeMovieStopped, string filename) {
            logger.Debug("OnPlayBackStoppedOrChanged");
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

            logger.Debug("OnVolumeInserted  volume: {0}; serial: {1}", volume, serial);

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

        #endregion

    }
}