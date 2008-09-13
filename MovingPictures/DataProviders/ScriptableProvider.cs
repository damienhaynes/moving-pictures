using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.ScraperEngine;
using System.IO;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;
using Cornerstone.Database;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    public class ScriptableProvider : IMovieProvider, ICoverArtProvider, IBackdropProvider {
        private static Dictionary<string, ScriptableProvider> existingScrapers;

        private ScriptableScraper scraper;

        #region Properties

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

        #endregion

        #region Private Constructors
        private ScriptableProvider(FileInfo scriptFile) {
            throw new NotImplementedException();
        }

        private ScriptableProvider(string script) {
            scraper = new ScriptableScraper(script);

            if (!scraper.LoadSuccessful)
                return;

            if (scraper.ScriptType.Contains("MovingPicturesMovieData"))
                providesMovieDetails = true;

            if (scraper.ScriptType.Contains("MovingPicturesCoverArt"))
                providesCoverArt = true;

            if (scraper.ScriptType.Contains("MovingPicturesBackdrops"))
                providesBackdrops = true;

        }
        #endregion

        #region Static Methods
        public static ScriptableProvider Load(string script) {
            if (existingScrapers == null)
                existingScrapers = new Dictionary<string,ScriptableProvider>();

            if (existingScrapers.ContainsKey(script))
                return existingScrapers[script];

            ScriptableProvider newProvider = new ScriptableProvider(script);
            if (newProvider.scraper.LoadSuccessful == false)
                return null;

            return newProvider;
        }

        public static ScriptableProvider Load(FileInfo scriptFile) {
            ScriptableProvider newProvider = new ScriptableProvider(scriptFile);
            if (newProvider.scraper.LoadSuccessful == false)
                return null;

            return newProvider;
        }
        #endregion

        #region Public Methods
        public List<DBMovieInfo> Get(string movieTitle) {
            List<DBMovieInfo> rtn = new List<DBMovieInfo>();

            Dictionary<string, string> paramList = new Dictionary<string, string>();
            Dictionary<string, string> results;

            paramList["search_string"] = movieTitle;
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

                rtn.Add(newMovie);
                count++;
            }


            return rtn;
        }

        public void Update(DBMovieInfo movie) {
            Dictionary<string, string> paramList = new Dictionary<string, string>();
            Dictionary<string, string> results;

            // load params
            foreach (DBField currField in DBField.GetFieldList(typeof(DBMovieInfo))) 
                paramList["movie." + currField.FieldName] = currField.GetValue(movie).ToString();

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
            return false;
        }

        public bool GetBackdrop(DBMovieInfo movie) {
            return false;
        }
        #endregion
    }
}
