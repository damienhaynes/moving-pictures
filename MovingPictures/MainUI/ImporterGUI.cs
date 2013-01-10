using System;
using System.Collections.Generic;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using Action = MediaPortal.GUI.Library.Action;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.MainUI {
    public class ImporterGUI : GUIWindow {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [SkinControlAttribute(310)]
        private GUIListControl fileListControl = null;

        private bool initialPageLoad = true;
        private object statusChangedSyncToken = new object();
        private object progressSyncToken = new object();
        private Dictionary<MovieMatch, GUIListItem> listItemLookup = new Dictionary<MovieMatch, GUIListItem>();

        public override int GetID {
            get { return 96743; }
        }

        public override bool Init() {
            logger.Debug("Initializing Importer Screen");
            return Load(GUIGraphicsContext.Skin + @"\movingpictures.importer.xml");
        }

        protected override void OnPageLoad() {
            logger.Debug("Launching Importer Screen");

            bool enabled = VerifyImporterEnabled();
            if (!enabled) {
                GUIWindowManager.ShowPreviousWindow();
                return;
            }
            
            if (_loadParameter != null) {
                // possible future parameters?
            }

            if (initialPageLoad) {
                MovingPicturesCore.Importer.Stop();
                MovingPicturesCore.Importer.Progress += new MovieImporter.ImportProgressHandler(ImporterProgress);
                MovingPicturesCore.Importer.MovieStatusChanged += new MovieImporter.MovieStatusChangedHandler(MovieStatusChangedListener);
                MovingPicturesCore.Importer.RestartScanner();
                initialPageLoad = false;
            }
            else {
                foreach (GUIListItem currItem in listItemLookup.Values)
                    fileListControl.ListItems.Add(currItem);
            }

            base.OnPageLoad();
        }

        protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType) {
            if (control == fileListControl && actionType == Action.ActionType.ACTION_SELECT_ITEM) {
                DisplayMatchesDialog();
            }
            base.OnClicked(controlId, control, actionType);
        }

        private void ImporterProgress(int percentDone, int taskCount, int taskTotal, string taskDescription) {
            lock (progressSyncToken) {
                if (GUIPropertyManager.GetProperty("#MovingPictures.Importer.Status") != taskDescription) {
                    logger.Debug("IMPORTER TASK: " + taskDescription);
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
                        icon = "led_blue.png";
                        break;

                    // files that need help from the user are yellow
                    case MovieImporterAction.NEED_INPUT:
                        icon = "led_yellow.png";
                        break;
                    
                    // files that have successfully imported are green
                    case MovieImporterAction.COMMITED:
                        icon = "led_green.png";
                        break;
                    
                    // importer started or stopped. do nothing for now...
                    case MovieImporterAction.STARTED:
                    case MovieImporterAction.STOPPED:
                        //listItemLookup.Clear();
                        //fileListControl.ListItems.Clear();
                        return;
                }

                // create or grab our list item
                GUIListItem listItem = null;
                if (listItemLookup.ContainsKey(match)) {
                    listItem = listItemLookup[match];
                }
                else {
                    listItem = new GUIListItem();
                    fileListControl.ListItems.Add(listItem);
                    listItemLookup[match] = listItem;
                }

                // populate it with most recent info
                listItem.Label = match.LocalMediaString;
                listItem.Label2 = (match.Selected != null ? match.Selected.DisplayMember : "");
                listItem.Label3 = action.ToString();
                listItem.PinImage = icon;
                listItem.AlbumInfoTag = match;

            }
        }

        private bool VerifyImporterEnabled() {
            if (MovingPicturesCore.Settings.EnableImporterInGUI) return true;

            bool enable = MovingPicturesGUI.ShowCustomYesNo("Importer Disabled", "The importer has been disabled in the\n" +
                                                                                 "MediaPortal GUI. Would you like to\n" +
                                                                                 "reenable it?", "Yes", "No", false, GetID);

            if (enable) MovingPicturesCore.Settings.EnableImporterInGUI = true;

            return enable;
        }

        private void DisplayMatchesDialog() {
            MovieMatch selectedFile = fileListControl.SelectedListItem.AlbumInfoTag as MovieMatch;

            // create our dialog
            GUIDialogMenu matchDialog = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            if (matchDialog == null) {
                logger.Error("Could not create matches dialog.");
                return;
            }

            int maxid = 0; 
            matchDialog.Reset();
            matchDialog.SetHeading("Possible Matches");

            // add our possible matches to it (list of movies)
            foreach (var match in selectedFile.PossibleMatches) {
                matchDialog.Add(match.DisplayMember);
                maxid++;
            }

            int searchId = ++maxid;
            matchDialog.Add("Search for More...");
            
            int ignoreId = ++maxid;
            matchDialog.Add("Ignore File");

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
            dialog.Add(string.Format("Title: {0}", selectedFile.Signature.Title));

            int yearId = ++maxid;
            dialog.Add(string.Format("Year: {0}", selectedFile.Signature.Year));

            int imdbId = ++maxid;
            dialog.Add(string.Format("IMDb Id: {0}", selectedFile.Signature.ImdbId));

            dialog.DoModal(GUIWindowManager.ActiveWindow);

            // user picked nothing, go back to previous dialog
            if (dialog.SelectedId == -1) {
                return false;
            }

            // build and display our virtual keyboard
            VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
            keyboard.Reset();
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
