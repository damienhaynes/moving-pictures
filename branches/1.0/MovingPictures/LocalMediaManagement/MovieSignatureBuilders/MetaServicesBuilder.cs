using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Cornerstone.Tools;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.SignatureBuilders {
    class MetaServicesBuilder : ISignatureBuilder {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static string urlWindowsMetaServicesQueryDiscId = "http://metaservices.windowsmedia.com/dvdinfopages/querycrcextendedMCE.aspx?DVDID=";
        public SignatureBuilderResult UpdateSignature(MovieSignature signature) {
            if (MovingPicturesCore.Settings.EnableDiscIdLookup || signature.DiscId == null)
                return SignatureBuilderResult.INCONCLUSIVE;

            XmlNodeList mdrDVD = GetMovieMetaData(signature.DiscId);
            if (mdrDVD != null) {
                // todo: include more information?
                XmlNode nodeTitle = mdrDVD.Item(0).SelectSingleNode("dvdTitle");
                if (nodeTitle != null) {
                    string title = removeSuffix(nodeTitle.InnerText);
                    logger.Debug("Lookup DiscId={0}: Title= '{1}'", signature.DiscId, title);
                    signature.Title = removeSuffix(title);
                }
                else {
                    logger.Debug("Lookup DiscId={0}: no data available", signature.DiscId);
                }
            }

            return SignatureBuilderResult.INCONCLUSIVE;
        }

        /// <summary>
        /// Cleans the edition suffix from the movie title from the DVD metadata title
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private static string removeSuffix(string title) {
            Regex expr = new Regex(@"\[.+?\]", RegexOptions.IgnoreCase);
            return expr.Replace(title, "").Trim();
        }

        /// <summary>
        /// Grabs the movie meta data from the Windows Meta Services webservice
        /// using the DiscID
        /// </summary>
        /// <param name="DiscID"></param>
        /// <returns>Metadata in XML format</returns>
        private static XmlNodeList GetMovieMetaData(string DiscID) {
            WebGrabber grabber = new WebGrabber(urlWindowsMetaServicesQueryDiscId + DiscID);
            grabber.Encoding = Encoding.UTF8;
            if (grabber.GetResponse())
                return grabber.GetXML("METADATA");
            else
                return null;
        }

    }
}
