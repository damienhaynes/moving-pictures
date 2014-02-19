using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using Cornerstone.GUI.Dialogs;
using System.Collections;
using System.Windows.Forms;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    public partial class IgnoredFilesManager : Form {

        public List<DBLocalMedia> IgnoredFiles {
            get { return _ignoredFiles; }
        }
        private List<DBLocalMedia> _ignoredFiles;

        public IgnoredFilesManager() {
            InitializeComponent();

            populateGrid();
        }

        //populate datagrid with any ignored files
        private void populateGrid() {
            //setup List and populate with ignored movies
            _ignoredFiles = getIgnoredFiles();

            // add ignored files to the datagridview
            if (ignoredMovieGrid.Rows.Count > 0) ignoredMovieGrid.Rows.Clear();
            foreach (DBLocalMedia hidMov in _ignoredFiles) {
                ignoredMovieGrid.Rows.Add(hidMov.File.Name);
                ignoredMovieGrid.Rows[ignoredMovieGrid.RowCount - 1].Cells[0].ToolTipText = hidMov.File.FullName;
            }
        }

        // Gets and resturns a List<> of all ignored files
        private  List<DBLocalMedia> getIgnoredFiles() {
            List<DBLocalMedia> hidFil = new List<DBLocalMedia>();
            foreach (DBLocalMedia currFile in DBLocalMedia.GetAll()) {
                if (currFile.Ignored) {
                    hidFil.Add(currFile);
                }
            }
            return hidFil;
        }

        //process selected files to unignore
        private void unignoreSelectedFiles() {
            //setup array to store movie list and number of rows variable
            ArrayList selMovie = new ArrayList();
            int rowSel = ignoredMovieGrid.SelectedRows.Count;
            //process each selected cell and copy the movie name to the arraylist
            foreach (DataGridViewCell cellSel in ignoredMovieGrid.SelectedCells) {
                if (cellSel.ToolTipText.Length > 0) {
                    selMovie.Add(cellSel.ToolTipText);
                }
            }
            //look for all ignored DBLocalMedia and see if it was in the array build from the selectedCell list above
            foreach (DBLocalMedia currFile in DBLocalMedia.GetAll()) {
                if (currFile.Ignored && selMovie.Contains(currFile.File.FullName)) {
                    currFile.Delete();
                }
            }

            MovingPicturesCore.Importer.RestartScanner();
        }

        //remove selected movies from the ignore list.
        private void unignoreSelectedButton_Click(object sender, EventArgs e) {
            //prevent extra clicks by disabling buttons            
            unignoreSelectedButton.Enabled = false;

            DialogResult result = MessageBox.Show(this,
                "This will remove the selected files from the ignored list, and restart\n" +
                "the Importer. This means all uncommitted matches from this\n" +
                "import session will have to be reapproved. Do you want to\n" +
                "continue?\n", "Warning", MessageBoxButtons.YesNo);

            //only process if there is at least one row selected
            if (result == DialogResult.Yes && ignoredMovieGrid.SelectedRows.Count > 0) {
                //process selected files
                ProgressPopup popup = new ProgressPopup(new WorkerDelegate(unignoreSelectedFiles));
                popup.Owner = this.ParentForm;
                popup.Show();
                this.DialogResult = DialogResult.OK;
            }
            else {
                if (ignoredMovieGrid.SelectedRows.Count == 0) MessageBox.Show(this, "No movies were selected.");
                //enable the unignore buttons in case user wants to unignore again                
                unignoreSelectedButton.Enabled = true;
            }
        }
    }
}
