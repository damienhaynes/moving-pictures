using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using CookComputing.XmlRpc;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.DataProviders.OpenSubtitles {

    /// <summary>
    /// XML-RPC Interface for the OpenSubtitles API
    /// </summary>
    [XmlRpcUrl("http://www.opensubtitles.org/xml-rpc")]
    public interface IOpenSubtitles : IXmlRpcProxy {

        [XmlRpcMethod]
        LoginResponse LogIn(string username, string password, string language, string useragent);

        [XmlRpcMethod]
        void LogOut(string token);

        [XmlRpcMethod]
        BaseResponse NoOperation(string token);

        [XmlRpcMethod]
        HashResponse CheckMovieHash2(string token, string[] movieHashes);

        [XmlRpcMethod]
        SearchResponse SearchMoviesOnIMDB(string token, string query);

        [XmlRpcMethod]
        MovieDetailsResponse GetIMDBMovieDetails(string token, string query);

    }

    /// <summary>
    /// Contains all status codes returned by the API
    /// </summary>
    public enum ResponseStatus {
        OK = 200,
        PartialContent = 206,
        Unauthorized = 401,
        InvalidSubtitleFormat = 402,
        InvalidSubHash = 403,
        InvalidSubtitleLanguage = 404,
        MissingMandatoryParameters = 405,
        NoSession = 406,
        DownloadLimitReached = 407,
        InvalidParameters = 408,
        MethodNotFound = 409,
        UnknownError = 410,
        InvalidUseragent = 411,
        InvalidFormat = 412,
        InvalidImdbID = 413
    }

    /// <summary>
    /// Base for XML-RPC Response
    /// </summary>
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class BaseResponse {
        public string status;
        public double seconds;

        /// <summary>
        /// 
        /// </summary>
        public bool IsOK {
            get {
                if (GetStatus() == ResponseStatus.OK)
                    return true;
                else
                    return false;
            }
        }

        public ResponseStatus GetStatus() {
            int statusCode = int.Parse(status.Substring(0, 3));
            return (ResponseStatus)Enum.ToObject(typeof(ResponseStatus), statusCode);
        }
    }

    /// <summary>
    /// XML-RPC Token Response
    /// </summary>
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class LoginResponse : BaseResponse {
        public string token;
    }

    /// <summary>
    /// XML-RPC CheckHash Reponse
    /// </summary>
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class HashResponse : BaseResponse {
        [XmlRpcMember("data")]
        public object results;
    }

    /// <summary>
    /// Movie details for Hash Response
    /// </summary>
    public struct HashDetails {
        public string Title;
        public int Year;
        public string ImdbID;
        public int SeenCount;
    }

    /// <summary>
    /// XML-RPC Response for SearchMoviesOnIMDB
    /// </summary>
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class SearchResponse : BaseResponse {
        [XmlRpcMember("data")]
        public MovieResult[] results;
    }

    /// <summary>
    /// Data portion of the XML-RPC Response for SearchMoviesOnIMDB
    /// </summary>
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct MovieResult {
        public string id;
        public string title;
    }

    /// <summary>
    /// XML-RPC Response for GetIMDBMovieDetails
    /// </summary>
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class MovieDetailsResponse : BaseResponse {
        [XmlRpcMember("data")]
        public MovieDetails details;
    }

    /// <summary>
    /// Data portion of the XML-RPC Response for GetIMDBMovieDetails
    /// </summary>
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public class MovieDetails {

        #region Mapped XML-RPC Variables

        public string id;
        public string title;
        public string[] aka;
        public string tagline;
        public string plot;
        public string year;
        public string rating;
        public string[] genres;
        public string duration;
        public string[] language;
        public XmlRpcStruct cast;
        public XmlRpcStruct directors;
        public XmlRpcStruct writers;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the ImdbID formatted with the leading 'tt'.
        /// </summary>
        public string ImdbID {
            get {
                if (id != null)
                    return "tt" + id.PadLeft(7, '0');
                else
                    return null;
            }
        }

        /// <summary>
        /// Returns a list of Alternative Titles
        /// </summary>
        public List<string> AlternateTitles {
            get {
                if (_alternateTitles == null) {
                    _alternateTitles = new List<string>();
                    if (aka != null)
                        foreach (string title in aka)
                            _alternateTitles.Add(title);
                }

                return _alternateTitles;
            }
        } private List<string> _alternateTitles;

        /// <summary>
        /// Returns the year as an integer
        /// </summary>
        public int Year {
            get {
                if (_year == 0 && year != null)
                    int.TryParse(year, out _year);

                return _year;
            }
        } private int _year = 0;

        /// <summary>
        /// Returns the duration/runtime as an integer
        /// </summary>
        public int Duration {
            get {
                if (_duration == 0 && duration != null)
                    int.TryParse(duration.Replace("min", "").Trim(), out _duration);

                return _duration;
            }
        } private int _duration = 0;

        #endregion

    }

    /// <summary>
    /// Generic OpenSubtitles API
    /// </summary>
    public class OpenSubtitlesAPI {

        #region Private Variables

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IOpenSubtitles proxy;
        private string userAgent;
        private string _token;

        #endregion

        #region Constructors

        public OpenSubtitlesAPI(string useragent) {
            userAgent = useragent;
            initialize();
        }

        ~OpenSubtitlesAPI() {
            LogOut();
        }

        #endregion

        #region Properties

        public string Username {
            get { return username; }
            set { username = value; }
        } private string username = string.Empty;

        public string Password {
            get { return password; }
            set { password = value; }
        } private string password = string.Empty;

        public string Language {
            get { return language; }
            set { language = value; }
        } private string language = "en";

        public IOpenSubtitles Proxy {
            get { return proxy; }
        }

        public bool LoggedIn {
            get { return (getToken() != null); }
        }

        #endregion

        #region Private Methods

        private void initialize() {

            // Create the proxy with the user agent and set the
            // keep alive property to false
            proxy = XmlRpcProxyGen.Create<IOpenSubtitles>();
            proxy.KeepAlive = false;
            proxy.UserAgent = userAgent;
            logger.Debug("OSDb Proxy created: UserAgent={0}", userAgent);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Login to OSDb
        /// </summary>
        /// <returns>True, if login succeeded</returns>
        public bool Login() {
            LoginResponse response = null;
            int retryCount = 0;

            // Make a connection
            while (true) {
                retryCount++;
                try {
                    // Get the login response
                    response = proxy.LogIn(username, password, language, userAgent);
                    break;
                }
                catch (WebException e) {
                    if (retryCount == 3) {
                        logger.ErrorException("Login Failed: Connection error.", e);
                        return false;
                    }
                    else {
                        logger.DebugException("Login() Retry: Status=" + e.Status.ToString(), e);
                        // Sleep for a couple of seconds before retrying
                        Thread.Sleep(2000 + (retryCount * 1000));
                    }
                }
                catch (XmlRpcFaultException e) {
                    logger.Error("XML-RPC Error: {0} {1}", e.FaultCode, e.FaultString);
                    return false;
                }
            }

            // Process the login response
            if (response != null) {
                if (response.IsOK) {
                    if (response.token != null) {
                        _token = response.token;
                        logger.Info("Login Success: Username={0}, Language={1}, Token={2} ", username, language, _token);
                        return true;
                    }
                }
            }
            logger.Error("Login Failed: Username={1}, Language={2}", username, language);
            return false;
        }

        /// <summary>
        /// Logout from OSDb
        /// </summary>
        public void LogOut() {
            int retryCount = 0;
            string token = getToken();
            if (token != null) {
                while (true) {
                    retryCount++;
                    try {
                        proxy.LogOut(token);
                        _token = null;
                        return;
                    }
                    catch (WebException e) {
                        if (retryCount == 3) {
                            logger.ErrorException("Login Failed: Connection error.", e);
                            _token = null;
                            return;
                        }
                        else {
                            logger.DebugException("LogOut() Retry: Status=" + e.Status.ToString(), e);
                            // Sleep for a couple of seconds before retrying
                            Thread.Sleep(2000 + (retryCount * 1000));
                        }
                    }
                    catch (XmlRpcFaultException e) {
                        logger.Error("XML-RPC Error: {0} {1}", e.FaultCode, e.FaultString);
                        _token = null;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Wrapper for XML-RPC method CheckMovieHash2, token is handled internally.
        /// </summary>
        /// <param name="hashArray">array with file hashes to match</param>
        /// <returns></returns>
        public HashResponse CheckMovieHash2(string[] hashArray) {
            HashResponse response = null;
            string token = getToken();
            if (token != null) {
                int retryCount = 0;
                while (true) {
                    retryCount++;
                    try {
                        response = proxy.CheckMovieHash2(token, hashArray);
                        break;
                    }
                    catch (WebException e) {
                        if (retryCount == 3) {
                            logger.ErrorException("CheckMovieHash2() Failed: Connection error.", e);
                            break;
                        }
                        else {
                            logger.DebugException("CheckMovieHash2() Retry: Status=" + e.Status.ToString(), e);
                            // Sleep for a couple of seconds before retrying
                            Thread.Sleep(2000 + (retryCount * 1000));
                        }
                    }
                    catch (XmlRpcFaultException e) {
                        logger.Error("XML-RPC Error: {0} {1}", e.FaultCode, e.FaultString);
                        break;
                    }
                }
            }
            return response;
        }

        /// <summary>
        /// Wrapper for XML-RPC method SearchMoviesOnIMDB, token is handled internally.
        /// </summary>
        /// <param name="query">keyword/title</param>
        /// <returns></returns>
        public SearchResponse SearchMoviesOnIMDB(string query) {
            SearchResponse response = new SearchResponse();
            string token = getToken();
            int retryCount = 0;
            if (token != null) {
                while (true) {
                    retryCount++;
                    try {
                        response = proxy.SearchMoviesOnIMDB(token, query);
                        break;
                    }
                    catch (WebException e) {
                        if (retryCount == 3) {
                            logger.ErrorException("SearchMoviesOnIMDB() Failed: Connection error.", e);
                            break;
                        }
                        else {
                            logger.DebugException("SearchMoviesOnIMDB() Retry: Status=" + e.Status.ToString(), e);
                            Thread.Sleep(2000 + (retryCount * 1000));
                        }
                    }
                    catch (XmlRpcFaultException e) {
                        logger.Error("XML-RPC Error: {0} {1}", e.FaultCode, e.FaultString);
                        break;
                    }
                }
            }
            return response;
        }

        /// <summary>
        /// Wrapper for XML-RPC method GetIMDBMovieDetails, token is handled internally.
        /// </summary>
        /// <param name="imdbid">imdbid (with or without the tt)</param>
        /// <returns></returns>
        public MovieDetailsResponse GetIMDBMovieDetails(string imdbid) {
            MovieDetailsResponse response = new MovieDetailsResponse();
            string token = getToken();
            int retryCount = 0;
            if (token != null) {
                while (true) {
                    retryCount++;
                    try {
                        response = proxy.GetIMDBMovieDetails(token, imdbid.Replace("tt", ""));
                        break;
                    }
                    catch (WebException e) {
                        if (retryCount == 3) {
                            logger.ErrorException("GetIMDBMovieDetails() Failed: Connection error.", e);
                            break;
                        }
                        else {
                            logger.DebugException("GetIMDBMovieDetails() Retry: Status=" + e.Status.ToString(), e);
                            // Sleep for a couple of seconds before retrying
                            Thread.Sleep(2000 + (retryCount * 1000));
                        }
                    }
                    catch (XmlRpcFaultException e) {
                        logger.Error("XML-RPC Error: {0} {1}", e.FaultCode, e.FaultString);
                        break;
                    }
                }
            }
            return response;
        }

        private string getToken() {
            lock (proxy) {
                if (_token == null)
                    Login();

                return _token;
            }
        }

        #endregion

    }

}
