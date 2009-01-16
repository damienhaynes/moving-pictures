using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Cornerstone.Tools;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.SignatureBuilders {
    class MetaServicesBuilder : ISignatureBuilder {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public MovieSignature UpdateSignature(MovieSignature signature) {
            if (signature.DiscId != null && signature.DiscId != "0") {
                XmlNodeList mdrDVD = getMDRDVDByCRC(signature.DiscId);
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
            }
            return signature;
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

        private static XmlNodeList getMDRDVDByCRC(string DiscID) {
            WebGrabber grabber = new WebGrabber("http://movie.metaservices.microsoft.com/pas_movie_B/template/GetMDRDVDByCRC.xml?CRC=" + DiscID);
            grabber.Encoding = Encoding.UTF8;
            if (grabber.GetResponse())
                return grabber.GetXML("METADATA");
            else
                return null;
        }

    }
}
