using System;
using System.Collections.Generic;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using Action = MediaPortal.GUI.Library.Action;
using NLog;
using MediaPortal.Plugins.MovingPictures.Database;

namespace MediaPortal.Plugins.MovingPictures.MainUI {
    public class ImporterGUI : GUIWindow {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [SkinControlAttribute(310)]
        private GUIListControl allFilesListControl = null;
        
        [SkinControlAttribute(311)]
        private GUIListControl pendingFilesListControl = null;

        [SkinControlAttribute(312)]
        private GUIListControl completedFileListControl = null;
        
        [SkinControl(18)]
        protected GUIImage movieStartIndicator = null;

        [SkinControl(19)]
        protected GUIButtonControl scanButton = null;

        [SkinControl(20)]
        protected GUIButtonControl restoreIgnoredButton = null;

        private bool skinSupported = false;
        private bool initialPageLoad = true;
        private object statusChangedSyncToken = new object();
        private object progressSyncToken = new object();
        private Dictionary<MovieMatch, GUIListItem> listItemLookup = new Dictionary<MovieMatch, GUIListItem>();

        private List<GUIListItem> allItems = new List<GUIListItem>();
        private List<GUIListItem> pendingItems = new List<GUIListItem>();
        private List<GUIListItem> completedItems = new List<GUIListItem>();
        
        private GUIListControl activeList;

        public override int GetID {
            get { return 96743; }
        }

        public override bool Init() {
            logger.Debug("Initializing Importer Screen");
            return Load(GUIGraphicsContext.Skin + @"\movingpictures.importer.xml");
        }

        protected override void OnPageLoad() {
            logger.Debug("Launching Importer Screen");

            skinSupported = allFilesListControl != null && pendingFilesListControl != null && completedFileListControl != null;

            if (!skinSupported) {
                GUIWindowManager.ShowPreviousWindow();
                MovingPicturesGUI.ShowMessage("Moving Pictures", Translation.SkinDoesNotSupportImporter, GetID);
                return;
            }

            bool enabled = VerifyImporterEnabled();
            if (!enabled) {
                GUIWindowManager.ShowPreviousWindow();
                return;
            }
            
            if (_loadParameter != null) {
                // possible future parameters?
            }

            if (movieStartIndicator != null) movieStartIndicator.Visible = false;
            GUIPropertyManager.SetProperty("#MovingPictures.Importer.ListMode.Flag", "ALL");
            GUIPropertyManager.SetProperty("#MovingPictures.Importer.ListMode.Label", Translation.AllFiles);
            activeList = allFilesListControl;

            if (initialPageLoad) {
                MovingPicturesCore.Importer.Stop();
                MovingPicturesCore.Importer.Progress += new MovieImporter.ImportProgressHandler(ImporterProgress);
                MovingPicturesCore.Importer.MovieStatusChanged += new MovieImporter.MovieStatusChangedHandler(MovieStatusChangedListener);
                MovingPicturesCore.Importer.RestartScanner();
                initialPageLoad = false;

                allFilesListControl.Visible = true;
                pendingFilesListControl.Visible = false;
                completedFileListControl.Visible = false;
            }
            else {
                allFilesListControl.ListItems.AddRange(allItems);
                pendingFilesListControl.ListItems.AddRange(pendingItems);
                completedFileListControl.ListItems.AddRange(completedItems);
            }

            base.OnPageLoad();
        }

        protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType) {
            if ((control == allFilesListControl || control == pendingFilesListControl || control == completedFileListControl) && actionType == Action.ActionType.ACTION_SELECT_ITEM) {
                DisplayMatchesDialog();
                return;
            }

            switch (controlId) {
                case 19: // scan for new files
                    MovingPicturesCore.Importer.RestartScanner();
                    break;
                case 20: // restore ignored files
                    MovingPicturesCore.Importer.RestoreAllIgnoredFiles();
                    break;
            }
        }

