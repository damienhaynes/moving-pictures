using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MediaPortal.Plugins.MovingPictures.Database;

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
            string safeName = Utility.CreateFilename(movie.Title.Replace(' ', '.'));
            return artFolder + "\\{" + safeName + "} [" + source.GetHashCode() + "].jpg";
        }

        public static Backdrop FromUrl(DBMovieInfo movie, string url, out ArtworkLoadStatus status) {
            return FromUrl(movie, url, false, out status);
        }

        public static Backdrop FromUrl(DBMovieInfo movie, string url, bool ignoreRestrictions, out ArtworkLoadStatus status) {
            int minWidth = MovingPicturesCore.Settings.MinimumBackdropWidth;
            int minHeight = MovingPicturesCore.Settings.MinimumBackdropHeight;
            bool redownloadBackdrops = MovingPicturesCore.Settings.RedownloadBackdrops; 
            
            Backdrop newBackdrop = new Backdrop();
            newBackdrop.Filename = GenerateFilename(movie, url);

            // if we already have a file for this movie from this URL
            if (File.Exists(newBackdrop.Filename)) {
                // if we are redownloading, just delete what we have
                if (redownloadBackdrops) {
                    try {
                        File.Delete(newBackdrop.Filename);
                        File.Delete(newBackdrop.ThumbFilename);
                    }
                    catch (Exception) { }
                }
                // otherwise return an "already loaded" failure
                else {
                    logger.Debug("Backdrop art for '" + movie.Title + "' [" + movie.ID + "] already exists from " + url + ".");
                    status = ArtworkLoadStatus.ALREADY_LOADED;
                    return null;
                }
            }

            // try to grab the image if failed, exit
            bool success = newBackdrop.Download(url);
            if (!success) {
                logger.Error("Failed retrieving backdrop for '" + movie.Title + "' [" + movie.ID + "] from " + url + ".");
                status = ArtworkLoadStatus.FAILED;
                return null;
            }

            // check resolution
            System.Drawing.Image currImage = System.Drawing.Image.FromFile(newBackdrop.Filename);
            if (!ignoreRestrictions && (currImage.Width < minWidth || currImage.Height < minHeight)) {
                logger.Debug("Backdrop for '" + movie.Title + "' [" + movie.ID + "] failed minimum resolution requirements: " + url);
                currImage.Dispose();
                status = ArtworkLoadStatus.FAILED_RES_REQUIREMENTS;
                return null;
            }

            status = ArtworkLoadStatus.SUCCESS;
            return newBackdrop;
        }

        public static Cover FromFile(DBMovieInfo movie, string path) {
            throw new NotImplementedException();
        }

    }
}
