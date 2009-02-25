using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Management;
using System.Threading;
using NLog;
using MediaPortal.Plugins.MovingPictures.Database;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {

    /// <summary>
    /// Supplies basic information about the attached physical drive.
    /// </summary>
    public class VolumeInfo {

        #region Private Variables

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public Properties (Read-only)

        /// <summary>
        /// Gets a value indicating wether a volume (drive) is ready
        /// </summary>
        /// <remarks>
        /// Relays IsReady property from embedded DriveInfo object
        /// </remarks>
        public bool IsReady {
            get {
                return driveInfo.IsReady;
            }
        }

        /// <summary>
        /// Gets a value indicating wether this volume is of a removable type
        /// </summary>
        public bool IsRemovable {
            get {
                if (removable == null)
                    removable = (driveInfo.DriveType == DriveType.CDRom || driveInfo.DriveType == DriveType.Removable || driveInfo.DriveType == DriveType.Network);

                return (bool)removable;
            }
        } private bool? removable;

        /// <summary>
        /// Returns the DriveInfo object attached to this instance
        /// </summary>
        public DriveInfo DriveInfo {
            get {
                return driveInfo;
            }
        } private DriveInfo driveInfo;
        
        /// <summary>
        /// Gets the value of VolumeSerialNumber
        /// </summary>
        public string Serial {
            get {
                if (serial == null && driveInfo != null && driveInfo.IsReady)
                    RefreshSerial();

                return serial;
            }
        } private string serial;

        /// <summary>
        /// Gets the volume label of the drive
        /// </summary>
        public string Label {
            get {
                // if the drive is ready return the label
                if (driveInfo.IsReady)
                    return driveInfo.VolumeLabel;

                // if not return null
                return null;
            }
        }

        #endregion

        #region Contructors

        // construct using DriveInfo object
        public VolumeInfo(DriveInfo di) {
            driveInfo = di;
            RefreshSerial();
        }

        // construct using volume (drive letter)
        public VolumeInfo(string volume) {
            driveInfo = new DriveInfo(volume);
            RefreshSerial();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refresh the VolumeInfo object
        /// </summary>
        public void RefreshSerial() {
            lock (driveInfo) {
                
                string driveLetter = driveInfo.Name.Substring(0, 2);

                // Before we do request information check if the drive is ready.
                if (!driveInfo.IsReady) {
                    logger.Debug("Drive Information Update Failed: Drive {0} is not ready.", driveLetter);
                    serial = null;
                    return;
                }

                // Get Volume Serial Number Using WMI
                try {
                    ManagementObject disk = new ManagementObject("Win32_LogicalDisk.DeviceID='" + driveLetter + "'");
                    foreach (PropertyData diskProperty in disk.Properties) {
                        if (diskProperty.Name == "VolumeSerialNumber") {
                            serial = diskProperty.Value.ToString();
                            logger.Debug("Drive Information Updated: Drive={0}, Serial={1}", driveLetter, serial);
                            return;
                        }
                    }
                    
                    logger.Debug("Drive Information Update Failed: No Volume Serial Number Found. Drive={0}", driveLetter, serial);
                    serial = null;
                }
                catch (Exception e) {
                    // Catch Exceptions
                    logger.DebugException("Drive Information Update Failed: Drive=" + driveLetter, e);
                    serial = null;
                }
            }
        }

        #endregion       

    }
    
    /// <summary>
    /// Device Manager - needs proper description
    /// </summary>
    public class DeviceManager {

        #region Private Variables

        // Log object
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // Volume Information Cache
        private static Dictionary<string, VolumeInfo> volumes;
        
        // Monitor
        private static DeviceVolumeMonitor monitor;

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
            volumes = new Dictionary<string, VolumeInfo>();
            watchedDrives = new List<string>();
            
            watcherThread = new Thread(new ThreadStart(WatchDisks));
            watcherThread.Name = "DeviceManager.WatchDisks";
            
            driveStates = new Dictionary<string, bool>();
        }

        ~DeviceManager() {
            StopDiskWatcher();
        }

        #endregion

        #region Monitoring Logic

        public static void StartMonitor() {
            foreach (DBImportPath currPath in DBImportPath.GetAll()) {
                try {
                    if (currPath.IsRemovable)
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
            logger.Debug("Adding " + path + " to DiskWatcher.");
            try {
                string driveLetter = GetDriveLetter(path);
                lock (watchedDrives) {
                    if (IsRemovable(driveLetter) && !watchedDrives.Contains(driveLetter)) {
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
            lock (watchedDrives) {
                if (IsRemovable(driveLetter) && watchedDrives.Contains(driveLetter)) {
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
                if (!watcherThread.IsAlive) {
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
                            VolumeInfo volumeInfo = GetVolumeInfo(currDrive);
                            if (volumeInfo == null) isAvailable = false;
                            else isAvailable = volumeInfo.IsReady;

                            // if the previous drive state is not stored, store it and continue
                            if (!driveStates.ContainsKey(currDrive)) {
                                driveStates[currDrive] = isAvailable;
                                continue;
                            }

                            // if a change has occured
                            if (driveStates[currDrive] != isAvailable) {

                                // Refresh Serial
                                GetVolumeInfo(currDrive).RefreshSerial();

                                // notify any listeners
                                if (isAvailable) {
                                    logger.Info("Volume Inserted: " + currDrive);
                                    if (OnVolumeInserted != null)
                                        OnVolumeInserted(currDrive, GetDiskSerial(currDrive));
                                }
                                else {
                                    logger.Info("Volume Removed: " + currDrive);
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
            if (path == null)
                return null;

            // if the path is UNC return null
            if (path.StartsWith(@"/") || path.StartsWith(@"\"))
                return null;

            // return the first 2 characters
            if (path.Length > 1)
                return path.Substring(0, 2).ToUpper();
            else // or if only a letter was given add colon
                return path.ToUpper() + ":";
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
        /// Get the VolumeInfo object where this path is located.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static VolumeInfo GetVolumeInfo(string path) {
            string volume = GetDriveLetter(path);
            // if volume is null then it's UNC
            if (volume == null)
                return null;

            // if not loaded, load this VolumeInfo object
            if (!volumes.ContainsKey(volume)) {
                try {
                    lock (volumes) {
                        if (!volumes.ContainsKey(volume))
                            volumes.Add(volume, new VolumeInfo(volume));
                    }
                }
                catch (Exception e) {
                    logger.Error("Error adding drive '{0}' : {1}", volume, e.Message);
                }
            }

            // return from cache
            return volumes[volume];
        }
        
        /// <summary>
        /// Flushes the drive information cache for all drives
        /// </summary>
        public static void Flush() {
            volumes.Clear();
            logger.Debug("Flushed drive information cache for all drives.");
        }

        /// <summary>
        /// Flushes the drive information cache for the specified drive letter.
        /// </summary>
        /// <param name="driveletter">X:</param>
        public static void Flush(string volume) {
            if (volumes.ContainsKey(volume))
                volumes.Remove(volume);

            logger.Debug("Flushed drive information cache for '{0}'.", volume);
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
        public static bool IsAvailable(FileSystemInfo fsInfo, string serial) {         
            // Get Volume Info
            VolumeInfo volumeInfo = GetVolumeInfo(fsInfo);

            // Refresh the object information (important)
            fsInfo.Refresh();

            // do we have a disk serial?
            if (!String.IsNullOrEmpty(serial) && volumeInfo != null) {

                // do the disk serials match and does the file exist?
                if (volumeInfo.Serial == serial && fsInfo.Exists) {
                    return true; // file available.
                } else {
                    return false; // file not available.
                } 
            }
            else {
                // no serial so we have to do it the old way
                return fsInfo.Exists;
            }                    
                
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
            catch (Exception e) {
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
            if (path == null)
                return false;

            VolumeInfo volumeInfo = GetVolumeInfo(path);
            if (volumeInfo == null)
                return true; // true because it's UNC (=network)

            return volumeInfo.IsRemovable;
        }


        /// <summary>
        /// Gets the disk serial of the drive were the given FileSystemInfo object is located.
        /// </summary>
        /// <param name="fsInfo"></param>
        /// <returns></returns>
        public static string GetDiskSerial(FileSystemInfo fsInfo) {
            return GetDiskSerial(fsInfo.FullName);   
        }

        /// <summary>
        /// Gets the disk serial of the drive were the path is located.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetDiskSerial(string path) {
           VolumeInfo volumeInfo = GetVolumeInfo(path);
           if (volumeInfo != null)
               return volumeInfo.Serial;
           else
               return null;
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
            VolumeInfo volumeInfo = GetVolumeInfo(path);
            if (volumeInfo != null)
                return volumeInfo.DriveInfo.VolumeLabel;
            else
                return null;
        }

        #endregion

    }
}
