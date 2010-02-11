using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using System.IO;
using System.Threading;
using System.Drawing;

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

        public static Cover FromUrl(DBMovieInfo movie, string url, out ImageLoadResults status) {
            return FromUrl(movie, url, false, out status);
        }

        public static Cover FromUrl(DBMovieInfo movie, string url, bool ignoreRestrictions, out ImageLoadResults status) {
            ImageSize minSize = null;
            ImageSize maxSize = new ImageSize();

            if (!ignoreRestrictions) {
                minSize = new ImageSize();
                minSize.Width = MovingPicturesCore.Settings.MinimumCoverWidth;
                minSize.Height = MovingPicturesCore.Settings.MinimumCoverHeight;
            }

            maxSize.Width = MovingPicturesCore.Settings.MaximumCoverWidth;
            maxSize.Height = MovingPicturesCore.Settings.MaximumCoverHeight;

            bool redownload = MovingPicturesCore.Settings.RedownloadCoverArtwork;

            Cover newCover = new Cover();
            newCover.Filename = GenerateFilename(movie, url);
            status = newCover.FromUrl(url, ignoreRestrictions, minSize, maxSize, redownload);

            switch (status) {
                case ImageLoadResults.SUCCESS:
                    logger.Info("Added cover art for \"{0}\" from: {1}", movie.Title, url);
                    break;
                case ImageLoadResults.SUCCESS_REDUCED_SIZE:
                    logger.Info("Added resized cover art for \"{0}\" from: {1}", movie.Title, url);
                    break;
                case ImageLoadResults.FAILED_ALREADY_LOADED:
                    logger.Debug("Cover art for \"{0}\" from the following URL is already loaded: {1}", movie.Title, url);
                    return null;
                case ImageLoadResults.FAILED_TOO_SMALL:
                    logger.Debug("Downloaded cover art for \"{0}\" failed minimum resolution requirements: {1}", movie.Title, url);
                    return null;
                case ImageLoadResults.FAILED:
                    logger.Error("Failed downloading cover art for \"{0}\": {1}", movie.Title, url);
                    return null;
            }                       

            return newCover;
        }

        public static Cover FromFile(DBMovieInfo movie, string path) {
            throw new NotImplementedException();
        }
    }
}
