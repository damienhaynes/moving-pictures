using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Plugins.MovingPictures.SignatureBuilders;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.Properties;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;
using NLog;
using System.Diagnostics;
using Cornerstone.GUI.Dialogs;


namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class MovieImporterPane : UserControl {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Bitmap blank;
        private bool splitMode;
        private int lastSplitJoinLocation;
        private bool clearSelection;

        #region Database Managed Settings
        
        #endregion

        public MovieImporterPane() {
            InitializeComponent();

            blank = new Bitmap(1, 1);
            splitMode = true;
            clearSelection = false;

            // if we are in designer, break to prevent errors with rendering, it cant access the DB...
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;
        }

        private void progressListener(int percentDone, int taskCount, int taskTotal, string taskDescription) {
            // This ensures we are thread safe. Makes sure this method is run by
            // the thread that created this panel.
            if (InvokeRequired) {
                Invoke(new MovieImporter.ImportProgressHandler(progressListener), new object[] { percentDone, taskCount, taskTotal, taskDescription });
                return;
            }
            
            // set visibility of progressbar and info labels
            progressPanel.Visible = true;
            currentTaskDesc.Visible = true;
            countProgressLabel.Visible = true;

            // set the values of the progress bar and info labels
            progressBar.Value = percentDone;
            currentTaskDesc.Text = taskDescription;
            countProgressLabel.Text = "(" + taskCount + "/" + taskTotal + ")";

            // if finished hide the progress info components
            if (percentDone == 100) {
                progressPanel.Visible = false;

                currentTaskDesc.Visible = false;
                countProgressLabel.Visible = false;
                progressBar.Value = 0;
            }
        }

        private void movieStatusChangedListener(MovieMatch obj, MovieImporterAction action) {
            // This ensures we are thread safe. Makes sure this method is run by
            // the thread that created this panel.
            if (InvokeRequired) {
                Delegate method = new MovieImporter.MovieStatusChangedHandler(movieStatusChangedListener);
                object[] parameters = new object[] { obj, action };
                this.Invoke(method, parameters);
                return;
            }

            if (action == MovieImporterAction.STARTED)
                return;

            if (action == MovieImporterAction.STOPPED) {
                unapprovedMatchesBindingSource.Clear();
                return;
            }

            if (action == MovieImporterAction.REMOVED_FROM_SPLIT ||
                action == MovieImporterAction.REMOVED_FROM_JOIN) {

                lastSplitJoinLocation = unapprovedMatchesBindingSource.IndexOf(obj);
                unapprovedMatchesBindingSource.Remove(obj);
                clearSelection = true;
                return;
            }


            // add the match if necessary and grab the row number
            int rowNum;
            if (action == MovieImporterAction.ADDED)
                rowNum = unapprovedMatchesBindingSource.Add(obj);
            else if (action == MovieImporterAction.ADDED_FROM_SPLIT ||
                     action == MovieImporterAction.ADDED_FROM_JOIN) {

                unapprovedMatchesBindingSource.Insert(lastSplitJoinLocation, obj);
                if (clearSelection) {
                    unapprovedGrid.ClearSelection();
                    clearSelection = false;
                }
                unapprovedGrid.Rows[lastSplitJoinLocation].Selected = true;

                rowNum = lastSplitJoinLocation;
                lastSplitJoinLocation++;
            } else 
                rowNum = unapprovedMatchesBindingSource.IndexOf(obj);


            // setup tooltip for filename
            DataGridViewTextBoxCell filenameCell = (DataGridViewTextBoxCell)unapprovedGrid.Rows[rowNum].Cells["unapprovedLocalMediaColumn"];
            filenameCell.ToolTipText = obj.LongLocalMediaString;

            // setup the combo box of possible matches
            DataGridViewComboBoxCell movieListCombo = (DataGridViewComboBoxCell)unapprovedGrid.Rows[rowNum].Cells["unapprovedPossibleMatchesColumn"];
            movieListCombo.Items.Clear();
            foreach (PossibleMatch currMatch in obj.PossibleMatches)
                movieListCombo.Items.Add(currMatch);
             
            // set the status icon
            DataGridViewImageCell imageCell = (DataGridViewImageCell)unapprovedGrid.Rows[rowNum].Cells["statusColumn"];
            switch (action) {
                case MovieImporterAction.ADDED:
                case MovieImporterAction.ADDED_FROM_SPLIT:
                case MovieImporterAction.ADDED_FROM_JOIN:
                    imageCell.Value = blank;
                    break;
                case MovieImporterAction.PENDING:
                    imageCell.Value = Resources.arrow_rotate_clockwise;
                    break;
                case MovieImporterAction.GETTING_MATCHES:
                    imageCell.Value = Resources.arrow_down1;
                    break;
                case MovieImporterAction.NEED_INPUT:
                    imageCell.Value = Resources.information;
                    break;
                case MovieImporterAction.APPROVED:
                    imageCell.Value = Resources.approved;
                    break;
                case MovieImporterAction.GETTING_DETAILS:
                    imageCell.Value = Resources.approved;
                    break;
                case MovieImporterAction.COMMITED:
                    imageCell.Value = Resources.accept;
                    break;
                case MovieImporterAction.IGNORED:
                    imageCell.Value = Resources.ignored;
                    break;
                case MovieImporterAction.MANUAL:
                    imageCell.Value = Resources.accept; // @TODO change icon
                    break;
            }

            updateButtons();
        }

        private void scanButton_Click(object sender, EventArgs e) {
            MovingPicturesCore.Importer.Start();
            MovingPicturesCore.Importer.RestartScanner();
        }

        private void MovieImporterPane_Load(object sender, EventArgs e) {
            if (!DesignMode) {
                MovingPicturesCore.Importer.Progress += new MovieImporter.ImportProgressHandler(progressListener);
                MovingPicturesCore.Importer.MovieStatusChanged += new MovieImporter.MovieStatusChangedHandler(movieStatusChangedListener);

                // the importer needs to be displayed on load, however briefly, to get things 
                // initialized. It is currently the first tab so this is not needed right now.
                // tabControl.SelectedIndex = 1;
                // tabControl.SelectedIndex = 0;

                MovingPicturesCore.Importer.Start();
                
                updateButtons();

                automaticMediaInfoMenuItem.Checked = MovingPicturesCore.Settings.AutoRetrieveMediaInfo;
            }

            this.unapprovedGrid.AutoGenerateColumns = false;
            this.unapprovedGrid.DataSource = this.unapprovedMatchesBindingSource;

            this.unapprovedPossibleMatchesColumn.DisplayMember = "DisplayMember";
            this.unapprovedPossibleMatchesColumn.ValueMember = "ValueMember";
        }


        // If this pane has been destroyed, that means the Config screen has shut down, so 
        // the importer also should be  stopped.
        void MovieImporterPane_HandleDestroyed(object sender, System.EventArgs e) {
            if (!DesignMode)
                MovingPicturesCore.Importer.Stop();
        }

        // Handles when the user modifies data in the unapproved matches list
        private void unapprovedMatchesBindingSource_ListChanged(object sender, ListChangedEventArgs e) {
            if (e.ListChangedType == ListChangedType.ItemChanged) {
                MovieMatch match = (MovieMatch)unapprovedGrid.Rows[e.NewIndex].DataBoundItem;
                MovingPicturesCore.Importer.Approve(match);
            }
        }

        private void approveButton_Click(object sender, EventArgs e) {
            unapprovedGrid.EndEdit();

            foreach (DataGridViewRow currRow in unapprovedGrid.SelectedRows) {
                MovieMatch selectedMatch = (MovieMatch)currRow.DataBoundItem;
                selectedMatch.HighPriority = true;
                MovingPicturesCore.Importer.Approve(selectedMatch);
            }
        }

        private void ignoreButton_Click(object sender, EventArgs e) {
            unapprovedGrid.EndEdit();

            DialogResult result = MessageBox.Show("This will permanently ignore the selected file(s). This action is currently IRREVERSABLE on a file by file basis, are you sure?", "Warning!", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes) {

                foreach (DataGridViewRow currRow in unapprovedGrid.SelectedRows) {
                    MovieMatch selectedMatch = (MovieMatch)currRow.DataBoundItem;
                    MovingPicturesCore.Importer.Ignore(selectedMatch);
                }
            }
        }

        private void rescanButton_Click(object sender, EventArgs e) {
            unapprovedGrid.EndEdit();

            foreach (DataGridViewRow currRow in unapprovedGrid.SelectedRows) {
                MovieMatch selectedMatch = (MovieMatch)currRow.DataBoundItem;

                // Check if all files belonging to the movie are available.
                bool continueRescan = false;
                while (!continueRescan) {
                    continueRescan = true;
                    foreach (DBLocalMedia localMedia in selectedMatch.LocalMedia) {
                        // if the file is offline
                        if (!localMedia.IsAvailable) {
                            // do not continue
                            continueRescan = false;

                            // Prompt the user to insert the media containing the files
                            string connect = string.Empty;
                            if (localMedia.DriveLetter != null) {
                                if (localMedia.ImportPath.GetDriveType() == DriveType.CDRom)
                                    connect = "Please insert the disc labeled '" + localMedia.MediaLabel + "'.";
                                else
                                    connect = "Please reconnect the media labeled '" + localMedia.MediaLabel + "' to " + localMedia.DriveLetter;
                            }
                            else {
                                connect = "Please make sure the network share '" + localMedia.FullPath + "' is available.";
                            }

                            // Show dialog
                            DialogResult resultInsert = MessageBox.Show(
                            "The file or files you want to rescan are currently not available.\n\n" + connect,
                            "File(s) not available.", MessageBoxButtons.RetryCancel);

                            // if cancel is pressed stop the rescan process.
                            if (resultInsert == DialogResult.Cancel)
                                return;

                            // break foreach loop (and recheck condition)
                            break;
                        }
                    }
                }

                SearchStringPopup popup = new SearchStringPopup(selectedMatch);
                popup.ShowDialog(this);

                // reprocess
                if (popup.DialogResult == DialogResult.OK)              
                    MovingPicturesCore.Importer.Reprocess(selectedMatch);
            }
        }

        private void unapprovedGrid_SelectionChanged(object sender, EventArgs e) {
            updateButtons();
        }

        // updates enabled/disabled status of buttons on media importer based on 
        // the rows selected in the list
        private void updateButtons() {
            bool approveButtonEnabled = false;
            bool validMatchSelected = false;
            bool ignoreButtonEnabled = false;

            foreach (DataGridViewRow currRow in unapprovedGrid.SelectedRows) {
                MovieMatch selectedMatch = (MovieMatch)currRow.DataBoundItem;

                if (selectedMatch == null)
                    break;

                validMatchSelected = true;

                // check if this row is either approved or commited
                if (!approveButtonEnabled &&
                    !MovingPicturesCore.Importer.ApprovedMatches.Contains(selectedMatch) &&
                    !MovingPicturesCore.Importer.RetrievingDetailsMatches.Contains(selectedMatch) &&
                    !MovingPicturesCore.Importer.CommitedMatches.Contains(selectedMatch) &&
                    selectedMatch.Selected != null) {

                    approveButtonEnabled = true;
                }

                if (!selectedMatch.LocalMedia[0].Ignored)
                    ignoreButtonEnabled = true;
            }

            // if the selection has changed, enable the approve button
            approveButton.Enabled = approveButtonEnabled;
            rescanButton.Enabled = validMatchSelected;
            ignoreButton.Enabled = ignoreButtonEnabled;

            if (unapprovedGrid.SelectedRows.Count > 0)
                manualAssignButton.Enabled = true;
            else
                manualAssignButton.Enabled = false;

            // check if we have multiple rows selected to join
            if (unapprovedGrid.SelectedRows.Count > 1) {
                splitJoinButton.Image = Resources.arrow_join;
                splitJoinButton.ToolTipText = "Join Selected Files";
                splitJoinButton.Enabled = true;
                splitMode = false;
            }

            // check if we have one row with multiple files we can split
            else if (unapprovedGrid.SelectedRows.Count == 1 && unapprovedGrid.SelectedRows[0] != null) {
                MovieMatch match = (MovieMatch)unapprovedGrid.SelectedRows[0].DataBoundItem;
                if (match.LocalMedia.Count > 1) {
                    splitJoinButton.Image = Resources.arrow_divide;
                    splitJoinButton.ToolTipText = "Split Selected File Group";
                    splitJoinButton.Enabled = true;
                    splitMode = true;
                } else 
                    splitJoinButton.Enabled = false;
            }

            // split join button cant be used now, so disable it.
            else splitJoinButton.Enabled = false;
        }

        private void splitJoinButton_Click(object sender, EventArgs e) {
            if (splitMode) {
                MovieMatch match = (MovieMatch)unapprovedGrid.SelectedRows[0].DataBoundItem;
                MovingPicturesCore.Importer.Split(match);
            } else {
                List<MovieMatch> mediaList = new List<MovieMatch>();
                foreach (DataGridViewRow currRow in unapprovedGrid.SelectedRows) 
                    mediaList.Add((MovieMatch)currRow.DataBoundItem);

                MovingPicturesCore.Importer.Join(mediaList);
            }
            
        }

        private void unapprovedGrid_DataError(object sender, DataGridViewDataErrorEventArgs e) {
            logger.WarnException("Error from Importer DataGrid.", e.Exception);
        }

        private void unignoreAllFilesToolStripMenuItem_Click(object sender, EventArgs e) {
            DialogResult result =  MessageBox.Show(
                "This will unignore ALL previously ignored files, and restart\n" +
                "the Importer. This means all uncommitted matches from this\n" +
                "import session will have to be reapproved. Do you want to\n" +
                "continue?\n", "Warning", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes) {
                ProgressPopup popup = new ProgressPopup(new WorkerDelegate(unignoreAllFiles));
                popup.Owner = this.ParentForm;
                popup.Show();
            }
           
        }

        private void unignoreAllFiles() {
            foreach (DBLocalMedia currFile in DBLocalMedia.GetAll())
                if (currFile.Ignored)
                    currFile.Delete();

            MovingPicturesCore.Importer.RestartScanner();
        }

        private void restartImporterToolStripMenuItem_Click(object sender, EventArgs e) {
            DialogResult result = MessageBox.Show(
                "You are about to restart the Movie Importer. This means that\n" +
                "all uncommitted matches from this import session will have to\n" +
                "be reapproved. Do you want to continue?\n", "Warning", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes) 
                MovingPicturesCore.Importer.RestartScanner();

        }

        private void settingsButton_ButtonClick(object sender, EventArgs e) {
            settingsButton.ShowDropDown();
        }

        private void manualAssignButton_Click(object sender, EventArgs e) {
            unapprovedGrid.EndEdit();

            foreach (DataGridViewRow currRow in unapprovedGrid.SelectedRows) {
                MovieMatch selectedMatch = (MovieMatch)currRow.DataBoundItem;
                ManualAssignPopup popup = new ManualAssignPopup(selectedMatch);
                popup.ShowDialog(this);

                if (popup.DialogResult == DialogResult.OK) {

                    // create a movie with the user supplied information
                    DBMovieInfo movie = new DBMovieInfo();
                    movie.Title = popup.Title;
                    movie.Year = popup.Year.GetValueOrDefault(0);

                    // update the match
                    PossibleMatch selectedMovie = new PossibleMatch();
                    selectedMovie.Movie = movie;

                    MatchResult result = new MatchResult();
                    result.TitleScore = 0;
                    result.YearScore = 0;
                    result.ImdbMatch = true;

                    selectedMovie.Result = result;
                    
                    selectedMatch.PossibleMatches.Add(selectedMovie);
                    selectedMatch.Selected = selectedMovie;

             
                    // Manually Assign Movie
                    MovingPicturesCore.Importer.ManualAssign(selectedMatch);
                }
            }
        }

        private void unapprovedGrid_DataError_1(object sender, DataGridViewDataErrorEventArgs e) {
            logger.Warn("Importer removed possible matches without telling the importer!");
            DataGridViewComboBoxCell movieListCombo = (DataGridViewComboBoxCell)unapprovedGrid.Rows[e.RowIndex].Cells["unapprovedPossibleMatchesColumn"];
            movieListCombo.Items.Clear();
        }

        private void helpButton_Click(object sender, EventArgs e) {
            ProcessStartInfo processInfo = new ProcessStartInfo(Resources.MovieImporterHelpURL);
            Process.Start(processInfo);
        }

        private void automaticMediaInfoMenuItem_Click(object sender, EventArgs e) {
            automaticMediaInfoMenuItem.Checked = !automaticMediaInfoMenuItem.Checked;
            MovingPicturesCore.Settings.AutoRetrieveMediaInfo = automaticMediaInfoMenuItem.Checked;
        }

    }
}
