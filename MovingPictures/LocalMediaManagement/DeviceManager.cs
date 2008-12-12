using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Management;
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
                if (driveInfo.IsReady && managementObject == null)
                    Refresh();
                
                // Just relaying DriveInfo.IsReady for convience
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
            if (driveInfo.IsReady) {
                try {
                    // Query WMI for extra disk information
                    SelectQuery query = new SelectQuery("select * from win32_logicaldisk where deviceid = '" + driveInfo.Name.Substring(0, 2) + "'");
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
                    logger.Debug("Error during WMI query for '{0}', message: {1}", driveInfo.Name.Substring(0, 2), e.Message);
                }                
            }

          // reset the private variables if we make it this far
          serial = null;
          managementObject = null;
        }

        /// <summary>
        /// Refresh the VolumeInfo object with ManagementBaseObject information.
        /// </summary>
        /// <param name="mo"></param>
        public void Refresh(ManagementBaseObject mo) {
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
        private static Dictionary<string, VolumeInfo> volumes = new Dictionary<string, VolumeInfo>();
        
        // Monitor Types
        private ManagementEventWatcher wmiMonitor;
        private DeviceVolumeMonitor dvMonitor;

        #endregion

        #region Events / Delegates

        public delegate void DeviceManagerEvent(string volume, string serial);
        public event DeviceManagerEvent OnVolumeInserted;
        public event DeviceManagerEvent OnVolumeRemoved;

        #endregion

        #region Public Properties

        /// <summary>
        /// Check if the monitor is started
        /// </summary>
        public bool MonitorStarted {
            get {
                return monitorStarted;
            }
        } private bool monitorStarted = false;

        /// <summary>
        /// If you set this property to a form handle
        /// before starting the monitor it will use the WndProc
        /// method otherwise it will use the WMI monitor (slower)        
        /// </summary>
        public IntPtr Handle {
            get {return handle;}
            set { handle = value; }
        } private IntPtr handle = IntPtr.Zero;

        #endregion

        #region Constructor

        public DeviceManager() { }

        ~DeviceManager() {
            if (monitorStarted)
                StopMonitor();
        }

        #endregion

        #region Monitor: WMI

        /// <summary>
        /// Starts the WMI monitor
        /// </summary>
        private void startMonitorWmi() {
            logger.Debug("Monitor: WMI");
            if (wmiMonitor == null) {
               
                WqlEventQuery query;
                ManagementOperationObserver observer = new ManagementOperationObserver();
                ConnectionOptions options = new ConnectionOptions();
                options.EnablePrivileges = true;
                ManagementScope scope = new ManagementScope(@"root\CIMV2", options);

                try {
                    query = new WqlEventQuery();
                    query.EventClassName = "__InstanceModificationEvent";
                    //TWEAK: polling interval
                    query.WithinInterval = new TimeSpan(0, 0, 10);
                    //TWEAK: only poll for removable types
                    query.Condition = "TargetInstance ISA 'Win32_LogicalDisk' and TargetInstance.DriveType <> 3";
                    wmiMonitor = new ManagementEventWatcher(scope, query);
                    wmiMonitor.EventArrived += new EventArrivedEventHandler(OnEventArrived);
                    wmiMonitor.Start();
                    monitorStarted = true;
                }
                catch (Exception e) {
                    logger.Debug("WMI monitor error during startup: ", e.Message);
                    wmiMonitor = null;
                    monitorStarted = false;
                }

            }
        }

        /// <summary>
        /// Stops the WMI monitor
        /// </summary>
        private void stopMonitorWmi() {
            if (wmiMonitor != null) {
                try {
                    wmiMonitor.Stop();
                    wmiMonitor = null;
                    monitorStarted = false;
                }
                catch (Exception e) {
                    logger.Debug("WMI monitor error during shutdown: ", e.Message);
                }
            }
        }

        /// <summary>
        /// Handles events generated by the WMI monitor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEventArrived(object sender, EventArrivedEventArgs e) {
            PropertyData pdOld = e.NewEvent.Properties["PreviousInstance"];
            PropertyData pdNew = e.NewEvent.Properties["TargetInstance"];
            if (pdNew != null) {
                ManagementBaseObject moNew = (ManagementBaseObject)pdNew.Value;
                ManagementBaseObject moOld = (ManagementBaseObject)pdOld.Value;
                // DeviceId
                if (moNew != null && moOld != null) {
                    string eventType = "WMI_EVENT_UNKNOWN";
                    string volume = moNew.Properties["DeviceID"].Value.ToString();
                    object serial = moNew.Properties["VolumeSerialNumber"].Value;
                    object serialOld = moOld.Properties["VolumeSerialNumber"].Value;
                    if (serial != serialOld) {

                        VolumeInfo volumeInfo = GetVolumeInfo(volume);

                        // Trigger events
                        if (serial == null && serialOld != null) {
                            eventType = "WMI_EVENT_VOLUME_REMOVED";
                            volumeInfo.Refresh(moNew); // update volume Info
                            invokeOnVolumeRemoved(volume, serialOld.ToString().Trim());
                        }
                        else if (serial != null && serialOld == null) {
                            eventType = "WMI_EVENT_VOLUME_INSERTED";
                            volumeInfo.Refresh(moNew); // update volume Info
                            invokeOnVolumeInserted(volume, serial.ToString().Trim());
                        }

                        
                    }
                    logger.Debug("{0}: DeviceID= {1} ({2}), VolumeSerialNumber = {3} ({4})", eventType, volume, moOld.Properties["DeviceID"].Value.ToString(), serial, serialOld);
                }

            }
        }

        #endregion

        #region Monitor: DeviceVolumeMonitor

        private void startMonitor() {
            logger.Debug("Monitor: DeviceVolumeMonitor");
            if (dvMonitor == null) {
                try {
                    dvMonitor = new DeviceVolumeMonitor(handle);
                    dvMonitor.OnVolumeInserted += new DeviceVolumeAction(dvVolumeInserted);
                    dvMonitor.OnVolumeRemoved += new DeviceVolumeAction(dvVolumeRemoved);
                    dvMonitor.AsynchronousEvents = true;
                    dvMonitor.Enabled = true;
                    monitorStarted = true;
                }
                catch (Exception e) {
                    logger.Debug("DeviceVolumeMonitor Error during startup: ", e.Message);
                    dvMonitor = null;
                    monitorStarted = false;
                }
            }           
        }

        private void stopMonitor() {
            if (dvMonitor != null) {
                try {

                    dvMonitor = null;
                    monitorStarted = false;
                }
                catch (Exception e) {
                    logger.Debug("Device Volume Monitor error during shutdown: ", e.Message);
                }
            }
        }

        private void dvVolumeInserted(int bitMask) {
            // get volume letter
            string volume = dvMonitor.MaskToLogicalPaths(bitMask);

            // refresh volume cache
            VolumeInfo volumeInfo = GetVolumeInfo(volume);
            volumeInfo.Refresh();

            // invoke event
            invokeOnVolumeInserted(volume, volumeInfo.Serial);
        }

        private void dvVolumeRemoved(int bitMask) {
            // get volume letter
            string volume = dvMonitor.MaskToLogicalPaths(bitMask);
            
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
        private void invokeOnVolumeInserted(string volume, string serial) {
            if (OnVolumeInserted != null) {
                OnVolumeInserted(volume, serial);
                logger.Debug("Event: OnVolumeInserted, Volume: {0}, Serial: {1}", volume, serial);
                //OnVolumeInserted.BeginInvoke(volume, serial, null, null);
            }
            
        }

        /// <summary>
        /// Invokes the OnVolumeRemoved event
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="serial"></param>
        private void invokeOnVolumeRemoved(string volume, string serial) {
            if (OnVolumeRemoved != null) {
                OnVolumeRemoved(volume, serial);
                //OnVolumeRemoved.BeginInvoke(volume, serial, null, null);
                logger.Debug("Event: OnVolumeRemoved, Volume: {0}, Serial: {1}", volume, serial);
            }

        }

        #endregion
        
        #region Public Methods

        /// <summary>
        /// Start monitoring volume changes
        /// </summary>
        public void StartMonitor() {
            logger.Info("Starting device monitor ...");

            // Check which monitor we should run
            if (handle != IntPtr.Zero) {
                // If a control handle is set use the 
                // DeviceVolumeMonitor
                startMonitor();
            }
            else {
                // if no control handle is set
                // use the (slower) WMI monitor
                startMonitorWmi();
            }

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
        public void StopMonitor() {
            if (!monitorStarted) {
                logger.Debug("Device monitor was not running.");
                return;
            }

            // Check which monitor is running and try to stop it
            if (wmiMonitor != null) {
                stopMonitorWmi();
            }
            else {
                stopMonitor();
            }

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
                    // not cached so we have to create is
                    VolumeInfo volumeInfo = new VolumeInfo(volume);
                    // add it to the cache
                    volumes.Add(volume, volumeInfo);
                }
                catch (Exception e) {
                    logger.Error("Error adding drive '{0}' : {1}", volume, e.Message);
                    return null;
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
                if ((GetDiskSerial(file) == serial) && !file.Exists)
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
