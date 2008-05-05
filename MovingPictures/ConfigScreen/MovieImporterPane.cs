using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Collections;

using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;
using MediaPortal.Plugins.MovingPictures.Properties;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;


namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class MovieImporterPane : UserControl {
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
            currentTaskDesc.Visible = true;
            countProgressLabel.Visible = true;

            // set the values of the progress bar and info labels
            progressBar.Value = percentDone;
            currentTaskDesc.Text = taskDescription;
            countProgressLabel.Text = "(" + taskCount + "/" + taskTotal + ")";

            // if finished hide the progress info components
            if (percentDone == 100) {
                currentTaskDesc.Visible = false;
                countProgressLabel.Visible = false;
                progressBar.Value = 0;
            }
        }

        private void movieStatusChangedListener(MediaMatch obj, MovieImporterAction action) {
            // This ensures we are thread safe. Makes sure this method is run by
            // the thread that created this panel.
            if (InvokeRequired) {
                Delegate method = new MovieImporter.MovieStatusChangedHandler(movieStatusChangedListener);
                object[] parameters = new object[] { obj, action };
                this.Invoke(method, parameters);
                return;
            }

            if (action == MovieImporterAction.SPLIT ||
                action == MovieImporterAction.JOINED) {

                lastSplitJoinLocation = unapprovedMatchesBindingSource.IndexOf(obj);
                unapprovedMatchesBindingSource.Remove(obj);
                clearSelection = true;
                return;
            }


            // add the match if neccisary and grab the row number
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
                case MovieImporterAction.REPROCCESSING_PENDING:
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
                    if (obj.Selected.Movie.Cover != null)
                        pictureBox1.Image = obj.Selected.Movie.CoverThumb;
                    break;
                case MovieImporterAction.IGNORED:
                    imageCell.Value = Resources.ignored;
                    break;
            }

            updateButtons();
        }

        private void scanButton_Click(object sender, EventArgs e) {
            MovingPicturesPlugin.Importer.Start();
            MovingPicturesPlugin.Importer.ForceFullScan();
        }

        private void MovieImporterPane_Load(object sender, EventArgs e) {
            if (!DesignMode) {
                MovingPicturesPlugin.Importer.Progress += new MovieImporter.ImportProgressHandler(progressListener);
                MovingPicturesPlugin.Importer.MovieStatusChanged += new MovieImporter.MovieStatusChangedHandler(movieStatusChangedListener);

                // fixes a bug in DataGridView. switching to tab it is on forces
                // it to initialize, so adding stuff to the binding source works properly
                tabControl.SelectedIndex = 1;
                tabControl.SelectedIndex = 0;

                MovingPicturesPlugin.Importer.Start();
                
                updateButtons();
            }

            this.unapprovedGrid.AutoGenerateColumns = false;
            this.unapprovedGrid.DataSource = this.unapprovedMatchesBindingSource;

            this.unapprovedPossibleMatchesColumn.DisplayMember = "DisplayMember";
            this.unapprovedPossibleMatchesColumn.ValueMember = "ValueMember";
        }


        // If this pane has been destroyed, that means the Config screen has shut down, so 
        // the importer also should be  stopped.
        void MovieImporterPane_HandleDestroyed(object sender, System.EventArgs e) {
            MovingPicturesPlugin.Importer.Stop();
        }

        // Handles when the user modifies data in the unapproved matches list
        private void unapprovedMatchesBindingSource_ListChanged(object sender, ListChangedEventArgs e) {
            if (e.ListChangedType == ListChangedType.ItemChanged) {
                MediaMatch match = (MediaMatch)unapprovedGrid.Rows[e.NewIndex].DataBoundItem;
                MovingPicturesPlugin.Importer.Approve(match);
            }
        }

        private void approveButton_Click(object sender, EventArgs e) {
            unapprovedGrid.EndEdit();

            foreach (DataGridViewRow currRow in unapprovedGrid.SelectedRows) {
                MediaMatch selectedMatch = (MediaMatch)currRow.DataBoundItem;
                MovingPicturesPlugin.Importer.Approve(selectedMatch);
            }
        }

        private void ignoreButton_Click(object sender, EventArgs e) {
            unapprovedGrid.EndEdit();

            foreach (DataGridViewRow currRow in unapprovedGrid.SelectedRows) {
                MediaMatch selectedMatch = (MediaMatch)currRow.DataBoundItem;
                MovingPicturesPlugin.Importer.Ignore(selectedMatch);
            }
        }

        private void rescanButton_Click(object sender, EventArgs e) {
            unapprovedGrid.EndEdit();

            foreach (DataGridViewRow currRow in unapprovedGrid.SelectedRows) {
                MediaMatch selectedMatch = (MediaMatch)currRow.DataBoundItem;

                SearchStringPopup popup = new SearchStringPopup(selectedMatch);
                popup.ShowDialog(this);

                if (popup.DialogResult == DialogResult.OK) {
                    string searchStr = popup.GetSearchString();
                    MovingPicturesPlugin.Importer.Reprocess(selectedMatch, searchStr);
                }
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
                MediaMatch selectedMatch = (MediaMatch)currRow.DataBoundItem;

                if (selectedMatch == null)
                    break;

                validMatchSelected = true;

                // check if this row is either approved or commited
                if (!approveButtonEnabled &&
                    !MovingPicturesPlugin.Importer.ApprovedMatches.Contains(selectedMatch) &&
                    !MovingPicturesPlugin.Importer.RetrievingDetailsMatches.Contains(selectedMatch) &&
                    !MovingPicturesPlugin.Importer.CommitedMatches.Contains(selectedMatch) &&
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

            // check if we have multiple rows selected to join
            if (unapprovedGrid.SelectedRows.Count > 1) {
                splitJoinButton.Image = Resources.arrow_join;
                splitJoinButton.ToolTipText = "Join Selected Files";
                splitJoinButton.Enabled = true;
                splitMode = false;
            }

            // check if we have one row with multipel files we can split
            else if (unapprovedGrid.SelectedRows.Count == 1 && unapprovedGrid.SelectedRows[0] != null) {
                MediaMatch match = (MediaMatch)unapprovedGrid.SelectedRows[0].DataBoundItem;
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

            /*
            // set the display icon for starting or stopping the importer
            if (MovingPicturesPlugin.Importer.IsScanning()) {
                startStopButton.Image = Resources.noatunstop;
                startStopButton.ToolTipText = "Stop the Media Importer";

                filterSplitButton.Enabled = true;
            } else {
                startStopButton.Image = Resources.noatunplay;
                startStopButton.ToolTipText = "Start the Media Importer";

                splitJoinButton.Enabled = false;
                approveButton.Enabled = false;
                rescanButton.Enabled = false;
                ignoreButton.Enabled = false;
                filterSplitButton.Enabled = false;
            }
            */
        }

        private void splitJoinButton_Click(object sender, EventArgs e) {
            if (splitMode) {
                MediaMatch match = (MediaMatch)unapprovedGrid.SelectedRows[0].DataBoundItem;
                MovingPicturesPlugin.Importer.Split(match);
            } else {
                List<MediaMatch> mediaList = new List<MediaMatch>();
                foreach (DataGridViewRow currRow in unapprovedGrid.SelectedRows) 
                    mediaList.Add((MediaMatch)currRow.DataBoundItem);

                MovingPicturesPlugin.Importer.Join(mediaList);
            }
            
        }
        /*
        private void startStopButton_Click(object sender, EventArgs e) {
            if (MovingPicturesPlugin.Importer.IsScanning())
                MovingPicturesPlugin.Importer.Stop();
            else
                MovingPicturesPlugin.Importer.Start();

            updateButtons();
        }
        */
    }
}
