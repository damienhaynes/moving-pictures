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
using MediaPortal.Plugins.MovingPictures.SignatureBuilders;
using System.Reflection;
using System.Threading;
using System.Globalization;
using Cornerstone.Extensions;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    public class LocalProvider : IMovieProvider {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private DBMovieInfo movie;

        // we should be using the movie object but we have to assign it before locking which 
        // is not good if the thread gets interupted after the asssignment, but before it gets 
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

        public string LanguageCode {
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
            if (movie == null) 
                return false;

            // if we already have a backdrop move on for now
            if (movie.BackdropFullPath.Trim().Length > 0)
                return false;

            bool found = false;
            
            found |= getBackdropsFromBackdropFolder(movie);
            found |= getBackdropsFromMovieFolder(movie);
            found |= getOldBackdrops(movie);

            return found;
        }

        private bool getBackdropsFromBackdropFolder(DBMovieInfo movie) {
            if (movie == null) 
                return false;

            // grab a list of possible filenames for the backdrop based on the user pattern
            string pattern = MovingPicturesCore.Settings.BackdropFilenamePattern;
            List<string> filenames = getPossibleNamesFromPattern(pattern, movie);

            // check the backdrop folder for the user patterned backdrops
            string backdropFolderPath = MovingPicturesCore.Settings.BackdropFolder;
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
            // should never really happen, but if the database is corrupt, 
            // we dont want to crash, so just quit.
            if (movie.LocalMedia.Count == 0)
                return false;

            bool found = false;

            bool useMovieFolderBackdrops = MovingPicturesCore.Settings.SearchMovieFolderForBackdrops;
            string pattern = MovingPicturesCore.Settings.MovieFolderBackdropFilenamePattern;

            if (useMovieFolderBackdrops) {
                List<string> movieFolderFilenames = getPossibleNamesFromPattern(pattern, movie);
                foreach (string currFile in movieFolderFilenames) {
                    FileInfo newBackdrop = new FileInfo(Utility.GetMovieBaseDirectory(movie.LocalMedia[0].File.Directory).FullName + "\\" + currFile);
                    if (newBackdrop.Exists)
                        found |= movie.AddBackdropFromFile(newBackdrop.FullName);
                }
            }

            return found;
        }

        
        // check for backdrops in the backdrop folder loaded from previous installs
        private bool getOldBackdrops(DBMovieInfo movie) {
            bool found = false;

            string backdropFolderPath = MovingPicturesCore.Settings.BackdropFolder;
            DirectoryInfo backdropFolder = new DirectoryInfo(backdropFolderPath);

            string safeName = movie.Title.Replace(' ', '.').ToValidFilename();
            Regex oldBackdropRegex = new Regex("^{?" + Regex.Escape(safeName) + "}? \\[-?\\d+\\]\\.(jpg|png)");

            foreach (FileInfo currFile in backdropFolder.GetFiles()) {
                if (oldBackdropRegex.IsMatch(currFile.Name)) {
                    found |= movie.AddBackdropFromFile(currFile.FullName);
                }
            }

            return found;
        }

        public bool GetArtwork(DBMovieInfo movie) {
            try {
                bool found = false;

                found |= getCoversFromCoverFolder(movie);
                found |= getCoversFromMovieFolder(movie);
                found |= getOldCovers(movie);

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
            string pattern = MovingPicturesCore.Settings.CoverArtworkFilenamePattern;
            List<string> coverFolderFilenames = getPossibleNamesFromPattern(pattern, movie);


            // check the coverart folder for the user patterned covers
            string coverartFolderPath = MovingPicturesCore.Settings.CoverArtFolder;
            FileInfo newCover = getFirstFileFromFolder(coverartFolderPath, coverFolderFilenames);
            if (newCover != null && newCover.Exists) {
                return movie.AddCoverFromFile(newCover.FullName);
            }

            return false;
        }

        // if flagged, check for covers in the movie folder based on the user defined pattern
        private bool getCoversFromMovieFolder(DBMovieInfo movie) {
            bool found = false;

            bool useMovieFolderCovers = MovingPicturesCore.Settings.SearchMovieFolderForCoverArt;
            string pattern = MovingPicturesCore.Settings.MovieFolderCoverArtworkFilenamePattern;

            if (useMovieFolderCovers) {
                List<string> movieFolderFilenames = getPossibleNamesFromPattern(pattern, movie);
                foreach (string currFile in movieFolderFilenames) {
                    FileInfo newCover = new FileInfo(Utility.GetMovieBaseDirectory(movie.LocalMedia[0].File.Directory).FullName + "\\" + currFile);
                    if (newCover.Exists)
                        found |= movie.AddCoverFromFile(newCover.FullName);
                }
            }

            return found;
        }

        // check for coverart in the coverfolder loaded from previous installs
        private bool getOldCovers(DBMovieInfo movie) {
            bool found = false;

            string coverartFolderPath = MovingPicturesCore.Settings.CoverArtFolder;
            DirectoryInfo coverFolder = new DirectoryInfo(coverartFolderPath);
            
            string safeName = movie.Title.Replace(' ', '.').ToValidFilename();
            Regex oldCoverRegex = new Regex("^{?" + Regex.Escape(safeName) + "}? \\[-?\\d+\\]\\.(jpg|png)");
            
            foreach (FileInfo currFile in coverFolder.GetFiles()) {
                if (oldCoverRegex.IsMatch(currFile.Name)) {
                    bool success = movie.AddCoverFromFile(currFile.FullName);
                    found = found || success;
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
                    // replace db field patterns
                    string filename = parser.Replace(currPattern, new MatchEvaluator(dbNameParser)).Trim().ToLower();

                    // replace %filename% pattern
                    if (movie.LocalMedia.Count > 0) {
                        string videoFileName = Path.GetFileNameWithoutExtension(movie.LocalMedia[0].File.Name);
                        filename = filename.Replace("%filename%", videoFileName);
                    }

                    filenames.Add(filename.ToValidFilename());
                }
                return filenames;
            }
        }

        private string dbNameParser(Match match) {
            // try to grab the field object
            string fieldName = match.Value.Substring(1, match.Length - 2);
            DBField field = DBField.GetFieldByDBName(typeof(DBMovieInfo), fieldName);

            // if no dice, the user probably entered an invalid string.
            if (field == null && match.Value != "%filename") {
                logger.Error("Error parsing \"" + match.Value + "\" from local_backdrop_pattern advanced setting. Not a database field name.");
                return match.Value;
            }

            return (field.GetValue(movie) ?? string.Empty).ToString();
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
