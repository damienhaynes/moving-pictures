using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using Cornerstone.Tools;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.SignatureBuilders;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using NLog;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieResources;
using MediaPortal.Plugins.MovingPictures.DataProviders.TMDbAPI;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    class TheMovieDbProvider: InternalProvider, IMovieProvider {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        #region IMovieProvider

        public string Name {
            get {
                return "themoviedb.org";
            }
        }

        public string Description {
            get { 
                return "Returns localised details, covers and backdrops from themoviedb.org."; 
            }
        }

        public string Language {
            get { return string.Empty; }
        }

        public string LanguageCode {
            get { return string.Empty; }
        }

        public bool ProvidesMoviesDetails {
            get { return true; }
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
            string tmdbID = getTheMovieDbId(movie, true);
            if (tmdbID == null)
                return false;

            // try to get movie artwork
            var movieArtwork = TheMovieDbAPI.GetMovieImages(tmdbID);
            if (movieArtwork == null || movieArtwork.Backdrops == null || movieArtwork.Backdrops.Count == 0)
                return false;

            // filter out minimum size images
            int minWidth = MovingPicturesCore.Settings.MinimumBackdropWidth;
            int minHeight = MovingPicturesCore.Settings.MinimumBackdropHeight;

            movieArtwork.Backdrops.RemoveAll(b => b.Width < minWidth || b.Height < minHeight);
            if (movieArtwork.Backdrops.Count == 0)
                return false;

            // sort by highest rated / most popular
            var backdrops = movieArtwork.Backdrops.OrderByDescending(p => p.Score);

            // get the base url for images
            string baseImageUrl = getImageBaseUrl();
            if (string.IsNullOrEmpty(baseImageUrl))
                return false;

            // moving pics currently only supports 1 backdrop per movie
            string backdropURL = baseImageUrl + backdrops.First().FilePath;
            if (backdropURL.Trim().Length > 0) {
                if (movie.AddBackdropFromURL(backdropURL) == ImageLoadResults.SUCCESS) {
                    movie.GetSourceMovieInfo(SourceInfo).Identifier = tmdbID;
                    return true;
                }
            }

            // if we get here we didn't manage to find a proper backdrop
            // so return false
            return false;
        }

        public bool GetArtwork(DBMovieInfo movie) {
            if (movie == null)
                return false;

            // do we have an id?
            string tmdbID = getTheMovieDbId(movie, true);
            if (tmdbID == null)
                return false;

            // try to get movie artwork
            var movieArtwork = TheMovieDbAPI.GetMovieImages(tmdbID);
            if (movieArtwork == null || movieArtwork.Posters == null || movieArtwork.Posters.Count == 0)
                return false;

            // filter out minimum size images
            int minWidth = MovingPicturesCore.Settings.MinimumCoverWidth;
            int minHeight = MovingPicturesCore.Settings.MinimumCoverHeight;

            movieArtwork.Posters.RemoveAll(p => p.Width < minWidth || p.Height < minHeight);
            if (movieArtwork.Posters.Count == 0) 
                return false;

            // filter posters by language
            var langPosters = movieArtwork.Posters.Where(p => p.LanguageCode == MovingPicturesCore.Settings.DataProviderLanguageCode);
            if (MovingPicturesCore.Settings.DataProviderLanguageCode == "en") {
                // include no language posters with english (sometimes language is not assigned)
                langPosters = langPosters.Union(movieArtwork.Posters.Where(p => p.LanguageCode == null));
            }
            // if no localised posters available use all posters
            if (langPosters.Count() == 0) {
                langPosters = movieArtwork.Posters;
            }

            // sort by highest rated / most popular
            langPosters = langPosters.OrderByDescending(p => p.Score);

            // grab coverart loading settings
            int maxCovers = MovingPicturesCore.Settings.MaxCoversPerMovie;
            int maxCoversInSession = MovingPicturesCore.Settings.MaxCoversPerSession;
            int coversAdded = 0;

            // get the base url for images
            string baseImageUrl = getImageBaseUrl();
            if (string.IsNullOrEmpty(baseImageUrl))
                return false;

            // download posters
            foreach (var poster in langPosters) {
                // if we have hit our limit quit
                if (movie.AlternateCovers.Count >= maxCovers || coversAdded >= maxCoversInSession)
                    return true;

                // get url for cover and load it via the movie object
                string coverPath = baseImageUrl + poster.FilePath;
                if (coverPath.Trim() != string.Empty) {
                    if (movie.AddCoverFromURL(coverPath) == ImageLoadResults.SUCCESS)
                        coversAdded++;
                }
            }

            if (coversAdded > 0) {
                // Update source info
                movie.GetSourceMovieInfo(SourceInfo).Identifier = tmdbID;
                return true;
            }

            return false;
        }

        public List<DBMovieInfo> Get(MovieSignature movieSignature) {
           List<DBMovieInfo> results = new List<DBMovieInfo>();
           if (movieSignature == null)
               return results;

           if (movieSignature.ImdbId != null && movieSignature.ImdbId.Trim().Length == 9) {
               DBMovieInfo movie = getMovieInformation(movieSignature.ImdbId.Trim());
               if (movie != null) {
                   results.Add(movie);
                   return results;
               }
           }
           
           // grab results, if year based search comes up dry, search without a year
           results = Search(movieSignature.Title, movieSignature.Year);
           if (results.Count == 0) results = Search(movieSignature.Title);

           return results;
        }

        public UpdateResults Update(DBMovieInfo movie) {
            if (movie == null)
                return UpdateResults.FAILED;

            string tmdbId = getTheMovieDbId(movie, false);
            // check if TMDb id is still null, if so request id.
            if (string.IsNullOrEmpty(tmdbId))
                return UpdateResults.FAILED_NEED_ID;

            // Grab the movie using the TMDb ID
            DBMovieInfo newMovie = getMovieInformation(tmdbId);
            if (newMovie != null) {
                movie.GetSourceMovieInfo(SourceInfo).Identifier = tmdbId;
                movie.CopyUpdatableValues(newMovie);
                return UpdateResults.SUCCESS;
            }
            else {
                return UpdateResults.FAILED;
            }
        }

        #endregion

        #region Private Methods

        private List<DBMovieInfo> Search(string title, int? year = null) {
            List<DBMovieInfo> results = new List<DBMovieInfo>();

            string releaseYear = year == null ? string.Empty : year.ToString();

            var searchResults = TheMovieDbAPI.SearchMovies(title, language: MovingPicturesCore.Settings.DataProviderLanguageCode, year: releaseYear);
            if (searchResults == null || searchResults.TotalResults == 0)
                return results;
            
            foreach (var result in searchResults.Results) {
                var movie = getMovieInformation(result.Id.ToString());
                if (movie != null)
                    results.Add(movie);
            }
            return results;
        }

        private DBMovieInfo getMovieInformation(string movieId) {
            if (string.IsNullOrEmpty(movieId))
                return null;

            // request the movie details by imdb id or tmdb id
            var movieDetails = TheMovieDbAPI.GetMovieInfo(movieId, MovingPicturesCore.Settings.DataProviderLanguageCode);
            if (movieDetails == null)
                return null;

            var movie = new DBMovieInfo();

            // get the tmdb id 
            movie.GetSourceMovieInfo(SourceInfo).Identifier = movieDetails.Id.ToString();

            // get the localised title if available otherwise fallback to original title
            movie.Title = string.IsNullOrEmpty(movieDetails.Title) ? movieDetails.OriginalTitle : movieDetails.Title;

            // get alternative titles
            var altTitles = TheMovieDbAPI.GetAlternativeTitles(movieDetails.Id.ToString());
            if (altTitles != null && altTitles.Titles != null) {
                movie.AlternateTitles.AddRange(altTitles.Titles.Select(a => a.Title));
            }

            // get languages
            if (movieDetails.SpokenLanguages != null) {
                movie.Language = string.Join("|", movieDetails.SpokenLanguages.Select(l => l.Name).ToArray());
            }

            // get tagline
            movie.Tagline = movieDetails.Tagline;

            // get imdb id
            movie.ImdbID = movieDetails.ImdbId;

            // get homepage
            movie.DetailsURL = movieDetails.HomePage;

            // get movie overview
            movie.Summary = movieDetails.Overview;

            // get movie score
            movie.Score = movieDetails.Score;

            // get movie vote count
            movie.Popularity = movieDetails.Votes;

            // get runtime (mins)
            movie.Runtime = movieDetails.Runtime;

            // get movie cast
            var castInfo = TheMovieDbAPI.GetCastInfo(movieDetails.Id.ToString());
            if (castInfo != null) {
                // add actors, sort by order field
                if (castInfo.Cast != null)
                    movie.Actors.AddRange(castInfo.Cast.OrderBy(a => a.Order).Select(a => a.Name));

                // add directors
                if (castInfo.Crew != null) {
                    movie.Directors.AddRange(castInfo.Crew.Where(c => c.Department == "Directing").Select(c => c.Name).Distinct());
                }

                // add writers
                if (castInfo.Crew != null) {
                    movie.Writers.AddRange(castInfo.Crew.Where(c => c.Department == "Writing").Select(c => c.Name).Distinct());
                }
            }

            // add genres
            if (movieDetails.Genres != null) {
                movie.Genres.AddRange(movieDetails.Genres.Select(g => g.Name).ToArray());
            }

            // add production companies (studios)
            if (movieDetails.ProductionCompanies != null)  {
                movie.Studios.AddRange(movieDetails.ProductionCompanies.Select(p => p.Name));
            }

            // add certification (US MPAA rating)
            var movieCertifications = TheMovieDbAPI.GetReleaseInfo(movieDetails.Id.ToString());
            if (movieCertifications != null && movieCertifications.Countries != null) {
                // this could also be a scraper setting to get certification and release date from preferred country
                var releaseInfo = movieCertifications.Countries.Find(c => c.CountryCode == "US");
                if (releaseInfo != null) {
                    movie.Certification = releaseInfo.Certification;
                }
            }

            // get release year
            DateTime date;
            if (DateTime.TryParse(movieDetails.ReleaseDate, out date))
                movie.Year = date.Year;

            return movie;
        }

        private string getTheMovieDbId(DBMovieInfo movie, bool fuzzyMatch) {
            // check for internally stored TMDb ID
            DBSourceMovieInfo idObj = movie.GetSourceMovieInfo(SourceInfo);
            if (idObj != null && idObj.Identifier != null) {
                return idObj.Identifier;
            }

            // if available, lookup based on IMDb ID
            else if (movie.ImdbID != null && movie.ImdbID.Trim().Length == 9) {
                string imdbId = movie.ImdbID.Trim();
                var movieInfo = TheMovieDbAPI.GetMovieInfo(imdbId);
                if (movieInfo != null) {
                    return movieInfo.Id.ToString();
                }
            }

            // if asked for, do a fuzzy match based on title and year
            else if (fuzzyMatch) {
                // grab possible matches by main title + year
                List<DBMovieInfo> results = Search(movie.Title, movie.Year);
                if (results.Count == 0) results = Search(movie.Title);

                // grab possible matches by alt titles
                foreach (string currAltTitle in movie.AlternateTitles) {
                    List<DBMovieInfo> tempResults = Search(movie.Title, movie.Year);
                    if (tempResults.Count == 0) tempResults = Search(movie.Title);

                    results.AddRange(tempResults);
                }

                // pick a possible match if one meets our requirements
                foreach (DBMovieInfo currMatch in results) {
                    if (CloseEnough(currMatch, movie))
                        return currMatch.GetSourceMovieInfo(SourceInfo).Identifier;
                }
            }
            
            return null;
        }

        private bool CloseEnough(DBMovieInfo movie1, DBMovieInfo movie2) {
            if (CloseEnough(movie1.Title, movie2)) return true;

            foreach (string currAltTitle in movie1.AlternateTitles) 
                if (CloseEnough(currAltTitle, movie2)) return true;

            return false;
        }

        private bool CloseEnough(string title, DBMovieInfo movie) {
            int distance;
            
            distance = AdvancedStringComparer.Levenshtein(title, movie.Title);
            if (distance <= MovingPicturesCore.Settings.AutoApproveThreshold)
                return true;

            foreach (string currAltTitle in movie.AlternateTitles) {
                distance = AdvancedStringComparer.Levenshtein(title, currAltTitle);
                if (distance <= MovingPicturesCore.Settings.AutoApproveThreshold)
                    return true;
            }

            return false;
        }

        private string getImageBaseUrl() {
            DateTime now = DateTime.Now;

            DateTime lastCheckDate = DateTime.MinValue;
            if (DateTime.TryParse(MovingPicturesCore.Settings.TMDbConfigLastCheck, out lastCheckDate)) {
                if (now.Subtract(lastCheckDate).Days <= MovingPicturesCore.Settings.TMDbConfigPeriod) {
                    return MovingPicturesCore.Settings.TMDbImageBaseUrl;
                }
            }

            // request configuation object
            var tmdbConfig = TheMovieDbAPI.GetConfiguration();
            if (tmdbConfig == null || tmdbConfig.Images == null)
                return null;

            MovingPicturesCore.Settings.TMDbConfigLastCheck = now.ToString("yyyy-MM-dd");

            // we only ever need the original size
            MovingPicturesCore.Settings.TMDbImageBaseUrl = tmdbConfig.Images.BaseUrl + "original";
            return MovingPicturesCore.Settings.TMDbImageBaseUrl;
        }

        #endregion
    }
}
