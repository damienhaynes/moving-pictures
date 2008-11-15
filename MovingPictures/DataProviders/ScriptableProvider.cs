using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.ScraperEngine;
using System.IO;
using MediaPortal.Plugins.MovingPictures.Database;
using Cornerstone.Database;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.Properties;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using System.Reflection;
using System.Globalization;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    public class ScriptableProvider : IScriptableMovieProvider {
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
            bool debugMode = (bool)MovingPicturesCore.SettingsManager["source_manager_debug"].Value;
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
            if (movieSignature.Year != null) paramList["search.year"] = movieSignature.Year.ToString();
            if (movieSignature.ImdbId != null) paramList["search.imdb_id"] = movieSignature.ImdbId;
            if (movieSignature.Edition != null) paramList["search.edition"] = movieSignature.Edition;
            if (movieSignature.DiscId != null) paramList["search.disc_id"] = movieSignature.DiscId;
            if (movieSignature.Source != null) paramList["search.source"] = movieSignature.Source;

            results = scraper.Execute("search", paramList);

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

            // load params
            foreach (DBField currField in DBField.GetFieldList(typeof(DBMovieInfo))) 
                paramList["movie." + currField.FieldName] = currField.GetValue(movie).ToString().Trim();

            // try to load the id for the movie for this script
            DBSourceMovieInfo idObj = movie.GetSourceMovieInfo(ScriptID);
            if (idObj != null && idObj.Identifier != null)
                paramList["movie.site_id"] = idObj.Identifier;

            // try to retrieve results
            results = scraper.Execute("get_details", paramList);

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
            int maxCovers = (int)MovingPicturesCore.SettingsManager["max_covers_per_movie"].Value;
            int maxCoversInSession = (int)MovingPicturesCore.SettingsManager["max_covers_per_session"].Value;

            // if we have already hit our limit for the number of covers to load, quit
            if (movie.AlternateCovers.Count >= maxCovers)
                return true;

            // load params for scraper
            foreach (DBField currField in DBField.GetFieldList(typeof(DBMovieInfo)))
                paramList["movie." + currField.FieldName] = currField.GetValue(movie).ToString().Trim();

            // run the scraper
            results = scraper.Execute("get_cover_art", paramList);

            int coversAdded = 0;
            int count = 0;
            while (results.ContainsKey("cover_art[" + count + "].url")) {
                // if we have hit our limit quit
                if (movie.AlternateCovers.Count >= maxCovers || coversAdded >= maxCoversInSession)
                    return true;

                // get url for cover and load it via the movie object
                string coverPath = results["cover_art[" + count + "].url"];
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
