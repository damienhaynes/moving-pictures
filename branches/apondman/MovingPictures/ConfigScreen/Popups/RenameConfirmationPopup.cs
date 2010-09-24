using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.Database;
using System.IO;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using NLog;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieRenamer;
using Cornerstone.GUI.Dialogs;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    public partial class RenameConfirmationPopup : Form {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private delegate void InvokeDelegate();

        private const string warningDisk = "Files from a CD or DVD can not be renamed.";
        private const string warningOffline = "Media files are currently offline.";
        private const string confirmAllText = "Do this for the next {0} movies.";

        private MovieRenamer movieRenamer;
        private IEnumerator<DBMovieInfo> movieEnumerator;
        private IList<DBMovieInfo> movies;
        private List<Renamable> itemsToRename;
        private Dictionary<Renamable, ListViewItem> listItemLookup;
        private int remaining;
        private bool loading = false;
        private bool renamingAll = false;

        public RenameConfirmationPopup(IList<DBMovieInfo> movies) {
            InitializeComponent();

            movieRenamer = new MovieRenamer();
            
            // if we are only processing one movie change skip button to "cancel"
            if (movies.Count == 1)
                skipButton.Text = "Cancel";

            // initialize the rename settings menu items
            renameFoldersMenuItem.Checked = MovingPicturesCore.Settings.RenameFolders;
            renameFilesMenuItem.Checked = MovingPicturesCore.Settings.RenameFiles;
            renameSecondaryFilesMenuItem.Checked = MovingPicturesCore.Settings.RenameSecondaryFiles;
            UpdateConfigControls();

            // initialize the movie enumerator
            this.movies = movies;
            remaining = movies.Count;
            movieEnumerator = movies.GetEnumerator();
        }

        // Moves to the next movie in the list and updates the gui. If no movies remain
        // closes the window.
        private bool LoadNextMovie() {
            remaining--;
            if (!movieEnumerator.MoveNext()) {
                if (this.InvokeRequired) Invoke(new InvokeDelegate(Close));
                else Close();

                return false;
            }

            RebuildRenamableList();
            if (!renamingAll) {
                UpdateStandardControls();
                PopulateRenameList();
            }

            return true;
        }

        // Updates various screen controls based on status of the current movie.
        private void UpdateStandardControls() {
            bool available = movieEnumerator.Current.LocalMedia.IsAvailable();
            bool isDisk = movieEnumerator.Current.LocalMedia[0].ImportPath.IsOpticalDrive;

            renameButton.Enabled = available && !isDisk;
            warningPanel.Visible = !available || isDisk;
            retryLinkLabel.Visible = !isDisk;

            warningLabel.Text = isDisk ? warningDisk : warningOffline;
            movieTitleLabel.Text = movieEnumerator.Current.Title;

            confirmAllCheckBox.Text = String.Format(confirmAllText, remaining);
            confirmAllCheckBox.Visible = remaining > 1;

            // if all renamable items are unchecked, disable the rename button            
            bool allUnchecked = true;
            foreach (Renamable currItem in itemsToRename) {
                if (currItem.Approved) {
                    allUnchecked = false;
                    break;
                }
            }
            if (allUnchecked) renameButton.Enabled = false;
        }

        // Updates the menu items relating to Moving Pictures Settings.
        private void UpdateConfigControls() {
            renameFilesMenuItem.Enabled = renameFoldersMenuItem.Checked || renameSecondaryFilesMenuItem.Checked;
            renameFoldersMenuItem.Enabled = renameFilesMenuItem.Checked || renameSecondaryFilesMenuItem.Checked;
            renameSecondaryFilesMenuItem.Enabled = renameFilesMenuItem.Checked || renameFoldersMenuItem.Checked;
        }

        private void RebuildRenamableList() {
            DBMovieInfo currMovie = movieEnumerator.Current;
            itemsToRename = movieRenamer.GetRenameActionList(currMovie);
        }

        // Populates the ListView with the files to be renamed from the current movie.
        private void PopulateRenameList() {
            loading = true;

            fileListView.BeginUpdate();
            fileListView.Items.Clear();

            // grab information to be displayed
            listItemLookup = new Dictionary<Renamable, ListViewItem>();
            DBMovieInfo currMovie = movieEnumerator.Current;
            string importPathPrefix = currMovie.LocalMedia[0].ImportPath.FullPath;            

            // populate directories to be renamed in the list
            foreach (Renamable currItem in itemsToRename) {
                RenamableDirectory currDir = currItem as RenamableDirectory;
                if (currDir == null) continue;

                ListViewItem listItem = new ListViewItem(currItem.OriginalName);
                listItem.Tag = currItem;
                listItem.Checked = currItem.Approved;
                listItem.SubItems.Add(currItem.FinalNewName);
                fileListView.Items.Add(listItem);
                listItemLookup[currItem] = listItem;
            }
            
            // populate files to be renamed in the list
            foreach(Renamable currItem in itemsToRename) {
                RenamableFile currFile = currItem as RenamableFile;
                if (currFile == null) continue;

                string originalName = currFile.OriginalName.Replace(importPathPrefix, "");
                originalName = originalName.TrimStart(new char[] { '\\' });

                string newName = currFile.FinalNewName.Replace(importPathPrefix, "");
                newName = newName.TrimStart(new char[] { '\\' });

                ListViewItem listItem = new ListViewItem(originalName);
                listItem.Tag = currItem;
                listItem.Checked = currItem.Approved;
                listItem.SubItems.Add(newName);
                fileListView.Items.Add(listItem);
                listItemLookup[currItem] = listItem;
            }

            fileListView.EndUpdate();
            loading = false;
        }

        // Renames files and folder as needed for the current movie.
        private void RenameSelectedMovie() {
            itemsToRename.RenameApprovedItems();
        }

        // Renames files from remaining movies in the list. 
        private void RenameRemainingMovies() {
            renamingAll = true;
            this.Visible = false;

            ProgressPopup popup = new ProgressPopup(new TrackableWorkerDelegate(RenameRemainingWorker));
            popup.Owner = this.Owner;
            popup.ShowDialog();
        }

        private void RenameRemainingWorker(ProgressDelegate progress) {
            int total = remaining;
            int percentDone = (int)Math.Round(100.0 * (total - remaining) / total);
            progress("Renaming " + movieEnumerator.Current.Title + "...", percentDone);

            RenameSelectedMovie();
            while (LoadNextMovie()) {
                percentDone = (int)Math.Round(100.0 * (total - remaining) / total);
                progress("Renaming " + movieEnumerator.Current.Title + "...", percentDone);
                RenameSelectedMovie();
            }            
        }

        private void renameFilesMenuItem_Click(object sender, EventArgs e) {
            MovingPicturesCore.Settings.RenameFiles = renameFilesMenuItem.Checked;
            RebuildRenamableList();
            PopulateRenameList();
            UpdateConfigControls();
            UpdateStandardControls();
        }

        private void renameFoldersMenuItem_Click(object sender, EventArgs e) {
            MovingPicturesCore.Settings.RenameFolders = renameFoldersMenuItem.Checked;
            RebuildRenamableList();
            PopulateRenameList();
            UpdateConfigControls();
            UpdateStandardControls();
        }

        private void includeSecondaryFilesMenuItem_Click(object sender, EventArgs e) {
            MovingPicturesCore.Settings.RenameSecondaryFiles = renameSecondaryFilesMenuItem.Checked;
            RebuildRenamableList();
            PopulateRenameList();
            UpdateConfigControls();
            UpdateStandardControls();
        }

        private void skipButton_Click(object sender, EventArgs e) {
            if (confirmAllCheckBox.Checked) {
                Close();
                return;
            }

            LoadNextMovie();
        }

        private void renameButton_Click(object sender, EventArgs e) {
            if (confirmAllCheckBox.Checked) {
                RenameRemainingMovies();
                Close();
                return;
            }

            RenameSelectedMovie();
            LoadNextMovie();
        }

        private void RenameConfirmationPopup_Load(object sender, EventArgs e) {
            LoadNextMovie();
        }

        private void fileListView_ItemChecked(object sender, ItemCheckedEventArgs e) {
            if (loading) return;

            Renamable item = e.Item.Tag as Renamable;
            item.Approved = e.Item.Checked;

            itemsToRename.UpdateFinalFilenames();
            UpdateStandardControls();
            PopulateRenameList();
        }
    }
}
