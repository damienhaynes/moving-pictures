using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.Database {
    public class DatabaseMaintenanceManager {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        public static event ProgressDelegate MaintenanceProgress;

        // Loops through all local files in the system and removes anything that's invalid.
        public static void RemoveInvalidFiles() {
            logger.Info("Checking for invalid file entries in the database.");


            float count = 0;
            float total = DBLocalMedia.GetAll().Count;
            
            int cleaned = 0;
            foreach (DBLocalMedia currFile in DBLocalMedia.GetAll()) {
                if (MaintenanceProgress != null) MaintenanceProgress("", (int)(count*100/total));
                count++;
                
                // Skip previously deleted files
                if (currFile.ID == null)
                    continue;

                // remove missing files or files without an import path
                if (currFile.ImportPath == null || currFile.IsRemoved || currFile.ImportPath.ID == null) {
                    currFile.Delete();
                    cleaned++;
                    continue;
                }

                // Remove Orphan Files
                if (currFile.AttachedMovies.Count == 0 && !currFile.Ignored) {
                    logger.Info("Removing " + currFile.FullPath + " (orphan)");
                    currFile.Delete();
                    cleaned++;
                    continue;
                }
            }
            
            logger.Info("Removed {0} file entries.", cleaned.ToString());
            if (MaintenanceProgress != null) MaintenanceProgress("", 100);
        }

        // Loops through all movie in the system and removed anything that's invalid
        public static void RemoveInvalidMovies() {
            logger.Info("Checking for invalid movie entries in the database.");

            float count = 0;
            float total = DBMovieInfo.GetAll().Count;

            int cleaned = 0;
            foreach (DBMovieInfo currMovie in DBMovieInfo.GetAll()) {
                if (MaintenanceProgress != null) MaintenanceProgress("", (int)(count * 100 / total));
                count++;

                // Remove movie with no files
                if (currMovie.LocalMedia.Count == 0) {
                    logger.Info("'{0}' was removed from the system because it had no local media.", currMovie.Title);
                    currMovie.Delete();
                    cleaned++;
                    continue;
                }
            }

            logger.Info("Removed {0} movie entries.", cleaned.ToString());
            if (MaintenanceProgress != null) MaintenanceProgress("", 100);
        }

        // Updates missing user settings for a movie
        public static void UpdateUserSettings() {

            float count = 0;
            float total = DBMovieInfo.GetAll().Count + DBUserMovieSettings.GetAll().Count; 

            // add missing user settings
            foreach (DBMovieInfo currMovie in DBMovieInfo.GetAll()) {
                if (MaintenanceProgress != null) MaintenanceProgress("", (int)(count * 100 / total));
                count++;
                
                if (currMovie.UserSettings.Count == 0) {
                    logger.Info(currMovie.Title + " was missing UserMovingSettings, adding now.");
                    foreach (DBUser currUser in DBUser.GetAll()) {
                        DBUserMovieSettings userSettings = new DBUserMovieSettings();
                        userSettings.User = currUser;
                        userSettings.Commit();
                        currMovie.UserSettings.Add(userSettings);
                        userSettings.CommitNeeded = false;
                    }
                    currMovie.Commit();
                }
            }

            // Remove Orphan User Settings
            List<DBUserMovieSettings> allUserSettings = DBUserMovieSettings.GetAll();
            foreach (DBUserMovieSettings currSetting in allUserSettings) {
                if (MaintenanceProgress != null) MaintenanceProgress("", (int)(count * 100 / total));
                count++;

                if (currSetting.AttachedMovies.Count == 0)
                    currSetting.Delete();
            }

            if (MaintenanceProgress != null) MaintenanceProgress("", 100);
        }

        // create/update otpical drive import paths
        public static void UpdateMissingDiskInfoProperties() {
            float count = 0;
            float total = DBLocalMedia.GetAll().Count; 

            foreach (DBLocalMedia currFile in DBLocalMedia.GetAll()) {
                if (MaintenanceProgress != null) MaintenanceProgress("", (int)(count * 100 / total));
                count++;

                DriveType type = currFile.ImportPath.GetDriveType();
                if (String.IsNullOrEmpty(currFile.VolumeSerial) && type != DriveType.Unknown && type != DriveType.CDRom) {
                    if (currFile.IsAvailable) {
                        currFile.UpdateDiskProperties();
                        currFile.Commit();
                        logger.Info("Added missing disk info to: {0} (serial: {1}, label: {2})", currFile.FullPath, currFile.VolumeSerial, currFile.MediaLabel);
                    }
                }
            }

            if (MaintenanceProgress != null) MaintenanceProgress("", 100);
        }

        // Update System Managed Import Paths
        public static void UpdateImportPaths() {
            
            // remove invalid import paths
            foreach (DBImportPath currPath in DBImportPath.GetAll()) {
                if (currPath.Directory == null)
                    currPath.Delete();
            }

            float count = 0;
            float total = DriveInfo.GetDrives().Length; 

            bool daemonEnabled = MovingPicturesCore.MediaPortalSettings.GetValueAsBool("daemon", "enabled", false);
            string virtualDrive = MovingPicturesCore.MediaPortalSettings.GetValueAsString("daemon", "drive", "?:");
            
            // Get all drives
            foreach (DriveInfo drive in DriveInfo.GetDrives()) {
                if (MaintenanceProgress != null) MaintenanceProgress("", (int)(count * 100 / total));
                count++;

                // Add the import path if it does not exist and 
                // is not marked virtual by MediaPortal.
                DBImportPath importPath = DBImportPath.Get(drive.Name);
                bool isVirtual = drive.Name.StartsWith(virtualDrive, StringComparison.OrdinalIgnoreCase) && daemonEnabled;
                bool isCDRom = (drive.DriveType == DriveType.CDRom);

                if (importPath.ID != null) {
                    // Remove an system managed path if for any reason it's not of type CDRom
                    if (!isCDRom && importPath.InternallyManaged) {
                        importPath.Delete();
                        logger.Info("Removed system managed import path: {0} (drive type has changed)", importPath.FullPath);
                        continue;
                    }

                    // Remove an existing path if it's defined as the virtual drive
                    if (isVirtual) {
                        importPath.Delete();
                        logger.Info("Removed import path: {0} (drive is marked as virtual)", importPath.FullPath);
                        continue;
                    }

                    // Update an existing import path to a system managed import path
                    // if the drive type is CDRom but the system flag isn't set
                    if (isCDRom && !importPath.InternallyManaged) {
                        importPath.InternallyManaged = true;
                        importPath.Commit();
                        logger.Info("{0} was updated to a system managed import path.", importPath.FullPath);
                    }

                }
                else {
                    if (isCDRom && !isVirtual) {
                        importPath.InternallyManaged = true;
                        importPath.Commit();
                        logger.Info("Added system managed import path: {0}", importPath.FullPath);
                    }
                }
            }

            if (MaintenanceProgress != null) MaintenanceProgress("", 100);
        }

        // Removes Artwork From a Movie
        public static void RemoveOrphanArtwork() {
            float count = 0;
            float total = DBMovieInfo.GetAll().Count; 

            foreach (DBMovieInfo currMovie in DBMovieInfo.GetAll()) {
                if (MaintenanceProgress != null) MaintenanceProgress("", (int)(count * 100 / total));
                count++;

                // get the list of elements to remove
                List<string> toRemove = new List<string>();
                foreach (string currCoverPath in currMovie.AlternateCovers) {
                    if (!new FileInfo(currCoverPath).Exists)
                        toRemove.Add(currCoverPath);
                }

                // remove them
                foreach (string currItem in toRemove) {
                    currMovie.AlternateCovers.Remove(currItem);
                }

                // reset default cover is needed
                if (!currMovie.AlternateCovers.Contains(currMovie.CoverFullPath))
                    if (currMovie.AlternateCovers.Count == 0)
                        currMovie.CoverFullPath = " ";
                    else
                        currMovie.CoverFullPath = currMovie.AlternateCovers[0];

                // get rid of the backdrop link if it doesnt exist
                if (currMovie.BackdropFullPath.Trim().Length > 0 && !new FileInfo(currMovie.BackdropFullPath).Exists)
                    currMovie.BackdropFullPath = " ";

                currMovie.Commit();
            }

            if (MaintenanceProgress != null) MaintenanceProgress("", 100);
        }

        public static void UpdateDateAddedFields() {
            // update date added fields
            if (MovingPicturesCore.GetDBVersionNumber() < new Version("0.7.1")) {
                List<DBMovieInfo> movies = DBMovieInfo.GetAll();
                movies.Sort(delegate(DBMovieInfo movieX, DBMovieInfo movieY) {
                    return movieX.ID.GetValueOrDefault(0).CompareTo(movieY.ID.GetValueOrDefault(0));
                });

                float total = movies.Count;

                for (int i = 0; i < movies.Count; i++) {
                    if (MaintenanceProgress != null) MaintenanceProgress("", (int)(i * 100 / total));

                    if (movies[i].LocalMedia[0].IsAvailable && movies[i].LocalMedia[0].File.Extension.ToLower() != ".ifo") {
                        movies[i].DateAdded = movies[i].LocalMedia[0].File.CreationTime;
                    }
                    else {
                        // add 1 minute for offline media and dvds, to retain the same order
                        if (i > 0)
                            movies[i].DateAdded = movies[i - 1].DateAdded.AddMinutes(1);
                    }

                    movies[i].Commit();
                }
            }
        }
    }
}
