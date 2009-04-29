using System;
using System.Collections.Generic;
using System.IO;
using Cornerstone.Tools;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.SignatureBuilders {

    /// <summary>
    /// a movie signature object that is used as input to a dataprovider.
    /// </summary>
    public class MovieSignature {

        #region Private Variables

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public properties

        public string Title { // ex. "Pirates of Silicon Valley"
            get { return title; }
            set {
                if (value != null)
                    title = value.Trim();
                if (title == string.Empty)
                    title = null;
            }
        } private string title = null;

        public int? Year = null; // ex. 1999

        public string ImdbId { // ex. "tt0168122"
            get { return imdb_id; }
            set {
                if (value != null)
                    imdb_id = value.Trim();
                if (imdb_id == string.Empty)
                    imdb_id = null;
            }
        } private string imdb_id = null;

        public string DiscId { // ex. (16 character hash of a DVD)
            get {
                if (LocalMedia != null)
                    discid = LocalMedia[0].DiscId;
                return discid;
            }
            set { discid = value; }
        } private string discid = null;

        public string MovieHash { // ex. (16 character hash of a movie file)
            get {
                if (LocalMedia != null)
                    filehash = LocalMedia[0].FileHash;
                return filehash;
            }
            set { filehash = value; }
        } private string filehash = null;
        
        public List<DBLocalMedia> LocalMedia = null; // LocalMedia collection

        #region Read-only

        // base folder
        public string Folder {
            get {
                if (folder == null)
                    updatePropertiesFromLocalMedia();

                return folder;
            }
        } private string folder = null;

        // base file
        public string File {
            get {
                if (file == null)
                    updatePropertiesFromLocalMedia();

                return file;
            }
        } private string file = null;

        // path of base folder
        public string Path {
            get {
                if (path == null)
                    updatePropertiesFromLocalMedia();

                return path;
            }
        } private string path = null;

        #endregion

        #endregion

        #region Constructors

        public MovieSignature() {

        }

        public MovieSignature(List<DBLocalMedia> localMedia) {
            LocalMedia = localMedia;
        }

        public MovieSignature(DBMovieInfo movie) {
            Title = movie.Title;
            Year = movie.Year;
            ImdbId = movie.ImdbID;
            LocalMedia = movie.LocalMedia;
        }

        public MovieSignature(string title) {
            Title = title;
        }

        #endregion

        #region Public methods

        // todo: this logic was moved from the importer but this is not the final form
        public int MatchScore(DBMovieInfo movie) {

            // Create Signature From Movie
            MovieSignature movieSignature = new MovieSignature(movie);

            // Get the default score for this movie
            int bestScore = MatchScore(movieSignature);

            // If we have alternative titles iterate through these titles and check
            // for a better score
            foreach (string altTitle in movie.AlternateTitles.ToArray()) {
                movieSignature.Title = altTitle;
                int score = MatchScore(movieSignature);
                // if this match is better than the previous one save the score
                if (score < bestScore)
                    bestScore = score;
                // if the best score is 0 (the best possible score) then stop score checking
                if (bestScore == 0) break;
            }

            // return the best score for this movie
            return bestScore;
        }

        // todo: this logic was moved from the importer but this is not the final form
        public int MatchScore(MovieSignature signature) {

            bool imdbBoost = (bool)MovingPicturesCore.SettingsManager["importer_autoimdb"].Value;
            bool hasImdb = (imdb_id != null);
            bool hasYear = (Year > 0);
            string strYear = (hasYear) ? Year.ToString() : null;

            string compareThis = Utility.normalizeTitle(title); // normalize title
            string compareOther = Utility.normalizeTitle(signature.Title); // normalize title

            // Account for Year when criteria is met
            // this should give the match a higher priority
            if (hasYear && signature.Year > 0) {
                compareThis += ' ' + strYear;
                compareOther += ' ' + signature.Year.ToString();
            }

            // Account for IMDB when criteria is met
            if (hasImdb && signature.ImdbId != null) {
                if (imdbBoost && (ImdbId == signature.ImdbId)) {
                    // If IMDB Auto-Approval is active
                    // and the we have an ImdbId match,
                    // cheat the current match system into
                    // an auto-match
                    compareThis = ImdbId;
                    compareOther = signature.ImdbId;
                }
                else {
                    // add the imdb id tot the complete matching string
                    // this should improve priority
                    compareThis += ' ' + ImdbId;
                    compareOther += ' ' + signature.ImdbId;
                }
            }

            // get the Levenshtein distance between the two string and use them 
            // for as score for now
            int score = AdvancedStringComparer.Levenshtein(compareThis, compareOther);

            // uncomment this line to log matching process
            logger.Debug("Compare: '{0}', With: '{1}', Result: {2}", compareThis, compareOther, score);
            return score;
        }


        #endregion

        #region Private methods

        /// <summary>
        /// Updates the File, Folder and Path property using the LocalMedia data
        /// </summary>
        private void updatePropertiesFromLocalMedia() {
            if (LocalMedia != null) {
                DirectoryInfo baseFolder = Utility.GetMovieBaseDirectory(LocalMedia[0].File.Directory);
                folder = baseFolder.Name;
                file = LocalMedia[0].File.Name;
                path = baseFolder.FullName;
            }
        }

        #endregion

        #region Overrides

        public override string ToString() {
            return String.Format("Path= \"{0}\", Folder= \"{1}\", File= \"{2}\", Title= \"{3}\", Year= {4}, DiscId= \"{5}\", MovieHash= \"{6}\", ImdbId= \"{7}\"",
            this.Path, this.Folder, this.File, this.Title, this.Year.ToString(), this.DiscId, this.MovieHash, this.ImdbId);
        }

        #endregion

    }
}
