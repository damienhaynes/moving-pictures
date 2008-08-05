using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Plugins.MovingPictures.ConfigScreen;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using System.Threading;

namespace MediaPortal.Plugins.MovingPictures {
    public class MovingPicturesGUI : GUIWindow, ISetupForm {

        DBMovieInfo selectedMovie;

        [SkinControl(50)]
        protected GUIListControl movieList = null;

        public MovingPicturesGUI() {
            g_Player.PlayBackStarted += new MediaPortal.Player.g_Player.StartedHandler(OnPlayBackStarted);
        }

        ~MovingPicturesGUI() {
        }


        #region ISetupForm Members

        // Returns the name of the plugin which is shown in the plugin menu
        public string PluginName()
        {
            return "Moving Pictures";
        }

        // Returns the description of the plugin is shown in the plugin menu
        public string Description()
        {
            return "A comprehensive movie management plug-in.";
        }

        // Returns the author of the plugin which is shown in the plugin menu
        public string Author()
        {
            return "John";
        }

        // show the setup dialog
        public void ShowPlugin()
        {
            MovingPicturesCore.Initialize();
            
            MovingPicturesConfig configScr = new MovingPicturesConfig();
            configScr.ShowDialog();

            MovingPicturesCore.Shutdown();

        }

        // Indicates whether plugin can be enabled/disabled
        public bool CanEnable()
        {
            return true;
        }

        // get ID of windowplugin belonging to this setup
        public int GetWindowId()
        {
            return 96742;
        }

        // Indicates if plugin is enabled by default;
        public bool DefaultEnabled()
        {
            return true;
        }

        // indicates if a plugin has its own setup screen
        public bool HasSetup()
        {
            return true;
        }

        /// <summary>
        /// If the plugin should have its own button on the main menu of Media Portal then it
        /// should return true to this method, otherwise if it should not be on home
        /// it should return false
        /// </summary>
        /// <param name="strButtonText">text the button should have</param>
        /// <param name="strButtonImage">image for the button, or empty for default</param>
        /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
        /// <param name="strPictureImage">subpicture for the button or empty for none</param>
        /// <returns>true  : plugin needs its own button on home
        ///          false : plugin does not need its own button on home</returns>
        public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
        {
            strButtonText = "Moving Pictures";
            strButtonImage = String.Empty;
            strButtonImageFocus = String.Empty;
            strPictureImage = String.Empty;
            return true;
        }
        #endregion

        #region GUIWindow Members

        public override int GetID {
            get {
                return GetWindowId();
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
            if (movieList == null)
            {
                GUIDialogOK dialog = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                dialog.Reset();
                dialog.SetHeading("Problem loading skin file...");
                dialog.DoModal(GetID);
                GUIWindowManager.ShowPreviousWindow();
                return;
            }
            
            movieList.Clear();
            movieList.Focus = true;

            List<DBMovieInfo> movies = DBMovieInfo.GetAll();
            foreach (DBMovieInfo currMovie in movies) {
                GUIListItem currItem = new GUIListItem();
                currItem.Label = currMovie.Name;
                currItem.TVTag = currMovie;
                movieList.Add(currItem);
            }

            movieList.Sort(new GUIListControlMovieComparer());
        }

        protected override void OnPageDestroy(int new_windowId) {
            base.OnPageDestroy(new_windowId);
        }


        protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType) {
            base.OnClicked(controlId, control, actionType);
            if (actionType != Action.ActionType.ACTION_SELECT_ITEM) return;
            
            if (control == movieList) {
                selectedMovie = movieList.SelectedListItem.TVTag as DBMovieInfo;
                
                // play the movie! 
                GUIGraphicsContext.IsFullScreenVideo = true;
                GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
                bool success = g_Player.Play(selectedMovie.LocalMedia[0].FullPath, g_Player.MediaType.Video);
            }
        }

        private void OnPlayBackStarted(MediaPortal.Player.g_Player.MediaType type, string filename) {
            Thread newThread = new Thread(new ThreadStart(UpdatePlaybackInfo));
            newThread.Start();
        }

        // Updates the movie metadata on the playback screen (for when the user clicks info). 
        // The delay is neccisary because Player tries to use metadata from the MyVideos database.
        // We want to update this after that happens so the correct info is there.
        private void UpdatePlaybackInfo() {
            Thread.Sleep(2000);
            MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Title", selectedMovie.Name);
            MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Genre", selectedMovie.Genres[0]);
            MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Plot", selectedMovie.Summary);
            MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Thumb", selectedMovie.CoverThumbFullPath);
            MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Year", selectedMovie.Year.ToString());
            
        }

        #endregion
    }

    public class GUIListControlMovieComparer : IComparer<GUIListItem> {
        public int Compare(GUIListItem x, GUIListItem y) {
            try {
                DBMovieInfo movieX = (DBMovieInfo)x.TVTag;
                DBMovieInfo movieY = (DBMovieInfo)y.TVTag;

                return movieX.SortName.CompareTo(movieY.SortName);
            } catch {
                return 0;
            }
        }
    }
}
