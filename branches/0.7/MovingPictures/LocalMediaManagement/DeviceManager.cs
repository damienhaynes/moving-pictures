using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Management;
using System.Threading;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {

    /// <summary>
    /// Wrapper object for DriveInfo providing extended information using the WMI manager object.
    /// The WMI objects are cached as long as the drive state is valid.
    /// </summary>
    public class VolumeInfo {

        #region Private Variables

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private ManagementBaseObject managementObject;

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
                // If WMI information is not available
                // Refresh the object
                bool ready = driveInfo.IsReady;
                if (ready && managementObject == null)
                    Refresh();
                
                // Just relaying DriveInfo.IsReady for convience
                return ready;
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
        public DriveInfo Drive {
            get {
                return driveInfo;
            }
        } private DriveInfo driveInfo;
        
        /// <summary>
        /// Gets the value of VolumeSerialNumber
        /// </summary>
        public string Serial {
            get {
                if (!IsReady)
                    serial = null;

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
            Refresh();
        }

        // construct using volume (drive letter)
        public VolumeInfo(string volume) {
            driveInfo = new DriveInfo(volume);
            Refresh();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refresh the VolumeInfo object
        /// </summary>
        public void Refresh() {
            lock (driveInfo) {
                string driveLetter = driveInfo.Name.Substring(0, 2);
                if (driveInfo.IsReady) {
                    logger.Debug("Querying drive information for '{0}'", driveLetter);
                    try {
                        // Query WMI for extra disk information
                        SelectQuery query = new SelectQuery("select * from win32_logicaldisk where deviceid = '" + driveLetter + "'");
                        ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                        // this statement should return only one row (or none)
                        foreach (ManagementBaseObject mo in searcher.Get()) {
                            Refresh(mo);
                        }

                        // Return so the variables won't get reset after succesful refresh
                        return;
                    }
                    catch (Exception e) {
                        // Log the WMI query exception
                        logger.Debug("Error during query for '{0}', message: {1}", driveLetter, e.Message);
                    }
                }

                // reset the private variables if we make it this far
                logger.Debug("Drive '{0}' is not ready.", driveLetter);
                serial = null;
                managementObject = null;
            }
        }

        /// <summary>
        /// Refresh the VolumeInfo object with ManagementBaseObject information.
        /// </summary>
        /// <param name="mo"></param>
        private void Refresh(ManagementBaseObject mo) {
            // Update WMI management object
            managementObject = mo;
            
            // Update Volume Serial Number
            serial = null;
            if (managementObject != null)
                if (managementObject["volumeserialnumber"] != null)
                    serial = managementObject["volumeserialnumber"].ToString().Trim();

            logger.Debug("Succesfully updated drive information for '{0}'", driveInfo.Name.Substring(0, 2));
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
        }

        ~DeviceManager() {
            if (monitorStarted)
                StopMonitor();
        }

        #endregion

        #region Monitor

        private static void startMonitor() {
            if (monitor == null) {
                try {
                    monitor = new DeviceVolumeMonitor();
                    monitor.OnVolumeInserted += new DeviceVolumeAction(dvVolumeInserted);
                    monitor.OnVolumeRemoved += new DeviceVolumeAction(dvVolumeRemoved);
                    monitor.AsynchronousEvents = true;
                    monitor.Enabled = true;
                    monitorStarted = true;
                }
                catch (Exception e) {
                    logger.Debug("DeviceVolumeMonitor Error during startup: ", e.Message);
                    monitor = null;
                    monitorStarted = false;
                }
            }           
        }

        private static void stopMonitor() {
            if (monitor != null) {
                try {
                    monitor.Dispose();
                    monitor = null;
                    monitorStarted = false;
                }
                catch (Exception e) {
                    logger.Debug("Device Volume Monitor error during shutdown: ", e.Message);
                }
            }
        }

        private static void dvVolumeInserted(int bitMask) {
            // get volume letter
            string volume = monitor.MaskToLogicalPaths(bitMask);

            // get the volume information object
            VolumeInfo volumeInfo = GetVolumeInfo(volume);

            // if the volume is not ready yet wait for it
            while (!volumeInfo.IsReady)
                Thread.Sleep(100);

            // invoke event
            invokeOnVolumeInserted(volume, volumeInfo.Serial);
        }

        private static void dvVolumeRemoved(int bitMask) {
            // get volume letter
            string volume = monitor.MaskToLogicalPaths(bitMask);
            
            // refresh volume cache
            VolumeInfo volumeInfo = GetVolumeInfo(volume);
            volumeInfo.Refresh();

            // invoke event
            invokeOnVolumeRemoved(volume, null);
        }

        #endregion

        #region Event Logic

        /// <summary>
        /// Invokes the OnVolumeInserted event
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="serial"></param>
        private static void invokeOnVolumeInserted(string volume, string serial) {
            logger.Debug("Event: OnVolumeInserted, Volume: {0}, Serial: {1}", volume, serial);
            if (OnVolumeInserted != null) {
                OnVolumeInserted(volume, serial);
            }            
        }

        /// <summary>
        /// Invokes the OnVolumeRemoved event
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="serial"></param>
        private static void invokeOnVolumeRemoved(string volume, string serial) {
            logger.Debug("Event: OnVolumeRemoved, Volume: {0}, Serial: {1}", volume, serial);
            if (OnVolumeRemoved != null) {
                OnVolumeRemoved(volume, serial);               
            }

        }

        #endregion
        
        #region Public Methods

        /// <summary>
        /// Start monitoring volume changes
        /// </summary>
        public static void StartMonitor() {
            logger.Info("Starting device monitor ...");
            startMonitor();

            // Check if the monitor is running
            if (monitorStarted) {
                // Report success
                logger.Info("Device monitor started.");
            }
            else {
                // Report failure
                logger.Error("Device monitor failed to start.");
            }
        }

        /// <summary>
        /// Stop monitoring volume changes
        /// </summary>
        public static void StopMonitor() {
            if (!monitorStarted) {
                logger.Debug("Device monitor was not running.");
                return;
            }

            // try to stop the monitor
            stopMonitor();

            // If monitor is still running something has gone wrong
            // detailed logging supplied by monitor object. We just report
            // a failure here.
            if (monitorStarted) {
                logger.Info("Device monitor failed to stop.");
            }
            else {
                logger.Info("Device monitor stopped.");
            }

        }
        #endregion

        #region Public Static Methods
        
        /// <summary>
        /// Gets the volume (driveletter) of this FileSystemInfo object
        /// </summary>
        /// <param name="fsInfo"></param>
        /// <returns></returns>
        public static string GetVolume(FileSystemInfo fsInfo) {
            if (fsInfo != null)
                return GetVolume(fsInfo.FullName);
            else
                return null;
        }

        /// <summary>
        /// Gets the volume (driveletter) of this path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetVolume(string path) {
            if (path == null)
                return path;

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
            string volume = GetVolume(path);
            // if volume is null then it's UNC
            if (volume == null)
                return null;

            // check if we have previously cached this instance
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
        /// Gets a value indicating if the FileInfo object is removed from disk.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="serial">if serial is specified the result is more reliable.</param>
        /// <returns>true if file doesn't exist but disk/root is available.</returns>
        public static bool IsRemoved(FileInfo file, string serial) {
            file.Refresh();
            // If we got a volume serial then compare and judge
            if (!String.IsNullOrEmpty(serial))
                if ((GetDiskSerial(file) == serial) && !file.Exists && file.Directory.Root.Exists)
                    return true;
                else
                    return false;
            
            // Backwards compatibility / UNC-only support
            if (!file.Exists && file.Directory.Root.Exists)
                return true;
            else
                return false;
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
            return IsRemovable(fsInfo.FullName);
        }

        /// <summary>
        /// Gets a value indicating wether this path is located on a removable drive type.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsRemovable(string path) {
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
                return volumeInfo.Drive.VolumeLabel;
            else
                return null;
        }

        #endregion

    }
}
