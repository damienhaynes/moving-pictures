using System;
using System.Collections.Generic;
using System.Threading;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MovingPicturesSocialAPI;
using NLog;
using MediaPortal.Plugins.MovingPictures.BackgroundProcesses;

namespace MediaPortal.Plugins.MovingPictures {
    public class Social {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static object socialAPILock = new Object();
        private Timer taskListTimer;

        public string SocialAPIURL {
            get {
                return MovingPicturesCore.Settings.SocialURLBase + "api/1.0/";
            }
        }

        public bool HasSocial {
            get {
                return MovingPicturesCore.Settings.SocialUsername.Trim().Length > 0;
            }
        }

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
        } private static MovingPicturesSocialAPI.MpsAPI _socialAPI = null;

        public Social() {
            MovingPicturesCore.Settings.SettingChanged += new SettingChangedDelegate(Settings_SettingChanged);
            if (HasSocial) {
                MovingPicturesCore.Importer.MovieStatusChanged += new MovieImporter.MovieStatusChangedHandler(movieStatusChangedListener);
                MovingPicturesCore.DatabaseManager.ObjectDeleted += new DatabaseManager.ObjectAffectedDelegate(movieDeletedListener);
                MovingPicturesCore.DatabaseManager.ObjectUpdated += new DatabaseManager.ObjectAffectedDelegate(DatabaseManager_ObjectUpdated);

                if (MovingPicturesCore.Settings.SocialTaskListTimer > 0) {
                    taskListTimer = new Timer(taskListTimerCallback, null, 0, MovingPicturesCore.Settings.SocialTaskListTimer * 60000);
                }
            }
        }

        private void DatabaseManager_ObjectUpdated(DatabaseTable obj) {
            try {
                // we're looking for user rating changes and watch count changes
                if (obj.GetType() != typeof(DBUserMovieSettings))
                    return;

                DBUserMovieSettings settings = (DBUserMovieSettings)obj;
                if (settings.RatingChanged) {
                    DBMovieInfo movie = settings.AttachedMovies[0];

                    MPSBackgroundProcess bgProc = new MPSBackgroundProcess();
                    bgProc.Action = MPSActions.UpdateUserRating;
                    bgProc.Movies.Add(movie);
                    MovingPicturesCore.ProcessManager.StartProcess(bgProc);

                    // reset the flag
                    settings.RatingChanged = false;
                }

                if (settings.WatchCountChanged) {
                    DBMovieInfo movie = settings.AttachedMovies[0];

                    MPSBackgroundProcess bgProc = new MPSBackgroundProcess();
                    bgProc.Action = MPSActions.SetWatchCount;
                    bgProc.Movies.Add(movie);
                    MovingPicturesCore.ProcessManager.StartProcess(bgProc);

                    // reset the flag
                    settings.WatchCountChanged = false;
                }
            }
            catch (Exception ex) {
                logger.ErrorException("", ex);
            }
        }

        private void Settings_SettingChanged(DBSetting setting, object oldValue) {
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
                    MPSBackgroundProcess bgProc = new MPSBackgroundProcess();
                    bgProc.Action = MPSActions.AddMoviesToCollection;
                    bgProc.Movies.Add(movie);
                    MovingPicturesCore.ProcessManager.StartProcess(bgProc);
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
                MPSBackgroundProcess bgProc = new MPSBackgroundProcess();
                bgProc.Action = MPSActions.RemoveMovieFromCollection;
                bgProc.Movies.Add(movie);
                MovingPicturesCore.ProcessManager.StartProcess(bgProc);

            }
            catch (Exception ex) {
                logger.ErrorException("", ex);
            }

        }

        private void taskListTimerCallback(object state) {
            try {
                MPSBackgroundProcess bgProc = new MPSBackgroundProcess();
                bgProc.Action = MPSActions.ProcessTaskList;
                MovingPicturesCore.ProcessManager.StartProcess(bgProc);
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
            mpsMovie.InternalId = movie.ID.GetValueOrDefault();
            mpsMovie.Directors = "";
            mpsMovie.Cast = "";
            mpsMovie.Genres = "";
            foreach (var person in movie.Directors) {
                mpsMovie.Directors += "|" + person;
            }
            foreach (var person in movie.Actors) {
                mpsMovie.Cast += "|" + person;
            }
            foreach (var genre in movie.Genres) {
                mpsMovie.Genres += "|" + genre;
            }

            mpsMovie.ResourceNames = "";
            mpsMovie.ResourceIds = "";
            mpsMovie.Locale = "";
            foreach (DBSourceMovieInfo smi in movie.SourceMovieInfo) {
                mpsMovie.ResourceNames += "|" + smi.Source.Provider.Name;
                mpsMovie.ResourceIds += "|" + smi.Identifier;
                if (smi.Source == movie.PrimarySource) {
                    mpsMovie.Locale = smi.Source.Provider.LanguageCode;
                }
            }

            mpsMovie.Title = movie.Title;
            mpsMovie.Year = movie.Year.ToString();
            mpsMovie.Certification = movie.Certification;
            mpsMovie.Language = movie.Language;
            mpsMovie.Tagline = movie.Tagline;
            mpsMovie.Summary = movie.Summary;
            mpsMovie.Score = movie.Score.ToString();
            mpsMovie.Popularity = movie.Popularity.ToString();
            mpsMovie.Runtime = movie.Runtime.ToString();
            mpsMovie.TranslatedTitle = movie.Title;
            return mpsMovie;
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
    }
}
