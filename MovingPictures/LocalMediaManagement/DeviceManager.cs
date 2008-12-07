using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Management;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {
    public class DeviceManager {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static Dictionary<string, ManagementObject> cache = new Dictionary<string, ManagementObject>();
        private static Dictionary<string, DriveInfo> drives = new Dictionary<string, DriveInfo>();
        
        /// <summary>
        /// Flushes the drive information cache for all drives
        /// </summary>
        public static void Flush() {
            cache.Clear();
            logger.Debug("Flushed drive information cache for all drives.");
        }

        /// <summary>
        /// Flushes the drive information cache for the specified driveletter.
        /// Call this method whenever a (new) volume is 
        /// detected or removed for optimal results.
        /// </summary>
        /// <param name="driveletter">X:</param>
        public static void Flush(string driveletter) {
            if (cache.ContainsKey(driveletter))
                cache.Remove(driveletter);

            logger.Debug("Flushed drive information cache for '{0}'.", driveletter);
        }

        /// <summary>
        /// Check if a file is removed from the disk
        /// </summary>
        /// <param name="file"></param>
        /// <param name="serial">drive serial</param>
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

        public static bool IsAvailable(FileInfo file) {
            return IsAvailable(file, null);
        }

        public static bool IsAvailable(FileInfo file, string serial) {
            file.Refresh();
            if (!String.IsNullOrEmpty(serial)) {
                if ((GetDiskSerial(file) == serial) && file.Exists)
                    return true;
            } else {
                if (file.Exists)
                    return true;
            }

            return false;
        }
        
        public static bool IsRemovable(DirectoryInfo directory) {
            return IsRemovable(directory.FullName);
        }

        public static bool IsRemovable(string path) {
            DriveInfo driveInfo = GetDriveInfo(path);
            if (driveInfo == null)
                return false;

            return (driveInfo.DriveType == DriveType.CDRom || driveInfo.DriveType == DriveType.Removable || driveInfo.DriveType == DriveType.Network);
        }

        public static string GetDiskSerial(DirectoryInfo directory) {
            return GetDiskSerial(directory.FullName);   
        }

        public static string GetDiskSerial(FileInfo file) {
            return GetDiskSerial(file.FullName);   
        }

        public static string GetDiskSerial(string path) {
            ManagementObject managementObject = GetManagementObject(path);
            if ( managementObject != null )
                if (managementObject["volumeserialnumber"] != null)
                    return managementObject["volumeserialnumber"].ToString().Trim();

            return null;
        }

        public static DriveInfo GetDriveInfo(DirectoryInfo directory) {
            return GetDriveInfo(directory.FullName);
        }

        public static DriveInfo GetDriveInfo(string path) {
            string drive = GetDriveLetter(path);
            if (drive == null)
                return null;

            if (!drives.ContainsKey(drive)) {
                try {
                    drives.Add(drive, new DriveInfo(drive));
                }
                catch (Exception e) {
                    logger.Error("Error drive '{0}' : {1}", drive, e.Message);
                    return null;
                }
            }
            
            return drives[drive];
        }

        private static ManagementObject GetManagementObject(string path) {
            if ( GetDrive(path) )
                return cache[GetDriveLetter(path)];
            else
                return null;
        }

        private static bool GetDrive(string path) {
            string drive = GetDriveLetter(path);
            
            if (drive == null)
                return false;

            if ( !cache.ContainsKey(drive) )
                return collectDriveInformation(drive);
            else
                return true;
        }

        public static string GetDriveLetter(string path) {
            string drive = path.Substring(0, 2);
            if (drive.StartsWith(@"/") || drive.StartsWith(@"\")) 
                return null;

            return drive;
        }

        public static string GetDriveLetter(FileInfo file) {
            return GetDriveLetter(file.FullName);
        }

        public static string GetDriveLetter(DirectoryInfo directory) {
            return GetDriveLetter(directory.FullName);
        }
        
        private static bool collectDriveInformation(string driveletter) {
            try {
                SelectQuery query = new SelectQuery("select * from win32_logicaldisk where deviceid = '" + driveletter + "'");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

                // this statement should return only one row (or none)
                foreach (ManagementObject managementObject in searcher.Get()) {
                    if (cache.ContainsKey(driveletter))
                        cache[driveletter] = managementObject;
                    else
                        cache.Add(driveletter, managementObject);
                }
            }
            catch (Exception e) {
                logger.Debug("Error during WMI query for '{0}' message: {1}", driveletter, e.Message);     
                return false;
            }

            logger.Debug("Succesfully collected drive information for '{0}'", driveletter);     
            return true;
        }

    }
}
