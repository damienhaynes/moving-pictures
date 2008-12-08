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

        public DirectoryInfo Directory {
            get { return dirInfo; }

            set {
                dirInfo = value;
                commitNeeded = true;
            }
        }
        private DirectoryInfo dirInfo;

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

        #endregion

        #region Public Methods

        public List<DBLocalMedia> GetAllLocalMedia() {
            return GetLocalMedia(false);
        }

        public List<DBLocalMedia> GetNewLocalMedia() {
            return GetLocalMedia(true);
        }

        public static List<FileInfo> getFilesRecursive(DirectoryInfo inputDir) {
            List<FileInfo> fileList = new List<FileInfo>();
            DirectoryInfo[] childDirs = new DirectoryInfo[] { };

            try {
                fileList.AddRange(inputDir.GetFiles("*"));
                childDirs = inputDir.GetDirectories();
            }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;

                logger.Error("Error while retrieving files/directories for: " + inputDir.FullName, e);
            }

            foreach (DirectoryInfo currChildDir in childDirs) {
                try {
                    if ((currChildDir.Attributes & FileAttributes.System) == 0)
                        fileList.AddRange(getFilesRecursive(currChildDir));
                    else
                        logger.Debug("Rejecting directory " + currChildDir.FullName + " because it is flagged as a System folder.");
                }
                catch (Exception e) {
                    if (e.GetType() == typeof(ThreadAbortException))
                        throw e;
                    logger.Error("Error during attribute check for: " + currChildDir.FullName, e);
                }
            }

            return fileList;
        }


        public List<DBLocalMedia> GetLocalMedia(bool returnOnlyNew) {
            if (Directory == null)
                return null;

            logger.Debug("Starting scan for import path: {0}", Directory.FullName);
            string drive = Directory.Root.FullName;
            DriveInfo driveInfo = DeviceManager.GetDriveInfo(Directory);
            DriveType driveType = GetDriveType();
            string mediaLabel = null;

            int timeout = 0;
            if (driveInfo != null) {
                // If we have a DriveInfo object this means we are going 
                // to scan a logical disk (or mounted UNC)
                drive = driveInfo.Name;

                // Check if the drive is available before starting the scan
                while (!driveInfo.IsReady) {
                    // If not then wait it out with a timeout of 30 seconds (should be more then enough for optical media)
                    // before we cancel the scan or cancel immediatly when it's a fixed drive
                    if (timeout == 30 || driveType == DriveType.Fixed) {
                        logger.Error("Scan for '{0}' was cancelled because the drive is not available or empty.", Directory.FullName);
                        return null;
                    }
                    // wait one second
                    Thread.Sleep(1000);
                    timeout++;
                }
                mediaLabel = driveInfo.VolumeLabel;
            }
            else {
                // We do not have a DriveInfo object than we probably have a UNC path
                // we use some other logic here to detect if it's offline and wait for it to 
                // come online for 30 seconds.
                while (!Directory.Exists) {
                    if (timeout == 30 || Directory.Root.Exists) {
                        logger.Error("Scan for '{0}' was cancelled because the UNC path is not available.", Directory.FullName);
                        return null;
                    }
                    Thread.Sleep(1000);
                    timeout++;

                    // Refresh the state of the directory object
                    Directory.Refresh();
                }
            }

            // get disk serial
            string diskSerial = GetDiskSerial();
            logger.Debug("Drive: {0}, Type= {1}, Serial={2}", drive, driveType.ToString(), diskSerial);

            List<DBLocalMedia> rtn = new List<DBLocalMedia>();

            // grab the list of files and parse out appropriate ones based on extension
            try {
                List<FileInfo> fileList = getFilesRecursive(Directory);
                foreach (FileInfo currFile in fileList) {
                    DBLocalMedia newFile = DBLocalMedia.Get(currFile.FullName, diskSerial);

                    // if this file is in the database continue if we only want new files
                    if (newFile.ID != null && returnOnlyNew)
                        break;

                    if (newFile.IsVideo) {
                        // good extension for new file, so add it
                        logger.Debug("Pulling new file " + currFile.Name + " from import path.");
                        newFile.ImportPath = this;
                        rtn.Add(newFile);
                    }
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
            if (Directory != null) {
                DriveInfo driveInfo = DeviceManager.GetDriveInfo(Directory);
                if (driveInfo != null)
                    return driveInfo.DriveType;
            }
            return DriveType.Unknown;
        }

        public string GetVolumeLabel() {
            // this property won't be stored as it can differ in time
            if (Directory != null) {
                DriveInfo driveInfo = DeviceManager.GetDriveInfo(Directory);
                if (driveInfo != null)
                    if (driveInfo.IsReady)
                        return driveInfo.VolumeLabel;
                return null;
            }
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

        public static List<DBImportPath> GetAll() {
            return MovingPicturesCore.DatabaseManager.Get<DBImportPath>(null);
        }

        #endregion

    }
}