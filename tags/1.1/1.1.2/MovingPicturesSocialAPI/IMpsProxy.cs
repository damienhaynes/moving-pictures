using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CookComputing.XmlRpc;
using MovingPicturesSocialAPI.Data;

namespace MovingPicturesSocialAPI {
    [XmlRpcUrl("")]
    public interface IMpsProxy : IXmlRpcProxy {
        [XmlRpcMethod("CheckAuthentication")]
        object CheckAuthentication();

        [XmlRpcMethod("CheckUsernameAvailability", StructParams = true)]
        object CheckUsernameAvailability(string Username);

        [XmlRpcMethod("CreateUser", StructParams = true)]
        object CreateUser(string Username, string Password, string Email, string Locale, bool PrivateProfile);

        [XmlRpcMethod("UpdateUser", StructParams = true)]
        object UpdateUser(string Email, string Locale, bool PrivateProfile);

        [XmlRpcMethod("GetUserData")]
        object GetUserData();

        [XmlRpcMethod("AddMoviesToCollectionWithData", StructParams = true)]
        object[] AddMoviesToCollectionWithData(MpsMovie[] movies);

        [XmlRpcMethod("RemoveMovieFromCollection", StructParams = true)]
        object RemoveMovieFromCollection(int MovieId);

        [XmlRpcMethod("GetUserTaskList", StructParams = true)]
        object[] GetUserTaskList(DateTime? startDate);

        [XmlRpcMethod("UploadCover", StructParams = true)]
        object UploadCover(int MovieId, string Base64File);

        [XmlRpcMethod("SetMovieRating", StructParams = true)]
        object SetMovieRating(int MovieId, string Rating);

        [XmlRpcMethod("WatchMovie", StructParams = true)]
        object WatchMovie(int MovieId, bool InsertInStream);

        [XmlRpcMethod("UnwatchMovie", StructParams = true)]
        object UnwatchMovie(int MovieId);

        [XmlRpcMethod("WatchingMovie", StructParams = true)]
        object WatchingMovie(int MovieId);

        [XmlRpcMethod("StopWatchingMovie", StructParams = true)]
        object StopWatchingMovie(int MovieId);
    }
}
