using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Cornerstone.Tools;
using Cornerstone.Extensions;
using Cornerstone.Extensions.IO;
using MediaPortal.Util;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {

    public enum MountResult {
        Failed,
        Pending,
        Success
    }

   
    class Utility {

        #region Ctor / Private variables
        
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Utility() {

        }

        #endregion        

        #region FileSystem Methods     

        /// <summary>
        /// This method will dump the complete file/directory structure to the log
        /// </summary>
        /// <param name="path"></param>
        public static void LogDirectoryStructure(string path) {
            DirectoryInfo dir = new DirectoryInfo(path);
            List<FileInfo> fileList = dir.GetFilesRecursive();
            StringBuilder structure = new StringBuilder();
            structure.AppendLine("Listing files for: " + path);
            structure.AppendLine(""); // append a blank line
            foreach (FileInfo file in fileList)
                structure.AppendLine(file.FullName); // append full path to file and newline
            
            // Dump the file structure to the log
            logger.Debug(structure.ToString());
        }

        #region Multi-part / Stacking

        // Regular expression patterns used by the multipart detection and cleaning methods
        // Matches the substrings "cd/dvd/disc/disk/part #" or "(# of #)"
        // todo: convert constants to advanced settings
        private const string rxFileStackPattern = @"(\W*\b(cd|dvd|dis[ck]|part)\W*([a-z]|\d+|i+)\W*)|\W\d+\W*(of|-)\W*\d+\W$";
        private const string rxFolderStackPattern = @"^(cd|dvd|dis[ck]|part)\W*([a-z]|\d+|i+)$";

        /// <summary>
        /// Checks if a filename has stack markers (and is multi-part)
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>true if multipart, false if not</returns>
        public static bool isFileMultiPart(string fileName) {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            Regex expr = new Regex(rxFileStackPattern, RegexOptions.IgnoreCase);
            return expr.Match(fileName).Success;
        }

        public static bool isFileMultiPart(FileInfo file) {
            return isFileMultiPart(file.Name);
        }

        /// <summary>
        /// Checks if the foldername is a multipart marker.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool isFolderMultipart(string name) {
            Regex expr = new Regex(rxFolderStackPattern, RegexOptions.IgnoreCase);
            return expr.Match(name).Success;
        }

        /// <summary>
        /// Remove extension and stackmarkers from a filename
        /// </summary>
        /// <param name="fileName">Filename including the extension</param>
        /// <returns>the filename without stackmarkers and extension</returns>
        public static string GetFileNameWithoutExtensionAndStackMarkers(string fileName) {
            
            // Remove the file extension from the filename
            string cleanFileName = Path.GetFileNameWithoutExtension(fileName);
            
            // If file is classified as multipart clean the stack markers.
            if (isFileMultiPart(fileName)) {                
                Regex expr = new Regex(rxFileStackPattern, RegexOptions.IgnoreCase);
                cleanFileName = expr.Replace(cleanFileName, "");
            }

            // Return cleanFileName cleaned filename
            return cleanFileName;
        }

        public static string GetFileNameWithoutExtensionAndStackMarkers(FileInfo fileInfo) {
            return GetFileNameWithoutExtensionAndStackMarkers(fileInfo.Name);
        }

        #endregion

        #endregion

        #region General Methods (Unsorted)

        /// <summary>
        /// Checks if the foldername is ambigious (non descriptive)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool isKnownSubFolder(string name) {
            string[] folders = new string[] { 
                "video_ts", "hvdvd_ts", "adv_obj", "bdmv", "vcd", "cdda", "ext", "cdi",
                "stream", "playlist", "clipinf", "backup", "mpegav"
            };

            // Name is marked as being multi-part
            if (isFolderMultipart(name))
                return true;

            // Ignore specific names
            foreach (string folderName in folders) {
                if (name.Equals(folderName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        } 

        /// <summary>
        /// Checks if the directory is a logical drive root
        /// </summary>
        /// <param name="path"></param>
        /// <returns>True if this is a root</returns>
        public static bool IsDriveRoot(DirectoryInfo directory) {
            return IsDriveRoot(directory.FullName);
        }
        
        /// <summary>
        /// Checks if the given path is a logical drive root
        /// </summary>
        /// <param name="path"></param>
        /// <returns>True if this is a root</returns>
        public static bool IsDriveRoot(string path) {
            if (path.Length > 1 && path.Length < 4)
                return (path.Substring(1, 1) == ":");

            return false;
        }

        /// <summary>
        /// Returns the base directory for the movie files that are part of the input directory
        /// </summary>
        /// <param name="directory">directory to start in</param>
        /// <returns>the base directory for the movie files</returns>
        public static DirectoryInfo GetMovieBaseDirectory(DirectoryInfo directory) {
            DirectoryInfo dirLevel = directory;
            while (isKnownSubFolder(dirLevel.Name) && dirLevel.Parent != null)
                dirLevel = dirLevel.Parent;

            return dirLevel;
        }
    
        /// <summary>
        /// Checks if a folder contains a maximum amount of video files
        /// This is used to determine if a folder is dedicated to one movie
        /// </summary>
        /// <param name="folder">the directory to check</param>
        /// <param name="expectedCount">maximum count</param>
        /// <returns>True if folder is dedicated</returns>
        public static bool isFolderDedicated(DirectoryInfo folder, int expectedCount) {
            return (VideoUtility.GetVideoFileCount(folder) <= expectedCount);
        }

        #endregion
        
        /// <summary>
        /// Get a WebGrabber instance with the default moving pictures settings
        /// </summary>
        /// <param name="url">url to resource</param>
        /// <returns>webgrabber instance</returns>
        public static WebGrabber GetWebGrabberInstance(string url) {
            WebGrabber grabber = new WebGrabber(url);
            grabber.UserAgent = "MovingPictures/" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            grabber.MaxRetries = MovingPicturesCore.Settings.MaxTimeouts;
            grabber.Timeout = MovingPicturesCore.Settings.TimeoutLength;
            grabber.TimeoutIncrement = MovingPicturesCore.Settings.TimeoutIncrement;
            return grabber;
        }

        #region Mounting

        // note: These methods are wrappers around the Daemon Tools class supplied by mediaportal
        // we use this wrappers so we can move to other mounting logic in the future more easily.

        public static MountResult MountImage(string imagePath) {
            // Max cycles to try/wait when the virtual drive is not ready
            // after mounting it. One cycle is roughly 1/10 of a second 
           return Utility.MountImage(imagePath, 100);
        }

        public static MountResult MountImage(string imagePath, int maxWaitCycles) {
            string drive;

            // Check if the current image is already mounted
            if (!Utility.IsMounted(imagePath)) {
                logger.Info("Mounting image...");
                if (!DaemonTools.Mount(imagePath, out drive)) {
                    // there was a mounting error
                    logger.Error("Mounting image failed.");
                    return MountResult.Failed;
                }
            }
            else {
                // if the image was already mounted grab the drive letter
                drive = DaemonTools.GetVirtualDrive();
                // only check the drive once before reporting that the mounting is still pending
                maxWaitCycles = 1;
            }

            // Check if the mounted drive is ready to be read
            logger.Info("Mounted: Image='{0}', Drive={1}", imagePath, drive);

            int driveCheck = 0;
            while (true) {
                driveCheck++;
                // Try to create a DriveInfo object with the returned driveletter
                try {
                    DriveInfo d = new DriveInfo(drive);
                    if (d.IsReady) {
                        // This line will list the complete file structure of the image
                        // Output will only show when the log is set to DEBUG.
                        // Purpose of method is troubleshoot different image structures.
                        Utility.LogDirectoryStructure(drive);
                        return MountResult.Success;
                    }
                }
                catch (ArgumentNullException e) {
                    // The driveletter returned by Daemon Tools is invalid
                    logger.DebugException("Daemon Tools returned an invalid driveletter", e);
                    return MountResult.Failed;
                }
                catch (ArgumentException) {
                    // this exception happens when the driveletter is valid but the driveletter is not 
                    // finished mounting yet (at least not known to the system). We only need to catch
                    // this to stay in the loop
                }

                if (driveCheck == maxWaitCycles) {
                    return MountResult.Pending;
                }
                else if (maxWaitCycles == 1) {
                    logger.Info("Waiting for virtual drive to become available...");
                }

                // Sleep for a bit
                Thread.Sleep(100);
            }
        }

        public static bool IsImageFile(string imagePath) {
            return DaemonTools.IsImageFile(Path.GetExtension(imagePath));
        }
        
        public static bool IsMounted(string imagePath) {
            return DaemonTools.IsMounted(imagePath);
        }

        public static void UnMount(string imagePath) {
            if (IsMounted(imagePath)) {
                DaemonTools.UnMount();
                logger.Info("Unmounted: Image='{0}'", imagePath);
            }
        }

        public static string GetMountedVideoDiscPath(string imagePath) {
            if (!IsMounted(imagePath))
                return null;

           string drive = DaemonTools.GetVirtualDrive();
           return VideoUtility.GetVideoPath(drive);
        }

        #endregion

        #region MediaPortal

        /// <summary>
        /// Checks if file has valid video extensions (as specified by media portal
        /// </summary>
        /// <remarks>
        /// MediaPortal Dependency!
        /// </remarks>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsMediaPortalVideoFile(FileInfo file) {
            foreach (string ext in MediaPortal.Util.Utils.VideoExtensions) {
                if (file.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public static bool IsMediaPortalVideoFile(string file) {
            FileInfo fileInfo = new FileInfo(file);
            return IsMediaPortalVideoFile(fileInfo);
        }

        #endregion        
        
        #region MovieHash
        
        public static string GetMovieHashString(string filename) {
            FileInfo file = new FileInfo(filename);
            string hash = file.ComputeSmartHash();
            if (hash==null)
                logger.Error("Error while computing hash for '{0}'.", filename);

            return hash;
        }

        #endregion


    }
}
