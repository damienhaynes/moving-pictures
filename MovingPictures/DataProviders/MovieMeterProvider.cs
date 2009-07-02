using System;
using System.Collections.Generic;
using System.Globalization;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.DataProviders.MovieMeter;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Plugins.MovingPictures.SignatureBuilders;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    class MovieMeterProvider : InternalProvider, IMovieProvider {

        #region Private variables

        private static Logger logger = LogManager.GetCurrentClassLogger();

        // NOTE: To other developers creating other applications, using this code as a base
        // or as a reference. PLEASE get your own API key. Do not reuse the one listed here
        // it is intended for Moving Pictures use ONLY. API keys are free and easy to apply
        // for. Visit this url: http://wiki.moviemeter.nl/index.php/API
        private const string apiKey = "fps39a61pccchgw33bgkb10mdmj4vnzy";

        #endregion

        #region IMovieProvider Members

        public string Name {
            get {
                return "moviemeter.nl";
            }
        }

        public override string Author {
            get { return "Armand Pondman (armandp)"; }
        }

        public string Description {
            get { return "Returns details and covers from moviemeter.nl (API)."; }
        }

        public string Language {
            get { return new CultureInfo("nl").DisplayName; }
        }

        public bool ProvidesMoviesDetails {
            get { return true; }
        }

        public bool ProvidesCoverArt {
            get { return true; }
        }

        public bool ProvidesBackdrops {
            get { return false; }
        }

        public MovieMeterAPI Api {
            get {
                if (_mmApi == null)
                    _mmApi = new MovieMeterAPI(apiKey);

                return _mmApi;
            }
        }  private MovieMeterAPI _mmApi;

        public List<DBMovieInfo> Get(MovieSignature movieSignature) {
            List<DBMovieInfo> results = new List<DBMovieInfo>();
            if (movieSignature == null)
                return results;

            DBMovieInfo movie = null;
            if (movieSignature.ImdbId != null)
                movie = getMovieByImdb(movieSignature.ImdbId);

            if (movie != null)
                results.Add(movie);
            else
                results = getMoviesByTitle(movieSignature.Title);

            return results;
        }

        public UpdateResults Update(DBMovieInfo movie) {
            if (movie == null)
                return UpdateResults.FAILED;

            string mmId = getMovieMeterID(movie);
            if (mmId == null) {
                // Need id
                return UpdateResults.FAILED_NEED_ID;
            }
            else {
                // Update the movie
                DBMovieInfo newMovie = getMovieDetails(mmId);
                if (newMovie != null) {
                    movie.CopyUpdatableValues(newMovie);
                    return UpdateResults.SUCCESS;
                }
            }

            return UpdateResults.FAILED;
        }

        public bool GetArtwork(DBMovieInfo movie) {
            if (movie == null)
                return false;

            string mmId = getMovieMeterID(movie);
            if (mmId != null) {
                FilmDetail film = Api.GetMovieDetails(mmId);
                if (film != null) {
                    if (film.thumbnail != null) {
                        string coverPath = film.thumbnail.Replace(@"/thumbs/", @"/");
                        if (movie.AddCoverFromURL(coverPath) == ArtworkLoadStatus.SUCCESS)
                            return true;
                    }
                }
            }
            return false;
        }

        public bool GetBackdrop(DBMovieInfo movie) {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods

        private string getMovieMeterID(DBMovieInfo movie) {
            string mmId = null;
            DBSourceMovieInfo idObj = movie.GetSourceMovieInfo(SourceInfo);
            if (idObj != null && idObj.Identifier != null) {
                mmId = idObj.Identifier;
            }
            else {
                // Translate IMDB code to MovieMeter ID
                string imdbId = movie.ImdbID.Trim();
                if (imdbId != string.Empty) {
                    mmId = Api.GetMovieMeterId(imdbId);
                    if (mmId != null)
                        movie.GetSourceMovieInfo(SourceInfo).Identifier = mmId;
                }
            }
            return mmId;
        }

        private List<DBMovieInfo> getMoviesByTitle(string title) {
            List<DBMovieInfo> results = new List<DBMovieInfo>();
            Film[] films = Api.Search(title);
            if (films != null) {
                foreach (Film film in films) {
                    DBMovieInfo movie = new DBMovieInfo();
                    movie.Title = Utility.TitleToDisplayName(film.title);
                    movie.AlternateTitles.Add(film.alternative_title);
                    movie.GetSourceMovieInfo(SourceInfo).Identifier = film.filmId;
                    int year = 0;
                    if (int.TryParse(film.year, out year))
                        movie.Year = year;

                    // Add movie to the results
                    results.Add(movie);
                }
            }

            return results;
        }

        private DBMovieInfo getMovieByImdb(string imdbid) {
            if (imdbid == null)
                return null;

            string mmid = Api.GetMovieMeterId(imdbid);
            if (mmid != null)
                return getMovieDetails(mmid);
            else
                return null;
        }

        private DBMovieInfo getMovieDetails(string mmid) {
            FilmDetail film = Api.GetMovieDetails(mmid);
            if (film != null) {

                DBMovieInfo movie = new DBMovieInfo();
                movie.GetSourceMovieInfo(SourceInfo).Identifier = film.filmId.ToString();

                movie.Title = Utility.TitleToDisplayName(film.title);
                movie.Summary = film.plot;
                movie.ImdbID = "tt" + film.imdb;
                movie.DetailsURL = film.url;

                // Score (multiply by 2 to get the 1-10 scale)
                if (!String.IsNullOrEmpty(film.average))
                    movie.Score = (Convert.ToSingle(film.average, NumberFormatInfo.InvariantInfo) * 2);

                // Runtime
                int runtime = 0;
                if (int.TryParse(film.duration, out runtime))
                    movie.Runtime = runtime;

                // Year
                int year = 0;
                if (int.TryParse(film.year, out year))
                    movie.Year = year;

                // Try to add the alternative titles
                if (film.alternative_titles != null)
                    foreach (FilmDetail.Title alt in film.alternative_titles)
                        movie.AlternateTitles.Add(alt.title);

                // Try to add the genres
                if (film.genres != null)
                    foreach (string genre in film.genres)
                        movie.Genres.Add(genre);

                // Try to add the actors
                if (film.actors != null)
                    foreach (FilmDetail.Actor actor in film.actors)
                        movie.Actors.Add(actor.name);

                // Try to add the directors
                if (film.directors != null)
                    foreach (FilmDetail.Director director in film.directors)
                        movie.Directors.Add(director.name);

                return movie;
            }
            return null;
        }

        #endregion

    }
}
