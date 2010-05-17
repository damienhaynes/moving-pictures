using System;
using System.Collections.Generic;
using System.Threading;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MovingPicturesSocialAPI;
using NLog;

namespace MediaPortal.Plugins.MovingPictures {
    public class Social {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static object socialAPILock = new Object();
        private Timer taskListTimer;

        // The MpsAPI object that should be used by all components of the plugin.
        public MovingPicturesSocialAPI.MpsAPI SocialAPI {
            get {
                lock (socialAPILock) {
                    if (_socialAPI == null) {
                        _socialAPI = new MpsAPI(MovingPicturesCore.Settings.SocialUsername
                            , MovingPicturesCore.Settings.SocialPassword
                            , SocialAPIURL);
                        _socialAPI.RequestEvent += new MpsAPI.MpsAPIRequestDelegate(_socialAPI_RequestEvent);
                        _socialAPI.ResponseEvent += new MpsAPI.MpsAPIResponseDelegate(_socialAPI_ResponseEvent);
                    }
                    return _socialAPI;
                }
            }
        }

        void _socialAPI_RequestEvent(string RequestText) {
            string logtext = ScrubLogText(RequestText);
            logger.Debug("Request sent to MPS: " + logtext);
        }

        void _socialAPI_ResponseEvent(string ResponseText) {
            string logtext = ScrubLogText(ResponseText);
            logger.Debug("Response received from MPS: " + logtext);
        }

        private string ScrubLogText(string RequestText) {
            // remove new lines and truncate
            string logtext = RequestText;
            logtext = System.Text.RegularExpressions.Regex.Replace(logtext, @"\s+", " ");
            if (logtext.Length > 1000)
                logtext = logtext.Substring(0, 1000) + "(truncated)";
            return logtext;
        }

        private static MovingPicturesSocialAPI.MpsAPI _socialAPI = null;

        public bool HasSocial {
            get {
                return MovingPicturesCore.Settings.SocialUsername.Trim().Length > 0;
            }
        }

        public Social() {
            MovingPicturesCore.Settings.SettingChanged += new SettingChangedDelegate(Settings_SettingChanged);
            if (HasSocial) {
                MovingPicturesCore.Importer.MovieStatusChanged += new MovieImporter.MovieStatusChangedHandler(movieStatusChangedListener);
                MovingPicturesCore.DatabaseManager.ObjectDeleted += new DatabaseManager.ObjectAffectedDelegate(movieDeletedListener);

                if (MovingPicturesCore.Settings.SocialTaskListTimer > 0) {
                    taskListTimer = new Timer(taskListTimerCallback, null, 0, MovingPicturesCore.Settings.SocialTaskListTimer * 60000);
                }
            }
        }

        void Settings_SettingChanged(DBSetting setting, object oldValue) {
            try {
                // Reinitializes the SocialAPI object when Username, Password, or URLBase changes.
                if (setting.Key == "socialurlbase"
                    || setting.Key == "socialusername"
                    || setting.Key == "socialpassword") {
                    _socialAPI = null;
                    if (taskListTimer != null) taskListTimer.Dispose();
                }

                // Recreate the timer if the timer setting changes
                if (setting.Key == "socialtasklisttimer") {
                    if (taskListTimer != null) taskListTimer.Dispose();
                    if (MovingPicturesCore.Settings.SocialTaskListTimer > 0) {
                        taskListTimer = new Timer(taskListTimerCallback, null, 0, MovingPicturesCore.Settings.SocialTaskListTimer * 60000);
                    }
                }
            }
            catch (Exception ex) {
                logger.ErrorException("", ex);
            }
        }

        private static void movieStatusChangedListener(MovieMatch obj, MovieImporterAction action) {
            try {
                if (action == MovieImporterAction.COMMITED) {
                    DBMovieInfo movie = obj.Selected.Movie;
                    logger.Info("Adding {0} to Moving Pictures Social Collection", movie.Title);

                    MovingPicturesSocialAPI.MovieDTO mpsMovie = MovingPicturesCore.Social.MovieToMPSMovie(movie);
                    MovingPicturesCore.Social.SocialAPI.AddMoviesToCollection(mpsMovie);
                }
            }
            catch (Exception ex) {
                logger.ErrorException("", ex);
            }
        }


