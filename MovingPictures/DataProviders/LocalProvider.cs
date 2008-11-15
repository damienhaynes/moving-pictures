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
using System.Reflection;
using System.Threading;
using System.Globalization;

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

        public string Version {
            get {
                return "Internal";
            }
        }

        public string Author {
            get { return "Moving Pictures Team"; }
        }

        public string Description {
            get { return "Returns artwork and backdrops already available on the local system."; }
        }

        public string Language {
            get { return ""; }
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

            bool found = false;
            
            found &= getBackdropsFromBackdropFolder(movie);
            found &= getBackdropsFromMovieFolder(movie);
            found &= getOldBackdrops(movie);

            return found;
        }

        private bool getBackdropsFromBackdropFolder(DBMovieInfo movie) {
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

        // if flagged, check for backdrops in the movie folder based on the user defined pattern
        private bool getBackdropsFromMovieFolder(DBMovieInfo movie) {
            bool found = false;

            bool useMovieFolderBackdrops = (bool)MovingPicturesCore.SettingsManager["local_backdrop_from_movie_folder"].Value;
            string pattern = MovingPicturesCore.SettingsManager["local_moviefolder_backdrop_pattern"].StringValue;

            if (useMovieFolderBackdrops) {
                List<string> movieFolderFilenames = getPossibleNamesFromPattern(pattern, movie);
                foreach (string currFile in movieFolderFilenames) {
                    FileInfo newBackdrop = new FileInfo(movie.LocalMedia[0].File.DirectoryName + "\\" + currFile);
                    if (newBackdrop.Exists)
                        found &= movie.AddBackdropFromFile(newBackdrop.FullName);
                }
            }

            return found;
        }

        
        // check for backdrops in the backdrop folder loaded from previous installs
        private bool getOldBackdrops(DBMovieInfo movie) {
            bool found = false;
            
            string backdropFolderPath = MovingPicturesCore.SettingsManager["backdrop_folder"].StringValue;
            DirectoryInfo backdropFolder = new DirectoryInfo(backdropFolderPath);
            string safeName = HttpUtility.UrlEncode(movie.Title.Replace(' ', '.'));
            Regex oldBackdropRegex = new Regex("{?" + safeName + "}? \\[-?\\d+\\].jpg");
            foreach (FileInfo currFile in backdropFolder.GetFiles()) {
                if (oldBackdropRegex.IsMatch(currFile.Name)) {
                    found &= movie.AddBackdropFromFile(currFile.FullName);
                    logger.Info("Added old backdrop found in cover folder: " + currFile.Name);
                }
            }

            return found;
        }

        public bool GetArtwork(DBMovieInfo movie) {
            try {
                bool found = false;

                found &= getCoversFromCoverFolder(movie);
                found &= getCoversFromMovieFolder(movie);
                found &= getOldCovers(movie);

                return found;
            }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;
                logger.Warn("Unexpected problem loading artwork via LocalProvider.");
            }

            return false;
        }

        private bool getCoversFromCoverFolder(DBMovieInfo movie) {
            // grab a list of possible filenames for the coverart based on the user pattern
            string pattern = MovingPicturesCore.SettingsManager["local_coverart_pattern"].StringValue;
            List<string> coverFolderFilenames = getPossibleNamesFromPattern(pattern, movie);


            // check the coverart folder for the user patterned covers
            string coverartFolderPath = MovingPicturesCore.SettingsManager["cover_art_folder"].StringValue;
            FileInfo newCover = getFirstFileFromFolder(coverartFolderPath, coverFolderFilenames);
            if (newCover != null && newCover.Exists) {
                return movie.AddCoverFromFile(newCover.FullName);
            }

            return false;
        }

        // if flagged, check for covers in the movie folder based on the user defined pattern
        private bool getCoversFromMovieFolder(DBMovieInfo movie) {
            bool found = false;

            bool useMovieFolderCovers = (bool)MovingPicturesCore.SettingsManager["local_cover_from_movie_folder"].Value;
            string pattern = MovingPicturesCore.SettingsManager["local_moviefolder_coverart_pattern"].StringValue;

            if (useMovieFolderCovers) {
                List<string> movieFolderFilenames = getPossibleNamesFromPattern(pattern, movie);
                foreach (string currFile in movieFolderFilenames) {
                    FileInfo newCover = new FileInfo(movie.LocalMedia[0].File.DirectoryName + "\\" + currFile);
                    if (newCover.Exists)
                        found &= movie.AddCoverFromFile(newCover.FullName);
                }
            }

            return found;
        }

        // check for coverart in the coverfolder loaded from previous installs
        private bool getOldCovers(DBMovieInfo movie) {
            bool found = false;
            
            string coverartFolderPath = MovingPicturesCore.SettingsManager["cover_art_folder"].StringValue;
            DirectoryInfo coverFolder = new DirectoryInfo(coverartFolderPath);
            string safeName = HttpUtility.UrlEncode(movie.Title.Replace(' ', '.'));
            Regex oldCoverRegex = new Regex("{?" + safeName + "}? \\[-?\\d+\\].jpg");
            foreach (FileInfo currFile in coverFolder.GetFiles()) {
                if (oldCoverRegex.IsMatch(currFile.Name)) {
                    found &= movie.AddCoverFromFile(currFile.FullName);
                    logger.Info("Added old cover found in cover folder: " + currFile.Name);
                }
            }

            return found;
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

        public UpdateResults Update(DBMovieInfo movie) {
            throw new NotImplementedException();
        }

    }
}
