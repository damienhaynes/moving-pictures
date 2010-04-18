using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Web;

namespace MovingPicturesSocialAPI {
    public class MpsAPI {

        private string APIURLBase { get; set; }
        private string Username { get; set; }
        private string Password { get; set; }

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
            this.APIURLBase = APIURLBase;

            this.UserCheckAuthentication();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks the username/password against Moving Pictures Social
        /// </summary>
        /// <returns>True if the authentication passed, False if it did not</returns>
        public bool UserCheckAuthentication() {
            Dictionary<string, string> data = new Dictionary<string, string>();
            try {
                PostData("users/CheckAuthentication", data);
            }
            catch (WebException ex) {
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Unauthorized)
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
        public bool UserUpdate(string email, string locale, string pluginVersion, bool privateProfile) {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("email", email);
            data.Add("locale", locale);
            data.Add("pluginVersion", pluginVersion);
            data.Add("privateProfile", privateProfile.ToString());

            var resp = PostData("users/Update", data);
            if (resp.StatusCode == HttpStatusCode.OK && CheckForSuccess(resp))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Changes the password on an user profile
        /// </summary>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public bool UserChangePassword(string newPassword) {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("newpassword", newPassword);
            var resp = PostData("users/ChangePassword", data);
            if (resp.StatusCode == HttpStatusCode.OK && CheckForSuccess(resp)) {
                this.Password = newPassword;
                return true;
            }
            else {
                return false;
            }
        }


        /// <summary>
        /// Add a movie to an user's collection
        /// </summary>
        /// <param name="sourceName">The name of the third party site used to gather data (ex: imdb)</param>
        /// <param name="sourceId">The id of the movie at the third party site (ex: imdbid)</param>
        /// <param name="MpsId"></param>
        /// <returns></returns>
        /// <exception cref="MpsAPI.MovieDoesNotExistException">Indicates the movie does not exist in MPS yet.  The movie needs to be added using the MovieAddToCollectionWithData() method</exception>
        /// <seealso cref="MovieAddToCollectionWithData"/>
        public bool MovieAddToCollection(string sourceName, string sourceId, out int MpsId) {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("sourceName", sourceName);
            data.Add("sourceId", sourceId);
            var resp = PostData("movies/AddToCollection", data);

            // Check for success
            if (resp.StatusCode == HttpStatusCode.OK && CheckForSuccess(resp)) {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(resp.ResponseString);
                var mpsIdNode = xml.SelectSingleNode("/success/mpsId");
                if (mpsIdNode != null) {
                    MpsId = int.Parse(mpsIdNode.InnerText);
                    return true;
                }
            }

            // Check to see if we received an application error.
            if (resp.StatusCode == HttpStatusCode.OK) {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(resp.ResponseString);

                var errorCodeNode = xml.SelectSingleNode("/error/code");

                // Check to see if we received a movie does not exist error.
                // If so,the movie needs to be added using MovieAddToCollectionWithData
                if (errorCodeNode != null && errorCodeNode.InnerText == "2") {
                    throw new MovieDoesNotExistException();
                }
            }

            MpsId = -1;
            return false;
        }

        /// <summary>
        /// Adds a new movie with data to MPS, and adds it to the user's collection.
        /// This should only be used for movies that do not yet exist
        /// </summary>
        /// <param name="sourceName">The name of the third party site used to gather data (ex: imdb)</param>
        /// <param name="sourceId">The id of the movie at the third party site (ex: imdbid)</param>
        /// <param name="title"></param>
        /// <param name="year"></param>
        /// <param name="certification"></param>
        /// <param name="language"></param>
        /// <param name="tagline"></param>
        /// <param name="summary"></param>
        /// <param name="score"></param>
        /// <param name="popularity"></param>
        /// <param name="runtime"></param>
        /// <param name="genres">Pipe delimited list of genres</param>
        /// <param name="directors">Pipe delimited list of directors</param>
        /// <param name="cast">Pipe delimited list of actors</param>
        /// <param name="translatedTitle"></param>
        /// <param name="locale"></param>
        /// <param name="MpsId"></param>
        /// <returns></returns>
        public bool MovieAddToCollectionWithData(string sourceName, string sourceId, string title, string year, string certification, string language, string tagline
            , string summary, string score, string popularity, string runtime, string genres, string directors, string cast, string translatedTitle, string locale, out int MpsId) {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("sourceName", sourceName);
            data.Add("sourceId", sourceId);
            data.Add("title", title);
            data.Add("year", year);
            data.Add("certification", certification);
            data.Add("language", language);
            data.Add("tagline", tagline);
            data.Add("summary", summary);
            data.Add("score", score);
            data.Add("popularity", popularity);
            data.Add("runtime", runtime);
            data.Add("genres", genres);
            data.Add("directors", directors);
            data.Add("cast", cast);
            data.Add("translatedTitle", translatedTitle);
            data.Add("locale", locale);
            var resp = PostData("movies/AddToCollectionWithData", data);

            // Check for success
            if (resp.StatusCode == HttpStatusCode.OK && CheckForSuccess(resp)) {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(resp.ResponseString);
                var mpsIdNode = xml.SelectSingleNode("/success/mpsId");
                if (mpsIdNode != null) {
                    MpsId = int.Parse(mpsIdNode.InnerText);
                    return true;
                }
            }

            MpsId = -1;
            return false;
        }

        /// <summary>
        /// Adds a collection of new movies with data to MPS, and adds it to the user's collection.
        /// </summary>
        public bool MovieAddToCollectionWithData(ICollection<MovieDTO> movies) {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("movieCount", movies.Count.ToString());
            int i = 0;
            foreach (MovieDTO movie in movies) {
                data.Add(i + "_sourceName", movie.SourceName);
                data.Add(i + "_sourceId", movie.SourceId);
                data.Add(i + "_title", movie.Title);
                data.Add(i + "_year", movie.Year);
                data.Add(i + "_certification", movie.Certification);
                data.Add(i + "_language", movie.Language);
                data.Add(i + "_tagline", movie.Tagline);
                data.Add(i + "_summary", movie.Summary);
                data.Add(i + "_score", movie.Score);
                data.Add(i + "_popularity", movie.Popularity);
                data.Add(i + "_runtime", movie.Runtime);
                data.Add(i + "_genres", movie.Genres);
                data.Add(i + "_directors", movie.Directors);
                data.Add(i + "_cast", movie.Cast);
                data.Add(i + "_translatedTitle", movie.TranslatedTitle);
                data.Add(i + "_locale", movie.Locale);
                i++;
            }

            var resp = PostData("movies/BulkAddToCollectionWithData", data);

            // Check for success
            if (resp.StatusCode == HttpStatusCode.OK && CheckForSuccess(resp))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Remove a movie from an user's collection
        /// </summary>
        /// <param name="mpsId"></param>
        /// <returns></returns>
        public bool MovieRemoveFromCollection(int mpsId) {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("mpsId", mpsId.ToString());
            var resp = PostData("movies/RemoveFromCollection", data);
            if (resp.StatusCode == HttpStatusCode.OK && CheckForSuccess(resp))
                return true;
            else
                return false;
        }

        public bool MovieRemoveFromCollection(string sourceName, string sourceId) {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("sourceName", sourceName);
            data.Add("sourceId", sourceId);
            var resp = PostData("movies/RemoveFromCollectionBySource", data);
            if (resp.StatusCode == HttpStatusCode.OK && CheckForSuccess(resp))
                return true;
            else
                return false;
        }

        #region Static Methods

        public static bool UserCreate(string username, string password, string email, string locale, string pluginVersion, bool privateProfile, string APIURLBase) {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("username", username);
            data.Add("password", password);
            data.Add("email", email);
            data.Add("locale", locale);
            data.Add("pluginVersion", pluginVersion);
            data.Add("privateProfile", privateProfile.ToString());
            var resp = PostData(APIURLBase, "", "", "users/Create", data);

            // Check for success
            if (resp.StatusCode == HttpStatusCode.OK && CheckForSuccess(resp)) {
                return true;
            }

            // Check to see if we received an application error.
            if (resp.StatusCode == HttpStatusCode.OK) {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(resp.ResponseString);

                var errorCodeNode = xml.SelectSingleNode("/error/code");

                // missing parameter
                if (errorCodeNode != null && errorCodeNode.InnerText == "1") {
                    var errorMessageNode = xml.SelectSingleNode("/error/message");
                    string fieldname = errorMessageNode.InnerText.Substring(0, errorMessageNode.InnerText.IndexOf(" "));
                    throw new RequiredFieldMissingException(fieldname);
                }

                // username already exists
                if (errorCodeNode != null && errorCodeNode.InnerText == "2") {
                    throw new UsernameAlreadyExistsException();
                }
            }
            return false;
        }

        #endregion

        #endregion

        #region Private Methods

        private APIHttpResponse PostData(string urlFragment, Dictionary<string, string> postData) {
            return MpsAPI.PostData(this.APIURLBase, this.Username, this.Password, urlFragment, postData);
        }


        private static APIHttpResponse PostData(string APIURLBase, string Username, string Password, string urlFragment, Dictionary<string, string> postData) {
            string postDataStr = "";
            foreach (var item in postData) {
                string key = HttpUtility.UrlEncode(item.Key);
                string value = HttpUtility.UrlEncode(item.Value);
                postDataStr += String.Format("&{0}={1}", key, value);
            }
            if (postDataStr.Length > 0)
                postDataStr = postDataStr.Substring(1);

            return PostData(APIURLBase, Username, Password, urlFragment, postDataStr);
        }

        private static APIHttpResponse PostData(string APIURLBase, string Username, string Password, string urlFragment, string postDataStr) {
            byte[] data = Encoding.UTF8.GetBytes(postDataStr);

            // Prepare web request...
            HttpWebRequest request =
              (HttpWebRequest)WebRequest.Create(APIURLBase + urlFragment);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            if (Username.Length > 0 && Password.Length > 0)
                request.Credentials = new NetworkCredential(Username, Password);
            Stream newStream = request.GetRequestStream();
            // Send the data.
            newStream.Write(data, 0, data.Length);
            newStream.Close();
            // get the response
            HttpWebResponse WebResp = (HttpWebResponse)request.GetResponse();
            APIHttpResponse response = new APIHttpResponse();
            response.StatusCode = WebResp.StatusCode;
            response.ResponseString = new StreamReader(WebResp.GetResponseStream()).ReadToEnd();
            WebResp.Close();
            return response;

        }

        private static bool CheckForSuccess(APIHttpResponse resp) {
            XmlDocument xml = new XmlDocument();
            try {
                xml.LoadXml(resp.ResponseString);
            }
            catch {
                return false;
            }

            var successNode = xml.SelectSingleNode("/success");
            return (successNode != null);
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
}
