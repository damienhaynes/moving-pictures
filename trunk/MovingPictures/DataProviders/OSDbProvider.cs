using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.SignatureBuilders;
using MediaPortal.Plugins.MovingPictures.DataProviders.OpenSubtitles;
using CookComputing.XmlRpc;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {

    public class OSDbProvider : IMovieProvider {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region IMovieProvider Members

        public string Name {
            get {
                return "opensubtitles.org";
            }
        }

        public string Version {
            get {
                return "Internal";
            }
        }

        public string Author {
            get { return "Moving Pictures Team"; }
        }

        public string Description {
            get { return "Returns details from opensubtitles.org."; }
        }

        public string Language {
            get { return new CultureInfo("en").DisplayName; }
        }

        public bool ProvidesMoviesDetails {
            get { return true; }
        }

        public bool ProvidesCoverArt {
            get { return false; }
        }

        public bool ProvidesBackdrops {
            get { return false; }
        }

        public DBSourceInfo SourceInfo {
            get {
                if (_sourceInfo == null)
                    _sourceInfo = DBSourceInfo.GetFromProviderObject(this);
                return _sourceInfo;
            }
        } private DBSourceInfo _sourceInfo;

        public List<DBMovieInfo> Get(MovieSignature movieSignature) {
            List<DBMovieInfo> results = new List<DBMovieInfo>();
            if (movieSignature == null)
                return results;

            if (movieSignature.ImdbId != null) {
                // search by imdbid
                DBMovieInfo movie = GetMovieByImdbId(movieSignature.ImdbId);
                if (movie != null) results.Add(movie);
            }
            else {
                // search by title
                results = GetMoviesByTitle(movieSignature.Title);
            }

            return results;
        }

        public UpdateResults Update(DBMovieInfo movie) {
            if (movie == null)
                return UpdateResults.FAILED;

            string imdbId = movie.ImdbID.Trim();
            if (imdbId != string.Empty) {
                // if we have an imdb id we can also do a valid getInfo request
                // we first need to call the IMDB api search before we can move on
                DBMovieInfo imdbMovie = GetMovieByImdbId(imdbId);
                if (imdbMovie != null) {
                    movie.CopyUpdatableValues(imdbMovie);
                    return UpdateResults.SUCCESS;
                }
                else {
                    return UpdateResults.FAILED;
                }
            }
            
            return UpdateResults.FAILED_NEED_ID;
        }

        public bool GetArtwork(DBMovieInfo movie) {
            throw new NotImplementedException();
        }

        public bool GetBackdrop(DBMovieInfo movie) {
            throw new NotImplementedException();
        }

        #endregion

        #region Static Methods / OpenSubtitles

        private static OpenSubtitlesAPI apiOSDb;
        private static object osdbLock = new object();
        private static Regex exprOSDbMovieResultsTitle = new Regex(@"(.+?)\((\d{4})[\/IVX]*\)(.*)", RegexOptions.IgnoreCase);
        private static Regex exprOSDbMovieResultsAka = new Regex(@"aka ""(.+?)"" ", RegexOptions.IgnoreCase);

        /// <summary>
        /// Ensures a session to OSDb is initiated
        /// </summary>
        /// <returns>True if there's a session</returns>
        private static bool CheckSession() {
            lock (osdbLock) {
                if (apiOSDb == null) {
                    string userAgent = Assembly.GetExecutingAssembly().GetName().Name + "/" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    apiOSDb = new OpenSubtitlesAPI(userAgent);
                }

                // Check to see if we are currently logged in
                // if not try to login
                if (apiOSDb.LoggedIn)
                    return true;
                else
                    return apiOSDb.Login();
            }
        }

        /// <summary>
        /// Get movies from by title/keyword search
        /// </summary>
        /// <param name="query">keywords/title</param>
        /// <returns>List of movies</returns>
        public static List<DBMovieInfo> GetMoviesByTitle(string query) {
            List<DBMovieInfo> results = new List<DBMovieInfo>();
            if (CheckSession()) {
                try {
                    SearchResponse response = apiOSDb.SearchMoviesOnIMDB(query);
                    foreach (MovieResult movieResult in response.results) {
                        DBMovieInfo movie = new DBMovieInfo();
                        Match details = exprOSDbMovieResultsTitle.Match(movieResult.title);
                        if (details.Success) {
                            movie.Title = details.Groups[1].Value.Trim();
                            movie.Year = int.Parse(details.Groups[2].Value);
                            MatchCollection altTitles = exprOSDbMovieResultsAka.Matches(details.Groups[3].Value);
                            if (altTitles.Count > 0) {
                                foreach (Match altTitle in altTitles) {
                                    movie.AlternateTitles.Add(altTitle.Groups[1].Value);
                                }
                            }
                            movie.ImdbID = "tt" + movieResult.id.PadLeft(7, '0');
                            results.Add(movie);
                        }

                    }
                }
                catch (Exception e) {
                    logger.ErrorException("Error: ", e);
                }
            }

            return results;
        }

        /// <summary>
        /// Get movie details using imdbid.
        /// </summary>
        /// <param name="imdbid">ex. tt1234567</param>
        /// <returns>Movie</returns>
        public static DBMovieInfo GetMovieByImdbId(string imdbid) {
            if (CheckSession()) {
                try {
                    MovieDetailsResponse response = apiOSDb.GetIMDBMovieDetails(imdbid);
                    // Only proceed if the response is OK
                    if (response.IsOK) {
                        // Create a new movie object and try to fill all its properties
                        // from the returned OSDb Movie object.
                        DBMovieInfo movie = new DBMovieInfo();
                        MovieDetails movieDetails = response.details;

                        movie.Title = movieDetails.title;
                        movie.Tagline = movieDetails.tagline;
                        movie.Summary = movieDetails.plot;
                        movie.ImdbID = movieDetails.ImdbID;
                        movie.Year = movieDetails.Year;
                        movie.Runtime = movieDetails.Duration;

                        // Try to add all the languages as one comma seperated string
                        if (movieDetails.language != null)
                            movie.Language = string.Join(", ", movieDetails.language);

                        // Try to set the score
                        float score = 0;
                        if (float.TryParse(movieDetails.rating, out score))
                            movie.Score = score;

                        // Only add the title portion for the alternative titles
                        foreach (string aka in movieDetails.AlternateTitles)
                            movie.AlternateTitles.Add(aka.Split('(')[0]);

                        // Try to add the genres
                        if (movieDetails.genres != null)
                            foreach (string genre in movieDetails.genres)
                                movie.Genres.Add(genre);

                        // Try to add the actors
                        if (movieDetails.cast != null)
                            foreach (string actor in movieDetails.cast.Values)
                                movie.Actors.Add(actor);

                        // Try to add the directors
                        if (movieDetails.directors != null)
                            foreach (string director in movieDetails.directors.Values)
                                movie.Directors.Add(director);

                        // Try to add the writers
                        if (movieDetails.writers != null)
                            foreach (string writer in movieDetails.writers.Values)
                                movie.Writers.Add(writer);

                        // return the movie object
                        return movie;
                    }
                }
                catch (Exception e) {
                    logger.ErrorException("Error: ", e);
                }
            }

            return null;
        }

        /// <summary>
        /// Get movie details using filehash
        /// </summary>
        /// <param name="movieHash">Hash of movie file</param>
        /// <returns>Movie or null if no proper match is found.</returns>
        public static DBMovieInfo GetMovieByHash(string movieHash) {
            string[] list = new string[] { movieHash };
            Dictionary<string, DBMovieInfo> result = GetMoviesByHash(list);
            if (result.Count > 0)
                return result[movieHash];
            else
                return null;
        }

        /// <summary>
        /// Get multiple movies by file hash
        /// </summary>
        /// <param name="movieHashArray">an array containing all the hashes that need matching</param>
        /// <returns>a dictionairy containing hash/movie values. the movie value is null if no proper match is found.</returns>
        public static Dictionary<string, DBMovieInfo> GetMoviesByHash(string[] movieHashArray) {
            Dictionary<string, DBMovieInfo> results = new Dictionary<string, DBMovieInfo>();
            if (CheckSession()) {
                try {
                    HashResponse response = apiOSDb.CheckMovieHash2(movieHashArray);
                    // if data is not of type XmlRpcStruct we got no results
                    if (response.IsOK) {
                        if (response.results.GetType() != typeof(XmlRpcStruct))
                            return results;

                        XmlRpcStruct hashList = (XmlRpcStruct) response.results;
                        int osdbSeenTreshold = (int)MovingPicturesCore.SettingsManager["importer_lookup_hash_seencount"].Value;

                        // Get movie for each hash given
                        foreach (string hash in hashList.Keys) {
                            object[] movieResults = (object[])hashList[hash];
                            // Only returns the first result (the most seen hash-movie combination)
                            if (movieResults.Length > 0) {
                                // Translate the results to a DBMovieInfo object
                                XmlRpcStruct movieResult = (XmlRpcStruct)movieResults[0];
                                // check if the seen count meets the user's preference
                                if (int.Parse((string)movieResult["SeenCount"]) >= osdbSeenTreshold) {
                                    DBMovieInfo movie = new DBMovieInfo();
                                    movie.Title = (string)movieResult["MovieName"];
                                    movie.Year = int.Parse((string)movieResult["MovieYear"]);
                                    string imdbid = (string)movieResult["MovieImdbID"];
                                    movie.ImdbID = "tt" + imdbid.PadLeft(7, '0');
                                    results.Add(hash, movie);
                                }
                            }
                        }
                    }
                }
                catch (Exception e) {
                    logger.ErrorException("Error: ", e);
                }
            }
            return results;
        }

        #endregion

    }

}
