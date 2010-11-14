using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Tools;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;
using System.Threading;
using MovingPicturesSocialAPI;
using MovingPicturesSocialAPI.Data;

namespace MediaPortal.Plugins.MovingPictures.BackgroundProcesses {
    public enum MPSActions {
        AddMoviesToCollection,
        RemoveMovieFromCollection,
        UpdateUserRating,
        WatchMovie,
        WatchMovieIgnoreStream,
        ProcessTaskList,
        GetUserSyncData
    }
    public class MPSBackgroundProcess : AbstractBackgroundProcess {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public override string Name {
            get { return "MPS Background Process"; }
        }

        public override string Description {
            get {
                return "This process sends data to Moving Pictures Social.";
            }
        }

        public MPSActions Action;
        public List<DBMovieInfo> Movies = new List<DBMovieInfo>();
        private DBMovieInfo FirstMovie {
            get {
                return Movies[0];
            }
        }

        public override void Work() {
            try {
                switch (Action) {
                    case MPSActions.AddMoviesToCollection:
                        List<MpsMovie> movieDTOs = new List<MpsMovie>();
                        foreach (DBMovieInfo movie in Movies) {
                            logger.Info("Adding {0} to Moving Pictures Social Collection", movie.Title);
                            movieDTOs.Add(MovingPicturesCore.Social.MovieToMPSMovie(movie));
                        }
                        MovingPicturesCore.Social.SocialAPI.AddMoviesToCollection(ref movieDTOs);
                        // update MpsId on the DBMovieInfo object
                        foreach (MpsMovie mpsMovieDTO in movieDTOs) {
                            DBMovieInfo m = DBMovieInfo.Get(mpsMovieDTO.InternalId);
                            if (m != null) {
                                m.MpsId = mpsMovieDTO.MovieId;
                                if (mpsMovieDTO.UserRating > 0)
                                    m.ActiveUserSettings.UserRating = mpsMovieDTO.UserRating;
                                m.Commit();
                            }
                        }
                        break;

                    case MPSActions.RemoveMovieFromCollection:
                        if (FirstMovie.MpsId != null && FirstMovie.MpsId != 0) {
                            logger.Info("Removing {0} from MPS collection", FirstMovie.Title);
                            MovingPicturesCore.Social.SocialAPI.RemoveMovieFromCollection((int)FirstMovie.MpsId);
                            FirstMovie.MpsId = null;
                        }
                        break;


                    case MPSActions.UpdateUserRating:
                        if (FirstMovie.MpsId != null && FirstMovie.MpsId != 0) {
                            int newRating = FirstMovie.ActiveUserSettings.UserRating.GetValueOrDefault(0);
                            logger.Info("Updating MPS movie rating for {0} to {1} stars", FirstMovie.Title, newRating);
                            MovingPicturesCore.Social.SocialAPI.SetMovieRating((int)FirstMovie.MpsId, newRating);
                        }
                        break;
                        
                    case MPSActions.WatchMovie:
                        if (FirstMovie.MpsId != null && FirstMovie.MpsId != 0) {
                            int newWatchCount = FirstMovie.ActiveUserSettings.WatchedCount;
                            logger.Info("Setting MPS watch count for {0} to {1}", FirstMovie.Title, newWatchCount);
                            MovingPicturesCore.Social.SocialAPI.WatchMovie((int)FirstMovie.MpsId, newWatchCount, true);
                        }
                        break;

                    case MPSActions.WatchMovieIgnoreStream:
                        if (FirstMovie.MpsId != null && FirstMovie.MpsId != 0) {
                            int newWatchCount = FirstMovie.ActiveUserSettings.WatchedCount;
                            logger.Info("Setting MPS watch count for {0} to {1}", FirstMovie.Title, newWatchCount);
                            MovingPicturesCore.Social.SocialAPI.WatchMovie((int)FirstMovie.MpsId, newWatchCount, false);
                        }
                        break;


                    case MPSActions.ProcessTaskList:
                        logger.Debug("Getting task list from MPS");
                        List<TaskListItem> taskList = MovingPicturesCore.Social.SocialAPI.GetUserTaskList();

                        if (taskList.Count > 0) {
                            logger.Debug("MPS Task list contains {0} items", taskList.Count);
                            List<DBMovieInfo> allMovies = DBMovieInfo.GetAll();

                            foreach (TaskListItem taskItem in taskList) {
                                logger.Debug("Checking for cover for movie {0}", taskItem.MovieId);
                                DBMovieInfo foundMovie = allMovies.Find(delegate(DBMovieInfo m) {
                                    return m.MpsId == taskItem.MovieId;
                                });

                                if (foundMovie != null && foundMovie.CoverFullPath.Trim().Length > 0) {
                                    logger.Debug("Cover found.  Uploading for movie {0}", taskItem.MovieId);
                                    MovingPicturesCore.Social.SocialAPI.UploadCover((int)foundMovie.MpsId, foundMovie.CoverFullPath);
                                }
                            }
                        }
                        break;
                    case MPSActions.GetUserSyncData:
                        logger.Debug("Getting user sync data from MPS");
                        DateTime lastRetrived;
                        if (!DateTime.TryParse(MovingPicturesCore.Settings.SocialLastRetrieved, out lastRetrived))
                            lastRetrived = DateTime.MinValue;
                        List<UserSyncData> allUsd = MovingPicturesCore.Social.SocialAPI.GetUserSyncData(lastRetrived);

                        if (allUsd.Count > 0)
                        {
                            logger.Debug("User Sync Data contains {0} items", allUsd.Count);
                            List<DBMovieInfo> allMovies = DBMovieInfo.GetAll();

                            foreach (UserSyncData usd in allUsd)
                            {
                                logger.Debug("movie {0} was rated {1} on MPS", usd.MovieId, usd.Rating);
                                DBMovieInfo foundMovie = allMovies.Find(delegate(DBMovieInfo m) {
                                    return m.MpsId == usd.MovieId;
                                });

                                if (foundMovie != null) {
                                    foundMovie.ActiveUserSettings.UserRating = usd.Rating;
                                    foundMovie.ActiveUserSettings.RatingChanged = false;
                                }
                                if (usd.RatingDate > lastRetrived)
                                    lastRetrived = usd.RatingDate;
                            }

                            MovingPicturesCore.Settings.SocialLastRetrieved = lastRetrived.ToString();
                        }
                        break;


                    default:
                        break;
                }
            }
            catch (Exception ex) {
                logger.ErrorException("", ex);
            }

        }
    }
}
