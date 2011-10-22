using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Linq;
using System.Xml;
using Cornerstone.Database;
using Cornerstone.Database.CustomTypes;
using Cornerstone.Database.Tables;
using Cornerstone.MP;
using Cornerstone.Collections;
using Cornerstone.Extensions;
using Cornerstone.Extensions.Collections;
using Cornerstone.MP.Extensions;
using Cornerstone.GUI.Dialogs;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Plugins.MovingPictures.DataProviders;
using MediaPortal.Plugins.MovingPictures.MainUI.Filters;
using MediaPortal.Plugins.MovingPictures.Extensions;
using MediaPortal.Profile;
using MediaPortal.Util;
using MediaPortal.Ripper;
using NLog;
using System.IO;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Reflection;


namespace MediaPortal.Plugins.MovingPictures.MainUI {
    public class MovingPicturesGUI : GUIWindow {
        public enum DiskInsertedAction { PLAY, DETAILS, NOTHING }

        #region Private Variables

        private static Logger logger = LogManager.GetCurrentClassLogger();

        MovieBrowser browser;
        RemoteNumpadFilter remoteFilter;
        DBFilter<DBMovieInfo> parentalControlsFilter;

        MovingPicturesSkinSettings skinSettings;

        Dictionary<string, bool> loggedProperties;
        private readonly object propertySync = new object();
        CachedDictionary<DBNode<DBMovieInfo>, DBMovieInfo> activeMovieLookup = new CachedDictionary<DBNode<DBMovieInfo>, DBMovieInfo>();
        private readonly object backdropSync = new object();


        private bool loaded = false;

        GUIDialogProgress initDialog;
        private bool initComplete = false;
        private string initProgressLastAction = string.Empty;
        private int initProgressLastPercent = 0;
        private Thread initThread;
        private bool preventDialogOnLoad = false;

        private ImageSwapper backdrop;
        private AsyncImageResource cover = null;

        private DiskInsertedAction diskInsertedAction;
        Dictionary<string, string> recentInsertedDiskSerials;

        private DBMovieInfo awaitingUserRatingMovie;
        private MoviePlayer moviePlayer;
        double _lastCommandTime = 0;

        private double lastPublished = 0;
        private Timer publishTimer;
        private Timer parentalFilterTimer;

        private string lastParsedParam = "";

        #endregion

        #region GUI Controls

        [SkinControl(50)]
        protected GUIFacadeControl facade = null;

        [SkinControl(51)]
        protected GUIFacadeControl categoriesFacade = null;

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

        [SkinControl(15)]
        protected GUIButtonControl toggleParentalControlsButton = null;

        [SkinControl(16)]
        protected GUIImage parentalControlsIndicator = null;

        [SkinControl(17)]
        protected GUIImage filteringIndicator = null;

        [SkinControl(18)]
        protected GUIImage movieStartIndicator = null;

        #endregion


        public MovingPicturesGUI() {
        }

        ~MovingPicturesGUI() { }

        public bool IsActive {
            get {
                return (GetID == GUIWindowManager.ActiveWindow);
            }
        }

