using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using System.IO;
using System.Threading;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieResources {
    public class Cover: ImageResource {

        public override string Filename {
            set {
                base.Filename = value;

                // build thumbnail filename
                string thumbsFolder = MovingPicturesCore.Settings.CoverArtThumbsFolder;
                FileInfo file = new FileInfo(Filename);
                ThumbFilename = thumbsFolder + "\\" + file.Name;
            }
        }

        // genrate a filename for a cover. should be unique based on the source hash
        private static string GenerateFilename(DBMovieInfo movie, string source) {
            string artFolder = MovingPicturesCore.Settings.CoverArtFolder;
            string safeName = Utility.CreateFilename(movie.Title.Replace(' ', '.'));
            return artFolder + "\\{" + safeName + "} [" + source.GetHashCode() + "].jpg";
        }

        public static Cover FromUrl(DBMovieInfo movie, string url, out ArtworkLoadStatus status) {
            return FromUrl(movie, url, false, out status);
        }

        public static Cover FromUrl(DBMovieInfo movie, string url, bool ignoreRestrictions, out ArtworkLoadStatus status) {
            int minWidth = MovingPicturesCore.Settings.MinimumCoverWidth;
            int minHeight = MovingPicturesCore.Settings.MinimumCoverHeight;
            bool redownloadCovers = MovingPicturesCore.Settings.RedownloadCoverArtwork;

            Cover newCover = new Cover();
            newCover.Filename = GenerateFilename(movie, url);

            // if we already have a file for this movie from this URL
            if (File.Exists(newCover.Filename)) {
                // if we are redownloading, just delete what we have
                if (redownloadCovers) {
                    try {
                        File.Delete(newCover.Filename);
                        File.Delete(newCover.ThumbFilename);
                    }
                    catch (Exception) {}
                }
                // otherwise return an "already loaded" failure
                else {
                    logger.Debug("Cover art for '" + movie.Title + "' [" + movie.ID + "] already exists from " + url + ".");
                    status = ArtworkLoadStatus.ALREADY_LOADED;
                    return null;
                }
            }

            // try to grab the image if failed, exit
            bool success = newCover.Download(url);
            if (!success) {
                logger.Error("Failed retrieving cover artwork for '" + movie.Title + "' [" + movie.ID + "] from " + url + ".");
                status = ArtworkLoadStatus.FAILED;
                return null;
            }

            // check resolution
            System.Drawing.Image currImage = System.Drawing.Image.FromFile(newCover.Filename);
            if (!ignoreRestrictions && (currImage.Width < minWidth || currImage.Height < minHeight)) {
                logger.Debug("Cover art for '" + movie.Title + "' [" + movie.ID + "] failed minimum resolution requirements: " + url);
                currImage.Dispose();
                status = ArtworkLoadStatus.FAILED_RES_REQUIREMENTS;
                return null;
            }

            status = ArtworkLoadStatus.SUCCESS;
            return newCover;
        }

        public static Cover FromFile(DBMovieInfo movie, string path) {
            throw new NotImplementedException();
        }
    }
}
