using System;
using System.Collections.Generic;
using System.Threading;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using NLog;
using MediaPortal.Plugins.MovingPictures.BackgroundProcesses;
using System.Linq;
using Cornerstone.GUI.Dialogs;
using CookComputing.XmlRpc;
using System.Net;
using System.Globalization;
using Follwit.API;
using Follwit.API.Data;

namespace MediaPortal.Plugins.MovingPictures {
    public class FollwitConnector {
        public enum StatusEnum { CONNECTED, DISABLED, BLOCKED, CONNECTION_ERROR, INTERNAL_ERROR }
        public delegate void StatusChangedDelegate(StatusEnum status);

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static object follwitAPILock = new Object();
        private Timer taskListTimer;

        private bool receivingEvents = false;

        private HashSet<DBMovieInfo> currentlySyncingMovies = new HashSet<DBMovieInfo>();

        private DateTime lastConnectAttempt = new DateTime(1900, 1, 1);

        public event StatusChangedDelegate StatusChanged;

        // The API object that should be used by all components of the plugin.
        public FollwitApi FollwitApi {
            get {
                TimeSpan retryDelay = new TimeSpan(0, MovingPicturesCore.Settings.FollwitRetryTime, 0);
                if (_follwitAPI == null && (DateTime.Now - lastConnectAttempt > retryDelay)) {
                    lastConnectAttempt = DateTime.Now;

                    try {
                        lock (follwitAPILock) {
                            _follwitAPI = FollwitApi.Login(MovingPicturesCore.Settings.FollwitUsername,
                                                      MovingPicturesCore.Settings.FollwitHashedPassword,
                                                      MovingPicturesCore.Settings.FollwitUrl);
                        }
                        if (_follwitAPI == null) {
                            logger.Error("Failed to log in to follw.it: Invalid Username or Password!");
                        }

                        if (_follwitAPI != null) {
                            _follwitAPI.RequestEvent += new FollwitApi.FitAPIRequestDelegate(_follwitAPI_RequestEvent);
                            _follwitAPI.ResponseEvent += new FollwitApi.FitAPIResponseDelegate(_follwitAPI_ResponseEvent);

                            logger.Info("Logged in to follw.it as {0}.", _follwitAPI.User.Name);
                            Status = StatusEnum.CONNECTED;
                        }
                    }
                    catch (Exception ex) {
                        if (ex is XmlRpcServerException && ((XmlRpcServerException)ex).Message == "Not Found") {
                            logger.Error("Failed to log in to follw.it: Unable to connect to server!");
                            Status = StatusEnum.CONNECTION_ERROR;
                        }
                        else if (ex is XmlRpcServerException && ((XmlRpcServerException)ex).Message == "Forbidden") {
                            logger.Error("Failed to log in to follw.it: This account is currently locked!");
                            Status = StatusEnum.BLOCKED;
                        }
                        else {
                            logger.Error("Failed to log in to follw.it, Unexpected Error: " + ex.Message);
                            Status = StatusEnum.INTERNAL_ERROR;
                        }
                    }
                }
                return _follwitAPI;
            }
        } private static FollwitApi _follwitAPI = null;

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
            get { return FollwitApi != null; }
        }

        public FollwitConnector() {
            MovingPicturesCore.Settings.SettingChanged += new SettingChangedDelegate(Settings_SettingChanged);
            Init();
        }

