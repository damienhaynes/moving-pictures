using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;
using System.Diagnostics;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class MovieManagerPane : UserControl {
        private Dictionary<DBMovieInfo, TreeNode> movieNodes;

        public DBMovieInfo CurrentMovie {
            get {
                if (movieTree == null || movieTree.SelectedNode == null ||
                    movieTree.SelectedNode.Tag == null ||
                    movieTree.SelectedNode.Tag.GetType() != typeof(DBMovieInfo))
                    return null;

                return (DBMovieInfo)movieTree.SelectedNode.Tag;
            }
        }

        public MovieManagerPane() {
            InitializeComponent();

            movieNodes = new Dictionary<DBMovieInfo, TreeNode>();
        }

        ~MovieManagerPane() {
            foreach (DBMovieInfo currMovie in movieNodes.Keys)
                currMovie.Commit();
        }

        private void MovieManagerPane_Load(object sender, EventArgs e) {
            if (!DesignMode) {
                ReloadList();

                MovingPicturesPlugin.Importer.MovieStatusChanged += new MovieImporter.MovieStatusChangedHandler(movieStatusChangedListener);
            }
        }

        // loads from scratch all movies in the database into the side panel
        public void ReloadList() {
            List<DBMovieInfo> movieList = DBMovieInfo.GetAll();
            movieList.Sort();

            // turn off redraws temporarily and clear the list
            movieTree.BeginUpdate();
            movieTree.Nodes.Clear();

            foreach (DBMovieInfo currMovie in movieList)
                addMovie(currMovie);

            movieTree.EndUpdate();
        }

        // adds the given movie and it's related files to the tree view
        private void addMovie(DBMovieInfo movie) {
            TreeNode movieNode = new TreeNode(movie.Name);
            movieNode.Tag = movie;
            movieTree.Nodes.Add(movieNode);
            movieNodes[movie] = movieNode;

            foreach (DBLocalMedia currFile in movie.LocalMedia) {
                TreeNode fileNode = new TreeNode(currFile.File.Name);
                fileNode.Tag = currFile;
                movieNode.Nodes.Add(fileNode);
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

            switch (action) {
                case MovieImporterAction.COMMITED:
                    addMovie(obj.Selected.Movie);
                    break;
                
                case MovieImporterAction.JOINED:
                case MovieImporterAction.SPLIT:
                case MovieImporterAction.IGNORED:
                case MovieImporterAction.REPROCCESSING_PENDING:
                    if (obj.Selected != null && movieNodes.ContainsKey(obj.Selected.Movie)) {
                        movieTree.Nodes.Remove(movieNodes[obj.Selected.Movie]);
                        movieNodes.Remove(obj.Selected.Movie);
                    }
                    break;
            }
        }

        private void updateMoviePanel() {
            if (CurrentMovie == null)
                return;

            // setup coverart thumbnail panel
            if (CurrentMovie.CoverThumb != null) {
                coverImage.Image = CurrentMovie.CoverThumb;
                resolutionLabel.Text = CurrentMovie.Cover.Width + " x " + CurrentMovie.Cover.Height;
                coverNumLabel.Text = (CurrentMovie.AlternateCovers.IndexOf(CurrentMovie.CoverFullPath) + 1) +
                                     " / " + CurrentMovie.AlternateCovers.Count;
            }
            else {
                coverImage.Image = null;
                resolutionLabel.Text = "";
                coverNumLabel.Text = "";
            }

            // enable/disable next/previous buttons for coverart
            if (CurrentMovie.AlternateCovers.Count <= 1) {
                previousCoverButton.Enabled = false;
                nextCoverButton.Enabled = false;
            }
            else {
                previousCoverButton.Enabled = true;
                nextCoverButton.Enabled = true;
            }

            // enable/disable zoom & delete buttons for cover art
            if (CurrentMovie.Cover != null) {
                zoomButton.Enabled = true;
                deleteCoverButton.Enabled = true;
            }
            else {
                zoomButton.Enabled = false;
                deleteCoverButton.Enabled = false;
            }
            
            // populate movie details fields
            movieTitleTextBox.DatabaseObject = CurrentMovie;
            movieDetailsList.DatabaseObject = CurrentMovie;

        }

        private void updateFilePanel() {
            if (CurrentMovie == null)
                return;

            // populate file list combo
            fileCombo.Items.Clear();
            foreach (DBLocalMedia currFile in CurrentMovie.LocalMedia) 
                fileCombo.Items.Add(currFile);

            // select first file            
            if (fileCombo.Items.Count > 0) {
                fileCombo.SelectedIndex = 0;
            }

            // only allow user to drop down if there is more than one movie file
            if (fileCombo.Items.Count > 1)
                fileCombo.Enabled = true;
            else
                fileCombo.Enabled = false;
        }

        private void movieTree_BeforeSelect(object sender, TreeViewCancelEventArgs e) {
            if (CurrentMovie == null)
                return;

            CurrentMovie.UnloadArtwork();
            CurrentMovie.Commit();
        }

        private void movieTree_AfterSelect(object sender, TreeViewEventArgs e) {
            // drop out if we for some reason dont have a tag from out tree
            if (movieTree.SelectedNode.Tag == null)
                return;

            updateMoviePanel();
            updateFilePanel();
        }

        private void previousCoverButton_Click(object sender, EventArgs e) {
            if (CurrentMovie == null)
                return;

            CurrentMovie.PreviousCover();
            updateMoviePanel();
        }

        private void nextCoverButton_Click(object sender, EventArgs e) {
            if (CurrentMovie == null)
                return;

            CurrentMovie.NextCover();
            updateMoviePanel();
        }

        private void zoomButton_Click(object sender, EventArgs e) {
            if (CurrentMovie == null || CurrentMovie.Cover == null)
                return;

            CoverPopup popup = new CoverPopup(CurrentMovie.Cover);
            popup.Owner = this.ParentForm;
            popup.Show();
        }

        private void coverImage_DoubleClick(object sender, EventArgs e) {
            if (CurrentMovie == null || CurrentMovie.Cover == null)
                return;

            CoverPopup popup = new CoverPopup(CurrentMovie.Cover);
            popup.Owner = this.ParentForm;
            popup.Show();
        }

        private void loadCoverArtFromFileToolStripMenuItem_Click(object sender, EventArgs e) {
            loadCoverFromFile();
        }

        private void loadNewCoverButton_ButtonClick(object sender, EventArgs e) {
            loadCoverFromFile();
        }

        private void loadCoverFromFile() {
            if (CurrentMovie == null)
                return;

            coverArtFileDialog.ShowDialog(this);

            if (coverArtFileDialog.FileName.Length != 0) {
                bool success = CurrentMovie.AddCoverFromFile(coverArtFileDialog.FileName);

                if (success) {
                    // set new cover to current and update screen
                    CurrentMovie.CoverFullPath = CurrentMovie.AlternateCovers[CurrentMovie.AlternateCovers.Count - 1];
                    updateMoviePanel();
                }
                else
                    MessageBox.Show("Failed loading cover art from specified URL.");
            }
        }

        private void loadCoverArtFromURLToolStripMenuItem_Click(object sender, EventArgs e) {
            if (CurrentMovie == null)
                return;

            CoverURLPopup popup = new CoverURLPopup();
            popup.ShowDialog(this);

            if (popup.DialogResult == DialogResult.OK) {
                CoverArtLoadStatus result = CurrentMovie.AddCoverFromURL(popup.GetURL(), true);

                switch (result) {
                    case CoverArtLoadStatus.SUCCESS:
                        // set new cover to current and update screen
                        CurrentMovie.CoverFullPath = CurrentMovie.AlternateCovers[CurrentMovie.AlternateCovers.Count - 1];
                        updateMoviePanel();
                        break;
                    case CoverArtLoadStatus.ALREADY_LOADED:
                        MessageBox.Show("Cover art from the specified URL has already been loaded.");
                        break;
                    case CoverArtLoadStatus.FAILED:
                        MessageBox.Show("Failed loading cover art from specified URL.");
                        break;
                }
            }
        }

        private void deleteCoverButton_Click(object sender, EventArgs e) {
            if (CurrentMovie == null || CurrentMovie.AlternateCovers.Count == 0)
                return;
            
            DialogResult result;
            result = MessageBox.Show("Permanently delete selected cover art?", "Delete Cover Art", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes) {
                CurrentMovie.DeleteCurrentCover();
                updateMoviePanel();
            }
        }

        private void fileCombo_SelectedIndexChanged(object sender, EventArgs e) {
            if (fileCombo.SelectedItem != null)
                fileDetailsList.DatabaseObject = (DBLocalMedia) fileCombo.SelectedItem;
        }

        private void deleteMovieButton_Click(object sender, EventArgs e) {
            if (CurrentMovie != null) {
                DialogResult result =
                    MessageBox.Show("Are you sure you want to remove this movie from the\n" +
                                    "database and ignore all related video files in\n" +
                                    "future scans?", "Delete Movie", MessageBoxButtons.YesNo);

                if (result == System.Windows.Forms.DialogResult.Yes) {
                    DBMovieInfo movie = CurrentMovie;
                    movie.DeleteAndIgnore();
                    movieTree.Nodes.Remove(movieNodes[movie]);
                    movieNodes.Remove(movie);

                }
            }
        }

        private void playMovieButton_Click(object sender, EventArgs e) {
            if (fileCombo.SelectedItem == null)
                return;

            ProcessStartInfo processInfo = new ProcessStartInfo(((DBLocalMedia)fileCombo.SelectedItem).File.FullName);
            Process.Start(processInfo);
        }

        private void refreshMovieButton_Click(object sender, EventArgs e) {
            if (CurrentMovie == null)
                return;
            DialogResult result = 
                MessageBox.Show("You are about to refresh all movie metadata, overwriting\n" +
                                "any custom modifications to this film.",
                                "Refresh Movie", MessageBoxButtons.OKCancel);

            if (result == System.Windows.Forms.DialogResult.OK) {
                MovingPicturesPlugin.MovieProvider.Update(CurrentMovie);
                updateMoviePanel();
            }
        }

    }
}
