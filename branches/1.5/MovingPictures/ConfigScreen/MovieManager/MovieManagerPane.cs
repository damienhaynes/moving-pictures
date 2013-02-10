using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
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
using MediaPortal.Plugins.MovingPictures.MainUI;
using NLog;
using System.Collections.ObjectModel;
using MediaPortal.Plugins.MovingPictures.DataProviders;
using System.IO;
using Cornerstone.GUI.Dialogs;
using Cornerstone.GUI.Filtering;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieResources;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieRenamer;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class MovieManagerPane : UserControl {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Dictionary<DBMovieInfo, ListViewItem> listItems;
        private List<DBLocalMedia> processingFiles;
        private List<DBMovieInfo> processingMovies;
        private DBSourceInfo selectedSource;
        private MenuEditorPopup menuEditorPopup = null;
        private DBMenu<DBMovieInfo> movieManagerFilterMenu;
        private bool translate;
        private readonly object lockList = new object();
        
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

            movieDetailsSubPane.FieldChanged += new FieldChangedListener(movieDetailsSubPane_FieldChanged);
        }

        void movieDetailsSubPane_FieldChanged(DatabaseTable obj, DBField field, object value) {
            // reset title field as needed
            if (field == DBField.GetFieldByDBName(typeof(DBMovieInfo), "title")) 
                listItems[obj as DBMovieInfo].Text = (obj as DBMovieInfo).Title;

            // resort as needed
            if (field == DBField.GetFieldByDBName(typeof(DBMovieInfo), "title") ||
                field == DBField.GetFieldByDBName(typeof(DBMovieInfo), "sortby")) {

                if (movieListBox != null) movieListBox.Sort();
            }
        }

        public void Commit() {
            try {
                foreach (DBMovieInfo currMovie in listItems.Keys.ToList()) {
                    if (currMovie.ID != null) {
                        currMovie.Commit();
                        foreach (DBLocalMedia currFile in currMovie.LocalMedia)
                            currFile.Commit();
                        foreach (DBUserMovieSettings currSetting in currMovie.UserSettings)
                            currSetting.Commit();
                    }
                }
            }
            catch (Exception e) {
                logger.Error(e);
            }
        }

        private void MovieManagerPane_Load(object sender, EventArgs e) {
            movieListBox.ListViewItemSorter = new DBMovieInfoComparer();
            
            if (!DesignMode) {
                // load filters
                movieListFilterBox.TreePanel.TranslationParser = new TranslationParserDelegate(Translation.ParseString);
                movieListFilterBox.Menu = MovingPicturesCore.Settings.MovieManagerFilterMenu;
                movieListFilterBox.SelectedNode = ((DBMenu<DBMovieInfo>)movieListFilterBox.Menu).RootNodes.First();
                movieListFilterBox.SelectedIndexChanged += movieListFilterBox_SelectedIndexChanged;

                // load movie list
                ReloadList();

                MovingPicturesCore.DatabaseManager.ObjectDeleted +=
                    new DatabaseManager.ObjectAffectedDelegate(movieDeletedListener);

                MovingPicturesCore.DatabaseManager.ObjectInserted +=
                    new DatabaseManager.ObjectAffectedDelegate(movieInsertedListener);

                MovingPicturesCore.DatabaseManager.ObjectUpdated +=
                    new DatabaseManager.ObjectAffectedDelegate(localMediaUpdatedListener);
            }
        }

        private void addMovieRefreshMenuItems() {
            // default the translator source in the drop down to imdb if the provider exists
            DBSourceInfo translatorSource = DBSourceInfo.GetFromScriptID(874902);

            refreshMovieButton.DropDownItems.Clear();

            ReadOnlyCollection<DBSourceInfo> sources = MovingPicturesCore.DataProviderManager.MovieDetailSources;
            foreach (DBSourceInfo currSource in sources) {
                if (currSource.GetPriority(DataType.DETAILS) == -1)
                    continue;

                if (translatorSource == null)
                    translatorSource = currSource;

                ToolStripMenuItem newItem = new ToolStripMenuItem();
                newItem.Name = currSource.Provider.Name + "ToolStripMenuItem";
                if (currSource.Provider.Language == "")
                    newItem.Text = "Refresh From " + currSource.Provider.Name;
                else
                    newItem.Text = "Refresh From " + currSource.Provider.Name + " [" + currSource.Provider.Language + "]";

                newItem.Click += new System.EventHandler(this.refreshMovieButton_Click);
                newItem.Tag = currSource;
                refreshMovieButton.DropDownItems.Add(newItem);
            }

            if (MovingPicturesCore.Settings.UseTranslator) {
                ToolStripMenuItem newItem = new ToolStripMenuItem();
                newItem.Name = "TranslateItem";
                newItem.Text = "Refresh From " + translatorSource.Provider.Name + " [Translated to " + MovingPicturesCore.Settings.TranslationLanguage.ToString() + "]";
                newItem.Click += new System.EventHandler(this.refreshMovieButton_Click);
                newItem.Tag = translatorSource;
                refreshMovieButton.DropDownItems.Add(new ToolStripSeparator());
                refreshMovieButton.DropDownItems.Add(newItem);
            }

        }

        // loads from scratch all movies in the database into the side panel
        public void ReloadList() {
            // turn off redraws temporarily and clear the list

            movieListBox.Items.Clear();
            movieListBox.Enabled = false;

            movieListFilterBox.Enabled = false;
            movieListButton.Enabled = false;

            // clear movie detail panels
            updateMoviePanel();
            updateFilePanel();

            Thread thread = new Thread(new ThreadStart(delegate {
                Invoke(new InvokeDelegate(delegate { loadingMoviesPanel.Visible = true; }));
                Invoke(new InvokeDelegate(delegate { movieListBox.BeginUpdate(); }));

                // get all movies
                var movies = DBMovieInfo.GetAll();

                // get current filter node and filter movie list
                var node = movieListFilterBox.SelectedNode;
                if (node != null) {
                    var nodeFilter = (node as DBNode<DBMovieInfo>).Filter;
                    if (nodeFilter != null) movies = nodeFilter.Filter(movies).ToList();
                }

                foreach (DBMovieInfo currMovie in movies) {
                    ListViewItem listItem = createMovieItem(currMovie);
                    addMovie(currMovie, listItem);
                }

                Invoke(new InvokeDelegate(delegate { movieListBox.EndUpdate(); }));
                Invoke(new InvokeDelegate(delegate { loadingMoviesPanel.Visible = false; }));
                Invoke(new InvokeDelegate(delegate { EnableListControls(); }));
            }));

            thread.IsBackground = true;
            thread.Name = "movie manager list populator";
            thread.Start();
        }

        private void EnableListControls() {
            movieListFilterBox.Enabled = true;
            movieListButton.Enabled = true;

            movieListBox.Enabled = true;
            if (movieListBox.Items.Count > 0)
                movieListBox.Items[0].Selected = true;
        }

        // adds the given movie and it's related files to the tree view
        private ListViewItem createMovieItem(DBMovieInfo movie) {
            ListViewItem newItem = new ListViewItem(movie.DisplayTitle);
            newItem.Tag = movie;
            
            // if the movie is offline color it red
            if (movie.LocalMedia.Count > 0) {
                if (!movie.LocalMedia[0].IsAvailable) {
                    newItem.ForeColor = Color.Red;
                    newItem.ToolTipText = "This movie is currently offline.";
                }
            }

            return newItem;
        }

        private void addMovie(DBMovieInfo movie) {
            Thread thread = new Thread(new ThreadStart(delegate() {
                addMovie(movie, createMovieItem(movie));
            }));
            thread.IsBackground = true;
            thread.Name = "add movie to moviemanager thread";
            thread.Start();
        }

        private void addMovie(DBMovieInfo movie, ListViewItem item) {
            if (InvokeRequired) {
                Invoke(new InvokeDelegate(delegate { addMovie(movie, item); }));
                return;
            }

            // only add it to the movie list if exists in current filter
            if (!IsMovieFiltered(movie)) {
                movieListBox.Items.Add(item);
            }

            this.movieCountField.Text = "" + movieListBox.Items.Count + " movies";

            lock (lockList) {
                listItems[movie] = item;
            }
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
            lock (lockList) {
                if (listItems.ContainsKey(movie)) {
                    listItems[movie].Selected = false;
                    updateMoviePanel();
                    updateFilePanel();

                    if (movieListBox.Items.Contains(listItems[movie]))
                        movieListBox.Items.Remove(listItems[movie]);

                    listItems.Remove(movie);
                }
            }
            
        }

        private void movieInsertedListener(DatabaseTable obj) {
            // if this is not a movie object, break
            if (obj.GetType() != typeof(DBMovieInfo))
                return;

            // This ensures we are thread safe. Makes sure this method is run by
            // the thread that created this panel.
            if (InvokeRequired) {
                if (!FindForm().Visible) return;
                Delegate method = new DatabaseManager.ObjectAffectedDelegate(movieInsertedListener);
                object[] parameters = new object[] { obj };
                this.Invoke(method, parameters);
                return;
            }

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
            lock (lockList) {
                if (reassigning)
                    listItems[(DBMovieInfo)obj].Selected = true;
            }

        }

        // if a dblocalmedia object is updated and it happens to belong to the currently
        // selected movie, and we are on the file details tab, we need to update.
        private void localMediaUpdatedListener(DatabaseTable obj) {
            if (!fileDetailsSubPane.Visible)
                return;

            DBLocalMedia file = obj as DBLocalMedia;
            if (file == null) return;

            // This ensures we are thread safe. Makes sure this method is run by
            // the thread that created this panel.
            if (InvokeRequired) {
                Delegate method = new DatabaseManager.ObjectAffectedDelegate(localMediaUpdatedListener);
                object[] parameters = new object[] { obj };
                this.Invoke(method, parameters);
                return;
            }

            if (file.AttachedMovies.Count > 0) {
                foreach (DBMovieInfo currMovie in file.AttachedMovies) {
                    if (CurrentMovie == currMovie) {
                        updateFilePanel();
                        return;
                    }
                }
            }
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
                    sendToImporterToolStripMenuItem.Enabled = false;
                    playMovieButton.Enabled = false;
                }
               
                return;
            }

            sendToImporterToolStripMenuItem.Enabled = true;
            advancedButton.Enabled = true;
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
            catch (Exception) {
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

            if (CurrentMovie.UserSettings[0].WatchedCount == 0) {
                watchedToggleButton.Visible = true;
                unwatchedToggleButton.Visible = false;
            }
            else {
                watchedToggleButton.Visible = false;
                unwatchedToggleButton.Visible = true;
            }
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
                    ImageLoadResults result = movie.AddCoverFromURL(popup.GetURL(), true);
                    stopArtProgressBar();

                    switch (result) {
                        case ImageLoadResults.SUCCESS:
                        case ImageLoadResults.SUCCESS_REDUCED_SIZE:
                            // set new cover to current and update screen
                            movie.CoverFullPath = movie.AlternateCovers[movie.AlternateCovers.Count - 1];
                            updateMoviePanel();
                            break;
                        case ImageLoadResults.FAILED_ALREADY_LOADED:
                            MessageBox.Show("Cover art from the specified URL has already been loaded.");
                            break;
                        case ImageLoadResults.FAILED:
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

            if (sender is ToolStripMenuItem) {
                selectedSource = (DBSourceInfo)((ToolStripMenuItem)sender).Tag;
                translate = ((ToolStripMenuItem)sender).Name == "TranslateItem";
            }
            else
                selectedSource = null;

            DialogResult result =
                MessageBox.Show("This action will overwrite existing movie details, including any custom modifications.",
                                "Refresh Movie", MessageBoxButtons.OKCancel);

            if (result == System.Windows.Forms.DialogResult.OK) {
                processingMovies = new List<DBMovieInfo>();
                foreach (ListViewItem currItem in movieListBox.SelectedItems)
                    processingMovies.Add((DBMovieInfo)currItem.Tag);

                if (movieListBox.SelectedItems.Count == 1) {
                    ProgressPopup popup = new ProgressPopup(new WorkerDelegate(refreshMovies));
                    popup.Owner = this.ParentForm;
                    popup.ShowDialog();
                }
                else {
                    ProgressPopup popup = new ProgressPopup(new TrackableWorkerDelegate(refreshMovies));
                    popup.Owner = this.ParentForm;
                    popup.ShowDialog();
                }

                updateMoviePanel();
                processingMovies.Clear();
            }
        }

        private void refreshMovies() {
            refreshMovies(null);
        }

        private void refreshMovies(ProgressDelegate progress) {
            float count = 0;
            float total = processingMovies.Count;
            int sentToImporter = 0;

            foreach (DBMovieInfo currItem in processingMovies) {
                count++;
                logger.Info("Refreshing movie #{0}: {1}", count, currItem.ToString());
                
                // if the user specified a specific source, try to update, and if failed
                // send to the importer
                if (selectedSource != null) {
                    
                    // make existing values overwritable
                    currItem.ProtectExistingValuesFromCopy(false); 
                    
                    UpdateResults result =  selectedSource.Provider.Update(currItem);
                    if (result == UpdateResults.FAILED_NEED_ID) {
                        currItem.ProtectExistingValuesFromCopy(false);
                        MovingPicturesCore.Importer.Update(currItem, selectedSource);
                        sentToImporter++;
                    }
                    else {
                        if (translate) currItem.Translate();
                    }
                }

                // just use the regular update logic
                else {
                    currItem.ProtectExistingValuesFromCopy(false); 
                    MovingPicturesCore.DataProviderManager.Update(currItem);
                }
                
                if (progress != null)
                    progress("", (int)(count*100/total));

                // delay a little so we dont hammer the webserver.
                Thread.Sleep(200);
                if (count % 5 == 0)
                    Thread.Sleep(1000);
            }

            if (sentToImporter > 0) {
                MessageBox.Show(
                "There were " + sentToImporter + " movie(s) that needed to be sent to the importer to\n" +
                "search for possible matches from the " + selectedSource.Provider.Name + " data source.\n" +
                "You will now be taken to the Movie Importer tab to check for\n" + 
                "any matches requiring approval.", "Movies Sent to Importer", MessageBoxButtons.OK);
                switchToImporter();
            }

        }

        private void sendToImporterToolStripMenuItem_Click(object sender, EventArgs e) {
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
                MovingPicturesCore.Importer.Reprocess(CurrentMovie);
                switchToImporter();
            }
        }

        private void switchToImporter() {
            if (InvokeRequired) {
                Invoke(new InvokeDelegate(switchToImporter));
                return;
            }

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
                
                // create movie list copy from dictionary
                processingMovies = listItems.Keys.ToList();

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

        private void unwatchedToggleButton_Click(object sender, EventArgs e) {
            foreach (ListViewItem currItem in movieListBox.SelectedItems) {
                ((DBMovieInfo)currItem.Tag).UserSettings[0].WatchedCount = 0;
                MovingPicturesCore.Follwit.UnwatchMovie((DBMovieInfo)currItem.Tag);
            }

            updateMoviePanel();
        }

        private void watchedToggleButton_Click(object sender, EventArgs e) {
            foreach (ListViewItem currItem in movieListBox.SelectedItems)
                if (((DBMovieInfo)currItem.Tag).UserSettings[0].WatchedCount == 0) {
                    ((DBMovieInfo)currItem.Tag).UserSettings[0].WatchedCount = 1;
                    MovingPicturesCore.Follwit.WatchMovie((DBMovieInfo)currItem.Tag, false);
                }

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

        private void updateMediaInfoToolStripMenuItem_Click(object sender, EventArgs e) {
            processingMovies = new List<DBMovieInfo>();
            foreach (ListViewItem currItem in movieListBox.SelectedItems)
                processingMovies.Add((DBMovieInfo)currItem.Tag);

            ProgressPopup popup = new ProgressPopup(new TrackableWorkerDelegate(updateMediaInfoWorker));
            popup.Owner = this.ParentForm;
            popup.ShowDialog();

            processingMovies.Clear();

            updateFilePanel();
        }

        private void updateMediaInfoWorker(ProgressDelegate progress) {
            int count = 0;
            bool mount = false;
            bool askMount = false;

            // if we are processing a batch ask about mounting images to get the mediainfo
            if (processingMovies.Count > 1) {
                askMount = true; // flag that we asked about mounting
                DialogResult result = MessageBox.Show("You are about to refresh MediaInfo for the selected movies. \n" +
                    "Do you want movies with disk images to be mounted for this information? \n" +
                    "Mounting disk images will make the update take a while longer. However \n" +                
                    "without mounting the MediaInfo will not be available untill playback.\n",
                    "Mount disk images?", MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Yes) mount = true;
                else if (result == DialogResult.Cancel) return;
            }

            foreach (DBMovieInfo currMovie in processingMovies) {
                foreach (DBLocalMedia localMedia in currMovie.LocalMedia) {
                    count++;

                    // if we are processing just one movie and we encounter a disk image ask (only once) if we should mount
                    // all disk images contained in this movie
                    if (processingMovies.Count == 1 && localMedia.IsImageFile && !askMount) {
                        askMount = true; // flag that we asked about mounting
                        DialogResult result = MessageBox.Show("You are about to refresh MediaInfo for the selected movie. \n" +
                            "This movie contains at least one disk image, do you want to mount it for this information? \n" +
                            "Mounting disk images will make the update take some more time. However \n" +
                            "without mounting the MediaInfo will not be available untill playback.\n",
                            "Mount disk images?", MessageBoxButtons.YesNo);

                        if (result == DialogResult.Yes) mount = true;
                    }
                    
                    progress("", (int)((count / (processingMovies.Count + 1.0))*100));
                    
                    UpdateMediaInfoResults updateResult = localMedia.UpdateMediaInfo(mount);

                    // if the media is not available, ask the user to retry/ignore (or abort the batch)
                    while (updateResult == UpdateMediaInfoResults.MediaNotAvailable) {
                        DialogResult retry = MessageBox.Show("MediaInfo could not be updated for " + currMovie.Title + " because the media is not available. Please insert the media and try again.", "Retry?", MessageBoxButtons.AbortRetryIgnore);
                        if (retry == DialogResult.Ignore) {
                            break;
                        }
                        else if (retry == DialogResult.Abort) {
                            return;
                        }
                        else {
                            updateResult = localMedia.UpdateMediaInfo(mount);
                        }
                    }

                    // if we are mounting images but the mounting failed, ask the user to retry/ignore (or abort the batch)
                    while (updateResult == UpdateMediaInfoResults.ImageFileNotMounted && mount) {
                        DialogResult retry = MessageBox.Show("MediaInfo could not be updated for " + currMovie.Title + " because mounting failed.", "Retry image mount?", MessageBoxButtons.AbortRetryIgnore);
                        if (retry == DialogResult.Ignore) {
                            break;
                        }
                        else if (retry == DialogResult.Abort) {
                            return;
                        }
                        else {
                            updateResult = localMedia.UpdateMediaInfo(mount);
                        }
                    }
                }
            }
        }

        private void updateTitleSortingMenuItem_Click(object sender, EventArgs e) {
            foreach (DBMovieInfo currItem in listItems.Keys.ToList()) {
                    currItem.PopulateSortBy();
            }

            updateMoviePanel();
            MessageBox.Show("Title sorting values (the \"Sort By\" field) have been updated.");
        }

        private void refreshMovieButton_DropDownOpening(object sender, EventArgs e) {
            addMovieRefreshMenuItems();
        }

        private void updateDateSortingMenuItem_Click(object sender, EventArgs e) {
            ProgressPopup popup = new ProgressPopup(new TrackableWorkerDelegate(updateDateSortingWorker));
            popup.Owner = this.ParentForm;
            popup.ShowDialog();

            updateMoviePanel();
        }

        private void updateDateSortingWorker(ProgressDelegate progress) {
            bool success = true;
            int count = 0;
            int total = listItems.Keys.Count;

            foreach (DBMovieInfo currItem in listItems.Keys.ToList()) {
                success = currItem.PopulateDateAdded() && success;
                count++;

                progress("", (int)((count / (total + 1.0)) * 100));
            }

            if (success)
                MessageBox.Show("Date sorting values have been updated.");
            else
                MessageBox.Show("Some movies could not be updated because they are offline.");
        }

        private void updateFileNameToolStripMenuItem_Click(object sender, EventArgs e) {
            if (movieListBox.SelectedItems.Count == 0)
                return;

            List<DBMovieInfo> movies = new List<DBMovieInfo>();
            foreach (ListViewItem currItem in movieListBox.SelectedItems)
                movies.Add((DBMovieInfo)currItem.Tag);

            RenameConfirmationPopup popup = new RenameConfirmationPopup(movies);
            popup.Owner = ParentForm;
            popup.ShowDialog(this);
        }

        //medthod to undo a file name change processed by the MovieFileRenamer class.
        private void returnToOriginalFileNameToolStripMenuItem_Click(object sender, EventArgs e) {
            if (CurrentMovie != null && !verifyAvailability())
                return;

            // If we made it this far all files are available and we can notify the user
            // about what the reassign process is going to do.
            DialogResult result = MessageBox.Show(
                    "You are about to revert the selected file or files to\n" +
                    "the original file and folder names.\n\n" +
                    "Are you sure you want to continue?",
                    "Rename Movie File", MessageBoxButtons.YesNo);

            if (result == System.Windows.Forms.DialogResult.Yes) {
                processingMovies = new List<DBMovieInfo>();
                foreach (ListViewItem currItem in movieListBox.SelectedItems)
                    processingMovies.Add((DBMovieInfo)currItem.Tag);

                ProgressPopup popup = new ProgressPopup(new TrackableWorkerDelegate(RevertRenamesWorker));
                popup.Owner = ParentForm;
                popup.ShowDialog();
            }
        }

        private bool verifyAvailability() {
            //do a check to see if the first file in this movie has been renamed. If not, halt.
            if (CurrentMovie.LocalMedia[0].OriginalFileName == String.Empty) {
                DialogResult noOriginal = MessageBox.Show(
                    "It is not possible to revert to the previous\n" +
                    "filename because the current movie has not been\n" +
                    "renamed.",
                    "Error", MessageBoxButtons.OK);
                return false;
            }

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
                                connect = "Cannot rename a file on a CD/DVD.";
                            else
                                connect = "Please reconnect the media labeled '" + localMedia.MediaLabel + "' to " + localMedia.DriveLetter;
                        }
                        else {
                            connect = "Please make sure the network share '" + localMedia.FullPath + "' is available.";
                        }

                        // Show dialog
                        DialogResult resultInsert = MessageBox.Show(
                        "The file or files you want to rename are currently not available to rename.\n\n" + connect,
                        "File(s) not available.", MessageBoxButtons.RetryCancel);

                        // if cancel is pressed stop the reassign process.
                        if (resultInsert == DialogResult.Cancel)
                            return false;

                        // break foreach loop (and recheck condition)
                        break;
                    }
                }
            }

            return true;
        }

        private void RevertRenamesWorker(ProgressDelegate progress) {
            MovieRenamer renamer = new MovieRenamer();

            int total = processingMovies.Count;
            int processed = 0;
            int percentDone = 0;

            foreach(DBMovieInfo currMovie in processingMovies) {
                percentDone = (int)Math.Round(100.0 * (processed) / total);

                bool available = currMovie.LocalMedia.IsAvailable();
                bool isDisk = currMovie.LocalMedia[0].ImportPath.IsOpticalDrive;

                if (available && !isDisk) {
                    progress("Reverting " + currMovie.Title + "...", percentDone);
                    renamer.Revert(currMovie);
                }

                processed++;
            }

            progress("Done!", 100);
        }

        private void ValidateSelectedFilterNode() {
            // check we have not removed all nodes
            // if we have reset to defaults
            if (((DBMenu<DBMovieInfo>)movieListFilterBox.Menu).RootNodes.Count == 0) {
                DatabaseMaintenanceManager.VerifyMovieManagerFilterMenu();
                movieListFilterBox.Menu = MovingPicturesCore.Settings.MovieManagerFilterMenu;
            }

            // get the current selected node id
            int? selectedNodeId = null;
            if (movieListFilterBox.SelectedNode as DBNode<DBMovieInfo> != null) {
                selectedNodeId = ((DBNode<DBMovieInfo>)movieListFilterBox.SelectedNode).ID;
            }
            
            // keep the current node selected if it still exists
            DBNode<DBMovieInfo> selectedNode = null;
            if (selectedNodeId != null) {
                selectedNode = MovingPicturesCore.DatabaseManager.Get<DBNode<DBMovieInfo>>((int)selectedNodeId);
                // the node still exists and is already selected
                if (selectedNode != null) return;
            }

            // select the first node if current one no longer exists
            // also trigger a re-load of the filtered movie list
            if (selectedNode == null) {
                movieListFilterBox.SelectedIndexChanged -= movieListFilterBox_SelectedIndexChanged;
                movieListFilterBox.SelectedNode = ((DBMenu<DBMovieInfo>)movieListFilterBox.Menu).RootNodes.First();
                movieListFilterBox.SelectedIndexChanged += movieListFilterBox_SelectedIndexChanged;
                ReloadList();
            }
        }

        private void movieListFilterBox_SelectedIndexChanged(object sender, EventArgs e) {
            // re-load list with new filter
            ReloadList();
        }

        private void movieListButton_Click(object sender, EventArgs e) {
            menuEditorPopup = new MenuEditorPopup();
            menuEditorPopup.MenuTree.FieldDisplaySettings.Table = typeof(DBMovieInfo);

            movieManagerFilterMenu = null;
            ProgressPopup loadingPopup = new ProgressPopup(new WorkerDelegate(LoadMovieManagerFiltersMenu));
            loadingPopup.Owner = FindForm();
            loadingPopup.Text = "Loading Menu...";
            loadingPopup.ShowDialog();

            menuEditorPopup.ShowMovieNodeSettings = false;
            menuEditorPopup.ShowDialog();
            menuEditorPopup = null;

            MovingPicturesCore.DatabaseManager.BeginTransaction();
            ProgressPopup savingPopup = new ProgressPopup(new WorkerDelegate(movieManagerFilterMenu.Commit));
            savingPopup.Owner = FindForm();
            savingPopup.Text = "Saving Menu...";
            savingPopup.ShowDialog();
            MovingPicturesCore.DatabaseManager.EndTransaction();

            // validate current selected filter node
            ValidateSelectedFilterNode();
        }

        private void LoadMovieManagerFiltersMenu() {
            // grab or create the menu object for movie manager filters
            string menuID = MovingPicturesCore.Settings.MovieManagerFilterMenuID;
            if (menuID == "null") {
                movieManagerFilterMenu = new DBMenu<DBMovieInfo>();
                movieManagerFilterMenu.Name = "Movie Manager Filters Menu";
                MovingPicturesCore.DatabaseManager.Commit(movieManagerFilterMenu);
                MovingPicturesCore.Settings.MovieManagerFilterMenuID = movieManagerFilterMenu.ID.ToString();
            }
            else {
                movieManagerFilterMenu = MovingPicturesCore.DatabaseManager.Get<DBMenu<DBMovieInfo>>(int.Parse(menuID));
            }

            menuEditorPopup.MenuTree.Menu = movieManagerFilterMenu;
        }

        private bool IsMovieFiltered(DBMovieInfo movie) {
            var node = movieListFilterBox.SelectedNode;
            if (node != null) {
                var nodeFilter = (node as DBNode<DBMovieInfo>).Filter;
                if (nodeFilter != null) {
                    return !nodeFilter.Filter(DBMovieInfo.GetAll()).ToList().Exists(m => m.Equals(movie));
                }
            }
            return false;
        }

        private void movieListBox_KeyDown(object sender, KeyEventArgs e) {
            // handle 'ctrl+a' to select all movies in listbox
            if (e.KeyCode == Keys.A && e.Control) {
                foreach (ListViewItem item in movieListBox.Items) {
                    item.Selected = true;
                }
            }
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
}
