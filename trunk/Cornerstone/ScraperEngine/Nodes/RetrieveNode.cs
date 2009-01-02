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

              userAgent = "Mozilla/5.0 (compatible; MSIE 7.0; Windows NT 5.1)";
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
                        if (DebugMode) logger.Debug("UserAgent: {0}", userAgent);  
                        request.CookieContainer = new CookieContainer();
                        request.Timeout = timeout + (timeoutIncrement * tryCount);

                        // if we already had a session for this host name persist it
                        string sessionKey = "urn://scraper/header/" + request.RequestUri.Host;
                        if (variables.ContainsKey(sessionKey))
                            request.CookieContainer.SetCookies(request.RequestUri, variables[sessionKey]);

                        // get the response
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        
                        // save the current session to a variable using the hostname as an identifier
                        if (DebugMode) logger.Debug("CookieHeader: {0}", request.CookieContainer.GetCookieHeader(request.RequestUri));
                        setVariable(variables, sessionKey, request.CookieContainer.GetCookieHeader(request.RequestUri));

                        // converts the resulting stream to a string for easier use
                        Stream resultData = response.GetResponseStream();
                        
                        // use the proper encoding
                        Encoding encoding = Encoding.UTF8;
                        if (response.ContentType != "text/xml")
                            encoding = Encoding.GetEncoding(response.CharacterSet);

                        StreamReader reader = new StreamReader(resultData, encoding, true);
                        pageContents = reader.ReadToEnd();

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
            catch (Exception e) {
                if (e is ThreadAbortException)
                    throw e;

                logger.Warn("Could not connect to " + parsedUrl, e);
            }
        }

        #endregion
    }
}
