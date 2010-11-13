using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CookComputing.XmlRpc;
using MovingPicturesSocialAPI.Data;

namespace MovingPicturesSocialAPI {
    [XmlRpcUrl("http://localhost:8080/api/1.0/api")]
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
