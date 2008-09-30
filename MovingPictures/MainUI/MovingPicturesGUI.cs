using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Plugins.MovingPictures.ConfigScreen;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using System.Threading;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;
using System.Collections;
using System.Xml;
using System.Timers;
using MediaPortal.Plugins.MovingPictures.DataProviders;
using Cornerstone.Database.Tables;
using Cornerstone.Database;
using Cornerstone.Database.CustomTypes;
using MediaPortal.Util;

namespace MediaPortal.Plugins.MovingPictures {
    public class MovingPicturesGUI : GUIWindow {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public enum ViewMode { LIST, SMALLICON, LARGEICON, FILMSTRIP, FANART_FILMSTRIP, DETAILS }

        Dictionary<string, string> defines;
        Dictionary<DBMovieInfo, GUIListItem> listItemLookup;
        Dictionary<string, bool> loggedProperties;
        
        private bool currentlyPlaying = false;
        private int currentPart = 1;

        private bool loaded = false;

        private System.Timers.Timer updateArtworkTimer;

        #region GUI Controls

        [SkinControl(50)]
        protected GUIFacadeControl movieBrowser = null;

        [SkinControl(1)]
        protected GUIImage movieBackdrop = null;

        [SkinControl(2)]
        protected GUIButtonControl cycleViewButton = null;

        [SkinControl(3)]
        protected GUIButtonControl viewMenuButton = null;
        
        [SkinControl(4)]
        protected GUIButtonControl filterButton = null;
        
        [SkinControl(5)]
        protected GUIButtonControl settingsButton = null;

        [SkinControl(6)]
        protected GUIButtonControl playButton = null;

        [SkinControl(7)]
        protected GUIButtonControl textToggleButton = null;

        #endregion

        // Defines the current view mode. Reassign to switch modes.        
        public ViewMode CurrentView {
            get {
                return currentView;
            }

            set {
                // update the state variables
                if (currentView != value)
                    previousView = currentView;
                currentView = value;

                // if we are leaving details view, set focus back on the facade
                if (previousView == ViewMode.DETAILS) {
                    clearFocus();
                    movieBrowser.Focus = true;
                    movieBrowser.Visible = true;
                }

                switch (currentView) {
                    case ViewMode.LIST:
                        movieBrowser.View = GUIFacadeControl.ViewMode.List;
                        break;
                    case ViewMode.SMALLICON:
                        movieBrowser.View = GUIFacadeControl.ViewMode.SmallIcons;
                        break;
                    case ViewMode.LARGEICON:
                        movieBrowser.View = GUIFacadeControl.ViewMode.LargeIcons;
                        break;
                    case ViewMode.FILMSTRIP:
                        movieBrowser.View = GUIFacadeControl.ViewMode.Filmstrip;
                        break;
                    case ViewMode.DETAILS:
                        clearFocus();

                        movieBrowser.Visible = false;
                        movieBrowser.ListView.Visible = false;
                        movieBrowser.ThumbnailView.Visible = false;
                        movieBrowser.FilmstripView.Visible = false;

                        playButton.Focus = true;
                        break;
                }

                if (movieBrowser.SelectedListItem != null)
                    SelectedMovie = movieBrowser.SelectedListItem.TVTag as DBMovieInfo;

                updateArtwork();
            }
        } 
        private ViewMode currentView;
        private ViewMode previousView;

        // The currenty selected movie.
        public DBMovieInfo SelectedMovie {
            get {
                return selectedMovie;
            }

            set {
                if (selectedMovie == value)
                    return;

                if (selectedMovie != null)
                    logger.Debug("SelectedMovie changed: " + selectedMovie.Title);

                // load new data
                selectedMovie = value;
                selectedIndex = movieBrowser.SelectedListItemIndex;
                publishDetails(SelectedMovie, "SelectedMovie");

                // start the timer for new artwork loading
                updateArtworkTimer.Stop();
                updateArtworkTimer.Start();
            }
        }
        private DBMovieInfo selectedMovie;

        public int SelectedIndex {
            get {
                return selectedIndex;
            }
        }
        private int selectedIndex = 0;


        public MovingPicturesGUI() {
            selectedMovie = null;

            g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayBackStarted);
            g_Player.PlayBackEnded += new g_Player.EndedHandler(OnPlayBackEnded);

