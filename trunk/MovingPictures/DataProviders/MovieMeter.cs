using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using CookComputing.XmlRpc;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.DataProviders.MovieMeter {

    /// <summary>
    /// XML-RPC Interface for the MovieMeter API
    /// http://wiki.moviemeter.nl/index.php/API
    /// </summary>
    [XmlRpcUrl("http://www.moviemeter.nl/ws")]
    public interface IMovieMeter : IXmlRpcProxy {

        /* The following methods are currently (19-06-2009) available:
        
        system.listMethods() 
        system.methodHelp(string method_name) 
        system.methodSignature(string method_name)
        api.startSession(string apikey), returns array with sessionkey and unix timestamp for session's expiration date 
        api.closeSession(string sessionkey), returns boolean 
        film.search(string sessionkey, string search), returns array with films 
        film.retrieveScore(string sessionkey, int filmId), returns array with information about the current score (average, total, amount of votes) 
        film.retrieveImdb(string sessionkey, int filmId), returns array with imdb code, url, score and votes for this film 
        film.retrieveByImdb(string sessionkey, string imdb code), returns filmId corresponding to the imdb code supplied 
        film.retrieveDetails(string sessionkey, int filmId), returns array with information about the film 
        director.search(string sessionkey, string search), returns array with directors 
        director.retrieveDetails(string sessionkey, int directorId), returns array with director's information 
        director.retrieveFilms(string sessionkey, int directorId), returns array with director's films 
         */

        [XmlRpcMethod("api.startSession")]
        Session StartSession(string apikey);

        [XmlRpcMethod("api.closeSession")]
        bool CloseSession(string sessionkey);

        [XmlRpcMethod("film.search")]
        Film[] Search(string sessionkey, string search);

        [XmlRpcMethod("film.retrieveScore")]
        FilmScore RetrieveScore(string sessionkey, int filmId);

        [XmlRpcMethod("film.retrieveImdb")]
        FilmImdb RetrieveImdb(string sessionkey, int filmId);

        [XmlRpcMethod("film.retrieveByImdb")]
        string RetrieveByImdb(string sessionkey, string imdb_code);

        [XmlRpcMethod("film.retrieveDetails")]
        FilmDetail RetrieveDetails(string sessionkey, int filmId);

        [XmlRpcMethod("director.search")]
        Director[] DirectorSearch(string sessionkey, string search);

        [XmlRpcMethod("director.retrieveDetails")]
        DirectorDetail DirectorRetrieveDetails(string sessionkey, int directorId);

        [XmlRpcMethod("director.retrieveFilms")]
        DirectorFilm[] DirectorRetrieveFilms(string sessionkey, int directorId);

    }

    #region XML-RPC Method Results

    public class Session {
        public string session_key;
        public int valid_till;
        public string disclaimer;
    }

    public class Film {
        public string filmId;
        public string url;
        public string title;
        public string alternative_title;
        public string year;
        public string average;
        public string votes_count;
        public string similarity;

    }

    public class FilmScore {
        public string votes;
        public string total;
        public string average;
    }

    public class FilmImdb {
        public string code;
        public string url;
        public string score;
        public int votes;
    }

    public class FilmDetail {
        public string url;
        public string thumbnail;
        public string title;
        public Title[] alternative_titles;
        public string year;
        public string imdb;
        public string plot;
        public string duration;
        public Duration[] durations;
        public Actor[] actors;
        public string actors_text;
        public Director[] directors;
        public string directors_text;
        public Country[] countries;
        public string countries_text;
        public string[] genres;
        public string genres_text;
        public Date[] dates_cinema;
        public Date[] dates_video;
        public string average;
        public string votes_count;
        public int filmId;

        public class Duration {
            public string duration;
            public string description;
        }

        public class Actor {
            public string name;
            public string voice;
        }

        public class Director {
            public string id;
            public string name;
        }

        public class Country {
            public string iso_3166_1;
            public string name;
        }

        public class Date {
            public string date;
        }

        public class Title {
            public string title;
        }
    }

    public class Director {
        public string directorId;
        public string url;
        public string name;
        public string similarity;
    }

    public class DirectorDetail {
        public string url;
        public string thumbnail;
        public string name;
        public string born;
        public string deceased;
    }

    public class DirectorFilm {
        public string filmId;
        public string url;
        public string title;
        public string alternative_title;
        public string year;
    }

    #endregion

    /// <summary>
    /// Simple MovieMeter API with local caching.
    /// </summary>
    public class MovieMeterAPI {

        #region Private variables

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string sessionKey = string.Empty;
        private DateTime sessionExpires;

        private readonly object lockProxy = new object();
        private readonly object lockSession = new object();

        private static readonly object lockCache = new object();
        private static Dictionary<string, string> mmIdLookupCache;
        private static Dictionary<string, FilmDetail> mmFilmDetailCache;

        #endregion

        #region Constructors

        public MovieMeterAPI() {

        }

        public MovieMeterAPI(string apikey) {
            _apiKey = apikey;
        }

        static MovieMeterAPI() {
            lock (lockCache) {
                if (mmIdLookupCache == null)
                    mmIdLookupCache = new Dictionary<string, string>();
                if (mmFilmDetailCache == null)
                    mmFilmDetailCache = new Dictionary<string, FilmDetail>();
            }
        }

        #endregion

        #region Public properties

        public string ApiKey {
            get {
                return _apiKey;
            }
            set {
                _apiKey = value;
            }
        }  private string _apiKey = string.Empty;

        public string UserAgent {
            get {
                return _userAgent;
            }
            set {
                _userAgent = value;
            }
        }  private string _userAgent = string.Empty;

        public int MaxRetries {
            get {
                return _maxRetries;
            }
            set {
                _maxRetries = value;
            }
        }  private int _maxRetries = 3;

        public IMovieMeter Proxy {
            get {
                lock (lockProxy) {
                    if (_proxy == null) {
                        // Create the proxy with the user agent and set the
                        // keep alive property to false
                        try {
                            logger.Debug("Attempting to create proxy...");
                            _proxy = XmlRpcProxyGen.Create<IMovieMeter>();
                            _proxy.KeepAlive = false;
                            _proxy.UserAgent = _userAgent;
                            logger.Debug("Proxy Created: UserAgent={0}, ApiKey={1}", _userAgent, _apiKey);
                        }
                        catch (Exception e) {
                            _proxy = null;
                            logger.DebugException("Proxy creation failed.", e);
                        }
                    }
                }
                return _proxy;
            }
        } private IMovieMeter _proxy;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the MovieMeter ID linked to a IMDB code
        /// </summary>
        /// <param name="imdbId">the numeric part of the IMDB code</param>
        /// <returns>MovieMeter ID</returns>
        public string GetMovieMeterId(string imdbId) {
            int retryCount = 0;

            // try to correct invalid imdb input
            imdbId = Regex.Replace(imdbId, @"[^\d]", "").Trim();

            lock (lockCache) {
                if (mmIdLookupCache.ContainsKey(imdbId))
                    return mmIdLookupCache[imdbId];
            }

            while (true) {
                retryCount++;
                try {
                    string mmid = Proxy.RetrieveByImdb(getSessionKey(), imdbId);
                    lock (lockCache) {
                        if (!mmIdLookupCache.ContainsKey(imdbId))
                            mmIdLookupCache.Add(imdbId, mmid);
                    }

                    return mmid;
                }
                catch (WebException e) { if (!retryAfterWebException(e, retryCount)) break; }
                catch (XmlRpcFaultException e) {
                    logger.Debug("XML-RPC Fault: Code={0}, String='{1}'", e.FaultCode, e.FaultString);
                    break;
                }
                catch (Exception e) { 
                    logger.Error(e); 
                    break;
                }
            }

            return null;
        }

        public Film[] Search(string keywords) {
            if (String.IsNullOrEmpty(keywords))
                return null;
            
            int retryCount = 0;
            while (true) {
                retryCount++;
                try {
                    return Proxy.Search(getSessionKey(), keywords);
                }
                catch (WebException e) { if (!retryAfterWebException(e, retryCount)) break; }
                catch (XmlRpcFaultException e) {
                    logger.Debug("XML-RPC Fault: Code={0}, String='{1}'", e.FaultCode, e.FaultString);
                    break;
                }
                catch (Exception e) {
                    logger.Error(e);
                    break;
                }
            }

            return null;
        }

        public FilmDetail GetMovieDetails(string movieMeterID) {
            lock (lockCache) {
                if (mmFilmDetailCache.ContainsKey(movieMeterID))
                    return mmFilmDetailCache[movieMeterID];
            }

            int retryCount = 0;
            while (true) {
                retryCount++;
                try {
                    FilmDetail details = Proxy.RetrieveDetails(getSessionKey(), Int32.Parse(movieMeterID));

                    lock (lockCache) {
                        // Update FilmDetail Cache
                        if (!mmFilmDetailCache.ContainsKey(movieMeterID))
                            mmFilmDetailCache.Add(movieMeterID, details);

                        // Update IMDB Cache
                        if (!mmIdLookupCache.ContainsKey(details.imdb))
                            mmIdLookupCache.Add(details.imdb, movieMeterID);
                    }

                    return details;
                }
                catch (WebException e) { if (!retryAfterWebException(e, retryCount)) break; }
                catch (XmlRpcFaultException e) {
                    logger.Debug("XML-RPC Fault: Code={0}, String='{1}'", e.FaultCode, e.FaultString);
                    break;
                }
                catch (Exception e) {
                    logger.Error(e);
                    break;
                }
            }

            return null;
        }

        /// <summary>
        /// Flushes the movie information cache
        /// </summary>
        public void FlushCache() {
            lock (lockCache) {
                mmIdLookupCache.Clear();
                mmFilmDetailCache.Clear();
            }
        }

        #endregion

        #region Private methods

        private string getSessionKey() {
            lock (lockSession) {

                // Check if the session is expired or invalid
                bool expired = false;
                if (sessionKey == string.Empty)
                    expired = true;
                else
                    expired = (sessionExpires < DateTime.Now.ToUniversalTime());

                // If the session is expired request a new session key
                if (expired) {
                    sessionKey = string.Empty;
                    try {
                        Session session = Proxy.StartSession(_apiKey);
                        sessionKey = session.session_key;
                        sessionExpires = new DateTime(1970, 1, 1);
                        sessionExpires.AddSeconds(session.valid_till);
                    }
                    catch (WebException e) { logger.DebugException("getSessionKey() Connection failed.", e); }
                    catch (XmlRpcFaultException e) {
                        logger.Debug("XML-RPC Fault: Code={0}, String='{1}'", e.FaultCode, e.FaultString);
                    }
                    catch (Exception e) {
                        logger.Error(e);
                    }
                }

            }
            return sessionKey;
        }

        private bool retryAfterWebException(WebException e, int tryCount) {
            bool retry = true;

            // Skip retry when max retries has been reached
            if (tryCount >= _maxRetries)
                retry = false;

            // Skip retry logic on protocol errors
            if (retry && (e.Status == WebExceptionStatus.ProtocolError)) {
                HttpStatusCode statusCode = ((HttpWebResponse)e.Response).StatusCode;
                if (statusCode != HttpStatusCode.ServiceUnavailable)
                    retry = false;
            }

            // Retry
            if (retry) {
                // If we did not experience a timeout but some other error
                // Sleep for a couple of seconds before retrying
                if (e.Status != WebExceptionStatus.Timeout)
                    Thread.Sleep(2000 + (tryCount * 1000));
            }
            else {
                // Log when we stop trying
                logger.DebugException("API Connection failed.", e);
            }

            return retry;
        }

        #endregion
    }

}