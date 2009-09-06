using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Cornerstone.Tools;
using MediaPortal.Util;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {

    public enum MountResult {
        Failed,
        Pending,
        Success
    }

    #region String Extensions

    static class StringExtensions {

        // Regular expression pattern that matches a selection of non-word characters
        private const string rxMatchNonWordCharacters = @"[^\w]";

        /// <summary>
        /// Returns the string converted to a sortable string (The String -> String, The)
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string ToSortable(this String self) {
            Regex expr = new Regex(@"^(" + MovingPicturesCore.Settings.ArticlesForRemoval + @")\s(.+)", RegexOptions.IgnoreCase);
            return expr.Replace(self, "$2, $1").Trim();
        }

        /// <summary>
        /// Returns the string as converted from a sortable string (String, The -> The String)
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string FromSortable(this String self) {
            Regex expr = new Regex(@"(.+?)(?:, (" + MovingPicturesCore.Settings.ArticlesForRemoval + @"))?\s*$", RegexOptions.IgnoreCase);
            return expr.Replace(self, "$2 $1").Trim();
        }

        /// <summary>
        /// Filters non descriptive words/characters from a title so that only keywords remain.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string ToKeywords(this String self) {

            // Remove articles and non-descriptive words
            string newTitle = Regex.Replace(self, @"\b(" + MovingPicturesCore.Settings.ArticlesForRemoval + @")\b", "", RegexOptions.IgnoreCase);
            newTitle = Regex.Replace(newTitle, @"\b(and|or|of|und|en|et|y)\b", "", RegexOptions.IgnoreCase);

            // Replace non-word characters with spaces
            newTitle = Regex.Replace(newTitle, rxMatchNonWordCharacters, " ");

            // Remove double spaces and return the keywords
            return newTitle.TrimWhiteSpace();
        }

        /// <summary>
        /// Replaces multiple white-spaces with one space
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string TrimWhiteSpace(this String self) {
            return Regex.Replace(self, @"\s{2,}", " ").Trim();
        }

        /// <summary>
        /// Returns and converts the string into a common format.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string Equalize(this String self) {
            if (self == null) return string.Empty;

            // Convert title to lowercase culture invariant
            string newTitle = self.ToLowerInvariant();

            // Swap article
            newTitle = newTitle.FromSortable();

            // Replace non-descriptive characters with spaces
            newTitle = Regex.Replace(newTitle, rxMatchNonWordCharacters, " ");

            // Equalize: Convert to base character string
            newTitle = newTitle.RemoveDiacritics();

            // Equalize: Common characters with words of the same meaning
            newTitle = Regex.Replace(newTitle, @"\b(and|und|en|et|y)\b", " & ");

            // Equalize: Roman Numbers To Numeric
            newTitle = Regex.Replace(newTitle, @"\si(\b)", @" 1$1");
            newTitle = Regex.Replace(newTitle, @"\sii(\b)", @" 2$1");
            newTitle = Regex.Replace(newTitle, @"\siii(\b)", @" 3$1");
            newTitle = Regex.Replace(newTitle, @"\siv(\b)", @" 4$1");
            newTitle = Regex.Replace(newTitle, @"\sv(\b)", @" 5$1");
            newTitle = Regex.Replace(newTitle, @"\svi(\b)", @" 6$1");
            newTitle = Regex.Replace(newTitle, @"\svii(\b)", @" 7$1");
            newTitle = Regex.Replace(newTitle, @"\sviii(\b)", @" 8$1");
            newTitle = Regex.Replace(newTitle, @"\six(\b)", @" 9$1");

            // Remove the number 1 from the end of a title string
            newTitle = Regex.Replace(newTitle, @"\s(1)$", "");


            // Remove double spaces and return the cleaned title
            return newTitle.TrimWhiteSpace();
        }

        /// <summary>
        /// Translates characters to their base form. ( ë/é/è -> e)
        /// </summary>
        /// <example>
        /// characters: ë, é, è
        /// result: e
        /// </example>
        /// <remarks>
        /// source: http://blogs.msdn.com/michkap/archive/2007/05/14/2629747.aspx
        /// </remarks>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string RemoveDiacritics(this String self) {
            string stFormD = self.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < stFormD.Length; ich++) {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark) {
                    sb.Append(stFormD[ich]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

    }

    #endregion

    class Utility {

        #region Ctor / Private variables
        
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Utility() {

        }

        #endregion        

        #region Enum Helper Methods

        public static string GetEnumValueDescription(object value) {
            Type objType = value.GetType();
            FieldInfo fieldInfo = objType.GetField(Enum.GetName(objType, value));
            DescriptionAttribute attribute = (DescriptionAttribute)
            (fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false)[0]);

            // Return the description.
            return attribute.Description;
        }

        #endregion

        #region FileSystem Methods

        /// <summary>
        /// Get all files from directory and it's subdirectories.
        /// </summary>
        /// <param name="inputDir"></param>
        /// <returns></returns>
        public static List<FileInfo> GetFilesRecursive(DirectoryInfo directory) {
            List<FileInfo> fileList = new List<FileInfo>();
            DirectoryInfo[] subdirectories = new DirectoryInfo[] { };

            try {
                fileList.AddRange(directory.GetFiles("*"));
                subdirectories = directory.GetDirectories();
            }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;

                logger.Debug("Error while retrieving files/directories for: {0} {1}", directory.FullName, e);
            }

            foreach (DirectoryInfo subdirectory in subdirectories) {
                try {
                    if ((subdirectory.Attributes & FileAttributes.System) == 0)
                        fileList.AddRange(GetFilesRecursive(subdirectory));
                    else
                        logger.Debug("Rejecting directory {0} because it is flagged as a System folder.", subdirectory.FullName);
                }
                catch (Exception e) {
                    if (e.GetType() == typeof(ThreadAbortException))
                        throw e;

                    logger.Debug("Error during attribute check for: {0} {1}", subdirectory.FullName, e);
                }
            }

            return fileList;
        } 

        /// <summary>
        /// This method will dump the complete file/directory structure to the log
        /// </summary>
        /// <param name="path"></param>
        public static void LogDirectoryStructure(string path) {
            DirectoryInfo dir = new DirectoryInfo(path);
            List<FileInfo> fileList = GetFilesRecursive(dir);
            StringBuilder structure = new StringBuilder();
            structure.AppendLine("Listing files for: " + path);
            structure.AppendLine(""); // append a blank line
            foreach (FileInfo file in fileList)
                structure.AppendLine(file.FullName); // append full path to file and newline
            
            // Dump the file structure to the log
            logger.Debug(structure.ToString());
        }
        
        /// <summary>
        /// This method will create a string that can be safely used as a filename.
        /// </summary>
        /// <param name="subject">the string to process</param>
        /// <returns>the processed string</returns>
        public static string CreateFilename(string subject) {
            if (String.IsNullOrEmpty(subject))
                return string.Empty;

            string rtFilename = subject;

            char[] invalidFileChars = System.IO.Path.GetInvalidFileNameChars();
            foreach (char invalidFileChar in invalidFileChars)
                rtFilename = rtFilename.Replace(invalidFileChar, '_');

            return rtFilename;
        }

        #region Multi-part / Stacking

        // Regular expression patterns used by the multipart detection and cleaning methods
        private const string rxStackKeywords = @"(cd|dvd|dis[ck]|part)";
        private const string rxStackPatterns = @"(\W*\b" + rxStackKeywords + @"\W*([a-c]|\d+|i+)\W*)|[\(\[]\d(of|-)\d[\)\]]$";

        /// <summary>
        /// Checks if a filename has stack markers (and is multi-part)
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>true if multipart, false if not</returns>
        public static bool isFileMultiPart(string fileName) {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            Regex expr = new Regex(rxStackPatterns + @"|[^\s\d](\d+)$|([a-c])$", RegexOptions.IgnoreCase);
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
            Regex expr = new Regex(@"^" + rxStackKeywords + @"\W*\d+$", RegexOptions.IgnoreCase);
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

                Regex expr = new Regex(rxStackPatterns, RegexOptions.IgnoreCase);
                Match match = expr.Match(cleanFileName);

                // if we have a match on this expression we will remove the complete match.
                if (match.Success)
                    cleanFileName = expr.Replace(cleanFileName, "");
                // no match means we just remove one character
                else
                    cleanFileName = cleanFileName.Substring(0, (cleanFileName.Length - 1));
            }

            // Return cleanFileName cleaned filename
            return cleanFileName;
        }

        public static string GetFileNameWithoutExtensionAndStackMarkers(FileInfo fileInfo) {
            return GetFileNameWithoutExtensionAndStackMarkers(fileInfo.Name);
        }

        #endregion

        /// <summary>
        /// Get the largest file from a directory matching the specified file mask
        /// </summary>
        /// <param name="targetDir">the directory to scan</param>
        /// <param name="fileMask">the filemask to match</param>
        /// <returns>path to the largest file or null if no file was found or an error occured</returns>
        public static string GetLargestFileInDirectory(DirectoryInfo targetDir, string fileMask) {
            string largestFile = null;
            long largestSize = 0;
            try {
                FileInfo[] files = targetDir.GetFiles(fileMask);
                foreach (FileInfo file in files) {
                    long fileSize = file.Length;
                    if (fileSize > largestSize) {
                        largestSize = fileSize;
                        largestFile = file.FullName;
                    }
                }
            }
            catch (Exception e) {
                if (e is ThreadAbortException)
                    throw e;

                logger.ErrorException("Error while retrieving files for: " + targetDir.FullName, e);
            }
            return largestFile;
        }

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
            if (IsMounted(imagePath))
                DaemonTools.UnMount();
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
        
        /// <summary>
        /// Generates a SHA1-Hash from a given filepath
        /// </summary>
        /// <param name="filePath">path to the file</param>
        /// <returns>hash as an hexadecimal string </returns>
        public static string ComputeSHA1Hash(string filePath) {
            string hashHex = null;
            if (File.Exists(filePath)) {
                Stream file = null;
                try {
                    file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    HashAlgorithm hashObj = new SHA1Managed();
                    byte[] hash = hashObj.ComputeHash(file);
                    hashHex = BitConverter.ToString(hash);
                    logger.Debug("SHA1: Success, File='{0}', Hash='{1}'", filePath, hashHex);
                }
                catch (Exception e) {
                    if (e.GetType() == typeof(ThreadAbortException))
                        throw e;

                    logger.DebugException("SHA1: Failed, File='" + hashHex + "' ", e);
                }
                finally {
                    if (file != null)
                        file.Close();
                }
            }
            else {
                // File does not exist
                logger.Debug("SHA1: Failed, File='{0}', Reason='File is not available'", filePath);
            }

            // Return
            return hashHex;
        }

        #region MovieHash
        
        public static string GetMovieHashString(string filename) {
            string hash;
            try {
                byte[] moviehash = ComputeMovieHash(filename);
                hash = ToHexadecimal(moviehash);
            }
            catch (Exception e) {
                logger.Error("Error while generating FileHash for: " + filename, e);
                hash = null;
            }
            return hash;
        }
        
        private static byte[] ComputeMovieHash(string filename) {
            byte[] result;
            using (Stream input = File.OpenRead(filename)) {
                result = ComputeMovieHash(input);
            }
            return result;
        }

        private static byte[] ComputeMovieHash(Stream input) {
            ulong lhash;
            long streamsize;
            streamsize = input.Length;
            lhash = (ulong)streamsize;

            long i = 0;
            byte[] buffer = new byte[sizeof(long)];
            input.Position = 0;
            while (i < 65536 / sizeof(long) && (input.Read(buffer, 0, sizeof(long)) > 0)) {
                i++;
                unchecked { lhash += BitConverter.ToUInt64(buffer, 0); }
            }

            input.Position = Math.Max(0, streamsize - 65536);
            i = 0;
            while (i < 65536 / sizeof(long) && (input.Read(buffer, 0, sizeof(long)) > 0)) {
                i++;
                unchecked { lhash += BitConverter.ToUInt64(buffer, 0); }
            }
            byte[] result = BitConverter.GetBytes(lhash);
            Array.Reverse(result);
            return result;
        }

        private static string ToHexadecimal(byte[] bytes) {
            StringBuilder hexBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++) {
                hexBuilder.Append(bytes[i].ToString("x2"));
            }
            return hexBuilder.ToString();
        }

        #endregion


    }
}