            // setup the timer for delayed artwork loading
            int artworkDelay = (int)MovingPicturesCore.SettingsManager["gui_artwork_delay"].Value;
            updateArtworkTimer = new System.Timers.Timer();
            updateArtworkTimer.Elapsed += new ElapsedEventHandler(OnUpdateArtworkTimerElapsed);
            updateArtworkTimer.Interval = artworkDelay;
            updateArtworkTimer.AutoReset = false;

            listItemLookup = new Dictionary<DBMovieInfo, GUIListItem>();
            loggedProperties = new Dictionary<string, bool>();
        }

        ~MovingPicturesGUI() {
        }

        // this timer creates a delay for loading background art. it waits a user defined
        // period of time (usually something like 200ms, THEN updates the backdrop. If the
        // user switches again *before* the timer expires, the timer is reset. THis allows
        // quick traversal on the GUI
        private void OnUpdateArtworkTimerElapsed(object sender, ElapsedEventArgs e) {
            updateArtwork();
        }

        private void updateArtwork() {
            if (SelectedMovie == null)
                return;

            string oldBackdrop = GUIPropertyManager.GetProperty("#MovingPictures.Backdrop");
            string oldCover = GUIPropertyManager.GetProperty("#MovingPictures.Coverart"); 

            // update backdrop and cover art
            setProperty("#MovingPictures.Backdrop", SelectedMovie.BackdropFullPath.Trim());
            setProperty("#MovingPictures.Coverart", SelectedMovie.CoverFullPath.Trim());

            // clear out previous textures for backdrop and cover art
            GUITextureManager.ReleaseTexture(oldBackdrop);
            GUITextureManager.ReleaseTexture(oldCover);

            updateBackdropVisibility();
        }

        private void updateBackdropVisibility() {
            bool backdropActive = true;

            // grab the skin supplied setting for backdrop visibility
            switch (CurrentView) {
                case ViewMode.FILMSTRIP:
                    backdropActive = defines["#filmstrip.backdrop.used"].Equals("true");
                    break;
                case ViewMode.LARGEICON:
                    backdropActive = defines["#largeicons.backdrop.used"].Equals("true");
                    break;
                case ViewMode.SMALLICON:
                    backdropActive = defines["#smallicons.backdrop.used"].Equals("true");
                    break;
                case ViewMode.LIST:
                    backdropActive = defines["#list.backdrop.used"].Equals("true");
                    break;
                case ViewMode.DETAILS:
                    backdropActive = defines["#details.backdrop.used"].Equals("true");
                    break;
            }

            // set backdrop visibility
            if (backdropActive && SelectedMovie != null && 
                SelectedMovie.BackdropFullPath.Trim().Length != 0)

                movieBackdrop.Visible = true;
            else
                movieBackdrop.Visible = false;
        }

        private bool isAvailable(ViewMode view) {
            switch (view) {
                case ViewMode.FILMSTRIP:
                    logger.Debug("FILMSTRIP: " + defines["#filmstrip.available"].Equals("true"));
                    return defines["#filmstrip.available"].Equals("true");
                case ViewMode.LARGEICON:
                    logger.Debug("LARGEICON: " + defines["#largeicons.available"].Equals("true"));
                    return defines["#largeicons.available"].Equals("true");
                case ViewMode.SMALLICON:
                    logger.Debug("SMALLICON: " + defines["#smallicons.available"].Equals("true"));
                    return defines["#smallicons.available"].Equals("true");
                case ViewMode.LIST:
                    logger.Debug("LIST: " + defines["#list.available"].Equals("true"));
                    return defines["#list.available"].Equals("true");
                case ViewMode.FANART_FILMSTRIP:
                    logger.Debug("FANART_FILMSTRIP: false");
                    return false;
                case ViewMode.DETAILS:
                    logger.Debug("DETAILS: " + defines["#filmstrip.available"].Equals("true"));
                    return true;
                default:
                    logger.Debug("DEFAULT: false");
                    return false;
            }
        }

