using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;

namespace Cornerstone.ScraperEngine.Nodes {
    [ScraperNode("retrieve")]
    public class RetrieveNode : ScraperNode{
        #region Properties

        public string Url {
            get { return url; }
        } protected String url;

        public int MaxRetries {
            get { return maxRetries; }
        } protected int maxRetries;

        public int Timeout {
            get { return timeout; }
        } protected int timeout;

        public int TimeoutIncrement {
            get { return timeoutIncrement; }
        } protected int timeoutIncrement;

        #endregion

        #region Methods

        public RetrieveNode(XmlNode xmlNode, bool debugMode)
            : base(xmlNode, debugMode) {

            // try to grab the url
            try { url = xmlNode.Attributes["url"].Value; }
            catch (Exception) {
                logger.Error("Missing URL attribute on: " + xmlNode.OuterXml);
                loadSuccess = false;
                return;
            }

            // grab timeout and retry values. if none specified use defaults
            try { maxRetries = int.Parse(xmlNode.Attributes["retries"].Value); }
            catch (Exception) {
                maxRetries = 5;
            }

            try { timeout = int.Parse(xmlNode.Attributes["timeout"].Value); }
            catch (Exception) {
                timeout = 5000;
            }

            try { timeoutIncrement = int.Parse(xmlNode.Attributes["timeout_increment"].Value); }
            catch (Exception) {
                timeoutIncrement = 2000;
            }
        }

        public override void Execute(Dictionary<string, string> variables) {
            logger.Debug("executing retrieve: " + xmlNode.OuterXml);

            string parsedUrl = parseString(variables, url);
            string parsedName = parseString(variables, name);
            string pageContents = string.Empty;

            if (DebugMode) logger.Debug("Retrieving URL: " + parsedUrl);

            // start tryng to retrieve the document
            int tryCount = 0;
            while (pageContents == string.Empty) {
                try {
                    // builds the request and retrieves the respones from movie-xml.com
                    tryCount++;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(parsedUrl);
                    request.Timeout = timeout + (timeoutIncrement * tryCount);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    // converts the resulting stream to a string for easier use
                    Stream resultData = response.GetResponseStream();
                    StreamReader reader = new StreamReader(resultData, Encoding.Default, true);
                    pageContents = reader.ReadToEnd().Replace('\0', ' ');

                    resultData.Close();
                    reader.Close();
                    response.Close();

                    setVariable(variables, parsedName, pageContents);
                }
                catch (WebException e) {
                    if (tryCount == maxRetries) {
                        logger.ErrorException("Error connecting to URL. Reached retry limit of " + maxRetries + ". " + parsedUrl, e);
                        return;
                    }
                }
            }
        }

        #endregion
    }
}
