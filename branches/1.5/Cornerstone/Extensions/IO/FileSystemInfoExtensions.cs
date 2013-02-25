using System;
using System.IO;
using Cornerstone.Tools;

namespace Cornerstone.Extensions.IO {
    public static class FileSystemInfoExtensions {

        /// <summary>
        /// Gets the driveletter of this instance
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string GetDriveLetter(this FileSystemInfo self) {
            string driveletter = self.FullName.PathToDriveletter();
            return driveletter;
        }

        /// <summary>
        /// Gets the DriveInfo object related to this instance
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static DriveInfo GetDriveInfo(this FileSystemInfo self) {
            string path = self.FullName;
            return DriveInfoHelper.GetDriveInfoFromFilePath(path);
        }
        
        /// <summary>
        /// Returns the volume serial of the drive where this path resides or empty if no serial is found.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string GetDriveVolumeSerial(this FileSystemInfo self) {
            DriveInfo driveInfo = self.GetDriveInfo();
            string serial = (driveInfo != null) ? driveInfo.GetVolumeSerial() : string.Empty;
            return serial;
        }

        /// <summary>
        /// Get a value indicating whether the current file exists on the same volume. 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="VolumeSerialNumber">a volume serial number in hexidecimal format</param>
        /// <returns>True, if the file exists on the same volume</returns>
        public static bool Exists(this FileSystemInfo self, string compareSerial) {
            // Refresh the object information (important)
            self.Refresh();

            // Check if the file exists on the current path
            bool exists = self.Exists;

            // Return if the path is UNC or it does not exist
            if (!exists || self.IsUncPath())
                return exists;

            // If the path exists and is a local drive check the given serial
            string currentSerial = self.GetDriveVolumeSerial();
            return currentSerial.Trim().Equals(compareSerial.Trim());
        }

        /// <summary>
        /// Gets a value indicating whether this filesysteminfo object is a reparse point
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsReparsePoint(this FileSystemInfo self) {
            if (self.IsUncPath())
                return false;

            try {
                if ((self.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                    return true;
            }
            // ignore the exceptions that can occur
            catch (ArgumentException) { }
            catch (System.Security.SecurityException) { }
            catch (IOException) { }

            return false;
        }     

        /// <summary>
        /// Gets a value indicating whether the current path is formatted as UNC
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsUncPath(this FileSystemInfo self) {
            return self.FullName.PathIsUnc();
        }

        /// <summary>
        /// Gets a value indicating wether this FileSystemInfo object is located on a optical drive.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsOpticalPath(this FileSystemInfo self) {
            DriveInfo driveInfo = self.GetDriveInfo();
            return (driveInfo != null && driveInfo.IsOptical());
        }

        /// <summary>
        /// Gets a value indicating wether this FileSystemInfo object is located on a removable drive.
        /// </summary>
        /// <param name="fsInfo"></param>
        /// <returns></returns>
        public static bool IsRemovablePath(this FileSystemInfo self) {
            // UNC paths and reparse points will be regarded removable
            if (self.IsUncPath() || self.Exists && self.IsReparsePoint())
                return true;

            return self.GetDriveInfo().IsRemovable();
        }

    }
}
