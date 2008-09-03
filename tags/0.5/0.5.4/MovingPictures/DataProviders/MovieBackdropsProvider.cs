using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;
using NLog;
using System.Net;
using System.IO;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    class MovieBackdropsProvider: IBackdropProvider {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        // this gives a url list of backdrops and their thumbs. not needed now since
        // we only download the first backdrop
        private const string availableBackdropsURL = "http://moviebackdrops.com/backdrops/";
        
        // drect url access to a backdrop for a movie, given it's imdb id
        private const string moviebackdropsURL = "http://moviebackdrops.com/backdrops/"; // %imdb%/1.jpg
        private const string meligroveURL = "http://www.meligrove.com/images/posters/movies/jpg/"; // %imdb%.jpg

        public bool GetBackdrop(DBMovieInfo movie) {
            if (movie == null)
                return false;

            // if we already have a backdrop move on for now
            if (movie.BackdropFullPath.Trim().Length > 0)
                return true;
            
            // an imdb tag is required for this dataprovider. if it is not set, fail
            if (movie.ImdbID.Trim().Length == 0)
                return false;

            // This is needed for when expanded fanart support is added
            // List<string[]> backdropList = getBackdropList(movie);

            // try to grab a backdrop from meligrove
            ArtworkLoadStatus status = movie.AddBackdropFromURL(meligroveURL + movie.ImdbID.Trim() + ".jpg"); 

            // if we didnt get one
            if (movie.BackdropFullPath.Trim().Length > 0)
                return true;
            
            // try to grab a backdrop from moviebackdrops.com
            movie.AddBackdropFromURL(moviebackdropsURL + movie.ImdbID.Trim() + "/1.jpg");

            if (movie.BackdropFullPath.Trim().Length > 0)
                return true;
            
            return false;
        }

        private List<string[]> getBackdropList(DBMovieInfo movie) {
            int tryCount = 0;
            int maxRetries = (int)MovingPicturesCore.SettingsManager["moviedb_max_timeouts"].Value;
            int timeout = (int)MovingPicturesCore.SettingsManager["moviedb_timeout_length"].Value;
            int timeoutIncrement = (int)MovingPicturesCore.SettingsManager["moviedb_timeout_increment"].Value;

            String url = availableBackdropsURL + movie.ImdbID + "/";
            String backdropURLResponse = string.Empty;
            while (backdropURLResponse == string.Empty) {
                try {
                    // builds the request and retrieves the respones from moviebackdrops.com
                    tryCount++;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Timeout = timeout + (timeoutIncrement * tryCount);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    // converts the resulting stream to a string for easier use
                    Stream resultData = response.GetResponseStream();
                    StreamReader reader = new StreamReader(resultData, Encoding.Default, true);
                    backdropURLResponse = reader.ReadToEnd().Replace('\0', ' ');

                    resultData.Close();
                    reader.Close();
                    response.Close();
                }
                catch (WebException e) {
                    if (e.Message.Contains("404")) {
                        logger.Debug("MovieBackdrops.com returned 404 for " + url + ". Usually means no art available.");
                        return null;
                    }
                    if (tryCount == maxRetries) {
                        logger.ErrorException("Error connecting to MovieBackdrops.com web service (" + url + "). Reached retry limit of " + maxRetries, e);
                        return null;
                    }
                }
            }

            // results are a list of backdrops and their thumbnails, pairs seperated by a new
            // line and cover and thumb seperated by a tab. So lets split things up.
            List<string[]> backdrops = new List<string[]>();
            string[] backdropPairList = backdropURLResponse.Split('\n');
            foreach (string currPair in backdropPairList) {
                if (currPair.Trim().Length == 0)
                    continue;

                string[] parsedPair = currPair.Split('\t');
                backdrops.Add(parsedPair);
                logger.Debug("Backdrop URLs: " + parsedPair[0] + ", " + parsedPair[1]);
            }

            return backdrops;

        }
    }
}
