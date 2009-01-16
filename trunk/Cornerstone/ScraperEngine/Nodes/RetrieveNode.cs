using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using Cornerstone.Tools;

namespace Cornerstone.ScraperEngine.Nodes {
    [ScraperNode("retrieve")]
    public class RetrieveNode : ScraperNode {
        #region Properties

        public string Url {
            get { return url; }
        } protected String url;

        public int MaxRetries {
            get { return maxRetries; }
        } protected int maxRetries;

        public Encoding Encoding {
            get { return encoding; }
        } protected Encoding encoding;

        public String UserAgent {
            get { return userAgent; }
        } protected String userAgent;

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
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;

                logger.Error("Missing URL attribute on: " + xmlNode.OuterXml);
                loadSuccess = false;
                return;
            }

            // grab user agent. if none specified use defaults
            try { userAgent = xmlNode.Attributes["useragent"].Value; }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;

                userAgent = "Mozilla/5.0 (Windows; U; MSIE 7.0; Windows NT 6.0; en-US)";
            }

            // grab encoding, if not specified it will try to set 
            // the encoding using information from the response header.
            try {
                encoding = Encoding.GetEncoding(xmlNode.Attributes["encoding"].Value);
            }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;
            }

            // grab timeout and retry values. if none specified use defaults
            try { maxRetries = int.Parse(xmlNode.Attributes["retries"].Value); }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;

                maxRetries = 5;
            }

            try { timeout = int.Parse(xmlNode.Attributes["timeout"].Value); }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;

                timeout = 5000;
            }

            try { timeoutIncrement = int.Parse(xmlNode.Attributes["timeout_increment"].Value); }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;

                timeoutIncrement = 2000;
            }
        }

        public override void Execute(Dictionary<string, string> variables) {
            if (DebugMode) logger.Debug("executing retrieve: " + xmlNode.OuterXml);

            string parsedUrl = parseString(variables, url);
            string parsedName = parseString(variables, name);
            string pageContents = string.Empty;

            if (DebugMode) logger.Debug("Retrieving URL: {0}", parsedUrl);

            // Try to grab the document
            try {
                WebGrabber grabber = new WebGrabber(parsedUrl);
                //grabber.Request.Accept = "text/xml,application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5";
                grabber.UserAgent = userAgent;
                grabber.Encoding = encoding;
                grabber.Timeout = timeout;
                grabber.TimeoutIncrement = timeoutIncrement;
                grabber.MaxRetries = maxRetries;
                grabber.Debug = DebugMode;

                // Keep session / chaining
                string sessionKey = "urn://scraper/header/" + grabber.Request.RequestUri.Host;
                if (variables.ContainsKey(sessionKey))
                    grabber.CookieHeader = variables[sessionKey];

                // Retrieve the document
                if (grabber.GetResponse()) {
                    // save the current session
                    setVariable(variables, sessionKey, grabber.CookieHeader);
                    // save the contents of the page
                    pageContents = grabber.GetString();
                }
                setVariable(variables, parsedName, pageContents);
            }
            catch (Exception e) {
                if (e is ThreadAbortException)
                    throw e;

                logger.Warn("Could not connect to " + parsedUrl, e);
            }
        }

        #endregion
    }
}
