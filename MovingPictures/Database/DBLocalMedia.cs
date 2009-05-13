using System;
using System.Collections.Generic;
using System.IO;
using Cornerstone.Database;
using Cornerstone.Database.CustomTypes;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using NLog;
using MediaPortal.Player;
using MediaPortal.Util;

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
        /// Checks whether the file is a DVD.
        /// </summary>
        public bool IsDVD {
            get {
                if (VideoDiscFormat == VideoDiscFormat.DVD)
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Checks whether the file is an entry path for a video disc.
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
            get { return _duration; }
            set {
                _duration = value;
                commitNeeded = true;
            }
        }
        private int _duration;

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


        [DBFieldAttribute]
        public int VideoWidth {
            get { return _videoWidth; }
            set {
                _videoWidth = value;
                commitNeeded = true;
            }
        } private int _videoWidth;

        [DBFieldAttribute]
        public int VideoHeight {
            get { return _videoHeight; }
            set {
                _videoHeight = value;
                commitNeeded = true;
            }
        } private int _videoHeight;

        [DBFieldAttribute]
        public string VideoResolution {
            get { return _videoResolution; }
            set {
                _videoResolution = value;
                commitNeeded = true;
            }
        } private string _videoResolution;

        [DBFieldAttribute]
        public string VideoCodec {
            get { return _videoCodec; }
            set {
                _videoCodec = value;
                commitNeeded = true;
            }
        } private string _videoCodec;

        [DBFieldAttribute]
        public double VideoFrameRate {
            get { return _videoFrameRate; }
            set {
                _videoFrameRate = value;
                commitNeeded = true;
            }
        } private double _videoFrameRate;

        [DBFieldAttribute]
        public string VideoAspectRatio {
            get { return _videoAspectRatio; }
            set {
                _videoAspectRatio = value;
                commitNeeded = true;
            }
        } private string _videoAspectRatio;

        [DBFieldAttribute]
        public string AudioCodec {
            get { return _audioCodec; }
            set {
                _audioCodec = value;
                commitNeeded = true;
            }
        } private string _audioCodec;

        [DBFieldAttribute]
        public int AudioChannels {
            get { return _audioChannels; }
            set {
                _audioChannels = value;
                commitNeeded = true;
            }
        } private int _audioChannels;

        public string AudioChannelsFriendly {
            get {
                switch (this.AudioChannels) {
                    case 8:
                        return "7.1";
                    case 6:
                        return "5.1";
                    case 2:
                        return "stereo";
                    case 1:
                        return "mono";
                    default:
                        return this.AudioChannels.ToString();
                }
            }
        }

        [DBFieldAttribute]
        public bool HasSubtitles {
            get { return _hasSubtitles; }
            set {
                _hasSubtitles = value;
                commitNeeded = true;
            }
        } private bool _hasSubtitles;

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

        public bool HasMediaInfo {
            get {
                // check for invalid data
                if (this.VideoCodec.Trim().Length == 0)
                    return false;
                if (this.VideoAspectRatio != "16_9" && this.VideoAspectRatio != "4_3")
                    return false;

                return true;
            }
        }

        public void UpdateMediaInfo() {
            string mediaPath = this.FullPath;
            if (this.IsDVD) {
                mediaPath = Path.Combine(Path.GetDirectoryName(mediaPath), "VTS_01_0.IFO");
            }
            else if (DaemonTools.IsMounted(mediaPath)) {
                mediaPath = Path.Combine(DaemonTools.GetVirtualDrive(), @"VIDEO_TS\VTS_01_0.IFO");
            }
            else if (DaemonTools.IsImageFile(Path.GetExtension(mediaPath))) {
                // if it's an image file and it's not mounted
                // we can't get the media info
                return;
            }

            MediaInfoWrapper mInfoWrapper = new MediaInfoWrapper(mediaPath);
            this.VideoWidth = mInfoWrapper.Width;
            this.VideoHeight = mInfoWrapper.Height;
            this.VideoFrameRate = mInfoWrapper.Framerate;
            this.HasSubtitles = mInfoWrapper.HasSubtitles;

            if ((float)mInfoWrapper.Width / (float)mInfoWrapper.Height >= 1.4)
                this.VideoAspectRatio = "16_9";
            else
                this.VideoAspectRatio = "4_3";

            if (mInfoWrapper.IsDIVX)
                this.VideoCodec = "DIVX";
            else if (mInfoWrapper.IsXVID)
                this.VideoCodec = "XVID";
            else if (mInfoWrapper.IsH264)
                this.VideoCodec = "H264";
            else if (mInfoWrapper.IsMP1V)
                this.VideoCodec = "MP1V";
            else if (mInfoWrapper.IsMP2V)
                this.VideoCodec = "MP2V";
            else if (mInfoWrapper.IsWMV)
                this.VideoCodec = "WMV";
            else
                this.VideoCodec = mInfoWrapper.VideoCodec;

            if (mInfoWrapper.IsAC3 || mInfoWrapper.AudioCodec.ToLower().Contains("ac-3"))
                this.AudioCodec = "AC3";
            else if (mInfoWrapper.IsMP3)
                this.AudioCodec = "MP3";
            else if (mInfoWrapper.IsMP2A)
                this.AudioCodec = "MP2A";
            else if (mInfoWrapper.IsDTS)
                this.AudioCodec = "DTS";
            else if (mInfoWrapper.IsOGG)
                this.AudioCodec = "OGG";
            else if (mInfoWrapper.IsAAC)
                this.AudioCodec = "AAC";
            else if (mInfoWrapper.IsWMA)
                this.AudioCodec = "WMA";
            else if (mInfoWrapper.IsPCM)
                this.AudioCodec = "PCM";
            else
                this.AudioCodec = mInfoWrapper.AudioCodec;

            //if (mInfoWrapper.Is1080P)
            //    this.VideoResolution = "1080p";
            //else if (mInfoWrapper.Is1080I)
            //    this.VideoResolution = "1080i";
            //else if (mInfoWrapper.Is720P)
            //    this.VideoResolution = "720p";
            //else if (mInfoWrapper.IsHDTV)
            //    this.VideoResolution = "HD";
            //else
            //    this.VideoResolution = "SD";


            // get duration
            // duration is not currently included in the MediaInfoWrapper,
            // so we must call MediaInfo directly.
            MediaInfo mInfo = new MediaInfo();
            try {
                int intValue;
                mInfo.Open(mediaPath);
                if (int.TryParse(mInfo.Get(StreamKind.Video, 0, "PlayTime"), out intValue))
                    this.Duration = intValue;
            }
            finally {
                if (mInfo != null)
                    mInfo.Close();
            }


            // Get audio channel count.
            // because dvds have multiple files with multiple audio streams
            // we must build a list of all files, and loop through them.
            // the stream with the highest number of audio channels wins.

            // build list of files
            List<string> files = new List<string>();
            if (this.IsDVD || DaemonTools.IsMounted(this.FullPath)) {
                files.AddRange(Directory.GetFiles(Path.GetDirectoryName(mediaPath), "*.ifo"));
            }
            else {
                files.Add(mediaPath);
            }

            // get highest audio channel count
            foreach (var file in files) {
                try {
                    int intValue;
                    mInfo.Open(file);

                    bool _isInterlaced = mInfo.Get(StreamKind.Video, 0, "ScanType").ToLower().Contains("interlaced");
                    if ((mInfoWrapper.Width == 1920 || mInfoWrapper.Height == 1080) && !_isInterlaced)
                        this.VideoResolution = "1080p";
                    else if ((mInfoWrapper.Width == 1920 || mInfoWrapper.Height == 1080) && _isInterlaced)
                        this.VideoResolution = "1080i";
                    else if ((mInfoWrapper.Width == 1280 || mInfoWrapper.Height == 720) && !_isInterlaced)
                        this.VideoResolution = "720p";
                    else if (mInfoWrapper.Height >= 720)
                        this.VideoResolution = "HD";
                    else
                        this.VideoResolution = "SD";

                    int iAudioStreams = mInfo.Count_Get(StreamKind.Audio);
                    for (int i = 0; i < iAudioStreams - 1; i++) {
                        if (int.TryParse(mInfo.Get(StreamKind.Audio, i, "Channel(s)"), out intValue)
                            && intValue > this.AudioChannels)
                            this.AudioChannels = intValue;
                    }
                }
                finally {
                    if (mInfo != null)
                        mInfo.Close();
                }
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
