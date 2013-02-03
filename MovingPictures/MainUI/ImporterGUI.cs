using System;
using System.Collections.Generic;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using Action = MediaPortal.GUI.Library.Action;
using NLog;
using MediaPortal.Plugins.MovingPictures.Database;
using System.Collections;
using Cornerstone.MP;

namespace MediaPortal.Plugins.MovingPictures.MainUI {
    public class ImporterGUI : GUIWindow {
        #region Skin Controls

        [SkinControlAttribute(310)]
        private GUIListControl allFilesListControl = null;
        
        [SkinControlAttribute(311)]
        private GUIListControl pendingFilesListControl = null;

        [SkinControlAttribute(312)]
        private GUIListControl completedFileListControl = null;

        [SkinControl(400)]
        protected GUIImage movieBackdropControl = null;

        [SkinControl(401)]
        protected GUIImage movieBackdropControl2 = null;
        
        [SkinControl(18)]
        protected GUIImage movieStartIndicator = null;

        [SkinControl(19)]
        protected GUIButtonControl scanButton = null;

        [SkinControl(20)]
        protected GUIButtonControl restoreIgnoredButton = null;

        #endregion

        public enum FilterMode {
            ALL,
            PENDING,
            COMPLETED
        }

        private ImageSwapper backdrop;

        private const string IdleIcon = "";
        private const string IgnoredIcon = "";
        private const string ProcessingIcon = "movingpictures_processing.png";
        private const string NeedInputIcon = "movingpictures_needinput.png";
        private const string DoneIcon = "movingpictures_done.png";

        private static Logger logger = LogManager.GetCurrentClassLogger();

        // used to prevent us from manipulating the UI from multiple threads at the same time
        private object statusChangedSyncToken = new object();
        private object progressSyncToken = new object();

        // keeps track of what we are displaying and stores our GUIListItems for reuse
        private Dictionary<MovieMatch, GUIListItem> listItemLookup = new Dictionary<MovieMatch, GUIListItem>();
        private List<GUIListItem> allItems = new List<GUIListItem>();
        private List<GUIListItem> pendingItems = new List<GUIListItem>();
        private List<GUIListItem> completedItems = new List<GUIListItem>();
        
        private bool connected = false;

        /// <summary>
        /// The current list the user is viewing. Can be set to change the view.
        /// </summary>
        public FilterMode Mode {
            set {
                _mode = value;

                string label = "";
                switch (_mode) {
                    case FilterMode.ALL:
                        label = Translation.AllFiles;
                        if (!allFilesListControl.IsFocused) GUIControl.FocusControl(GetID, allFilesListControl.GetID);
                        break;
                    case FilterMode.PENDING:
                        label = Translation.FilesNeedingAttention;
                        if (!pendingFilesListControl.IsFocused) GUIControl.FocusControl(GetID, pendingFilesListControl.GetID);
                        break;
                    case FilterMode.COMPLETED:
                        label = Translation.CompletedFiles;
                        if (!completedFileListControl.IsFocused) GUIControl.FocusControl(GetID, completedFileListControl.GetID);
                        break;
                }

                GUIPropertyManager.SetProperty("#MovingPictures.Importer.ListMode.Flag", _mode.ToString());
                GUIPropertyManager.SetProperty("#MovingPictures.Importer.ListMode.Label", label);
                
                allFilesListControl.Visible = _mode == FilterMode.ALL;
                pendingFilesListControl.Visible = _mode == FilterMode.PENDING;
                completedFileListControl.Visible = _mode == FilterMode.COMPLETED;

                MovingPicturesCore.Settings.LastGuiImporterMode = _mode; 

                UpdateArtwork();
            }

            get {
                return _mode;
            }
        } private FilterMode _mode;

        /// <summary>
        /// The GUIListControl currently being used by the user.
        /// </summary>
        private GUIListControl ActiveListControl {
            get {
                switch (_mode) {
                    case FilterMode.ALL:
                        return allFilesListControl;
                    case FilterMode.PENDING:
                        return pendingFilesListControl;
                    case FilterMode.COMPLETED:
                        return completedFileListControl;
                    default:
                        return null;
                } 
            }
        }

