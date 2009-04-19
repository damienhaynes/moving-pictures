using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.ScraperEngine;
using System.IO;
using MediaPortal.Plugins.MovingPictures.Database;
using Cornerstone.Database;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.Properties;
using MediaPortal.Plugins.MovingPictures.SignatureBuilders;
using System.Reflection;
using System.Globalization;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    public class ScriptableProvider : IScriptableMovieProvider {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Properties

        public string Name {
            get {
                return scraper.Name;
            }
        }

        public int ScriptID {
            get {
                return scraper.ID;
            }
        }


        public string Version {
            get { return scraper.Version; } 
        }

        public string Author {
            get { return scraper.Author; } 
        }

        public string Language {
            get {
                try {
                    return new CultureInfo(scraper.Language).DisplayName;
                }
                catch (ArgumentException) {
                    return "";
                }
            }
        }

        public DateTime? Published {
            get {
                return scraper.Published;
            }
        }

        public bool DebugMode { 
            get {
                return scraper.DebugMode;
            }
            set {
                scraper.DebugMode = value;
            }
        }

        public string Description {
            get { return scraper.Description; } 
        }

        public bool ProvidesMoviesDetails {
            get { return providesMovieDetails; }
        }
        private bool providesMovieDetails = false;

        public bool ProvidesCoverArt {
            get { return providesCoverArt; }
        }
        private bool providesCoverArt = false;

        public bool ProvidesBackdrops {
            get { return providesBackdrops; }
        }
        private bool providesBackdrops = false;

        public ScriptableScraper Scraper {
            get { return scraper; }
        }
        private ScriptableScraper scraper = null;

        #endregion

        #region Public Methods

        public ScriptableProvider() {
        }

        public bool Load(string script) {
            bool debugMode = MovingPicturesCore.Settings.DataSourceDebugActive;
            scraper = new ScriptableScraper(script, debugMode);

            if (!scraper.LoadSuccessful) {
                scraper = null;
                return false;
            }

            providesMovieDetails = scraper.ScriptType.Contains("MovieDetailsFetcher");
            providesCoverArt = scraper.ScriptType.Contains("MovieCoverFetcher");
            providesBackdrops = scraper.ScriptType.Contains("MovieBackdropFetcher");

            return true;
        }

        public List<DBMovieInfo> Get(MovieSignature movieSignature) {
            if (scraper == null)
                return null;

            List<DBMovieInfo> rtn = new List<DBMovieInfo>();

            Dictionary<string, DBMovieInfo> addedMovies = new Dictionary<string, DBMovieInfo>();

            Dictionary<string, string> paramList = new Dictionary<string, string>();
            Dictionary<string, string> results;

            if (movieSignature.Title != null) paramList["search.title"] = movieSignature.Title;
            if (movieSignature.Keywords != null) paramList["search.keywords"] = movieSignature.Keywords;
            if (movieSignature.Year != null) paramList["search.year"] = movieSignature.Year.ToString();
            if (movieSignature.ImdbId != null) paramList["search.imdb_id"] = movieSignature.ImdbId;
            if (movieSignature.DiscId != null) paramList["search.disc_id"] = movieSignature.DiscId;
            if (movieSignature.MovieHash != null) paramList["search.moviehash"] = movieSignature.MovieHash;
            if (movieSignature.Path != null) paramList["search.basepath"] = movieSignature.Path;
            if (movieSignature.Folder != null) paramList["search.foldername"] = movieSignature.Folder;
            if (movieSignature.File != null) paramList["search.filename"] = movieSignature.File;
            //if (!String.IsNullOrEmpty(movieSignature.File)) paramList["search.filenamewithoutextension"] = Path.GetFileNameWithoutExtension(movieSignature.File);

            results = scraper.Execute("search", paramList);
            if (results == null) {
                logger.Error(Name + " scraper script failed to execute \"search\" node.");
                return rtn;
            }

            int count = 0;
            while (results.ContainsKey("movie[" + count + "].title")) {
                string prefix = "movie[" + count + "].";
                DBMovieInfo newMovie = new DBMovieInfo();
                
                foreach (DBField currField in DBField.GetFieldList(typeof(DBMovieInfo))) {
                    string value;
                    bool success = results.TryGetValue(prefix + currField.FieldName, out value);

                    if (success)
                        currField.SetValue(newMovie, value.Trim());
                }

                // try to store the site id
                string siteId;
                bool success2 = results.TryGetValue(prefix + "site_id", out siteId);
                if (success2) newMovie.GetSourceMovieInfo(ScriptID).Identifier = siteId;

                count++;

                // check if the movie has already been added, if not, add it
                if (!addedMovies.ContainsKey(newMovie.DetailsURL)) {
                    rtn.Add(newMovie);
                    addedMovies[newMovie.DetailsURL] = newMovie;
                }
            }


            return rtn;
        }

        public UpdateResults Update(DBMovieInfo movie) {
            if (scraper == null)
                return UpdateResults.FAILED;

            Dictionary<string, string> paramList = new Dictionary<string, string>();
            Dictionary<string, string> results;

            // try to load the id for the movie for this script
            DBSourceMovieInfo idObj = movie.GetSourceMovieInfo(ScriptID);
            if (idObj != null && idObj.Identifier != null)
                paramList["movie.site_id"] = idObj.Identifier;
            else
                return UpdateResults.FAILED_NEED_ID;

            // load params
            foreach (DBField currField in DBField.GetFieldList(typeof(DBMovieInfo))) {
                if (currField.GetValue(movie) != null)
                    paramList["movie." + currField.FieldName] = currField.GetValue(movie).ToString().Trim();
            }

            // try to retrieve results
            results = scraper.Execute("get_details", paramList);
            if (results == null) {
                logger.Error(Name + " scraper script failed to execute \"get_details\" node.");
                return UpdateResults.FAILED;
            }


            // get our new movie details
            DBMovieInfo newMovie = new DBMovieInfo();
            foreach (DBField currField in DBField.GetFieldList(typeof(DBMovieInfo))) {
                string value;
                bool success = results.TryGetValue("movie." + currField.FieldName, out value);

                if (success && value.Trim().Length > 0)
                    currField.SetValue(newMovie, value.Trim());
            }

            // and update as neccisary
            movie.CopyUpdatableValues(newMovie);
            return UpdateResults.SUCCESS;
        }

        public bool GetArtwork(DBMovieInfo movie) {
            if (scraper == null)
                return false;

            Dictionary<string, string> paramList = new Dictionary<string, string>();
            Dictionary<string, string> results;

            // grab coverart loading settings
            int maxCovers = MovingPicturesCore.Settings.MaxCoversPerMovie;
            int maxCoversInSession = MovingPicturesCore.Settings.MaxCoversPerSession;

            // if we have already hit our limit for the number of covers to load, quit
            if (movie.AlternateCovers.Count >= maxCovers)
                return true;

            // try to load the id for the movie for this script
            DBSourceMovieInfo idObj = movie.GetSourceMovieInfo(ScriptID);
            if (idObj != null && idObj.Identifier != null)
                paramList["movie.site_id"] = idObj.Identifier;

            // load params for scraper
            foreach (DBField currField in DBField.GetFieldList(typeof(DBMovieInfo)))
                if (currField.GetValue(movie) != null)
                    paramList["movie." + currField.FieldName] = currField.GetValue(movie).ToString().Trim();

            // run the scraper
            results = scraper.Execute("get_cover_art", paramList);
            if (results == null) {
                logger.Error(Name + " scraper script failed to execute \"get_cover_art\" node.");
                return false;
            }

            int coversAdded = 0;
            int count = 0;
            while (results.ContainsKey("cover_art[" + count + "].url")) {
                // if we have hit our limit quit
                if (movie.AlternateCovers.Count >= maxCovers || coversAdded >= maxCoversInSession)
                    return true;

                // get url for cover and load it via the movie object
                string coverPath = results["cover_art[" + count + "].url"];
                if (coverPath.Trim() != string.Empty)
                    if (movie.AddCoverFromURL(coverPath) == ArtworkLoadStatus.SUCCESS)
                        coversAdded++;

                count++;
            }

            if (coversAdded > 0)
                return true;

            return false;
        }

        public bool GetBackdrop(DBMovieInfo movie) {
            if (scraper == null)
                return false;

            Dictionary<string, string> paramList = new Dictionary<string, string>();
            Dictionary<string, string> results;

            // if we already have a backdrop move on for now
            if (movie.BackdropFullPath.Trim().Length > 0)
                return true;

            // try to load the id for the movie for this script
            DBSourceMovieInfo idObj = movie.GetSourceMovieInfo(ScriptID);
            if (idObj != null && idObj.Identifier != null)
                paramList["movie.site_id"] = idObj.Identifier;

            // load params for scraper
            foreach (DBField currField in DBField.GetFieldList(typeof(DBMovieInfo)))
                if (currField.GetValue(movie) != null)
                    paramList["movie." + currField.FieldName] = currField.GetValue(movie).ToString().Trim();

            // run the scraper
            results = scraper.Execute("get_backdrop", paramList);
            if (results == null) {
                logger.Error(Name + " scraper script failed to execute \"get_backdrop\" node.");
                return false;
            }

            int count = 0;
            string backdropURL = string.Empty;

            // Loop through all the results until a valid backdrop is found
            // todo: support multiple backdrops
            while (results.ContainsKey("backdrop[" + count + "].url")) {
                backdropURL = results["backdrop[" + count + "].url"]; 
                if (backdropURL.Trim().Length > 0)
                    if (movie.AddBackdropFromURL(backdropURL) == ArtworkLoadStatus.SUCCESS)
                            return true;

                count++;
            }
            
            // no valid backdrop found
            return false;
        }

        public override bool Equals(object obj) {
            if (obj.GetType() != typeof(ScriptableProvider))
                return base.Equals(obj);

            return Version.Equals(((ScriptableProvider)obj).Version) &&
                   Scraper.ID == ((ScriptableProvider)obj).Scraper.ID;
        }

        public override int GetHashCode() {
            return (Version + Scraper.ID).GetHashCode();
        }


        #endregion
    }
}
