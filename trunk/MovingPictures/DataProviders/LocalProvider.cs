using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;
using System.Net;
using Cornerstone.Database;
using System.Web;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    public class LocalProvider : IMovieProvider {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private DBMovieInfo movie;

        // we should be using the movie object but we have to assign it before locking which 
        // is not good if the thread gets interupted after the asssignment, but vefore it gets 
        // locked. So we use this dumby var.
        private String lockObj = "";

        public string Name {
            get {
                return "Local Data";
            }
        }

        public bool ProvidesMoviesDetails {
            get { return false; }
        }

        public bool ProvidesCoverArt {
            get { return true; }
        }

        public bool ProvidesBackdrops {
            get { return true; }
        }

        public bool GetBackdrop(DBMovieInfo movie) {
            // if we already have a backdrop move on for now
            if (movie.BackdropFullPath.Trim().Length > 0)
                return false;

            // grab a list of possible filenames for the backdrop based on the user pattern
            string pattern = MovingPicturesCore.SettingsManager["local_backdrop_pattern"].StringValue;
            List<string> filenames = getPossibleNamesFromPattern(pattern, movie);


            // check the backdrop folder for the user patterned backdrops
            string backdropFolderPath = MovingPicturesCore.SettingsManager["backdrop_folder"].StringValue;
            FileInfo newBackdrop = getFirstFileFromFolder(backdropFolderPath, filenames);
            if (newBackdrop != null && newBackdrop.Exists) {
                movie.BackdropFullPath = newBackdrop.FullName;
                logger.Info("Loaded backdrop from " + newBackdrop.FullName);
                return true;
            }

            return false;
        }

        public bool GetArtwork(DBMovieInfo movie) {
            try {
                // grab a list of possible filenames for the coverart based on the user pattern
                string pattern = MovingPicturesCore.SettingsManager["local_coverart_pattern"].StringValue;
                List<string> filenames = getPossibleNamesFromPattern(pattern, movie);


                // check the coverart folder for the user patterned covers
                string coverartFolderPath = MovingPicturesCore.SettingsManager["cover_art_folder"].StringValue;
                FileInfo newCover = getFirstFileFromFolder(coverartFolderPath, filenames);
                if (newCover != null && newCover.Exists) {
                    movie.AddCoverFromFile(newCover.FullName);
                    return true;
                }

                // check for folder.jpg if the setting is turned on
                bool useFolderJpg = (bool)MovingPicturesCore.SettingsManager["local_use_folder_jpg"].Value;
                if (useFolderJpg) {
                    newCover = new FileInfo(movie.LocalMedia[0].File.DirectoryName + "\\" + "folder.jpg");
                    if (newCover.Exists) {
                        movie.AddCoverFromFile(newCover.FullName);
                        return true;
                    }

                    newCover = new FileInfo(movie.LocalMedia[0].File.DirectoryName + "\\" + "folder.png");
                    if (newCover.Exists) {
                        movie.AddCoverFromFile(newCover.FullName);
                        return true;
                    }


                    newCover = new FileInfo(movie.LocalMedia[0].File.DirectoryName + "\\" + "folder.bmp");
                    if (newCover.Exists) {
                        movie.AddCoverFromFile(newCover.FullName);
                        return true;
                    }
                }

                // check for coverart in the coverfolder loaded from previous installs
                DirectoryInfo coverFolder = new DirectoryInfo(coverartFolderPath);
                string safeName = HttpUtility.UrlEncode(movie.Title.Replace(' ', '.'));
                Regex oldCoverRegex = new Regex("{?" + safeName + "}? \\[-?\\d+\\].jpg");
                foreach (FileInfo currFile in coverFolder.GetFiles()) {
                    if (oldCoverRegex.IsMatch(currFile.Name)) {
                        movie.AddCoverFromFile(currFile.FullName);
                        logger.Info("Added old cover found in cover folder: " + currFile.Name); 
                    }
                }

            }
            catch (Exception) {
                logger.Warn("Unexpected problem loading artwork via LocalProvider.");
            }

            return false;
        }


        // parses and replaces variables from a filename based on the pattern supplied
        // returning a list of possible file matches
        private List<string> getPossibleNamesFromPattern(string pattern, DBMovieInfo movie) {
            // try to create our filename(s)
            this.movie = movie;
            lock (lockObj){
                Regex parser = new Regex("%(.*?)%", RegexOptions.IgnoreCase);
                List<string> filenames = new List<string>();
                foreach (string currPattern in pattern.Split('|')) {
                    filenames.Add(parser.Replace(currPattern, new MatchEvaluator(dbNameParser)).Trim().ToLower());
                }
                return filenames;
            }
        }

        // based on the filename list, returns the first file in the folder, otherwise null
        private FileInfo getFirstFileFromFolder(string folder, List<string> filenames) {
            foreach (string currFilename in filenames) {
                FileInfo newImage = new FileInfo(folder + "\\" + currFilename);
                if (newImage.Exists) 
                    return newImage;
            }

            return null;
        }

        public List<DBMovieInfo> Get(MovieSignature movieSignature) {
            throw new NotImplementedException();
        }

        public void Update(DBMovieInfo movie) {
            throw new NotImplementedException();
        }


        private string dbNameParser(Match match) {
            // try to grab the field object
            string fieldName = match.Value.Substring(1, match.Length - 2);
            DBField field = DBField.GetFieldByDBName(typeof(DBMovieInfo), fieldName);
            
            // if no dice, the user probably entered an invalid string.
            if (field == null) {
                logger.Error("Error parsing \"" + match.Value + "\" from local_backdrop_pattern advanced setting. Not a database field name.");
                return match.Value;
            }

            return field.GetValue(movie).ToString();
        }

    }
}
