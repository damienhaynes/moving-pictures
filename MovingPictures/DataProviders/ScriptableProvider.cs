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

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    public class ScriptableProvider : IScriptableMovieProvider {
        #region Properties

        public string Name {
            get {
                return scraper.Name;
            }
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
            scraper = new ScriptableScraper(script);

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
            if (movieSignature.DvdId != null) paramList["search.dvd_id"] = movieSignature.DvdId;
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
                        currField.SetValue(newMovie, value);
                }

                count++;

                // check if the movie has already been added, if not, add it
                if (!addedMovies.ContainsKey(newMovie.ImdbID)) {
                    rtn.Add(newMovie);
                    addedMovies[newMovie.ImdbID] = newMovie;
                }
            }


            return rtn;
        }

        public void Update(DBMovieInfo movie) {
            if (scraper == null)
                return;

            Dictionary<string, string> paramList = new Dictionary<string, string>();
            Dictionary<string, string> results;

            // load params
            foreach (DBField currField in DBField.GetFieldList(typeof(DBMovieInfo))) 
                paramList["movie." + currField.FieldName] = currField.GetValue(movie).ToString().Trim();

            // try to retrieve results
            results = scraper.Execute("get_details", paramList);

            // get our new movie details
            DBMovieInfo newMovie = new DBMovieInfo();
            foreach (DBField currField in DBField.GetFieldList(typeof(DBMovieInfo))) {
                string value;
                bool success = results.TryGetValue("movie." + currField.FieldName, out value);

                if (success && value.Trim().Length > 0)
                    currField.SetValue(newMovie, value);
            }

            // and update as neccisary
            movie.CopyUpdatableValues(newMovie);
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

            // rn the scraper
            results = scraper.Execute("get_cover_art", paramList);

            int coversAdded = 0;
            int count = 0;
            while (results.ContainsKey("cover_art[" + count + "].url")) {
                // if we have hit our limit quit
                if (movie.AlternateCovers.Count >= maxCovers || coversAdded > maxCoversInSession)
                    return true;

                // get url for cover and load it via the movie object
                string coverPath = results["cover_art[" + count + "].url"];
                if (movie.AddCoverFromURL(coverPath) == ArtworkLoadStatus.SUCCESS)
                    coversAdded++;

                count++;
            }

            return false;
        }

        public bool GetBackdrop(DBMovieInfo movie) {
            if (scraper == null)
                return false;

            return false;
        }

        #endregion
    }
}
