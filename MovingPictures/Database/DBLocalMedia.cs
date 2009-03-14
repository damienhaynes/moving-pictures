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

        private bool deleting = false;

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
        
        public VideoDiscFormat VideoDiscFormat {
            get {
                if (File != null)
                    return Utility.GetVideoDiscFormat(fileInfo.FullName);
               
                return VideoDiscFormat.Unknown;
            }
        } 

        /// <summary>
        /// Checks wether the file is a DVD.
        /// </summary>
        public bool IsDVD {
            get {
                if (VideoDiscFormat == VideoDiscFormat.DVD)
                    return true;

                return false;
            }
        }        

        /// <summary>
        /// Checks wether the file is an entry path for a video disc.
        /// </summary>
        public bool IsVideoDisc {
            get {
                if (File != null)
                    return Utility.IsVideoDiscPath(File.FullName);

                return false;
            }
        }

        /// <summary>
        /// Checks if the file is currently available.
        /// Online returns true, offline returns false)
        /// </summary>
        public bool IsAvailable {
            get {
                if (fileInfo != null) {
                    bool available = DeviceManager.IsAvailable(fileInfo, volume_serial);

                    // If this media is a DVD in an optical drive
                    // double check the DiscID to make sure we are looking
                    // at the same disc. 
                    if (available && IsDVD && (ImportPath.GetDriveType() == DriveType.CDRom)) {
                        // Grab the current DiscID and compare it to the stored DiscID
                        string currentDiscID = Utility.GetDiscIdString(fileInfo.DirectoryName);
                        // If the id's don't match the media is not available
                        if (currentDiscID != DiscId)
                            return false;
                    }

                    return available;
                }
                else {
                    return false;
                }
            }
        }

        public string DriveLetter {
            get {
                return DeviceManager.GetDriveLetter(fileInfo);
            }
        }

        /// <summary>
        /// Checks if the file is removed from the disk.
        /// If the file is removed (this is different from being offline) this will return true
        /// </summary>
        public bool IsRemoved {
            get {
                // Skip this check for CDRom drives
                if (ImportPath.GetDriveType() == DriveType.CDRom)
                    return false;
                
                if (fileInfo == null)
                    return true;

                fileInfo.Refresh();

                // if we have a volume serial then check if the right media is inserted
                // by verifying the volume serial number
                bool correctMedia = true;
                if (!String.IsNullOrEmpty(volume_serial))
                    correctMedia = DeviceManager.GetDiskSerial(fileInfo.DirectoryName) == volume_serial;

                // if the import path is online, we have the right media inserted and the file 
                // is not there, we assume it has been deleted, so return true
                if (importPath.IsAvailable && correctMedia && !fileInfo.Exists) 
                    return true;

                return false;
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
                // todo: how to handle iso's?
                if (IsDVD && _discid == null)                    
                    _discid = Utility.GetDiscIdString(fileInfo.DirectoryName);
                
                return _discid;            
            }
            set {
                _discid = value;
                commitNeeded = true;
            }
        }
        private string _discid;

        [DBFieldAttribute(Default = null)]
        public string FileHash {
            get {
                if (fileHash == null) {
                    if (IsAvailable && (VideoDiscFormat == VideoDiscFormat.Unknown))
                        FileHash = Utility.GetMovieHashString(fileInfo.FullName);
                }

                return fileHash;
            }
            set {
                fileHash = value;
                commitNeeded = true;
            }
        } private string fileHash;

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
            // make sure we have a dblocalmedia object
            if (obj == null || obj.GetType() != typeof(DBLocalMedia))
                return false;

            // make sure both objects are linked to a file
            DBLocalMedia otherLocalMedia = (DBLocalMedia)obj;
            if (otherLocalMedia.File == null || this.File == null)
                return false;

            // make sure we have the same file
            if (!this.File.FullName.Equals(otherLocalMedia.File.FullName))
                return false;
            
            // if we have a volume serial for either both, make sure they are equal
            if (this.VolumeSerial != otherLocalMedia.VolumeSerial)
                return false;

            // if we have a Disc Id for either both, make sure they are equal
            if (IsDVD) {
                if (this.DiscId != otherLocalMedia.DiscId)
                    return false;
            }

            return true;
        }

        public override int GetHashCode() {
            if (File != null)
                return (VolumeSerial + "|" + File.FullName + "|" + DiscId).GetHashCode();

            return base.GetHashCode();
        }

        public override string ToString() {
            if (File != null)
                return File.Name;

            return base.ToString();
        }
        #endregion

        #region Database Management Methods

        public override void Delete() {
            if (deleting)
                return;

            deleting = true;
            logger.Info("Removing " + FullPath + " and associated movie.");
            foreach (DBMovieInfo currMovie in AttachedMovies) {
                foreach (DBLocalMedia otherFile in currMovie.LocalMedia) {
                    if (otherFile != this)
                        DBManager.Delete(otherFile);
                }
                currMovie.Delete();
            }

            base.Delete();
            deleting = false;
        }

        public static DBLocalMedia Get(string fullPath) {
            return Get(fullPath, null);
        }

        public static DBLocalMedia GetDVD(string fullPath, string discId) {
            DBField discIdField = DBField.GetField(typeof(DBLocalMedia), "DiscId");
            ICriteria criteria = new BaseCriteria(discIdField, "=", discId);
            List<DBLocalMedia> resultSet = MovingPicturesCore.DatabaseManager.Get<DBLocalMedia>(criteria);
            if (resultSet.Count > 0)
                return resultSet[0];

            DBLocalMedia newFile = new DBLocalMedia();
            newFile.FullPath = fullPath;

            return newFile;
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
    
    public class DBLocalMediaComparer : IComparer<DBLocalMedia> {
        public int Compare(DBLocalMedia fileX, DBLocalMedia fileY) {
            if (fileX.Part < fileY.Part)
                return -1;
            else
                return 1;
        }
    }

}