        private void clearFocus() {
            if (movieBrowser != null) {
                movieBrowser.Focus = false;
                if (movieBrowser.ListView != null) movieBrowser.ListView.Focus = false;
                if (movieBrowser.ThumbnailView != null) movieBrowser.ThumbnailView.Focus = false;
                if (movieBrowser.FilmstripView != null) movieBrowser.FilmstripView.Focus = false;
            }

            if (cycleViewButton != null) cycleViewButton.Focus = false;
            if (viewMenuButton != null) viewMenuButton.Focus = false;
            if (filterButton != null) filterButton.Focus = false;
            if (settingsButton != null) settingsButton.Focus = false;
            if (playButton != null) playButton.Focus = false;
            if (textToggleButton != null) textToggleButton.Focus = false;
        }

        #region GUIWindow Members

        private void showMessage(string heading, string line1, string line2, string line3, string line4) {
            GUIDialogOK dialog = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
            dialog.Reset();
            dialog.SetHeading(heading);
            if (line1 != null) dialog.SetLine(1, line1);
            if (line2 != null) dialog.SetLine(2, line2);
            if (line3 != null) dialog.SetLine(3, line3);
            if (line4 != null) dialog.SetLine(4, line4);
            dialog.DoModal(GetID);

        }

        public override int GetID {
            get {
                return 96742;
            }
        }

        public override bool Init() {
            bool success;

            // initialize the moving pictures core services
            success = Load(GUIGraphicsContext.Skin + @"\movingpictures.xml");
            success = success && MovingPicturesCore.Initialize();
            
            // start the background importer
            MovingPicturesCore.Importer.Start();

            // grab any <define> tags from the skin for later use
            LoadDefinesFromSkin();

            // setup listeners for new or removed movies
            MovingPicturesCore.DatabaseManager.ObjectDeleted += new DatabaseManager.ObjectAffectedDelegate(OnMovieDeleted);
            MovingPicturesCore.DatabaseManager.ObjectInserted += new DatabaseManager.ObjectAffectedDelegate(OnMovieAdded);

            return success;
        }

        public override void DeInit() {
            base.DeInit();
            MovingPicturesCore.Shutdown();
        }

        protected override void OnPageLoad() {
            // if the component didn't load properly we probably have a bad skin file
            if (movieBrowser == null) {
                GUIDialogOK dialog = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                dialog.Reset();
                dialog.SetHeading("Sorry, there was a problem loading skin file...");
                dialog.DoModal(GetID);
                GUIWindowManager.ShowPreviousWindow();
                return;
            }

            // if the user hasn't defined any import paths they need to goto the config screen
            if (DBImportPath.GetAll().Count == 0) {
                GUIDialogOK dialog = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                dialog.Reset();
                dialog.SetHeading("No Import Paths!");
                dialog.SetLine(1, "It doesn't look like you have defined any");
                dialog.SetLine(2, "import paths in the configuration screen.");
                dialog.SetLine(3, "You shold close MediaPortal and launch the");
                dialog.SetLine(4, "MediaPortal Configuration Screen to configure");
                dialog.SetLine(5, "Moving Pictures.");
                dialog.DoModal(GetID);
                GUIWindowManager.ShowPreviousWindow();
                return;
            }

            // add movies to facade
            addAllMoviesToFacade();
            movieBrowser.Focus = true;
            movieBrowser.Sort(new GUIListItemMovieComparer());

            
            // if this is our first time loading, we need to setup our default view data
            if (!loaded) {
                loaded = true;

                // set the default view for the facade
                string defaultView = ((string)MovingPicturesCore.SettingsManager["default_view"].Value).Trim().ToLower();
                if (defaultView.Equals("list")) {
                    CurrentView = ViewMode.LIST;
                }
                else if (defaultView.Equals("thumbs")) {
                    CurrentView = ViewMode.SMALLICON;
                }
                else if (defaultView.Equals("largethumbs")) {
                    CurrentView = ViewMode.LARGEICON;
                }
                else if (defaultView.Equals("filmstrip")) {
                    CurrentView = ViewMode.FILMSTRIP;
                }
                else {
                    CurrentView = ViewMode.LIST;
                    logger.Warn("The DEFAULT_VIEW setting contains an invalid value. Defaulting to List View.");
                }
            }

            // if we have loaded before, lets update the facade selection to match our actual selection
            else {
                reloadSelection();
                ViewMode tmp = previousView;
                CurrentView = CurrentView;
                previousView = tmp;
            }

            // load fanart and coverart
            updateArtwork();
        }

