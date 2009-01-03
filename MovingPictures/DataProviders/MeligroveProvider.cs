using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;
using System.Net;
using System.IO;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using System.Reflection;
using System.Globalization;
using System.Xml;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    class MeligroveProvider: IMovieProvider {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        // NOTE: To other developers creating other applications, using this code as a base
        //       or as a reference. PLEASE get your own API key. Do not reuse the one listed here
        //       it is intended for Moving Pictures use ONLY. API keys are free and easy to apply
        //       for. Visit this url: http://api.themoviedb.org/2.0/docs/
        private const string retrieveByImdb = "http://api.themoviedb.org/2.0/Movie.imdbLookup?api_key=cc25933c4094ca50635f94574491f320&imdb_id="; 

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
            get { return "Returns backdrops from themoviedb.org."; }
        }

        public string Language {
            get { return new CultureInfo("en").DisplayName; }
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
            
            // an imdb tag is required for this dataprovider. if it is not set, fail
            if (movie.ImdbID.Trim().Length != 9)
                return false;

            // try to grab the xml results
            XmlNodeList xml = getXML(retrieveByImdb + movie.ImdbID.Trim());
            if (xml == null)
                return false;

            // try to grab backdrops from the resulting xml doc
            string backdropURL = string.Empty;
            XmlNodeList backdropNodes = xml.Item(0).SelectNodes("//backdrop");
            foreach (XmlNode currNode in backdropNodes) {
                if (currNode.Attributes["size"].Value == "original") {
                    backdropURL = currNode.InnerText;
                    break;
                }
            }

            if (backdropURL == string.Empty)
                return false;
            ArtworkLoadStatus status = movie.AddBackdropFromURL(backdropURL); 

            if (movie.BackdropFullPath.Trim().Length > 0)
                return true;
            
            return false;
        }

        public List<DBMovieInfo> Get(MovieSignature movieSignature) {
            throw new NotImplementedException();
        }

        public UpdateResults Update(DBMovieInfo movie) {
            throw new NotImplementedException();
        }

        public bool GetArtwork(DBMovieInfo movie) {
            throw new NotImplementedException();
        }

        // given a url, retrieves the xml result set and returns the nodelist of Item objects
        private XmlNodeList getXML(string url) {
            String sXmlData = string.Empty;

            int tryCount = 0;
            int maxRetries = (int)MovingPicturesCore.SettingsManager["tmdb_max_timeouts"].Value;
            int timeout = (int)MovingPicturesCore.SettingsManager["tmdb_timeout_length"].Value;
            int timeoutIncrement = (int)MovingPicturesCore.SettingsManager["tmdb_timeout_increment"].Value;

            while (sXmlData == string.Empty) {
                try {
                    // builds the request and retrieves the respones from the url
                    tryCount++;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Timeout = timeout + (timeoutIncrement * tryCount);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    // converts the resulting stream to a string for easier use
                    Stream resultData = response.GetResponseStream();
                    StreamReader reader = new StreamReader(resultData, Encoding.UTF8, true);
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
                catch (IOException e) {
                    logger.ErrorException("Error reading data from connection to themoviedb.org", e);
                    return null;
                }
            }

            try {
                // attempts to convert the returned string into an XmlDocument
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(sXmlData);
                XmlNode root = doc.FirstChild.NextSibling;
                if (root.Name == "results")
                    return root.ChildNodes;

            }
            catch (XmlException e) {
                logger.ErrorException("Error parsing results from themoviedb.org web service (" + url + ").", e);
            }

            return null;
        }

    }
}
