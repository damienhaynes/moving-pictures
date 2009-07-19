using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {

    /// <summary>
    /// Class that holds volume information
    /// </summary>
    public class VolumeInfo {

        #region Static variables

        private static readonly object lockCache = new object();
        private static Dictionary<string, VolumeInfo> _cachedStorageInfo;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Private variables

        private readonly object refreshLock = new object();
        private bool refreshNeeded;

        #endregion

        #region Constructors

        /// <summary>
        /// Static Constructor which will create the VolumeInfo cache
        /// </summary>
        static VolumeInfo() {
            lock (lockCache) {
                _cachedStorageInfo = new Dictionary<string, VolumeInfo>();
            }
        }

        /// <summary>
        /// Creates a new VolumeInfo object for the specified driveletter
        /// </summary>
        /// <param name="driveletter"></param>
        public VolumeInfo(string driveletter) {
            DriveInfo driveInfo = new DriveInfo(driveletter);
            init(driveInfo);
        }

        /// <summary>
        /// Creates a new VolumeInfo object for the specified DriveInfo object
        /// </summary>
        /// <param name="driveInfo"></param>
        public VolumeInfo(DriveInfo driveInfo) {
            init(driveInfo);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns the DriveInfo object of the volume
        /// </summary>
        public DriveInfo DriveInfo {
            get {
                return _driveInfo;
            }
        } private DriveInfo _driveInfo = null;

        /// <summary>
        /// Returns the name of the volume
        /// </summary>
        public string Name {
            get { return DriveInfo.Name.Substring(0, 2); }
        }

        /// <summary>
        /// Returns the serial of the volume or empty
        /// </summary>
        public string Serial {
            get {
                validate();
                return _serial;
            }
        } private string _serial = string.Empty;

        /// <summary>
        /// Returns the label of the volume or empty
        /// </summary>
        public string Label {
            get {
                validate();
                return _label;
            }
        } private string _label = string.Empty;

        /// <summary>
        /// Returns the drive type of this volume
        /// </summary>
        public DriveType Type {
            // todo: use WMI when the drivetype is NoRootDirectory?
            get { return DriveInfo.DriveType; }
        }

        /// <summary>
        /// Gets a value indicating wether this volume is available/ready to be read
        /// </summary>
        public bool IsAvailable {
            get {
                bool currentState = DriveInfo.IsReady;
                lock (refreshLock) {
                    // if the state was changed flag a refresh
                    if (lastState != currentState)
                        refreshNeeded = true;

                    // update the last state with the current state 
                    lastState = currentState;
                }
                //  return the current state
                return currentState;
            }
        } private bool lastState;

        #endregion

        #region Public Methods

        /// <summary>
        /// Will force a refresh of this volume
        /// </summary>
        public void Refresh() {
            lock (refreshLock) {
                refreshNeeded = true;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Initializes the VolumeINfo object using a DriveInfo object
        /// </summary>
        /// <param name="driveInfo"></param>
        private void init(DriveInfo driveInfo) {
            refreshNeeded = true;
            this._driveInfo = driveInfo;
        }

        /// <summary>
        /// Validates the properties of the volume and updates them if needed
        /// </summary>
        private void validate() {
            lock (refreshLock) {
                // Check if the volume is available
                if (IsAvailable) {

                    // if a refresh is not needed return
                    if (!refreshNeeded)
                        return;

                    // update private properties
                    _label = DriveInfo.VolumeLabel;
                    _serial = DriveInfo.GetVolumeSerialNumber();

                    // verify that the serial not empty
                    // and disable the refresh flag
                    if (_serial != string.Empty)
                        refreshNeeded = false;
                }
                else {
                    // the volume is not available so these values should be empty
                    _label = string.Empty;
                    _serial = string.Empty;
                    refreshNeeded = true;
                }
            }
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Get a VolumeInfo object for the given driveletter
        /// When the object was created before it will be returned
        /// from cache.
        /// </summary>
        /// <param name="driveletter">ex. E:</param>
        /// <returns>VolumeInfo object</returns>
        public static VolumeInfo Get(string volume) {
            lock (lockCache) {
                if (!_cachedStorageInfo.ContainsKey(volume)) {
                    try {
                        _cachedStorageInfo.Add(volume, (new VolumeInfo(volume)));
                    }
                    catch (Exception e) {
                        logger.Error("Error adding volume '{0}' to cache. {1}", volume, e.Message);
                        return null;
                    }
                }
                return _cachedStorageInfo[volume];
            }
        }

        /// <summary>
        /// Flushes all VolumeInfo objects from the cache
        /// </summary>
        public static void Flush() {
            lock (lockCache) {
                int cachedObjects = _cachedStorageInfo.Count;
                _cachedStorageInfo.Clear();
                logger.Debug("Flushed cache for {0} volumes.", cachedObjects);
            }
        }

        /// <summary>
        /// Flushes the VolumeInfo object matching the specified driveletter from the cache
        /// </summary>
        /// <param name="driveletter">X:</param>
        public static void Flush(string driveletter) {
            lock (lockCache) {
                if (_cachedStorageInfo.ContainsKey(driveletter))
                    _cachedStorageInfo.Remove(driveletter);
            }
            logger.Debug("Flushed cache for volume '{0}'.", driveletter);
        }

        #endregion

        #region Overrides

        public override string ToString() {
            return ("Volume='" + this.Name + "', Label='" + this.Label + "', Serial='" + this.Serial + "'");
        }

        #endregion
    }

    /// <summary>
    /// Extensions to the DriveInfo class
    /// </summary>
    public static class DriveInfoExtensions {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets the driveletter in a two character format. 
        /// </summary>
        /// <param name="self"></param>
        /// <returns>example: "C:"</returns>
        public static string GetDriveLetter(this DriveInfo self) {
            return self.Name.Substring(0, 2);
        }

        /// <summary>
        /// Gets the VolumeSerialNumber for this drive
        /// </summary>
        /// <param name="self"></param>
        /// <returns>VolumeSerialNumber or empty string</returns>
        public static string GetVolumeSerialNumber(this DriveInfo self) {
            string serial = string.Empty;

            // Before we do request information check if the drive is ready.
            if (self.IsReady) {
                try {
                    ManagementObject disk = new ManagementObject("Win32_LogicalDisk.DeviceID='" + self.GetDriveLetter() + "'");
                    foreach (PropertyData diskProperty in disk.Properties) {
                        if (diskProperty.Name == "VolumeSerialNumber") {
                            serial = diskProperty.Value.ToString().Trim();
                            logger.Debug("DriveInfo: Name='{0}', Serial='{1}', Status=OK", self.GetDriveLetter(), serial);
                            return serial;
                        }
                    }
                    logger.Debug("DriveInfo: Name='{0}' Serial=N/A, Status=FAIL", self.GetDriveLetter(), serial);
                }
                catch (Exception e) {
                    // Catch Exceptions
                    logger.DebugException("DriveInfo: Name='" + self.GetDriveLetter() + "' Status=ERROR", e);
                }
            }
            else {
                logger.Debug("DriveInfo: Name='{0}', Status=NotReady.", self.GetDriveLetter());
            }

            return serial;
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

    }

}
