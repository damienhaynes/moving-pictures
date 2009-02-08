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
using NLog;
using System.Collections.ObjectModel;
using MediaPortal.Plugins.MovingPictures.DataProviders;
using System.IO;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class MovieManagerPane : UserControl {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Dictionary<DBMovieInfo, ListViewItem> listItems;
        private List<DBLocalMedia> processingFiles;
        private List<DBMovieInfo> processingMovies;
        private DBSourceInfo selectedSource;

        private Image coverThumb;
        
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

        public void Commit() {
            try {
                foreach (DBMovieInfo currMovie in listItems.Keys) {
                    currMovie.Commit();
                    foreach (DBLocalMedia currFile in currMovie.LocalMedia)
                        currFile.Commit();
                    foreach (DBUserMovieSettings currSetting in currMovie.UserSettings)
                        currSetting.Commit();
                }
            }
            catch (Exception e) {
                logger.Error(e);
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

                addMovieRefreshMenuItems();
            }
        }

        private void addMovieRefreshMenuItems() {
            ReadOnlyCollection<DBSourceInfo> sources = MovingPicturesCore.DataProviderManager.MovieDetailSources;
            foreach (DBSourceInfo currSource in sources) {
                ToolStripMenuItem newItem = new ToolStripMenuItem();
                newItem.Name = currSource.Provider.Name + "ToolStripMenuItem";
                newItem.Text = "Refresh from " + currSource.Provider.Name + " (" + currSource.Provider.Language + ")";
                newItem.Click += new System.EventHandler(this.refreshMovieButton_Click);
                newItem.Tag = currSource;
                refreshMovieButton.DropDownItems.Add(newItem);
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
            
            // if the movie is offline color it red
            if (movie.LocalMedia.Count > 0) {
                if (!movie.LocalMedia[0].IsAvailable) {
                    newItem.ForeColor = Color.Red;
                    newItem.ToolTipText = "This movie is currently offline.";
                }
            }
            
            // add to list
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

            movieDetailsSubPane.DatabaseObject = CurrentMovie;

            if (CurrentMovie == null) {
                // if we have no movie selcted (or multiple movies selected) clear out details
                titleLabel.Text = "";
                
                coverImage.Image = null;
                resolutionLabel.Text = "";
                coverNumLabel.Text = "";

                if (movieListBox.SelectedItems.Count > 1) {
                    reassignMovieButton.Enabled = false;
                    playMovieButton.Enabled = false;
                }
               
                return;
            }

            reassignMovieButton.Enabled = true;
            playMovieButton.Enabled = true;
            
            // update cover artwork
            try {
                Image newCover = Image.FromFile(CurrentMovie.CoverThumbFullPath);
                Image oldCover = coverImage.Image;
                coverImage.Image = newCover;
                if (oldCover != null) oldCover.Dispose();

                resolutionLabel.Text = newCover.Width + " x " + newCover.Height;
                coverNumLabel.Text = (CurrentMovie.AlternateCovers.IndexOf(CurrentMovie.CoverFullPath) + 1) +
                                     " / " + CurrentMovie.AlternateCovers.Count;
            }
            catch (Exception e) {
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
            if (CurrentMovie.CoverFullPath.Trim().Length != 0) {
                zoomButton.Enabled = true;
                deleteCoverButton.Enabled = true;
            }
            else {
                zoomButton.Enabled = false;
                deleteCoverButton.Enabled = false;
            }
            
            // populate movie details fields
            titleLabel.Text = CurrentMovie.Title;
        }

        private void updateFilePanel() {
            fileDetailsSubPane.DatabaseObject = CurrentMovie;
        }

        private void movieTree_AfterSelect(object sender, EventArgs e) {
            // drop out if we for some reason dont have a movie from out list
            if (movieListBox.SelectedItems.Count == 0)
                return;

            // commit the previous movie if necessary
            if (movieDetailsSubPane.DatabaseObject != null) {
                DBMovieInfo oldMovie = (DBMovieInfo)movieDetailsSubPane.DatabaseObject;
                if (oldMovie.ID != null)
                    oldMovie.Commit();
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
            if (CurrentMovie == null || CurrentMovie.CoverFullPath.Trim().Length == 0)
                return;

            CoverPopup popup = new CoverPopup(CurrentMovie.CoverFullPath);
            popup.Owner = this.ParentForm;
            popup.Show();
        }

        private void coverImage_DoubleClick(object sender, EventArgs e) {
            if (CurrentMovie == null || CurrentMovie.CoverFullPath.Trim().Length == 0)
                return;

            CoverPopup popup = new CoverPopup(CurrentMovie.CoverFullPath);
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
            fileDetailsSubPane.playSelected();
        }

        private void refreshMovieButton_Click(object sender, EventArgs e) {
            if (movieListBox.SelectedItems.Count == 0)
                return;

            if (sender is ToolStripMenuItem)
                selectedSource = (DBSourceInfo)((ToolStripMenuItem)sender).Tag;
            else
                selectedSource = null;

            DialogResult result =
                MessageBox.Show("This action will overwrite existing movie details, including any custom modifications.",
                                "Refresh Movie", MessageBoxButtons.OKCancel);

            if (result == System.Windows.Forms.DialogResult.OK) {
                processingMovies = new List<DBMovieInfo>();
                foreach (ListViewItem currItem in movieListBox.SelectedItems)
                    processingMovies.Add((DBMovieInfo)currItem.Tag);

                if (movieListBox.SelectedItems.Count <= 1)
                    refreshMovies(null);
                else {
                    ProgressPopup popup = new ProgressPopup(new TrackableWorkerDelegate(refreshMovies));
                    popup.Owner = this.ParentForm;
                    popup.ShowDialog();
                }

                updateMoviePanel();
                processingMovies.Clear();
            }
        }

        private void refreshMovies(ProgressDelegate progress) {
            float count = 0;
            float total = processingMovies.Count;
            int sentToImporter = 0;

            foreach (DBMovieInfo currItem in processingMovies) {
                // if the user specified a specific source, try to update, and if failed
                // send to the importer
                if (selectedSource != null) {
                    UpdateResults result =  selectedSource.Provider.Update(currItem);
                    if (result == UpdateResults.FAILED_NEED_ID) {
                        MovingPicturesCore.Importer.Update(currItem, selectedSource);
                        sentToImporter++;
                    }
                }

                // just use the regular update logic
                else {
                    MovingPicturesCore.DataProviderManager.Update(currItem);
                }

                count++;
                if (progress != null)
                    progress("", (int)(count*100/total));

                // delay a little so we dont hammer the webserver.
                logger.Info("refreshing movie #" + count);
                Thread.Sleep(200);
                if (count % 5 == 0)
                    Thread.Sleep(1000);
            }

            if (sentToImporter > 0) {
                MessageBox.Show(this.ParentForm,
                "There were " + sentToImporter + " movie(s) that needed to be sent to the importer to\n" +
                "search for possible matches from the " + selectedSource.Provider.Name + " data source.\n" +
                "You will now be taken to the Movie Importer tab to check for\n" + 
                "any matches requiring approval.", "Movies Sent to Importer", MessageBoxButtons.OK);
                switchToImporter();
            }

        }

        private void reassignMovieButton_Click(object sender, EventArgs e) {
            if (CurrentMovie == null)
                return;

            // Check if all files belonging to the movie are available.
            bool continueReassign = false;
            while (!continueReassign) {
                continueReassign = true;
                foreach (DBLocalMedia localMedia in CurrentMovie.LocalMedia) {
                    // if the file is offline
                    if (!localMedia.IsAvailable) {
                        // do not continue
                        continueReassign = false;

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
                        "The file or files you want to reassign are currently not available.\n\n" + connect,
                        "File(s) not available.", MessageBoxButtons.RetryCancel);

                        // if cancel is pressed stop the reassign process.
                        if (resultInsert == DialogResult.Cancel)
                            return;
                        
                        // break foreach loop (and recheck condition)
                        break;
                    }
                }
            }

            // If we made it this far all files are available and we can notify the user
            // about what the reassign process is going to do.
            DialogResult result = MessageBox.Show(
                    "You are about to reassign this file or set of files\n" +
                    "to a new movie. This will send the file(s) back to\n" +
                    "the importer and you will loose all custom modifications\n" +
                    "to metadata and all user settings.\n\n" +
                    "Are you sure you want to continue?",
                    "Reassign Movie", MessageBoxButtons.YesNo);

            if (result == System.Windows.Forms.DialogResult.Yes) {
                List<DBLocalMedia> localMedia = new List<DBLocalMedia>(CurrentMovie.LocalMedia);
                CurrentMovie.Delete();
                MovingPicturesCore.Importer.Start();
                MovingPicturesCore.Importer.Reprocess(localMedia);
                switchToImporter();
            }
        }

        private void switchToImporter() {
            MovingPicturesConfig configWindow = ((MovingPicturesConfig)this.TopLevelControl);
            TabControl mainTabControl = (TabControl)configWindow.Controls["mainTabControl"];
            mainTabControl.SelectedTab = (TabPage)mainTabControl.Controls["importSettingsTab"];
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

                ProgressPopup popup = new ProgressPopup(new TrackableWorkerDelegate(refreshMovies));
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

        private void markAsUnwatchedToolStripMenuItem_Click(object sender, EventArgs e) {
            foreach (ListViewItem currItem in movieListBox.SelectedItems)
                ((DBMovieInfo)currItem.Tag).UserSettings[0].Watched = 0;

            updateMoviePanel();
        }

        private void watchedToolStripMenuItem_Click(object sender, EventArgs e) {
            foreach (ListViewItem currItem in movieListBox.SelectedItems)
                if (((DBMovieInfo)currItem.Tag).UserSettings[0].Watched == 0)
                    ((DBMovieInfo)currItem.Tag).UserSettings[0].Watched = 1;

            updateMoviePanel();
        }

        private void movieDetailsToolStripMenuItem_Click(object sender, EventArgs e) {
            detailsViewDropDown.Text = movieDetailsToolStripMenuItem.Text;
            movieDetailsSubPane.Visible = true;
            fileDetailsSubPane.Visible = false;
        }

        private void fileDetailsToolStripMenuItem_Click(object sender, EventArgs e) {
            detailsViewDropDown.Text = fileDetailsToolStripMenuItem.Text;
            movieDetailsSubPane.Visible = false;
            fileDetailsSubPane.Visible = true;
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
