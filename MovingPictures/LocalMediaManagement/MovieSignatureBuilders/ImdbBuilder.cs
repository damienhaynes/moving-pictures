using System;
using System.Text.RegularExpressions;
using System.Web;
using Cornerstone.Tools;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.SignatureBuilders {
    
    /// <summary>
    /// The IMDB Builder looks up details from IMDB.com when a signature contains an imdbid.
    /// note: signature property values get overwritten
    /// </summary>
    class ImdbBuilder: ISignatureBuilder {

        #region Variables

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region ISignatureBuilder Members

        public SignatureBuilderResult UpdateSignature(MovieSignature signature) {
            bool imdbLookup = (bool)MovingPicturesCore.SettingsManager["importer_lookup_imdb"].Value;

            // If there's no ImdbId in the signature return the signature immediatly
            if (!imdbLookup | String.IsNullOrEmpty(signature.ImdbId))
                return SignatureBuilderResult.INCONCLUSIVE;

            // Try to retrieve the IMDB details page
            string detailsPage = getImdbDetailsPage(signature.ImdbId);
            
            // if we don't have a details page then return the signature
            if (String.IsNullOrEmpty(detailsPage))
                return SignatureBuilderResult.INCONCLUSIVE;

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
                    logger.Error("Lookup Imdbid={0}: Failed. {1}", signature.ImdbId, e.Message);
                    return SignatureBuilderResult.INCONCLUSIVE;
                }
            }
            else {
                logger.Debug("Lookup Imdbid={0}: Failed.", signature.ImdbId);
                return SignatureBuilderResult.INCONCLUSIVE;
            }

            return SignatureBuilderResult.CONCLUSIVE;
        }

        #endregion

        #region Static Methods

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

        #endregion
        
    }
}
