using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Plugins.MovingPictures.ConfigScreen;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using System.Threading;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;
using System.Collections;
using System.Xml;
using MediaPortal.Plugins.MovingPictures.Database.CustomTypes;

namespace MediaPortal.Plugins.MovingPictures {
    public class MovingPicturesGUI : GUIWindow {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public enum ViewMode { LIST, SMALLICON, LARGEICON, FILMSTRIP, FANART_FILMSTRIP, DETAILS }

        Dictionary<string, string> defines;
        private bool currentlyPlaying = false;
        private int currentPart = 1;

        [SkinControl(50)]
        protected GUIFacadeControl movieBrowser = null;

        [SkinControl(1)]
        protected GUIImage movieBackdrop = null;

        [SkinControl(6)]
        protected GUIButtonControl playButton = null;

        
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

                switch (currentView) {
                    case ViewMode.LIST:
                        movieBrowser.Focus = true;
                        movieBrowser.View = GUIFacadeControl.ViewMode.List;
                        movieBrowser.Visible = true;
                        break;
                    case ViewMode.SMALLICON:
                        movieBrowser.Focus = true;
                        movieBrowser.View = GUIFacadeControl.ViewMode.SmallIcons;
                        movieBrowser.Visible = true;
                        break;
                    case ViewMode.LARGEICON:
                        movieBrowser.Focus = true;
                        movieBrowser.View = GUIFacadeControl.ViewMode.LargeIcons;
                        movieBrowser.Visible = true;
                        break;
                    case ViewMode.FILMSTRIP:
                        movieBrowser.Focus = true;
                        movieBrowser.View = GUIFacadeControl.ViewMode.Filmstrip;
                        movieBrowser.Visible = true;
                        break;
                    case ViewMode.DETAILS:
                        this.GetFocusControlId();
                        movieBrowser.Focus = false;
                        movieBrowser.ListView.Focus = false;
                        movieBrowser.ThumbnailView.Focus = false;
                        movieBrowser.FilmstripView.Focus = false;

                        movieBrowser.Visible = false;
                        movieBrowser.ListView.Visible = false;
                        movieBrowser.ThumbnailView.Visible = false;
                        movieBrowser.FilmstripView.Visible = false;

                        playButton.Focus = true;
                        break;
                }

                if (movieBrowser.SelectedListItem != null)
                    SelectedMovie = movieBrowser.SelectedListItem.TVTag as DBMovieInfo;

