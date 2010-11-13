using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using CookComputing.XmlRpc;
using MovingPicturesSocialAPI.Data;
using System.Security.Cryptography;

namespace MovingPicturesSocialAPI {
    public class MpsAPI {

        public static readonly string DefaultUrl = "http://social.moving-pictures.tv/api/1.0/";

        public MpsUser User {
            get;
            internal set;
        }

        public IMpsProxy Proxy { 
            get; 
            internal set; 
        }
        
        public string ApiUrl { 
            get; 
            internal set; 
        }

        public event MpsAPIRequestDelegate RequestEvent;
        public event MpsAPIResponseDelegate ResponseEvent;

        public delegate void MpsAPIRequestDelegate(string RequestText);
        public delegate void MpsAPIResponseDelegate(string ResponseText);

        #region Static Methods

        /// <summary>
        /// Attempts to login with the given username and password.
        /// </summary>
        /// <returns>An MpsAPI object if login is successful.</returns>
        public static MpsAPI Login(string username, string hashedPassword) {
            return Login(username, hashedPassword, DefaultUrl);
        }

        /// <summary>
        /// Attempts to login with the given username and password.
        /// </summary>
        /// <returns>An MpsAPI object if login is successful.</returns>
        public static MpsAPI Login(string username, string hashedPassword, string apiUrl) {
            if (apiUrl == null) apiUrl = DefaultUrl;

            IMpsProxy proxy = CreateProxy(username, hashedPassword, apiUrl);

            try { object result = proxy.CheckAuthentication(); }
            catch (XmlRpcServerException ex) {
                if (ex.Message == "Unauthorized") return null;
                else throw;
            }

            MpsAPI api = new MpsAPI(username, hashedPassword, apiUrl, proxy);
            proxy.RequestEvent += new XmlRpcRequestEventHandler(api.proxy_RequestEvent);
            proxy.ResponseEvent += new XmlRpcResponseEventHandler(api.proxy_ResponseEvent);

            return api;
        }

        public static bool CreateUser(string username, string password, string email, string locale, bool privateProfile) {
            return CreateUser(username, password, email, locale, privateProfile, DefaultUrl);
        }

        public static bool CreateUser(string username, string password, string email, string locale, bool privateProfile, string APIURL) {
            if (APIURL == null) APIURL = DefaultUrl;
            
            var proxy = CreateProxy(null, null, APIURL);
            try {
                proxy.CreateUser(username, password, email, locale, privateProfile);
            }
            catch (XmlRpcFaultException ex) {
                if (ex.FaultCode == 1) { // missing required field
                    string fieldname = ex.FaultString.Substring(0, ex.FaultString.IndexOf(" "));
                    throw new RequiredFieldMissingException(fieldname);
                }

                else if (ex.FaultCode == 2)
                    throw new UsernameAlreadyExistsException();

                else
                    throw;
            }

            return true;
        }

        public static bool IsUsernameAvailable(string username) {
            return IsUsernameAvailable(username, DefaultUrl);
        }

        public static bool IsUsernameAvailable(string username, string APIURL) {
            if (APIURL == null) APIURL = DefaultUrl;
            var proxy = CreateProxy(null, null, APIURL);

            var xmlData = (XmlRpcStruct)proxy.CheckUsernameAvailability(username);
            return bool.Parse(xmlData["available"].ToString());
        }

        public static string HashPassword(string password) {
            // salt + hash
            string salt = "52c3a0d0-f793-46fb-a4c0-35a0ff6844c8";
            string saltedPassword = password + salt;
            SHA1CryptoServiceProvider sha1Obj = new SHA1CryptoServiceProvider();
            byte[] bHash = sha1Obj.ComputeHash(Encoding.ASCII.GetBytes(saltedPassword));
            string sHash = "";
            foreach (byte b in bHash) 
                sHash += b.ToString("x2");            
            return sHash;
        }

        private static IMpsProxy CreateProxy(string username, string password, string apiUrl) {
            if (apiUrl == null) apiUrl = DefaultUrl;
            
            IMpsProxy proxy = XmlRpcProxyGen.Create<IMpsProxy>();
            proxy.Url = apiUrl;
            proxy.UserAgent = "Moving Pictures Social C# API";
            proxy.KeepAlive = false;

            if (username != null && username.Length > 0 && password != null && password.Length > 0) {
                string auth = username + ":" + password;
                auth = Convert.ToBase64String(Encoding.Default.GetBytes(auth));
                proxy.Headers["Authorization"] = "Basic " + auth;
            }

            return proxy;
        }

        #endregion

        #region Constructors

        private MpsAPI(string username, string hashedPassword, string apiUrl, IMpsProxy proxy) {
            User = new MpsUser();
            User.Name = username;
            User.HashedPassword = hashedPassword;            
            
            ApiUrl = apiUrl;
            Proxy = proxy;
        }

        #endregion

        #region Public Methods


