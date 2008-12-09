using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Management;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {

    public class VolumeInfo {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private ManagementBaseObject managementObject;

        public DriveInfo Drive {
            get {
                return driveInfo;
            }
        } private DriveInfo driveInfo;
               
        public string Serial {
            get {
                if (driveInfo.IsReady) {
                    if (managementObject == null)
                        Refresh();

                    if (managementObject != null)
                        if (managementObject["volumeserialnumber"] != null)
                            return managementObject["volumeserialnumber"].ToString().Trim();
                    
                    return null;
                }
                else {
                    return null;
                }
            }
        }

        public void Refresh() {
            if (driveInfo.IsReady) {
                try {
                    SelectQuery query = new SelectQuery("select * from win32_logicaldisk where deviceid = '" + driveInfo.Name.Substring(0, 2) + "'");
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

                    // this statement should return only one row (or none)
                    foreach (ManagementBaseObject mo in searcher.Get()) {
                        Refresh(mo);
                    }
                }
                catch (Exception e) {
                    managementObject = null;
                    logger.Debug("Error during WMI query for '{0}', message: {1}", driveInfo.Name.Substring(0, 2), e.Message);
                }                
            }
            else {
                managementObject = null;
            }
        }

        public void Refresh(ManagementBaseObject mo) {
            managementObject = mo;
            logger.Debug("Succesfully updated drive information for '{0}'", driveInfo.Name.Substring(0, 2));
        }

        public VolumeInfo(DriveInfo di) {
            driveInfo = di;
            Refresh();
        }

    }
    
    public class DeviceManager {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<string, VolumeInfo> volumes;
        private ManagementEventWatcher monitor;

        public delegate void DeviceAction(string volume);
        public event DeviceAction OnVolumeInserted;
        public event DeviceAction OnVolumeRemoved;

        public bool Monitoring {
            get {
                return monitorStarted;
            }
        } private bool monitorStarted = false;

        public DeviceManager() {
            volumes = new Dictionary<string, VolumeInfo>();
            Start();
        }

        ~DeviceManager() {
            Stop();
        }

        public void Start() {
            if (monitor == null) {
                logger.Info("Starting device monitor ...");
                WqlEventQuery query;
                ManagementOperationObserver observer = new ManagementOperationObserver();
                ConnectionOptions options = new ConnectionOptions();
                options.EnablePrivileges = true;
                ManagementScope scope = new ManagementScope(@"root\CIMV2", options);

                try {
                    query = new WqlEventQuery();
                    query.EventClassName = "__InstanceModificationEvent";
                    query.WithinInterval = new TimeSpan(0, 0, 1);
                    // TODO: make query more specific only monitor import path volumes/drivetypes?
                    query.Condition = "TargetInstance ISA 'Win32_LogicalDisk'";
                    //+ " and TargetInstance.DriveType = 5";
                    monitor = new ManagementEventWatcher(scope, query);
                    monitor.EventArrived += new EventArrivedEventHandler(OnEventArrived);
                    monitor.Start();
                    monitorStarted = true;
                    logger.Info("Device monitor started.");
                }
                catch (Exception e) {
                    logger.Error("Device monitor error during startup: ", e.Message);
                    monitor = null;
                }

            }

        }

        public void Stop() {
            if (monitor != null) {
                monitor.Stop();
                monitor = null;
                monitorStarted = false;
                logger.Info("Device monitor stopped.");
            }
        }

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
                            if (OnVolumeRemoved != null)
                                OnVolumeRemoved(volume); // fire event
                        }
                        else if (serial != null && serialOld == null) {
                            eventType = "WMI_EVENT_VOLUME_INSERTED";
                            volumeInfo.Refresh(moNew); // update volume Info
                            if (OnVolumeInserted != null)
                                OnVolumeInserted(volume); // fire event
                        }

                        
                    }
                    logger.Debug("{0}: DeviceID= {1} ({2}), VolumeSerialNumber = {3} ({4})", eventType, volume, moOld.Properties["DeviceID"].Value.ToString(), serial, serialOld);
                }

            }
        }

        public string GetVolume(FileSystemInfo fsInfo) {
            return GetVolume(fsInfo.FullName);
        }

        public string GetVolume(string path) {
            // if the path is UNC return null
            if (path.StartsWith(@"/") || path.StartsWith(@"\"))
                return null;

            // return the first 2 characters
            if (path.Length > 1)
                return path.Substring(0, 2);
            else // or if only a letter was given add colon
                return path + ":";
        }

        public VolumeInfo GetVolumeInfo(FileSystemInfo fsInfo) {
            return GetVolumeInfo(fsInfo.FullName);
        }

        public VolumeInfo GetVolumeInfo(string path) {
            string volume = GetVolume(path);
            // if volume is null then it's UNC
            if (volume == null)
                return null;

            // check if we have previously cached this instance
            if (!volumes.ContainsKey(volume)) {
                try {
                    // not cached so we have to create is
                    DriveInfo driveInfo = new DriveInfo(volume);
                    VolumeInfo volumeInfo = new VolumeInfo(driveInfo);
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
        public void Flush() {
            volumes.Clear();
            logger.Debug("Flushed drive information cache for all drives.");
        }

        /// <summary>
        /// Flushes the drive information cache for the specified driveletter.
        /// Call this method whenever a (new) volume is 
        /// detected or removed for optimal results.
        /// </summary>
        /// <param name="driveletter">X:</param>
        public void Flush(string volume) {
            if (volumes.ContainsKey(volume))
                volumes.Remove(volume);

            logger.Debug("Flushed drive information cache for '{0}'.", volume);
        }

        /// <summary>
        /// Check if a file is removed from the disk
        /// </summary>
        /// <param name="file"></param>
        /// <param name="serial">drive serial</param>
        /// <returns>true if file doesn't exist but disk/root is available.</returns>
        public bool IsRemoved(FileInfo file, string serial) {
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

        public bool IsAvailable(FileSystemInfo fsInfo) {
            return IsAvailable(fsInfo, null);
        }

        public bool IsAvailable(FileSystemInfo fsInfo, string serial) {         
            
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

        public bool IsRemovable(FileSystemInfo fsInfo) {
            return IsRemovable(fsInfo.FullName);
        }

        public bool IsRemovable(string path) {
            VolumeInfo volumeInfo = GetVolumeInfo(path);
            if (volumeInfo == null)
                return true; // true because it's UNC (=network)

            return (volumeInfo.Drive.DriveType == DriveType.CDRom || volumeInfo.Drive.DriveType == DriveType.Removable || volumeInfo.Drive.DriveType == DriveType.Network);
        }

        public string GetDiskSerial(FileSystemInfo fsInfo) {
            return GetDiskSerial(fsInfo.FullName);   
        }

        public string GetDiskSerial(string path) {
           VolumeInfo volumeInfo = GetVolumeInfo(path);
           if (volumeInfo != null)
               return volumeInfo.Serial;
           else
               return null;
        }

        public string GetVolumeLabel(FileSystemInfo fsInfo) {
            return GetVolumeLabel(fsInfo.FullName);
        }
        
        public string GetVolumeLabel(string path) {
            VolumeInfo volumeInfo = GetVolumeInfo(path);
            if (volumeInfo != null)
                return volumeInfo.Drive.VolumeLabel;
            else
                return null;            
        }

        

        
    }
}