        private void Init() {
            _follwitAPI = null;
            lastConnectAttempt = new DateTime(1900, 1, 1);

            if (MovingPicturesCore.Settings.FollwitEnabled) {
                if (!receivingEvents) {
                    MovingPicturesCore.DatabaseManager.ObjectDeleted += new DatabaseManager.ObjectAffectedDelegate(movieDeletedListener);
                    MovingPicturesCore.DatabaseManager.ObjectUpdatedEx += new DatabaseManager.ObjectUpdatedDelegate(DatabaseManager_ObjectUpdated);
                    MovingPicturesCore.DatabaseManager.ObjectInserted += new DatabaseManager.ObjectAffectedDelegate(DatabaseManager_ObjectInserted);
                    receivingEvents = true;
                }
                if (MovingPicturesCore.Settings.FollwitTaskListTimer > 0) {
                    taskListTimer = new Timer(taskListTimerCallback, null, 0, MovingPicturesCore.Settings.FollwitTaskListTimer * 60000);
                }
            }
            else {
                if (taskListTimer != null) taskListTimer.Dispose();
                MovingPicturesCore.DatabaseManager.ObjectDeleted -= new DatabaseManager.ObjectAffectedDelegate(movieDeletedListener);
                MovingPicturesCore.DatabaseManager.ObjectUpdatedEx -= new DatabaseManager.ObjectUpdatedDelegate(DatabaseManager_ObjectUpdated);
                MovingPicturesCore.DatabaseManager.ObjectInserted -= new DatabaseManager.ObjectAffectedDelegate(DatabaseManager_ObjectInserted);
                receivingEvents = false;
            }

        }

        public bool Reconnect() {
            lastConnectAttempt = new DateTime(1900, 1, 1);
            return IsOnline;
        }

        public void Synchronize() {
            Synchronize(null);
        }

