using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using NLog;
using System.Xml;

namespace MediaPortal.Plugins.MovingPictures.SignatureProviders {
    class WindowsMetaServices {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static XmlNodeList GetMDRDVDByCRC(string DiscID) {
            String sXmlData = string.Empty;

            int tryCount = 0;
            int maxRetries = 3;
            int timeout = 5000;
            int timeoutIncrement = 1000;

            string url = "http://movie.metaservices.microsoft.com/pas_movie_B/template/GetMDRDVDByCRC.xml?CRC=" + DiscID;
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
