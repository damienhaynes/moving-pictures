﻿using System;
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

namespace MediaPortal.Plugins.MovingPictures {
    public class MovingPicturesGUI : GUIWindow {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        DBMovieInfo selectedMovie;
        bool playingMovie = false;

        [SkinControl(50)]
        protected GUIListControl movieList = null;

        [SkinControl(60)]
        protected GUIImage coverImage = null;

        public MovingPicturesGUI() {
            selectedMovie = null;

            g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayBackStarted);
            g_Player.PlayBackEnded += new g_Player.EndedHandler(OnPlayBackEnded);
        }

        ~MovingPicturesGUI() {
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
                currItem.Label = currMovie.Title;
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
                if (selectedMovie == null)
                    selectedMovie = movieList.SelectedListItem.TVTag as DBMovieInfo;

                // play the movie! 
                GUIGraphicsContext.IsFullScreenVideo = true;
                GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
                bool success = g_Player.Play(selectedMovie.LocalMedia[0].FullPath, g_Player.MediaType.Video);
                playingMovie = success;
            }
        }

        public override void OnAction(Action action) {
            base.OnAction(action);
            if (action.wID == Action.ActionType.ACTION_MOVE_DOWN) {
                //showMessage("ACTION_MOVE_DOWN");
            }
        }

        public override bool OnMessage(GUIMessage message) {
            if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED) {
                switch (message.SenderControlId) {
                    case 50:
                        selectedMovie = movieList.SelectedListItem.TVTag as DBMovieInfo;
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
                string property = "#MovingPictures." + prefix + "." + currField.Name;
                string value = currField.GetValue(obj).ToString().Trim();
                
                GUIPropertyManager.SetProperty(property, value);
                logger.Info(property + " = \"" + value + "\"");

            }
        }
    }

    public class GUIListControlMovieComparer : IComparer<GUIListItem> {
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