        public void Synchronize(ProgressDelegate progress) {
            if (!MovingPicturesCore.Settings.FollwitEnabled) {
                logger.Warn("Attempt to call follw.it made when service is disabled.");
                return;
            }

            if (!IsOnline) {
                logger.Warn("Can not synchonize to follw.it because service is offline");
                return;
            }

            try {
                logger.Info("Synchronizing with follw.it.");
                List<DBMovieInfo> moviesToSynch;
                List<DBMovieInfo> moviesToExclude;

                if (MovingPicturesCore.Settings.RestrictSynchronizedMovies) {
                    moviesToSynch = new List<DBMovieInfo>();
                    moviesToExclude = new List<DBMovieInfo>();

                    moviesToSynch.AddRange(MovingPicturesCore.Settings.FollwitSyncFilter.Filter(DBMovieInfo.GetAll()));
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
                logger.ErrorException("Unexpected error synchronizing with follw.it!", ex);
                _follwitAPI = null;
                Status = StatusEnum.INTERNAL_ERROR;
                return;
            }

            return;
        }

        private bool UploadMovieInfo(List<DBMovieInfo> movies, ProgressDelegate progress) {
            if (!MovingPicturesCore.Settings.FollwitEnabled) {
                logger.Warn("Attempt to call follw.it made when service is disabled.");
                return false;
            }

            if (!IsOnline) {
                logger.Warn("Can not upload movie info to follw.it because service is offline");
                return false;
            }

            try {

                List<FitMovie> fitMovies = new List<FitMovie>();

                int count = 0;
                foreach (var movie in movies) {
                    count++;
                    FitMovie fitMovie = FollwitConnector.MovieToFitMovie(movie);
                    if (fitMovie.Resources.Length > 1) {
                        logger.Debug("Adding '{0}' to list of movies to be synced.", movie.Title);
                        fitMovies.Add(fitMovie);
                    }
                    else {
                        logger.Debug("Skipping '{0}' because it doesn't have source information.", movie.Title);
                    }

                    if (progress != null)
                        progress("Syncing All Movies to follw.it", (int)(count * 100 / movies.Count));

                    if (fitMovies.Count >= MovingPicturesCore.Settings.FollwitBatchSize || count == movies.Count) {
                        logger.Debug("Sending batch of {0} movies", fitMovies.Count);
                        MovingPicturesCore.Follwit.FollwitApi.AddMoviesToCollection(ref fitMovies);

                        // update fitId on the DBMovieInfo object
                        foreach (FitMovie fitMovieDTO in fitMovies) {
                            DBMovieInfo m = DBMovieInfo.Get(fitMovieDTO.InternalId);
                            if (m != null) {
                                m.FitId = fitMovieDTO.MovieId;
                                if (fitMovieDTO.UserRating > 0)
                                    m.ActiveUserSettings.UserRating = fitMovieDTO.UserRating;
                                if (fitMovieDTO.Watched && m.ActiveUserSettings.WatchedCount == 0)
                                    m.ActiveUserSettings.WatchedCount = 1;

                                m.Commit();
                            }
                        }

                        fitMovies.Clear();
                    }
                }
            }
            catch (WebException ex) {
                logger.Error("There was a problem connecting to the follw.it Server! " + ex.Message);
                MovingPicturesCore.Follwit.Status = FollwitConnector.StatusEnum.CONNECTION_ERROR;
            }
            catch (Exception ex) {
                logger.ErrorException("Unexpected error uploading movie information to follw.it!", ex);
                _follwitAPI = null;
                MovingPicturesCore.Follwit.Status = FollwitConnector.StatusEnum.INTERNAL_ERROR;
                return false;
            }

            return true;
        }

        private bool RemoveFilteredMovies(List<DBMovieInfo> movies, ProgressDelegate progress) {
            if (!MovingPicturesCore.Settings.FollwitEnabled) {
                logger.Warn("Attempt to call follw.it made when service is disabled.");
                return false;
            }

            if (!IsOnline) {
                logger.Warn("Can not remove movies from follw.it collection because service is offline");
                return false;
            }

            try {
                int count = 0;
                foreach (var movie in movies) {
                    count++;
                    if (movie.FitId != null && movie.FitId != 0) {
                        logger.Debug("Removing '{0}' from follw.it because it has been excluded by a filter.", movie.Title);
                        FollwitApi.RemoveMovieFromCollection((int)movie.FitId);
                        movie.FitId = null;
                    }

                    if (progress != null)
                        progress("Removing excluded movies from follw.it", (int)(count * 100 / movies.Count));
                }
            }
            catch (WebException ex) {
                logger.Error("There was a problem connecting to the follw.it Server! " + ex.Message);
                MovingPicturesCore.Follwit.Status = FollwitConnector.StatusEnum.CONNECTION_ERROR;
            }
            catch (Exception ex) {
                logger.ErrorException("Unexpected error removing movies from your follw.it collection!", ex);
                _follwitAPI = null;
                MovingPicturesCore.Follwit.Status = FollwitConnector.StatusEnum.INTERNAL_ERROR;
                return false;
            }

            return true;
        }

        public void ProcessTasks() {
            ProcessTasks(null);
        }

        public void ProcessTasks(DateTime? forcedStartTime) {
            try {
                // store the current time and grab info on the last time we processed our task list
                DateTime currentSyncTime;
                DateTime lastRetrived;
                if (!DateTime.TryParse(MovingPicturesCore.Settings.LastSynchTime, new CultureInfo("en-US"), System.Globalization.DateTimeStyles.None, out lastRetrived))
                    lastRetrived = DateTime.MinValue;

                // if we have been passed a hard coded start time, use that instead
                if (forcedStartTime != null) lastRetrived = (DateTime)forcedStartTime;

                logger.Debug("Synchronizing with follw.it (last synced {0})...", lastRetrived);

                List<TaskListItem> taskList = FollwitApi.GetUserTaskList(lastRetrived, out currentSyncTime);
                logger.Debug("{0} follw.it synchronization tasks require attention.", taskList.Count);
                if (taskList.Count == 0) return;
                

                // first process all id changes
                foreach (TaskListItem currTask in taskList)
                    if (currTask.Task == TaskItemType.UpdatedMovieId)
                        UpdateLocalMovieId(currTask);

                // then process everything else
                foreach (TaskListItem currTask in taskList)
                    switch (currTask.Task) {
                        case TaskItemType.CoverRequest:
                            SubmitCover(currTask.MovieId);
                            break;
                        case TaskItemType.NewRating:
                            UpdateLocalRating(currTask);
                            break;
                        case TaskItemType.NewWatchedStatus:
                            UpdateLocalWatchedStatus(currTask);
                            break;
                        case TaskItemType.UpdatedMovieId:
                            // handled above
                            break;
                    }

                // set the time only at the end in case an error occured
                MovingPicturesCore.Settings.LastSynchTime = currentSyncTime.ToString(new CultureInfo("en-US"));
                logger.Info("follw.it synchronization complete...");
            }
            catch (WebException ex) {
                logger.Error("There was a problem connecting to the follw.it Server! " + ex.Message);
                Status = FollwitConnector.StatusEnum.CONNECTION_ERROR;
            }
            catch (Exception ex) {
                logger.ErrorException("Unexpected error connecting to follw.it.\n", ex);
                Status = FollwitConnector.StatusEnum.INTERNAL_ERROR;
            }
        }

        public void SubmitCover(int fitId) {
            logger.Debug("Checking for cover for follw.it ID #{0}", fitId);

            DBMovieInfo matchingMovie = DBMovieInfo.GetByFitId(fitId);

            if (matchingMovie != null && matchingMovie.CoverFullPath.Trim().Length > 0) {
                logger.Info("Submitting cover for '{0}' to follw.it.", matchingMovie.Title);
                MovingPicturesCore.Follwit.FollwitApi.UploadCover(fitId, matchingMovie.CoverFullPath);
            }
        }

        private void UpdateLocalMovieId(TaskListItem taskDetails) {
            DBMovieInfo matchingMovie = DBMovieInfo.GetByFitId(taskDetails.MovieId);
            if (matchingMovie == null) return;

            logger.Debug("The follw.it ID for {0} has changed from {1} to {2}.", matchingMovie.Title, taskDetails.MovieId, taskDetails.NewMovieId);
            matchingMovie.FitId = taskDetails.NewMovieId;
            matchingMovie.Commit();
        }

        private void UpdateLocalRating(TaskListItem taskDetails) {
            DBMovieInfo matchingMovie = DBMovieInfo.GetByFitId(taskDetails.MovieId);
            if (matchingMovie == null) return;

            currentlySyncingMovies.Add(matchingMovie);

            logger.Info("Importing rating of {0} for '{1}' from follw.it.", taskDetails.Rating, matchingMovie.Title);
            matchingMovie.ActiveUserSettings.UserRating = taskDetails.Rating;
            matchingMovie.Commit();

            currentlySyncingMovies.Remove(matchingMovie);
        }

        private void UpdateLocalWatchedStatus(TaskListItem taskDetails) {
            DBMovieInfo matchingMovie = DBMovieInfo.GetByFitId(taskDetails.MovieId);
            if (matchingMovie == null) return;

            currentlySyncingMovies.Add(matchingMovie);

            logger.Info("Importing watched status from follw.it for '{0}'. Watched: {1}", matchingMovie.Title, taskDetails.Watched);
            if (((bool)taskDetails.Watched) && matchingMovie.ActiveUserSettings.WatchedCount == 0) {
                matchingMovie.ActiveUserSettings.WatchedCount = 1;
                matchingMovie.Commit();
            }

            if (!((bool)taskDetails.Watched) && matchingMovie.ActiveUserSettings.WatchedCount > 0) {
                matchingMovie.ActiveUserSettings.WatchedCount = 0;
                matchingMovie.Commit();
            }

            matchingMovie.Commit();
            currentlySyncingMovies.Remove(matchingMovie);
        }

        public bool CurrentlyWatching(DBMovieInfo movie, bool isWatching) {
            if (!MovingPicturesCore.Settings.FollwitEnabled) {
                logger.Warn("Attempt to call follw.it made when service is disabled.");
                return false;
            }

            if (!IsOnline) {
                logger.Warn("Can not send movie watched count to follw.it because service is offline");
                return false;
            }

            try {
                if (MovingPicturesCore.Settings.RestrictSynchronizedMovies) {
                    var filtered = MovingPicturesCore.Settings.FollwitSyncFilter.Filter(new List<DBMovieInfo>() {movie});
                    if (filtered.Count == 0)
                        return false;
                }                

                FollwitBackgroundProcess bgProc = new FollwitBackgroundProcess();
                bgProc.Action = isWatching ? FitActions.BeginWatching : FitActions.EndWatching;
                bgProc.Movies.Add(movie);
                MovingPicturesCore.ProcessManager.StartProcess(bgProc);
            }
            catch (Exception ex) {
                logger.ErrorException("Unexpected error sending 'now watching' information to follw.it!", ex);
                _follwitAPI = null;
                MovingPicturesCore.Follwit.Status = FollwitConnector.StatusEnum.INTERNAL_ERROR;
                return false;
            }

            return true;
        }

        public bool WatchMovie(DBMovieInfo movie, bool includeInStream) {
            if (currentlySyncingMovies.Contains(movie))
                return true;

            if (!MovingPicturesCore.Settings.FollwitEnabled) {
                logger.Warn("Attempt to call follw.it made when service is disabled.");
                return false;
            }

            if (!IsOnline) {
                logger.Warn("Can not send movie watched count to follw.it because service is offline");
                return false;
            }

            try {
                FollwitBackgroundProcess bgProc = new FollwitBackgroundProcess();
                bgProc.Action = includeInStream ? FitActions.WatchMovie : FitActions.WatchMovieIgnoreStream;
                bgProc.Movies.Add(movie);
                MovingPicturesCore.ProcessManager.StartProcess(bgProc);
            }
            catch (Exception ex) {
                logger.ErrorException("Unexpected error sending 'movie watched' information to follw.it!", ex);
                _follwitAPI = null;
                MovingPicturesCore.Follwit.Status = FollwitConnector.StatusEnum.INTERNAL_ERROR;
                return false;
            }

            return true;
        }

        public bool UnwatchMovie(DBMovieInfo movie) {
            if (currentlySyncingMovies.Contains(movie))
                return true;

            if (!MovingPicturesCore.Settings.FollwitEnabled) {
                logger.Warn("Attempt to call follw.it made when service is disabled.");
                return false;
            }

            if (!IsOnline) {
                logger.Warn("Can not send movie watched count to follw.it because service is offline");
                return false;
            }

            try {
                FollwitBackgroundProcess bgProc = new FollwitBackgroundProcess();
                bgProc.Action = FitActions.UnwatchMovie;
                bgProc.Movies.Add(movie);
                MovingPicturesCore.ProcessManager.StartProcess(bgProc);
            }
            catch (Exception ex) {
                logger.ErrorException("Unexpected error sending 'unwatch movie' information to follw.it!", ex);
                _follwitAPI = null;
                MovingPicturesCore.Follwit.Status = FollwitConnector.StatusEnum.INTERNAL_ERROR;
                return false;
            }

            return true;
        }

        private void DatabaseManager_ObjectInserted(DatabaseTable obj) {
            if (!MovingPicturesCore.Settings.FollwitEnabled) {
                logger.Warn("Attempt to call follw.it made when service is disabled.");
                return;
            }

            try {
                if (obj.GetType() == typeof(DBWatchedHistory)) {
                    DBWatchedHistory wh = (DBWatchedHistory)obj;
                    DBMovieInfo movie = wh.Movie;

                    WatchMovie(movie, true);
                }
                else if (obj.GetType() == typeof(DBMovieInfo)) {
                    DBMovieInfo movie = (DBMovieInfo)obj;
                    FollwitBackgroundProcess bgProc = new FollwitBackgroundProcess();
                    bgProc.Action = FitActions.AddMoviesToCollection;
                    bgProc.Movies.Add(movie);
                    MovingPicturesCore.ProcessManager.StartProcess(bgProc);
                }
            }
            catch (Exception ex) {
                logger.ErrorException("Unexpected error connecting to follw.it!", ex);
                _follwitAPI = null;
                MovingPicturesCore.Follwit.Status = FollwitConnector.StatusEnum.INTERNAL_ERROR;
            }
        }

        private void DatabaseManager_ObjectUpdated(DatabaseTable obj, TableUpdateInfo updateInfo) {
            if (!MovingPicturesCore.Settings.FollwitEnabled) {
                logger.Warn("Attempt to call follw.it made when service is disabled.");
                return;
            }

            try {
                // we're looking for user rating changes
                if (obj.GetType() != typeof(DBUserMovieSettings))
                    return;

                DBUserMovieSettings settings = (DBUserMovieSettings)obj;
                if (updateInfo.RatingChanged()) {
                    if (!IsOnline) {
                        logger.Warn("Can not send rating info to follw.it because service is offline");
                        return;
                    }
                    
                    DBMovieInfo movie = settings.AttachedMovies[0];

                    if (currentlySyncingMovies.Contains(movie))
                        return;

                    FollwitBackgroundProcess bgProc = new FollwitBackgroundProcess();
                    bgProc.Action = FitActions.UpdateUserRating;
                    bgProc.Movies.Add(movie);
                    MovingPicturesCore.ProcessManager.StartProcess(bgProc);
                }

            }
            catch (Exception ex) {
                logger.ErrorException("Unexpected error sending rating information to follw.it!", ex);
                _follwitAPI = null;
                MovingPicturesCore.Follwit.Status = FollwitConnector.StatusEnum.INTERNAL_ERROR;
            }
        }

        private void Settings_SettingChanged(DBSetting setting, object oldValue) {
            try {
                // Reinitializes the follwitAPI object when Username, Password, or URLBase changes.
                if (setting.Key == "socialurlbase" || setting.Key == "socialusername" || setting.Key == "socialpassword") {
                    Init();
                }

                // Recreate the timer if the timer setting changes
                if (setting.Key == "socialtasklisttimer") {
                    if (taskListTimer != null) taskListTimer.Dispose();
                    if (MovingPicturesCore.Settings.FollwitTaskListTimer > 0) {
                        taskListTimer = new Timer(taskListTimerCallback, null, 0, MovingPicturesCore.Settings.FollwitTaskListTimer * 60000);
                    }
                }
            }
            catch (Exception ex) {
                logger.ErrorException("", ex);
            }
        }

        private void movieDeletedListener(DatabaseTable obj) {
            if (!MovingPicturesCore.Settings.FollwitEnabled) {
                logger.Warn("Attempt to call follw.it made when service is disabled.");
                return;
            }

            try {
                // if this is not a movie object, break
                if (obj.GetType() != typeof(DBMovieInfo))
                    return;

                if (!IsOnline) {
                    logger.Warn("Can not remove movie from follw.it collection because service is offline");
                    return;
                }

                DBMovieInfo movie = (DBMovieInfo)obj;

                List<DBMovieInfo> allMovies = DBMovieInfo.GetAll();

                int fitIdMovieCount = (from m in allMovies
                                       where m.FitId == movie.FitId
                                       select m).Count();

                if (fitIdMovieCount == 0)
                {
                    FollwitBackgroundProcess bgProc = new FollwitBackgroundProcess();
                    bgProc.Action = FitActions.RemoveMovieFromCollection;
                    bgProc.Movies.Add(movie);
                    MovingPicturesCore.ProcessManager.StartProcess(bgProc);
                }

            }
            catch (Exception ex) {
                logger.ErrorException("Unexpected error removing an object from your follw.it collection!", ex);
                _follwitAPI = null;
                MovingPicturesCore.Follwit.Status = FollwitConnector.StatusEnum.INTERNAL_ERROR;
            }

        }

        private void taskListTimerCallback(object state) {
            try {
                FollwitBackgroundProcess bgProc = new FollwitBackgroundProcess();
                bgProc.Action = FitActions.ProcessTaskList;
                MovingPicturesCore.ProcessManager.StartProcess(bgProc);
            }
            catch (Exception ex) {
                logger.ErrorException("", ex); 
                _follwitAPI = null;
                MovingPicturesCore.Follwit.Status = FollwitConnector.StatusEnum.INTERNAL_ERROR;
                return;
            }
        }

        /// <summary>
        /// Translates a DBMovieInfo object to a follw.it MovieDTO object.
        /// </summary>
        public static FitMovie MovieToFitMovie(DBMovieInfo movie) {
            FitMovie fitMovie = new FitMovie();
            fitMovie.InternalId = movie.ID.GetValueOrDefault();
            fitMovie.Directors = "";
            fitMovie.Writers = "";
            fitMovie.Cast = "";
            fitMovie.Genres = "";
            foreach (var person in movie.Directors) {
                fitMovie.Directors += "|" + person;
            }
            foreach (var person in movie.Writers) {
                fitMovie.Writers += "|" + person;
            }
            foreach (var person in movie.Actors) {
                fitMovie.Cast += "|" + person;
            }
            foreach (var genre in movie.Genres) {
                fitMovie.Genres += "|" + genre;
            }

            fitMovie.Resources = "";
            fitMovie.Locale = "";
            bool foundIMDB = false;

            if (movie.PrimarySource != null && movie.PrimarySource.Provider != null)
                fitMovie.Locale = movie.PrimarySource.Provider.LanguageCode;

            foreach (DBSourceMovieInfo smi in movie.SourceMovieInfo) {
                if (smi.Source == null || smi.Source.Provider == null)
                    continue;

                if (String.IsNullOrEmpty(smi.Identifier))
                    continue;

                fitMovie.Resources += "|" 
                    + System.Web.HttpUtility.UrlEncode(smi.Source.Provider.Name)
                    + "="
                    + System.Web.HttpUtility.UrlEncode(smi.Identifier)
                    ;

                if (smi.Source.Provider.Name.ToLower().Contains("imdb"))
                    foundIMDB = true;

                if (string.IsNullOrEmpty(fitMovie.Locale)) {
                    fitMovie.Locale = smi.Source.Provider.LanguageCode;
                }
            }

            if (!foundIMDB) {
                fitMovie.Resources += "|"
                    + "imdb.com="
                    + System.Web.HttpUtility.UrlEncode(movie.ImdbID)
                    ;
            }

            if (MovingPicturesCore.Settings.EnableFollwitFileHashSync)
                fitMovie.FileHash = movie.LocalMedia[0].FileHash ?? "";
            else
                fitMovie.FileHash = "";

            fitMovie.Title = movie.Title ?? "";
            fitMovie.Year = movie.Year.ToString() ?? "";
            fitMovie.Certification = movie.Certification ?? "";
            fitMovie.Language = movie.Language ?? "";
            fitMovie.Tagline = movie.Tagline ?? "";
            fitMovie.Summary = movie.Summary ?? "";
            fitMovie.Score = movie.Score.ToString() ?? "";
            fitMovie.Popularity = movie.Popularity.ToString() ?? "";
            fitMovie.Runtime = movie.Runtime.ToString() ?? "";
            fitMovie.TranslatedTitle = movie.Title ?? "";

            if (movie.ActiveUserSettings.WatchedCount > 0) {
                fitMovie.Watched = true;
                if (movie.WatchedHistory.Count > 0)
                    fitMovie.LastWatchDate = movie.WatchedHistory[movie.WatchedHistory.Count - 1].DateWatched;
            }
            else {
                fitMovie.Watched = false;
                fitMovie.LastWatchDate = DateTime.MinValue;
            }
            
            fitMovie.UserRating = movie.ActiveUserSettings.UserRating.GetValueOrDefault(0);

            return fitMovie;
        }

        internal void _follwitAPI_RequestEvent(string RequestText) {
            string logtext = ScrubLogText(RequestText);
            logger.Debug("Request sent to follw.it: " + logtext);
        }

        internal void _follwitAPI_ResponseEvent(string ResponseText) {
            string logtext = ScrubLogText(ResponseText);
            logger.Debug("Response received from follw.it: " + logtext);
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
