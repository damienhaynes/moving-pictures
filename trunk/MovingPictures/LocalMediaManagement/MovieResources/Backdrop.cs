using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MediaPortal.Plugins.MovingPictures.Database;
using Cornerstone.Extensions;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieResources {
    public class Backdrop: ImageResource {
        public override string Filename {
            set {
                base.Filename = value;

                // build thumbnail filename
                string thumbsFolder = MovingPicturesCore.Settings.BackdropThumbsFolder;
                FileInfo file = new FileInfo(Filename);
                ThumbFilename = thumbsFolder + "\\" + file.Name;
            }
        }

        // genrate a filename for a backdrop. should be unique based on the source hash
        private static string GenerateFilename(DBMovieInfo movie, string source) {
            string artFolder = MovingPicturesCore.Settings.BackdropFolder;
            string safeName = movie.Title.Replace(' ', '.').ToValidFilename();
            return artFolder + "\\{" + safeName + "} [" + source.GetHashCode() + "].jpg";
        }

        public static Backdrop FromUrl(DBMovieInfo movie, string url, out ImageLoadResults status) {
            return FromUrl(movie, url, false, out status);
        }

        public static Backdrop FromUrl(DBMovieInfo movie, string url, bool ignoreRestrictions, out ImageLoadResults status) {
            ImageSize minSize = null;
            ImageSize maxSize = new ImageSize();

            if (!ignoreRestrictions) {
                minSize = new ImageSize();
                minSize.Width = MovingPicturesCore.Settings.MinimumBackdropWidth;
                minSize.Height = MovingPicturesCore.Settings.MinimumBackdropHeight;
            }

            maxSize.Width = MovingPicturesCore.Settings.MaximumBackdropWidth;
            maxSize.Height = MovingPicturesCore.Settings.MaximumBackdropHeight;

            bool redownload = MovingPicturesCore.Settings.RedownloadBackdrops;

            Backdrop newBackdrop = new Backdrop();
            newBackdrop.Filename = GenerateFilename(movie, url);
            status = newBackdrop.FromUrl(url, ignoreRestrictions, minSize, maxSize, redownload);

            switch (status) {
                case ImageLoadResults.SUCCESS:
                    logger.Info("Added backdrop for \"{0}\" from: {1}", movie.Title, url);
                    break;
                case ImageLoadResults.SUCCESS_REDUCED_SIZE:
                    logger.Debug("Added resized backdrop for \"{0}\" from: {1}", movie.Title, url);
                    break;
                case ImageLoadResults.FAILED_ALREADY_LOADED:
                    logger.Debug("Backdrop for \"{0}\" from the following URL is already loaded: {1}", movie.Title, url);
                    return null;
                case ImageLoadResults.FAILED_TOO_SMALL:
                    logger.Error("Downloaded backdrop for \"{0}\" failed minimum resolution requirements: {1}", movie.Title, url);
                    return null;
                case ImageLoadResults.FAILED:
                    logger.Error("Failed downloading backdrop for \"{0}\": {1}", movie.Title, url);
                    return null;
            }

            return newBackdrop;
        }

        public static Cover FromFile(DBMovieInfo movie, string path) {
            throw new NotImplementedException();
        }

    }
}