        private void movieDeletedListener(DatabaseTable obj) {
            try {
                // if this is not a movie object, break
                if (obj.GetType() != typeof(DBMovieInfo))
                    return;

                DBMovieInfo movie = (DBMovieInfo)obj;
                logger.Info("Removing {0} from MPS collection", movie.Title);
                string sourceName = movie.PrimarySource.Provider.Name;
                string sourceId = movie.GetSourceMovieInfo(movie.PrimarySource).Identifier;
                MovingPicturesCore.Social.SocialAPI.RemoveMovieFromCollection(sourceName, sourceId);
            }
            catch (Exception ex) {
                logger.ErrorException("", ex);
            }

        }

        private void taskListTimerCallback(object state) {
            try {
                logger.Debug("Getting task list from MPS");
                List<TaskListItem> taskList = this.SocialAPI.GetUserTaskList();

                if (taskList.Count > 0) {
                    logger.Debug("MPS Task list contains {0} items", taskList.Count);
                    List<DBMovieInfo> allMovies = DBMovieInfo.GetAll();

                    foreach (TaskListItem taskItem in taskList) {
                        logger.Debug("Checking for cover for movie {0} {1}", taskItem.SourceName, taskItem.SourceId);
                        DBMovieInfo foundMovie = allMovies.Find(delegate(DBMovieInfo m) {
                            return
                            m.PrimarySource.Provider.Name == taskItem.SourceName
                            && m.GetSourceMovieInfo(m.PrimarySource).Identifier == taskItem.SourceId;
                        });

                        if (foundMovie != null && foundMovie.CoverFullPath.Trim().Length > 0) {
                            logger.Debug("Cover found.  Uploading for movie {0} {1}", taskItem.SourceName, taskItem.SourceId);
                            this.SocialAPI.UploadCover(taskItem.SourceName, taskItem.SourceId
                                , foundMovie.CoverFullPath);
                        }
                    }
                }
            }
            catch (Exception ex) {
                logger.ErrorException("", ex);
            }
        }

        /// <summary>
        /// Translates a DBMovieInfo object to a MPS MovieDTO object.
        /// </summary>
        public MovingPicturesSocialAPI.MovieDTO MovieToMPSMovie(DBMovieInfo movie) {
            MovingPicturesSocialAPI.MovieDTO mpsMovie = new MovingPicturesSocialAPI.MovieDTO();
            string directors = "";
            string actors = "";
            string writers = "";
            string genres = "";
            foreach (var person in movie.Directors) {
                directors += "|" + person;
            }
            foreach (var person in movie.Actors) {
                actors += "|" + person;
            }
            foreach (var person in movie.Writers) {
                writers += "|" + person;
            }
            foreach (var genre in movie.Genres) {
                genres += "|" + genre;
            }

            mpsMovie.SourceName = movie.PrimarySource.Provider.Name;
            mpsMovie.SourceId = movie.GetSourceMovieInfo(movie.PrimarySource).Identifier;
            mpsMovie.Title = movie.Title;
            mpsMovie.Year = movie.Year.ToString();
            mpsMovie.Certification = movie.Certification;
            mpsMovie.Language = movie.Language;
            mpsMovie.Tagline = movie.Tagline;
            mpsMovie.Summary = movie.Summary;
            mpsMovie.Score = movie.Score.ToString();
            mpsMovie.Popularity = movie.Popularity.ToString();
            mpsMovie.Runtime = movie.Runtime.ToString();
            mpsMovie.Genres = genres;
            mpsMovie.Directors = directors;
            mpsMovie.Cast = actors;
            mpsMovie.TranslatedTitle = movie.Title;
            mpsMovie.Locale = movie.PrimarySource.Provider.LanguageCode;
            return mpsMovie;
        }

        public string SocialAPIURL {
            get {
                return MovingPicturesCore.Settings.SocialURLBase + "api/1.0/";
            }
        }
    }
}
