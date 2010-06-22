using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Tools;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;
using System.Threading;
using MovingPicturesSocialAPI;

namespace MediaPortal.Plugins.MovingPictures.BackgroundProcesses {
    public enum MPSActions {
        AddMoviesToCollection,
        RemoveMovieFromCollection,
        UpdateUserRating,
        SetWatchCount,
        ProcessTaskList
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
            switch (Action) {
                case MPSActions.AddMoviesToCollection:
                    List<MovieDTO> movieDTOs = new List<MovieDTO>();
                    foreach (DBMovieInfo movie in Movies)
	                {
                        logger.Info("Adding {0} to Moving Pictures Social Collection", movie.Title);
                        movieDTOs.Add(MovingPicturesCore.Social.MovieToMPSMovie(movie));
	                }
                    MovingPicturesCore.Social.SocialAPI.AddMoviesToCollection(movieDTOs);
                    break;

                case MPSActions.RemoveMovieFromCollection:
                    logger.Info("Removing {0} from MPS collection", FirstMovie.Title);
                    string sourceName = FirstMovie.PrimarySource.Provider.Name;
                    string sourceId = FirstMovie.GetSourceMovieInfo(FirstMovie.PrimarySource).Identifier;
                    MovingPicturesCore.Social.SocialAPI.RemoveMovieFromCollection(sourceName, sourceId);
                    break;


                case MPSActions.UpdateUserRating:
                    int newRating = FirstMovie.ActiveUserSettings.UserRating.GetValueOrDefault(0);
                    logger.Info("Updating MPS movie rating for {0} to {1} stars", FirstMovie.Title, newRating);
                    string sourceName2 = FirstMovie.PrimarySource.Provider.Name;
                    string sourceId2 = FirstMovie.GetSourceMovieInfo(FirstMovie.PrimarySource).Identifier;
                    MovingPicturesCore.Social.SocialAPI.SetMovieRating(sourceName2, sourceId2, newRating);
                    break;

                case MPSActions.SetWatchCount:
                    int newWatchCount = FirstMovie.ActiveUserSettings.WatchedCount;
                    logger.Info("Setting MPS watch count for {0} to {1}", FirstMovie.Title, newWatchCount);
                    string sourceName3 = FirstMovie.PrimarySource.Provider.Name;
                    string sourceId3 = FirstMovie.GetSourceMovieInfo(FirstMovie.PrimarySource).Identifier;
                    MovingPicturesCore.Social.SocialAPI.SetWatchCount(sourceName3, sourceId3, newWatchCount);
                    break;


                case MPSActions.ProcessTaskList:
                    logger.Debug("Getting task list from MPS");
                    List<TaskListItem> taskList = MovingPicturesCore.Social.SocialAPI.GetUserTaskList();

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
                                MovingPicturesCore.Social.SocialAPI.UploadCover(taskItem.SourceName, taskItem.SourceId
                                    , foundMovie.CoverFullPath);
                            }
                        }
                    }
                    break;



                default:
                    break;
            }
        }
    }
}
