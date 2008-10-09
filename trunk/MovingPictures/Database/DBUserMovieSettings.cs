using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database;
using Cornerstone.Database.CustomTypes;
using Cornerstone.Database.Tables;

namespace MediaPortal.Plugins.MovingPictures.Database {
    [DBTableAttribute("user_movie_settings")]
    public class DBUserMovieSettings: MovingPicturesDBTable {

        public override void AfterDelete() {
        }

        #region Database Fields

        [DBFieldAttribute]
        public DBUser User {
            get { return user; }
            set {
                user = value;
                commitNeeded = true;
            }
        } private DBUser user;

        //[DBFieldAttribute]
        //public DBMovieInfo Movie {
        //    get { return movie; }
        //    set {
        //        movie = value;
        //        commitNeeded = true;
        //    }
        //} private DBMovieInfo movie;

        // Value between 0 and 4. As in 4 stars.
        [DBFieldAttribute(FieldName = "user_rating", Default = null)]
        public int? UserRating {
            get { return _userRating; }
            set {
                _userRating = value;
                commitNeeded = true;
            }
        } private int? _userRating;

        [DBFieldAttribute(FieldName = "watched")]
        public int Watched {
            get { return _watched; }
            set {
                _watched = value;
                commitNeeded = true;
            }
        } private int _watched;
        
        [DBFieldAttribute(FieldName = "resume_part")]
        public int ResumePart {
          get { return _resumePart; }

          set {
            _resumePart = value;
            commitNeeded = true;
          }
        } private int _resumePart;


        [DBFieldAttribute(FieldName = "resume_time")]
        public int ResumeTime {
          get { return _resumeTime; }

          set {
            _resumeTime = value;
            commitNeeded = true;
          }
        } private int _resumeTime;

        [DBFieldAttribute(FieldName = "resume_data", Default = null)]
        public byte[] ResumeData {
          get { return _resumeData; }

          set {
            _resumeData = value;
            commitNeeded = true;
          }
        } private byte[] _resumeData;

        [DBRelation(AutoRetrieve = true)]
        public RelationList<DBUserMovieSettings, DBMovieInfo> AttachedMovies {
          get {
            if (_attachedMovies == null) {
              _attachedMovies = new RelationList<DBUserMovieSettings, DBMovieInfo>(this);
            }
            return _attachedMovies;
          }
        } RelationList<DBUserMovieSettings, DBMovieInfo> _attachedMovies;

        #endregion

        #region Database Management Methods
   
        public static DBUserMovieSettings Get(int id) {
            return MovingPicturesCore.DatabaseManager.Get<DBUserMovieSettings>(id);
        }

        public static List<DBUserMovieSettings> GetAll() {
            return MovingPicturesCore.DatabaseManager.Get<DBUserMovieSettings>(null);
        }

        #endregion
    }
}
