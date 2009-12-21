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

        /// <summary>
        /// Checks whether the file is a DVD.
        /// </summary>
        public bool IsDVD {
            get {
                if (this.VideoFormat == VideoFormat.DVD)
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Checks whether the file is a HD-DVD.
        /// </summary>
        public bool IsHDDVD {
            get {
                return (this.VideoFormat == VideoFormat.HDDVD);
            }
        }

        /// <summary>
        /// Checks whether the file is a Bluray.
        /// </summary>
        public bool IsBluray {
            get {
                return (this.VideoFormat == VideoFormat.Bluray);
            }
        }

        /// <summary>
        /// Checks whether the file is an entry path for a video disc.
        /// </summary>
        public bool IsVideoDisc {
            get {
                if (File != null)
                    return (VideoFormat != VideoFormat.Unknown && VideoFormat != VideoFormat.File);

                return false;
            }
        }

        /// <summary>
        /// Checks whether the file is an image file.
        /// </summary>
        public bool IsImageFile {
            get {
                if (File != null)
                    return Utility.IsImageFile(File.FullName);

                return false;
            }
        }

        /// <summary>
        /// Returns the state of the media (Online, NotMounted, Removed, Offline)
        /// </summary>
        public MediaState State {
            get {
                // Check if the path is available
                if (!this.IsAvailable) {
                    // if not available recheck if it has been removed entirely
                    if (this.IsRemoved)
                        return MediaState.Removed;
                    else
                        return MediaState.Offline;
                }

                // Check if the path is an image
                if (this.IsImageFile)
                    if (!this.IsMounted)
                        return MediaState.NotMounted;

                // Return that the file is ready to be played back
                return MediaState.Online;
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

                    // If this media is a DVD/Bluray in an optical drive
                    // double check the DiscID to make sure we are looking
                    // at the same disc. 
                    if (available && ( IsDVD || IsBluray ) && (ImportPath.GetDriveType() == DriveType.CDRom)) {
                        // Grab the current DiscID and compare it to the stored DiscID
                        string currentDiscID = this.VideoFormat.GetIdentifier(FullPath);
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
                if (ImportPath.IsOpticalDrive)
                    return false;

                if (fileInfo == null)
                    return true;

                // make sure the file info is refreshed
                fileInfo.Refresh();

                bool correctMedia = true;
                // if this not an UNC path we should check the volume serial
                // to check wether this is the correct media
                if (!importPath.IsUnc && VolumeSerial.Trim().Length > 0) {
                    correctMedia = (DeviceManager.GetVolumeSerial(fileInfo.DirectoryName) == VolumeSerial);
                }

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
                    return VideoUtility.IsVideoFile(fileInfo);
                else
                    return false;
            }
        }

        /// <summary>
        /// Check if the file is mounted
        /// </summary>
        public bool IsMounted {
            get {
                if (this.IsImageFile)
                    return Utility.IsMounted(this.FullPath);
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

        [DBFieldAttribute(AllowDynamicFiltering = false)]
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

        [DBFieldAttribute(AllowDynamicFiltering = false)]
        public string OriginalFileName {
            get {
                if (_originalFileName == " ")
                    return String.Empty;

                return _originalFileName;
            }

            set {
                _originalFileName = value;
                commitNeeded = true;
            }
        } private string _originalFileName;

        [DBFieldAttribute(Default = "", FieldName = "volume_serial", Filterable = false)]
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

        [DBFieldAttribute(Default = null, Filterable = false)]
        public string DiscId {
            get {
                // When this object is a DVD/Bluray and we don't have a Disc ID yet
                if (_discid == null && ( IsDVD || IsBluray ) ) {
                    // if this is an image file but not mounted
                    // skip the disc id utility
                    if (IsImageFile && !IsMounted)
                        return _discid;
                    
                    // Calculate the DiscID
                    DiscId = this.VideoFormat.GetIdentifier(FullPath);
                }
                
                // return the DiscID
                return _discid;            
            }
            set {
                _discid = value;
                commitNeeded = true;
            }
        } private string _discid;

        [DBFieldAttribute(Default = null, Filterable = false)]
        public string FileHash {
            get {
                if (fileHash == null && IsAvailable) {
                    // Only try to get file hashes when the format is File or when it's an image
                    if (this.VideoFormat == VideoFormat.File || this.IsImageFile)
                        FileHash = this.VideoFormat.GetIdentifier(FullPath);
                }
                return fileHash;
            }
            set {
                fileHash = value;
                commitNeeded = true;
            }
        } private string fileHash;

        [DBFieldAttribute(Default = "1", Filterable = false)]
        public int Part {
            get { return part; }
            set {
                part = value;
                commitNeeded = true;
            }
        } private int part;

        /// <summary>
        /// The duration of the video file in milliseconds
        /// </summary>
        [DBFieldAttribute(Default = "0", Filterable = false)]
        public int Duration {
            get { return _duration; }
            set {
                _duration = value;
                commitNeeded = true;
            }
        } private int _duration;

        [DBFieldAttribute(Default = "false", Filterable = false)]
        public bool Ignored {
            get { return ignored; }
            set {
                ignored = value;
                commitNeeded = true;
            }
        } private bool ignored;

        [DBFieldAttribute(Filterable=false)]
        public DBImportPath ImportPath {
            get { return importPath; }
            set {
                importPath = value;
                commitNeeded = true;
            }
        } private DBImportPath importPath;

        [DBRelation(AutoRetrieve = true, Filterable = false)]
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

        [DBFieldAttribute(AllowManualFilterInput = false)]
        public string VideoResolution {
            get { return _videoResolution; }
            set {
                _videoResolution = value;
                commitNeeded = true;
            }
        } private string _videoResolution;

        [DBFieldAttribute(AllowManualFilterInput=false)]
        public string VideoCodec {
            get { return _videoCodec; }
            set {
                _videoCodec = value;
                commitNeeded = true;
            }
        } private string _videoCodec;

        [DBFieldAttribute(AllowManualFilterInput = false)]
        public float VideoFrameRate {
            get { return _videoFrameRate; }
            set {
                _videoFrameRate = value;
                commitNeeded = true;
            }
        } private float _videoFrameRate;

        [DBFieldAttribute(AllowManualFilterInput = false)]
        public string VideoAspectRatio {
            get { return _videoAspectRatio; }
            set {
                _videoAspectRatio = value;
                commitNeeded = true;
            }
        } private string _videoAspectRatio;

        [DBFieldAttribute(AllowManualFilterInput = false)]
        public string AudioCodec {
            get { return _audioCodec; }
            set {
                _audioCodec = value;
                commitNeeded = true;
            }
        } private string _audioCodec;

        [DBFieldAttribute]
        public string AudioChannels {
            get { return _audioChannels; }
            set {
                _audioChannels = value;
                commitNeeded = true;
            }
        } private string _audioChannels;

        [DBFieldAttribute(AllowDynamicFiltering = false)]
        public bool HasSubtitles {
            get { return _hasSubtitles; }
            set {
                _hasSubtitles = value;
                commitNeeded = true;
            }
        } private bool _hasSubtitles;

        [DBFieldAttribute(AllowManualFilterInput = false)]
        public VideoFormat VideoFormat {
            get {
                // Get the video format (for the first time or when an image is mounted)
                if (_videoFormat == VideoFormat.NotSupported || (_videoFormat == VideoFormat.Unknown && IsMounted))             
                    // store the format, so we can skip this check in the future
                    VideoFormat = VideoUtility.GetVideoFormat(this.GetVideoPath());             

                return _videoFormat;
            }
            set {
                _videoFormat = value;
                commitNeeded = true;
            }
        } private VideoFormat _videoFormat = VideoFormat.NotSupported;

        #endregion

        #region Public methods

        /// <summary>
        /// This method will update/overwrite volume information of this object
        /// with the current information from the import path.
        /// </summary>
        /// <returns></returns>
        public void UpdateVolumeInformation() {
            // we can only update when we have an import path.
            if (importPath != null) {
                VolumeSerial = importPath.GetVolumeSerial();
                MediaLabel = importPath.GetVolumeLabel();
            }
        }

        public bool HasMediaInfo {
            get {
                // check for invalid data
                if (this.VideoCodec.Trim().Length == 0)
                    return false;
                if (this.VideoAspectRatio != "widescreen" && this.VideoAspectRatio != "fullscreen")
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Updates the MediaInfo fields using the fileinfo (does not mount disk image)
        /// </summary>
        /// <returns></returns>
        public UpdateMediaInfoResults UpdateMediaInfo() {
            return UpdateMediaInfo(false);
        }

        /// <summary>
        /// Updates the MediaInfo fields using the fileinfo (can mount disk image if needed)
        /// </summary>
        /// <param name="mountImage">If true automounts disk images to grab MediaInfo</param>
        /// <returns></returns>
        public UpdateMediaInfoResults UpdateMediaInfo(bool mountImage) {
            MediaState state = this.State;
            switch (state) {
                case MediaState.Offline:
                    return UpdateMediaInfoResults.MediaNotAvailable;
                case MediaState.Removed:
                    return UpdateMediaInfoResults.GeneralError;
                case MediaState.NotMounted:
                    if (mountImage) {
                        if (this.Mount() != MountResult.Success)
                            return UpdateMediaInfoResults.GeneralError;
                        else
                            break;
                    }
                    else {
                        return UpdateMediaInfoResults.ImageFileNotMounted;
                    }
            }

            // Get the path to the video file
            string videoPath = this.GetVideoPath();           
            
            // Start to update media info
            try {
                logger.Debug("Updating media info for '{0}'", videoPath);
                MediaInfoWrapper mInfoWrapper = this.VideoFormat.GetMediaInfo(videoPath);
                this.Duration = mInfoWrapper.Duration;
                this.VideoWidth = mInfoWrapper.Width;
                this.VideoHeight = mInfoWrapper.Height;
                this.VideoFrameRate = (float)mInfoWrapper.Framerate;
                this.HasSubtitles = mInfoWrapper.HasSubtitles;
                this.VideoCodec = mInfoWrapper.VideoCodec;
                this.AudioCodec = mInfoWrapper.AudioCodec;
                this.VideoResolution = mInfoWrapper.VideoResolution;
                this.AudioChannels = mInfoWrapper.AudioChannelsFriendly;
                this.AudioCodec = mInfoWrapper.AudioCodec;
                this.VideoAspectRatio = mInfoWrapper.AspectRatio;

                return UpdateMediaInfoResults.Success;
            }
            catch (Exception ex) {
                logger.DebugException("", ex);
                return UpdateMediaInfoResults.GeneralError;
            }
        }

        /// <summary>
        /// Mounts the file
        /// </summary>
        /// <returns></returns>
        public MountResult Mount() {
            return Utility.MountImage(this.FullPath);
        }

        /// <summary>
        /// Unmount the file if necessary
        /// </summary>
        public void UnMount() {
            Utility.UnMount(this.FullPath);
        }

        /// <summary>
        /// Gets the path to the video file. If this object is an image and it is mounted it will
        /// return the path to the main video file on the mounted image.
        /// </summary>
        /// <returns>video path</returns>
        public string GetVideoPath() {
            // if we are dealing with a mounted path
            if (this.IsMounted)
                // return the path to the video on the mounted image
                return Utility.GetMountedVideoDiscPath(this.FullPath);

            // by default return the original file path
            return this.FullPath;
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
            if (IsDVD && this.DiscId != otherLocalMedia.DiscId)
                 return false;

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
            logger.Info("Removing '{0}' and associated movie.", FullPath);
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
            return Get(fullPath, string.Empty);
        }

        public static DBLocalMedia GetDisc(string fullPath, string discId) {
            List<DBLocalMedia> resultSet = GetEntriesByDiscId(discId);
            if (resultSet.Count > 0)
                return resultSet[0];

            DBLocalMedia newFile = new DBLocalMedia();
            newFile.FullPath = fullPath;

            return newFile;
        }

        /// <summary>
        /// Returns a list of DBLocalMedia objects that match the filehash
        /// </summary>
        /// <param name="filehash"></param>
        /// <returns></returns>
        public static List<DBLocalMedia> GetEntriesByHash(string filehash) {
            DBField fileHashField = DBField.GetField(typeof(DBLocalMedia), "FileHash");
            ICriteria criteria = new BaseCriteria(fileHashField, "=", filehash);
            return MovingPicturesCore.DatabaseManager.Get<DBLocalMedia>(criteria);
        }

        /// <summary>
        /// Returns a list of DBLocalMedia objects that match the discid
        /// </summary>
        /// <param name="filehash"></param>
        /// <returns></returns>
        public static List<DBLocalMedia> GetEntriesByDiscId(string discId) {
            DBField discIdField = DBField.GetField(typeof(DBLocalMedia), "DiscId");
            ICriteria criteria = new BaseCriteria(discIdField, "=", discId);
            return MovingPicturesCore.DatabaseManager.Get<DBLocalMedia>(criteria);
        }

        public static DBLocalMedia Get(string fullPath, string volumeSerial) {
            List<DBLocalMedia> resultSet = GetAll(fullPath, volumeSerial);
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

        public static List<DBLocalMedia> GetAll(string fullPath, string volumeSerial) {
            
            DBField pathField = DBField.GetField(typeof(DBLocalMedia), "FullPath");
            // using operator LIKE to make the search case insensitive
            ICriteria pathCriteria = new BaseCriteria(pathField, "like", fullPath);

            ICriteria criteria = pathCriteria;
            if (volumeSerial != null) {
                DBField serialField = DBField.GetField(typeof(DBLocalMedia), "VolumeSerial");
                ICriteria serialCriteria = new BaseCriteria(serialField, "=", volumeSerial);
                criteria = new GroupedCriteria(pathCriteria, GroupedCriteria.Operator.AND, serialCriteria);
            }

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

    public class DBLocalMediaPathComparer : IComparer<DBLocalMedia> {
        public int Compare(DBLocalMedia mediaX, DBLocalMedia mediaY) {
            
            if (mediaX == null && mediaY == null)
                return 0;
            else if (mediaX == null)
                return -1;
            else if (mediaY == null)
                return 1;

            return mediaX.FullPath.CompareTo(mediaY.FullPath);
        }
    }

    public enum UpdateMediaInfoResults {
        Success,
        ImageFileNotMounted,
        MediaNotAvailable,
        GeneralError
    }

    public enum MediaState {
        Offline,
        Removed,
        NotMounted,
        Online
    }

}