        public string UnparsedLoadParameter {
            get {
                FieldInfo method = typeof(GUIWindow).GetField("_loadParameter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method == null) return null;

                return (string) method.GetValue(this);
            }
        } 

        /// <summary>
        /// The movie passed to Moving Pictures when the plugin window was activated.
        /// </summary>
        public DBMovieInfo MovieLoadParamater {
            get {
                if (lastParsedParam != UnparsedLoadParameter) 
                    ParseParameters();
                    
                return _movieLoadParamater;
            }

            internal set {
                _movieLoadParamater = value;
            }
        } 
        private DBMovieInfo _movieLoadParamater = null;
        

        /// <summary>
        /// The category passed to Moving Pictures when the plugin window was activated.
        /// </summary>
        public DBNode<DBMovieInfo> CategoryLoadParamater {
            get {
                if (lastParsedParam != UnparsedLoadParameter) 
                    ParseParameters();

                return _categoryLoadParamater;
            }

            internal set {
                _categoryLoadParamater = value;
            }
        } 
        private DBNode<DBMovieInfo> _categoryLoadParamater = null;
        public void ShowMessage(string heading, string lines) {
            string line1 = null, line2 = null, line3 = null, line4 = null;
            string[] linesArray = lines.Split(new string[] { "\\n", "\n" }, StringSplitOptions.None);

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
            GUIDialogYesNo dialog = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            try {
                dialog.Reset();
                dialog.SetHeading(heading);
                string[] linesArray = lines.Split(new string[] { "\\n", "\n" }, StringSplitOptions.None);
                if (linesArray.Length > 0) dialog.SetLine(1, linesArray[0]);
                if (linesArray.Length > 1) dialog.SetLine(2, linesArray[1]);
                if (linesArray.Length > 2) dialog.SetLine(3, linesArray[2]);
                if (linesArray.Length > 3) dialog.SetLine(4, linesArray[3]);
                dialog.SetDefaultToYes(defaultYes);

                foreach (GUIControl item in dialog.GetControlList()) {
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
                if (dialog != null) {
                    dialog.ClearAll();
                }
            }
        }

        private void OnBrowserContentsChanged() {
            // update properties
            PublishViewDetails();

            // publish current node settings if any
            if (browser.CurrentNode != null) {
                PublishDetails(browser.CurrentNode, "CurrentNode");
                PublishDetails(browser.CurrentNode.AdditionalSettings, "CurrentNode.Extra.AdditionalSettings");
            }
            else {
                SetProperty("#MovingPictures.CurrentNode.name", MovingPicturesCore.Settings.HomeScreenName);
            }

            // set the label for the remoteFiltering indicator
            if (remoteFilteringIndicator != null && remoteFilter.Active) {
                SetProperty("#MovingPictures.Filter.Label", remoteFilter.Text);
                remoteFilteringIndicator.Visible = true;
            }
            else if (remoteFilteringIndicator != null)
                remoteFilteringIndicator.Visible = false;

            // set the label for the parental controls indicator
            if (parentalControlsIndicator != null && parentalControlsFilter.Active != parentalControlsIndicator.Visible)
                parentalControlsIndicator.Visible = parentalControlsFilter.Active;

            // set the parental controls toggle button visible or 
            // invisible based on if functionality is turned on
            if (toggleParentalControlsButton != null && MovingPicturesCore.Settings.ParentalControlsEnabled != toggleParentalControlsButton.Visible)
                toggleParentalControlsButton.Visible = MovingPicturesCore.Settings.ParentalControlsEnabled;

            // Publish filter details
            PublishFilterDetails();

            // Publish sort details
            if (browser.CurrentView != BrowserViewMode.CATEGORIES && browser.CurrentView != BrowserViewMode.DETAILS) {
                SetProperty("#MovingPictures.Sort.Field", Sort.GetFriendlySortName(browser.CurrentSortField));
                SetProperty("#MovingPictures.Sort.Direction", browser.CurrentSortDirection.ToString());
            }

        }

        private void OnBrowserViewChanged(BrowserViewMode previousView, BrowserViewMode currentView) {
            if (currentView == BrowserViewMode.DETAILS) {
                GUIWindowManager.Process();
                GUIControl.FocusControl(GUIWindowManager.ActiveWindow, playButton.GetID);
            }

            // set the backdrop visibility based on the skin settings
            if (movieBackdropControl != null)
                backdrop.Active = skinSettings.UseBackdrop(currentView);

            if (currentView != BrowserViewMode.CATEGORIES)
                PublishMovieDetails(browser.SelectedMovie);
        }

        void GUIWindowManager_OnActivateWindow(int windowId) {
            if (windowId == (int)Window.WINDOW_HOME || windowId == (int)Window.WINDOW_SECOND_HOME || windowId == (int)Window.WINDOW_MYPLUGINS) {
                if (MovingPicturesCore.Settings.ParentalControlsEnabled && parentalControlsFilter != null && !parentalControlsFilter.Active) {
                    parentalControlsFilter.Active = true;
                    if (parentalFilterTimer != null) {
                        parentalFilterTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    }
                }
            }
        }

        void GUIWindowManager_OnNewAction(MediaPortal.GUI.Library.Action action) {
            ResetParentalFilterTimer();
        }

        #region GUIWindow Methods

        public override int GetID {
            get {
                return 96742;
            }
        }

        public override bool Init() {
            logger.Info("Initializing GUI...");

            // check if we can load the skin
            bool success = Load(GUIGraphicsContext.Skin + @"\movingpictures.xml");

            // get last active module settings 
            bool lastActiveModuleSetting = MovingPicturesCore.MediaPortalSettings.GetValueAsBool("general", "showlastactivemodule", false);
            int lastActiveModule = MovingPicturesCore.MediaPortalSettings.GetValueAsInt("general", "lastactivemodule", -1);
            preventDialogOnLoad = (lastActiveModuleSetting && (lastActiveModule == GetID));

            // set some skin properties
            SetProperty("#MovingPictures.Settings.HomeScreenName", MovingPicturesCore.Settings.HomeScreenName);

            // start initialization of the moving pictures core services in a seperate thread
            initThread = new Thread(new ThreadStart(MovingPicturesCore.Initialize));
            initThread.Start();

            // ... and listen to the progress
            MovingPicturesCore.InitializeProgress += new ProgressDelegate(onCoreInitializationProgress);

            GUIWindowManager.OnNewAction += new OnActionHandler(GUIWindowManager_OnNewAction);
            GUIWindowManager.OnActivateWindow += new GUIWindowManager.WindowActivationHandler(GUIWindowManager_OnActivateWindow);

            return success;
        }

        public override void DeInit() {
            base.DeInit();

            logger.Info("Deinitializing GUI...");

            // if the plugin was not fully initialized yet
            // abort the initialization
            if (!initComplete && initThread.IsAlive) {
                initThread.Abort();
                // wait for the thread to be aborted
                initThread.Join();
            }

            MovingPicturesCore.Shutdown();
            initComplete = false;
            logger.Info("GUI Deinitialization Complete");
        }

        protected override void OnPageLoad() {
            logger.Debug("OnPageLoad() Started.");

            // if the component didn't load properly we probably have a bad skin file
            if (facade == null) {
                // avoid showing a dialog on load when we are the last active module being started
                if (!preventDialogOnLoad) {
                    GUIDialogOK dialog = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                    dialog.Reset();
                    dialog.SetHeading(Translation.Error);
                    dialog.SetLine(1, Translation.ProblemLoadingSkinFile);
                    dialog.DoModal(GetID);
                }
                else {
                    preventDialogOnLoad = false;
                }
                GUIWindowManager.ShowPreviousWindow();
                return;
            }

            // Check whether the plugin is initialized.
            if (!initComplete) {

                // avoid showing a dialog on load when we are the last active module being started
                if (preventDialogOnLoad) {
                    preventDialogOnLoad = false;
                    GUIWindowManager.ShowPreviousWindow();
                    return;
                }

                // if we are not initialized yet show a loading dialog
                // this will 'block' untill loading has finished or the user 
                // pressed cancel or ESC
                showLoadingDialog();

                // if the initialization is not complete the user cancelled
                if (!initComplete) {
                    // return to where the user came from
                    GUIWindowManager.ShowPreviousWindow();
                    return;
                }
            }

            // notify if the skin doesnt support categories
            if (categoriesFacade == null && MovingPicturesCore.Settings.CategoriesEnabled) {
                // avoid showing a dialog on load when we are the last active module being started
                if (!preventDialogOnLoad) {
                    GUIDialogOK dialog = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                    dialog.Reset();
                    dialog.SetHeading(Translation.Error);
                    dialog.SetLine(1, "This skin does not support Categories...");
                    dialog.DoModal(GetID);
                }
                else {
                    preventDialogOnLoad = false;
                    GUIWindowManager.ShowPreviousWindow();
                    return;
                }
            }

            // if the user hasn't defined any import paths they need to goto the config screen
            if (DBImportPath.GetAllUserDefined().Count == 0) {
                if (!preventDialogOnLoad) {
                    ShowMessage(Translation.NoImportPathsHeading, Translation.NoImportPathsBody);
                }
                else {
                    preventDialogOnLoad = false;
                }
                GUIWindowManager.ShowPreviousWindow();
                return;
            }

            // if we were passed a parameter we cant parse, exit back
            if (MovieLoadParamater == null && CategoryLoadParamater == null && !string.IsNullOrEmpty(UnparsedLoadParameter)) {
                logger.Warn("Moving Pictures can not understand the following paramater: " + UnparsedLoadParameter);
                GUIWindowManager.ShowPreviousWindow();           
            }

            if (browser == null) {
                browser = new MovieBrowser(skinSettings);
                MovingPicturesCore.Browser = browser;

                // add available filters to browser
                remoteFilter = new RemoteNumpadFilter();
                browser.Filters.Add(remoteFilter);

                parentalControlsFilter = MovingPicturesCore.Settings.ParentalControlsFilter;
                parentalControlsFilter.Active = MovingPicturesCore.Settings.ParentalControlsEnabled;
                browser.Filters.Add(parentalControlsFilter);

                // setup browser events
                browser.MovieSelectionChanged += new MovieBrowser.MovieSelectionChangedDelegate(MovieDetailsPublisher);
                browser.NodeSelectionChanged += new MovieBrowser.NodeSelectionChangedDelegate(PublishCategoryDetails);
                browser.ContentsChanged += new MovieBrowser.ContentsChangedDelegate(OnBrowserContentsChanged);
                browser.ViewChanged += new MovieBrowser.ViewChangedDelegate(OnBrowserViewChanged);

                // Load all available translation strings
                foreach (string name in Translation.Strings.Keys) {
                    SetProperty("#MovingPictures.Translation." + name + ".Label", Translation.Strings[name]);
                }

                SetProperty("#MovingPictures.Sort.Field", Sort.GetFriendlySortName(browser.CurrentSortField));
                SetProperty("#MovingPictures.Sort.Direction", browser.CurrentSortDirection.ToString());

                // if enabled, activate the default filter
                if (MovingPicturesCore.Settings.DefaultFilterEnabled)
                    browser.FilterNode = MovingPicturesCore.Settings.DefaultFilter;

                if (filteringIndicator != null) filteringIndicator.Visible = false;

                PublishFilterDetails();
            }

            if (recentInsertedDiskSerials == null) {
                // Also listen to new movies added as part of the autoplay/details functionality
                if (diskInsertedAction != DiskInsertedAction.NOTHING) {
                    recentInsertedDiskSerials = new Dictionary<string, string>();
                    MovingPicturesCore.DatabaseManager.ObjectInserted += new DatabaseManager.ObjectAffectedDelegate(OnMovieAdded);
                    logger.Debug("Autoplay/details is now listening for movie additions");
                }
            }

            // (re)link our backdrop image controls to the backdrop image swapper
            backdrop.GUIImageOne = movieBackdropControl;
            backdrop.GUIImageTwo = movieBackdropControl2;
            backdrop.LoadingImage = loadingImage;

            // (re)link the facade controls to the browser object
            browser.Facade = facade;
            browser.CategoriesFacade = categoriesFacade;

            // we are supposed to launch on a movie page, pass this to movie browser
            if (MovieLoadParamater != null) {
                browser.RememberLastState = false;

                browser.SelectedMovie = MovieLoadParamater;
                browser.CurrentView = BrowserViewMode.DETAILS;
                browser.TopLevelView = BrowserViewMode.DETAILS;
            }

            // we are supposed to launch on a category page, pass this to movie browser
            else if (CategoryLoadParamater != null && browser.CategoriesAvailable) {
                browser.RememberLastState = true;

                browser.TopLevelNode = CategoryLoadParamater;
                browser.CurrentNode = CategoryLoadParamater;

                if (CategoryLoadParamater.HasChildren) {
                    browser.TopLevelView = BrowserViewMode.CATEGORIES;
                    browser.CurrentView = BrowserViewMode.CATEGORIES;
                }
                else {
                    browser.TopLevelView = browser.DefaultView;
                    browser.CurrentView = browser.DefaultView;
                }

                loaded = true;
                preventDialogOnLoad = false;

            }

            // standard loading logic
            else {
                if (browser.CategoriesAvailable || browser.LastView != BrowserViewMode.CATEGORIES) {
                    browser.TopLevelView = BrowserViewMode.CATEGORIES;
                    browser.TopLevelNode = null;
                    browser.RememberLastState = true;

                    if (browser.LastView == BrowserViewMode.DETAILS) {
                        browser.SelectedMovie = browser.LastSelectedMovie;
                        browser.CurrentView = BrowserViewMode.DETAILS;
                    }
                    else {
                        browser.SelectedMovie = browser.LastSelectedMovie;
                        browser.CurrentNode = browser.LastNode;
                        browser.CurrentView = browser.LastView;
                    }
                }
                else {
                    //the skin doesn't support categories
                    browser.CurrentNode = null;
                    browser.CurrentView = browser.DefaultView;
                }
            }

            MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#currentmodule", MovingPicturesCore.Settings.HomeScreenName);
            browser.AutoRefresh = true;

            setWorkingAnimationStatus(false);
            if (movieStartIndicator != null)
                movieStartIndicator.Visible = false;

            // Take control and disable MediaPortal AutoPlay when the plugin has focus
            disableNativeAutoplay();

            if (awaitingUserRatingMovie != null) {
                GetUserRating(awaitingUserRatingMovie);
                awaitingUserRatingMovie = null;
            }

            logger.Debug("OnPageLoad() Ended.");
        }

        /// <summary>
        /// If functionality is available, parses paramaters from GUIWindow.LoadParamater
        /// </summary>
        /// <param name="parameterString">String that should be parsed</param>
        private void ParseParameters() {
            lastParsedParam = UnparsedLoadParameter;
            MovieLoadParamater = null;
            CategoryLoadParamater = null;

            // if we cant load params or there is no param passed, quit
            if (string.IsNullOrEmpty(UnparsedLoadParameter)) return;

            try {
                foreach (String currParam in UnparsedLoadParameter.Split('|')) {
                    String[] keyValue = currParam.Split(':');
                    String key = keyValue[0];
                    String value = keyValue[1];

                    try {
                        switch (key) {
                            case "categoryid":
                                CategoryLoadParamater = MovingPicturesCore.DatabaseManager.Get<DBNode<DBMovieInfo>>(Int32.Parse(value));
                                if (CategoryLoadParamater == null) ShowMessage("Moving Pictures", Translation.BadCategory);
                                break;
                            case "categoryname":
                                CategoryLoadParamater = GetCategoryByName(value);
                                if (CategoryLoadParamater == null) ShowMessage("Moving Pictures", Translation.BadCategory);
                                break;
                            case "movieid":
                                MovieLoadParamater = DBMovieInfo.Get(Int32.Parse(value));
                                if (MovieLoadParamater == null) ShowMessage("Moving Pictures", Translation.BadMovie);
                                break;
                        }
                    }
                    catch (FormatException) {
                        logger.Warn("Received invalid parameter: " + currParam);
                        ShowMessage("Moving Pictures", Translation.BadParam);
                    }
                }
            }
            catch (Exception ex) {
                logger.ErrorException("Unexpected error parsing paramater: " + UnparsedLoadParameter, ex);
            }
            

        }

        /// <summary>
        /// Finds a menu level by it's name.
        /// </summary>
        /// <param name="categoryName">Name of the category</param>
        /// <returns>The first match found or null if nothing is found</returns>
        private DBNode<DBMovieInfo> GetCategoryByName(string categoryName) {
            return GetCategoryByName(MovingPicturesCore.Settings.CategoriesMenu.RootNodes, categoryName);
        }

        /// <summary>
        /// Finds a menu level by it's name
        /// </summary>
        /// <param name="nodeList">List of the root nodes where the search is started</param>
        /// <param name="categoryName">Name of the category</param>
        /// <returns>The first match found or null if nothing is found</returns>
        private DBNode<DBMovieInfo> GetCategoryByName(List<DBNode<DBMovieInfo>> nodeList, string categoryName) {
            for (int i = 0; i < nodeList.Count; i++) {
                if (nodeList[i].Name.Equals(categoryName)) {
                    return nodeList[i];
                }

                if (nodeList[i].HasChildren) {
                    DBNode<DBMovieInfo> result = GetCategoryByName(nodeList[i].Children, categoryName);
                    if (result != null) return result;
                }
            }
            return null;
        }

        protected override void OnPageDestroy(int new_windowId) {
            // only execute this when we are initialized
            if (initComplete) {

                // Disable browser auto-refresh
                browser.AutoRefresh = false;

                // Enable autoplay again when we are leaving the plugin
                // But only when we are not playing something
                if (!moviePlayer.IsPlaying) {
                    enableNativeAutoplay();
                }

                base.OnPageDestroy(new_windowId);
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

                case 51:
                    if (actionType == MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM) {
                        browser.CurrentNode = browser.SelectedNode;
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

                // a click on the filter button
                case 4:
                    showFilterContext();
                    browser.Focus();
                    break;

                // a click on the play button
                case 6:
                    playSelectedMovie();
                    break;

                // a click on the sort menu button
                case 14:
                    showSortContext();
                    browser.Focus();
                    break;

                // parental controls button clicked
                case 15:
                    toggleParentalControls();
                    browser.Focus();
                    break;
            }

            base.OnClicked(controlId, control, actionType);
        }

        public override void OnAction(MediaPortal.GUI.Library.Action action) {
            // do nothing when we are not initialized
            if (!initComplete)
                return;

            switch (action.wID) {
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_PARENT_DIR:
                    GUIWindowManager.ShowPreviousWindow();
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_PREVIOUS_MENU:

                    if (browser.CurrentView == BrowserViewMode.DETAILS) {
                        if (browser.TopLevelView == BrowserViewMode.DETAILS) {
                            //the skin opened movingpictures to show movie details -> return to mp on back
                            browser.SelectedMovie = null;
                            GUIWindowManager.ShowPreviousWindow();
                        }
                        else {
                            // return to the one of the facade screens
                            browser.CurrentView = browser.PreviousView;
                        }
                    }
                    else if (remoteFilter.Active) {
                        // if a remote filter is active remove it
                        remoteFilter.Clear();
                    }
                    else if (browser.CategoriesAvailable && browser.CurrentNode != null && browser.CurrentNode != browser.TopLevelNode) {
                        // go to the parent category
                        browser.CurrentNode = browser.CurrentNode.Parent;
                    }
                    else {
                        // The user presses back in the topmost node -> clear the history and exit
                        browser.LastNode = null;
                        browser.LastView = BrowserViewMode.CATEGORIES;
                        browser.LastSelectedMovie = null;
                        browser.SelectedMovie = null;
                        // show previous screen (exit the plug-in)
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
                    if (remoteFilterEnabled && browser.CurrentView != BrowserViewMode.DETAILS && browser.CurrentView != BrowserViewMode.CATEGORIES) {
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
                    lock (browser.Facade) {
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

        private void ResetParentalFilterTimer() {
            if (MovingPicturesCore.Settings.ParentalControlsEnabled
                && parentalControlsFilter != null
                && !parentalControlsFilter.Active
                && MovingPicturesCore.Settings.ParentalControlsTimeout > 0
                && !moviePlayer.IsPlaying
                ) {
                if (parentalFilterTimer == null) {
                    parentalFilterTimer = new Timer(delegate {
                        logger.Info("Reactivating parental filter after {0} minutes idle", MovingPicturesCore.Settings.ParentalControlsTimeout);
                        parentalControlsFilter.Active = true;
                    }, null, MovingPicturesCore.Settings.ParentalControlsTimeout * 60 * 1000, Timeout.Infinite);
                }
                else {
                    parentalFilterTimer.Change(MovingPicturesCore.Settings.ParentalControlsTimeout * 60 * 1000, Timeout.Infinite);
                }
            }
        }

        public override bool OnMessage(GUIMessage message) {
            return base.OnMessage(message);
        }

        #endregion

        #region Loading and initialization

        private void showLoadingDialog() {
            initDialog = (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
            initDialog.Reset();
            initDialog.ShowProgressBar(true);
            initDialog.SetHeading("Loading Moving Pictures");
            initDialog.SetLine(1, string.Empty);
            initDialog.SetLine(2, initProgressLastAction);
            initDialog.SetPercentage(initProgressLastPercent);
            initDialog.Progress();
            initDialog.DoModal(GetID);
        }

        private void onCoreInitializationProgress(string actionName, int percentDone) {

            // Update the progress variables
            if (percentDone == 100) {
                actionName = "Loading GUI ...";
            }
            initProgressLastAction = actionName;
            initProgressLastPercent = percentDone;

            // If the progress dialog exists, update it.
            if (initDialog != null) {
                initDialog.SetLine(2, initProgressLastAction);
                initDialog.SetPercentage(initProgressLastPercent);
                initDialog.Progress();
            }

            // When we are finished initializing
            if (percentDone == 100) {

                // Start the background importer
                if (MovingPicturesCore.Settings.EnableImporterInGUI) {
                    MovingPicturesCore.Importer.Start();
                    MovingPicturesCore.Importer.Progress += new MovieImporter.ImportProgressHandler(Importer_Progress);
                }

                // Load skin based settings from skin file
                skinSettings = new MovingPicturesSkinSettings(_windowXmlFileName);

                // Get Moving Pictures specific autoplay setting
                try {
                    diskInsertedAction = (DiskInsertedAction)Enum.Parse(typeof(DiskInsertedAction), MovingPicturesCore.Settings.DiskInsertionBehavior);
                }
                catch {
                    diskInsertedAction = DiskInsertedAction.DETAILS;
                }

                // setup the image resources for cover and backdrop display
                int artworkDelay = MovingPicturesCore.Settings.ArtworkLoadingDelay;

                // setup the time for the random category backdrop refresh
                activeMovieLookup.Timeout = new TimeSpan(0, 0, MovingPicturesCore.Settings.CategoryRandomArtworkRefreshInterval);

                // create backdrop image swapper
                backdrop = new ImageSwapper();
                backdrop.ImageResource.Delay = artworkDelay;
                backdrop.PropertyOne = "#MovingPictures.Backdrop";
                backdrop.PropertyTwo = "#MovingPictures.Backdrop2";

                // create cover image swapper
                cover = new AsyncImageResource();
                cover.Property = "#MovingPictures.Coverart";
                cover.Delay = artworkDelay;



                // instantiate player
                moviePlayer = new MoviePlayer(this);
                moviePlayer.MovieEnded += new MoviePlayerEvent(onMovieEnded);
                moviePlayer.MovieStopped += new MoviePlayerEvent(onMovieStopped);

                // Listen to the DeviceManager for external media activity (i.e. disks inserted)
                logger.Debug("Listening for device changes.");
                DeviceManager.OnVolumeInserted += new DeviceManager.DeviceManagerEvent(OnVolumeInserted);
                DeviceManager.OnVolumeRemoved += new DeviceManager.DeviceManagerEvent(OnVolumeRemoved);

                // Flag that the GUI is initialized
                initComplete = true;

                // If the initDialog is present close it
                if (initDialog != null) {
                    initDialog.Close();
                }

                // Report that we completed the init
                logger.Info("GUI Initialization Complete");
            }
        }

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
            IDialogbox dialog = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dialog.Reset();
            dialog.SetHeading("Moving Pictures");  // not translated because it's a proper noun

            GUIListItem movieOptionsItem = new GUIListItem(Translation.MovieOptions + " ...");
            GUIListItem parentalControlsItem = new GUIListItem(parentalControlsFilter.Active ? Translation.UnlockRestrictedMovies : Translation.LockRestrictedMovies);
            GUIListItem filterItem = new GUIListItem(Translation.FilterBy + " ...");
            GUIListItem searchItem = new GUIListItem(Translation.SearchBy + "...");
            GUIListItem sortItem = new GUIListItem(Translation.SortBy + " ...");
            GUIListItem viewItem = new GUIListItem(Translation.ChangeView + " ...");
            GUIListItem rescanItem = new GUIListItem(Translation.ScanForNewMovies);
            
            int currID = 1;

            // if we are not in categories view make Movie Options the primary selection
            if (browser.CurrentView != BrowserViewMode.CATEGORIES) {
                movieOptionsItem.ItemId = currID++;
                dialog.Add(movieOptionsItem);
            }

            
            if (MovingPicturesCore.Settings.ParentalControlsEnabled) {
                parentalControlsItem.ItemId = currID++;
                dialog.Add(parentalControlsItem);
            }

            
            filterItem.ItemId = currID++;
            dialog.Add(filterItem);

            searchItem.ItemId = currID++;
            dialog.Add(searchItem);

            // show these options only when we are not in the categories view
            if (browser.CurrentView != BrowserViewMode.CATEGORIES) {
                sortItem.ItemId = currID++;
                dialog.Add(sortItem);

                viewItem.ItemId = currID++;
                dialog.Add(viewItem);
            }

            // add rescan menu item if needed
            if (MovingPicturesCore.Settings.ShowRescanMenuItem) {
                rescanItem.ItemId = currID++;
                dialog.Add(rescanItem);
            }

            dialog.DoModal(GUIWindowManager.ActiveWindow);
            if (dialog.SelectedId == filterItem.ItemId) {
                showFilterContext();
            }
            else if (dialog.SelectedId == sortItem.ItemId) {
                showSortContext();
            }
            else if (dialog.SelectedId == viewItem.ItemId) {
                showChangeViewContext();
            }
            else if (dialog.SelectedId == movieOptionsItem.ItemId) {
                showDetailsContext();
            }
            else if (dialog.SelectedId == parentalControlsItem.ItemId) {
                toggleParentalControls();
            }
            else if (dialog.SelectedId == rescanItem.ItemId) {
                MovingPicturesCore.Importer.RestartScanner();
            }
            else if (dialog.SelectedId == searchItem.ItemId) {
                showSearchContext();
            }
        }

        private bool showFilterContext() {
            return showFilterContext(MovingPicturesCore.Settings.FilterMenu.RootNodes, browser.FilterNode != null, null);
        }

        private bool showFilterContext(ICollection<DBNode<DBMovieInfo>> nodeList, bool showClearMenuItem, HashSet<DBMovieInfo> availableMovies) {
            if (nodeList.Count == 0) {
                ShowMessage("No Filters", "There are no filters to display.");
                return false;
            }

            // build list of available movies as if no filter were currently applied
            // used for checking which filter nodes have movies to display
            if (availableMovies == null) {
                availableMovies = new HashSet<DBMovieInfo>();
                foreach (DBMovieInfo currMovie in browser.AllMovies) {
                    availableMovies.Add(currMovie);
                }

                if (browser.CurrentNode != null) {
                    foreach (DBFilter<DBMovieInfo> currFilter in browser.CurrentNode.GetAllFilters())
                        availableMovies = currFilter.Filter(availableMovies);
                }
            }

            while (true) {
                IDialogbox dialog = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                dialog.Reset();
                dialog.SetHeading(Translation.FilterBy);

                Dictionary<int, DBNode<DBMovieInfo>> nodeLookup = new Dictionary<int, DBNode<DBMovieInfo>>();

                // add clear menu item as necessary
                int currID = 1;
                GUIListItem clearListItem = new GUIListItem(Translation.AllMovies);
                if (showClearMenuItem) {
                    clearListItem.ItemId = currID++;
                    dialog.Add(clearListItem);
                }

                // build menu
                foreach (DBNode<DBMovieInfo> currNode in nodeList) {
                    HashSet<DBMovieInfo> possibleMovies = currNode.GetPossibleFilteredItems();
                    if (possibleMovies.Count == 0)
                        continue;

                    possibleMovies.IntersectWith(availableMovies);
                    if (possibleMovies.Count == 0)
                        continue;

                    if (browser.FilterNode == currNode)
                        continue;

                    GUIListItem newListItem = new GUIListItem(Translation.ParseString(currNode.Name));
                    newListItem.ItemId = currID++;
                    dialog.Add(newListItem);

                    nodeLookup[newListItem.ItemId] = currNode;
                }

                // display popup
                dialog.DoModal(GUIWindowManager.ActiveWindow);

                // handle selection
                if (dialog.SelectedId == -1) {
                    return false;
                }
                if (dialog.SelectedId == clearListItem.ItemId) {
                    browser.FilterNode = null;
                    PublishFilterDetails();
                    return true;
                }
                else {
                    // apply the filter
                    DBNode<DBMovieInfo> selectedNode = nodeLookup[dialog.SelectedId];
                    if (selectedNode.Children.Count == 0 && selectedNode.Filter != null) {
                        browser.FilterNode = selectedNode;
                        PublishFilterDetails();
                        return true;
                    }
                    // handle sub menus if needed
                    else {
                        if (showFilterContext(selectedNode.Children, false, availableMovies))
                            return true;
                    }
                }
            }
        }

        private void showSortContext() {
            IDialogbox dialog = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dialog.Reset();
            dialog.SetHeading(Translation.SortBy);

            foreach (int value in Enum.GetValues(typeof(SortingFields))) {
                SortingFields field = (SortingFields)Enum.Parse(typeof(SortingFields), value.ToString());
                string menuCaption = Sort.GetFriendlySortName(field);
                GUIListItem listItem = new GUIListItem(menuCaption);
                listItem.ItemId = value;

                if (field == browser.CurrentSortField) {
                    // indicate the current sort direction using a graphic image, if the file does not exist
                    // use a string label to represent the sort direction
                    if (browser.CurrentSortDirection == SortingDirections.Ascending) {
                        string filename = GUIGraphicsContext.Skin + @"\Media\movingpictures_SortAscending.png";
                        if (!System.IO.File.Exists(filename))
                            listItem.Label2 = Translation.DownAbbreviation;
                        else
                            listItem.IconImage = filename;
                    }
                    else {
                        string filename = GUIGraphicsContext.Skin + @"\Media\movingpictures_SortDescending.png";
                        if (!System.IO.File.Exists(filename))
                            listItem.Label2 = Translation.UpAbbreviation;
                        else
                            listItem.IconImage = filename;
                    }
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

            // If we are in a category store the sorting used
            if (browser.CurrentNode != null) {
                DBMovieNodeSettings nodeSettings = (DBMovieNodeSettings)browser.CurrentNode.AdditionalSettings;
                nodeSettings.UseDefaultSorting = false;
                nodeSettings.SortField = browser.CurrentSortField;
                nodeSettings.SortDirection = browser.CurrentSortDirection;
                nodeSettings.Commit();
            }

            // Reload movie facade with selection reset
            browser.ReloadMovieFacade();
            browser.SelectedMovie = null;

            SetProperty("#MovingPictures.Sort.Field", Sort.GetFriendlySortName(browser.CurrentSortField));
            SetProperty("#MovingPictures.Sort.Direction", browser.CurrentSortDirection.ToString());

        }

        private void showSearchContext() {
            IDialogbox dialog = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dialog.Reset();
            dialog.SetHeading("Moving Pictures - " + Translation.SearchBy);

            int currID = 1;
            GUIListItem titleItem = new GUIListItem(Translation.Title);
            titleItem.ItemId = currID++;
            dialog.Add(titleItem);

            GUIListItem castItem = new GUIListItem(Translation.CastAndCrew);
            castItem.ItemId = currID++;
            dialog.Add(castItem);

            GUIListItem themeItem = new GUIListItem(Translation.Theme);
            themeItem.ItemId = currID++;
            dialog.Add(themeItem);

            dialog.DoModal(GUIWindowManager.ActiveWindow);

            if (dialog.SelectedId == titleItem.ItemId) {
                Search();
            }
            else if (dialog.SelectedId == castItem.ItemId) {
                Search();
            }
            else if (dialog.SelectedId == themeItem.ItemId) {
                Search();
            }
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

            GUIListItem flowItem = new GUIListItem(Translation.CoverFlowView);
            if (skinSettings.CoverFlowViewAvailable)
            {
                flowItem.ItemId = currID++;
                dialog.Add(flowItem);
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
            else if (dialog.SelectedId == flowItem.ItemId)
            {
                browser.CurrentView = BrowserViewMode.COVERFLOW;
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
            int selectedIndex = browser.SelectedIndex;
            DBMovieInfo selectedMovie = browser.SelectedMovie;

            detailsItem = new GUIListItem(Translation.UpdateDetailsFromOnline);
            detailsItem.ItemId = currID;
            dialog.Add(detailsItem);
            currID++;

            if (selectedMovie.AlternateCovers.Count > 1) {
                cycleArtItem = new GUIListItem(Translation.CycleCoverArt);
                cycleArtItem.ItemId = currID;
                dialog.Add(cycleArtItem);
                currID++;
            }

            if (selectedMovie.CoverFullPath.Trim().Length == 0 ||
                selectedMovie.BackdropFullPath.Trim().Length == 0) {

                retrieveArtItem = new GUIListItem(Translation.CheckForMissingArtwork);
                retrieveArtItem.ItemId = currID;
                dialog.Add(retrieveArtItem);
                currID++;
            }

            if (selectedMovie.ActiveUserSettings.WatchedCount > 0) {
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

            rateItem = new GUIListItem(Translation.RateHeading);
            rateItem.ItemId = currID;
            dialog.Add(rateItem);
            currID++;

            dialog.DoModal(GUIWindowManager.ActiveWindow);
            if (dialog.SelectedId == detailsItem.ItemId) {
                updateMovieDetails(selectedMovie);
            }
            else if (dialog.SelectedId == cycleArtItem.ItemId) {

                selectedMovie.NextCover();
                browser.AutoRefresh = false;
                selectedMovie.Commit();
                browser.AutoRefresh = true;

                // update the new cover art in the facade
                GUIListItem listItem = browser.GetMovieListItem(selectedMovie);
                if (listItem != null) {
                    listItem.IconImage = selectedMovie.CoverThumbFullPath.Trim();
                    listItem.IconImageBig = selectedMovie.CoverThumbFullPath.Trim();
                    listItem.RefreshCoverArt();
                }

                PublishArtwork(selectedMovie);
            }
            else if (dialog.SelectedId == retrieveArtItem.ItemId) {
                logger.Info("Updating artwork for " + selectedMovie.Title);
                updateMovieArtwork(selectedMovie);
            }
            else if (dialog.SelectedId == unwatchedItem.ItemId) {
                logger.Info("Marking '{0}' as unwatched.", selectedMovie.Title);
                updateMovieWatchedCount(selectedMovie, 0);
                browser.SelectedIndex = selectedIndex;

            }
            else if (dialog.SelectedId == watchedItem.ItemId) {
                logger.Info("Marking '{0}' as watched.", selectedMovie.Title);
                updateMovieWatchedCount(selectedMovie, 1);
                browser.SelectedIndex = selectedIndex;
            }
            else if (dialog.SelectedId == deleteItem.ItemId) {
                deleteMovie(selectedMovie);
                browser.SelectedIndex = selectedIndex;
            }
            else if (dialog.SelectedId == rateItem.ItemId) {
                if (GetUserRating(selectedMovie)) {
                    PublishMovieDetails(selectedMovie);
                }
            }
        }

        private bool GetUserRating(DBMovieInfo movie) {
            GUIGeneralRating ratingDlg = (GUIGeneralRating)GUIWindowManager.GetWindow(GUIGeneralRating.ID);
            ratingDlg.Reset();
            ratingDlg.SetHeading(Translation.RateHeading);
            ratingDlg.SetLine(1, String.Format(Translation.SelectYourRating, movie.Title));
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

        private void toggleParentalControls() {
            if (!MovingPicturesCore.Settings.ParentalControlsEnabled) {
                ShowMessage("Moving Pictures", Translation.ParentalControlsDisabled);
                return;
            }

            if (!parentalControlsFilter.Active || ValidatePin())
                parentalControlsFilter.Active = !parentalControlsFilter.Active;
        }

        private void Search() {
            string searchStr = getUserInput();
            Search(searchStr);
        }

        private void Search(string searchStr) {
            ShowMessage("what have you done???", searchStr + "? wtf?!");
        }

        /// <summary>
        /// Displays on screen keyboard prompting the user for input.
        /// </summary>
        /// <returns>the string entered by the user or null if the user canceled.</returns>
        private string getUserInput() {
            VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
            if (keyboard == null) {
                ShowMessage("Oops", "Internal error launching virtual keyboard.");
                return null;
            }

            keyboard.Reset();
            keyboard.IsSearchKeyboard = true;
            keyboard.DoModal(GUIWindowManager.ActiveWindow);

            if (keyboard.IsConfirmed) 
                return keyboard.Text;

            return null;
        }

        private bool ValidatePin() {
            GUIPinCodeDialog pinCodeDialog = (GUIPinCodeDialog)GUIWindowManager.GetWindow(GUIPinCodeDialog.ID);
            pinCodeDialog.Reset();
            pinCodeDialog.MasterCode = MovingPicturesCore.Settings.ParentalContolsPassword;
            pinCodeDialog.SetHeading(Translation.PinCodeHeader);
            pinCodeDialog.SetLine(1, Translation.PinCodePrompt);
            pinCodeDialog.InvalidPinMessage = Translation.InvalidPin;

            try {
                pinCodeDialog.DoModal(GetID);
            }
            catch (ArgumentNullException) {
                ShowMessage(Translation.Error, Translation.SkinDoesNotSupportPinDialog);
                return false;
            }

            return pinCodeDialog.IsCorrect;
        }

        private void deleteMovie(DBMovieInfo movie) {
            DBLocalMedia firstFile = movie.LocalMedia[0];

            // if the file is available and read only, or known to be stored on optical media, prompt to ignore.
            if ((firstFile.IsAvailable && firstFile.File.IsReadOnly) || firstFile.ImportPath.IsOpticalDrive) {
                bool bIgnore = ShowCustomYesNo("Moving Pictures", Translation.CannotDeleteReadOnly, null, null, false);

                if (bIgnore) {
                    movie.DeleteAndIgnore();
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
            string sDoYouWant = String.Format(Translation.DoYouWantToDelete, movie.Title);
            bool bConfirm = ShowCustomYesNo("Moving Pictures", sDoYouWant, null, null, false);

            if (bConfirm) {
                bool deleteSuccesful = movie.DeleteFiles();

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

        #endregion

        #region Movie Update Methods

        delegate DBMovieInfo MovieUpdateWorker(DBMovieInfo movie);

        /// <summary>
        /// Updates the movie's watched counter
        /// </summary>
        /// <param name="movie"></param>
        /// <param name="newWatchedCount">new watched count</param>
        private void updateMovieWatchedCount(DBMovieInfo movie, int newWatchedCount) {
            movie.ActiveUserSettings.WatchedCount = newWatchedCount;
            movie.ActiveUserSettings.Commit();
            browser.UpdateListColors(movie);
            browser.ReapplyFilters();
            if (browser.CurrentView != BrowserViewMode.DETAILS) {
                browser.ReloadMovieFacade();
            }

            // (re-)publish movie details if the movie is still the same
            if (browser.SelectedMovie == movie)
                PublishMovieDetails(movie);

            if (newWatchedCount == 0)
                MovingPicturesCore.Follwit.UnwatchMovie(movie);
            else
                MovingPicturesCore.Follwit.WatchMovie(movie, false);
        }

        /// <summary>
        /// Updates the movie details in a background thread using the preferred dataprovider.
        /// </summary>
        /// <param name="movie"></param>
        private void updateMovieDetails(DBMovieInfo movie) {
            bool bConfirm = ShowCustomYesNo(Translation.UpdateMovieDetailsHeader, Translation.UpdateMovieDetailsBody, null, null, false);
            if (bConfirm && movie != null) {
                MovieUpdateWorker updater = new MovieUpdateWorker(movieDetailsUpdateWorker);
                updater.BeginInvoke(movie, new AsyncCallback(movieDetailsUpdateFinished), updater);
            }
        }

        private DBMovieInfo movieDetailsUpdateWorker(DBMovieInfo movie) {

            // indicate that we are doing some work here
            setWorkingAnimationStatus(true);

            logger.Info("Updating movie details for '{0}'", movie.Title);

            MovingPicturesCore.DataProviderManager.Update(movie);
            movie.Commit();
            foreach (DBLocalMedia lm in movie.LocalMedia) {
                lm.UpdateMediaInfo();
                lm.Commit();
            }

            // indicate we are done
            setWorkingAnimationStatus(false);

            return movie;
        }

        private void movieDetailsUpdateFinished(IAsyncResult result) {
            MovieUpdateWorker updater = (MovieUpdateWorker)result.AsyncState;

            // End the operation
            DBMovieInfo movie = updater.EndInvoke(result);

            logger.Info("Finished updating details for '{0}'", movie.Title);

            // Reload the active facade to enforce changes in sorting and publishing
            browser.ReapplyFilters();
            browser.ReloadFacade();

            // Re-publish when this movie is still the selected movie
            if (browser.SelectedMovie == movie) {
                PublishMovieDetails(movie);
            }
        }

        /// <summary>
        /// Updates the movie artwork in a background thread using the preferred dataprovider.
        /// </summary>
        /// <param name="movie"></param>
        private void updateMovieArtwork(DBMovieInfo movie) {
            if (movie != null) {
                MovieUpdateWorker updater = new MovieUpdateWorker(movieArtworkUpdateWorker);
                updater.BeginInvoke(movie, new AsyncCallback(movieArtworkUpdateFinished), updater);
            }
        }

        private DBMovieInfo movieArtworkUpdateWorker(DBMovieInfo movie) {
            // indicate that we are doing some work here
            setWorkingAnimationStatus(true);

            logger.Info("Updating covers for '{0}'", movie.Title);

            if (movie.CoverFullPath.Trim().Length == 0) {
                MovingPicturesCore.DataProviderManager.GetArtwork(movie);
                movie.Commit();
            }

            logger.Info("Updating backdrop for '{0}'", movie.Title);

            if (movie.BackdropFullPath.Trim().Length == 0) {
                new LocalProvider().GetBackdrop(movie);
                MovingPicturesCore.DataProviderManager.GetBackdrop(movie);
                movie.Commit();
            }

            // indicate we are done
            setWorkingAnimationStatus(false);

            return movie;
        }

        private void movieArtworkUpdateFinished(IAsyncResult result) {
            MovieUpdateWorker updater = (MovieUpdateWorker)result.AsyncState;

            // End the operation
            DBMovieInfo movie = updater.EndInvoke(result);

            logger.Info("Finished updating artwork for '{0}'", movie.Title);

            // Update Artwork
            if (browser.SelectedMovie == movie) {
                // Publish it when the current selection is this movie
                PublishArtwork(movie);
            }

            // Refresh the artwork on the related list item
            RefreshMovieArtwork(movie);

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

            if (movieStartIndicator != null) {
                movieStartIndicator.Visible = true;
                GUIWindowManager.Process();
            }

            if (parentalFilterTimer != null)
                parentalFilterTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            // Play movie
            moviePlayer.Play(browser.SelectedMovie);
            if (movieStartIndicator != null)
                movieStartIndicator.Visible = false;
        }

        private void onMovieStopped(DBMovieInfo movie) {
            // If we or stopping in another windows enable native auto-play again
            // This will most of the time be the fullscreen playback window, 
            // if we would re-enter the plugin, autoplay will be disabled again.
            if (GetID != GUIWindowManager.ActiveWindow)
                enableNativeAutoplay();

            ResetParentalFilterTimer();
        }

        private void onMovieEnded(DBMovieInfo movie) {

            // Rating
            if (MovingPicturesCore.Settings.AutoPromptForRating)
                awaitingUserRatingMovie = movie;

            // Reapply Filters
            browser.UpdateListColors(movie);
            browser.ReapplyFilters();

            onMovieStopped(movie);
        }

        private void setWorkingAnimationStatus(bool visible) {
            try {
                if (workingAnimation != null) {
                    if (visible)
                        workingAnimation.AllocResources();
                    else
                        workingAnimation.Dispose();

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
            if (GUIWindowManager.ActiveWindow != GetID && !moviePlayer.IsPlaying && !GUIWindowManager.IsRouted)
                return;

            logger.Debug("OnVolumeInserted: Volume={0}, Serial={1}", volume, serial);

            if (GUIWindowManager.ActiveWindow == GetID) {

                // Clear recent disc information
                logger.Debug("Resetting Recent Disc Information.");
                recentInsertedDiskSerials.Clear();

                // DVD / Blu-ray 
                // Try to grab a valid video path from the disc
                string moviePath = VideoUtility.GetVideoPath(volume);

                // Try to grab the movie from our DB
                DBLocalMedia localMedia = DBLocalMedia.Get(moviePath, serial);

                DBMovieInfo movie = null;
                int part = 1;

                if (localMedia.IsVideoDisc) {

                    logger.Info("Video Disc Detected.");

                    // For DVDs we have to make sure this is the correct
                    // media file, a simple availability check will point out 
                    // if we should requery the database using the discid
                    if (!localMedia.IsAvailable)
                        localMedia = DBLocalMedia.GetDisc(moviePath, localMedia.VideoFormat.GetIdentifier(moviePath));

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

        public void SetProperty(string property, string value) {
            SetProperty(property, value, false);
        }

        public void SetProperty(string property, string value, bool forceLogging) {
            if (property == null)
                return;

            if (MovingPicturesCore.Settings.LogAllSkinPropertyChanges)
                forceLogging = true;

            try {
                lock (propertySync) {

                    if (loggedProperties == null)
                        loggedProperties = new Dictionary<string, bool>();

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

            // If the value is empty add a space
            // otherwise the property will keep 
            // displaying it's previous value
            if (String.IsNullOrEmpty(value))
                GUIPropertyManager.SetProperty(property, " ");

            GUIPropertyManager.SetProperty(property, value);
        }

        /// <summary>
        /// Resets the property values for every key that starts with the given string
        /// </summary>
        /// <param name="startsWith">the prefix to reset</param>
        public void ResetProperties(string startsWith) {
            logger.Debug("Resetting properties: {0}", startsWith);
            foreach (string key in loggedProperties.Keys) {
                if (key.StartsWith(startsWith))
                    SetProperty(key, "");
            }
        }

        delegate void MoviePublishWorker(DBMovieInfo movie);

        /// <summary>
        /// Publishes movie details to the skin. If multiple requests are done within a short period of time the publishing is delayed 
        /// and only the last request is then published to the skin. This will speed up browsing on "heavy" skins that show a lot of
        /// detail on the current selection.
        /// </summary>
        /// <param name="movie">the movie that needs to be published</param>
        private void MovieDetailsPublisher(DBMovieInfo movie) {
            double tickCount = AnimationTimer.TickCount;

            // Publish instantly when previous request has passed the required delay
            if (MovingPicturesCore.Settings.DetailsLoadingDelay < (int)(tickCount - lastPublished)) {
                lastPublished = tickCount;

                // publish using the threadpool (experimental tweak!)
                MoviePublishWorker publisher = new MoviePublishWorker(PublishMovieDetails);
                publisher.BeginInvoke(movie, null, null);
                return;
            }

            // Publish on timer using the delay specified in settings
            lastPublished = tickCount;

            if (publishTimer == null) {
                publishTimer = new Timer(delegate { PublishMovieDetails(browser.SelectedMovie); }, null, MovingPicturesCore.Settings.DetailsLoadingDelay, Timeout.Infinite);
            }
            else {
                publishTimer.Change(MovingPicturesCore.Settings.DetailsLoadingDelay, Timeout.Infinite);
            }

        }

        /// <summary>
        /// Publishes movie details to the skin instantly
        /// </summary>
        /// <param name="movie"></param>
        private void PublishMovieDetails(DBMovieInfo movie) {
            if (movie == null) {
                // Clear the movie related skin properties
                ResetProperties("#MovingPictures.SelectedMovie");
                ResetProperties("#MovingPictures.UserMovieSettings");
                ResetProperties("#MovingPictures.LocalMedia");
            }
            else {
                // publish details on selected movie
                PublishDetails(movie, "SelectedMovie");
                PublishDetails(movie.ActiveUserSettings, "UserMovieSettings");
                PublishDetails(movie.LocalMedia[0], "LocalMedia");

                // publish easily usable subtitles info
                SetProperty("#MovingPictures.LocalMedia.Subtitles", movie.LocalMedia.HasSubtitles() ? "subtitles" : "nosubtitles");

                // publish details on selected movie
                PublishDetails(movie, "SelectedMovie");
                PublishDetails(movie.ActiveUserSettings, "UserMovieSettings");
                if (movie.ActiveUserSettings.UserRating == null)
                    SetProperty("#MovingPictures.UserMovieSettings.10point_user_rating", null);
                else
                    SetProperty("#MovingPictures.UserMovieSettings.10point_user_rating", (movie.ActiveUserSettings.UserRating.GetValueOrDefault() * 2).ToString());

                PublishDetails(movie.LocalMedia[0], "LocalMedia");

                // publish the selected index in the facade
                int selectedIndex = browser.SelectedIndex + 1;
                SetProperty("#MovingPictures.SelectedIndex", selectedIndex.ToString());
                SetProperty("#MovingPictures.Settings.BackdropMovieTitle", movie.Title);
            }

            if (selectedMovieWatchedIndicator != null) {
                selectedMovieWatchedIndicator.Visible = false;
                if (movie != null && movie.ActiveUserSettings != null)
                    if (movie.ActiveUserSettings.WatchedCount > 0)
                        selectedMovieWatchedIndicator.Visible = true;
            }

            PublishArtwork(movie);
        }

        /// <summary>
        /// Publishes categorie (node) details to the skin instantly
        /// </summary>
        /// <param name="node"></param>
        private void PublishCategoryDetails(DBNode<DBMovieInfo> node) {
            if (node == null) {
                // Clear the category related skin properties
                ResetProperties("#MovingPictures.SelectedNode");
            }
            else {
                // Publish category details
                PublishDetails(node, "SelectedNode");
                PublishDetails(node.AdditionalSettings, "SelectedNode.Extra.AdditionalSettings");

                // publish category node name in a format that can always be used as a filename.
                SetProperty("#MovingPictures.SelectedNode.FileFriendlyName", Translation.ParseString(node.Name).ToValidFilename());
            }

            // Publish Category Artwork
            PublishArtwork(node);
        }

        /// <summary>
        /// Publishes standard object details to the skin
        /// </summary>
        /// <param name="obj">any object derived from databasetable</param>
        /// <param name="prefix">prefix for the generated skin properties</param>
        private void PublishDetails(DatabaseTable obj, string prefix) {
            PublishDetails(obj, prefix, false);
        }

        /// <summary>
        /// Publishes standard object details to the skin
        /// </summary>
        /// <param name="obj">any object derived from databasetable</param>
        /// <param name="prefix">prefix for the generated skin properties</param>
        /// <param name="forceLogging">indicate wether to log all properties</param>
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
                else if (currField.FieldName == "user" && tableType == typeof(DBUserMovieSettings)) {
                    // rename the "user" field to "username" to prevent overlapping variable names
                    propertyStr = "#MovingPictures." + prefix + ".username";
                    valueStr = currField.GetValue(obj).ToString().Trim();
                    SetProperty(propertyStr, valueStr, forceLogging);
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
                    for (int i = 0; i < maxStringListElements; i++) {
                        // note, the "extra" in the middle is needed due to a bug in skin parser
                        propertyStr = "#MovingPictures." + prefix + ".extra." + currField.FieldName + "." + (i + 1);
                        if (i < max) {
                            valueStr = valueStrList[i];
                        } else {
                            valueStr = null;
                        }
                        SetProperty(propertyStr, valueStr, forceLogging);
                    }
                }
                // for the movie score we add some special properties to give skinners more options
                else if (currField.FieldName == "score" && tableType == typeof(DBMovieInfo)) {
                    propertyStr = "#MovingPictures." + prefix + "." + currField.FieldName;

                    float score = (float)currField.GetValue(obj);
                    int percentage = (int)Math.Floor((score * 10));
                    int major = (int)Math.Floor(score);
                    int minor = (int)Math.Floor(((score - major) * 10));
                    int rounded = (int)(score + 0.5f);

                    NumberFormatInfo localizedScoreFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                    localizedScoreFormat.NumberDecimalDigits = 1;

                    NumberFormatInfo invariantScoreFormat = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
                    invariantScoreFormat.NumberDecimalDigits = 1;

                    SetProperty(propertyStr + ".localized", score.ToString("N", localizedScoreFormat), forceLogging);
                    SetProperty(propertyStr + ".invariant", score.ToString("N", invariantScoreFormat), forceLogging);
                    SetProperty(propertyStr + ".rounded", rounded.ToString(), forceLogging);
                    SetProperty(propertyStr + ".percentage", percentage.ToString(), forceLogging);
                    SetProperty(propertyStr + ".major", major.ToString(), forceLogging);
                    SetProperty(propertyStr + ".minor", minor.ToString(), forceLogging);

                }
                // for the movie runtime we also add some special properties
                else if (currField.FieldName == "runtime" && tableType == typeof(DBMovieInfo)) {
                    propertyStr = "#MovingPictures." + prefix + "." + currField.FieldName;
                    DBMovieInfo movie = (DBMovieInfo)obj;

                    int seconds = 0;
                    bool actualRuntime = false;

                    // Check the user preference and display the runtime requested
                    // If the user choose actual runtime and it is not available default
                    // to the imported runtime
                    if (MovingPicturesCore.Settings.DisplayActualRuntime && movie.ActualRuntime > 0) {
                        // Actual runtime (as calculated by mediainfo)
                        // convert duration from milliseconds to seconds
                        seconds = (movie.ActualRuntime / 1000);
                        actualRuntime = true;
                    }
                    else {
                        // Runtime (as provided by the dataprovider)
                        // convert from minutes to seconds
                        seconds = (movie.Runtime * 60);
                    }

                    // Publish the runtime
                    PublishRuntime(seconds, actualRuntime, "#MovingPictures." + prefix + ".runtime.", forceLogging);

                }
                // for the popularity we add a localized property to make it easier to read in skins
                else if (currField.FieldName == "popularity" && tableType == typeof(DBMovieInfo)) {
                    propertyStr = "#MovingPictures." + prefix + "." + currField.FieldName;

                    int popularity = (int)currField.GetValue(obj);

                    NumberFormatInfo localizedScoreFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                    localizedScoreFormat.NumberDecimalDigits = 0;

                    // Publish Popularity
                    SetProperty(propertyStr + ".raw", popularity.ToString(), forceLogging);
                    SetProperty(propertyStr + ".localized", popularity.ToString("N", localizedScoreFormat), forceLogging);

                }
                // for floats we need to make sure we use english style printing or imagelist controls
                // will break. 
                else if (value.GetType() == typeof(float)) {
                    propertyStr = "#MovingPictures." + prefix + "." + currField.FieldName;
                    valueStr = ((float)currField.GetValue(obj)).ToString(CultureInfo.CreateSpecificCulture("en-US"));
                    SetProperty(propertyStr, valueStr, forceLogging);

                    // vanilla publication
                }
                else {
                    propertyStr = "#MovingPictures." + prefix + "." + currField.FieldName;
                    valueStr = currField.GetValue(obj).ToString().Trim();

                    // Category names have an extra translation check
                    if (currField.FieldName == "name" && tableType == typeof(DBNode<DBMovieInfo>))
                        valueStr = Translation.ParseString(valueStr);

                    SetProperty(propertyStr, valueStr, forceLogging);
                }
            }
        }

        /// <summary>
        /// Calculates and publishes information about the runtime to the skin
        /// </summary>
        /// <param name="totalSeconds">the runtime in seconds</param>
        /// <param name="actualRuntime">indicates wether this information is the actual runtime</param>
        /// <param name="labelPrefix">>prefix for the generated skin properties</param>
        /// <param name="forceLogging">indicate wether to log all properties</param>
        private void PublishRuntime(int totalSeconds, bool actualRuntime, string labelPrefix, bool forceLogging) {
            string valueStr;

            // pre-0.8 prefix
            // todo: remove these properties in 0.9 ?
            string bcPrefix = labelPrefix.Replace(".runtime.", ".extra.runtime.");

            // create the time components
            int hours = (totalSeconds / 3600);
            int minutes = ((totalSeconds / 60) % 60);
            int seconds = (totalSeconds % 60);
            int totalMinutes = (totalSeconds / 60);
            
            // publish the value indicating wether this is the actual runtime
            SetProperty(labelPrefix + "actual", actualRuntime.ToString(), forceLogging);

            // publish the runtime in total seconds 
            SetProperty(labelPrefix + "totalseconds", totalSeconds.ToString(), forceLogging);

            // publish hour component of runtime
            SetProperty(labelPrefix + "hour", hours.ToString(), forceLogging);
            SetProperty(bcPrefix + "hour", hours.ToString(), forceLogging);

            // publish minute component of runtime
            SetProperty(labelPrefix + "minute", minutes.ToString(), forceLogging);
            // pre-0.8
            SetProperty(bcPrefix + "minute", minutes.ToString(), forceLogging);

            // publish second component of runtime
            SetProperty(labelPrefix + "second", seconds.ToString(), forceLogging);

            // give localized hour string
            string hourLocalized = (hours != 1) ? Translation.RuntimeHours : Translation.RuntimeHour;
            hourLocalized = string.Format(hourLocalized, hours);
            SetProperty(labelPrefix + "localized.hour", hourLocalized, forceLogging);

            // publish localized minute string
            string minLocalized = (minutes != 1) ? Translation.RuntimeMinutes : Translation.RuntimeMinute;
            minLocalized = string.Format(minLocalized, minutes);
            SetProperty(labelPrefix + "localized.minute", minLocalized, forceLogging);

            // publish localized second string
            string secLocalized = (seconds != 1) ? Translation.RuntimeSeconds : Translation.RuntimeSecond;
            secLocalized = string.Format(secLocalized, seconds);
            SetProperty(labelPrefix + "localized.second", secLocalized, forceLogging);

            // publish localized short string
            valueStr = string.Format(Translation.RuntimeShort, hours, minutes, seconds);
            SetProperty(labelPrefix + "localized.short", valueStr, forceLogging);
            // pre-0.8
            SetProperty(bcPrefix + "short", valueStr, forceLogging);

            // publish localized extended short string
            valueStr = string.Format(Translation.RuntimeShortExtended, hours, minutes, seconds);
            SetProperty(labelPrefix + "localized.short.extended", valueStr, forceLogging);

            // publish localized long string
            // When the duration is less than an hour it will display the localized minute string.
            if (hours > 0) {
                if (minutes > 0) // show hours and minutes
                    valueStr = string.Format(Translation.RuntimeLong, hourLocalized, minLocalized, secLocalized);
                else // display hours
                    valueStr = hourLocalized;
            } else { // display minutes
                valueStr = minLocalized;
            }
            SetProperty(labelPrefix + "localized.long", valueStr, forceLogging);

            // pre-0.8
            SetProperty(bcPrefix + "en.pretty", valueStr, forceLogging);

            // publish localized extended long string
            // When the duration is less than an hour it will display the localized long string.
            valueStr = (hours < 1) ? string.Format(Translation.RuntimeLong, minLocalized, secLocalized) : string.Format(Translation.RuntimeLongExtended, hourLocalized, minLocalized, secLocalized);
            SetProperty(labelPrefix + "localized.long.extended", valueStr, forceLogging);

            // publish the runtime in total minutes 
            SetProperty(labelPrefix + "totalminutes", totalMinutes.ToString(), forceLogging);
        }

        /// <summary>
        /// Publishes details about the current view  to the skin
        /// </summary>
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

            // combined filtered movie count with Movie(s) translation
            propertyStr = "#MovingPictures.general.itemcount";
            valueStr = string.Format(valueStr + " {0}", browser.FilteredMovies.Count == 1 ? Translation.Movie : Translation.Movies);
            SetProperty(propertyStr, valueStr);
        }

        /// <summary>
        /// Publishes details about the filters  to the skin
        /// </summary>
        private void PublishFilterDetails() {
            logger.Debug("updating filter properties");
            if (browser.FilterNode == null) {
                if (filteringIndicator != null) filteringIndicator.Visible = false;
                SetProperty("#MovingPictures.Filter.Combined", " ");
                SetProperty("#MovingPictures.Filter.Name", " ");
                SetProperty("#MovingPictures.Filter.Category", " ");
            }
            else if (browser.FilterNode.Parent == null) {
                SetProperty("#MovingPictures.Filter.Combined", Translation.ParseString(browser.FilterNode.Name));
                SetProperty("#MovingPictures.Filter.Name", Translation.ParseString(browser.FilterNode.Name));
                SetProperty("#MovingPictures.Filter.Category", " ");
                if (filteringIndicator != null) filteringIndicator.Visible = true;
            }
            else {
                SetProperty("#MovingPictures.Filter.Combined", Translation.ParseString(browser.FilterNode.Parent.Name) + ": " + Translation.ParseString(browser.FilterNode.Name));
                SetProperty("#MovingPictures.Filter.Name", Translation.ParseString(browser.FilterNode.Name));
                SetProperty("#MovingPictures.Filter.Category", Translation.ParseString(browser.FilterNode.Parent.Name));
                if (filteringIndicator != null) filteringIndicator.Visible = true;
            }

        }

        /// <summary>
        /// Publishes movie related artwork  to the skin
        /// </summary>
        /// <param name="movie"></param>
        private void PublishArtwork(DBMovieInfo movie) {
            if (movie == null) {
                logger.Debug("Clearing Movie Artwork");
                cover.Filename = string.Empty;
                backdrop.Filename = string.Empty;
                return;
            }

            logger.Debug("Publishing Movie Artwork");

            cover.Filename = movie.CoverFullPath;
            backdrop.Filename = movie.BackdropFullPath;
        }

        /// <summary>
        /// Publishes category (node) related artwork to the skin
        /// </summary>
        /// <param name="node"></param>
        private void PublishArtwork(DBNode<DBMovieInfo> node) {
            if (node == null) {
                logger.Debug("Clearing Category Artwork");
                cover.Filename = string.Empty;
                backdrop.Filename = string.Empty;
                return;
            }

            logger.Debug("Publishing Category Artwork");

            cover.Filename = string.Empty;

            // grab the node settings
            DBMovieNodeSettings settings = node.AdditionalSettings as DBMovieNodeSettings;
            if (settings == null) {
                settings = new DBMovieNodeSettings();
                node.AdditionalSettings = settings;
            }

            // grab the backdrop
            switch (settings.BackdropType) {
                case MenuBackdropType.FILE:
                    backdrop.Filename = settings.BackdropFilePath;
                    SetProperty("#MovingPictures.Settings.BackdropMovieTitle", "");
                    break;
                case MenuBackdropType.MOVIE:
                    backdrop.Filename = settings.BackdropMovie.BackdropFullPath;
                    SetProperty("#MovingPictures.Settings.BackdropMovieTitle", settings.BackdropMovie.Title);
                    break;
                case MenuBackdropType.RANDOM:
                    DBMovieInfo movie = null;
                    HashSet<DBMovieInfo> movies = browser.GetAvailableMovies(node);
                    lock (backdropSync) {
                        // Check if this node has an active movie cached and if it's still active.
                        if (activeMovieLookup.ContainsKey(node) && movies.Contains(activeMovieLookup[node])) {
                            movie = activeMovieLookup[node];
                        } else {
                            // grab a new random movie from the visible movies that has a backdrop
                            movie = movies.Where(m => m.BackdropFullPath != null && m.BackdropFullPath.Trim().Length > 0).ToList().Random();
                            // if we found one add it to our lookup list to speed up future requests
                            if (movie != null) activeMovieLookup[node] = movie;
                        }
                    }
                    // change the backdrop or set to null 
                    backdrop.Filename = (movie != null) ? movie.BackdropFullPath : null;
                    SetProperty("#MovingPictures.Settings.BackdropMovieTitle"
                        , (movie != null) ? movie.Title : null);
                    break;
            }
        }

        /// <summary>
        /// Forces a refresh of the artwork allocated to the GUIListItem of this movie
        /// </summary>
        /// <param name="movie"></param>
        private void RefreshMovieArtwork(DBMovieInfo movie) {
            GUIListItem listItem = browser.GetMovieListItem(movie);
            if (listItem != null) listItem.RefreshCoverArt();
        }

        #endregion

    }
}