using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.Database {
    [DBTableAttribute("import_path")]
    public class DBImportPath : MovingPicturesDBTable {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public DBImportPath()
            : base() {
            dirInfo = null;
        }

        public override void AfterDelete() {
        }

        public bool IsAvailable {
            get {
                if (dirInfo != null)
                    return DeviceManager.IsAvailable(dirInfo);
                else
                    return false;
            }
        }

        public bool IsRemovable {
            get {
                if (dirInfo != null)
                    return DeviceManager.IsRemovable(dirInfo);
                else
                    return false;
            }
        }

        public string Volume {
            get {
                return DeviceManager.GetVolume(dirInfo);
            }
        }

        public DirectoryInfo Directory {
            get { return dirInfo; }

            set {
                dirInfo = value;
                commitNeeded = true;
            }
        }
        private DirectoryInfo dirInfo;

        /// <summary>
        /// Gets a value indicating wether this path is active.
        /// </summary>
        public bool Active {
            get {
                // Paths that are not of CDRom type are always active
                if (this.GetDriveType() != DriveType.CDRom)
                    return true;
                else // If CDRom type check the configuration setting 
                    return (bool)MovingPicturesCore.SettingsManager["importer_disc_enabled"].Value;
             }
        }

        #region Database Fields

        [DBFieldAttribute(FieldName = "path")]
        public string FullPath {
            get {
                if (dirInfo == null)
                    return "";

                return dirInfo.FullName;
            }

            set {
                if (value.Trim() == "")
                    dirInfo = null;
                else
                    dirInfo = new DirectoryInfo(value);

                commitNeeded = true;
            }
        }

        [DBFieldAttribute(Default = "false")]
        public bool InternallyManaged {
            get { return _internallyManaged;}
            set {_internallyManaged = value;}
        }
        private bool _internallyManaged;

        #endregion

        #region Public Methods

        public List<DBLocalMedia> GetAllLocalMedia() {
            return GetLocalMedia(false);
        }

        public List<DBLocalMedia> GetNewLocalMedia() {
            return GetLocalMedia(true);
        }     

        public List<DBLocalMedia> GetLocalMedia(bool returnOnlyNew) {
            logger.Debug("Starting scan for import path: {0}", Directory.FullName);
            string volume = null;
            string label = null;
            string serial = null;
            DriveType type = DriveType.Unknown;
            if (IsAvailable) {                
                VolumeInfo vi = DeviceManager.GetVolumeInfo(Directory);
                if (vi != null) {
                    volume = vi.Drive.Name;
                    label = vi.Label;
                    serial = vi.Serial;
                    type = vi.Drive.DriveType;
                }
                logger.Debug("Volume: {0}, Type= {1}, Serial={2}", volume, type, serial);
            }
            else {
                if (this.IsRemovable)
                    logger.Info("Scanning of removable import path '{0}' was skipped because it is not available.", Directory.FullName);
                else
                    logger.Error("Scan for '{0}' was cancelled because the import path is not available.", Directory.FullName);

                return null;
            }

            List<DBLocalMedia> rtn = new List<DBLocalMedia>();

            // grab the list of files and parse out appropriate ones based on extension
            try {
                List<FileInfo> fileList = Utility.GetVideoFilesRecursive(Directory);
                foreach (FileInfo videoFile in fileList) {
                    DBLocalMedia newFile = DBLocalMedia.Get(videoFile.FullName, serial);

                    // if this file is in the database continue if we only want new files
                    if (newFile.ID != null && returnOnlyNew)
                        continue;

                    logger.Debug("Pulling new file " + videoFile.Name + " from import path.");
                    newFile.ImportPath = this;

                    // we could use the UpdateDiskInformation() method but because we already have the information
                    // let's just fill the properties manually
                    // newFile.UpdateDiskInformation();
                    newFile.VolumeSerial = serial;
                    newFile.MediaLabel = label;
                    rtn.Add(newFile);
                }
            }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;
                logger.Error("Error scanning " + Directory.FullName, e);
            }

            return rtn;
        }


        public DriveType GetDriveType() {
            // this property won't be stored as it can differ in time
            if (Directory != null) {
                VolumeInfo vi = DeviceManager.GetVolumeInfo(Directory);
                if (vi != null)
                    return vi.Drive.DriveType;
            }
            return DriveType.Unknown;
        }

        public string GetVolumeLabel() {
            // this property won't be stored as it can differ in time
            if (Directory != null)
                return DeviceManager.GetVolumeLabel(Directory);
            else
                return null;
        }

        public string GetDiskSerial() {
            // this property won't be stored as it can differ in time
            if (Directory != null)
                return DeviceManager.GetDiskSerial(Directory);
            else
                return null;
        }

        public override string ToString() {
            return FullPath;
        }


        #endregion

        // Unfortunately static methods don't work quite right with inheritance, there is
        // no way to determine the superclass, so these can't be placed in the base class.
        // If you have an idea of how to move this to DatabaseTable.cs, by all means let
        // me know.
        #region Database Management Methods

        // Gets the cooresponding Setting based on the given record ID.
        public static DBImportPath Get(int id) {
            return MovingPicturesCore.DatabaseManager.Get<DBImportPath>(id);
        }

        // Gets the corresponding DBImportPath object using it's FullPath property
        public static DBImportPath Get(string fullPath) {
            DBField pathField = DBField.GetField(typeof(DBImportPath), "FullPath");
            ICriteria criteria = new BaseCriteria(pathField, "like", fullPath);
            List<DBImportPath> resultSet = MovingPicturesCore.DatabaseManager.Get<DBImportPath>(criteria);
            if (resultSet.Count > 0)
                return resultSet[0];

            DBImportPath newImportPath = new DBImportPath();
            newImportPath.FullPath = fullPath;

            return newImportPath;
        }

        public static List<DBImportPath> GetAll() {
            return MovingPicturesCore.DatabaseManager.Get<DBImportPath>(null);
        }

        /// <summary>
        /// Returns all user defined import paths
        /// </summary>
        public static List<DBImportPath> GetAllUserDefined() {
            List<DBImportPath> paths = new List<DBImportPath>();
            foreach (DBImportPath path in DBImportPath.GetAll()) {
                if (!path.InternallyManaged)
                    paths.Add(path);
            }
            return paths;
        }

        #endregion

    }
}