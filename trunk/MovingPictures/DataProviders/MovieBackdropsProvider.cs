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
        
        private const string availableBackdropsURL = "http://moviebackdrops.com/backdrops/";
        
        // alternative source. this needs to be split off into another class
        private const string alternateURL = "http://posters.consumeroo.com/movies/jpg/";

        public bool GetBackdrop(DBMovieInfo movie) {
            if (movie == null)
                return false;

            // if we already have a backdrop move on for now
            if (movie.BackdropFullPath.Trim().Length > 0)
                return true;
            
            // an imdb tag is required for this dataprovider. if it is not set, fail
            if (movie.ImdbID.Trim().Length == 0)
                return false;

            List<string[]> backdropList = getBackdropList(movie);

            if (backdropList != null && backdropList.Count > 0)
                movie.AddBackdropFromURL(backdropList[0][0]);
            else
                movie.AddBackdropFromURL(alternateURL + movie.ImdbID.Trim() + ".jpg");
            

            return true;
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
