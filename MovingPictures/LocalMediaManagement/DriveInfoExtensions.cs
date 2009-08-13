using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {

    /// <summary>
    /// Extensions to the DriveInfo class
    /// </summary>
    public static class DriveInfoExtensions {

        /// <summary>
        /// Gets the driveletter in a two character format. 
        /// </summary>
        /// <param name="self"></param>
        /// <returns>example: "C:"</returns>
        public static string GetDriveLetter(this DriveInfo self) {
            return self.Name.Substring(0, 2);
        }

        /// <summary>
        /// Gets a value indicating wether this drive is removable
        /// </summary>
        /// <param name="self"></param>
        /// <returns>True if removable</returns>
        public static bool IsRemovable(this DriveInfo self) {
            // Every other type than fixed will be considered removable
            return (self.DriveType != DriveType.Fixed);
        }

        /// <summary>
        /// Gets a value indicating wether this is an optical drive
        /// </summary>
        /// <returns>True if this is an optical drive</returns>
        public static bool IsOptical(this DriveInfo self) {
            return (self.DriveType == DriveType.CDRom);
        }

        /// <summary>
        /// Gets a value indicating wether the drive exists
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool Exists(this DriveInfo self) {
            return (self.DriveType == DriveType.NoRootDirectory);
        }

        /// <summary>
        /// Gets the volume serial number of the drive
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string GetVolumeSerial(this DriveInfo self) {
            uint serNum = 0;
            uint maxCompLen = 0;
            UInt32 volumeFlags = new UInt32();
            StringBuilder volumeLabel = new StringBuilder(256);
            StringBuilder FSName = new StringBuilder(256);
            long rt = GetVolumeInformation(self.Name, volumeLabel, (UInt32)volumeLabel.Capacity, ref serNum, ref maxCompLen, ref volumeFlags, FSName, (UInt32)FSName.Capacity);
            // return the serial number as a hexidecimal string
            return ( (serNum != 0) ? Convert.ToString(serNum, 16).ToUpper() : string.Empty );
        }

        [DllImport("kernel32.dll")]
        private static extern long GetVolumeInformation(string PathName, StringBuilder VolumeNameBuffer, UInt32 VolumeNameSize, ref UInt32 VolumeSerialNumber, ref UInt32 MaximumComponentLength, ref UInt32 FileSystemFlags, StringBuilder FileSystemNameBuffer, UInt32 FileSystemNameSize);

    }

}