        #region MediaPortal Logic

        public override int GetID {
            get { return 96743; }
        }

        public override bool Init() {
            logger.Debug("Initializing Importer Screen");

            // create backdrop image swapper
            backdrop = new ImageSwapper();
            backdrop.ImageResource.Delay = MovingPicturesCore.Settings.ArtworkLoadingDelay;
            backdrop.PropertyOne = "#MovingPictures.Importer.Backdrop1";
            backdrop.PropertyTwo = "#MovingPictures.Importer.Backdrop2";

            return Load(GUIGraphicsContext.Skin + @"\movingpictures.importer.xml");
        }

        protected override void OnPageLoad() {
            logger.Debug("Launching Importer Screen");

            // make sure we have what we need to proceed
            GUIPropertyManager.SetProperty("#MovingPictures.Importer.Status", " ");
            if (!VerifySkinSupport() || !VerifyImporterEnabled() || !VerifyParentalControls()) return;

            ConnectToImporter();
            InitializeControls();

            base.OnPageLoad();
        }

        protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType) {
            // clicked on one of the lists
            if ((control == allFilesListControl || control == pendingFilesListControl || control == completedFileListControl) && actionType == Action.ActionType.ACTION_SELECT_ITEM) {
                DisplayMovieOptionsDialog();
                return;
            }

            // button clicks
            if (control == scanButton) MovingPicturesCore.Importer.RestartScanner();
            if (control == restoreIgnoredButton) MovingPicturesCore.Importer.RestoreAllIgnoredFiles();
        }

        public override void OnAction(Action action) {
            base.OnAction(action);

            switch (action.wID) {
                case Action.ActionType.ACTION_MOVE_RIGHT:
                case Action.ActionType.ACTION_MOVE_LEFT:
                case Action.ActionType.ACTION_MOVE_UP:
                case Action.ActionType.ACTION_MOVE_DOWN:
                    int focusedControlId = GetFocusControlId();
                    if (focusedControlId == allFilesListControl.GetID) Mode = FilterMode.ALL;
                    if (focusedControlId == pendingFilesListControl.GetID) Mode = FilterMode.PENDING;
                    if (focusedControlId == completedFileListControl.GetID) Mode = FilterMode.COMPLETED;
                    break;
            }
        }

        protected override void OnShowContextMenu() {
            base.OnShowContextMenu();
            DisplayContextMenu();
        }

        private void ListItemSelected(GUIListItem item, GUIControl parent) {
            UpdateArtwork();
        }

        #endregion

        #region Startup Logic

        /// <summary>
        /// Check if the importer is meant to run in the GUI, if not let the user know. Backs out if the user keeps the importer off.
        /// </summary>
        private bool VerifyImporterEnabled() {
            if (MovingPicturesCore.Settings.EnableImporterInGUI) return true;

            bool enable = MovingPicturesGUI.ShowCustomYesNo(Translation.ImporterDisabled, Translation.ImporterDisabledMessage, null, null, false, GetID);

            MovingPicturesCore.Settings.EnableImporterInGUI = enable;

            if (enable) MovingPicturesCore.Importer.Start();
            else GUIWindowManager.ShowPreviousWindow();

            return enable;
        }

        /// <summary>
        /// Checks that the current skin supports the importer. Backs out if it does not.
        /// </summary>
        private bool VerifySkinSupport() {
            bool skinSupported = allFilesListControl != null && pendingFilesListControl != null && completedFileListControl != null;
            if (!skinSupported) {
                GUIWindowManager.ShowPreviousWindow();
                MovingPicturesGUI.ShowMessage("Moving Pictures", Translation.SkinDoesNotSupportImporter, GetID);
                return false;
            }

            return true;
        }

