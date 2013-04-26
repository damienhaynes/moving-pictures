using System;
using System.Collections.Generic;
using System.IO;
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

        public string File {
            get { return file; }
        } protected String file;

        public int MaxRetries {
            get { return maxRetries; }
        } protected int maxRetries;

        public Encoding Encoding {
            get { return encoding; }
        } protected Encoding encoding = null;

        public String UserAgent {
            get { return userAgent; }
        } protected String userAgent = null;

        public int Timeout {
            get { return timeout; }
        } protected int timeout;

        public int TimeoutIncrement {
            get { return timeoutIncrement; }
        } protected int timeoutIncrement;

        public bool AllowUnsafeHeader {
            get { return allowUnsafeHeader; }
        } protected bool allowUnsafeHeader;

        public bool UseCaching {
            get { return _useCaching; }
        } protected bool _useCaching;

        public string Cookies {
            get { return cookies; }
        } protected string cookies = null;

        public string Method {
            get { return _method; }
        } protected string _method;


        #endregion

        #region Methods

        public RetrieveNode(XmlNode xmlNode,  ScriptableScraper context)
            : base(xmlNode, context) {

            // Set default attribute valuess
            _useCaching = true;
            allowUnsafeHeader = false;
            maxRetries = 5;
            timeout = 5000;
            timeoutIncrement = 2000;
            _method = "GET";

            // Load attributes
            foreach (XmlAttribute attr in xmlNode.Attributes) {
                switch (attr.Name) {
                    case "url":
                        url = attr.Value;
                        break;
                    case "file":
                        file = attr.Value;
                        break;
                    case "useragent":
                        userAgent = attr.Value;
                        break;
                    case "allow_unsafe_header":
                        try { allowUnsafeHeader = bool.Parse(attr.Value); }
                        catch (Exception e) {
                            if (e.GetType() == typeof(ThreadAbortException))
                                throw e;
                        }
                        break;
                    case "use_caching":
                        try { _useCaching = bool.Parse(attr.Value); }
                        catch (Exception e) {
                            if (e.GetType() == typeof(ThreadAbortException))
                                throw e;
                        }
                        break;
                    case "encoding":
                        // grab encoding, if not specified it will try to set 
                        // the encoding using information from the response header.
                        try { encoding = Encoding.GetEncoding(attr.Value); }
                        catch (Exception e) {
                            if (e.GetType() == typeof(ThreadAbortException))
                                throw e;
                        }
                        break;
                    case "retries":
                        try { maxRetries = int.Parse(attr.Value); }
                        catch (Exception e) {
                            if (e.GetType() == typeof(ThreadAbortException))
                                throw e;
                        }
                        break;
                    case "timeout":
                        try { timeout = int.Parse(attr.Value); }
                        catch (Exception e) {
                            if (e.GetType() == typeof(ThreadAbortException))
                                throw e;
                        }
                        break;
                    case "timeout_increment":
                        try { timeoutIncrement = int.Parse(attr.Value); }
                        catch (Exception e) {
                            if (e.GetType() == typeof(ThreadAbortException))
                                throw e;                            
                        }
                        break;
                    case "cookies":
                        cookies = attr.Value;
                        break;
                    case "method":
                        _method = attr.Value.Trim().ToUpper();
                        break;

                }
            }

            // Validate URL / FILE attribute
            if (url == null && file == null) {
                logger.Error("Missing URL or FILE attribute on: " + xmlNode.OuterXml);
                loadSuccess = false;
                return;
            }

        }

        public override void Execute(Dictionary<string, string> variables) {
            if (Context.DebugMode) logger.Debug("executing retrieve: " + xmlNode.OuterXml);

            // Check for calling class provided useragent
            if (userAgent == null && variables.ContainsKey("settings.defaultuseragent")) {
                userAgent = variables["settings.defaultuseragent"];
            }

            if (userAgent == null)
                userAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.95 Safari/537.11";

            string parsedName = parseString(variables, name);
            string stringData = string.Empty;

            if (url != null)
                stringData = RetrieveUrl(variables);
            else
                stringData = ReadFile(variables);

            // Set variable
            if (stringData != null) {
                setVariable(variables, parsedName, stringData);
            }
        }

        // Retrieves an URL
        private string RetrieveUrl(Dictionary<string, string> variables) {
            string parsedUrl = parseString(variables, url);
            string parsedUserAgent = parseString(variables, userAgent);
            string pageContents = string.Empty;

            if (_useCaching && Context.Cache.ContainsKey(parsedUrl)) {
                logger.Debug("Using Cached Version of URL: {0}", parsedUrl);
                return Context.Cache[parsedUrl];
            }

            if (Context.DebugMode) logger.Debug("Retrieving URL: {0}", parsedUrl);

            // Try to grab the document
            try {
                WebGrabber grabber = new WebGrabber(parsedUrl);
                grabber.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                grabber.UserAgent = parsedUserAgent;
                grabber.Encoding = encoding;
                grabber.Timeout = timeout;
                grabber.TimeoutIncrement = timeoutIncrement;
                grabber.MaxRetries = maxRetries;
                grabber.AllowUnsafeHeader = allowUnsafeHeader;
                grabber.CookieHeader = cookies;
                
                grabber.Debug = Context.DebugMode;


                // Keep session / chaining
                string sessionKey = "urn://scraper/header/" + grabber.Request.RequestUri.Host;
                if (variables.ContainsKey(sessionKey)) {
                    if (grabber.CookieHeader == null)
                        grabber.CookieHeader = variables[sessionKey];
                    else
                        grabber.CookieHeader = grabber.CookieHeader + "," + variables[sessionKey];
                }


                // Retrieve the document
                if (grabber.GetResponse()) {
                    // save cookie session data for future requests
                    setVariable(variables, sessionKey, grabber.CookieHeader);
                    
                    // grab the request results and store in our cache for later retrievals
                    pageContents = grabber.GetString();
                    if (_useCaching) Context.Cache[parsedUrl] = pageContents;
                }
            }
            catch (Exception e) {
                if (e is ThreadAbortException)
                    throw e;

                logger.Warn("Could not connect to " + parsedUrl + ". " + e.Message);
            }

            return pageContents;
        }

        // Reads a file
        private string ReadFile(Dictionary<string, string> variables) {
            string parsedFile = parseString(variables, file);
            string fileContents = string.Empty;

            if (System.IO.File.Exists(parsedFile)) {

                if (Context.DebugMode) logger.Debug("Reading file: {0}", parsedFile);

                try {
                    StreamReader streamReader;
                    if (encoding != null) streamReader = new StreamReader(parsedFile, encoding);
                    else streamReader = new StreamReader(parsedFile);

                    fileContents = streamReader.ReadToEnd();
                    streamReader.Close();
                }
                catch (Exception e) {
                    if (e is ThreadAbortException)
                        throw e;

                    logger.Warn("Could not read file: " + parsedFile, e);
                }
            }
            else {
                if (Context.DebugMode) logger.Debug("File does not exist: {0}", parsedFile);
            }

            return fileContents;
        }

        

        #endregion
    }
}
