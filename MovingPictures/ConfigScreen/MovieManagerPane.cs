using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using System.Diagnostics;
using System.Collections;
using System.Threading;
using Cornerstone.Database.Tables;
using Cornerstone.Database;
using Cornerstone.GUI.Controls;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class MovieManagerPane : UserControl {

        private Dictionary<DBMovieInfo, ListViewItem> listItems;
        private List<DBLocalMedia> processingFiles;
        private List<DBMovieInfo> processingMovies;
        
        private delegate void InvokeDelegate();
        private delegate DBMovieInfo DBMovieInfoDelegate();

        public DBMovieInfo CurrentMovie {
            get {
                if (movieListBox == null || movieListBox.SelectedItems.Count != 1)
                    return null;

                return (DBMovieInfo)movieListBox.SelectedItems[0].Tag;
            }
        }

        public MovieManagerPane() {
            InitializeComponent();
            
            listItems = new Dictionary<DBMovieInfo, ListViewItem>();
            processingFiles = new List<DBLocalMedia>();
        }

        ~MovieManagerPane() {
            foreach (DBMovieInfo currMovie in listItems.Keys) {
                currMovie.Commit();
                foreach (DBLocalMedia currFile in currMovie.LocalMedia)
                    currFile.Commit();
            }
        }

        private void MovieManagerPane_Load(object sender, EventArgs e) {
            movieListBox.ListViewItemSorter = new DBMovieInfoComparer();
            
            if (!DesignMode) {
                ReloadList();

                MovingPicturesCore.DatabaseManager.ObjectDeleted +=
                    new DatabaseManager.ObjectAffectedDelegate(movieDeletedListener);

                MovingPicturesCore.DatabaseManager.ObjectInserted +=
                    new DatabaseManager.ObjectAffectedDelegate(movieInsertedListener);
            }
        }

        // loads from scratch all movies in the database into the side panel
        public void ReloadList() {
            // turn off redraws temporarily and clear the list
            movieListBox.BeginUpdate();
            movieListBox.Items.Clear();
            
            foreach (DBMovieInfo currMovie in DBMovieInfo.GetAll()) 
                addMovie(currMovie);
            
            movieListBox.EndUpdate();

            if (movieListBox.Items.Count > 0)
                movieListBox.Items[0].Selected = true;
        }

        // adds the given movie and it's related files to the tree view
        private void addMovie(DBMovieInfo movie) {
            ListViewItem newItem = new ListViewItem(movie.Title);
            newItem.Tag = movie;
            movieListBox.Items.Add(newItem);
            listItems[movie] = newItem;
        }

        private void movieDeletedListener(DatabaseTable obj) {
            // This ensures we are thread safe. Makes sure this method is run by
            // the thread that created this panel.
            if (InvokeRequired) {
                Delegate method = new DatabaseManager.ObjectAffectedDelegate(movieDeletedListener);
                object[] parameters = new object[] { obj };
                this.Invoke(method, parameters);
                return;
            }

            // if this is not a movie object, break
            if (obj.GetType() != typeof(DBMovieInfo))
                return;

            // remove movie from list
            DBMovieInfo movie = (DBMovieInfo)obj;
            if (listItems.ContainsKey(movie)) {
                listItems[movie].Selected = false;
                updateMoviePanel();
                updateFilePanel();

                movieListBox.Items.Remove(listItems[movie]);
                listItems.Remove(movie);
            }
            
        }

        private void movieInsertedListener(DatabaseTable obj) {
            // This ensures we are thread safe. Makes sure this method is run by
            // the thread that created this panel.
            if (InvokeRequired) {
                Delegate method = new DatabaseManager.ObjectAffectedDelegate(movieInsertedListener);
                object[] parameters = new object[] { obj };
                this.Invoke(method, parameters);
                return;
            }

            // if this is not a movie object, break
            if (obj.GetType() != typeof(DBMovieInfo))
                return;

            // add movie to the list
            addMovie((DBMovieInfo)obj);

            bool reassigning = false;
            foreach (DBLocalMedia currFile in processingFiles) {
                if (((DBMovieInfo)obj).LocalMedia.Contains(currFile))
                    reassigning = true;
                else {
                    reassigning = false;
                    break;
                }
            }

            if (reassigning)
                listItems[(DBMovieInfo)obj].Selected = true;

        }

        private void updateMoviePanel() {
            if (InvokeRequired) {
                this.Invoke(new InvokeDelegate(updateMoviePanel));
                return;
            }

            if (CurrentMovie == null) {
                // if we have no movie selcted (or multiple movies selected) clear out details
                movieTitleTextBox.DatabaseObject = null;
                movieDetailsList.DatabaseObject = null;
                movieDetailsList.Enabled = false;
                coverImage.Image = null;
                resolutionLabel.Text = "";
                coverNumLabel.Text = "";

                if (movieListBox.SelectedItems.Count > 1) {
                    reassignMovieButton.Enabled = false;
                    playMovieButton.Enabled = false;
                }
               
                return;
            }
            movieDetailsList.Enabled = true;

            reassignMovieButton.Enabled = true;
            playMovieButton.Enabled = true;
            
            // setup coverart thumbnail panel
            if (CurrentMovie.CoverThumb != null && CurrentMovie.Cover != null) {
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
            // if no object selected (or multiple objects selected) clear details
            if (CurrentMovie == null) {
                fileList.Items.Clear();
                fileList.Enabled = false;
                fileDetailsList.DatabaseObject = null;
                return;
            }

            // populate file list combo
            fileList.Items.Clear();
            CurrentMovie.LocalMedia.Sort(new DBLocalMediaComparer());
            foreach (DBLocalMedia currFile in CurrentMovie.LocalMedia) 
                fileList.Items.Add(currFile);

            // select first file            
            if (fileList.Items.Count > 0) {
                fileList.SelectedIndex = 0;
            }

            // only allow user to drop down if there is more than one movie file
            if (fileList.Items.Count > 1)
                fileList.Enabled = true;
            else
                fileList.Enabled = false;
        }

        private void movieTree_AfterSelect(object sender, EventArgs e) {
            // drop out if we for some reason dont have a movie from out list
            if (movieListBox.SelectedItems.Count == 0)
                return;

            // unload the artowrk and commit for the previously selected movie
            if (movieDetailsList.DatabaseObject != null) {
                ((DBMovieInfo)movieDetailsList.DatabaseObject).UnloadArtwork();
                ((DBMovieInfo)movieDetailsList.DatabaseObject).Commit();
            }

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

            // get the result of the dialog box and only update if the user clicked OK
            DialogResult answerCover = coverArtFileDialog.ShowDialog(this);

            if (coverArtFileDialog.FileName.Length != 0 && answerCover == DialogResult.OK) {
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

            // do not waste time processing any more if there is no length to the URL when the user clicks OK
            if (popup.GetURL().Trim().Length > 0 && popup.DialogResult == DialogResult.OK) {
                DBMovieInfo movie = CurrentMovie;

                // the retrieval process can take a little time, so spawn it off in another thread
                ThreadStart actions = delegate {
                    startArtProgressBar();
                    ArtworkLoadStatus result = movie.AddCoverFromURL(popup.GetURL(), true);
                    stopArtProgressBar();

                    switch (result) {
                        case ArtworkLoadStatus.SUCCESS:
                            // set new cover to current and update screen
                            movie.CoverFullPath = movie.AlternateCovers[movie.AlternateCovers.Count - 1];
                            updateMoviePanel();
                            break;
                        case ArtworkLoadStatus.ALREADY_LOADED:
                            MessageBox.Show("Cover art from the specified URL has already been loaded.");
                            break;
                        case ArtworkLoadStatus.FAILED:
                            MessageBox.Show("Failed loading cover art from specified URL.");
                            break;
                    }
                };

                Thread thread = new Thread(actions);
                thread.Name = "ArtUpdater";
                thread.Start();
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

        private void fileList_SelectedIndexChanged(object sender, EventArgs e) {
            if (fileList.SelectedItem != null)
                fileDetailsList.DatabaseObject = (DBLocalMedia)fileList.SelectedItem;
        }

        private void deleteMovieButton_Click(object sender, EventArgs e) {
            if (movieListBox.SelectedItems.Count > 0) {
                DialogResult result =
                    MessageBox.Show("Are you sure you want to remove this movie from the\n" +
                                    "database and ignore all related video files in\n" +
                                    "future scans?", "Delete Movie", MessageBoxButtons.YesNo);

                if (result == System.Windows.Forms.DialogResult.Yes) {
                    List<DBMovieInfo> deleteList = new List<DBMovieInfo>();
                    foreach (ListViewItem currItem in movieListBox.SelectedItems) 
                        deleteList.Add((DBMovieInfo)currItem.Tag);

                    foreach (DBMovieInfo currMovie in deleteList)
                        currMovie.DeleteAndIgnore();
                    
                }

                updateMoviePanel();
                updateFilePanel();
            }
        }

        private void playMovieButton_Click(object sender, EventArgs e) {
            if (fileList.SelectedItem == null)
                return;

            ProcessStartInfo processInfo = new ProcessStartInfo(((DBLocalMedia)fileList.SelectedItem).File.FullName);
            Process.Start(processInfo);
        }

        private void refreshMovieButton_Click(object sender, EventArgs e) {
            if (movieListBox.SelectedItems.Count == 0)
                return;

            DialogResult result =
                MessageBox.Show("You are about to refresh metadata for the selected movie, overwriting\n" +
                                "any custom modifications. If operating on a large number of\n" +
                                "movies this can be time consuming. Are you sure you want to\n" +
                                "continue?",
                                "Refresh Movie", MessageBoxButtons.OKCancel);

            if (result == System.Windows.Forms.DialogResult.OK) {
                processingMovies = new List<DBMovieInfo>();
                foreach (ListViewItem currItem in movieListBox.SelectedItems)
                    processingMovies.Add((DBMovieInfo)currItem.Tag);

                if (movieListBox.SelectedItems.Count <= 1)
                    refreshMovies(null);
                else {
                    ProgressPopup popup = new ProgressPopup(new ProgressPopup.TrackableWorkerDelegate(refreshMovies));
                    popup.Owner = this.ParentForm;
                    popup.ShowDialog();
                }

                updateMoviePanel();
                processingMovies.Clear();
            }
        }

        private void refreshMovies(ProgressPopup.ProgressDelegate progress) {
            int count = 0;
            int total = processingMovies.Count;

            foreach (DBMovieInfo currItem in processingMovies) {
                MovingPicturesCore.DataProviderManager.Update(currItem);

                count++;
                if (progress != null)
                    progress(count, total);
            }
        }

        private void reassignMovieButton_Click(object sender, EventArgs e) {
            if (CurrentMovie == null)
                return;
            DialogResult result = MessageBox.Show(
                    "You are about to reassign this file or set of files\n" +
                    "to a new movie. You will loose all custom modifications\n" +
                    "to metadata and all user settings.\n\n" +
                    "Are you sure you want to continue?",
                    "Reassign Movie", MessageBoxButtons.YesNo);

            if (result == System.Windows.Forms.DialogResult.Yes) {
                processingFiles.AddRange(CurrentMovie.LocalMedia);
                SingleMovieImporterPopup popup = new SingleMovieImporterPopup(CurrentMovie);
                popup.ShowDialog();
                processingFiles.Clear();
            }
        }

        private void refreshAllMoviesToolStripMenuItem_Click(object sender, EventArgs e) {
            DialogResult result =
                MessageBox.Show("You are about to refresh all movie metadata, overwriting\n" +
                                "any custom modifications. This operation will be performed\n" +
                                "on ALL MOVIES IN YOUR DATABASE, and can be very time\n" +
                                "consuming. Are you sure you want to continue?",
                                "Refresh All Movies", MessageBoxButtons.OKCancel);

            if (result == System.Windows.Forms.DialogResult.OK) {
                processingMovies = new List<DBMovieInfo>();
                foreach (DBMovieInfo currItem in listItems.Keys)
                    processingMovies.Add(currItem);

                ProgressPopup popup = new ProgressPopup(new ProgressPopup.TrackableWorkerDelegate(refreshMovies));
                popup.Owner = this.ParentForm;
                popup.ShowDialog();

                updateMoviePanel();
                processingMovies.Clear();
            }
        }

        private void refreshArtButton_Click(object sender, EventArgs e) {
            if (CurrentMovie == null)
                return;

            DBMovieInfo movie = CurrentMovie;

            // the update process can take a little time, so spawn it off in another thread
            ThreadStart actions = delegate {
                startArtProgressBar();
                MovingPicturesCore.DataProviderManager.GetArtwork(movie);
                stopArtProgressBar();
                updateMoviePanel();
            };

            Thread thread = new Thread(actions);
            thread.Name = "ArtUpdater";
            thread.Start();
        }


        private void startArtProgressBar() {
            if (InvokeRequired) {
                Invoke(new InvokeDelegate(startArtProgressBar));
                return;
            }

            artworkProgressBar.Visible = true;
        }

        private void stopArtProgressBar() {
            if (InvokeRequired) {
                Invoke(new InvokeDelegate(stopArtProgressBar));
                return;
            }

            artworkProgressBar.Visible = false;
        }

        private void fileUpButton_Click(object sender, EventArgs e) {
            int index = fileList.SelectedIndex;
            if (index == 0)
                return;

            // swap the part# with the file above
            int partA = ((DBLocalMedia)fileList.Items[index]).Part;
            int partB = ((DBLocalMedia)fileList.Items[index - 1]).Part;
            ((DBLocalMedia)fileList.Items[index]).Part = partB;
            ((DBLocalMedia)fileList.Items[index - 1]).Part = partA;

            updateFilePanel();
            fileList.SelectedIndex = index - 1;
        }

        private void fileDownButton_Click(object sender, EventArgs e) {
            int index = fileList.SelectedIndex;
            if (index == fileList.Items.Count - 1)
                return;

            // swap the part# with the file above
            int partA = ((DBLocalMedia)fileList.Items[index]).Part;
            int partB = ((DBLocalMedia)fileList.Items[index + 1]).Part;
            ((DBLocalMedia)fileList.Items[index]).Part = partB;
            ((DBLocalMedia)fileList.Items[index + 1]).Part = partA;

            updateFilePanel();
            fileList.SelectedIndex = index + 1;
        }

    }


    public class DBMovieInfoComparer : IComparer {
        public int Compare(object x, object y) {
            try {
                DBMovieInfo movieX = ((DBMovieInfo)((ListViewItem)x).Tag);
                DBMovieInfo movieY = ((DBMovieInfo)((ListViewItem)y).Tag);

                return movieX.SortBy.CompareTo(movieY.SortBy);
            }
            catch {
                return 0;
            }
        }
    }

    public class DBLocalMediaComparer : IComparer<DBLocalMedia> {
        public int Compare(DBLocalMedia fileX, DBLocalMedia fileY) {
            if (fileX.Part < fileY.Part)
                return -1;
            else
                return 1;
        }
    }
}
