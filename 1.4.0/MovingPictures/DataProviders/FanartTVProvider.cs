using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using Cornerstone.Extensions;
using Cornerstone.Tools;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.SignatureBuilders;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using NLog;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieResources;
using MediaPortal.Plugins.MovingPictures.DataProviders.FanartTV;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    class FanartTVProvider : InternalProvider, IMovieProvider {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // NOTE: To other developers creating other applications, using this code as a base
        //       or as a reference. PLEASE get your own API key. Do not reuse the one listed here
        //       it is intended for Moving Pictures use ONLY. API keys are free and easy to apply
        //       for. Visit this url: http://fanart.tv/get-an-api-key/

        #region API variables

        private const string apiMovieBackdrops = "http://fanart.tv/webservice/movie/4f26c36ab3d97e3a4a0c1e081710e3a6/{0}/JSON/moviebackground/";

        #endregion

        #region IMovieProvider

        public string Name {
            get { return "fanart.tv"; }
        }

        public string Description {
            get { return "Returns backdrops from fanart.tv."; }
        }

        public string Language {
            get { return string.Empty; }
        }

        public string LanguageCode {
            get { return string.Empty; }
        }

        public bool ProvidesMoviesDetails {
            get { return false; }
        }

        public bool ProvidesCoverArt {
            get { return false; }
        }

        public bool ProvidesBackdrops {
            get { return true; }
        }

        public bool GetBackdrop(DBMovieInfo movie) {
            if (movie == null)
                return false;

            // if we already have a backdrop move on for now
            if (movie.BackdropFullPath.Trim().Length > 0)
                return true;

            // do we have an id?
            string movieId = getMovieId(movie);
            if (string.IsNullOrEmpty(movieId))
                return false;

            string response = getJson(string.Format(apiMovieBackdrops, movieId));
            if (response == null)
                return false;

            // de-serialize json response
            var movies = response.FromJsonDictionary<Dictionary<string, MovieImages>>();
            if (movies == null || movies.Count == 0)
                return false;

            // we are only getting back 1 movie in dictionary so lets get it
            var movieImages = movies.First().Value;

            if (movieImages.moviebackground == null || movieImages.moviebackground.Count == 0)
                return false;

            // get the 1st (highest rated) movie backdrop
            // only single backdrops are currently supported
            string backdropUrl = movieImages.moviebackground.First().url;
            if (backdropUrl.Trim().Length > 0) {
                if (movie.AddBackdropFromURL(backdropUrl) == ImageLoadResults.SUCCESS) {
                    // store either the imdb id or tmdb id of movie for identifier
                    // there is no per movie id from fanart.tv, only per image id
                    movie.GetSourceMovieInfo(SourceInfo).Identifier = movieId;
                    return true;
                }
            }

            // if we get here we didn't manage to find a proper backdrop
            return false;
        }

        public List<DBMovieInfo> Get(MovieSignature movieSignature) {
            return new List<DBMovieInfo>();
        }

        public UpdateResults Update(DBMovieInfo movie) {
            return UpdateResults.FAILED;
        }

        public bool GetArtwork(DBMovieInfo movie) {
            return false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// returns the imdbid or tmdbid of the movie
        /// </summary>
        private string getMovieId(DBMovieInfo movie) {
            if (movie == null)
                return null;

            // check for imdb id
            if (movie.ImdbID != null && movie.ImdbID.Trim().Length == 9)
                return movie.ImdbID.Trim();

            // check for tmdb id
            string tmdbId = getTmdbId(movie);
            if (!string.IsNullOrEmpty(tmdbId))
                return tmdbId;

            // fanart.tv lookup can only be done on imdb/tmdb ids
            return null;
        }

        /// <summary>
        /// returns the tmdb id for a movie if its exists
        /// </summary>
        private string getTmdbId(DBMovieInfo movie) {
            // get source info, maybe already have it from poster or movieinfo
            var tmdbSource = DBSourceInfo.GetAll().Find(s => s.ToString() == "themoviedb.org");
            if (tmdbSource == null)
                return null;

            string id = movie.GetSourceMovieInfo(tmdbSource).Identifier;
            if (id == null || id.Trim() == string.Empty)
                return null;

            return id;
        }

        private static string getJson(string url) {
            
            WebGrabber grabber = Utility.GetWebGrabberInstance(url);
            grabber.Encoding = Encoding.UTF8;

            if (grabber.GetResponse())
                return grabber.GetString();
            else
                return null;
        }

        #endregion
    }
}