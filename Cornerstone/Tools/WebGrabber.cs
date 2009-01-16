using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using NLog;

namespace Cornerstone.Tools {

    public class WebGrabber {
        
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string requestUrl;
        private string data;

        public WebGrabber(string url) {
            requestUrl = url;
            request = (HttpWebRequest)WebRequest.Create(requestUrl);
        }

        ~WebGrabber() {
            data = null;
            request = null;
            if (response != null) {
                response.Close();
                response = null;
            }
        }

        public HttpWebRequest Request {
            get { return request; }
        } private HttpWebRequest request;

        public HttpWebResponse Response {
            get { return response; }
        } private HttpWebResponse response;

        public Encoding Encoding {
            get { return encoding; }
            set { encoding = value; }            
        } private Encoding encoding;

        public int MaxRetries {
            get { return maxRetries; }
            set { maxRetries = value; }
        } private int maxRetries = 3;

        public int Timeout {
            get { return timeout; }
            set { timeout = value; }
        } private int timeout = 5000;

        public int TimeoutIncrement {
            get { return timeoutIncrement; }
            set { timeoutIncrement = value; }
        } private int timeoutIncrement = 1000;

        public string UserAgent {
            get { return userAgent; }
            set { userAgent = value; }
        } private string userAgent = "Cornerstone/" + Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public string CookieHeader {
            get { return cookieHeader; }
            set { cookieHeader = value; }
        } private string cookieHeader;

        public bool Debug {
            get { return _debug; }
            set { _debug = value; }
        } private bool _debug = false;

        public bool GetResponse() {
            data = string.Empty;
            int tryCount = 0;
            while (data == string.Empty) {
                try {
                    request.UserAgent = userAgent;
                    request.Timeout = timeout + (timeoutIncrement * tryCount);
                    request.CookieContainer = new CookieContainer();
                    if (cookieHeader != null)
                        request.CookieContainer.SetCookies(request.RequestUri, cookieHeader);
                    response = (HttpWebResponse)request.GetResponse();
                    cookieHeader = request.CookieContainer.GetCookieHeader(request.RequestUri);

                    // converts the resulting stream to a string for easier use
                    Stream resultData = response.GetResponseStream();

                    // use the proper encoding
                    if (encoding == null)
                        encoding = Encoding.GetEncoding(response.CharacterSet);

                    // Debug
                    if (_debug) {
                        logger.Debug("URL: {0}", requestUrl);
                        logger.Debug("UserAgent: {0}", userAgent);
                        logger.Debug("CookieHeader: {0}", cookieHeader);
                        logger.Debug("Encoding: {0}", encoding.EncodingName);
                    }
                    
                    // Read to string
                    StreamReader reader = new StreamReader(resultData, encoding, true);
                    data = reader.ReadToEnd(); 

                    // Close stream and response objects
                    resultData.Close();
                    reader.Close();
                    response.Close();                
                }
                catch (WebException e) {
                    // Don't retry on protocol errors
                    if (e.Status == WebExceptionStatus.ProtocolError) {
                        logger.Error("Connection failed: URL={0}, Status={1}, Description={2}.", requestUrl, ((HttpWebResponse)e.Response).StatusCode, ((HttpWebResponse)e.Response).StatusDescription);
                        return false;
                    }
                    
                    // Return when hitting maximum retries.
                    if (tryCount == maxRetries) {
                        logger.ErrorException("Connection failed: Reached retry limit of " + maxRetries + ". URL=" + requestUrl, e);
                        return false;
                    }
                }
            }
            return true;
        }

        public string GetString() {
            return data;
        }

        public XmlNodeList GetXML() {
            return GetXML(null);
        }

        public XmlNodeList GetXML(string rootNode) {
            // if there's no data return nothing
            if (String.IsNullOrEmpty(data))
                return null;

            // attempts to convert data into an XmlDocument
            try {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(data);
                XmlNode xmlRoot = xml.FirstChild.NextSibling;

                // if a root node name is given check for it
                // return null when the root name doesn't match
                if (rootNode != null)
                    if (xmlRoot.Name != rootNode)
                        return null;

                // return the node list
                return xmlRoot.ChildNodes;
            }
            catch (XmlException e) {
                logger.ErrorException("XML Parse error: URL=" + requestUrl, e);
                return null;
            }          
        }
    }
}
