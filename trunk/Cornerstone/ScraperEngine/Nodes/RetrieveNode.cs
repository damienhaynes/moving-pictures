using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;
using System.Threading;

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

            // start trying to retrieve the document
            int tryCount = 0;
            try {
                while (pageContents == string.Empty) {
                    try {
                        // builds the request and retrieves the responses from movie-xml.com
                        tryCount++;
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(parsedUrl);
                        request.UserAgent = userAgent;
                        request.CookieContainer = new CookieContainer();
                        request.Timeout = timeout + (timeoutIncrement * tryCount);
                        request.Accept = "text/xml,application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5";

                        // if we already had a session for this host name persist it
                        string sessionKey = "urn://scraper/header/" + request.RequestUri.Host;
                        if (variables.ContainsKey(sessionKey))
                            request.CookieContainer.SetCookies(request.RequestUri, variables[sessionKey]);

                        // get the response
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        // get the cookie header
                        string cookieHeader = request.CookieContainer.GetCookieHeader(request.RequestUri);
                        // save the current session to a variable using the hostname as an identifier
                        setVariable(variables, sessionKey, cookieHeader);

                        // converts the resulting stream to a string for easier use
                        Stream resultData = response.GetResponseStream();
                        
                        // use the proper encoding
                        if (encoding == null)
                            encoding = Encoding.GetEncoding(response.CharacterSet);

                        // Log some debug values
                        if (DebugMode) {
                            logger.Debug("UserAgent: {0}", userAgent);
                            logger.Debug("CookieHeader: {0}", cookieHeader);
                            logger.Debug("Encoding: {0}", encoding.EncodingName);
                        }

                        StreamReader reader = new StreamReader(resultData, encoding, true);
                        pageContents = reader.ReadToEnd();

                        resultData.Close();
                        reader.Close();
                        response.Close();

                        setVariable(variables, parsedName, pageContents);
                    }

                    catch (WebException e) {
                        // Don't retry on protocol errors
                        if (e.Status == WebExceptionStatus.ProtocolError) {
                            logger.Error("Error connecting to: URL={0}, Status={1}, Description={2}.", parsedUrl, ((HttpWebResponse)e.Response).StatusCode, ((HttpWebResponse)e.Response).StatusDescription);
                            return;
                        }
                        // Return when hitting maximum retries.
                        if (tryCount == maxRetries) {
                            logger.ErrorException("Error connecting to URL. Reached retry limit of " + maxRetries + ". " + parsedUrl, e);
                            return;
                        }
                    }
                }
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
