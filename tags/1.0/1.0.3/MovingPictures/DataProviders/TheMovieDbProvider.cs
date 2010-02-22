using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using Cornerstone.Tools;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.SignatureBuilders;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using NLog;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieResources;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {

    class TheMovieDbProvider: InternalProvider, IMovieProvider {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        // NOTE: To other developers creating other applications, using this code as a base
        //       or as a reference. PLEASE get your own API key. Do not reuse the one listed here
        //       it is intended for Moving Pictures use ONLY. API keys are free and easy to apply
        //       for. Visit this url: http://api.themoviedb.org/2.0/docs/

        #region API variables

        private const string apiMovieUrl = "http://api.themoviedb.org/2.1/{0}/{1}/xml/cc25933c4094ca50635f94574491f320/";

        private static string apiMovieImdbLookup = string.Format(apiMovieUrl, "Movie.imdbLookup", "en");
        private static string apiMovieSearch = string.Format(apiMovieUrl, "Movie.search", "en");
        private static string apiMovieGetInfo = string.Format(apiMovieUrl, "Movie.getInfo", "en");
        private static string apiHashGetInfo = string.Format(apiMovieUrl, "Hash.getInfo", "en");

        #endregion

        public string Name {
            get {
                return "themoviedb.org";
            }
        }

        public string Description {
            get { return "Returns details, covers and backdrops from themoviedb.org."; }
        }

        public string Language {
            get { return new CultureInfo("en").DisplayName; }
        }

        public string LanguageCode {
            get { return "en"; }
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
            string tmdbID = getTheMovieDbId(movie);
            if (tmdbID == null) {
                return false;
            }

            // Try to get movie information
            XmlNodeList xml = getXML(apiMovieGetInfo + tmdbID);
            if (xml == null) {
                return false;
            }

            // try to grab backdrops from the resulting xml doc
            string backdropURL = string.Empty;
            XmlNodeList backdropNodes = xml.Item(0).SelectNodes("//image[@type='backdrop']");
            foreach (XmlNode currNode in backdropNodes) {
                if (currNode.Attributes["size"].Value == "original") {
                    backdropURL = currNode.Attributes["url"].Value;
                    if (backdropURL.Trim().Length > 0) {
                        if (movie.AddBackdropFromURL(backdropURL) == ImageLoadResults.SUCCESS) {
                            movie.GetSourceMovieInfo(SourceInfo).Identifier = tmdbID;
                            return true;
                        }
                    }
                }
            }

            // if we get here we didn't manage to find a proper backdrop
            // so return false
            return false;
        }

        public List<DBMovieInfo> Get(MovieSignature movieSignature) {
           List<DBMovieInfo> results = new List<DBMovieInfo>();
           if (movieSignature == null)
               return results;

           if (movieSignature.ImdbId != null) {
               DBMovieInfo movie = getMovieByImdb(movieSignature.ImdbId);
               if (movie != null) {
                   results.Add(movie);
                   return results;
               }
           }
           
           results = getMoviesByTitle(movieSignature.Title);
           return results;
        }

        private List<DBMovieInfo> getMoviesByTitle(string title) {
            List<DBMovieInfo> results = new List<DBMovieInfo>();
            XmlNodeList xml = getXML(apiMovieSearch + title);
            if (xml == null)
                return results;

            XmlNodeList movieNodes = xml.Item(0).SelectNodes("//movie");
            foreach (XmlNode node in movieNodes) {
                DBMovieInfo movie = getMovieInformation(node);
                if (movie != null)
                    results.Add(movie);
            }
            return results;
        }

        public List<DBMovieInfo> GetMoviesByHash(string hash) {
            List<DBMovieInfo> results = new List<DBMovieInfo>();
            XmlNodeList xml = getXML(apiHashGetInfo + hash);
            if (xml == null)
                return results;

            XmlNodeList movieNodes = xml.Item(0).SelectNodes("//movie");
            foreach (XmlNode node in movieNodes) {
                DBMovieInfo movie = getMovieInformation(node);
                if (movie != null)
                    results.Add(movie);
            }
            return results;
        }

        private DBMovieInfo getMovieById(string id) {
            XmlNodeList xml = getXML(apiMovieGetInfo + id);
            if (xml == null)
                return null;
            
            XmlNodeList movieNodes = xml.Item(0).SelectNodes("//movie");
            if (movieNodes.Count > 0)
                return getMovieInformation(movieNodes[0]);
            else
                return null;
        }

        private DBMovieInfo getMovieByImdb(string imdbid) {
            XmlNodeList xml = getXML(apiMovieImdbLookup + imdbid);
            if (xml == null)
                return null;

            XmlNodeList movieNodes = xml.Item(0).SelectNodes("//movie");
            if (movieNodes.Count > 0)
                foreach (XmlNode node in movieNodes)
                    return getMovieInformation(node);
            
            return null;
        }

        private DBMovieInfo getMovieInformation(XmlNode movieNode) {
            if (movieNode == null)
                return null;

            if (movieNode.ChildNodes.Count < 2 || movieNode.Name != "movie")
                return null;

            DBMovieInfo movie = new DBMovieInfo();
            foreach (XmlNode node in movieNode.ChildNodes) {
                string value = node.InnerText;
                switch (node.Name) {
                    case "id":
                        movie.GetSourceMovieInfo(SourceInfo).Identifier = value;
                        break;
                    case "name":
                        movie.Title = value;
                        break;
                    case "alternative_name":
                        // todo: remove this check when the api is fixed
                        if (value.Trim() != "None found." && value.Trim().Length > 0 )
                            movie.AlternateTitles.Add(value);
                        break;
                    case "released":
                        DateTime date;
                        if (DateTime.TryParse(value, out date))
                            movie.Year = date.Year;      
                        break;
                    case "imdb_id":
                        movie.ImdbID = value;
                        break;
                    case "url":
                        movie.DetailsURL = value;
                        break;
                    case "overview":
                        movie.Summary = value;
                        break;
                    case "rating":
                        float rating = 0;
                        if (float.TryParse(value, out rating))
                            movie.Score = rating;
                        break;
                    case "popularity":
                        int popularity = 0;
                        if (int.TryParse(value, out popularity))
                            movie.Popularity = popularity;
                        break;
                    case "runtime":
                        int runtime = 0;
                        if (int.TryParse(value, out runtime))
                            movie.Runtime = runtime;
                        break;
                    case "cast": // Actors, Directors and Writers
                        foreach (XmlNode person in node.ChildNodes) {
                            string name = person.Attributes["name"].Value;
                            string job = person.Attributes["job"].Value;
                            switch (job) {
                                case "Director":
                                    movie.Directors.Add(name);
                                    break;
                                case "Actor":
                                    movie.Actors.Add(name);
                                    break;
                                case "Screenplay":
                                    movie.Writers.Add(name);
                                    break;
                            }
                        }
                        break;
                    case "categories":
                        foreach (XmlNode category in node.ChildNodes) {
                            string genre = category.Attributes["name"].Value;
                            string type = category.Attributes["type"].Value;
                            if (type == "genre") {
                                movie.Genres.Add(genre);
                            }
                        }
                        break;
                }
            }
            return movie;
        }

        public UpdateResults Update(DBMovieInfo movie) {
            if (movie == null)
                return UpdateResults.FAILED;

            string tmdbId = getTheMovieDbId(movie);
            // check if tmdbId is still null, if so request id.
            if (tmdbId == null)
                return UpdateResults.FAILED_NEED_ID;

            // Grab the movie using the TMDB ID
            DBMovieInfo newMovie = getMovieById(tmdbId);
            if (newMovie != null) {
                movie.GetSourceMovieInfo(SourceInfo).Identifier = tmdbId;
                movie.CopyUpdatableValues(newMovie);
                return UpdateResults.SUCCESS;
            }
            else {
                return UpdateResults.FAILED;
            }
        }

        public bool GetArtwork(DBMovieInfo movie) {
            if (movie == null)
                return false;

            // do we have an id?
            string tmdbID = getTheMovieDbId(movie);
            if (tmdbID == null) {
                return false;
            }

            // Tro to get movie information
            XmlNodeList xml = getXML(apiMovieGetInfo + tmdbID);
            if (xml == null) {
                return false;
            }

            // grab coverart loading settings
            int maxCovers = MovingPicturesCore.Settings.MaxCoversPerMovie;
            int maxCoversInSession = MovingPicturesCore.Settings.MaxCoversPerSession;

            int coversAdded = 0;
            int count = 0;
            
            // try to grab posters from the xml results
            XmlNodeList posterNodes = xml.Item(0).SelectNodes("//image[@type='poster']");
            foreach (XmlNode posterNode in posterNodes) {
                if (posterNode.Attributes["size"].Value == "original") {
                    // if we have hit our limit quit
                    if (movie.AlternateCovers.Count >= maxCovers || coversAdded >= maxCoversInSession)
                        return true;

                    // get url for cover and load it via the movie object
                    string coverPath = posterNode.Attributes["url"].Value;
                    if (coverPath.Trim() != string.Empty)
                        if (movie.AddCoverFromURL(coverPath) == ImageLoadResults.SUCCESS)
                            coversAdded++;

                    count++;
                }
            }



            if (coversAdded > 0) {
                // Update source info
                movie.GetSourceMovieInfo(SourceInfo).Identifier = tmdbID;
                return true;
            }

            return false;
        }

        private string getTheMovieDbId(DBMovieInfo movie) {
            string tmdbID = null;
            DBSourceMovieInfo idObj = movie.GetSourceMovieInfo(SourceInfo);
            if (idObj != null && idObj.Identifier != null) {
                tmdbID = idObj.Identifier;
            }
            else {
                // Translate IMDB code to a TMDB ID

                if (movie.ImdbID == null) {
                    return null;
                }

                string imdbId = movie.ImdbID.Trim();
                if (imdbId.Length == 0)
                    return null;

                // Do an IMDB lookup
                XmlNodeList xml = getXML(apiMovieImdbLookup + imdbId);
                if (xml != null && xml.Count > 0) {
                    // Get TMDB Id
                    XmlNodeList idNodes = xml.Item(0).SelectNodes("//id");
                    if (idNodes.Count != 0) {
                        tmdbID = idNodes[0].InnerText;
                    }
                }
            }
            return tmdbID;
        }

        /// <summary>
        /// Returns a movie list queries by filehash
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static List<DBMovieInfo> GetMoviesByHashLookup(string hash) {
            List<DBMovieInfo> results = new List<DBMovieInfo>();
            XmlNodeList xml = getXML(apiHashGetInfo + hash);
            if (xml == null)
                return results;

            XmlNodeList movieNodes = xml.Item(0).SelectNodes("//movie");
            foreach (XmlNode movieNode in movieNodes) {

                if (movieNode == null || movieNode.ChildNodes.Count < 2)
                    continue;

                DBMovieInfo movie = new DBMovieInfo();
                foreach (XmlNode node in movieNode.ChildNodes) {
                    string value = node.InnerText;
                    switch (node.Name) {
                        case "name":
                            movie.Title = value;
                            break;
                        case "imdb_id":
                            movie.ImdbID = value;
                            break;
                        case "released":
                            DateTime date;
                            if (DateTime.TryParse(value, out date))
                                movie.Year = date.Year;
                            break;
                    }
                }
                results.Add(movie);
            }
            return results;
        }

        // given a url, retrieves the xml result set and returns the nodelist of Item objects
        private static XmlNodeList getXML(string url) {
            WebGrabber grabber = Utility.GetWebGrabberInstance(url);
            grabber.Encoding = Encoding.UTF8;

            if (grabber.GetResponse())
                return grabber.GetXML("OpenSearchDescription");
            else
                return null;
        }

    }

}
