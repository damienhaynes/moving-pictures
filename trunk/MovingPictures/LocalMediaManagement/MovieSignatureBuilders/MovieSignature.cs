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
        private string baseTitle = null;

        #endregion

        #region Public properties

        /// <summary>
        /// The full movie title
        /// </summary>
        public string Title { // ex. "Pirates of Silicon Valley"
            get { return title; }
            set {
                if (value != null)
                    title = value.Trim();

                if (!String.IsNullOrEmpty(title)) {
                    keywords = Utility.TitleToKeywords(title);
                    baseTitle = Utility.normalizeTitle(title);
                } else {
                    title = null;
                    keywords = null;
                    baseTitle = null;
                } 

            }
        } 

        private string title = null;        

        /// <summary>
        /// Keywords derived from the full movie title, can be used
        /// by a data provider for better results.
        /// </summary>
        public string Keywords {
            get { return keywords; }
        } private string keywords = null;

        /// <summary>
        /// Year / Release Date 
        /// </summary>
        /// <example>
        /// 1999
        /// </example>
        public int? Year = null;

        /// <summary>
        /// The IMDB Id including the "tt" prefix
        /// </summary>
        /// <example>
        /// "tt0168122"
        /// </example>
        public string ImdbId {
            get { return imdb_id; }
            set {
                if (value != null)
                    imdb_id = value.Trim();
                if (imdb_id == string.Empty)
                    imdb_id = null;
            }
        } private string imdb_id = null;

        /// <summary>
        /// String version of the Disc ID (16 character hash of a DVD)
        /// </summary>
        public string DiscId {
            get {
                if (LocalMedia != null)
                    discid = LocalMedia[0].DiscId;
                return discid;
            }
            set { discid = value; }
        } private string discid = null;

        /// <summary>
        /// String version of the filehash of the first movie file (16 characters)
        /// </summary>
        public string MovieHash {
            get {
                if (LocalMedia != null)
                    filehash = LocalMedia[0].FileHash;
                return filehash;
            }
            set { filehash = value; }
        } private string filehash = null;
        
        public List<DBLocalMedia> LocalMedia = null; // LocalMedia collection 

        #region Read-only

        /// <summary>
        /// The base foldername of the movie
        /// </summary>
        public string Folder {
            get {
                if (folder == null)
                    updatePropertiesFromLocalMedia();

                return folder;
            }
        } private string folder = null;

        /// <summary>
        /// The filename of the movie
        /// </summary>
        public string File {
            get {
                if (file == null)
                    updatePropertiesFromLocalMedia();

                return file;
            }
        } private string file = null;

        /// <summary>
        /// Complete path to the base folder
        /// </summary>
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

        public MatchResult GetMatchResult(DBMovieInfo movie) {
            
            // Create a new score card
            MatchResult result = new MatchResult();

            // Get the default scores for this movie
            result.TitleScore = matchTitle(movie.Title);
            result.YearScore = matchYear(movie.Year);
            result.ImdbMatch = matchImdb(movie.ImdbID);

            // If we don't have a perfect score on the original title
            // iterate through the available alternate titles and check
            // them to lower the score if possible
            if (result.TitleScore > 0) {
                foreach (string alternateTitle in movie.AlternateTitles.ToArray()) {
                    int score = matchTitle(alternateTitle);
                    // if this match is better than the previous one save the score
                    if (score < result.TitleScore) {
                        result.TitleScore = score;
                        result.AlternateTitle = alternateTitle;
                    }
                    // if the best score is 0 (the best possible score) then stop score checking
                    if (result.TitleScore == 0) break;
                }
            }

            // return the result
            logger.Debug("Final MatchResult for '{0}': {1}", movie.Title, result.ToString());
            return result;
        }

        private int matchTitle(string title) {
            string otherTitle = Utility.normalizeTitle(title);
            int score = AdvancedStringComparer.Levenshtein(baseTitle, otherTitle);
            logger.Debug("Compare: '{0}', With: '{1}', Result: {2}", baseTitle, otherTitle, score);
            return score;
        }

        private int matchYear(int year) {
            if (Year > 0 && year > 0) {
                int score = ((int)Year - year);
                score = (score < 0) ? score * -1 : score;
                return score;
            } else {
                return 0;
            }
        }

        private bool matchImdb(string imdbid) {
            if (imdb_id == null || imdbid == null)
                return false;
          
            return (imdb_id == imdbid);
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
            return String.Format("Path= \"{0}\", Folder= \"{1}\", File= \"{2}\", Keywords= \"{3}\", Title= \"{4}\", Year= {5}, DiscId= \"{6}\", MovieHash= \"{7}\", ImdbId= \"{8}\"",
            this.Path, this.Folder, this.File, this.Keywords, this.Title, this.Year.ToString(), this.DiscId, this.MovieHash, this.ImdbId);
        }

        #endregion

    }

    /// <summary>
    /// This struct represents a score card that is the result of
    /// comparing a signature with movie information. The value can 
    /// be used to rank a list of possible matches and to determine 
    /// if they can be auto-approved.
    /// </summary>
    public struct MatchResult {
        
        #region Public Properties

        public int TitleScore;
        public int YearScore;
        public bool ImdbMatch;
        public string AlternateTitle;

        #endregion

        #region Public Methods

        /// <summary>
        /// Get a value indicating if an alternate title was used for the title score
        /// </summary>
        /// <returns>True if an alternate title was used for the title score</returns>
        public bool AlternateTitleUsed() {
            return (!String.IsNullOrEmpty(AlternateTitle));
        }

        /// <summary>
        /// Get a value indicating wether this result can be auto-approved because
        /// it meets the minimal requirements
        /// </summary>
        /// <returns>True, if the result can be auto-approved</returns>
        public bool AutoApprove() {
            
            // IMDB Auto-Approval
            if (ImdbMatch && MovingPicturesCore.Settings.AutoApproveOnIMDBMatch)
                return true;

            // Alternate Title Auto-Approve Limitation
            if (!MovingPicturesCore.Settings.AutoApproveOnAlternateTitle && AlternateTitleUsed())
                return false;

            // Title + Year Auto-Approval
            if (TitleScore <= MovingPicturesCore.Settings.AutoApproveThreshold)
                if (YearScore <= MovingPicturesCore.Settings.AutoApproveYearDifference)
                    return true;   

            return false;
        }

        #endregion

        #region Overrides

        public override string ToString() {
            return String.Format("TitleScore={0}, YearScore={1}, ImdbMatch={2}, AlternateTitleUsed={3}, AlternateTitle='{4}', AutoApprove={5}",
            TitleScore, YearScore, ImdbMatch, AlternateTitleUsed().ToString(), AlternateTitle, AutoApprove().ToString());
        }

        #endregion
    }
}
