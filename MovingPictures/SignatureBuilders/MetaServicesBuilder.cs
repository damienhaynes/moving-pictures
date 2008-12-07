using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
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
            String sXmlData = string.Empty;
            string url = "http://movie.metaservices.microsoft.com/pas_movie_B/template/GetMDRDVDByCRC.xml?CRC=" + DiscID;

            int tryCount = 0;
            int maxRetries = 3;
            int timeout = 5000;
            int timeoutIncrement = 1000;

            while (sXmlData == string.Empty) {
                try {
                    // builds the request and retrieves the respones from the url
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
                        logger.ErrorException("Error connecting to metaservices.microsoft.com. Reached retry limit of " + maxRetries, e);
                        return null;
                    }
                }
            }

            try {
                // attempts to convert the returned string into an XmlDocument
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(sXmlData);
                XmlNode root = doc.FirstChild.NextSibling;
                if (root.Name == "METADATA")
                    return root.ChildNodes;

            }
            catch (XmlException e) {
                logger.ErrorException("Error while trying to convert metadata to XMLDocument.", e);
            }

            return null;
        }

    }
}
