using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using System.Net;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Web;
using System.Threading;
using MediaPortal.Plugins.MovingPictures.Database.CustomTypes;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;
using NLog;
using System.Drawing;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    class MovieXMLProvider: IMovieProvider {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private const string urlGetByName = "http://www.movie-xml.com/interfaces/getmovie.php?moviename=";
        private const string urlGetByID = "http://www.movie-xml.com/interfaces/getmovieID.php?id=";
        private const string urlGetByImdbID = "http://www.movie-xml.com/interfaces/getmovieimdb.php?imdb=";
        private const string urlGetCoverByID = "http://www.movie-xml.com/interfaces/getmoviecovers.php?id=";
        private const string urlCoverRetrievePrefix = "http://www.movie-xml.com/banners/";

        // Returns a list of DBMovieInfo objects closely matching the given movie title, 
        // using http://www.movie-xml.com as a datasource.
        public List<DBMovieInfo> Get(string movieTitle) {
            string safeTitle = HttpUtility.UrlEncode(movieTitle);
            safeTitle = safeTitle.Replace("'", "''");

            XmlNodeList xmlNodes = getXML(urlGetByName + safeTitle);
            return parseXML(xmlNodes);
        }

        // Returns a DBMovieInfo object cooresponding to the movie-xml.com ID provided,
        // using http://www.movie-xml.com as a datasource.
        public DBMovieInfo getByMovieXmlID(int movieXmlID) {
            XmlNodeList xmlNodes = getXML(urlGetByID + movieXmlID);
            List<DBMovieInfo> movieList = parseXML(xmlNodes);

            if (movieList.Count == 0)
                return null;

            return movieList[0];
        }

        // Returns a DBMovieInfo object cooresponding to the movie-xml.com ID provided,
        // using http://www.movie-xml.com as a datasource.
        public DBMovieInfo getByImdbID(string imdbID) {
            XmlNodeList xmlNodes = getXML(urlGetByImdbID + imdbID);
            List<DBMovieInfo> movieList = parseXML(xmlNodes);

            if (movieList.Count == 0)
                return null;

            return movieList[0];
        }

        public void Update(DBMovieInfo movie) {
            if (movie.MovieXmlID != 0) {
                DBMovieInfo newInfo = getByImdbID(movie.ImdbID);

                movie.CopyUpdatableValues(newInfo);
            }
        }

        // Grabs artwork provided by movie-xml.com. Much of this needs to be refactored to
        // DBMovieInfo. Should pass in a URL to an image.
        public bool GetArtwork(DBMovieInfo movie) {
            if (movie == null || movie.MovieXmlID == null)
                return false;

            // grab coverart loading settings
            int maxCovers = (int) MovingPicturesPlugin.SettingsManager["max_covers_per_movie"].Value;
            int maxCoversInSession = (int) MovingPicturesPlugin.SettingsManager["max_covers_per_session"].Value;

            // if we have already hit our limit for the number of covers to load, quit
            if (movie.AlternateCovers.Count >= maxCovers)
                return true;

            // grab the list of URLs for cover art from movie-xml.com 
            XmlNodeList xmlNodes = getXML(urlGetCoverByID + movie.MovieXmlID);
            if (xmlNodes == null)
                return false;

            int coversAdded = 0;
            foreach (XmlNode currNode in xmlNodes) {
                // if we have hit our limit quit
                if (movie.AlternateCovers.Count >= maxCovers || coversAdded > maxCoversInSession)
                    return true;
                
                // check we have a valid xml element
                XmlElement filenameElement = currNode["filename"];
                if (filenameElement == null)
                    continue;

                // get url for cover and load it via the movie object
                string coverPath = urlCoverRetrievePrefix + filenameElement.InnerText;
                if (movie.AddCoverFromURL(coverPath) == CoverArtLoadStatus.SUCCESS) 
                    coversAdded++;
            }
            return true;

        }

        #region Private Methods

        // given a url, retrieves the xml result set and returns the nodelist of Item objects
        private XmlNodeList getXML(string url) {
            String sXmlData = string.Empty;

            int tryCount = 0;
            int maxRetries       = (int) MovingPicturesPlugin.SettingsManager["xml_max_timeouts"].Value;
            int timeout          = (int) MovingPicturesPlugin.SettingsManager["xml_timeout_length"].Value;
            int timeoutIncrement = (int) MovingPicturesPlugin.SettingsManager["xml_timeout_increment"].Value; 

            while (sXmlData == string.Empty) {
                try {
                    // builds the request and retrieves the respones from movie-xml.com
                    tryCount++;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Timeout = timeout + (timeoutIncrement * tryCount);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    // converts the resulting stream to a string for easier use
                    Stream resultData = response.GetResponseStream();
                    StreamReader reader = new StreamReader(resultData, Encoding.Default, true);
                    sXmlData = reader.ReadToEnd().Replace('\0', ' ');

                    resultData.Close();
                    reader.Close();
                    response.Close();
                }
                catch (WebException e) {
                    if (tryCount == maxRetries) {
                        logger.ErrorException("Error connecting to MovieXML web service. Reached retry limit of " + maxRetries, e);
                        return null;
                    }
                }
            }

            try {
                // attempts to convert the returned string into an XmlDocument
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(sXmlData);
                XmlNode root = doc.FirstChild.NextSibling;
                if (root.Name == "Items") 
                    return root.ChildNodes;
                
            }
            catch (XmlException e) {
                logger.ErrorException("Error parsing results from MovieXML web service (" + url + ").", e);
            }

            return null;
        }
        
        // Takes an XmlNodeList of Item nodes returned from movie-xml.com and converts it to 
        // a list of DBMovieInfo objects.
        private List<DBMovieInfo> parseXML(XmlNodeList xmlNodes) {
            List<DBMovieInfo> movieList = new List<DBMovieInfo>();

            if (xmlNodes == null)
                return movieList;

            foreach (XmlNode currMovieNode in xmlNodes) {
                DBMovieInfo newMovie = new DBMovieInfo();

                // temp variables for parsing non string data types
                bool ok;
                int tmpInt;
                float tmpFloat;

                // loop through each tag and store the data appropriately
                foreach (XmlElement currElement in currMovieNode) {
                    switch (currElement.Name) {
                        case "id":
                            ok = int.TryParse(currElement.InnerText.Trim(), out tmpInt);
                            if (ok)
                                newMovie.MovieXmlID = tmpInt;
                            break;
                        case "SeriesName":
                            newMovie.Name = currElement.InnerText;
                            break;
                        case "SeriesID":
                            newMovie.ImdbID = currElement.InnerText;
                            break;
                        case "MainLanguage":
                            newMovie.Language = currElement.InnerText;
                            break;
                        case "Yearmade":
                            ok = int.TryParse(currElement.InnerText.Trim(), out tmpInt);
                            if (ok)
                                newMovie.Year = tmpInt;
                            break;
                        case "Director":
                            newMovie.Directors = new StringList(currElement.InnerText);
                            break;
                        case "Writer":
                            newMovie.Writers = new StringList(currElement.InnerText);
                            break;
                        case "Runtime":
                            ok = int.TryParse(currElement.InnerText.Trim(), out tmpInt);
                            if (ok)
                                newMovie.Runtime = tmpInt;
                            break;
                        case "Genre":
                            newMovie.Genres = new StringList(currElement.InnerText);
                            break;
                        case "Actors":
                            newMovie.Actors = new StringList(currElement.InnerText);
                            break;
                        case "Tagline":
                            newMovie.Tagline = currElement.InnerText;
                            break;
                        case "Shortoverview":
                            if (currElement.InnerText.Length > newMovie.Summary.Length)
                                newMovie.Summary = currElement.InnerText;
                            break;
                        case "Overview":
                            if (currElement.InnerText.Length > newMovie.Summary.Length)
                                newMovie.Summary = currElement.InnerText;
                            break;
                        case "Score":
                            ok = float.TryParse(currElement.InnerText.Trim(), out tmpFloat);
                            if (ok)
                                newMovie.Score = tmpFloat;
                            break;
                        case "Trailerlink":
                            newMovie.TrailerLink = currElement.InnerText;
                            break;
                        case "Certification":
                            newMovie.Certification = currElement.InnerText;
                            break;
                        default:
                            break;
                    }
                }

                // if we at least got a moviename, store the results for return
                if (newMovie.Name != string.Empty) {
                    movieList.Add(newMovie);
                }
            }

            return movieList;
        }

        #endregion
    }
}
