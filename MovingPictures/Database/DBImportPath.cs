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

        public bool IsUnc {
            get { 
                if (dirInfo == null) 
                    return true;

                return DeviceManager.PathIsUnc(dirInfo.FullName);
                }
        }

        public bool IsAvailable {
            get {
                if (dirInfo == null) 
                    return false;

                // basic check to see if the path is online
                if (!DeviceManager.IsAvailable(dirInfo))
                    return false;

                // we can get to the root folder, if this is a reparse point make sure the 
                // contents are currently accessible 
                try {
                    if ((dirInfo.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                        dirInfo.GetDirectories();

                    // directory access successful, disk is online
                    return true;
                }
                catch (Exception e) {
                    if (e is ThreadAbortException)
                        throw e;

                    // failed to look in the director, so it's not available
                    return false;
                }
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

        /// <summary>
        /// Returns true if this import path represents an optical drive.
        /// </summary>
        public bool IsOpticalDrive {
            get {
                if (_isOpticalDrive == null)
                    _isOpticalDrive = DeviceManager.IsOpticalDrive(this.FullPath);

                return (bool) _isOpticalDrive;
            }
        } private bool? _isOpticalDrive = null;

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
                    return MovingPicturesCore.Settings.AutomaticallyImportDVDs;
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
            logger.Debug("Scanning: {0}", Directory.FullName);
            
            // default values
            string volume = string.Empty;
            string label = string.Empty;
            string serial = string.Empty;

            // validate the import path
            if (this.IsAvailable) {
                if (!this.IsUnc) {
                    // Logical volume (can be a mapped network share)
                    int retry = 0;
                    while (serial == string.Empty) {
                        
                        // Grab information for this logical volume
                        VolumeInfo volumeInfo = DeviceManager.GetVolumeInfo(Directory);
                        if (volumeInfo != null) {
                            // get the volume properties
                            volume = volumeInfo.Name;
                            label = volumeInfo.Label;
                            serial = volumeInfo.Serial;
                        }

                        // check if the serial is empty
                        // logical volumes SHOULD have a serial number or something went wrong
                        if (serial == string.Empty) {
                            
                            // If we tried 3 times already then we should report a failure 
                            if (retry == 3) {
                                logger.Error("Canceled scan for '{0}': Could not get required volume information.", Directory.FullName);
                                return null;
                            }

                            // up the retry count and wait for 1 second
                            retry++;
                            Thread.Sleep(1000);
                            logger.Debug("Retrying: {1} ({0})", Directory.FullName, retry);
                        }
                    }
                } // todo: for UNC paths we could consider using the host part of the UNC path as the MediaLabel (ex. '//SomeHost/../..' => 'SomeHost')
            }
            else {
                if (this.IsRemovable) {
                    if (this.IsUnc) {
                        // network share
                        logger.Info("Skipping scan for '{0}': the share is offline.", Directory.FullName);
                    }
                    else if (this.IsOpticalDrive) {
                        // optical drive
                        logger.Info("Skipping scan for '{0}': the drive is empty.", Directory.FullName);
                    } 
                    else {
                        // all other removable paths
                        logger.Info("Skipping scan for '{0}': the volume is disconnected.", Directory.FullName);
                    }
                }
                else {
                    logger.Error("Scan for '{0}' was cancelled because the import path is not available.", Directory.FullName);
                }
                
                // returning nothing
                return null;
            }

            // Grab the list of files and validate them
            List<DBLocalMedia> rtn = new List<DBLocalMedia>();
            try {
                List<FileInfo> fileList = VideoUtility.GetVideoFilesRecursive(Directory);
                foreach (FileInfo videoFile in fileList) {

                    DBLocalMedia newFile = DBLocalMedia.Get(videoFile.FullName, serial);

                    // The file is in the database
                    if (newFile.ID != null) {

                        // for optical paths + DVDs we have to check the actual DiscId
                        if (IsOpticalDrive) {
                            if (newFile.IsDVD && !newFile.IsAvailable) {
                                string discId = Utility.GetDiscIdString(videoFile.FullName);
                                // Create/get a DBLocalMedia object using the the DiscID
                                newFile = DBLocalMedia.GetDVD(videoFile.FullName, discId);
                            }
                        }

                        // If the file is still in the database continue if we only want new files
                        if (newFile.ID != null && returnOnlyNew)
                            continue;
                    }

                    logger.Debug("New File: {0}", videoFile.Name);
                    newFile.ImportPath = this;

                    // Fill in the logical volume details (are both empty when dealing with UNC)
                    newFile.VolumeSerial = serial;
                    newFile.MediaLabel = label;
                    rtn.Add(newFile);
                }
            }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;
                logger.ErrorException("Error scanning " + Directory.FullName, e);
            }

            return rtn;
        }
        
        public DriveType GetDriveType() {
            // this property won't be stored as it can differ in time
            if (Directory != null) {
                if (DeviceManager.PathIsUnc(Directory.FullName))
                    return DriveType.Network;
                else {
                    VolumeInfo volume = DeviceManager.GetVolumeInfo(Directory);
                    if (volume != null)
                        return volume.Type;
                }
            }
            return DriveType.Unknown;
        }

        public string GetVolumeLabel() {
            // this property won't be stored as it can differ in time
            if (Directory != null)
                return DeviceManager.GetVolumeLabel(Directory);
            else
                return string.Empty;
        }

        public string GetVolumeSerial() {
            // this property won't be stored as it can differ in time
            if (Directory != null)
                return DeviceManager.GetVolumeSerial(Directory);
            else
                return string.Empty;
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