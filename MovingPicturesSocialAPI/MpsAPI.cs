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
        /// <param name="pluginVersion"></param>
        /// <param name="privateProfile"></param>
        /// <returns></returns>
        public bool UpdateUser(string email, string locale, string pluginVersion, bool privateProfile) {
            var proxy = CreateProxy();
            proxy.UpdateUser(email, locale, pluginVersion, privateProfile.ToString());
            return true;
        }



        /// <summary>
        /// Adds a collection of new movies with data to MPS, and adds it to the user's collection.
        /// </summary>
        public bool AddMoviesToCollection(ICollection<MovieDTO> movies) {
            var proxy = CreateProxy();
            proxy.AddMoviesToCollectionWithData(((List<MovieDTO>)movies).ToArray());
            return true;
        }

        /// <summary>
        /// Adds a new movie with data to MPS, and adds it to the user's collection.
        /// </summary>
        public void AddMoviesToCollection(MovieDTO mpsMovie) {
            List<MovieDTO> movies = new List<MovieDTO>();
            movies.Add(mpsMovie);
            AddMoviesToCollection(movies);
        }



        /// <summary>
        /// Remove a movie from an user's collection
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public bool RemoveMovieFromCollection(string sourceName, string sourceId) {
            var proxy = CreateProxy();
            proxy.RemoveMovieFromCollection(sourceName, sourceId);
            return true;
        }

        public bool UploadCover(string sourceName, string sourceId, string file) {
            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);

            byte[] binaryData = new Byte[fileStream.Length];
            long bytesRead = fileStream.Read(binaryData, 0, (int)fileStream.Length);
            fileStream.Close();

            string base64String =
                System.Convert.ToBase64String(binaryData, 0, binaryData.Length);

            var proxy = CreateProxy();
            proxy.UploadCover(sourceName, sourceId, base64String);

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
                tli.SourceName = task["SourceName"].ToString();
                tli.SourceId = task["SourceId"].ToString();
                result.Add(tli);
            }

            return result;
        }

        public void SetMovieRating(string sourceName, string sourceId, int rating) {
            var proxy = CreateProxy();
            proxy.SetMovieRating(sourceName, sourceId, rating.ToString());
        }

        public void SetWatchCount(string sourceName, string sourceId, int watchCount) {
            var proxy = CreateProxy();
            proxy.SetWatchCount(sourceName, sourceId, watchCount);
        }

        #region Static Methods

        public static bool CreateUser(string username, string password, string email, string locale, string pluginVersion, bool privateProfile, string APIURL) {
            var proxy = CreateProxy(APIURL);
            try {
                proxy.CreateUser(username, password, email, locale, pluginVersion, privateProfile.ToString());
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
        public string SourceName;
        public string SourceId;
    }

    public enum TaskItemType {
        Cover
    }


    [XmlRpcUrl("http://localhost:8080/api/1.0/api")]
    public interface IMPSApi : IXmlRpcProxy {
        [XmlRpcMethod("CheckAuthentication")]
        object CheckAuthentication();

        [XmlRpcMethod("CreateUser", StructParams = true)]
        object CreateUser(string Username, string Password, string Email, string Locale, string PluginVersion, string PrivateProfile);

        [XmlRpcMethod("UpdateUser", StructParams = true)]
        object UpdateUser(string Email, string Locale, string PluginVersion, string PrivateProfile);

        [XmlRpcMethod("AddMoviesToCollectionWithData", StructParams = true)]
        object AddMoviesToCollectionWithData(MovieDTO[] movies);

        [XmlRpcMethod("RemoveMovieFromCollection", StructParams = true)]
        object RemoveMovieFromCollection(string SourceName, string SourceId);

        [XmlRpcMethod("GetUserTaskList")]
        object[] GetUserTaskList();

        [XmlRpcMethod("UploadCover", StructParams = true)]
        object UploadCover(string SourceName, string SourceId, string Base64File);

        [XmlRpcMethod("SetMovieRating", StructParams = true)]
        object SetMovieRating(string SourceName, string SourceId, string Rating);

        [XmlRpcMethod("SetWatchCount", StructParams = true)]
        object SetWatchCount(string SourceName, string SourceId, int WatchCount);
    }

    public struct MovieData {
        public string SourceName;
        public string SourceId;
        public string Title;
        public string Year;
        public string Certification;
        public string Language;
        public string Tagline;
        public string Summary;
        public string Score;
        public string Popularity;
        public string Runtime;
        public string Genres;
        public string Directors;
        public string Cast;
        public string TranslatedTitle;
        public string Locale;

    }

}
