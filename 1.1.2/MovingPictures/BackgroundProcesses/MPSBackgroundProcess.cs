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
using System.Net;

namespace MediaPortal.Plugins.MovingPictures.BackgroundProcesses {
    public enum MPSActions {
        AddMoviesToCollection,
        RemoveMovieFromCollection,
        UpdateUserRating,
        BeginWatching,
        EndWatching,
        WatchMovie,
        WatchMovieIgnoreStream,
        UnwatchMovie,
        ProcessTaskList
    }
    public class MPSBackgroundProcess : AbstractBackgroundProcess {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public override string Name {
            get { return "follw.it Background Process"; }
        }

        public override string Description {
            get {
                return "This process sends data to follw.it.";
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
                if (MovingPicturesCore.Settings.SocialEnabled == false || !MovingPicturesCore.Social.IsOnline) {
                    logger.Warn("Attempting to perform follw.it actions when feature is disabled or when server is unavailable.");
                    return;
                }

                switch (Action) {
                    case MPSActions.AddMoviesToCollection:
                        List<MpsMovie> movieDTOs = new List<MpsMovie>();
                        foreach (DBMovieInfo movie in Movies) {
                            logger.Info("Adding {0} to follw.it Collection", movie.Title);
                            movieDTOs.Add(Social.MovieToMPSMovie(movie));
                        }
                        MovingPicturesCore.Social.SocialAPI.AddMoviesToCollection(ref movieDTOs);
                        // update MpsId on the DBMovieInfo object
                        foreach (MpsMovie mpsMovieDTO in movieDTOs) {
                            DBMovieInfo m = DBMovieInfo.Get(mpsMovieDTO.InternalId);
                            if (m != null) {
                                m.MpsId = mpsMovieDTO.MovieId;
                                if (mpsMovieDTO.UserRating > 0)
                                    m.ActiveUserSettings.UserRating = mpsMovieDTO.UserRating;
                                if (mpsMovieDTO.Watched && m.ActiveUserSettings.WatchedCount == 0)
                                    m.ActiveUserSettings.WatchedCount = 1;
                                m.ActiveUserSettings.WatchCountChanged = false;
                                m.ActiveUserSettings.RatingChanged = false;
                                m.Commit();
                            }
                        }
                        break;

                    case MPSActions.RemoveMovieFromCollection:
                        if (FirstMovie.MpsId != null && FirstMovie.MpsId != 0) {
                            logger.Info("Removing {0} from follw.it collection", FirstMovie.Title);
                            MovingPicturesCore.Social.SocialAPI.RemoveMovieFromCollection((int)FirstMovie.MpsId);
                            FirstMovie.MpsId = null;
                        }
                        break;


                    case MPSActions.UpdateUserRating:
                        if (FirstMovie.MpsId != null && FirstMovie.MpsId != 0) {
                            int newRating = FirstMovie.ActiveUserSettings.UserRating.GetValueOrDefault(0);
                            logger.Info("Updating follw.it movie rating for {0} to {1} stars", FirstMovie.Title, newRating);
                            MovingPicturesCore.Social.SocialAPI.SetMovieRating((int)FirstMovie.MpsId, newRating);
                        }
                        break;

                    case MPSActions.BeginWatching:
                        if (FirstMovie.MpsId != null && FirstMovie.MpsId != 0) {
                            logger.Info("Notifying follw.it you are watching '{0}'.", FirstMovie.Title);
                            MovingPicturesCore.Social.SocialAPI.WatchingMovie((int)FirstMovie.MpsId);
                        }
                        break;
                    
                    case MPSActions.EndWatching:
                        if (FirstMovie.MpsId != null && FirstMovie.MpsId != 0) {
                            logger.Info("Notifying follw.it you are done watching '{0}'.", FirstMovie.Title);
                            MovingPicturesCore.Social.SocialAPI.StopWatchingMovie((int)FirstMovie.MpsId);
                        }
                        break;
                    
                    case MPSActions.WatchMovie:
                        if (FirstMovie.MpsId != null && FirstMovie.MpsId != 0) {
                            logger.Info("Setting follw.it watched for {0}", FirstMovie.Title);
                            MovingPicturesCore.Social.SocialAPI.WatchMovie((int)FirstMovie.MpsId, true);
                        }
                        break;

                    case MPSActions.WatchMovieIgnoreStream:
                        if (FirstMovie.MpsId != null && FirstMovie.MpsId != 0) {
                            logger.Info("Setting follw.it watched for {0}", FirstMovie.Title);
                            MovingPicturesCore.Social.SocialAPI.WatchMovie((int)FirstMovie.MpsId, false);
                        }
                        break;


                    case MPSActions.UnwatchMovie:
                        if (FirstMovie.MpsId != null && FirstMovie.MpsId != 0) {
                            logger.Info("Setting follw.it unwatched for {0}", FirstMovie.Title);
                            MovingPicturesCore.Social.SocialAPI.UnwatchMovie((int)FirstMovie.MpsId);
                        }
                        break;

                    case MPSActions.ProcessTaskList:
                        MovingPicturesCore.Social.ProcessTasks();
                        break;

                    default:
                        break;
                }
            }
            catch (WebException ex) {
                logger.Error("There was a problem connecting to the follw.it Server! " + ex.Message);
                MovingPicturesCore.Social.Status = Social.StatusEnum.CONNECTION_ERROR;
            }
            catch (Exception ex) {
                logger.ErrorException("Unexpected error connecting to follw.it.\n", ex);
                MovingPicturesCore.Social.Status = Social.StatusEnum.INTERNAL_ERROR;
            }

        }
    }
}
