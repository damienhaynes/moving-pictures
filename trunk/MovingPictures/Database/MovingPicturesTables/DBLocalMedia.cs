using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MediaPortal.Plugins.MovingPictures.Database.CustomTypes;
using System.Collections.ObjectModel;

namespace MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables {
    [DBTableAttribute("local_media")]
    public class DBLocalMedia: MoviesPluginDBTable {

        public override void CleanUpForDeletion() {
        }

        public FileInfo File {
            get { return fileInfo; }

            set {
                fileInfo = value;
                commitNeeded = true;
            }
        }
        private FileInfo fileInfo;

         
        #region Database Fields

        [DBFieldAttribute]
        public string FullPath {
            get {
                if (fileInfo == null)
                    return "";

                return fileInfo.FullName;
            }

            set {
                if (value.Trim() == "")
                    fileInfo = null;
                else
                    fileInfo = new FileInfo(value);

                commitNeeded = true;
            }
        }
        /*
        [DBFieldAttribute(Default=null, FieldName="movie_info_id")]
        public int? MovieID {
            get { return movieID; }
            set {
                if (value == null) {
                    movieID = null;
                    commitNeeded = true;
                    return;
                }

                Movie = DBMovieInfo.Get((int)value);
                commitNeeded = true;
            }
        }
        private int? movieID;
        */

        [DBFieldAttribute(Default="1")]
        public int Part {
            get { return part; }
            set {
                part = value;
                commitNeeded = true;
            }
        }
        private int part;

        [DBFieldAttribute(Default = "false")]
        public bool Ignored {
            get { return ignored; }
            set {
                ignored = value;
                commitNeeded = true;
            }
        }
        private bool ignored;

        [DBRelation(AutoRetrieve=true)]
        public RelationList<DBLocalMedia, DBMovieInfo> AttachedMovies {
            get {
                if (_attachedMovies == null) {
                    _attachedMovies = new RelationList<DBLocalMedia, DBMovieInfo>(this);
                }
                return _attachedMovies;
            }
        } RelationList<DBLocalMedia, DBMovieInfo> _attachedMovies;

        #endregion

        #region Overrides
        public override bool Equals(object obj) {
            if (obj.GetType() == typeof(DBLocalMedia) && ((DBLocalMedia)obj).File != null && this.File != null)
                return (this.File.FullName.Equals(((DBLocalMedia)obj).File.FullName));

            return false;
        }

        public override int GetHashCode() {
            if (File != null)
                return File.FullName.GetHashCode();

            return base.GetHashCode();
        }

        public override string ToString() {
            if (File != null)
                return File.Name;

            return base.ToString();
        }
        #endregion

        #region Database Management Methods

        public static DBLocalMedia Get(string fullPath) {
            DBField pathField = DBField.GetField(typeof(DBLocalMedia), "FullPath");
            ICriteria criteria = new BaseCriteria(pathField, "=", fullPath);
            List<DBLocalMedia> resultSet = MovingPicturesPlugin.DatabaseManager.Get<DBLocalMedia>(criteria);

            if (resultSet.Count > 0) {
                return resultSet[0];
            }

            DBLocalMedia newFile = new DBLocalMedia();
            newFile.FullPath = fullPath;

            return newFile;
        }

        #endregion
    }
}
