using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.SignatureBuilders {
    class ImdbBuilder: ISignatureBuilder {
        
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public MovieSignature UpdateSignature(MovieSignature signature) {
            
            // If there's no ImdbId in the signature return the signature immediatly
            if (String.IsNullOrEmpty(signature.ImdbId))
                return signature;

            // Try to retrieve the IMDB details page
            string detailsPage = getImdbDetailsPage(signature.ImdbId);
            
            // if we don't have a details page then return the signature
            if (String.IsNullOrEmpty(detailsPage))
                return signature;

            // See if we get a Title and Year from the title node
            Regex expr = new Regex(@"<title>([^\(]+?)\((\d{4})[\/IVX]*\).*?</title>", RegexOptions.IgnoreCase);
            Match details = expr.Match(detailsPage);
            if (details.Success) {
                try {
                    signature.Title = details.Groups[1].Value;
                    signature.Year = int.Parse(details.Groups[2].Value);
                    logger.Debug("Lookup Imdbid={0}: Title= '{1}', Year= {2}", signature.ImdbId, details.Groups[1], details.Groups[2]);
                }
                catch (Exception e) {
                    logger.Error("Error while parsing IMDB details for '{0}': {1}", signature.ImdbId, e.Message);
                }
            }
            else {
                logger.Debug("Lookup failed for Imdbid={0}", signature.ImdbId);
            }

            return signature;
        }

        /// <summary>
        /// Prefetches some IMDB details like title and year to assist other data providers
        /// </summary>
        /// <remarks>
        /// We might have to replace/link this to the data provider for IMDB so we don't have redundant logic
        /// </remarks>
        /// <param name="ImdbId"></param>
        /// <returns></returns>
        private static string getImdbDetailsPage(string ImdbId) {
            String sData = string.Empty;
            string url = "http://www.imdb.com/title/" + ImdbId;

            int tryCount = 0;
            int maxRetries = 3;
            int timeout = 5000;
            int timeoutIncrement = 1000;

            while (sData == string.Empty) {
                try {
                    // builds the request and retrieves the respones from the url
                    tryCount++;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Timeout = timeout + (timeoutIncrement * tryCount);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    // converts the resulting stream to a string for easier use
                    Stream resultData = response.GetResponseStream();
                    StreamReader reader = new StreamReader(resultData, Encoding.UTF8, true);
                    sData = reader.ReadToEnd().Replace('\0', ' ');
                    sData = HttpUtility.HtmlDecode(sData);
                    resultData.Close();
                    reader.Close();
                    response.Close();
                }
                catch (WebException e) {
                    if (tryCount == maxRetries) {
                        logger.ErrorException("Error connecting to imdb.com Reached retry limit of " + maxRetries, e);
                        return null;
                    }
                }
            }

            return sData;
        }


    }
}
