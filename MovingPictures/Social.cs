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
using CookComputing.XmlRpc;
using System.Net;

namespace MediaPortal.Plugins.MovingPictures {
    public class Social {
        public enum StatusEnum { CONNECTED, DISABLED, BLOCKED, CONNECTION_ERROR, INTERNAL_ERROR }
        public delegate void StatusChangedDelegate(StatusEnum status);

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static object socialAPILock = new Object();
        private Timer taskListTimer;

        private DateTime lastConnectAttempt = new DateTime(1900, 1, 1);

        public event StatusChangedDelegate StatusChanged;
        
        // The MpsAPI object that should be used by all components of the plugin.
        public MovingPicturesSocialAPI.MpsAPI SocialAPI {
            get {
                lock (socialAPILock) {
                    TimeSpan retryDelay = new TimeSpan(0, MovingPicturesCore.Settings.SocialRetryTime, 0);
                    if (_socialAPI == null && (DateTime.Now - lastConnectAttempt > retryDelay)) {
                        lastConnectAttempt = DateTime.Now;

                        try {
                            _socialAPI = MpsAPI.Login(MovingPicturesCore.Settings.SocialUsername,
                                                      MovingPicturesCore.Settings.SocialHashedPassword,
                                                      MovingPicturesCore.Settings.SocialUrl);

                            if (_socialAPI == null) {
                                logger.Error("Failed to log in to Moving Pictures Social: Invalid Username or Password!");
                            }

                            if (_socialAPI != null) {
                                _socialAPI.RequestEvent += new MpsAPI.MpsAPIRequestDelegate(_socialAPI_RequestEvent);
                                _socialAPI.ResponseEvent += new MpsAPI.MpsAPIResponseDelegate(_socialAPI_ResponseEvent);
                            }
                        }
                        catch (Exception ex){
                            if (ex is XmlRpcServerException && ((XmlRpcServerException)ex).Message == "Not Found")
                                logger.Error("Failed to log in to Moving Pictures Social: Unable to connect to server!");
                            else if (ex is XmlRpcServerException && ((XmlRpcServerException)ex).Message == "Forbidden")
                                logger.Error("Failed to log in to Moving Pictures Social: This account is currently locked!");
                            else
                                logger.Error("Failed to log in to Moving Pictures Social, Unexpected Error: " + ex.Message);
                        }
                    }
                    return _socialAPI;
                }
            }
        } private static MovingPicturesSocialAPI.MpsAPI _socialAPI = null;

        public StatusEnum Status {
            get {
                if (IsOnline && _status != StatusEnum.CONNECTED) {
                    _status = StatusEnum.CONNECTED;
                    if (StatusChanged != null) StatusChanged(_status);
                }

                return _status;
            }
            internal set {
                _status = value;

                if (StatusChanged != null) StatusChanged(_status);
            }
        } private StatusEnum _status = StatusEnum.CONNECTED;

        public bool IsOnline {
            get { return SocialAPI != null; }
        }

        public Social() {
            MovingPicturesCore.Settings.SettingChanged += new SettingChangedDelegate(Settings_SettingChanged);
            Init();
        }

        private void Init() {
            _socialAPI = null;

            if (MovingPicturesCore.Settings.SocialEnabled) {
                MovingPicturesCore.DatabaseManager.ObjectDeleted += new DatabaseManager.ObjectAffectedDelegate(movieDeletedListener);
                MovingPicturesCore.DatabaseManager.ObjectUpdated += new DatabaseManager.ObjectAffectedDelegate(DatabaseManager_ObjectUpdated);
                MovingPicturesCore.DatabaseManager.ObjectInserted += new DatabaseManager.ObjectAffectedDelegate(DatabaseManager_ObjectInserted);
                if (MovingPicturesCore.Settings.SocialTaskListTimer > 0) {
                    taskListTimer = new Timer(taskListTimerCallback, null, 0, MovingPicturesCore.Settings.SocialTaskListTimer * 60000);
                }
            }
            else {
                if (taskListTimer != null) taskListTimer.Dispose();
                MovingPicturesCore.DatabaseManager.ObjectDeleted -= new DatabaseManager.ObjectAffectedDelegate(movieDeletedListener);
                MovingPicturesCore.DatabaseManager.ObjectUpdated -= new DatabaseManager.ObjectAffectedDelegate(DatabaseManager_ObjectUpdated);
                MovingPicturesCore.DatabaseManager.ObjectInserted -= new DatabaseManager.ObjectAffectedDelegate(DatabaseManager_ObjectInserted);
            }

        }

        private bool Reconnect() {
            lastConnectAttempt = new DateTime(1900, 1, 1);
            return IsOnline;
        }

        public void Synchronize() {
            Synchronize(null);
        }

