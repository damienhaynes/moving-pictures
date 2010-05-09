using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.Database;
using MovingPicturesSocialAPI;
using NLog;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;

namespace MediaPortal.Plugins.MovingPictures {
    public class Social {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static object socialAPILock = new Object();


        // The MpsAPI object that should be used by all components of the plugin.
        public MovingPicturesSocialAPI.MpsAPI SocialAPI {
            get {
                lock (socialAPILock) {
                    if (_socialAPI == null) {
                        _socialAPI = new MpsAPI(MovingPicturesCore.Settings.SocialUsername
                            , MovingPicturesCore.Settings.SocialPassword
                            , MovingPicturesCore.Settings.SocialURLBase + "api/1.0/");
                    }
                    return _socialAPI;
                }
            }
        } private static MovingPicturesSocialAPI.MpsAPI _socialAPI = null;

        public bool HasSocial {
            get {
                return MovingPicturesCore.Settings.SocialUsername.Trim().Length > 0;
            }
        }

        public Social() {
            if (this.HasSocial) {
                MovingPicturesCore.Importer.MovieStatusChanged += new MovieImporter.MovieStatusChangedHandler(movieStatusChangedListener);
                MovingPicturesCore.DatabaseManager.ObjectDeleted += new DatabaseManager.ObjectAffectedDelegate(movieDeletedListener);

                MovingPicturesCore.Settings.SettingChanged += new SettingChangedDelegate(Settings_SettingChanged);
            }
        }

        void Settings_SettingChanged(DBSetting setting, object oldValue) {
            /// Reinitializes the SocialAPI object when Username, Password, or URLBase changes.
            if (setting.Key == "socialurlbase"
                || setting.Key == "socialusername"
                || setting.Key == "socialpassword")
                _socialAPI = null;
        }

        private static void movieStatusChangedListener(MovieMatch obj, MovieImporterAction action) {
            if (action == MovieImporterAction.COMMITED) {
                DBMovieInfo movie = obj.Selected.Movie;
                logger.Info("Adding {0} to Moving Pictures Social Collection", movie.Title);
                MovingPicturesSocialAPI.MovieDTO mpsMovie = new MovingPicturesSocialAPI.MovieDTO();
                string directors = "";
                string actors = "";
                string writers = "";
                string genres = "";
                foreach (var person in movie.Directors) {
                    directors += "|" + person;
                }
                foreach (var person in movie.Actors) {
                    actors += "|" + person;
                }
                foreach (var person in movie.Writers) {
                    writers += "|" + person;
                }
                foreach (var genre in movie.Genres) {
                    genres += "|" + genre;
                }


                int mpsId;
                MovingPicturesCore.Social.SocialAPI.MovieAddToCollectionWithData(
                    movie.PrimarySource.Provider.Name
                    , movie.GetSourceMovieInfo(movie.PrimarySource).Identifier
                    , movie.Title
                    , movie.Year.ToString()
                    , movie.Certification
                    , movie.Language
                    , movie.Tagline
                    , movie.Summary
                    , movie.Score.ToString()
                    , movie.Popularity.ToString()
                    , movie.Runtime.ToString()
                    , genres
                    , directors
                    , actors
                    , movie.Title
                    , movie.PrimarySource.Provider.LanguageCode
                    , out mpsId
                );

            }
        }


        private void movieDeletedListener(DatabaseTable obj) {
            // if this is not a movie object, break
            if (obj.GetType() != typeof(DBMovieInfo))
                return;

            // remove movie from list
            DBMovieInfo movie = (DBMovieInfo)obj;

            if (MovingPicturesCore.Social.HasSocial) {
                logger.Info("Removing {0} from MPS colleciton", movie.Title);
                string sourceName = movie.PrimarySource.Provider.Name;
                string sourceId = movie.GetSourceMovieInfo(movie.PrimarySource).Identifier;
                MovingPicturesCore.Social.SocialAPI.MovieRemoveFromCollection(sourceName, sourceId);
            }

        }
    
    }
}
