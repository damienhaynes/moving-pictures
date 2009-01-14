using System;
using System.Collections.Generic;
using System.IO;
using Cornerstone.Database;
using Cornerstone.Database.CustomTypes;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.Database {
    [DBTableAttribute("local_media")]
    public class DBLocalMedia : MovingPicturesDBTable {

        private static Logger logger = LogManager.GetCurrentClassLogger();

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

        /// <summary>
        /// Checks if the file is currently available.
        /// Online returns true, offline returns false)
        /// </summary>
        public bool IsAvailable {
            get {
                if (fileInfo != null)
                    return DeviceManager.IsAvailable(fileInfo, volume_serial);
                else
                    return false;
            }
        }

        public string Volume {
            get {
                return DeviceManager.GetVolume(fileInfo);
            }

        }

        /// <summary>
        /// Checks if the file is removed from the disk.
        /// If the file is removed (this is different from being offline) this will return true
        /// </summary>
        public bool IsRemoved {
            get {
                // if fileInfo exists then let DeviceManager figure it out
                if (fileInfo != null)
                    return DeviceManager.IsRemoved(fileInfo, volume_serial);
                else
                    // no file so yes.. it's removed
                    return true;
            }
        }

        /// <summary>
        /// Check if the file is a video
        /// </summary>
        public bool IsVideo {
            get {
                if (fileInfo != null)
                    return Utility.IsVideoFile(fileInfo);
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

        [DBFieldAttribute(Default = null)]
        public string DiscId {
            get {
                // todo: how to handle iso?
                bool getDiscId = (bool)MovingPicturesCore.SettingsManager["importer_discid"].Value;
                if (discid == null && IsAvailable && getDiscId && (Utility.GetVideoDiscType(fileInfo.FullName) == Utility.VideoDiscType.DVD)) 
                    DiscId = Utility.GetDiscIdString(fileInfo.DirectoryName);

                return discid;
            }
            set {
                discid = value;
                commitNeeded = true;
            }
        }
        private string discid;

        [DBFieldAttribute(Default = null)]
        public string FileHash {
            get {
                if (fileHash == null && IsAvailable && (Utility.GetVideoDiscType(fileInfo.FullName) == Utility.VideoDiscType.UnknownFormat))
                    FileHash = Utility.GetMovieHashString(fileInfo.FullName);

                return fileHash;
            }
            set {
                fileHash = value;
                commitNeeded = true;
            }
        }
        private string fileHash;

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
                commitNeeded = true;
            }
        } private DBImportPath importPath;

        [DBRelation(AutoRetrieve = true)]
        public RelationList<DBLocalMedia, DBMovieInfo> AttachedMovies {
            get {
                if (_attachedMovies == null) {
                    _attachedMovies = new RelationList<DBLocalMedia, DBMovieInfo>(this);
                }
                return _attachedMovies;
            }
        } RelationList<DBLocalMedia, DBMovieInfo> _attachedMovies;

        #endregion


        #region Public methods

        public bool UpdateDiskProperties() {
            // This will overwrite/update the label and serial field with the current 
            // disk information available from it's import path
            if (importPath != null) {
                VolumeSerial = importPath.GetDiskSerial();
                MediaLabel = importPath.GetVolumeLabel();
                return true; 
            }
            else {
                return false; 
            }
        }

        #endregion
        
        #region Overrides
        public override bool Equals(object obj) {
            if (obj.GetType() == typeof(DBLocalMedia) && ((DBLocalMedia)obj).File != null && this.File != null)
                return (this.File.FullName.Equals(((DBLocalMedia)obj).File.FullName) &&
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
            List<DBLocalMedia> resultSet = GetAll(fullPath, diskSerial);
            if (resultSet.Count > 0) {
                return resultSet[0];
            }

            DBLocalMedia newFile = new DBLocalMedia();
            newFile.FullPath = fullPath;

            return newFile;
        }

        public static List<DBLocalMedia> GetAll(string fullPath) {
            return GetAll(fullPath, null);
        }

        public static List<DBLocalMedia> GetAll(string fullPath, string diskSerial) {
            DBField pathField = DBField.GetField(typeof(DBLocalMedia), "FullPath");
            // using operator LIKE to make the search case insensitive
            ICriteria pathCriteria = new BaseCriteria(pathField, "like", fullPath);
            DBField serialField = DBField.GetField(typeof(DBLocalMedia), "VolumeSerial");
            string op = (diskSerial != null) ? "=" : "is";
            ICriteria serialCriteria = new BaseCriteria(serialField, op, diskSerial);

            ICriteria criteria = new GroupedCriteria(pathCriteria, GroupedCriteria.Operator.AND, serialCriteria);

            List<DBLocalMedia> resultSet = MovingPicturesCore.DatabaseManager.Get<DBLocalMedia>(criteria);
            return resultSet;
        }

        public static List<DBLocalMedia> GetAll() {
            return MovingPicturesCore.DatabaseManager.Get<DBLocalMedia>(null);
        }

        #endregion
    }
}
