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

namespace MediaPortal.Plugins.MovingPictures {
    public class MovingPicturesGUI : GUIWindow {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public enum ViewMode { LIST, SMALLICON, LARGEICON, FILMSTRIP, FANART_FILMSTRIP, DETAILS }

        Dictionary<string, string> defines;
        private DBMovieInfo selectedMovie;
        private bool playingMovie = false;

        [SkinControl(50)]
        protected GUIFacadeControl movieBrowser = null;

        [SkinControl(1)]
        protected GUIImage movieBackdrop = null;

        
        // Defines the current view mode. Reassign to switch modes.        
        public ViewMode? CurrentView {
            get {
                return currentView;
            }

            set {
                if (value == null)
                    return;

                // update the state variables
                if (currentView != value)
                    previousView = currentView;
                currentView = value;
                
                switch (currentView) {
                    case ViewMode.LIST:
                        movieBrowser.View = GUIFacadeControl.ViewMode.List;
                        movieBrowser.Visible = true;
                        break;
                    case ViewMode.SMALLICON:
                        movieBrowser.View = GUIFacadeControl.ViewMode.SmallIcons;
                        movieBrowser.Visible = true;
                        break;
                    case ViewMode.LARGEICON:
                        movieBrowser.View = GUIFacadeControl.ViewMode.LargeIcons;
                        movieBrowser.Visible = true;
                        break;
                    case ViewMode.FILMSTRIP:
                        movieBrowser.View = GUIFacadeControl.ViewMode.Filmstrip;
                        movieBrowser.Visible = true;
                        break;
                    case ViewMode.DETAILS:
                        movieBrowser.Visible = false;
                        movieBrowser.ListView.Visible = false;
                        movieBrowser.ThumbnailView.Visible = false;
                        movieBrowser.FilmstripView.Visible = false;
                        break;
                }

                updateBackdropVisibility();
            }
        } 
        private ViewMode? currentView = null;
        private ViewMode? previousView = null;

        public MovingPicturesGUI() {
            selectedMovie = null;

            g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayBackStarted);
            g_Player.PlayBackEnded += new g_Player.EndedHandler(OnPlayBackEnded);
        }

        ~MovingPicturesGUI() {
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
            if (backdropActive && selectedMovie != null && selectedMovie.BackdropFullPath.Trim().Length != 0)
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

            // set the default view for the facade
            string defaultView = ((string)MovingPicturesCore.SettingsManager["default_view"].Value).Trim().ToLower();
            if (defaultView.Equals("list")) {
                CurrentView = ViewMode.LIST;
            } else if (defaultView.Equals("thumbs")) {
                CurrentView = ViewMode.SMALLICON;
            } else if (defaultView.Equals("largethumbs")) {
                CurrentView = ViewMode.LARGEICON;
            } else if (defaultView.Equals("filmstrip")) {
                CurrentView = ViewMode.FILMSTRIP;
            } else {
                CurrentView = ViewMode.LIST;
                logger.Warn("The DEFAULT_VIEW setting contains an invalid value. Defaulting to List View.");
            }

            // populate the facade
            List<DBMovieInfo> movies = DBMovieInfo.GetAll();
            foreach (DBMovieInfo currMovie in movies) {
                GUIListItem currItem = new GUIListItem();
                currItem.Label = currMovie.Title;
                currItem.IconImage = currMovie.CoverThumbFullPath.Trim();
                currItem.IconImageBig = currMovie.CoverThumbFullPath.Trim();
                currItem.TVTag = currMovie;
                movieBrowser.Add(currItem);
            }

            movieBrowser.Sort(new GUIListItemMovieComparer());
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

            base.OnClicked(controlId, control, actionType);
        }

        private void playSelectedMovie() {
            if (selectedMovie == null)
                selectedMovie = movieBrowser.SelectedListItem.TVTag as DBMovieInfo;

            // play the movie! 
            GUIGraphicsContext.IsFullScreenVideo = true;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
            bool success = g_Player.Play(selectedMovie.LocalMedia[0].FullPath, g_Player.MediaType.Video);
            playingMovie = success;
        }

        protected override void OnShowContextMenu() {
            base.OnShowContextMenu();

            IDialogbox dialog = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dialog.Reset();
            dialog.SetHeading("Context Menu");

            GUIListItem listItem = new GUIListItem("List");
            listItem.ItemId = 1;
            dialog.Add(listItem);

            GUIListItem thumbItem = new GUIListItem("Thumbnail");
            thumbItem.ItemId = 2;
            dialog.Add(thumbItem);

            GUIListItem largeThumbItem = new GUIListItem("Large Thumbnails");
            largeThumbItem.ItemId = 3;
            dialog.Add(largeThumbItem);

            GUIListItem filmItem = new GUIListItem("Filmstrip");
            filmItem.ItemId = 4;
            dialog.Add(filmItem);

            GUIListItem detailsItem = new GUIListItem("Details");
            detailsItem.ItemId = 5;
            dialog.Add(detailsItem);

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
                case 5:
                    CurrentView = ViewMode.DETAILS;
                    break;
            }
        }

        public override void OnAction(Action action) {
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
                default:
                    base.OnAction(action);
                    break;
            }
        }

        public override bool OnMessage(GUIMessage message) {
            if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED) {
                switch (message.SenderControlId) {
                    case 50:
                        if (selectedMovie != null) {
                            // clear out old fanart and coverart
                            GUITextureManager.ReleaseTexture(selectedMovie.CoverFullPath);
                            GUITextureManager.ReleaseTexture(selectedMovie.CoverThumbFullPath);
                            GUITextureManager.ReleaseTexture(selectedMovie.BackdropFullPath);
                        }

                        // load new data
                        selectedMovie = movieBrowser.SelectedListItem.TVTag as DBMovieInfo;
                        updateBackdropVisibility();
                        publishDetails(selectedMovie, "SelectedMovie");
                        break;
                }
            }

            return base.OnMessage(message);
        }


        private void OnPlayBackStarted(MediaPortal.Player.g_Player.MediaType type, string filename) {
            if (selectedMovie != null && playingMovie) {
                Thread newThread = new Thread(new ThreadStart(UpdatePlaybackInfo));
                newThread.Start();
            }
        }

        private void OnPlayBackEnded(MediaPortal.Player.g_Player.MediaType type, string filename) {
            playingMovie = false;
        }

        // Updates the movie metadata on the playback screen (for when the user clicks info). 
        // The delay is neccisary because Player tries to use metadata from the MyVideos database.
        // We want to update this after that happens so the correct info is there.
        private void UpdatePlaybackInfo() {
            Thread.Sleep(2000);
            if (selectedMovie != null) {
                MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Title", selectedMovie.Title);
                MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Genre", selectedMovie.Genres[0]);
                MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Plot", selectedMovie.Summary);
                MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Thumb", selectedMovie.CoverThumbFullPath);
                MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Year", selectedMovie.Year.ToString());
            }
        }

        #endregion

        private void publishDetails(DatabaseTable obj, string prefix) {
            Type tableType = obj.GetType();
            foreach (DBField currField in DBField.GetFieldList(tableType)) {
                string property = "#MovingPictures." + prefix + "." + currField.FieldName;
                string value = currField.GetValue(obj).ToString().Trim();
                
                GUIPropertyManager.SetProperty(property, value);
                logger.Debug(property + " = \"" + value + "\"");

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
