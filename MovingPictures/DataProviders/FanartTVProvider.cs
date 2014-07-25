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

        private const string apiMovieArtwork = "http://webservice.fanart.tv/v3/movies/{0}?api_key=4f26c36ab3d97e3a4a0c1e081710e3a6";

        #endregion

        #region IMovieProvider

        public string Name {
            get { return "fanart.tv"; }
        }

        public string Description {
            get { return "Returns localised covers and backdrops from fanart.tv."; }
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
                return true;

            // do we have an id?
            string movieId = getMovieId(movie);
            if (string.IsNullOrEmpty(movieId))
                return false;

            string response = getJson(string.Format(apiMovieArtwork, movieId));
            if (response == null)
                return false;

            // de-serialize json response
            var movieImages = response.FromJson<Artwork>();
            if (movieImages == null)
                return false;

            // check if we have any backdrops
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
            if (movie == null)
                return false;

            // do we have an id?
            string movieId = getMovieId(movie);
            if (string.IsNullOrEmpty(movieId))
                return false;

            // try to get movie artwork
            string response = getJson(string.Format(apiMovieArtwork, movieId));
            if (response == null)
                return false;

            // de-serialize json response
            var movieImages = response.FromJson<Artwork>();
            if (movieImages == null)
                return false;

            // check if we have any posters
            if (movieImages.movieposter == null || movieImages.movieposter.Count == 0)
                return false;

            // filter posters by language
            var langPosters = movieImages.movieposter.Where(p => p.lang == MovingPicturesCore.Settings.DataProviderLanguageCode);

            // if no localised posters available use all posters
            if (langPosters.Count() == 0) {
                langPosters = movieImages.movieposter;
            }

            // sort by highest rated / most popular
            langPosters = langPosters.OrderByDescending(p => p.likes);

            // grab coverart loading settings
            int maxCovers = MovingPicturesCore.Settings.MaxCoversPerMovie;
            int maxCoversInSession = MovingPicturesCore.Settings.MaxCoversPerSession;
            int coversAdded = 0;

            // download posters
            foreach (var poster in langPosters) {
                // if we have hit our limit quit
                if (movie.AlternateCovers.Count >= maxCovers || coversAdded >= maxCoversInSession)
                    return true;

                // get url for cover and load it via the movie object
                string coverPath = poster.url;
                if (coverPath.Trim() != string.Empty) {
                    if (movie.AddCoverFromURL(coverPath) == ImageLoadResults.SUCCESS)
                        coversAdded++;
                }
            }

            if (coversAdded > 0) {
                // Update source info
                movie.GetSourceMovieInfo(SourceInfo).Identifier = movieId;
                return true;
            }

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