using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Tools;
using NLog;
using MediaPortal.Plugins.MovingPictures.Database;
using System.IO;
using System.Threading;
using MediaPortal.Plugins.MovingPictures.DataProviders;

namespace MediaPortal.Plugins.MovingPictures.BackgroundProcesses {
    internal class UpdateArtworkProcess: AbstractBackgroundProcess {
        private static Logger logger = LogManager.GetCurrentClassLogger();


        public override string Name {
            get { return "Artwork Updater"; }
        }

        public override string Description {
            get { return "This process removes invalid artwork references and attempts to " +
                         "retrieve artwork for movies currently missing a cover or backdrop."; }
        }

        public override void Work() {
            logger.Info("Beginging artwork updater background process.");

            RemoveOrphanArtwork();
            LookForMissingArtwork();

            logger.Info("Background artwork updater process complete.");
        }

        // Removes Artwork From a Movie
        public void RemoveOrphanArtwork() {
            float count = 0;
            float total = DBMovieInfo.GetAll().Count;

            foreach (DBMovieInfo currMovie in DBMovieInfo.GetAll()) {
                //OnProgress(count / total);
                count++;

                if (currMovie.ID == null)
                    continue;

                // get the list of elements to remove
                List<string> toRemove = new List<string>();
                foreach (string currCoverPath in currMovie.AlternateCovers) {
                    if (!new FileInfo(currCoverPath).Exists)
                        toRemove.Add(currCoverPath);
                }

                // remove them
                foreach (string currItem in toRemove) {
                    currMovie.AlternateCovers.Remove(currItem);
                }

                // reset default cover is needed
                if (!currMovie.AlternateCovers.Contains(currMovie.CoverFullPath))
                    if (currMovie.AlternateCovers.Count == 0)
                        currMovie.CoverFullPath = " ";
                    else
                        currMovie.CoverFullPath = currMovie.AlternateCovers[0];

                // get rid of the backdrop link if it doesnt exist
                if (currMovie.BackdropFullPath.Trim().Length > 0 && !new FileInfo(currMovie.BackdropFullPath).Exists)
                    currMovie.BackdropFullPath = " ";

                currMovie.Commit();
            }

            //OnProgress(1.0);
        }

        private void LookForMissingArtwork() {
            foreach (DBMovieInfo currMovie in DBMovieInfo.GetAll()) {
                try {
                    if (currMovie.ID == null)
                        continue;

                    if (currMovie.CoverFullPath.Trim().Length == 0) {
                        MovingPicturesCore.DataProviderManager.GetArtwork(currMovie);
                        
                        // because this operation can take some time we check again
                        // if the movie was not deleted while we were getting artwork
                        if (currMovie.ID == null)
                            continue;
                        
                        currMovie.Commit();
                    }

                    if (currMovie.BackdropFullPath.Trim().Length == 0) {
                        new LocalProvider().GetBackdrop(currMovie);
                        MovingPicturesCore.DataProviderManager.GetBackdrop(currMovie);
                        
                        // because this operation can take some time we check again
                        // if the movie was not deleted while we were getting the backdrop
                        if (currMovie.ID == null)
                            continue;
                        
                        currMovie.Commit();
                    }
                }
                catch (Exception e) {
                    if (e is ThreadAbortException)
                        throw e;

                    logger.ErrorException("Error retrieving artwork for " + currMovie.Title, e);
                }
            }
        }
    }
}