        public void Synchronize(ProgressDelegate progress) {
            if (!MovingPicturesCore.Settings.SocialEnabled) {
                logger.Warn("Attempt to call Moving Pictures Social made when service is disabled.");
                return;
            }

            if (!IsOnline) {
                logger.Warn("Can not synchonize to Moving Pictures Social because service is offline");
                return;
            }

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
                _socialAPI = null;
                Status = StatusEnum.INTERNAL_ERROR;
                return;
            }

            return;
        }

        private bool UploadMovieInfo(List<DBMovieInfo> movies, ProgressDelegate progress) {
            if (!MovingPicturesCore.Settings.SocialEnabled) {
                logger.Warn("Attempt to call Moving Pictures Social made when service is disabled.");
                return false;
            }

            if (!IsOnline) {
                logger.Warn("Can not upload movie info to Moving Pictures Social because service is offline");
                return false;
            }

            try {

                List<MpsMovie> mpsMovies = new List<MpsMovie>();

                int count = 0;
                foreach (var movie in movies) {
                    count++;
                    MpsMovie mpsMovie = Social.MovieToMPSMovie(movie);
                    if (mpsMovie.Resources.Length > 1) {
                        logger.Debug("Adding '{0}' to list of movies to be synced.", movie.Title);
                        mpsMovies.Add(mpsMovie);
                    }
                    else {
                        logger.Debug("Skipping '{0}' because it doesn't have source information.", movie.Title);
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
            catch (WebException ex) {
                logger.Error("There was a problem connecting to the Moving Pictures Social Server! " + ex.Message);
                MovingPicturesCore.Social.Status = Social.StatusEnum.CONNECTION_ERROR;
            }
            catch (Exception ex) {
                logger.ErrorException("Unexpected error uploading movie information to Moving Pictures Social!", ex);
                _socialAPI = null;
                MovingPicturesCore.Social.Status = Social.StatusEnum.INTERNAL_ERROR;
                return false;
            }

            return true;
        }

        private bool RemoveFilteredMovies(List<DBMovieInfo> movies, ProgressDelegate progress) {
            if (!MovingPicturesCore.Settings.SocialEnabled) {
                logger.Warn("Attempt to call Moving Pictures Social made when service is disabled.");
                return false;
            }

            if (!IsOnline) {
                logger.Warn("Can not remove movies from Moving Pictures Social collection because service is offline");
                return false;
            }

            try {
                int count = 0;
                foreach (var movie in movies) {
                    count++;
                    if (movie.MpsId != null && movie.MpsId != 0) {
                        logger.Debug("Removing '{0}' from Moving Pictures Social because it has been excluded by a filter.", movie.Title);
                        SocialAPI.RemoveMovieFromCollection((int)movie.MpsId);
                        movie.MpsId = null;
                    }

                    if (progress != null)
                        progress("Removing excluded movies from MPS", (int)(count * 100 / movies.Count));
                }
            }
            catch (WebException ex) {
                logger.Error("There was a problem connecting to the Moving Pictures Social Server! " + ex.Message);
                MovingPicturesCore.Social.Status = Social.StatusEnum.CONNECTION_ERROR;
            }
            catch (Exception ex) {
                logger.ErrorException("Unexpected error removing movies from your Moving Pictures Social collection!", ex);
                _socialAPI = null;
                MovingPicturesCore.Social.Status = Social.StatusEnum.INTERNAL_ERROR;
                return false;
            }

            return true;
        }

        public bool UpdateWatchedCount(DBMovieInfo movie, bool includeInStream) {
            if (!MovingPicturesCore.Settings.SocialEnabled) {
                logger.Warn("Attempt to call Moving Pictures Social made when service is disabled.");
                return false;
            }

            if (!IsOnline) {
                logger.Warn("Can not send movie watched count to Moving Pictures Social because service is offline");
                return false;
            }

            try {
                MPSBackgroundProcess bgProc = new MPSBackgroundProcess();
                bgProc.Action = includeInStream ? MPSActions.WatchMovie : MPSActions.WatchMovieIgnoreStream;
                bgProc.Movies.Add(movie);
                MovingPicturesCore.ProcessManager.StartProcess(bgProc);
            }
            catch (Exception ex) {
                logger.ErrorException("Unexpected error sending 'movie watched' information to Moving Pictures Social!", ex);
                _socialAPI = null;
                MovingPicturesCore.Social.Status = Social.StatusEnum.INTERNAL_ERROR;
                return false;
            }

            return true;
        }

        private void DatabaseManager_ObjectInserted(DatabaseTable obj) {
            if (!MovingPicturesCore.Settings.SocialEnabled) {
                logger.Warn("Attempt to call Moving Pictures Social made when service is disabled.");
                return;
            }

            if (!IsOnline) {
                logger.Warn("Can not connect to Moving Pictures Social because service is offline");
                return;
            }

            try {
                if (obj.GetType() == typeof(DBWatchedHistory)) {
                    DBWatchedHistory wh = (DBWatchedHistory)obj;
                    DBMovieInfo movie = wh.Movie;

                    MPSBackgroundProcess bgProc = new MPSBackgroundProcess();
                    bgProc.Action = MPSActions.WatchMovie;
                    bgProc.Movies.Add(movie);
                    MovingPicturesCore.ProcessManager.StartProcess(bgProc);
                }
                else if (obj.GetType() == typeof(DBMovieInfo)) {
                    DBMovieInfo movie = (DBMovieInfo)obj;
                    MPSBackgroundProcess bgProc = new MPSBackgroundProcess();
                    bgProc.Action = MPSActions.AddMoviesToCollection;
                    bgProc.Movies.Add(movie);
                    MovingPicturesCore.ProcessManager.StartProcess(bgProc);
                }
            }
            catch (Exception ex) {
                logger.ErrorException("Unexpected error connecting to Moving Pictures Social!", ex);
                _socialAPI = null;
                MovingPicturesCore.Social.Status = Social.StatusEnum.INTERNAL_ERROR;
            }
        }

        private void DatabaseManager_ObjectUpdated(DatabaseTable obj) {
            if (!MovingPicturesCore.Settings.SocialEnabled) {
                logger.Warn("Attempt to call Moving Pictures Social made when service is disabled.");
                return;
            }

            if (!IsOnline) {
                logger.Warn("Can not send rating info to Moving Pictures Social because service is offline");
                return;
            }

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
                logger.ErrorException("Unexpected error sending rating information to MovingPicturesSocial!", ex);
                _socialAPI = null;
                MovingPicturesCore.Social.Status = Social.StatusEnum.INTERNAL_ERROR;
            }
        }

        private void Settings_SettingChanged(DBSetting setting, object oldValue) {
            try {
                // Reinitializes the SocialAPI object when Username, Password, or URLBase changes.
                if (setting.Key == "socialurlbase" || setting.Key == "socialusername" || setting.Key == "socialpassword") {
                        Init();
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

        private void movieDeletedListener(DatabaseTable obj) {
            if (!MovingPicturesCore.Settings.SocialEnabled) {
                logger.Warn("Attempt to call Moving Pictures Social made when service is disabled.");
                return;
            }

            if (!IsOnline) {
                logger.Warn("Can not remove movie from Moving Pictures Social collection because service is offline");
                return;
            }

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
                logger.ErrorException("Unexpected error removing an object from your Moving Pictures Social collection!", ex);
                _socialAPI = null;
                MovingPicturesCore.Social.Status = Social.StatusEnum.INTERNAL_ERROR;
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
                _socialAPI = null;
                MovingPicturesCore.Social.Status = Social.StatusEnum.INTERNAL_ERROR;
                return;
            }

            try {
                MPSBackgroundProcess bgProc = new MPSBackgroundProcess();
                bgProc.Action = MPSActions.GetUserSyncData;
                MovingPicturesCore.ProcessManager.StartProcess(bgProc);
            }
            catch (Exception ex) {
                logger.ErrorException("", ex);
                _socialAPI = null;
                MovingPicturesCore.Social.Status = Social.StatusEnum.INTERNAL_ERROR;
            }
        }

        /// <summary>
        /// Translates a DBMovieInfo object to a MPS MovieDTO object.
        /// </summary>
        public static MpsMovie MovieToMPSMovie(DBMovieInfo movie) {
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

            if (movie.PrimarySource != null && movie.PrimarySource.Provider != null)
                mpsMovie.Locale = movie.PrimarySource.Provider.LanguageCode;

            foreach (DBSourceMovieInfo smi in movie.SourceMovieInfo) {
                if (smi.Source == null || smi.Source.Provider == null)
                    continue;

                if (String.IsNullOrEmpty(smi.Identifier))
                    continue;

                mpsMovie.Resources += "|" 
                    + System.Web.HttpUtility.UrlEncode(smi.Source.Provider.Name)
                    + "="
                    + System.Web.HttpUtility.UrlEncode(smi.Identifier)
                    ;

                if (smi.Source.Provider.Name.ToLower().Contains("imdb"))
                    foundIMDB = true;

                if (string.IsNullOrEmpty(mpsMovie.Locale)) {
                    mpsMovie.Locale = smi.Source.Provider.LanguageCode;
                }
            }

            if (!foundIMDB) {
                mpsMovie.Resources += "|"
                    + "imdb.com="
                    + System.Web.HttpUtility.UrlEncode(movie.ImdbID)
                    ;
            }

            if (MovingPicturesCore.Settings.EnableSocialFileHashSync)
                mpsMovie.FileHash = movie.LocalMedia[0].FileHash ?? "";
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

        internal void _socialAPI_RequestEvent(string RequestText) {
            string logtext = ScrubLogText(RequestText);
            logger.Debug("Request sent to MPS: " + logtext);
        }

        internal void _socialAPI_ResponseEvent(string ResponseText) {
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