        public override void OnAction(Action action) {
            base.OnAction(action);

            switch (action.wID) {
                case Action.ActionType.ACTION_MOVE_RIGHT:
                case Action.ActionType.ACTION_MOVE_LEFT:
                case Action.ActionType.ACTION_MOVE_UP:
                case Action.ActionType.ACTION_MOVE_DOWN:
                    int focusedControlId = GetFocusControlId();
                    if (focusedControlId == allFilesListControl.GetID) {
                        GUIPropertyManager.SetProperty("#MovingPictures.Importer.ListMode.Flag", "ALL");
                        GUIPropertyManager.SetProperty("#MovingPictures.Importer.ListMode.Label", Translation.AllFiles);
                        activeList = allFilesListControl;
                    }
                    if (focusedControlId == pendingFilesListControl.GetID) {
                        GUIPropertyManager.SetProperty("#MovingPictures.Importer.ListMode.Flag", "PENDING");
                        GUIPropertyManager.SetProperty("#MovingPictures.Importer.ListMode.Label", Translation.PendingFiles);
                        activeList = pendingFilesListControl;
                    }
                    if (focusedControlId == completedFileListControl.GetID) {
                        GUIPropertyManager.SetProperty("#MovingPictures.Importer.ListMode.Flag", "COMPLETED");
                        GUIPropertyManager.SetProperty("#MovingPictures.Importer.ListMode.Label", Translation.CompletedFiles);
                        activeList = completedFileListControl;
                    }
                    break;
            }
        }

        protected override void OnShowContextMenu() {
            base.OnShowContextMenu();
            showMainContext();
        }


        private void ImporterProgress(int percentDone, int taskCount, int taskTotal, string taskDescription) {
            lock (progressSyncToken) {
                if (GUIPropertyManager.GetProperty("#MovingPictures.Importer.Status") != taskDescription) {
                    GUIPropertyManager.SetProperty("#MovingPictures.Importer.Status", taskDescription);
                }

                GUIPropertyManager.SetProperty("#MovingPictures.Importer.CurrentTask.Count", taskCount.ToString());
                GUIPropertyManager.SetProperty("#MovingPictures.Importer.CurrentTask.Total", taskTotal.ToString());
                GUIPropertyManager.SetProperty("#MovingPictures.Importer.CurrentTask.Percentage", taskTotal.ToString());
                GUIPropertyManager.SetProperty("#MovingPictures.Importer.TotalProgressPercent", percentDone.ToString());

                GUIPropertyManager.SetProperty("#MovingPictures.Importer.IsActive", MovingPicturesCore.Importer.IsScanning.ToString());

                int unprocessed = MovingPicturesCore.Importer.PendingMatches.Count + MovingPicturesCore.Importer.PriorityPendingMatches.Count;
                int needInput = MovingPicturesCore.Importer.MatchesNeedingInput.Count;
                int approved = MovingPicturesCore.Importer.ApprovedMatches.Count + MovingPicturesCore.Importer.PriorityApprovedMatches.Count;
                int retrieving = MovingPicturesCore.Importer.RetrievingDetailsMatches.Count;
                int done = MovingPicturesCore.Importer.CommitedMatches.Count;

                GUIPropertyManager.SetProperty("#MovingPictures.Importer.Waiting.Count", unprocessed.ToString());
                GUIPropertyManager.SetProperty("#MovingPictures.Importer.NeedInput.Count", needInput.ToString());
                GUIPropertyManager.SetProperty("#MovingPictures.Importer.Processing.Count", (approved + retrieving).ToString());
                GUIPropertyManager.SetProperty("#MovingPictures.Importer.Done.Count", done.ToString());
            }
        }

