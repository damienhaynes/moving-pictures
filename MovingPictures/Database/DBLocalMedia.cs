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
        /// Checks whether the file is a HD-DVD.
        /// </summary>
        public bool IsHDDVD {
            get {
                return (VideoDiscFormat == VideoDiscFormat.HDDVD);
            }
        }

        /// <summary>
        /// Checks whether the file is a Bluray.
        /// </summary>
        public bool IsBluray {
            get {
                return (VideoDiscFormat == VideoDiscFormat.Bluray);
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
        /// Checks whether the file is an image file.
        /// </summary>
        public bool IsImageFile {
            get {
                if (File != null)
                    return DaemonTools.IsImageFile(Path.GetExtension(File.FullName));

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

        [DBFieldAttribute(Default = null, FieldName = "volume_serial", Filterable = false)]
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

        [DBFieldAttribute(Default = null, Filterable = false)]
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

        [DBFieldAttribute(Default = "1", Filterable = false)]
        public int Part {
            get { return part; }
            set {
                part = value;
                commitNeeded = true;
            }
        }
        private int part;

        [DBFieldAttribute(Default = "0", Filterable = false)]
        public int Duration {
            get { return _duration; }
            set {
                _duration = value;
                commitNeeded = true;
            }
        }
        private int _duration;

        [DBFieldAttribute(Default = "false", Filterable = false)]
        public bool Ignored {
            get { return ignored; }
            set {
                ignored = value;
                commitNeeded = true;
            }
        }
        private bool ignored;

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
                if (this.VideoAspectRatio != "widescreen" && this.VideoAspectRatio != "fullscreen")
                    return false;

                return true;
            }
        }

        public UpdateMediaInfoResults UpdateMediaInfo() {
            try {
                if (!this.IsAvailable) return UpdateMediaInfoResults.MediaNotAvailable;

                VideoDiscFormat mountedVideoDiscFormat = VideoDiscFormat.Unknown;
                if (!MovingPicturesCore.Settings.UseMediaInfo)
                    return UpdateMediaInfoResults.Success;

                string mediaPath = this.FullPath;

                if (this.IsDVD || this.IsHDDVD || this.IsBluray) {
                    mediaPath = Utility.GetMainFeatureStreamFromVideoDisc(mediaPath, this.VideoDiscFormat);
                    logger.Debug("Updating mediainfo for a {0} video disc.  mediapath={1}", this.VideoDiscFormat.ToString(), mediaPath);
                }
                else if (this.IsImageFile && DaemonTools.IsMounted(mediaPath)) {
                    string videoDiscPath = Utility.GetVideoDiscPath(DaemonTools.GetVirtualDrive());
                    mountedVideoDiscFormat = Utility.GetVideoDiscFormat(videoDiscPath);
                    mediaPath = Utility.GetMainFeatureStreamFromVideoDisc(videoDiscPath, mountedVideoDiscFormat);
                }
                else if (this.IsImageFile) {
                    // if it's an image file and it's not mounted
                    // we can't get the media info
                    return UpdateMediaInfoResults.ImageFileNotMounted;
                }

                string featureFilmFile;
                // because dvds have multiple files
                // we must build a list of all IFO files, and loop through them.
                // the best file wins

                // build list of files
                if (this.IsDVD || (this.IsImageFile && mountedVideoDiscFormat == VideoDiscFormat.DVD)) {
                    List<string> files = new List<string>();
                    files.AddRange(Directory.GetFiles(Path.GetDirectoryName(mediaPath), "*.ifo"));
                    featureFilmFile = FindFeatureFilm(files);
                    logger.Debug("Feature Film File: ", featureFilmFile);
                }
                else {
                    featureFilmFile = mediaPath;
                }

                Database.MediaInfoWrapper mInfoWrapper;
                mInfoWrapper = new Database.MediaInfoWrapper(featureFilmFile);
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
        /// Finds the most optimal file in a collection of files.
        /// Uses the aspect ratio, resolution, audio channel count, and duration to find the file
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private static string FindFeatureFilm(List<string> files) {
            if (files.Count == 1) return files[0];

            Dictionary<string, MediaInfoWrapper> mediaInfos = new Dictionary<string,MediaInfoWrapper>();
            foreach (string file in files)
	        {
                mediaInfos.Add(file, new Database.MediaInfoWrapper(file));
	        }
            
            // first filter out the fullscreen files if there are widescreen files present
            List<string> potentialFiles = new List<string>();
            foreach (var mediaInfo in mediaInfos)
	        {
                if (mediaInfo.Value.AspectRatio == "widescreen")
                    potentialFiles.Add(mediaInfo.Key);
	        }
            if (potentialFiles.Count == 0) potentialFiles.AddRange(files);
            if (potentialFiles.Count == 1) return potentialFiles[0];
            
            // next filter out by the highest resolution

            // find max height
            int maxHeight = 0;
            foreach (string file in potentialFiles)
	        {
                if (mediaInfos[file].Height > maxHeight)
                    maxHeight = mediaInfos[file].Height;
	        }

            // remove everything that is not max height
            for (int i = potentialFiles.Count-1; i >= 0; i--)
			{
                if (mediaInfos[potentialFiles[i]].Height != maxHeight)
                    potentialFiles.RemoveAt(i);
			}
            if (potentialFiles.Count == 1) return potentialFiles[0];

            // next filter by audio channel count
            // find max audio channel count
            int maxChannelCount = 0;
            foreach (string file in potentialFiles)
	        {
                if (mediaInfos[file].AudioChannels > maxChannelCount)
                    maxChannelCount = mediaInfos[file].AudioChannels;
	        }

            // remove everything that is not max channel count
            for (int i = potentialFiles.Count-1; i >= 0; i--)
			{
                if (mediaInfos[potentialFiles[i]].AudioChannels != maxChannelCount)
                    potentialFiles.RemoveAt(i);
			}

            // find max duration
            int maxDuration = 0;
            foreach (string file in potentialFiles) {
                if (mediaInfos[file].Duration > maxDuration)
                    maxDuration = mediaInfos[file].Duration;
            }
            // remove everything that is not max duration
            for (int i = potentialFiles.Count - 1; i >= 0; i--) {
                if (mediaInfos[potentialFiles[i]].Duration != maxDuration)
                    potentialFiles.RemoveAt(i);
            }
            
            return potentialFiles[0];
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

    public enum UpdateMediaInfoResults {
        Success,
        ImageFileNotMounted,
        MediaNotAvailable,
        GeneralError
    }
}
