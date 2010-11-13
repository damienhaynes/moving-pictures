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
using System.Linq;
using MovingPicturesSocialAPI.Data;
using Cornerstone.GUI.Dialogs;

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
                        _socialAPI = MpsAPI.Login(MovingPicturesCore.Settings.SocialUsername,
                                                  MovingPicturesCore.Settings.SocialHashedPassword,
                                                  MovingPicturesCore.Settings.SocialUrl);

                        if (_socialAPI != null) {
                            _socialAPI.RequestEvent += new MpsAPI.MpsAPIRequestDelegate(_socialAPI_RequestEvent);
                            _socialAPI.ResponseEvent += new MpsAPI.MpsAPIResponseDelegate(_socialAPI_ResponseEvent);
                        }
                    }
                    return _socialAPI;
                }
            }
        } private static MovingPicturesSocialAPI.MpsAPI _socialAPI = null;

        public Social() {
            MovingPicturesCore.Settings.SettingChanged += new SettingChangedDelegate(Settings_SettingChanged);
            if (MovingPicturesCore.Settings.SocialEnabled) {
                MovingPicturesCore.Importer.MovieStatusChanged += new MovieImporter.MovieStatusChangedHandler(movieStatusChangedListener);
                MovingPicturesCore.DatabaseManager.ObjectDeleted += new DatabaseManager.ObjectAffectedDelegate(movieDeletedListener);
                MovingPicturesCore.DatabaseManager.ObjectUpdated += new DatabaseManager.ObjectAffectedDelegate(DatabaseManager_ObjectUpdated);
                MovingPicturesCore.DatabaseManager.ObjectInserted += new DatabaseManager.ObjectAffectedDelegate(DatabaseManager_ObjectInserted);
                if (MovingPicturesCore.Settings.SocialTaskListTimer > 0) {
                    taskListTimer = new Timer(taskListTimerCallback, null, 0, MovingPicturesCore.Settings.SocialTaskListTimer * 60000);
                }
            }
        }

        public void Synchronize() {
            Synchronize(null);
        }

        public void Synchronize(ProgressDelegate progress) {
            try {
                logger.Info("Synchronizing with moving Pictures Social.");
                List<DBMovieInfo> moviesToSynch;
                List<DBMovieInfo> moviesToExclude;

                if (MovingPicturesCore.Settings.RestrictSynchronizedMovies) {
                    moviesToSynch = new List<DBMovieInfo>();
                    moviesToExclude = new List<DBMovieInfo>();

                    moviesToSynch.AddRange(MovingPicturesCore.Settings.SocialSyncFilter.Filter(DBMovieInfo.GetAll()));
                    moviesToExclude.AddRange(DBMovieInfo.GetAll().Except(moviesToSynch));

                    logger.Debug("Using synchronization filter. Syncing {0} movies. Excluding {1} movies.", moviesToSynch.Count, moviesToExclude.Count);
                }
                else {
                    moviesToSynch = DBMovieInfo.GetAll();
                    moviesToExclude = new List<DBMovieInfo>();
                    logger.Debug("Syncing {0} movies.", moviesToSynch.Count);
                }

                moviesToSynch = moviesToSynch.OrderBy(m => m.DateAdded).ToList();

                UploadMovieInfo(moviesToSynch, progress);
                RemoveFilteredMovies(moviesToExclude, progress);
            }
            catch (Exception ex) {
                logger.ErrorException("Unexpected error synchronizing with Moving Pictures Social!", ex);
            }
        }

        private void UploadMovieInfo(List<DBMovieInfo> movies, ProgressDelegate progress) {
            List<MpsMovie> mpsMovies = new List<MpsMovie>();

            int count = 0;
            foreach (var movie in movies) {
                count++;
                MpsMovie mpsMovie = MovingPicturesCore.Social.MovieToMPSMovie(movie);
                if (mpsMovie.Resources.Length > 1) {
                    logger.Debug("Adding {0} to movies to be synced", movie.Title);
                    mpsMovies.Add(mpsMovie);
                }
                else {
                    logger.Debug("Skipping {0} because it doesn't have source information", movie.Title);
                }

                if (progress != null)
                    progress("Syncing All Movies to MPS", (int)(count * 100 / movies.Count));

                if (mpsMovies.Count >= 100 || count == movies.Count) {
                    logger.Debug("Sending batch of {0} movies", mpsMovies.Count);
                    MovingPicturesCore.Social.SocialAPI.AddMoviesToCollection(ref mpsMovies);

                    // update MpsId on the DBMovieInfo object
                    foreach (MpsMovie mpsMovieDTO in mpsMovies) {
                        DBMovieInfo m = DBMovieInfo.Get(mpsMovieDTO.InternalId);
                        if (m != null) {
                            m.MpsId = mpsMovieDTO.MovieId;
                            m.Commit();
                        }
                    }

                    mpsMovies.Clear();
                }
            }
        }

        private void RemoveFilteredMovies(List<DBMovieInfo> movies, ProgressDelegate progress) {
            int count = 0;
            foreach (var movie in movies) {
                count++;
                if (movie.MpsId != null && movie.MpsId != 0) {
                    logger.Debug("Removing {0} from Moving Pictures Social because it has been excluded by a filter.", movie.Title);
                    SocialAPI.RemoveMovieFromCollection((int)movie.MpsId);
                }

                if (progress != null)
                    progress("Removing excluded movies from MPS", (int)(count * 100 / movies.Count));
            }
        }

        void DatabaseManager_ObjectInserted(DatabaseTable obj)
        {
            if (obj.GetType() != typeof(DBWatchedHistory))
                return;

            DBWatchedHistory wh = (DBWatchedHistory)obj;
            DBMovieInfo movie = wh.Movie;

            MPSBackgroundProcess bgProc = new MPSBackgroundProcess();
            bgProc.Action = MPSActions.WatchMovie;
            bgProc.Movies.Add(movie);
            MovingPicturesCore.ProcessManager.StartProcess(bgProc);
        }

        private void DatabaseManager_ObjectUpdated(DatabaseTable obj) {
            try {
                // we're looking for user rating changes
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

                List<DBMovieInfo> allMovies = DBMovieInfo.GetAll();

                int mpsIdMovieCount = (from m in allMovies
                                       where m.MpsId == movie.MpsId
                                       select m).Count();

                if (mpsIdMovieCount == 0)
                {
                    MPSBackgroundProcess bgProc = new MPSBackgroundProcess();
                    bgProc.Action = MPSActions.RemoveMovieFromCollection;
                    bgProc.Movies.Add(movie);
                    MovingPicturesCore.ProcessManager.StartProcess(bgProc);
                }

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

            try {
                MPSBackgroundProcess bgProc = new MPSBackgroundProcess();
                bgProc.Action = MPSActions.GetUserSyncData;
                MovingPicturesCore.ProcessManager.StartProcess(bgProc);
            }
            catch (Exception ex) {
                logger.ErrorException("", ex);
            }
        }

        /// <summary>
        /// Translates a DBMovieInfo object to a MPS MovieDTO object.
        /// </summary>
        public MpsMovie MovieToMPSMovie(DBMovieInfo movie) {
            MpsMovie mpsMovie = new MpsMovie();
            mpsMovie.InternalId = movie.ID.GetValueOrDefault();
            mpsMovie.Directors = "";
            mpsMovie.Writers = "";
            mpsMovie.Cast = "";
            mpsMovie.Genres = "";
            foreach (var person in movie.Directors) {
                mpsMovie.Directors += "|" + person;
            }
            foreach (var person in movie.Writers) {
                mpsMovie.Writers += "|" + person;
            }
            foreach (var person in movie.Actors) {
                mpsMovie.Cast += "|" + person;
            }
            foreach (var genre in movie.Genres) {
                mpsMovie.Genres += "|" + genre;
            }

            mpsMovie.Resources = "";
            mpsMovie.Locale = "";
            bool foundIMDB = false;
            foreach (DBSourceMovieInfo smi in movie.SourceMovieInfo) {
                if (smi.Source == null || smi.Source.Provider == null)
                    continue;
                mpsMovie.Resources += "|" 
                    + System.Web.HttpUtility.UrlEncode(smi.Source.Provider.Name)
                    + "="
                    + System.Web.HttpUtility.UrlEncode(smi.Identifier)
                    ;
                if (smi.Source == movie.PrimarySource) {
                    mpsMovie.Locale = smi.Source.Provider.LanguageCode;
                }
                if (smi.Source.Provider.Name.ToLower().Contains("imdb"))
                    foundIMDB = true;
            }

            if (!foundIMDB) {
                mpsMovie.Resources += "|"
                    + "imdb.com="
                    + System.Web.HttpUtility.UrlEncode(movie.ImdbID)
                    ;
            }

            if (MovingPicturesCore.Settings.EnableSocialFileHashSync)
                mpsMovie.FileHash = movie.LocalMedia[0].FileHash;
            else
                mpsMovie.FileHash = "";

            mpsMovie.Title = movie.Title ?? "";
            mpsMovie.Year = movie.Year.ToString() ?? "";
            mpsMovie.Certification = movie.Certification ?? "";
            mpsMovie.Language = movie.Language ?? "";
            mpsMovie.Tagline = movie.Tagline ?? "";
            mpsMovie.Summary = movie.Summary ?? "";
            mpsMovie.Score = movie.Score.ToString() ?? "";
            mpsMovie.Popularity = movie.Popularity.ToString() ?? "";
            mpsMovie.Runtime = movie.Runtime.ToString() ?? "";
            mpsMovie.TranslatedTitle = movie.Title ?? "";

            mpsMovie.WatchCount = movie.ActiveUserSettings.WatchedCount;
            if (movie.WatchedHistory.Count > 0)
                mpsMovie.LastWatchDate = movie.WatchedHistory[movie.WatchedHistory.Count - 1].DateWatched;
            else
                mpsMovie.LastWatchDate = DateTime.MinValue;
            
            mpsMovie.UserRating = movie.ActiveUserSettings.UserRating.GetValueOrDefault(0);

            return mpsMovie;
        }

        public void _socialAPI_RequestEvent(string RequestText) {
            string logtext = ScrubLogText(RequestText);
            logger.Debug("Request sent to MPS: " + logtext);
        }

        public void _socialAPI_ResponseEvent(string ResponseText) {
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
