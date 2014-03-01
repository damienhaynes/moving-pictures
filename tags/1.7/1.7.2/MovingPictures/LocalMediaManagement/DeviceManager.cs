using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cornerstone.Extensions.IO;
using Cornerstone.Tools;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {
    
    /// <summary>
    /// Device Manager monitors drive states
    /// </summary>
    public class DeviceManager {

        #region Private Variables

        // Log object
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly object syncRoot = new object();

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
                lock (syncRoot) {
                    return (watcherThread != null && watcherThread.IsAlive);
                }
            }
        }

        #endregion

        #region Constructor

        private DeviceManager() {

        }

        ~DeviceManager() {
            StopDiskWatcher();
        }

        #endregion

        #region Monitoring Logic

        public static void StartMonitor() {
            if (MonitorStarted)
                return;

            // register listener to start receiving notifications about new import paths
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
        }

        public static void StopMonitor() {
            if (!MonitorStarted)
                return;

            // unregister listener to stop receiving notifications about new import paths
            MovingPicturesCore.DatabaseManager.ObjectInserted -= new DatabaseManager.ObjectAffectedDelegate(onPathAdded);
            
            ClearWatchDrives();
        }

        public static void AddWatchDrive(string path) {
            try {
                // get the driveletter
                string driveLetter = path.PathToDriveletter();
                if (driveLetter == null)
                    return;

                // add the drive to the drive watcher
                lock (syncRoot) {
                    if (watchedDrives == null)
                        watchedDrives = new List<string>();

                    if (!watchedDrives.Contains(driveLetter)) {
                        watchedDrives.Add(driveLetter);
                        StartDiskWatcher();
                        logger.Info("Added " + driveLetter + " to Disk Watcher");
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
            string driveLetter = path.PathToDriveletter();
            if (driveLetter == null)
                return;

            lock (syncRoot) {
                if (watchedDrives != null && watchedDrives.Contains(driveLetter)) {
                    watchedDrives.Remove(driveLetter);
                    logger.Info("Removed " + driveLetter + " from Disk Watcher");
                    if (watchedDrives.Count == 0)
                        StopDiskWatcher();
                }
            }

        }

        public static void ClearWatchDrives() {
            lock (syncRoot) {
                if (watchedDrives != null && watchedDrives.Count > 0) {
                    logger.Info("Removing all drives from Disk Watcher");
                    watchedDrives.Clear();
                }
                StopDiskWatcher();
            }
        }

        public static void StartDiskWatcher() {
            lock (syncRoot) {
                if (MovingPicturesCore.Settings.DeviceManagerEnabled && !MonitorStarted) {
                    logger.Info("Starting Disk Watcher...");
                    watcherThread = new Thread(new ThreadStart(WatchDisks));
                    watcherThread.Name = "DeviceManager.WatchDisks";
                    watcherThread.IsBackground = true;
                    watcherThread.Start();
                    logger.Info("Successfully started Disk Watcher.");
                }
            }
        }

        public static void StopDiskWatcher() {
            lock (syncRoot) {
                if (MonitorStarted) {
                    logger.Info("Stopping Disk Watcher...");
                    watcherThread.Abort();
                    logger.Info("Successfully stopped Disk Watcher.");
                }
            }
        }

        private static void WatchDisks() {
             while (true) {
                 lock (syncRoot) {
                    if (driveStates == null)
                        driveStates = new Dictionary<string, bool>();

                    foreach (string currDrive in watchedDrives) {
                        try {
                            // check if the drive is available
                            bool isAvailable;
                            DriveInfo driveInfo = DriveInfoHelper.GetDriveInfo(currDrive);
                            if (driveInfo == null) isAvailable = false;
                            else isAvailable = driveInfo.IsReady;

                            // if the previous drive state is not stored, store it and continue
                            if (!driveStates.ContainsKey(currDrive)) {
                                driveStates[currDrive] = isAvailable;
                                continue;
                            }

                            // if a change has occured
                            if (driveStates[currDrive] != isAvailable) {

                                // update our state
                                driveStates[currDrive] = isAvailable;

                                // notify any listeners
                                if (isAvailable) {

                                    if (driveInfo.DriveType != DriveType.Network)
                                        logger.Info("Volume Inserted: " + currDrive);
                                    else
                                        logger.Info("Volume Online: " + currDrive);

                                    if (OnVolumeInserted != null)
                                        OnVolumeInserted(currDrive, driveInfo.GetVolumeSerial());
                                }
                                else {
                                    // if a mapped network share gets disconnected it can either show Network or NoRootDirectory
                                    if (driveInfo.DriveType != DriveType.Network && driveInfo.DriveType != DriveType.NoRootDirectory)
                                        logger.Info("Volume Removed: " + currDrive);
                                    else
                                        logger.Info("Volume Offline: " + currDrive);

                                    if (OnVolumeRemoved != null)
                                        OnVolumeRemoved(currDrive, null);
                                }
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

    }
}
