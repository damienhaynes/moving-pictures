using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using NLog;
using Cornerstone.Extensions.IO;
using Cornerstone.GUI.Dialogs;
using Cornerstone.Database.Tables;
using Cornerstone.Database;
using Cornerstone.Database.CustomTypes;
using Cornerstone.Extensions;
using System.Reflection;
using MediaPortal.Plugins.MovingPictures.MainUI;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;

namespace MediaPortal.Plugins.MovingPictures.Database {
    public class DatabaseMaintenanceManager {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        public static event ProgressDelegate MaintenanceProgress;

        // Loops through all local files in the system and removes anything that's invalid.
        public static void RemoveInvalidFiles() {
           
            logger.Info("Checking for invalid file entries in the database.");

            float count = 0;
            List<DBLocalMedia> files = DBLocalMedia.GetAll();
            float total = files.Count;
            
            int cleaned = 0;
            
            foreach (DBLocalMedia currFile in files) {
                if (MaintenanceProgress != null) MaintenanceProgress("", (int)(count * 100 / total));
                count++;

                // Skip previously deleted files
                if (currFile.ID == null)
                    continue;

                // remove files without an import path
                if (currFile.ImportPath == null || currFile.ImportPath.ID == null) {
                    logger.Info("Invalid file: {0} (no import path)", currFile.FullPath);
                    MovingPicturesCore.Importer.MarkMediaForRemoval(currFile);
                    cleaned++;
                    continue;
                }

                // Remove Orphan Files
                if (currFile.AttachedMovies.Count == 0 && !currFile.Ignored) {
                    logger.Info("Invalid file: {0} (orphan)", currFile.FullPath);
                    MovingPicturesCore.Importer.MarkMediaForRemoval(currFile);
                    cleaned++;
                    continue;
                }

                // Remove entries from the database that have their file removed
                if (currFile.IsRemoved) {
                    logger.Info("Invalid file: {0} (does not exist)", currFile.FullPath);
                    MovingPicturesCore.Importer.MarkMediaForRemoval(currFile);
                    cleaned++;
                }

            }
            
            logger.Info("{0} file entries were marked for removal by the importer.", cleaned.ToString());
            if (MaintenanceProgress != null) MaintenanceProgress("", 100);
        }
        
        // Loops through all movie in the system to verify them
        public static void VerifyMovieInformation() {
            logger.Info("Updating Movie Information...");

            float count = 0;
            List<DBMovieInfo> movies = DBMovieInfo.GetAll();
            List<DBUser> users = DBUser.GetAll();
            float total = movies.Count;

            int removed = 0;
            int settings = 0;
            foreach (DBMovieInfo movie in movies) {
                if (MaintenanceProgress != null) MaintenanceProgress("", (int)(count * 100 / total));
                count++;

                // Skip uncommited files
                if (movie.ID == null)
                    continue;

                #region Remove movie without attached local media

                // Remove movie with no files
                if (movie.LocalMedia.Count == 0) {
                    logger.Info("'{0}' was removed from the system because it had no local media.", movie.Title);
                    movie.Delete();
                    removed++;
                    continue;
                }

                #endregion

                #region Add missing user settings

                if (movie.UserSettings.Count == 0) {
                    logger.Info("'{0}' was missing UserMovingSettings, adding now.", movie.Title);
                    foreach (DBUser currUser in users) {
                        DBUserMovieSettings userSettings = new DBUserMovieSettings();
                        userSettings.User = currUser;
                        userSettings.Commit();
                        movie.UserSettings.Add(userSettings);
                        userSettings.CommitNeeded = false;
                    }
                    movie.Commit();
                    settings++;
                }

                #endregion

            }

            logger.Info("Removed {0} movie entries.", removed.ToString());
            logger.Info("Updated {0} movie entries with default user setting.", settings.ToString());
            if (MaintenanceProgress != null) MaintenanceProgress("", 100);
        }

        public static void VerifyDataSources() {
            logger.Info("Checking for bad data source entries...");

            // check records tieing a movie to a data source for bad data
            foreach (DBMovieInfo currMovie in DBMovieInfo.GetAll()) {
                List<DBSourceMovieInfo> infoList = currMovie.SourceMovieInfo.FindAll(info => info.Source == null);

                if (infoList.Count == 0)
                    continue;

                logger.Debug("Found " + infoList.Count + " invalid source references for '" + currMovie.Title + "'. Removing.");

                foreach (DBSourceMovieInfo currInfo in infoList) {
                    currMovie.SourceMovieInfo.Remove(currInfo);
                    currInfo.Delete();
                }

                currMovie.Commit();
            }

            // check for scripts that have no contents
            List<DBScriptInfo> scripts = MovingPicturesCore.DatabaseManager.Get<DBScriptInfo>(null);
            foreach (DBScriptInfo currScript in scripts) {
                if (currScript.Contents == null || currScript.Contents.Trim() == "") {
                    logger.Debug("Found empty script, removing...");

                    // first get rid of any references to this script
                    foreach (DBSourceInfo currSource in DBSourceInfo.GetAll()) {
                        bool changed = false;
                        if (currSource.Scripts.Contains(currScript)) {
                            currSource.Scripts.Remove(currScript);
                            changed = true;
                        }

                        if (currSource.SelectedScript == currScript) {
                            currSource.SelectedScript = null;
                            changed = true;
                        }

                        if (changed) currSource.Commit();
                    }
                    
                    // and get rid of the script
                    currScript.Delete();
                }
            }

            // check for sources that have no scripts
            foreach (DBSourceInfo currSource in  DBSourceInfo.GetAll()) {
                if (currSource.IsScriptable() && currSource.SelectedScript == null) {
                    if (currSource.Scripts.Count > 0) {
                        currSource.SelectedScript = currSource.Scripts[0];
                        currSource.Commit();
                    }
                    else {
                        logger.Debug("Found scriptable provider with no attached scripts! Removing...");
                        
                        // remove any references to this source from our movie collection
                        foreach (DBMovieInfo currMovie in DBMovieInfo.GetAll()) {
                            if (currMovie.SourceMovieInfo.FindAll(info => info.Source == currSource).Count > 0) {
                                currMovie.SourceMovieInfo.RemoveAll(info => info.Source == currSource);
                                currMovie.Commit();
                            }
                        }

                        // and get rid of the source
                        currSource.Delete();
                    }
                }
            }
        }