        /// <summary>
        /// If parental controls are enabled make sure the user is not up to no good.
        /// </summary>
        private bool VerifyParentalControls() {
            if (MovingPicturesCore.Settings.ParentalControlsEnabled && !MovingPicturesGUI.ValidatePin(GetID)) {
                GUIWindowManager.ShowPreviousWindow();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Grab current importer information and begin listening for updates.
        /// </summary>
        private void ConnectToImporter() {
            if (connected) return;

            lock (progressSyncToken) {
                lock (statusChangedSyncToken) {
                    MovingPicturesCore.Importer.Progress += new MovieImporter.ImportProgressHandler(ImporterProgress);
                    MovingPicturesCore.Importer.MovieStatusChanged += new MovieImporter.MovieStatusChangedHandler(MovieStatusChangedListener);

                    ArrayList importerAll = MovingPicturesCore.Importer.AllMatches;
                    ArrayList importerNeedInput = MovingPicturesCore.Importer.MatchesNeedingInput;
                    ArrayList importerRetrieving = MovingPicturesCore.Importer.RetrievingDetailsMatches;
                    ArrayList importerApproved = MovingPicturesCore.Importer.ApprovedMatches;
                    ArrayList importerPriorityApproved = MovingPicturesCore.Importer.PriorityApprovedMatches;
                    ArrayList importerCommitted = MovingPicturesCore.Importer.CommitedMatches;

                    foreach (MovieMatch match in importerAll) {
                        GUIListItem listItem = GetListItem(match);
                        AddToList(listItem, FilterMode.ALL);

                        if (importerNeedInput.Contains(match)) {
                            listItem.PinImage = NeedInputIcon;
                            AddToList(listItem, FilterMode.PENDING);
                        }

                        if (importerRetrieving.Contains(match) || importerApproved.Contains(match) || importerPriorityApproved.Contains(match)) {
                            listItem.PinImage = ProcessingIcon;
                        }

                        if (importerCommitted.Contains(match)) {
                            listItem.PinImage = DoneIcon;
                            AddToList(listItem, FilterMode.COMPLETED);
                        }
                    }
                }
            }

            connected = true;
        }

        /// <summary>
        /// Initializes and populates controls on the screen. 
        /// </summary>
        private void InitializeControls() {
            Mode = MovingPicturesCore.Settings.LastGuiImporterMode;

            AddRangeToList(allFilesListControl, allItems);
            AddRangeToList(pendingFilesListControl, pendingItems);
            AddRangeToList(completedFileListControl, completedItems);

            backdrop.GUIImageOne = movieBackdropControl;
            backdrop.GUIImageTwo = movieBackdropControl2;

            if (movieStartIndicator != null) movieStartIndicator.Visible = false;
            UpdateArtwork();
        }

        #endregion

        #region Update Logic

        /// <summary>
        /// Publish progress updates to the skin as they are received.
        /// </summary>
        private void ImporterProgress(int percentDone, int taskCount, int taskTotal, string taskDescription) {
            lock (progressSyncToken) {
                if (GUIPropertyManager.GetProperty("#MovingPictures.Importer.Status") != taskDescription) {
                    GUIPropertyManager.SetProperty("#MovingPictures.Importer.Status", taskDescription);
                }

                GUIPropertyManager.SetProperty("#MovingPictures.Importer.CurrentTask.Count", taskCount.ToString());
                GUIPropertyManager.SetProperty("#MovingPictures.Importer.CurrentTask.Total", taskTotal.ToString());
                if (taskTotal > 0) GUIPropertyManager.SetProperty("#MovingPictures.Importer.CurrentTask.Percentage", (Convert.ToInt16(((decimal)taskCount / taskTotal) * 100)).ToString());                    
                GUIPropertyManager.SetProperty("#MovingPictures.Importer.TotalProgressPercent", percentDone.ToString());

                GUIPropertyManager.SetProperty("#MovingPictures.Importer.IsActive", MovingPicturesCore.Importer.IsScanning.ToString());

                int unprocessed = MovingPicturesCore.Importer.PendingMatches.Count + MovingPicturesCore.Importer.PriorityPendingMatches.Count;
                int needInput = MovingPicturesCore.Importer.MatchesNeedingInput.Count;
                int approved = MovingPicturesCore.Importer.ApprovedMatches.Count + MovingPicturesCore.Importer.PriorityApprovedMatches.Count;
                int retrieving = MovingPicturesCore.Importer.RetrievingDetailsMatches.Count;
                int done = MovingPicturesCore.Importer.CommitedMatches.Count;

                GUIPropertyManager.SetProperty("#MovingPictures.Importer.All.Count", (unprocessed + needInput + approved + retrieving + done).ToString());
                GUIPropertyManager.SetProperty("#MovingPictures.Importer.NeedInput.Count", needInput.ToString());
                GUIPropertyManager.SetProperty("#MovingPictures.Importer.Done.Count", done.ToString());

                GUIPropertyManager.SetProperty("#MovingPictures.Importer.Waiting.Count", unprocessed.ToString());
                GUIPropertyManager.SetProperty("#MovingPictures.Importer.Processing.Count", (approved + retrieving).ToString()); 
            }
        }

        /// <summary>
        /// Receives updates about specific items in the importer and updates the UI accordingly.
        /// </summary>
        private void MovieStatusChangedListener(MovieMatch match, MovieImporterAction action) {
            lock (statusChangedSyncToken) {
                // if the importer fired up or shut down clear out our display
                if (action == MovieImporterAction.STARTED || action == MovieImporterAction.STOPPED) {
                    allItems.Clear();
                    pendingItems.Clear();
                    completedItems.Clear();

                    listItemLookup.Clear();
                    allFilesListControl.ListItems.Clear();
                    pendingFilesListControl.ListItems.Clear();
                    completedFileListControl.ListItems.Clear();
                    return;
                }

                // our message is about a specific match
                GUIListItem listItem = GetListItem(match);
                AddToList(listItem, FilterMode.ALL);

                switch (action) {
                    // file is queued but not yet processed have no icon
                    case MovieImporterAction.ADDED:
                    case MovieImporterAction.ADDED_FROM_SPLIT:
                    case MovieImporterAction.ADDED_FROM_JOIN:
                        listItem.PinImage = IdleIcon;    
                        break;

                    // files that are currently being scanned are blue
                    case MovieImporterAction.PENDING:
                    case MovieImporterAction.GETTING_MATCHES:
                    case MovieImporterAction.APPROVED:
                    case MovieImporterAction.GETTING_DETAILS:
                        listItem.PinImage = ProcessingIcon;
                        break;

                    // files that need help from the user are yellow
                    case MovieImporterAction.NEED_INPUT:
                        listItem.PinImage = NeedInputIcon;
                        AddToList(listItem, FilterMode.PENDING);
                        break;
                    
                    // files that have successfully imported are green
                    case MovieImporterAction.COMMITED:
                        listItem.PinImage = DoneIcon;
                        AddToList(listItem, FilterMode.COMPLETED);
                        RemoveFromList(listItem, FilterMode.PENDING);
                        break;

                    case MovieImporterAction.IGNORED:
                        listItem.PinImage = IgnoredIcon;
                        AddToList(listItem, FilterMode.PENDING);
                        RemoveFromList(listItem, FilterMode.COMPLETED);
                        break;
                }

                UpdateArtwork();
            }
        }

        private void AddRangeToList(GUIListControl listControl, List<GUIListItem> listItems) {
            listControl.Clear();

            foreach (var item in listItems) {
                listControl.Add(item);
            }
        }

        private void AddToList(GUIListItem listItem, FilterMode mode) {
            GUIListControl listControl = allFilesListControl;
            List<GUIListItem> internalList = allItems;

            switch (mode) {
                case FilterMode.ALL:
                    listControl = allFilesListControl;
                    internalList = allItems;
                    break;
                case FilterMode.PENDING:
                    listControl = pendingFilesListControl;
                    internalList = pendingItems;
                    break;
                case FilterMode.COMPLETED:
                    listControl = completedFileListControl;
                    internalList = completedItems;
                    break;
            }

            if (!internalList.Contains(listItem)) {
                internalList.Add(listItem);
                listControl.Add(listItem);
            }
        }

        private void RemoveFromList(GUIListItem listItem, FilterMode mode) {
            GUIListControl listControl = allFilesListControl;
            List<GUIListItem> internalList = allItems;

            switch (mode) {
                case FilterMode.ALL:
                    listControl = allFilesListControl;
                    internalList = allItems;
                    break;
                case FilterMode.PENDING:
                    listControl = pendingFilesListControl;
                    internalList = pendingItems;
                    break;
                case FilterMode.COMPLETED:
                    listControl = completedFileListControl;
                    internalList = completedItems;
                    break;
            }

            if (internalList.Contains(listItem)) {
                internalList.Remove(listItem);
                listControl.ListItems.Remove(listItem);
            }
        }

        private GUIListItem GetListItem(MovieMatch match) {
            // create or grab our list item
            GUIListItem listItem = null;
            if (listItemLookup.ContainsKey(match)) {
                listItem = listItemLookup[match];
            }
            else {
                listItem = new GUIListItem();
                listItem.OnItemSelected += new GUIListItem.ItemSelectedHandler(ListItemSelected);
                listItemLookup[match] = listItem;
            }

            // populate it with most recent info
            listItem.Label = match.LocalMediaString;
            listItem.Label2 = (match.Selected != null ? match.Selected.DisplayMember : "");
            listItem.Label3 = match.LongLocalMediaString;
            listItem.IsPlayed = match.LocalMedia.Count == 0 ? false : match.LocalMedia[0].Ignored;
            listItem.AlbumInfoTag = match;

            return listItem;
        }

        private void UpdateArtwork() {
            GUIListItem item = ActiveListControl.SelectedListItem;
            if (item == null) return;

            MovieMatch match = item.AlbumInfoTag as MovieMatch;
            if (match == null) return;

            if (MovingPicturesCore.Importer.CommitedMatches.Contains(match) && match.Selected != null) {
                backdrop.Filename = match.Selected.Movie.BackdropFullPath;
            }
            else {
                backdrop.Filename = string.Empty;
            }
        }

        #endregion

        #region Popup Menus

        private void DisplayContextMenu() {
            if (ActiveListControl == null) return;
            
            IDialogbox dialog = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dialog.Reset();
            dialog.SetHeading("Moving Pictures");  // not translated because it's a proper noun

            GUIListItem rescanItem = new GUIListItem(Translation.ScanForNewMovies);
            GUIListItem unignoreItem = new GUIListItem(Translation.RestoreIgnoredFiles);

            int currID = 1;

            rescanItem.ItemId = currID++;
            dialog.Add(rescanItem);

            unignoreItem.ItemId = currID++;
            dialog.Add(unignoreItem);

            dialog.DoModal(GUIWindowManager.ActiveWindow);
            if (dialog.SelectedId == rescanItem.ItemId) {
                MovingPicturesCore.Importer.RestartScanner();
            }

            else if (dialog.SelectedId == unignoreItem.ItemId) {
                MovingPicturesCore.Importer.RestoreAllIgnoredFiles();
            }
        }

        private void DisplayMovieOptionsDialog() {
            MovieMatch selectedFile = ActiveListControl.SelectedListItem == null ? null : ActiveListControl.SelectedListItem.AlbumInfoTag as MovieMatch;

            // create our dialog
            GUIDialogMenu matchDialog = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            if (matchDialog == null) {
                logger.Error("Could not create matches dialog.");
                return;
            }

            int maxid = 0; 
            matchDialog.Reset();
            matchDialog.SetHeading(Translation.PossibleMatches);

            int selectId = ++maxid;
            matchDialog.Add(Translation.SelectCorrectMovie + " ...");

            int playId = ++maxid;
            matchDialog.Add(Translation.PlayMovie);

            int ignoreId = ++maxid;
            matchDialog.Add(Translation.IgnoreMovie);

            // launch dialog and let user make choice
            matchDialog.DoModal(GUIWindowManager.ActiveWindow);

            // if the user canceled bail
            if (matchDialog.SelectedId == -1)
                return;

            // open selection dialog
            if (matchDialog.SelectedId == selectId) {
                bool success = DisplayMovieSelectionDialog();
                if (!success) DisplayMovieOptionsDialog();
                return;
            }

            // ignore file
            if (matchDialog.SelectedId == ignoreId) {
                MovingPicturesCore.Importer.Ignore(selectedFile);
            }

            if (matchDialog.SelectedId == playId) {
                if (movieStartIndicator != null) {
                    movieStartIndicator.Visible = true;
                    GUIWindowManager.Process();
                }

                // Play movie
                if (MovingPicturesCore.Importer.CommitedMatches.Contains(selectedFile))
                    MovingPicturesCore.Player.Play(selectedFile.Selected.Movie);
                else 
                    MovingPicturesCore.Player.playFile(selectedFile.LocalMedia[0].FullPath);

                if (movieStartIndicator != null)
                    movieStartIndicator.Visible = false;
            }
        }

        private bool DisplayMovieSelectionDialog() {
            MovieMatch selectedFile = ActiveListControl.SelectedListItem == null ? null : ActiveListControl.SelectedListItem.AlbumInfoTag as MovieMatch;

            // create our dialog
            GUIDialogMenu matchDialog = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            if (matchDialog == null) {
                logger.Error("Could not create matches dialog.");
                return false;
            }

            int maxid = 0;
            matchDialog.Reset();
            matchDialog.SetHeading(Translation.PossibleMatches);

            // add our possible matches to it (list of movies)
            foreach (var match in selectedFile.PossibleMatches) {
                matchDialog.Add(match.DisplayMember);
                maxid++;
            }

            int searchId = ++maxid;
            matchDialog.Add(Translation.SearchForMore + " ...");

            // launch dialog and let user make choice
            matchDialog.DoModal(GUIWindowManager.ActiveWindow);

            // if the user canceled bail
            if (matchDialog.SelectedId == -1)
                return false;

            // get new search criteria
            if (matchDialog.SelectedId == searchId) {
                bool searched = DisplaySearchDialog(selectedFile);
                if (!searched) return DisplayMovieSelectionDialog();
                return true;
            }

                        // user picked a movie, assign it
            selectedFile.Selected = selectedFile.PossibleMatches[matchDialog.SelectedId - 1];
            MovingPicturesCore.Importer.Approve(selectedFile);

            return true;
        }

        private bool DisplaySearchDialog(MovieMatch selectedFile) {
            GUIDialogMenu dialog = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            if (dialog == null) {
                logger.Error("Could not create search dialog.");
                return false;
            }

            int maxid = 0;
            dialog.Reset();
            dialog.SetHeading(Translation.Search);
            
            int titleId = ++maxid;
            dialog.Add(string.Format("{0}: {1}", Translation.Title, selectedFile.Signature.Title));

            int yearId = ++maxid;
            dialog.Add(string.Format("{0}: {1}", Translation.Year, selectedFile.Signature.Year));

            int imdbId = ++maxid;
            dialog.Add(string.Format("{0}: {1}", Translation.ImdbId, selectedFile.Signature.ImdbId));

            dialog.DoModal(GUIWindowManager.ActiveWindow);

            // user picked nothing, go back to previous dialog
            if (dialog.SelectedId == -1) {
                return false;
            }

            // build and display our virtual keyboard
            VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
            keyboard.Reset();
            keyboard.IsSearchKeyboard = true;

            if (dialog.SelectedId == titleId) keyboard.Text = selectedFile.Signature.Title;
            if (dialog.SelectedId == yearId) keyboard.Text = (selectedFile.Signature.Year == null) ? "" : selectedFile.Signature.Year.ToString();
            if (dialog.SelectedId == imdbId) keyboard.Text = (selectedFile.Signature.ImdbId == null) ? "" : selectedFile.Signature.ImdbId;
            keyboard.DoModal(GUIWindowManager.ActiveWindow);

            // if the user escaped out redisplay the searchdialog
            if (!keyboard.IsConfirmed) return DisplaySearchDialog(selectedFile);

            // user entered something so update the movie signature
            if (dialog.SelectedId == titleId) selectedFile.Signature.Title = keyboard.Text;
            if (dialog.SelectedId == yearId) selectedFile.Signature.Year = Convert.ToInt32(keyboard.Text);
            if (dialog.SelectedId == imdbId) selectedFile.Signature.ImdbId = keyboard.Text;
            MovingPicturesCore.Importer.Reprocess(selectedFile);

            return true;
        }

        #endregion
    }
}
