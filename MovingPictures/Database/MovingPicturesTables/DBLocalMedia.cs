using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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

        /*
        public DBMovieInfo Movie {
            get { return movie; }
            set {
                movie = value;
                if (movie != null)
                    movieID = movie.ID;
                
                commitNeeded = true;
            }
        }
        private DBMovieInfo movie;
        */
          
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

        #endregion

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

        #region Database Management Methods

        // Gets the cooresponding Setting based on the given record ID.
        public static DBLocalMedia Get(int id) {
            return MovingPicturesPlugin.DatabaseManager.Get<DBLocalMedia>(id);
        }

        public static List<DBLocalMedia> GetAll() {
            return MovingPicturesPlugin.DatabaseManager.Get<DBLocalMedia>(null);
        }

        public static DBLocalMedia Get(string fullPath) {
            DBField pathField = GetField("FullPath");
            ICriteria criteria = new BaseCriteria(pathField, "=", fullPath);
            List<DBLocalMedia> resultSet = MovingPicturesPlugin.DatabaseManager.Get<DBLocalMedia>(criteria);

            if (resultSet.Count > 0) {
                return resultSet[0];
            }

            return null;
        }

        public static DBField GetField(string fieldName) {
            List<DBField> fieldList = DatabaseManager.GetFieldList(typeof(DBLocalMedia));
            foreach (DBField currField in fieldList) {
                if (currField.Name.ToLower() == fieldName.ToLower())
                    return currField;
            }
            return null;
        }

        #endregion
    }
}