        private void reloadSelection() {
            //Choose old value depending on type of view
            if (movieBrowser.PlayListView != null)
                movieBrowser.PlayListView.SelectedListItemIndex = SelectedIndex;

            if (movieBrowser.ListView != null)
                movieBrowser.ListView.SelectedListItemIndex = SelectedIndex;

            if (movieBrowser.AlbumListView != null)
                movieBrowser.AlbumListView.SelectedListItemIndex = SelectedIndex;

            if (movieBrowser.ThumbnailView != null)
                movieBrowser.ThumbnailView.SelectedListItemIndex = SelectedIndex;

            if (movieBrowser.FilmstripView != null) {
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, this.GetID, 0, movieBrowser.FilmstripView.GetID, SelectedIndex, 0, null);
                OnMessage(msg);
            }
        }

        private void OnMovieDeleted(DatabaseTable obj) {
            // if this is not a movie object, break
            if (obj.GetType() != typeof(DBMovieInfo))
                return;

            logger.Info("Removing " + ((DBMovieInfo)obj).Title + " from facade.");

            // remove movie from list
            DBMovieInfo movie = (DBMovieInfo)obj;
            removeMovieFromBrowser(movie);
            
            // if our selection has changed, update it
            if (SelectedMovie != movieBrowser.SelectedListItem.TVTag)
                SelectedMovie = movieBrowser.SelectedListItem.TVTag as DBMovieInfo;
        }

        private void OnMovieAdded(DatabaseTable obj) {
            // if this is not a movie object, break
            if (obj.GetType() != typeof(DBMovieInfo))
                return;

            // if this item is already in the list exit. This should never really happen though...
            if (listItemLookup.ContainsKey((DBMovieInfo)obj)) {
                logger.Warn("Received two \"added\" messages for " + (DBMovieInfo)obj);
                return;
            }

            logger.Info("Adding " + ((DBMovieInfo)obj).Title + " to facade.");

            // add movie to the list
            addMovieToBrowser((DBMovieInfo)obj);
        }

        private void addAllMoviesToFacade() {
            // initialize the facade
            movieBrowser.Clear();

            // populate the facade
            List<DBMovieInfo> movies = DBMovieInfo.GetAll();
            foreach (DBMovieInfo currMovie in movies) 
                addMovieToBrowser(currMovie);
            
        }

        private void addMovieToBrowser(DBMovieInfo newMovie) {
            // if we already created this item, assume we did a clear first and reuse
            // the same GUIListItem object
            if (listItemLookup.ContainsKey(newMovie)) {
                movieBrowser.Add(listItemLookup[newMovie]);
                movieBrowser.Sort(new GUIListItemMovieComparer());
                return;
            }
            
            // create a new list object
            GUIListItem currItem = new GUIListItem();
            currItem.Label = newMovie.Title;
            currItem.IconImage = newMovie.CoverThumbFullPath.Trim();
            currItem.IconImageBig = newMovie.CoverThumbFullPath.Trim();
            currItem.TVTag = newMovie;
            currItem.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(OnItemSelected);
            
            // and add
            movieBrowser.Add(currItem);
            movieBrowser.Sort(new GUIListItemMovieComparer());
            listItemLookup[newMovie] = currItem;
        }

        private void removeMovieFromBrowser(DBMovieInfo movie) {
            if (listItemLookup.ContainsKey(movie)) {

                // sadly there is no current way to remove an item from the facade.
                movieBrowser.Clear();
                addAllMoviesToFacade();
                movieBrowser.Sort(new GUIListItemMovieComparer());
            }
        }

        // Grabs the <define> tags from the skin for skin parameters from skinner.
        private void LoadDefinesFromSkin() {
            try {
                // Load the XML file
                XmlDocument doc = new XmlDocument();
                logger.Info("Loading defines from skin.");
                doc.Load(_windowXmlFileName);
                
                // parse out the define tags and store them
                defines = new Dictionary<string, string>();
                foreach (XmlNode node in doc.SelectNodes("/window/define")) {
                    string[] tokens = node.InnerText.Split(':');

                    if (tokens.Length < 2)
                        continue;

                    defines[tokens[0]] = tokens[1];
                    logger.Debug("Loaded define from skin: " + tokens[0] + ": " + tokens[1]);
                }
            }
            catch (Exception e) {
                logger.ErrorException("Unexpected error loading <define> tags from skin file.", e);
            }
        }


        protected override void OnPageDestroy(int new_windowId) {
            base.OnPageDestroy(new_windowId);
        }


        protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType) {
            switch (controlId) {
                // a click from the facade
                case 50:
                    bool clickToDetails = (bool)MovingPicturesCore.SettingsManager["click_to_details"].Value;

                    switch (actionType) {
                        case Action.ActionType.ACTION_PLAY:
                            playSelectedMovie();
                            break;
                        case Action.ActionType.ACTION_SELECT_ITEM:
                            if (control == movieBrowser) {
                                if (clickToDetails)
                                    CurrentView = ViewMode.DETAILS;
                                else
                                    playSelectedMovie();
                            }
                            break;
                    }
                    break;

                // a click on the rotate view button
                case 2:
                    cycleView();
                    break;

                // a click on the view menu button
                case 3:
                    showViewMenu();
                    break;

                // a click on the play button
                case 6:
                    playSelectedMovie();
                    break;
            }

            base.OnClicked(controlId, control, actionType);
        }

        private void showViewMenu() {
            IDialogbox dialog = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dialog.Reset();
            dialog.SetHeading("Views Menu");

            if (isAvailable(ViewMode.LIST)) {
                GUIListItem listItem = new GUIListItem("List View");
                listItem.ItemId = 1;
                dialog.Add(listItem);
            }

            if (isAvailable(ViewMode.SMALLICON)) {
                GUIListItem thumbItem = new GUIListItem("Thumbnails View");
                thumbItem.ItemId = 2;
                dialog.Add(thumbItem);
            }

            if (isAvailable(ViewMode.LARGEICON)) {
                GUIListItem largeThumbItem = new GUIListItem("Large Thumbnails View");
                largeThumbItem.ItemId = 3;
                dialog.Add(largeThumbItem);
            }

            if (isAvailable(ViewMode.FILMSTRIP)) {
                GUIListItem filmItem = new GUIListItem("Filmstrip View");
                filmItem.ItemId = 4;
                dialog.Add(filmItem);
            }

            dialog.DoModal(GUIWindowManager.ActiveWindow);
            switch (dialog.SelectedId) {
                case 1:
                    CurrentView = ViewMode.LIST;
                    break;
                case 2:
                    CurrentView = ViewMode.SMALLICON;
                    break;
                case 3:
                    CurrentView = ViewMode.LARGEICON;
                    break;
                case 4:
                    CurrentView = ViewMode.FILMSTRIP;
                    break;
            }
        }

        // rotates the current view to the next available
        private void cycleView() {
            if (CurrentView == ViewMode.DETAILS)
                return;

            ViewMode newView = CurrentView;

            do {
                // rotate view until one is available
                if (newView == ViewMode.FILMSTRIP)
                    newView = ViewMode.LIST;
                else
                    newView++;

                logger.Debug("trying " + newView.ToString());

            } while (!isAvailable(newView));

            previousView = CurrentView;
            CurrentView = newView;
        }



        private void playSelectedMovie() {
            playSelectedMovie(1);
        }

        private void playSelectedMovie(int part) {
            if (SelectedMovie == null)
                SelectedMovie = movieBrowser.SelectedListItem.TVTag as DBMovieInfo;

            DBLocalMedia localMediaToPlay = SelectedMovie.LocalMedia[part - 1];

            // check for removable
            if (!localMediaToPlay.File.Exists && localMediaToPlay.ImportPath.Removable) {
                showMessage("Removable Media Not Available",
                            "The media for the Movie you have selected is not",
                            "currently available. Please insert or connect this",
                            "media source and try again.", null);
                return;
            }
            else if (!localMediaToPlay.File.Exists) {
                showMessage("Error",
                            "The media for the Movie you have selected is missing!",
                            "Very sorry but something has gone wrong...", null, null);
                return;
            }

            // grab media info
            string media = SelectedMovie.LocalMedia[part - 1].FullPath;
            string ext = SelectedMovie.LocalMedia[part - 1].File.Extension;
            
            // check if the current media is an image file
            if (DaemonTools.IsImageFile(ext))
                playImage(media);
            else
                playFile(media);

            // set the currently playign part if playback was successful
            if (currentlyPlaying)
                currentPart = part;
        }


        // start playback of a regular file
        private void playFile(string media) {
            GUIGraphicsContext.IsFullScreenVideo = true;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);

            bool success = g_Player.Play(media, g_Player.MediaType.Video);
            currentlyPlaying = success;
        }


        // start playback of an image file (ISO)
        private void playImage(string media) {
            string drive;
            bool alreadyMounted;

            // Check if the current image is already mounted
            if (!DaemonTools.IsMounted(media)) {
                // if not try to mount the image
                logger.Info("Trying to mount image.");
                alreadyMounted = false;
                if (!DaemonTools.Mount(media, out drive)) {
                    showMessage("Error", "Sorry, failed mounting DVD Image...", null, null, null);
                    return;
                }
            }
            else {
                logger.Info("DVD Image already mounted.");
                drive = DaemonTools.GetVirtualDrive();
                alreadyMounted = true;
            }

            // try to grab a DVD IFO file off the newly mounted drive, and check if it exists
            string ifoPath = drive + @"\VIDEO_TS\VIDEO_TS.IFO";
            if (!System.IO.File.Exists(media)) {
                showMessage("Error", "The image file does not contain an DVD!", null, null, null);
                return;
            }

            // if this image was already mounted, autoplay doesn't work, so manually launch the DVD
            if (alreadyMounted)
                playFile(ifoPath);

            // if we have not already mounted and we let the auto play system start the movie
            // this data could be wrong because the user could chose not to play via the prmpt.
            // this should be handled in the future. possible bug.
            currentlyPlaying = true;
            return;
        }

       
        protected override void OnShowContextMenu() {
            base.OnShowContextMenu();
            switch (CurrentView) {
                case ViewMode.DETAILS:
                    showDetailsContext();
                    break;
                default:
                    showMainContext();
                    break;
            }

            base.OnShowContextMenu();
        }

        private void showDetailsContext() {
            IDialogbox dialog = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dialog.Reset();
            dialog.SetHeading("Moving Pictures");

            GUIListItem detailsItem = new GUIListItem("Update Details from Online");
            detailsItem.ItemId = 1;
            dialog.Add(detailsItem);

            if (SelectedMovie.AlternateCovers.Count > 1) {
                GUIListItem cycleArtItem = new GUIListItem("Cycle Cover-Art");
                cycleArtItem.ItemId = 2;
                dialog.Add(cycleArtItem);
            }

            if (selectedMovie.CoverFullPath.Trim().Length == 0 ||
                selectedMovie.BackdropFullPath.Trim().Length == 0) {
                GUIListItem retrieveArtItem = new GUIListItem("Check for Missing Artwork Online");
                retrieveArtItem.ItemId = 3;
                dialog.Add(retrieveArtItem);
            }

            dialog.DoModal(GUIWindowManager.ActiveWindow);
            switch (dialog.SelectedId) {
                case 1:
                    updateDetails();
                    break;
                case 2:
                    SelectedMovie.NextCover();
                    SelectedMovie.Commit();
                    updateArtwork();
                    break;
                case 3:
                    retrieveMissingArt();
                    break;
            }
        }

        private void retrieveMissingArt() {
            if (SelectedMovie == null)
                return;

            if (SelectedMovie.CoverFullPath.Trim().Length == 0) {
                MovingPicturesCore.DataProviderManager.GetArtwork(SelectedMovie);
                SelectedMovie.UnloadArtwork();
                SelectedMovie.Commit();
            }

            if (SelectedMovie.BackdropFullPath.Trim().Length == 0) {
                new LocalProvider().GetBackdrop(SelectedMovie);
                MovingPicturesCore.DataProviderManager.GetBackdrop(SelectedMovie);
                SelectedMovie.Commit();
            }

            updateArtwork();
        }

        private void updateDetails() {
            GUIDialogYesNo dialog = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (dialog == null)
                return;

            dialog.Title = "Update Movie Details";
            dialog.SetLine(1, "You are about to refresh all movie metadata, overwriting");
            dialog.SetLine(2, "any custom modifications to this film. Do you want");
            dialog.SetLine(3, "to continue?");
            dialog.SetDefaultToYes(false);
            dialog.DoModal(GetID);

            if (dialog.IsConfirmed && SelectedMovie != null) {
                MovingPicturesCore.DataProviderManager.Update(SelectedMovie);
                SelectedMovie.Commit();
                publishDetails(SelectedMovie, "SelectedMovie");
            }
            

        }

        private void showMainContext() {
            IDialogbox dialog = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dialog.Reset();
            dialog.SetHeading("Moving Pictures");

            GUIListItem listItem = new GUIListItem("List View");
            listItem.ItemId = 1;
            dialog.Add(listItem);

            GUIListItem thumbItem = new GUIListItem("Thumbnail View");
            thumbItem.ItemId = 2;
            dialog.Add(thumbItem);

            GUIListItem largeThumbItem = new GUIListItem("Large Thumbnail View");
            largeThumbItem.ItemId = 3;
            dialog.Add(largeThumbItem);

            GUIListItem filmItem = new GUIListItem("Filmstrip View");
            filmItem.ItemId = 4;
            dialog.Add(filmItem);

            dialog.DoModal(GUIWindowManager.ActiveWindow);
            switch (dialog.SelectedId) {
                case 1:
                    CurrentView = ViewMode.LIST;
                    break;
                case 2:
                    CurrentView = ViewMode.SMALLICON;
                    break;
                case 3:
                    CurrentView = ViewMode.LARGEICON;
                    break;
                case 4:
                    CurrentView = ViewMode.FILMSTRIP;
                    break;
            }
 
        }

        public override void OnAction(Action action) {
            switch (action.wID) {
                case Action.ActionType.ACTION_PARENT_DIR:
                case Action.ActionType.ACTION_HOME:
                    currentlyPlaying = false;
                    GUIWindowManager.ShowPreviousWindow();
                    break;
                case Action.ActionType.ACTION_PREVIOUS_MENU:
                    if (CurrentView == ViewMode.DETAILS)
                        CurrentView = previousView;
                    else {
                        currentlyPlaying = false;
                        GUIWindowManager.ShowPreviousWindow();
                    }
                    break;
                case Action.ActionType.ACTION_MUSIC_PLAY:
                    // yes, the generic "play" action is called.... ACTION_MUSIC_PLAY...
                    playSelectedMovie();
                    break;
                case Action.ActionType.ACTION_MOVE_RIGHT:
                case Action.ActionType.ACTION_MOVE_LEFT:
                case Action.ActionType.ACTION_MOVE_UP:
                default:
                    base.OnAction(action);
                    break;
            }
        }

        public override bool OnMessage(GUIMessage message) {
              return base.OnMessage(message);
        }

        private void OnItemSelected(GUIListItem item, GUIControl parent) {
            // if this is not a message from the facade, exit
            if (parent != movieBrowser && parent != movieBrowser.FilmstripView &&
                parent != movieBrowser.ThumbnailView && parent != movieBrowser.ListView)
                return;
            

            // if we already are on this movie, exit
            if (SelectedMovie == item.TVTag)
                return;

            SelectedMovie = item.TVTag as DBMovieInfo;
        }


        private void OnPlayBackStarted(MediaPortal.Player.g_Player.MediaType type, string filename) {
            if (SelectedMovie != null && currentlyPlaying) {
                Thread newThread = new Thread(new ThreadStart(UpdatePlaybackInfo));
                newThread.Start();
            }
        }

        private void OnPlayBackEnded(MediaPortal.Player.g_Player.MediaType type, string filename) {
            if (SelectedMovie != null && currentlyPlaying) {
                if (SelectedMovie.LocalMedia.Count > 1 && SelectedMovie.LocalMedia.Count <= currentPart + 1) {
                    currentPart++;
                    playSelectedMovie(currentPart);
                }
                else {
                    currentPart = 0;
                    currentlyPlaying = false;
                }
            }
        }

        // Updates the movie metadata on the playback screen (for when the user clicks info). 
        // The delay is neccisary because Player tries to use metadata from the MyVideos database.
        // We want to update this after that happens so the correct info is there.
        private void UpdatePlaybackInfo() {
            Thread.Sleep(2000);
            if (SelectedMovie != null) {
                setProperty("#Play.Current.Title", SelectedMovie.Title);
                setProperty("#Play.Current.Genre", SelectedMovie.Genres[0]);
                setProperty("#Play.Current.Plot", SelectedMovie.Summary);
                setProperty("#Play.Current.Thumb", SelectedMovie.CoverThumbFullPath);
                setProperty("#Play.Current.Year", SelectedMovie.Year.ToString());
            }
        }

        #endregion

        // this does standard object publishing for any database object.
        private void publishDetails(DatabaseTable obj, string prefix) {
            int maxStringListElements = (int) MovingPicturesCore.SettingsManager["max_string_list_items"].Value;

            Type tableType = obj.GetType();
            foreach (DBField currField in DBField.GetFieldList(tableType)) {
                string propertyStr;
                string valueStr;
                
                // for string lists, lets do some nice formating
                object value = currField.GetValue(obj);
                if (value.GetType() == typeof(StringList)) {
                   
                    // make sure we dont go overboard with listing elements :P
                    StringList valueStrList = (StringList) value;
                    int max = maxStringListElements;
                    if (max > valueStrList.Count)
                        max = valueStrList.Count;

                    // add the coma seperated string
                    propertyStr = "#MovingPictures." + prefix + "." + currField.FieldName;
                    valueStr = valueStrList.ToPrettyString(max);
                    setProperty(propertyStr, valueStr);

                    // add each value individually
                    for (int i = 0; i < max; i++) {
                        // note, the "extra" in the middle is needed due to a bug in skin parser
                        propertyStr = "#MovingPictures." + prefix + ".extra." + currField.FieldName + "." + (i + 1);
                        valueStr = valueStrList[i];
                        setProperty(propertyStr, valueStr);
                    }
                
                // vanilla publication
                } else {
                    propertyStr = "#MovingPictures." + prefix + "." + currField.FieldName;
                    valueStr = currField.GetValue(obj).ToString().Trim();
                    setProperty(propertyStr, valueStr);
                }
            }

            publishBonusDetails(obj, prefix);
        }

        // publishing for special fields not specifically in the database
        private void publishBonusDetails(DatabaseTable obj, string prefix) {
            string propertyStr;
            string valueStr;

            if (obj is DBMovieInfo) {
                DBMovieInfo movie = (DBMovieInfo)obj;
                int minValue;
                int hourValue;

                // hour component of runtime
                propertyStr = "#MovingPictures." + prefix + ".extra.runtime.hour";
                hourValue = (movie.Runtime / 60);
                setProperty(propertyStr, hourValue.ToString());

                // minute component of runtime
                propertyStr = "#MovingPictures." + prefix + ".extra.runtime.minute";
                minValue = (movie.Runtime % 60);
                setProperty(propertyStr, minValue.ToString());

                // give short runtime string 0:00
                propertyStr = "#MovingPictures." + prefix + ".extra.runtime.short";
                valueStr = string.Format("{0}:{1:00}", hourValue, minValue);
                setProperty(propertyStr, valueStr);

                // give pretty runtime string
                propertyStr = "#MovingPictures." + prefix + ".extra.runtime.en.pretty";
                valueStr = string.Empty;
                if (hourValue > 0) 
                    valueStr = string.Format("{0} hour{1}", hourValue, hourValue != 1 ? "s" : string.Empty);
                if (minValue > 0) 
                    valueStr = valueStr + string.Format(", {0} minute{1}", minValue, minValue != 1 ? "s" : string.Empty);
                setProperty(propertyStr, valueStr);
            }
        }

        private void setProperty(string property, string value) {
            if (!loggedProperties.ContainsKey(property)) {
                logger.Debug(property + " = \"" + value + "\"");
                loggedProperties[property] = true;
            }

            // If the value is empty always add a space
            // otherwise the property will keep 
            // displaying it's previous value
            if (String.IsNullOrEmpty(value))
              value = " ";
            
            GUIPropertyManager.SetProperty(property, value);
        }
    }


    public class GUIListItemMovieComparer : IComparer<GUIListItem> {
        public int Compare(GUIListItem x, GUIListItem y) {
            try {
                DBMovieInfo movieX = (DBMovieInfo)x.TVTag;
                DBMovieInfo movieY = (DBMovieInfo)y.TVTag;

                return movieX.SortBy.CompareTo(movieY.SortBy);
            } catch {
                return 0;
            }
        }
    }
}
