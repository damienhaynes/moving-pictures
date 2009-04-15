using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Reflection;
using NLog;
using System.Threading;

namespace Cornerstone.Tools {

    public class WebGrabber {
        
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static int unsafeHeaderUserCount;
        private static object lockingObj;

        private string requestUrl;
        private string data;

        static WebGrabber() {
            unsafeHeaderUserCount = 0;
            lockingObj = new object();
        }

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

        public bool AllowUnsafeHeader
        {
            get { return _allowUnsafeHeader; }
            set { _allowUnsafeHeader = value; }
        } private bool _allowUnsafeHeader = false;


        public bool GetResponse() {
            data = string.Empty;
            int tryCount = 0;

            while (data == string.Empty) {
                tryCount++;
                try {
                    if (_allowUnsafeHeader) 
                        SetAllowUnsafeHeaderParsing(true);

                    request.UserAgent = userAgent;
                    request.Timeout = timeout + (timeoutIncrement * tryCount);
                    request.CookieContainer = new CookieContainer();
                    if (cookieHeader != null)
                        request.CookieContainer.SetCookies(request.RequestUri, cookieHeader);
                    response = (HttpWebResponse)request.GetResponse();
                    cookieHeader = request.CookieContainer.GetCookieHeader(request.RequestUri);

                    // Get result as stream
                    Stream resultData = response.GetResponseStream();

                    // If encoding was not set manually try to detect it
                    if (encoding == null) {
                        try {
                            // Try to get the encoding using the characterset
                            encoding = Encoding.GetEncoding(response.CharacterSet);
                        }
                        catch (Exception e) {
                            // If this fails default to the system's default encoding
                            logger.DebugException("Encoding could not be determined, using default.", e);
                            encoding = Encoding.Default;
                        }
                    }

                    // Debug
                    if (_debug)
                        logger.Debug("GetResponse: URL={0}, UserAgent={1}, Encoding={2}, CookieHeader={3}",
                            requestUrl, userAgent, encoding.EncodingName, cookieHeader);

                    // Converts the stream to a string
                    try {
                        StreamReader reader = new StreamReader(resultData, encoding, true);
                        data = reader.ReadToEnd();
                        reader.Close();
                    }
                    catch (Exception e) {
                        if (e.GetType() == typeof(ThreadAbortException))
                            throw e;

                        logger.DebugException("Error while trying to read stream data: ", e);
                    }

                    // Close stream and response objects
                    resultData.Close();
                    response.Close();
                }
                catch (WebException e) {

                    // Skip retry logic on protocol errors
                    if (e.Status == WebExceptionStatus.ProtocolError) {
                        HttpStatusCode statusCode = ((HttpWebResponse)e.Response).StatusCode;
                        switch (statusCode) {
                            // Currently the only exception is the service temporarily unavailable status
                            // So keep retrying when this is the case
                            case HttpStatusCode.ServiceUnavailable:
                                break;
                            // all other status codes mostly indicate problems that won't be
                            // solved within the retry period so fail these immediatly
                            default:
                                logger.Error("Connection failed: URL={0}, Status={1}, Description={2}.", requestUrl, statusCode, ((HttpWebResponse)e.Response).StatusDescription);
                                return false;
                        }
                    }

                    // Return when hitting maximum retries.
                    if (tryCount == maxRetries) {
                        logger.ErrorException("Connection failed: Reached retry limit of " + maxRetries + ". URL=" + requestUrl, e);
                        return false;
                    }
                    else {
                        logger.DebugException("Connection retry (" + tryCount.ToString() + "): URL=" + requestUrl + ", Status=" + e.Status.ToString() + ". ", e);
                    }               

                    // If we did not experience a timeout but some other error
                    // use the timeout value as a pause between retries
                    if (e.Status != WebExceptionStatus.Timeout) {
                        Thread.Sleep(timeout + (timeoutIncrement * tryCount));
                    }
                }
                finally { 
                    if (_allowUnsafeHeader) 
                        SetAllowUnsafeHeaderParsing(false); 
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

        //Method to change the AllowUnsafeHeaderParsing property of HttpWebRequest.
        private bool SetAllowUnsafeHeaderParsing(bool setState) {
            try {
                lock (lockingObj) {
                    // update our counter of the number of requests needing 
                    // unsafe header processing
                    if (setState == true) unsafeHeaderUserCount++;
                    else unsafeHeaderUserCount--;

                    // if there was already a request using unsafe heaser processing, we
                    // dont need to take any action.
                    if (unsafeHeaderUserCount > 1)
                        return true;

                    // if the request tried to turn off unsafe header processing but it is
                    // still needed by another request, we should wait.
                    if (unsafeHeaderUserCount >= 1 && setState == false)
                        return true;

                    //Get the assembly that contains the internal class
                    Assembly aNetAssembly = Assembly.GetAssembly(typeof(System.Net.Configuration.SettingsSection));
                    if (aNetAssembly == null)
                        return false;

                    //Use the assembly in order to get the internal type for the internal class
                    Type aSettingsType = aNetAssembly.GetType("System.Net.Configuration.SettingsSectionInternal");
                    if (aSettingsType == null)
                        return false;

                    //Use the internal static property to get an instance of the internal settings class.
                    //If the static instance isn't created allready the property will create it for us.
                    object anInstance = aSettingsType.InvokeMember("Section",
                                                                    BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic,
                                                                    null, null, new object[] { });
                    if (anInstance == null)
                        return false;

                    //Locate the private bool field that tells the framework is unsafe header parsing should be allowed or not
                    FieldInfo aUseUnsafeHeaderParsing = aSettingsType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (aUseUnsafeHeaderParsing == null)
                        return false;

                    // and finally set our setting
                    aUseUnsafeHeaderParsing.SetValue(anInstance, setState);
                    return true;
                }

            }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;

                logger.Error("Unsafe header parsing setting change failed.");
                return false;
            }
        }
    }
}
