using System;
using System.Text.RegularExpressions;
using System.Web;
using Cornerstone.Tools;
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
                    logger.Debug("Lookup Imdbid={0}: Title='{1}', Year={2}", signature.ImdbId, signature.Title, signature.Year);
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
            WebGrabber grabber = new WebGrabber("http://www.imdb.com/title/" + ImdbId);
            if (grabber.GetResponse())
                return HttpUtility.HtmlDecode(grabber.GetString());
            else
                return null;            
        }


    }
}