        private void MovieStatusChangedListener(MovieMatch match, MovieImporterAction action) {
            lock (statusChangedSyncToken) {
                string icon = "";
                bool ignored = false;
                
                bool inAllList = true;
                bool inPendingList = false;
                bool inCompletedList = false;


                switch (action) {
                    // file is queued but not yet processed have no icon
                    case MovieImporterAction.ADDED:
                    case MovieImporterAction.ADDED_FROM_SPLIT:
                    case MovieImporterAction.ADDED_FROM_JOIN:
                        icon = "";
                        break;

                    // files that are currently being scanned are blue
                    case MovieImporterAction.PENDING:
                    case MovieImporterAction.GETTING_MATCHES:
                    case MovieImporterAction.APPROVED:
                    case MovieImporterAction.GETTING_DETAILS:
                        icon = "movingpictures_processing.png";
                        break;

                    // files that need help from the user are yellow
                    case MovieImporterAction.NEED_INPUT:
                        icon = "movingpictures_needinput.png";
                        inPendingList = true;
                        break;
                    
                    // files that have successfully imported are green
                    case MovieImporterAction.COMMITED:
                        icon = "movingpictures_done.png";
                        inCompletedList = true;
                        break;

                    case MovieImporterAction.IGNORED:
                        icon = "";
                        ignored = true;
                        inPendingList = true;
                        break;

                    // importer started or stopped. do nothing for now...
                    case MovieImporterAction.STARTED:
                    case MovieImporterAction.STOPPED:
                        allItems.Clear();
                        pendingItems.Clear();
                        completedItems.Clear();
                    
                        listItemLookup.Clear();
                        allFilesListControl.ListItems.Clear();
                        pendingFilesListControl.ListItems.Clear();
                        completedFileListControl.ListItems.Clear();
                        return;
                }

                // create or grab our list item
                GUIListItem listItem = null;
                if (listItemLookup.ContainsKey(match)) {
                    listItem = listItemLookup[match];
                }
                else {
                    listItem = new GUIListItem();
                    listItemLookup[match] = listItem;
                }

                if (inAllList && !allFilesListControl.ListItems.Contains(listItem)) {
                    allFilesListControl.ListItems.Add(listItem);
                    allItems.Add(listItem);
                }

                if (inPendingList && !pendingFilesListControl.ListItems.Contains(listItem)) {
                    pendingFilesListControl.ListItems.Add(listItem);
                    pendingItems.Add(listItem);
                }

                if (inCompletedList && !completedFileListControl.ListItems.Contains(listItem)) {
                    completedFileListControl.ListItems.Add(listItem);
                    completedItems.Add(listItem);
                }

                if (!inPendingList && inCompletedList) {
                    pendingItems.Remove(listItem);
                    pendingFilesListControl.ListItems.Remove(listItem);
                }
                
                if (!inCompletedList) completedItems.Remove(listItem);

                // populate it with most recent info
                listItem.Label = match.LocalMediaString;
                listItem.Label2 = (match.Selected != null ? match.Selected.DisplayMember : "");
                listItem.Label3 = action.ToString();
                listItem.PinImage = icon;
                listItem.IsPlayed = ignored;
                listItem.AlbumInfoTag = match;

            }
        }

        private bool VerifyImporterEnabled() {
            if (MovingPicturesCore.Settings.EnableImporterInGUI) return true;

            bool enable = MovingPicturesGUI.ShowCustomYesNo(Translation.ImporterDisabled, Translation.ImporterDisabledMessage, null, null, false, GetID);
            if (enable) MovingPicturesCore.Settings.EnableImporterInGUI = true;

            return enable;
        }

        private void showMainContext() {
            if (activeList == null) return;
            
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

        private void DisplayMatchesDialog() {
            MovieMatch selectedFile = activeList.SelectedListItem == null ? null : activeList.SelectedListItem.AlbumInfoTag as MovieMatch;

            // create our dialog
            GUIDialogMenu matchDialog = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            if (matchDialog == null) {
                logger.Error("Could not create matches dialog.");
                return;
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
            
            int ignoreId = ++maxid;
            matchDialog.Add(Translation.IgnoreMovie);

            int playId = ++maxid;
            matchDialog.Add(Translation.PlayMovie);

            // launch dialog and let user make choice
            matchDialog.DoModal(GUIWindowManager.ActiveWindow);

            // if the user canceled bail
            if (matchDialog.SelectedId == -1)
                return;

            // get new search criteria
            if (matchDialog.SelectedId == searchId) {
                bool searched = DisplaySearchDialog(selectedFile);
                if (!searched) DisplayMatchesDialog();
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
                MovingPicturesCore.Player.playFile(selectedFile.LocalMedia[0].FullPath);
                if (movieStartIndicator != null)
                    movieStartIndicator.Visible = false;
            }

            // user picked a movie, assign it
            selectedFile.Selected = selectedFile.PossibleMatches[matchDialog.SelectedId - 1];
            MovingPicturesCore.Importer.Approve(selectedFile);
        }

        private bool DisplaySearchDialog(MovieMatch selectedFile) {
            GUIDialogMenu dialog = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            if (dialog == null) {
                logger.Error("Could not create search dialog.");
                return false;
            }

            int maxid = 0;
            dialog.Reset();
            dialog.SetHeading("Search");
            
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

    }
}