        /// <summary>
        /// Updates an user profile in Moving Pictures Social.  Any fields left blank are not updated
        /// </summary>
        /// <param name="email"></param>
        /// <param name="locale"></param>
        /// <param name="privateProfile"></param>
        /// <returns></returns>
        public bool UpdateUser(string email, string locale, bool? privateProfile) {
            if (email == null) email = User.Email;
            if (locale == null) locale = User.Locale;
            if (privateProfile == null) privateProfile = User.PrivateProfile;
            Proxy.UpdateUser(email, locale, (bool)privateProfile);
            return true;
        }



        /// <summary>
        /// Adds a collection of new movies with data to MPS, and adds it to the user's collection.
        /// </summary>
        public void AddMoviesToCollection(ref List<MpsMovie> movies) {
            var movieIds = Proxy.AddMoviesToCollectionWithData(movies.ToArray());

            // insert the MpsId's into the original list
            foreach (XmlRpcStruct movieId in movieIds) {
                MpsMovie movieDTO = movies.Find(
                    delegate(MpsMovie a) {
                        return a.InternalId == Convert.ToInt32(movieId["InternalId"]);
                    });

                if (movieDTO != null) {
                    movieDTO.MovieId = Convert.ToInt32(movieId["MovieId"]);
                    movieDTO.UserRating = Convert.ToInt32(movieId["UserRating"]);
                }
            }
        }

        /// <summary>
        /// Adds a new movie with data to MPS, and adds it to the user's collection.
        /// </summary>
        public void AddMoviesToCollection(ref MpsMovie mpsMovie) {
            List<MpsMovie> movies = new List<MpsMovie>() { mpsMovie };
            movies.Add(mpsMovie);
            AddMoviesToCollection(ref movies);
        }



        /// <summary>
        /// Remove a movie from an user's collection
        /// </summary>
        /// <param name="MovieId"></param>
        /// <returns></returns>
        public bool RemoveMovieFromCollection(int movieId) {
            Proxy.RemoveMovieFromCollection(movieId);
            return true;
        }

        public bool UploadCover(int movieId, string file) {
            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);

            byte[] binaryData = new Byte[fileStream.Length];
            long bytesRead = fileStream.Read(binaryData, 0, (int)fileStream.Length);
            fileStream.Close();

            string base64String =
                System.Convert.ToBase64String(binaryData, 0, binaryData.Length);

            Proxy.UploadCover(movieId, base64String);

            return true;
        }


        public List<TaskListItem> GetUserTaskList() {
            List<TaskListItem> result = new List<TaskListItem>();

            var tasks = Proxy.GetUserTaskList();

            foreach (XmlRpcStruct task in tasks) {
                TaskListItem tli = new TaskListItem();
                switch (task["ItemType"].ToString()) {
                    case "cover":
                        tli.ItemType = TaskItemType.Cover;
                        break;
                    default:
                        tli.ItemType = TaskItemType.Cover;
                        break;
                }
                tli.MovieId = (int)task["MovieId"];
                result.Add(tli);
            }

            return result;
        }

        public List<UserSyncData> GetUserSyncData(DateTime startDate) {
            var xmlData = Proxy.GetUserSyncData(startDate);
            List<UserSyncData> result = new List<UserSyncData>();

            foreach (XmlRpcStruct item in xmlData)
            {
                UserSyncData usd = new UserSyncData();
                usd.MovieId = int.Parse(item["MovieId"].ToString());
                usd.Rating = int.Parse(item["Rating"].ToString());
                usd.RatingDate = DateTime.Parse(item["RatingDate"].ToString());
                result.Add(usd);
            }

            return result;
        }

        public void SetMovieRating(int movieId, int rating) {
            Proxy.SetMovieRating(movieId, rating.ToString());
        }

        public void WatchMovie(int movieId, int newWatchCount) {
            Proxy.WatchMovie(movieId, newWatchCount);
        }

        #endregion

        #region Private Methods


        void proxy_RequestEvent(object sender, XmlRpcRequestEventArgs args) {
            if (RequestEvent != null) {
                TextReader txt = new StreamReader(args.RequestStream);
                string request = txt.ReadToEnd();
                RequestEvent(request);
            }
        }

        void proxy_ResponseEvent(object sender, XmlRpcResponseEventArgs args) {
            if (ResponseEvent != null) {
                TextReader txt = new StreamReader(args.ResponseStream);
                string response = txt.ReadToEnd();
                ResponseEvent(response);
            }
        }

        #endregion
    }

    #region Custom Exceptions
    public class RequiredFieldMissingException : Exception {
        public string FieldName { get; set; }
        public RequiredFieldMissingException(string fieldName) {
            this.FieldName = fieldName;
        }
    }
    public class MovieDoesNotExistException : Exception { }
    public class UsernameAlreadyExistsException : Exception { }
    #endregion

    class APIHttpResponse {
        public HttpStatusCode StatusCode { get; set; }
        public string ResponseString { get; set; }
    }

    public struct TaskListItem {
        public TaskItemType ItemType;
        public int MovieId;
    }

    public enum TaskItemType {
        Cover
    }

    public struct UserSyncData {
        public int MovieId;
        public int Rating;
        public DateTime RatingDate;
    }

}
