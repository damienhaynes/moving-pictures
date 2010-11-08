using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using CookComputing.XmlRpc;

namespace MovingPicturesSocialAPI {
    public class MpsAPI {

        private string APIURL { get; set; }
        private string Username { get; set; }
        private string Password { get; set; }
        public event MpsAPIRequestDelegate RequestEvent;
        public event MpsAPIResponseDelegate ResponseEvent;
        public delegate void MpsAPIRequestDelegate(string RequestText);
        public delegate void MpsAPIResponseDelegate(string ResponseText);

        #region Constructors

        private MpsAPI() {
            // do not allow empty constructor
        }

        public MpsAPI(string Username, string Password) {
            new MpsAPI(Username, Password, "http://social.moving-pictures.tv/api/1.0/");
        }

        public MpsAPI(string Username, string Password, string APIURLBase) {
            this.Username = Username;
            this.Password = Password;
            this.APIURL = APIURLBase;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks the username/password against Moving Pictures Social
        /// </summary>
        /// <returns>True if the authentication passed, False if it did not</returns>
        public bool CheckAuthentication() {

            IMPSApi proxy = CreateProxy();
            try {
                object result = proxy.CheckAuthentication();
            }
            catch (XmlRpcServerException ex) {
                if (ex.Message == "Unauthorized")
                    return false;
                else
                    throw;
            }
            return true;
        }


        /// <summary>
        /// Updates an user profile in Moving Pictures Social.  Any fields left blank are not updated
        /// </summary>
        /// <param name="email"></param>
        /// <param name="locale"></param>
        /// <param name="privateProfile"></param>
        /// <returns></returns>
        public bool UpdateUser(string email, string locale, bool privateProfile) {
            var proxy = CreateProxy();
            proxy.UpdateUser(email, locale, privateProfile.ToString());
            return true;
        }



        /// <summary>
        /// Adds a collection of new movies with data to MPS, and adds it to the user's collection.
        /// </summary>
        public void AddMoviesToCollection(ref List<MovieDTO> movies) {
            var proxy = CreateProxy();
            var movieIds = proxy.AddMoviesToCollectionWithData(movies.ToArray());

            // insert the MpsId's into the original list
            foreach (XmlRpcStruct movieId in movieIds) {
                MovieDTO movieDTO = movies.Find(
                    delegate(MovieDTO a) {
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
        public void AddMoviesToCollection(ref MovieDTO mpsMovie) {
            List<MovieDTO> movies = new List<MovieDTO>();
            movies.Add(mpsMovie);
            AddMoviesToCollection(ref movies);
        }



        /// <summary>
        /// Remove a movie from an user's collection
        /// </summary>
        /// <param name="MovieId"></param>
        /// <returns></returns>
        public bool RemoveMovieFromCollection(int movieId) {
            var proxy = CreateProxy();
            proxy.RemoveMovieFromCollection(movieId);
            return true;
        }

        public bool UploadCover(int movieId, string file) {
            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);

            byte[] binaryData = new Byte[fileStream.Length];
            long bytesRead = fileStream.Read(binaryData, 0, (int)fileStream.Length);
            fileStream.Close();

            string base64String =
                System.Convert.ToBase64String(binaryData, 0, binaryData.Length);

            var proxy = CreateProxy();
            proxy.UploadCover(movieId, base64String);

            return true;
        }


        public List<TaskListItem> GetUserTaskList() {
            List<TaskListItem> result = new List<TaskListItem>();

            var proxy = CreateProxy();
            var tasks = proxy.GetUserTaskList();

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
            var proxy = CreateProxy();
            var xmlData = proxy.GetUserSyncData(startDate);
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
            var proxy = CreateProxy();
            proxy.SetMovieRating(movieId, rating.ToString());
        }

        public void WatchMovie(int movieId, int newWatchCount) {
            var proxy = CreateProxy();
            proxy.WatchMovie(movieId, newWatchCount);
        }

        #region Static Methods

        public static bool CheckUsernameAvailability(string username, string APIURL) {
            var proxy = CreateProxy(APIURL);

            var xmlData = (XmlRpcStruct)proxy.CheckUsernameAvailability(username);
            return bool.Parse(xmlData["available"].ToString());
        }

        public static bool CreateUser(string username, string password, string email, string locale, bool privateProfile, string APIURL) {
            var proxy = CreateProxy(APIURL);
            try {
                proxy.CreateUser(username, password, email, locale, privateProfile.ToString());
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

        #endregion

        #endregion

        #region Private Methods


        private IMPSApi CreateProxy() {
            IMPSApi proxy = XmlRpcProxyGen.Create<IMPSApi>();
            proxy.Url = this.APIURL;
            proxy.UserAgent = "MovPic"; // todo
            proxy.KeepAlive = false;

            proxy.RequestEvent += new XmlRpcRequestEventHandler(proxy_RequestEvent);
            proxy.ResponseEvent += new XmlRpcResponseEventHandler(proxy_ResponseEvent);

            if (this.Username.Length > 0 && this.Password.Length > 0) {
                string auth = this.Username + ":" + this.Password;
                auth = Convert.ToBase64String(Encoding.Default.GetBytes(auth));
                proxy.Headers["Authorization"] = "Basic " + auth;
            }
            return proxy;
        }


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


        private static IMPSApi CreateProxy(string APIURL) {
            IMPSApi proxy = XmlRpcProxyGen.Create<IMPSApi>();
            proxy.Url = APIURL;
            proxy.UserAgent = "MovPic"; // todo

            return proxy;
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

    [XmlRpcUrl("http://localhost:8080/api/1.0/api")]
    public interface IMPSApi : IXmlRpcProxy {
        [XmlRpcMethod("CheckAuthentication")]
        object CheckAuthentication();

        [XmlRpcMethod("CheckUsernameAvailability", StructParams = true)]
        object CheckUsernameAvailability(string Username);

        [XmlRpcMethod("CreateUser", StructParams = true)]
        object CreateUser(string Username, string Password, string Email, string Locale, string PrivateProfile);

        [XmlRpcMethod("UpdateUser", StructParams = true)]
        object UpdateUser(string Email, string Locale, string PrivateProfile);

        [XmlRpcMethod("AddMoviesToCollectionWithData", StructParams = true)]
        object[] AddMoviesToCollectionWithData(MovieDTO[] movies);

        [XmlRpcMethod("RemoveMovieFromCollection", StructParams = true)]
        object RemoveMovieFromCollection(int MovieId);

        [XmlRpcMethod("GetUserTaskList")]
        object[] GetUserTaskList();

        [XmlRpcMethod("UploadCover", StructParams = true)]
        object UploadCover(int MovieId, string Base64File);

        [XmlRpcMethod("SetMovieRating", StructParams = true)]
        object SetMovieRating(int MovieId, string Rating);

        [XmlRpcMethod("WatchMovie", StructParams = true)]
        object WatchMovie(int MovieId, int NewWatchCount);

        [XmlRpcMethod("GetUserSyncData", StructParams = true)]
        object[] GetUserSyncData(DateTime startDate);
    }
}
