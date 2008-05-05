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

                movieTitleTextBox.DatabaseField = DBMovieInfo.GetField("Name");
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

            // setup coverart thumbnail
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

            // enable/disable zoom & delete buttons
            if (CurrentMovie.Cover != null) {
                zoomButton.Enabled = true;
                deleteButton.Enabled = true;
            }
            else {
                zoomButton.Enabled = false;
                deleteButton.Enabled = false;
            }
            
            // populate movie details fields
            movieTitleTextBox.Text = CurrentMovie.Name;

            movieTitleTextBox.DatabaseObject = CurrentMovie;
            dbObjectList1.DatabaseObject = CurrentMovie;

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

        private void movieTitleTextBox_TextChanged(object sender, EventArgs e) {
            if (CurrentMovie == null)
                return;

            CurrentMovie.Name = movieTitleTextBox.Text;
        }

        private void movieTitleTextBox_Enter(object sender, EventArgs e) {
            movieTitleTextBox.BorderStyle = BorderStyle.FixedSingle;
            movieTitleTextBox.BackColor = System.Drawing.SystemColors.Window;
        }

        private void movieTitleTextBox_Leave(object sender, EventArgs e) {
            movieTitleTextBox.BorderStyle = BorderStyle.None;
            movieTitleTextBox.BackColor = System.Drawing.SystemColors.Control;
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
            loadFromFile();
        }

        private void loadNewCoverButton_ButtonClick(object sender, EventArgs e) {
            loadFromFile();
        }

        private void loadFromFile() {
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

        private void deleteButton_Click(object sender, EventArgs e) {
            if (CurrentMovie == null || CurrentMovie.AlternateCovers.Count == 0)
                return;
            
            DialogResult result;
            result = MessageBox.Show("Permanently delete selected cover art?", "Delete Cover Art", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes) {
                CurrentMovie.DeleteCurrentCover();
                updateMoviePanel();
            }
        }


    }
}