        // Update System Managed Import Paths
        public static void UpdateImportPaths() {
            
            // remove obsolete or invalid import paths
            foreach (DBImportPath currPath in DBImportPath.GetAll()) {
                if (currPath.Directory == null)
                    currPath.Delete();

                if (currPath.InternallyManaged && currPath.GetDriveType() == DriveType.NoRootDirectory) {
                    currPath.Delete();
                    logger.Info("Removed system managed import path: {0} (drive does not exist)", currPath.FullPath);
                }

                // Automatically remove import paths that were marked as replaced and don't have any related media (left)
                if (currPath.Replaced) {
                    // get related local media (we append % so we get all paths starting with the import path)
                    List<DBLocalMedia> attachedLocalMedia = DBLocalMedia.GetAll(currPath.FullPath + "%");
                    if (attachedLocalMedia.Count == 0) {
                        currPath.Delete();
                        logger.Info("Removed import path: {0} (was marked as replaced and doesn't have any related files)", currPath.FullPath);
                    }
                }

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
        
        // One time upgrade tasks for movie information
        public static void PerformMovieInformationUpgradeCheck() {
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            logger.Info("Performing Movie Information Upgrade Check...");

            float count = 0;
            List<DBMovieInfo> movies = DBMovieInfo.GetAll();
            float total = movies.Count;

            #region Upgrades required for 1.7.3 (Initial)

            // Do this once for all movies in this upgrade
            // as its an expensive task

            DirectoryInfo coverartFolder = null;
            DirectoryInfo coverartThumbFolder = null;
            DirectoryInfo backdropFolder = null;

            List<string> coverartFiles = null;
            List<string> coverartThumbFiles = null;
            List<string> backdropFiles = null;

            if (MovingPicturesCore.GetDBVersionNumber() < new Version("1.7.3")) {
                logger.Info("Getting current artwork stored on disk...");

                try {
                    // get artwork locations
                    coverartFolder = new DirectoryInfo(MovingPicturesCore.Settings.CoverArtFolder);
                    coverartThumbFolder = new DirectoryInfo(MovingPicturesCore.Settings.CoverArtThumbsFolder);
                    backdropFolder = new DirectoryInfo(MovingPicturesCore.Settings.BackdropFolder);

                    // get existing artwork
                    coverartFiles = coverartFolder.GetFiles().Select(f => f.FullName).ToList();
                    coverartThumbFiles = coverartThumbFolder.GetFiles().Select(f => f.FullName).ToList();
                    backdropFiles = backdropFolder.GetFiles().ToList().Select(f => f.FullName).ToList();;
                }
                catch (Exception e) {
                    logger.ErrorException("Failed to get current artwork from disk.", e);
                }

                logger.Info("Finished getting current artwork from disk.");
            }

            #endregion

            foreach (DBMovieInfo movie in movies.OrderBy(m => m.DateAdded)) {
                if (MaintenanceProgress != null) MaintenanceProgress("", (int)(count * 100 / total));
                count++;

                // Skip uncommited files
                if (movie.ID == null)
                    continue;

                #region Upgrades required for 0.7.1

                if (MovingPicturesCore.GetDBVersionNumber() < new Version("0.7.1")) {

                    if (movie.LocalMedia.Count > 0 && movie.LocalMedia[0].ImportPath != null) {
                        if (movie.LocalMedia[0].ImportPath.IsOpticalDrive && movie.LocalMedia[0].IsAvailable) {
                            movie.DateAdded = movie.LocalMedia[0].File.CreationTime;
                        }
                        else {
                            movie.DateAdded = movie.DateAdded.AddSeconds((double)movie.ID);
                        }
                    }
                }

                #endregion

                #region Upgrades required for 1.3.0

                if (MovingPicturesCore.GetDBVersionNumber() < new Version("1.3.0")) {
                    if (movie.Title.StartsWith("IMDb - ")) movie.Title = movie.Title.Replace("IMDb - ", "");
                }

                #endregion

                #region Upgrades required for 1.5.2

                if (MovingPicturesCore.GetDBVersionNumber() < new Version("1.5.2"))
                {
                    // populate initial release date as first day of the year
                    movie.ReleaseDate = new DateTime(movie.Year, 1, 1);
                }

                #endregion

                #region Upgrades required for 1.7.3
                // Upgrade filenames and db entries for coverart and backdrops.
                // Movies are sorted by data_added, first movie of its title should
                // have correct artwork, delete any artwork reference if another movie
                // of the same title is encountered so it will be refreshed.
                
                // NB: we don't need to do this but in the event that the user
                // deletes their database in the future, the local artwork provider
                // will be able to find existing artwork from this upgrade.

                if (MovingPicturesCore.GetDBVersionNumber() < new Version("1.7.3")) {
                    if (coverartFiles == null || coverartThumbFiles == null || backdropFiles == null)
                        continue;

                    string safeName = movie.Title.Replace(' ', '.').ToValidFilename();

                    #region Upgrade CoverFullPath
                    // ensure we get the thumb before it gets cleared from the 
                    // fullsize 'set', Otherwise we needlessly cause a re-generation of the thumb
                    string coverThumbFullPath = movie.CoverThumbFullPath;
                    string coverFullPath = movie.CoverFullPath;
                    string newCoverFullPath = string.Empty;

                    if (coverartFiles.Contains(coverFullPath)) {
                        // the source hash should be the last match
                        var matchCollection = Regex.Matches(coverFullPath, @"\[[^]]+\]");
                        string sourceHash = matchCollection[matchCollection.Count - 1].Value;

                        // build up new filename
                        newCoverFullPath = coverartFolder + "\\{" + safeName + "-" + movie.Year + "} " + sourceHash + Path.GetExtension(coverFullPath);

                        // rename old file to new file ID
                        if (coverFullPath.MoveTo(newCoverFullPath)) {
                            logger.Info("Successfully upgraded old coverart '{0}' to new format '{1}'.", coverFullPath, newCoverFullPath);
                            movie.CoverFullPath = newCoverFullPath;
                        }
                        else {
                            logger.Warn("Failed to upgrade old coverart '{0}' to new format.", coverFullPath);
                        }

                        // remove so we don't process again for another movie with same title
                        coverartFiles.Remove(coverFullPath);
                    }
                    else {
                        // looks like we have encountered another movie with the same title
                        // remove reference to it so we refresh artwork later for this movie
                        logger.Info("Removing coverart for '{0} ({1})', reference no longer exists on disk.", movie.Title, movie.Year);

                        coverartThumbFiles.Remove(coverThumbFullPath);
                        backdropFiles.Remove(movie.BackdropFullPath);
                        foreach (var altCover in movie.AlternateCovers) {
                            coverartFiles.Remove(altCover);
                        }

                        movie.CoverFullPath = string.Empty;
                        movie.CoverThumbFullPath = string.Empty;
                        movie.AlternateCovers = new StringList();
                        movie.BackdropFullPath = string.Empty;
                        movie.Commit();
                        continue;
                    }
                    #endregion

                    #region Upgrade AlternateCovers
                    var newAlternativeCovers = new StringList();
                    foreach (var altCover in movie.AlternateCovers) {
                        // the main cover will also be in the alternative covers as well.
                        // add the already processed cover to the list and move on
                        if (altCover == coverFullPath) {
                            newAlternativeCovers.Add(newCoverFullPath);
                            continue;
                        }

                        if (coverartFiles.Contains(altCover)) {
                            var matchCollection = Regex.Matches(altCover, @"\[[^]]+\]");
                            string sourceHash = matchCollection[matchCollection.Count - 1].Value;

                            // build up new filename
                            string newAlternativeCoverFullPath = coverartFolder + "\\{" + safeName + "-" + movie.Year + "} " + sourceHash + Path.GetExtension(altCover);

                            // rename old file to new file ID
                            if (altCover.MoveTo(newAlternativeCoverFullPath)) {
                                logger.Info("Successfully upgraded old coverart '{0}' to new format '{1}'.", altCover, newAlternativeCoverFullPath);
                                newAlternativeCovers.Add(newAlternativeCoverFullPath);
                            }
                            else {
                                logger.Warn("Failed to upgrade old coverart '{0}' to new format.", altCover);
                            }

                            coverartFiles.Remove(altCover);
                        }
                    }
                    movie.AlternateCovers = newAlternativeCovers;
                    #endregion

                    #region Upgrade CoverThumbFullPath
                    if (coverartThumbFiles.Contains(coverThumbFullPath)) {
                        // the source hash should be the last match
                        var matchCollection = Regex.Matches(coverThumbFullPath, @"\[[^]]+\]");
                        string sourceHash = matchCollection[matchCollection.Count - 1].Value;

                        // build up new filename
                        string newCoverThumbFullPath = coverartThumbFolder + "\\{" + safeName + "-" + movie.Year + "} " + sourceHash + Path.GetExtension(coverThumbFullPath);

                        // rename old file to new file ID
                        if (coverThumbFullPath.MoveTo(newCoverThumbFullPath)) {
                            logger.Info("Successfully upgraded old coverart thumb '{0}' to new format '{1}'.", coverThumbFullPath, newCoverThumbFullPath);
                            movie.CoverThumbFullPath = newCoverThumbFullPath;
                        }
                        else {
                            logger.Warn("Failed to upgrade old coverart thumb '{0}' to new format.", coverThumbFullPath);
                        }

                        // remove so we don't process again for another movie with same title
                        coverartThumbFiles.Remove(coverThumbFullPath);
                    }
                    #endregion

                    #region Upgrade Backdrop
                    string backdropFullPath = movie.BackdropFullPath;
                    if (backdropFiles.Contains(backdropFullPath)) {
                        // the source hash should be the last match
                        var matchCollection = Regex.Matches(backdropFullPath, @"\[[^]]+\]");
                        string sourceHash = matchCollection[matchCollection.Count - 1].Value;

                        // build up new filename
                        string newBackdropFullPath = backdropFolder + "\\{" + safeName + "-" + movie.Year + "} " + sourceHash + Path.GetExtension(backdropFullPath);

                        // rename old file to new file ID
                        if (backdropFullPath.MoveTo(newBackdropFullPath)) {
                            logger.Info("Successfully upgraded old backdrop '{0}' to new format '{1}'.", backdropFullPath, newBackdropFullPath);
                            movie.BackdropFullPath = newBackdropFullPath;
                        }
                        else
                        {
                            logger.Warn("Failed to upgrade old backdrop '{0}' to new format.", coverThumbFullPath);
                        }

                        // remove so we don't process again for another movie with same title
                        backdropFiles.Remove(backdropFullPath);
                    }
                    #endregion
                }

                #endregion

                // commit movie
                movie.Commit();
            }

            #region Upgrades required for 1.7.3 (Finalise)

            if (MovingPicturesCore.GetDBVersionNumber() < new Version("1.7.3")) {
                // finialise tasks for this upgrade
                // remove any artwork on disk not referenced in database.

                // This could probably be a standalone maintenance task later
                coverartFiles.ForEach(DeleteOrphanedArtwork);
                coverartThumbFiles.ForEach(DeleteOrphanedArtwork);
                backdropFiles.ForEach(DeleteOrphanedArtwork);
            }
            #endregion

            if (MaintenanceProgress != null) MaintenanceProgress("", 100);
        }

        /// <summary>
        /// Removed orphaned artwork in maintenance task for v1.7.3
        /// </summary>
        /// <param name="artwork">Path to artwork to remove</param>
        static void DeleteOrphanedArtwork(string artwork) {
            logger.Info("Removing orphaned artwork '{0}'.", artwork);

            try {
                File.Delete(artwork);
            }
            catch (Exception e) {
                logger.Warn("Failed to remove orphaned artwork '{0}', due to error '{1}'.", artwork, e.Message);
            }
        }

        // One time upgrades tasks for file information
        public static void PerformFileInformationUpgradeCheck() {
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            logger.Info("Performing File Information Upgrade Check...");

            float count = 0;
            List<DBLocalMedia> files = DBLocalMedia.GetAll();
            float total = files.Count;

            foreach (DBLocalMedia currFile in files) {
                if (MaintenanceProgress != null) MaintenanceProgress("", (int)(count * 100 / total));
                count++;

                // Skip uncommited files
                if (currFile.ID == null)
                    continue;

                #region Upgrades required for 1.0.0

                if (MovingPicturesCore.GetDBVersionNumber() < new Version("1.0.0")) {
                    
                    // Disk Information Upgrade
                    if (!currFile.ImportPath.IsOpticalDrive && currFile.ImportPath.IsAvailable) {
                        // Skip optical drives, unc paths and unavailable files
                        if (String.IsNullOrEmpty(currFile.VolumeSerial)) {
                            // perform update
                            currFile.UpdateVolumeInformation();
                            logger.Info("Disk information updated for '{0}'", currFile.FullPath);
                        }
                    }

                }

                #endregion

                // commit file
                currFile.Commit();
            }

            if (MaintenanceProgress != null) MaintenanceProgress("", 100);
        }

        // All other one time upgrades
        public static void PerformGenericUpgradeChecks() {
            Version ver = MovingPicturesCore.GetDBVersionNumber();
            if (ver == new Version("0.0.0.0")) return;
            
            logger.Info("Performing Miscellaneous Upgrade Checks...");

            #region 1.0.2 Upgrade Tasks

            if (ver < new Version("1.0.2")) {
                // grab file list
                List<FileInfo> files = new List<FileInfo>();
                files.AddRange(new DirectoryInfo(MovingPicturesCore.Settings.BackdropFolder).GetFilesRecursive());
                files.AddRange(new DirectoryInfo(MovingPicturesCore.Settings.BackdropThumbsFolder).GetFilesRecursive());
                files.AddRange(new DirectoryInfo(MovingPicturesCore.Settings.CoverArtFolder).GetFilesRecursive());
                files.AddRange(new DirectoryInfo(MovingPicturesCore.Settings.CoverArtThumbsFolder).GetFilesRecursive());

                float count = 0;
                float total = files.Count;

                // rename PNGs to JPGs
                foreach (FileInfo currFile in files) {
                    count++;
                    if (MaintenanceProgress != null) MaintenanceProgress("", (int)(count * 100 / total));

                    if (currFile.Extension.ToLower() == ".png")
                        currFile.MoveTo(currFile.Directory.FullName + "\\" + currFile.Name.Replace(".png", ".jpg"));
                }

            }

            #endregion

            #region 1.1.5 / 1.2.3 Upgrade Tasks

            if (ver < new Version("1.1.5") || (ver >= new Version("1.2.0") && ver < new Version("1.2.3"))) {

                // reset default rescan interval to 90 minutes. previously 0 = dont scan. this will not
                // enable rescan, just set a sensible default interval
                if (MovingPicturesCore.Settings.RescanNetworkPathsInterval == 0)
                    MovingPicturesCore.Settings.RescanNetworkPathsInterval = 90;
                else 
                    MovingPicturesCore.Settings.RescanNetworkPaths = true;
            }

            #endregion

            #region Upgrades required for 1.1.2

            if (MovingPicturesCore.GetDBVersionNumber() < new Version("1.1.2.1216")) {
                MovingPicturesCore.Settings.FollwitURLBase = "http://follw.it/";
                MovingPicturesCore.Settings.FollwitBatchSize = 30;
            }

            if (MovingPicturesCore.GetDBVersionNumber() < new Version("1.1.2")) {
                // force a full follw.it synchronization for follw.it ID changes, watched status changes and ratings changes as these are new features
                if (MovingPicturesCore.Settings.FollwitEnabled)
                    MovingPicturesCore.Follwit.ProcessTasks(DateTime.MinValue);
            }

            #endregion

            #region Upgrades required for 1.3.0

            if (MovingPicturesCore.GetDBVersionNumber() < new Version("1.3.0")) {
             
                if (MovingPicturesCore.Settings.DataProviderRequestLimit == 3) // old default
                    MovingPicturesCore.Settings.DataProviderRequestLimit = 0;
            }

            #endregion

            #region Upgrades required for 1.4.0

            if (MovingPicturesCore.GetDBVersionNumber() < new Version("1.4.0.1388")) {
                PropertyInfo property = typeof(MovingPicturesSettings).GetProperty("ArticlesForRemoval");
                CornerstoneSettingAttribute attribute = (CornerstoneSettingAttribute)Attribute.GetCustomAttribute(property, typeof(CornerstoneSettingAttribute));
                MovingPicturesCore.Settings.ArticlesForRemoval = attribute.Default.ToString();
            }

            #endregion

            #region Upgrades required for 1.4.2

            if (MovingPicturesCore.GetDBVersionNumber() < new Version("1.4.2.0")) {
                if (MovingPicturesCore.Settings.UserAgent == @"Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0; SLCC2)")
                    MovingPicturesCore.Settings.UserAgent = @"Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.95 Safari/537.11";
            }

            #endregion
        }

        #region Menu Verification

        #region Filters
        public static void VerifyFilterMenu() {
            DBMenu<DBMovieInfo> menu = MovingPicturesCore.Settings.FilterMenu;

            if (menu.RootNodes.Count == 0) {
                int position = 1;

                DBNode<DBMovieInfo> unwatchedNode = new DBNode<DBMovieInfo>();
                unwatchedNode.Name = "${UnwatchedMovies}";
                unwatchedNode.DynamicNode = false;
                unwatchedNode.Filter = new DBFilter<DBMovieInfo>();
                DBCriteria<DBMovieInfo> criteria = new DBCriteria<DBMovieInfo>();
                criteria.Field = DBField.GetFieldByDBName(typeof(DBUserMovieSettings), "watched");
                criteria.Relation = DBRelation.GetRelation(typeof(DBMovieInfo), typeof(DBUserMovieSettings), "");
                criteria.Operator = DBCriteria<DBMovieInfo>.OperatorEnum.EQUAL;
                criteria.Value = "0";
                unwatchedNode.Filter.Criteria.Add(criteria);
                unwatchedNode.SortPosition = position++;
                unwatchedNode.DBManager = MovingPicturesCore.DatabaseManager;
                menu.RootNodes.Add(unwatchedNode);

                DBNode<DBMovieInfo> genreNode = new DBNode<DBMovieInfo>();
                genreNode.DynamicNode = true;
                genreNode.BasicFilteringField = DBField.GetFieldByDBName(typeof(DBMovieInfo), "genres");
                genreNode.Name = "${Genre}";
                genreNode.DBManager = MovingPicturesCore.DatabaseManager;
                genreNode.SortPosition = position++;
                menu.RootNodes.Add(genreNode);

                DBNode<DBMovieInfo> yearNode = new DBNode<DBMovieInfo>();
                yearNode.DynamicNode = true;
                yearNode.BasicFilteringField = DBField.GetFieldByDBName(typeof(DBMovieInfo), "year");
                yearNode.Name = "${" + yearNode.BasicFilteringField.Name + "}";
                yearNode.DBManager = MovingPicturesCore.DatabaseManager;
                yearNode.SortPosition = position++;
                menu.RootNodes.Add(yearNode);

                DBNode<DBMovieInfo> certNode = new DBNode<DBMovieInfo>();
                certNode.DynamicNode = true;                                       
                certNode.BasicFilteringField = DBField.GetFieldByDBName(typeof(DBMovieInfo), "certification");
                certNode.Name = "${" + certNode.BasicFilteringField.Name + "}";
                certNode.DBManager = MovingPicturesCore.DatabaseManager;
                certNode.SortPosition = position++;
                menu.RootNodes.Add(certNode);

                DBNode<DBMovieInfo> dateNode = new DBNode<DBMovieInfo>();
                dateNode.DynamicNode = true;
                dateNode.BasicFilteringField = DBField.GetFieldByDBName(typeof(DBMovieInfo), "date_added");
                dateNode.Name = "${DateAdded}";
                dateNode.DBManager = MovingPicturesCore.DatabaseManager;
                dateNode.SortPosition = position++;
                menu.RootNodes.Add(dateNode);

                menu.Commit();
            }

            foreach (DBNode<DBMovieInfo> currNode in menu.RootNodes) {
                currNode.UpdateDynamicNode();
                currNode.Commit();
            }
        }
        #endregion

        #region Categories
        public static void VerifyCategoryMenu() {
            DBMenu<DBMovieInfo> menu = MovingPicturesCore.Settings.CategoriesMenu;

            if (menu.RootNodes.Count == 0) {
                int position = 1;

                DBNode<DBMovieInfo> allNode = new DBNode<DBMovieInfo>();
                allNode.Name = "${AllMovies}";
                allNode.DynamicNode = false;
                allNode.Filter = new DBFilter<DBMovieInfo>();
                allNode.SortPosition = position++;
                allNode.DBManager = MovingPicturesCore.DatabaseManager;
                menu.RootNodes.Add(allNode);

                DBNode<DBMovieInfo> unwatchedNode = new DBNode<DBMovieInfo>();
                unwatchedNode.Name = "${UnwatchedMovies}";
                unwatchedNode.DynamicNode = false;
                unwatchedNode.Filter = new DBFilter<DBMovieInfo>();
                DBCriteria<DBMovieInfo> criteria = new DBCriteria<DBMovieInfo>();
                criteria.Field = DBField.GetFieldByDBName(typeof(DBUserMovieSettings), "watched");
                criteria.Relation = DBRelation.GetRelation(typeof(DBMovieInfo), typeof(DBUserMovieSettings), "");
                criteria.Operator = DBCriteria<DBMovieInfo>.OperatorEnum.EQUAL;
                criteria.Value = "0";
                unwatchedNode.Filter.Criteria.Add(criteria);
                unwatchedNode.SortPosition = position++;
                unwatchedNode.DBManager = MovingPicturesCore.DatabaseManager;
                menu.RootNodes.Add(unwatchedNode);

                DBNode<DBMovieInfo> recentNode = new DBNode<DBMovieInfo>();
                recentNode.Name = "${RecentlyAddedMovies}";
                recentNode.DynamicNode = false;
                recentNode.Filter = new DBFilter<DBMovieInfo>();
                recentNode.SortPosition = position++;
                recentNode.DBManager = MovingPicturesCore.DatabaseManager;
                
                DBCriteria<DBMovieInfo> recentCriteria = new DBCriteria<DBMovieInfo>();
                recentCriteria.Field = DBField.GetFieldByDBName(typeof(DBMovieInfo), "date_added");
                recentCriteria.Operator = DBCriteria<DBMovieInfo>.OperatorEnum.GREATER_THAN;
                recentCriteria.Value = "-30d";
                recentNode.Filter.Criteria.Add(recentCriteria);

                DBMovieNodeSettings additionalSettings = new DBMovieNodeSettings();
                additionalSettings.UseDefaultSorting = false;
                additionalSettings.SortField = SortingFields.DateAdded;
                additionalSettings.SortDirection = SortingDirections.Descending;
                recentNode.AdditionalSettings = additionalSettings;
                menu.RootNodes.Add(recentNode);

                DBNode<DBMovieInfo> genreNode = new DBNode<DBMovieInfo>();
                genreNode.DynamicNode = true;
                genreNode.BasicFilteringField = DBField.GetFieldByDBName(typeof(DBMovieInfo), "genres");
                genreNode.Name = "${Genres}";
                genreNode.SortPosition = position++;
                genreNode.DBManager = MovingPicturesCore.DatabaseManager;
                menu.RootNodes.Add(genreNode);

                DBNode<DBMovieInfo> certNode = new DBNode<DBMovieInfo>();
                certNode.DynamicNode = true;
                certNode.BasicFilteringField = DBField.GetFieldByDBName(typeof(DBMovieInfo), "certification");
                certNode.Name = "${" + certNode.BasicFilteringField.Name + "}";
                certNode.DBManager = MovingPicturesCore.DatabaseManager;
                certNode.SortPosition = position++;
                menu.RootNodes.Add(certNode);

                DBNode<DBMovieInfo> yearNode = new DBNode<DBMovieInfo>();
                yearNode.DynamicNode = true;
                yearNode.BasicFilteringField = DBField.GetFieldByDBName(typeof(DBMovieInfo), "year");
                yearNode.Name = "${" + yearNode.BasicFilteringField.Name + "}";
                yearNode.SortPosition = position++;
                yearNode.DBManager = MovingPicturesCore.DatabaseManager;
                menu.RootNodes.Add(yearNode);

                DBNode<DBMovieInfo> alphaNode = new DBNode<DBMovieInfo>();
                alphaNode.DynamicNode = true;
                alphaNode.BasicFilteringField = DBField.GetFieldByDBName(typeof(DBMovieInfo), "title");
                alphaNode.Name = "${BeginsWith}";
                alphaNode.DBManager = MovingPicturesCore.DatabaseManager;
                alphaNode.SortPosition = position++;
                menu.RootNodes.Add(alphaNode);

                menu.Commit();
            }

            foreach (DBNode<DBMovieInfo> currNode in menu.RootNodes) {
                currNode.UpdateDynamicNode();
                currNode.Commit();
            }
        }
        #endregion

        #region Movie Manager Filters
        public static void VerifyMovieManagerFilterMenu() {
            DBMenu<DBMovieInfo> menu = MovingPicturesCore.Settings.MovieManagerFilterMenu;

            if (menu.RootNodes.Count == 0) {
                int position = 1;

                DBNode<DBMovieInfo> allNode = new DBNode<DBMovieInfo>();
                allNode.Name = "${AllMovies}";
                allNode.DynamicNode = false;
                allNode.Filter = new DBFilter<DBMovieInfo>();
                allNode.SortPosition = position++;
                allNode.DBManager = MovingPicturesCore.DatabaseManager;
                menu.RootNodes.Add(allNode);

                DBNode<DBMovieInfo> unwatchedNode = new DBNode<DBMovieInfo>();
                unwatchedNode.Name = "${UnwatchedMovies}";
                unwatchedNode.DynamicNode = false;
                unwatchedNode.Filter = new DBFilter<DBMovieInfo>();
                DBCriteria<DBMovieInfo> criteria = new DBCriteria<DBMovieInfo>();
                criteria.Field = DBField.GetFieldByDBName(typeof(DBUserMovieSettings), "watched");
                criteria.Relation = DBRelation.GetRelation(typeof(DBMovieInfo), typeof(DBUserMovieSettings), "");
                criteria.Operator = DBCriteria<DBMovieInfo>.OperatorEnum.EQUAL;
                criteria.Value = "0";
                unwatchedNode.Filter.Criteria.Add(criteria);
                unwatchedNode.SortPosition = position++;
                unwatchedNode.DBManager = MovingPicturesCore.DatabaseManager;
                menu.RootNodes.Add(unwatchedNode);

                DBNode<DBMovieInfo> recentNode = new DBNode<DBMovieInfo>();
                recentNode.Name = "${RecentlyAddedMovies}";
                recentNode.DynamicNode = false;
                recentNode.Filter = new DBFilter<DBMovieInfo>();
                recentNode.SortPosition = position++;
                recentNode.DBManager = MovingPicturesCore.DatabaseManager;

                DBCriteria<DBMovieInfo> recentCriteria = new DBCriteria<DBMovieInfo>();
                recentCriteria.Field = DBField.GetFieldByDBName(typeof(DBMovieInfo), "date_added");
                recentCriteria.Operator = DBCriteria<DBMovieInfo>.OperatorEnum.GREATER_THAN;
                recentCriteria.Value = "-30d";
                recentNode.Filter.Criteria.Add(recentCriteria);

                DBMovieNodeSettings additionalSettings = new DBMovieNodeSettings();
                additionalSettings.UseDefaultSorting = false;
                additionalSettings.SortField = SortingFields.DateAdded;
                additionalSettings.SortDirection = SortingDirections.Descending;
                recentNode.AdditionalSettings = additionalSettings;
                menu.RootNodes.Add(recentNode);

                DBNode<DBMovieInfo> genreNode = new DBNode<DBMovieInfo>();
                genreNode.DynamicNode = true;
                genreNode.BasicFilteringField = DBField.GetFieldByDBName(typeof(DBMovieInfo), "genres");
                genreNode.Name = "${Genres}";
                genreNode.SortPosition = position++;
                genreNode.DBManager = MovingPicturesCore.DatabaseManager;
                menu.RootNodes.Add(genreNode);

                DBNode<DBMovieInfo> certNode = new DBNode<DBMovieInfo>();
                certNode.DynamicNode = true;
                certNode.BasicFilteringField = DBField.GetFieldByDBName(typeof(DBMovieInfo), "certification");
                certNode.Name = "${" + certNode.BasicFilteringField.Name + "}";
                certNode.DBManager = MovingPicturesCore.DatabaseManager;
                certNode.SortPosition = position++;
                menu.RootNodes.Add(certNode);

                DBNode<DBMovieInfo> yearNode = new DBNode<DBMovieInfo>();
                yearNode.DynamicNode = true;
                yearNode.BasicFilteringField = DBField.GetFieldByDBName(typeof(DBMovieInfo), "year");
                yearNode.Name = "${" + yearNode.BasicFilteringField.Name + "}";
                yearNode.SortPosition = position++;
                yearNode.DBManager = MovingPicturesCore.DatabaseManager;
                menu.RootNodes.Add(yearNode);

                DBNode<DBMovieInfo> alphaNode = new DBNode<DBMovieInfo>();
                alphaNode.DynamicNode = true;
                alphaNode.BasicFilteringField = DBField.GetFieldByDBName(typeof(DBMovieInfo), "title");
                alphaNode.Name = "${BeginsWith}";
                alphaNode.DBManager = MovingPicturesCore.DatabaseManager;
                alphaNode.SortPosition = position++;
                menu.RootNodes.Add(alphaNode);

                // maintenance node
                DBNode<DBMovieInfo> maintenanceNode = new DBNode<DBMovieInfo>();
                maintenanceNode.Name = "${Missing}";
                maintenanceNode.DynamicNode = false;
                maintenanceNode.SortPosition = position++;
                maintenanceNode.DBManager = MovingPicturesCore.DatabaseManager;
                menu.RootNodes.Add(maintenanceNode);

                // missing covers node
                DBNode<DBMovieInfo> missingCoversNode = new DBNode<DBMovieInfo>();
                missingCoversNode.Name = "${Cover}";
                missingCoversNode.Parent = maintenanceNode;
                missingCoversNode.DynamicNode = false;
                missingCoversNode.Filter = new DBFilter<DBMovieInfo>();
                missingCoversNode.SortPosition = position++;
                missingCoversNode.DBManager = MovingPicturesCore.DatabaseManager;
                maintenanceNode.Children.Add(missingCoversNode);

                DBCriteria<DBMovieInfo> missingCoversCriteria = new DBCriteria<DBMovieInfo>();
                missingCoversCriteria.Field = DBField.GetFieldByDBName(typeof(DBMovieInfo), "coverfullpath");
                missingCoversCriteria.Operator = DBCriteria<DBMovieInfo>.OperatorEnum.EQUAL;
                missingCoversCriteria.Value = "";
                missingCoversNode.Filter.Criteria.Add(missingCoversCriteria);

                // missing backdrops node
                DBNode<DBMovieInfo> missingBackdropsNode = new DBNode<DBMovieInfo>();
                missingBackdropsNode.Name = "${Backdrop}";
                missingBackdropsNode.Parent = maintenanceNode;
                missingBackdropsNode.DynamicNode = false;
                missingBackdropsNode.Filter = new DBFilter<DBMovieInfo>();
                missingBackdropsNode.SortPosition = position++;
                missingBackdropsNode.DBManager = MovingPicturesCore.DatabaseManager;
                maintenanceNode.Children.Add(missingBackdropsNode);

                DBCriteria<DBMovieInfo> missingBackdropsCriteria = new DBCriteria<DBMovieInfo>();
                missingBackdropsCriteria.Field = DBField.GetFieldByDBName(typeof(DBMovieInfo), "backdropfullpath");
                missingBackdropsCriteria.Operator = DBCriteria<DBMovieInfo>.OperatorEnum.EQUAL;
                missingBackdropsCriteria.Value = "";
                missingBackdropsNode.Filter.Criteria.Add(missingBackdropsCriteria);

                // invalid years, only 1900 -> next year considered valid
                DBNode<DBMovieInfo> invalidYearsNode = new DBNode<DBMovieInfo>();
                invalidYearsNode.Name = "${Year}";
                invalidYearsNode.Parent = maintenanceNode;
                invalidYearsNode.DynamicNode = false;
                invalidYearsNode.Filter = new DBFilter<DBMovieInfo>();
                invalidYearsNode.Filter.CriteriaGrouping = DBFilter<DBMovieInfo>.CriteriaGroupingEnum.ONE;
                invalidYearsNode.SortPosition = position++;
                invalidYearsNode.DBManager = MovingPicturesCore.DatabaseManager;
                maintenanceNode.Children.Add(invalidYearsNode);

                DBCriteria<DBMovieInfo> invalidYearsCriteria = new DBCriteria<DBMovieInfo>();
                invalidYearsCriteria.Field = DBField.GetFieldByDBName(typeof(DBMovieInfo), "year");
                invalidYearsCriteria.Operator = DBCriteria<DBMovieInfo>.OperatorEnum.LESS_THAN;
                invalidYearsCriteria.Value = 1900;
                invalidYearsNode.Filter.Criteria.Add(invalidYearsCriteria);
                invalidYearsCriteria = new DBCriteria<DBMovieInfo>();
                invalidYearsCriteria.Field = DBField.GetFieldByDBName(typeof(DBMovieInfo), "year");
                invalidYearsCriteria.Operator = DBCriteria<DBMovieInfo>.OperatorEnum.GREATER_THAN;
                invalidYearsCriteria.Value = 2030;
                invalidYearsNode.Filter.Criteria.Add(invalidYearsCriteria);

                // missing mediainfo node
                DBNode<DBMovieInfo> missingMediaInfoNode = new DBNode<DBMovieInfo>();
                missingMediaInfoNode.Name = "${MediaInfo}";
                missingMediaInfoNode.Parent = maintenanceNode;
                missingMediaInfoNode.DynamicNode = false;
                missingMediaInfoNode.Filter = new DBFilter<DBMovieInfo>();
                missingMediaInfoNode.SortPosition = position++;
                missingMediaInfoNode.DBManager = MovingPicturesCore.DatabaseManager;
                maintenanceNode.Children.Add(missingMediaInfoNode);

                DBCriteria<DBMovieInfo> missingMediaInfoCriteria = new DBCriteria<DBMovieInfo>();
                missingMediaInfoCriteria.Field = DBField.GetFieldByDBName(typeof(DBLocalMedia), "videowidth");
                missingMediaInfoCriteria.Relation = DBRelation.GetRelation(typeof(DBMovieInfo), typeof(DBLocalMedia), "");
                missingMediaInfoCriteria.Operator = DBCriteria<DBMovieInfo>.OperatorEnum.EQUAL;
                missingMediaInfoCriteria.Value = 0;
                missingMediaInfoNode.Filter.Criteria.Add(missingMediaInfoCriteria);

                menu.Commit();
            }

            foreach (DBNode<DBMovieInfo> currNode in menu.RootNodes) {
                currNode.UpdateDynamicNode();
                currNode.Commit();
            }
        }
        #endregion

        #endregion
    }
}
