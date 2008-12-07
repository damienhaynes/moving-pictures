using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using Cornerstone.Database;
using Cornerstone.Database.CustomTypes;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;

namespace MediaPortal.Plugins.MovingPictures.Database {
    [DBTableAttribute("local_media")]
    public class DBLocalMedia: MovingPicturesDBTable {

        public override void AfterDelete() {
        }

        public FileInfo File {
            get { return fileInfo; }

            set {
                fileInfo = value;
                commitNeeded = true;
            }
        }
        private FileInfo fileInfo;


        #region read-only properties

        public bool Available {
            get {
                if (fileInfo != null)
                    return DeviceManager.IsAvailable(fileInfo, volume_serial);
                else
                    return false;
            }
        }

        public bool Removed {
            get {
                // if fileInfo exists then let DeviceManager figure it out
                if (fileInfo != null)
                    return DeviceManager.IsRemoved(fileInfo, volume_serial);
                else
                // no file so yes.. it's removed
                    return true;
            }
        }

        public bool IsVideo {
            get {
                if (fileInfo != null)
                    return Utility.IsMediaPortalVideoFile(fileInfo);
                else
                    return false;
            }
        }

        #endregion

        #region Database Fields

        [DBFieldAttribute(Default = null, FieldName = "media_label")]
        public string MediaLabel {
            get {
                return media_label;
            }
            set {
                media_label = value;
                commitNeeded = true;
            }
        }
        private string media_label;
                
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

        [DBFieldAttribute(Default = null, FieldName = "volume_serial")]
        public string VolumeSerial {
            get {
                return volume_serial;
            }
            set {
                volume_serial = value;
                commitNeeded = true;
            }
        }
        private string volume_serial;

        [DBFieldAttribute(Default = "0")]
        public string DiscId {
            get {
                // Calculate DiscId
                // todo: how to handle iso?
                bool getDiscId = (bool)MovingPicturesCore.SettingsManager["importer_discid"].Value;
                if ((discid == "0") && (fileInfo != null) && getDiscId)
                    if (fileInfo.Name.ToLower() == "video_ts.ifo")
                        if (volume_serial == DeviceManager.GetDiskSerial(fileInfo.Directory) || String.IsNullOrEmpty(volume_serial))
                            discid = Utility.GetDiscIdString(fileInfo.DirectoryName);

                return discid;
            }
            set {
                discid = value;
                commitNeeded = true;
            }
        }
        private string discid;

        [DBFieldAttribute(Default = "1")]
        public int Part {
            get { return part; }
            set {
                part = value;
                commitNeeded = true;
            }
        }
        private int part;

        [DBFieldAttribute(Default = "0")]
        public int Duration {
            get { return duration; }
            set {
                duration = value;
                commitNeeded = true;
            }
        }
        private int duration;

        [DBFieldAttribute(Default = "false")]
        public bool Ignored {
            get { return ignored; }
            set {
                ignored = value;
                commitNeeded = true;
            }
        }
        private bool ignored;

        [DBFieldAttribute]
        public DBImportPath ImportPath {
            get { return importPath; }
            set {
                importPath = value;

                // When the importhPath is attached to the file
                // copy the current label and serial over
                // this keeps the logic in one place
                if (importPath != null) {
                    if (volume_serial == null)
                        volume_serial = importPath.Serial;
                    if (media_label == null)
                        media_label = importPath.VolumeLabel;
                }

                commitNeeded = true;
            }
        } private DBImportPath importPath;

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
                return ( this.File.FullName.Equals(((DBLocalMedia)obj).File.FullName) && 
                    this.VolumeSerial.Equals(((DBLocalMedia)obj).VolumeSerial)
                    );

            return false;
        }

        public override int GetHashCode() {
            if (File != null)
                return (VolumeSerial + "|" + File.FullName).GetHashCode();

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
            return Get(fullPath, null);
        }

        public static DBLocalMedia Get(string fullPath, string diskSerial) {
            DBField pathField = DBField.GetField(typeof(DBLocalMedia), "FullPath");
            ICriteria pathCriteria = new BaseCriteria(pathField, "=", fullPath);
            DBField serialField = DBField.GetField(typeof(DBLocalMedia), "VolumeSerial");
            string op = (diskSerial != null) ? "=" : "is";
            ICriteria serialCriteria = new BaseCriteria(serialField, op, diskSerial);

            ICriteria criteria= new GroupedCriteria(pathCriteria, GroupedCriteria.Operator.AND, serialCriteria);

            List<DBLocalMedia> resultSet = MovingPicturesCore.DatabaseManager.Get<DBLocalMedia>(criteria);

            if (resultSet.Count > 0) {
                return resultSet[0];
            }

            DBLocalMedia newFile = new DBLocalMedia();
            newFile.FullPath = fullPath;
            newFile.VolumeSerial = diskSerial;

            return newFile;
        }

        public static List<DBLocalMedia> GetAll() {
            return MovingPicturesCore.DatabaseManager.Get<DBLocalMedia>(null);
        }

        #endregion
    }
}