                updateBackdropVisibility();
            }
        } 
        private ViewMode currentView = ViewMode.LIST;
        private ViewMode previousView = ViewMode.LIST;

        // The currenty selected movie.
        public DBMovieInfo SelectedMovie {
            get {
                return selectedMovie;
            }

            set {
                DBMovieInfo previousMovie = selectedMovie;

                // load new data
                selectedMovie = value;
                updateBackdropVisibility();
                publishDetails(SelectedMovie, "SelectedMovie");

                if (previousMovie != null) {
                    // clear out old fanart and coverart
                    GUITextureManager.ReleaseTexture(previousMovie.CoverFullPath);
                    GUITextureManager.ReleaseTexture(previousMovie.BackdropFullPath);
                }
            }
        }
        private DBMovieInfo selectedMovie;

        public MovingPicturesGUI() {
            selectedMovie = null;

            g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayBackStarted);
            g_Player.PlayBackEnded += new g_Player.EndedHandler(OnPlayBackEnded);
        }

        ~MovingPicturesGUI() {
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

        private void updateBackdropVisibility() {
            bool backdropActive = true;
            
            // grab the skin supplied setting for backdrop visibility
            switch (movieBrowser.View) {
                case GUIFacadeControl.ViewMode.Filmstrip:
                    backdropActive = defines["#filmstrip.backdrop.used"].Equals("true");
                    break;
                case GUIFacadeControl.ViewMode.LargeIcons:
                    backdropActive = defines["#largeicons.backdrop.used"].Equals("true");
                    break;
                case GUIFacadeControl.ViewMode.SmallIcons:
                    backdropActive = defines["#smallicons.backdrop.used"].Equals("true");
                    break;
                case GUIFacadeControl.ViewMode.List:
                    backdropActive = defines["#list.backdrop.used"].Equals("true");
                    break;
            }

            // set backdrop visibility
            if (backdropActive && SelectedMovie != null && SelectedMovie.BackdropFullPath.Trim().Length != 0)
                movieBackdrop.Visible = true;
            else
                movieBackdrop.Visible = false;
        }
        
        #region GUIWindow Members

        private void showMessage(string message) {
            GUIDialogOK dialog = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
            dialog.Reset();
            dialog.SetHeading(message);
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
                dialog.SetHeading("Problem loading skin file...");
                dialog.DoModal(GetID);
                GUIWindowManager.ShowPreviousWindow();
                return;
            }

            LoadDefinesFromSkin();

            // initialize the facade
            movieBrowser.Clear();
            movieBrowser.Focus = true;

            // populate the facade
            List<DBMovieInfo> movies = DBMovieInfo.GetAll();
            foreach (DBMovieInfo currMovie in movies) {
                GUIListItem currItem = new GUIListItem();
                currItem.Label = currMovie.Title;
                currItem.IconImage = currMovie.CoverThumbFullPath.Trim();
                currItem.IconImageBig = currMovie.CoverThumbFullPath.Trim();
                currItem.TVTag = currMovie;
                currItem.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(OnItemSelected);
                movieBrowser.Add(currItem);
            }

            movieBrowser.Sort(new GUIListItemMovieComparer());

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

            // play the movie! 
            GUIGraphicsContext.IsFullScreenVideo = true;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
            bool success = g_Player.Play(SelectedMovie.LocalMedia[part - 1].FullPath, g_Player.MediaType.Video);
            currentlyPlaying = success;

            if (currentlyPlaying)
                currentPart = 1;
        }

        protected override void OnShowContextMenu() {
            base.OnShowContextMenu();
        }

        public override void OnAction(Action action) {
            logger.Debug(action.wID.ToString());
            switch (action.wID) {
                case Action.ActionType.ACTION_PARENT_DIR:
                case Action.ActionType.ACTION_HOME:
                    GUIWindowManager.ShowPreviousWindow();
                    break;
                case Action.ActionType.ACTION_PREVIOUS_MENU:
                    if (CurrentView == ViewMode.DETAILS)
                        CurrentView = previousView;
                    else
                        GUIWindowManager.ShowPreviousWindow();
                    break;
                case Action.ActionType.ACTION_MUSIC_PLAY:
                    // yes, the generic "play" action is called.... ACTION_MUSIC_PLAY...
                    playSelectedMovie();
                    break;
                case Action.ActionType.ACTION_MOVE_RIGHT:
                case Action.ActionType.ACTION_MOVE_LEFT:
                case Action.ActionType.ACTION_MOVE_UP:
                case Action.ActionType.ACTION_MOVE_DOWN:
                    /*
                    if (CurrentView == ViewMode.FILMSTRIP && movieBrowser.FilmstripView.IsFocused)
                        SelectedMovie = movieBrowser.SelectedListItem.TVTag as DBMovieInfo;
                     */
                    base.OnAction(action);
                    break;
                default:
                    base.OnAction(action);
                    break;
            }
        }

        public override bool OnMessage(GUIMessage message) {
            logger.Debug(message.Message.ToString());
            /*
            if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED) {
                switch (message.SenderControlId) {
                    case 50:
                        SelectedMovie = movieBrowser.SelectedListItem.TVTag as DBMovieInfo;
                        break;
                }
            }
            */
            return base.OnMessage(message);
        }

        private void OnItemSelected(GUIListItem item, GUIControl parent) {
            logger.Debug("OnItemSelected"); 
            if (parent == movieBrowser || parent == movieBrowser.FilmstripView || 
                parent == movieBrowser.ThumbnailView || parent == movieBrowser.ListView)
                SelectedMovie = item.TVTag as DBMovieInfo;
        }


        private void OnPlayBackStarted(MediaPortal.Player.g_Player.MediaType type, string filename) {
            if (SelectedMovie != null && currentlyPlaying) {
                Thread newThread = new Thread(new ThreadStart(UpdatePlaybackInfo));
                newThread.Start();
            }
        }

        private void OnPlayBackEnded(MediaPortal.Player.g_Player.MediaType type, string filename) {
            if (SelectedMovie.LocalMedia.Count > 1 && SelectedMovie.LocalMedia.Count <= currentPart + 1) {
                currentPart++;
                playSelectedMovie(currentPart);
            }
            else {
                currentPart = 0;
                currentlyPlaying = false;
            }
        }

        // Updates the movie metadata on the playback screen (for when the user clicks info). 
        // The delay is neccisary because Player tries to use metadata from the MyVideos database.
        // We want to update this after that happens so the correct info is there.
        private void UpdatePlaybackInfo() {
            Thread.Sleep(2000);
            if (SelectedMovie != null) {
                MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Title", SelectedMovie.Title);
                MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Genre", SelectedMovie.Genres[0]);
                MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Plot", SelectedMovie.Summary);
                MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Thumb", SelectedMovie.CoverThumbFullPath);
                MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Year", SelectedMovie.Year.ToString());
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
                    GUIPropertyManager.SetProperty(propertyStr, valueStr);
                    logger.Debug(propertyStr + " = \"" + valueStr + "\"");

                    // add each value individually
                    for (int i = 0; i < max; i++) {
                        // note, the "extra" in the middle is needed due to a bug in skin parser
                        propertyStr = "#MovingPictures." + prefix + ".extra." + currField.FieldName + "." + (i + 1);
                        valueStr = valueStrList[i];
                        GUIPropertyManager.SetProperty(propertyStr, valueStr);
                        logger.Debug(propertyStr + " = \"" + valueStr + "\"");
                    }
                
                // vanilla publication
                } else {
                    propertyStr = "#MovingPictures." + prefix + "." + currField.FieldName;
                    valueStr = currField.GetValue(obj).ToString().Trim();
                    GUIPropertyManager.SetProperty(propertyStr, valueStr);
                    logger.Debug(propertyStr + " = \"" + valueStr + "\"");
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

                // hour component of runtime
                propertyStr = "#MovingPictures." + prefix + ".extra.runtime.hour";
                valueStr = (movie.Runtime / 60).ToString();
                GUIPropertyManager.SetProperty(propertyStr, valueStr);
                logger.Debug(propertyStr + " = \"" + valueStr + "\"");

                // minute component of runtime
                propertyStr = "#MovingPictures." + prefix + ".extra.runtime.minute";
                valueStr = (movie.Runtime % 60).ToString();
                GUIPropertyManager.SetProperty(propertyStr, valueStr);
                logger.Debug(propertyStr + " = \"" + valueStr + "\"");

            }
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
