using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {
    
    /// <summary>
    /// Device Manager - needs proper description
    /// </summary>
    public class DeviceManager {

        #region Private Variables

        // Log object
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly object lockObject = new object();

        #endregion

        #region Events / Delegates

        public delegate void DeviceManagerEvent(string volume, string serial);
        public static event DeviceManagerEvent OnVolumeInserted;
        public static event DeviceManagerEvent OnVolumeRemoved;

        private static List<string> watchedDrives;
        private static Thread watcherThread;
        private static Dictionary<string, bool> driveStates;

        #endregion

        #region Public Properties

        /// <summary>
        /// Check if the monitor is started
        /// </summary>
        public static bool MonitorStarted {
            get {
                return monitorStarted;
            }
        } private static bool monitorStarted = false;

        #endregion

        #region Constructor

        private DeviceManager() {

        }

        static DeviceManager() {
            lock (lockObject) {
                watchedDrives = new List<string>();

                watcherThread = new Thread(new ThreadStart(WatchDisks));
                watcherThread.Name = "DeviceManager.WatchDisks";

                driveStates = new Dictionary<string, bool>();
            }
        }

        ~DeviceManager() {
            StopDiskWatcher();
        }

        #endregion

        #region Monitoring Logic

        public static void StartMonitor() {
            
            // Setup listener for added ImportPaths
            MovingPicturesCore.DatabaseManager.ObjectInserted += new DatabaseManager.ObjectAffectedDelegate(onPathAdded);
            
            foreach (DBImportPath currPath in DBImportPath.GetAll()) {
                try {
                    AddWatchDrive(currPath.FullPath);
                }
                catch (Exception e) {
                    if (e is ThreadAbortException)
                        throw e;

                    logger.FatalException("Failed adding " + currPath + " to the Disk Watcher!", e);
                }
            }

            StartDiskWatcher();
        }

        public static void StopMonitor() {
            StopDiskWatcher();
            ClearWatchDrives();
        }

        public static void AddWatchDrive(string path) {
            try {
                // if the path does not point to logical volume do not add it to the drive watcher
                if (PathIsUnc(path))
                    return;

                // get the driveletter
                string driveLetter = GetDriveLetter(path);
                if (driveLetter == null)
                    return;

                // add the drive to the drive watcher
                lock (watchedDrives) {
                    if (!watchedDrives.Contains(driveLetter)) {
                        watchedDrives.Add(driveLetter);
                        StartDiskWatcher();
                        logger.Info("Added " + driveLetter + " to DiskWatcher");
                    }
                }
            }
            catch (Exception e) {
                if (e is ThreadAbortException)
                    throw e;

                logger.FatalException("Error adding \"" + path + "\" to Disk Watcher!!", e);
            }
        }

        public static void RemoveWatchDrive(string path) {
            string driveLetter = GetDriveLetter(path);
            if (driveLetter == null)
                return;

            lock (watchedDrives) {
                if (watchedDrives.Contains(driveLetter)) {
                    watchedDrives.Remove(driveLetter);
                    logger.Info("Removed " + driveLetter + " from DiskWatcher");
                    if (watchedDrives.Count == 0)
                        StopDiskWatcher();
                }
            }

        }

        public static void ClearWatchDrives() {
            lock (watchedDrives) {
                logger.Info("Clearing all drives from Disk Watcher");
                watchedDrives.Clear();
                StopDiskWatcher();
            }
        }

        public static void StartDiskWatcher() {
            lock (watcherThread) {
                if (MovingPicturesCore.Settings.DeviceManagerEnabled && !watcherThread.IsAlive) {
                    logger.Info("Starting Disk Watcher");
                    watcherThread = new Thread(new ThreadStart(WatchDisks));
                    watcherThread.Name = "DeviceManager.WatchDisks";
                    watcherThread.Start();
                }
            }
        }

        public static void StopDiskWatcher() {
            lock (watcherThread) {
                if (watcherThread.IsAlive) {
                    logger.Info("Stopping Disk Watcher");
                    watcherThread.Abort();
                }
            }
        }

        private static void WatchDisks() {
             while (true) {
                lock (watchedDrives) {
                    foreach (string currDrive in watchedDrives) {
                        try {
                            // check if the drive is available
                            bool isAvailable;
                            VolumeInfo volume = VolumeInfo.Get(currDrive);
                            if (volume == null) isAvailable = false;
                            else isAvailable = volume.IsAvailable;

                            // if the previous drive state is not stored, store it and continue
                            if (!driveStates.ContainsKey(currDrive)) {
                                driveStates[currDrive] = isAvailable;
                                continue;
                            }

                            // if a change has occured
                            if (driveStates[currDrive] != isAvailable) {

                                // notify any listeners
                                if (isAvailable) {
                                    
                                    // Refresh Serial
                                    volume.Refresh();

                                    if (volume.Type != DriveType.Network)
                                        logger.Info("Volume Inserted: " + currDrive);
                                    else
                                        logger.Info("Volume Online: " + currDrive);

                                    if (OnVolumeInserted != null)
                                        OnVolumeInserted(currDrive, volume.Serial);
                                }
                                else {
                                    // if a mapped network share gets disconnected it can either show Network or NoRootDirectory
                                    if (volume.Type != DriveType.Network && volume.Type != DriveType.NoRootDirectory)
                                        logger.Info("Volume Removed: " + currDrive);
                                    else
                                        logger.Info("Volume Offline: " + currDrive);

                                    // Flush cache for this volume (just to be on the safe side)
                                    VolumeInfo.Flush(volume.Name);

                                    if (OnVolumeRemoved != null)
                                        OnVolumeRemoved(currDrive, null);
                                }

                                // update our state
                                driveStates[currDrive] = isAvailable;
                            }
                        }
                        catch (Exception e) {
                            if (e is ThreadAbortException)
                                throw e;

                            logger.ErrorException("Unexpected error in Disk Watcher thread!", e);
                        }
                    }
                }
                Thread.Sleep(5000);
            }
        }
        
        // Listens for new import paths and adds them to the DiskWatcher
        private static void onPathAdded(DatabaseTable obj) {
            // If this is not an import path object break
            if (obj.GetType() != typeof(DBImportPath))
                return;

            // Add the new import path to the watched drives
            AddWatchDrive(((DBImportPath)obj).FullPath);
        }

        #endregion

        #region Public Static Methods

        // Grab the drive letter from a FileSystemInfo object.
        public static string GetDriveLetter(FileSystemInfo fsInfo) {
            if (fsInfo != null)
                return GetDriveLetter(fsInfo.FullName);
            else
                return null;
        }

        // Grab drive letter from string
        public static string GetDriveLetter(string path) {
            // if the path is UNC return null
            if (path != null && PathIsUnc(path))
                return null;

            // return the first 2 characters
            if (path.Length > 1)
                return path.Substring(0, 2).ToUpper();
            else // or if only a letter was given add colon
                return path.ToUpper() + ":";
        }
        
        /// <summary>
        /// Gets a value indicating wether the path is in UNC format.
        /// </summary>
        /// <param name="path">path to check</param>
        /// <returns>True, if it's a UNC path</returns>
        public static bool PathIsUnc(string path) {
            return (path != null && path.StartsWith(@"\\"));
   
        }

        /// <summary>
        /// Get the VolumeInfo object where this FileSystemInfo object is located.
        /// </summary>
        /// <param name="fsInfo"></param>
        /// <returns></returns>
        public static VolumeInfo GetVolumeInfo(FileSystemInfo fsInfo) {
            return GetVolumeInfo(fsInfo.FullName);
        }

        /// <summary>
        /// Gets the VolumeInfo object for this drive
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static VolumeInfo GetVolumeInfo(string path) {
            if (!PathIsUnc(path)) {
                string driveletter = GetDriveLetter(path);
                return VolumeInfo.Get(driveletter);
            }
            else {
                // not a logical volume (no driveinfo)
                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating if the FileSystemInfo object is currently available
        /// </summary>
        /// <param name="fsInfo"></param>
        /// <returns></returns>
        public static bool IsAvailable(FileSystemInfo fsInfo) {
            return IsAvailable(fsInfo, null);
        }

        /// <summary>
        /// Gets a value indicating if the FileSystemInfo object is currently available
        /// </summary>
        /// <param name="fsInfo"></param>
        /// <param name="serial">if a serial is specified the return value is more reliable.</param>
        /// <returns></returns>
        public static bool IsAvailable(FileSystemInfo fsInfo, string recordedSerial) {
            // Get Drive Information
            VolumeInfo volume = GetVolumeInfo(fsInfo);
            
            // Refresh the object information (important)
            fsInfo.Refresh();

            // Check if the file exists
            bool fileExists = fsInfo.Exists;

            // Do we have a logical volume?
            if (volume != null && fileExists) {
                string currentSerial = volume.Serial;
                // if we have both the recorded and the current serial we can do this very exact
                // by checking if the serials match
                if (!String.IsNullOrEmpty(recordedSerial) && !String.IsNullOrEmpty(currentSerial))
                    // return the exact check result
                    return (currentSerial == recordedSerial);
            }
            
           // return the simple check result
           return fileExists;   
        }

        /// <summary>
        /// Gets a value indicating wether this FileSystemInfo object is located on a removable drive type.
        /// </summary>
        /// <param name="fsInfo"></param>
        /// <returns></returns>
        public static bool IsRemovable(FileSystemInfo fsInfo) {
            try {
                if ((fsInfo.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                    return true;
            }
            catch (Exception) {
                logger.Warn("Failed check if " + fsInfo.FullName + " is a reparse point");
            }

            return IsRemovable(fsInfo.FullName);
        }

        /// <summary>
        /// Gets a value indicating wether this path is located on a removable drive type.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsRemovable(string path) {
            VolumeInfo volume = GetVolumeInfo(path);
            if (volume != null)
                return volume.DriveInfo.IsRemovable();
            else
                return true;
        }

        public static bool IsOpticalDrive(string path) {
            VolumeInfo volume = DeviceManager.GetVolumeInfo(path);
            if (volume != null)
                return volume.DriveInfo.IsOptical();
            else
                return false;
        }

        /// <summary>
        /// Gets the disk serial of the drive were the given FileSystemInfo object is located.
        /// </summary>
        /// <param name="fsInfo"></param>
        /// <returns></returns>
        public static string GetVolumeSerial(FileSystemInfo fsInfo) {
            return GetVolumeSerial(fsInfo.FullName);   
        }

        /// <summary>
        /// Gets the disk serial of the drive were the path is located.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetVolumeSerial(string path) {
            VolumeInfo volume = GetVolumeInfo(path);
            if (volume != null)
                return volume.Serial;
            else
                return string.Empty;
        }

        /// <summary>
        /// Gets the volume label of the drive were the given FileSystemInfo object is located.
        /// </summary>
        /// <param name="fsInfo"></param>
        /// <returns></returns>
        public static string GetVolumeLabel(FileSystemInfo fsInfo) {
            return GetVolumeLabel(fsInfo.FullName);
        }

        /// <summary>
        /// Gets the volume label of the drive were the path is located.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetVolumeLabel(string path) {
            VolumeInfo volume = GetVolumeInfo(path);
            if (volume != null)
                return volume.Label;
            else
                return string.Empty;
        }

        #endregion

    }
}
