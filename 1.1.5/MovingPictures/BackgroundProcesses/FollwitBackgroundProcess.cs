using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Tools;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;
using System.Threading;
using System.Net;
using Follwit.API.Data;

namespace MediaPortal.Plugins.MovingPictures.BackgroundProcesses {
    public enum FitActions {
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
    public class FollwitBackgroundProcess : AbstractBackgroundProcess {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public override string Name {
            get { return "follw.it Background Process"; }
        }

        public override string Description {
            get {
                return "This process sends data to follw.it.";
            }
        }

        public FitActions Action;
        public List<DBMovieInfo> Movies = new List<DBMovieInfo>();
        private DBMovieInfo FirstMovie {
            get {
                return Movies[0];
            }
        }

        public override void Work() {
            try {
                if (MovingPicturesCore.Settings.FollwitEnabled == false || !MovingPicturesCore.Follwit.IsOnline) {
                    logger.Warn("Attempting to perform follw.it actions when feature is disabled or when server is unavailable.");
                    return;
                }

                switch (Action) {
                    case FitActions.AddMoviesToCollection:
                        List<FitMovie> movieDTOs = new List<FitMovie>();
                        foreach (DBMovieInfo movie in Movies) {
                            logger.Info("Adding {0} to follw.it Collection", movie.Title);
                            movieDTOs.Add(FollwitConnector.MovieToFitMovie(movie));
                        }
                        MovingPicturesCore.Follwit.FollwitApi.AddMoviesToCollection(ref movieDTOs);
                        // update FitId on the DBMovieInfo object
                        foreach (FitMovie fitMovieDTO in movieDTOs) {
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
                        break;

                    case FitActions.RemoveMovieFromCollection:
                        if (FirstMovie.FitId != null && FirstMovie.FitId != 0) {
                            logger.Info("Removing {0} from follw.it collection", FirstMovie.Title);
                            MovingPicturesCore.Follwit.FollwitApi.RemoveMovieFromCollection((int)FirstMovie.FitId);
                            FirstMovie.FitId = null;
                        }
                        break;


                    case FitActions.UpdateUserRating:
                        if (FirstMovie.FitId != null && FirstMovie.FitId != 0) {
                            int newRating = FirstMovie.ActiveUserSettings.UserRating.GetValueOrDefault(0);
                            logger.Info("Updating follw.it movie rating for {0} to {1} stars", FirstMovie.Title, newRating);
                            MovingPicturesCore.Follwit.FollwitApi.SetMovieRating((int)FirstMovie.FitId, newRating);
                        }
                        break;

                    case FitActions.BeginWatching:
                        if (FirstMovie.FitId != null && FirstMovie.FitId != 0) {
                            logger.Info("Notifying follw.it you are watching '{0}'.", FirstMovie.Title);
                            MovingPicturesCore.Follwit.FollwitApi.WatchingMovie((int)FirstMovie.FitId);
                        }
                        break;
                    
                    case FitActions.EndWatching:
                        if (FirstMovie.FitId != null && FirstMovie.FitId != 0) {
                            logger.Info("Notifying follw.it you are done watching '{0}'.", FirstMovie.Title);
                            MovingPicturesCore.Follwit.FollwitApi.StopWatchingMovie((int)FirstMovie.FitId);
                        }
                        break;
                    
                    case FitActions.WatchMovie:
                        if (FirstMovie.FitId != null && FirstMovie.FitId != 0) {
                            logger.Info("Setting follw.it watched for {0}", FirstMovie.Title);
                            MovingPicturesCore.Follwit.FollwitApi.WatchMovie((int)FirstMovie.FitId, true);
                        }
                        break;

                    case FitActions.WatchMovieIgnoreStream:
                        if (FirstMovie.FitId != null && FirstMovie.FitId != 0) {
                            logger.Info("Setting follw.it watched for {0}", FirstMovie.Title);
                            MovingPicturesCore.Follwit.FollwitApi.WatchMovie((int)FirstMovie.FitId, false);
                        }
                        break;


                    case FitActions.UnwatchMovie:
                        if (FirstMovie.FitId != null && FirstMovie.FitId != 0) {
                            logger.Info("Setting follw.it unwatched for {0}", FirstMovie.Title);
                            MovingPicturesCore.Follwit.FollwitApi.UnwatchMovie((int)FirstMovie.FitId);
                        }
                        break;

                    case FitActions.ProcessTaskList:
                        MovingPicturesCore.Follwit.ProcessTasks();
                        break;

                    default:
                        break;
                }
            }
            catch (WebException ex) {
                logger.Error("There was a problem connecting to the follw.it Server! " + ex.Message);
                MovingPicturesCore.Follwit.Status = FollwitConnector.StatusEnum.CONNECTION_ERROR;
            }
            catch (Exception ex) {
                logger.ErrorException("Unexpected error connecting to follw.it.\n", ex);
                MovingPicturesCore.Follwit.Status = FollwitConnector.StatusEnum.INTERNAL_ERROR;
            }

        }
    }
}
