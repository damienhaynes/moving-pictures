using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using Cornerstone.Tools;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.SignatureBuilders;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    class TheMovieDbProvider: IMovieProvider {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        // NOTE: To other developers creating other applications, using this code as a base
        //       or as a reference. PLEASE get your own API key. Do not reuse the one listed here
        //       it is intended for Moving Pictures use ONLY. API keys are free and easy to apply
        //       for. Visit this url: http://api.themoviedb.org/2.0/docs/

        #region API variables

        private const string apiImdbLookup = "http://api.themoviedb.org/2.0/Movie.imdbLookup?api_key=cc25933c4094ca50635f94574491f320&imdb_id=";
        private const string apiSearch = "http://api.themoviedb.org/2.0/Movie.search?api_key=cc25933c4094ca50635f94574491f320&title=";
        private const string apiGetInfo = "http://api.themoviedb.org/2.0/Movie.getInfo?api_key=cc25933c4094ca50635f94574491f320&id=";

        #endregion

        public string Name {
            get {
                return "themoviedb.org";
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
            get { return "Returns details, covers and backdrops from themoviedb.org."; }
        }

        public string Language {
            get { return new CultureInfo("en").DisplayName; }
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

        public DBSourceInfo SourceInfo {
            get {
                if (_sourceInfo == null)
                    _sourceInfo = DBSourceInfo.GetFromProviderObject(this);
                return _sourceInfo;
            }
        } private DBSourceInfo _sourceInfo;

        public bool GetBackdrop(DBMovieInfo movie) {
            if (movie == null)
                return false;

            // if we already have a backdrop move on for now
            if (movie.BackdropFullPath.Trim().Length > 0)
                return true;
            
            // an imdb tag is required for this dataprovider. if it is not set, fail
            if (movie.ImdbID.Trim().Length != 9)
                return false;

            // try to grab the xml results
            XmlNodeList xml = getXML(apiImdbLookup + movie.ImdbID.Trim());
            if (xml == null)
                return false;

            // try to grab backdrops from the resulting xml doc
            string backdropURL = string.Empty;
            XmlNodeList backdropNodes = xml.Item(0).SelectNodes("//backdrop");
            foreach (XmlNode currNode in backdropNodes) {
                if (currNode.Attributes["size"].Value == "original") {
                    backdropURL = currNode.InnerText;
                    if (backdropURL.Trim().Length > 0)
                        if (movie.AddBackdropFromURL(backdropURL) == ArtworkLoadStatus.SUCCESS)
                            return true;
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
               if (movie != null)
                   results.Add(movie);
           }
           else {
             results = getMoviesByTitle(movieSignature.Title);
           }

           return results;
        }

        private List<DBMovieInfo> getMoviesByTitle(string title) {
            List<DBMovieInfo> results = new List<DBMovieInfo>();
            XmlNodeList xml = getXML(apiSearch + title);
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

        private DBMovieInfo getMovieByImdb(string imdbid) {
            XmlNodeList xml = getXML(apiImdbLookup + imdbid);
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
                    case "title":
                        movie.Title = value;
                        break;
                    case "alternative_title":
                        // todo: remove this check when the api is fixed
                        if (value.Trim() != "None found." && value.Trim().Length > 0 )
                            movie.AlternateTitles.Add(value);
                        break;
                    case "release":
                        DateTime date;
                        if (DateTime.TryParse(value, out date))
                            movie.Year = date.Year;      
                        break;
                    case "imdb":
                        movie.ImdbID = value;
                        break;
                    case "url":
                        movie.DetailsURL = value;
                        break;
                    case "short_overview":
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
                    case "people":
                        foreach (XmlNode person in node.SelectNodes("person[@job='director']/name")) {
                            movie.Directors.Add(person.InnerText.Trim());
                        }
                        foreach (XmlNode person in node.SelectNodes("person[@job='screenplay']/name")) {
                            movie.Writers.Add(person.InnerText.Trim());
                        }
                        foreach (XmlNode person in node.SelectNodes("person[@job='actor']/name")) {
                            movie.Actors.Add(person.InnerText.Trim());
                        }
                        break;
                    case "categories":
                        //todo: uncomment and adapt when the genres are implemented
                        //foreach (XmlNode category in node.SelectNodes("category/name")) {
                        //    movie.Genres.Add(category.InnerText.Trim());
                        //}
                        break;
                }
            }
            return movie;
        }

        public UpdateResults Update(DBMovieInfo movie) {
            if (movie == null)
                return UpdateResults.FAILED;

            // try to load the id for the movie for this script
            string tmdbId = null;
            DBSourceMovieInfo idObj = movie.GetSourceMovieInfo(SourceInfo);
            if (idObj != null && idObj.Identifier != null) {
                tmdbId = idObj.Identifier;
            } else {
                // if we don't have a tmdb id check for imdb id
                string imdbId = movie.ImdbID.Trim();
                if (imdbId != string.Empty) {
                    // if we have an imdb id we can also do a valid getInfo request
                    // we first need to call the IMDB api search before we can move on
                    DBMovieInfo imdbMovie = getMovieByImdb(imdbId);
                    if (imdbMovie != null) {
                        // if we got a movie, check for identifier again
                        idObj = imdbMovie.GetSourceMovieInfo(SourceInfo);
                        if (idObj != null && idObj.Identifier != null) {
                            // we got an identifier so fill tmdbId
                            tmdbId = idObj.Identifier;
                        }
                    }
                }

                // check if tmdbId is still null, if so request id.
                if (tmdbId == null)
                    return UpdateResults.FAILED_NEED_ID;
            }

            XmlNodeList xml = getXML(apiGetInfo + tmdbId);
            if (xml != null) {
                XmlNodeList movieNodes = xml.Item(0).SelectNodes("//movie");
                if (movieNodes.Count > 0) {
                    DBMovieInfo newMovie = getMovieInformation(movieNodes[0]);
                    if (newMovie != null) {
                        movie.CopyUpdatableValues(newMovie);
                        return UpdateResults.SUCCESS;
                    }
                }
            }

            return UpdateResults.FAILED;
        }

        public bool GetArtwork(DBMovieInfo movie) {
            if (movie == null)
                return false;

            // grab coverart loading settings
            int maxCovers = (int)MovingPicturesCore.SettingsManager["max_covers_per_movie"].Value;
            int maxCoversInSession = (int)MovingPicturesCore.SettingsManager["max_covers_per_session"].Value;

            // if we have already hit our limit for the number of covers to load, quit
            if (movie.AlternateCovers.Count >= maxCovers)
                return true;

            // an imdb tag is required for this dataprovider. if it is not set, fail
            if (movie.ImdbID.Trim().Length != 9)
                return false;

            // try to grab the xml results
            XmlNodeList xml = getXML(apiImdbLookup + movie.ImdbID.Trim());
            if (xml == null)
                return false;

            int coversAdded = 0;
            int count = 0;
            
            // try to grab posters from the xml results
            XmlNodeList posterNodes = xml.Item(0).SelectNodes("//poster");
            foreach (XmlNode posterNode in posterNodes) {
                if (posterNode.Attributes["size"].Value == "original") {
                    // if we have hit our limit quit
                    if (movie.AlternateCovers.Count >= maxCovers || coversAdded >= maxCoversInSession)
                        return true;

                    // get url for cover and load it via the movie object
                    string coverPath = posterNode.InnerText;
                    if (movie.AddCoverFromURL(coverPath) == ArtworkLoadStatus.SUCCESS)
                        coversAdded++;

                    count++;
                }
            }

            if (coversAdded > 0)
                return true;

            return false;
        }

        // given a url, retrieves the xml result set and returns the nodelist of Item objects
        private XmlNodeList getXML(string url) {
            WebGrabber grabber = new WebGrabber(url);
            grabber.MaxRetries = (int)MovingPicturesCore.SettingsManager["tmdb_max_timeouts"].Value;
            grabber.Timeout = (int)MovingPicturesCore.SettingsManager["tmdb_timeout_length"].Value;
            grabber.TimeoutIncrement = (int)MovingPicturesCore.SettingsManager["tmdb_timeout_increment"].Value;
            grabber.Encoding = Encoding.UTF8;

            if (grabber.GetResponse())
                return grabber.GetXML("results");
            else
                return null;
        }

    }
}
