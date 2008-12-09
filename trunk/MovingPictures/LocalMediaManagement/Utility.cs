using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DirectShowLib;
using DirectShowLib.Dvd;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {
    class Utility {

        private static Logger logger = LogManager.GetCurrentClassLogger();

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

        /// <summary>
        /// Removes the extension from a filename
        /// </summary>
        /// <param name="file"></param>
        /// <returns>filename without extension</returns>
        public static string RemoveFileExtension(FileInfo file) {
            return Path.GetFileNameWithoutExtension(file.Name);
        }

        public static string RemoveFileExtension(string filename) {
            return RemoveFileExtension(new FileInfo(filename));
        }

        /// <summary>
        /// Remove extension and stackmarkers from a filename
        /// </summary>
        /// <param name="file">target file</param>
        /// <returns>the filename without stackmarkers and extension</returns>
        public static string RemoveFileStackMarkers(FileInfo file) {
            // Remove the file extension from the filename
            string fileName = RemoveFileExtension(file);

            // If file is classified as multipart clean the stack markers.
            if (isFileMultiPart(fileName)) {
                Regex expr = new Regex(rxFileStackMarkers, RegexOptions.IgnoreCase);
                Match match = expr.Match(fileName);
                // if we have a match on this expression we will remove the complete match.
                if (match.Success)
                    fileName = expr.Replace(fileName, "");
                // no match means we just remove one character
                else
                    fileName = fileName.Substring(0, (fileName.Length - 1));
            }

            // Return the cleaned filename
            return fileName;
        }

        public static string RemoveFileStackMarkers(string filename) {
            return RemoveFileStackMarkers(new FileInfo(filename));
        }

        // Regular expression patterns used by the multipart detection and cleaning methods
        private const string rxFileStackMarkers = @"([\s\-]*(cd|disk|disc|part)[\s\-]*([a-c]|\d+|i+))|[\(\[]\d(of|-)\d[\)\]]$";

        /// <summary>
        /// Checks if a file has stack markers (and is multi-part)
        /// </summary>
        /// <param name="file"></param>
        /// <returns>true if multipart, false if not</returns>
        public static bool isFileMultiPart(FileInfo file) {
            return isFileMultiPart(RemoveFileExtension(file));
        }

        /// <summary>
        /// Checks if a filename has stack markers (and is multi-part)
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>true if multipart, false if not</returns>
        public static bool isFileMultiPart(string filename) {
            Regex expr = new Regex(rxFileStackMarkers + @"|[^\s\d](\d+)$|([a-c])$", RegexOptions.IgnoreCase);
            return expr.Match(filename).Success;
        }

        // Regular expression pattern that matches an "article" that need to be moved for title conversions
        // todo: the articles should really be a user definable setting in the future
        private const string rxTitleSortPrefix = "(the|a|an|ein|das|die|der|les|la|le|el|une|de|het)";

        /// <summary>
        /// Converts a movie title to the display name.
        /// </summary>
        /// <example>
        /// Changes "Movie, The" into "The Movie"
        /// </example>
        /// <param name="title"></param>
        /// <returns>display name</returns>
        public static string TitleToDisplayName(string title) {
            Regex expr = new Regex(@"(.+?)(?:, " + rxTitleSortPrefix + @")?\s*$", RegexOptions.IgnoreCase);
            return expr.Replace(title, "$2 $1").Trim();
        }

        /// <summary>
        /// Converts a title to the archive name (sortable title)
        /// </summary>
        /// <example>
        /// Changes "The Movie" into "Movie, The"
        /// </example>
        /// <param name="title"></param>
        /// <returns>archive name</returns>
        public static string TitleToArchiveName(string title) {
            Regex expr = new Regex(@"^" + rxTitleSortPrefix + @"\s(.+)", RegexOptions.IgnoreCase);
            return expr.Replace(title, "$2, $1").Trim();
        }

        /// <summary>
        /// Converts a title string to a common format to be used in comparison.
        /// </summary>
        /// <param name="title">the original title</param>
        /// <returns>the normalized title</returns>
        public static string normalizeTitle(string title) {
            // todo: optimize, maybe also include htmldecode?

            // Convert title to lowercase
            string newTitle = title.ToLower();

            // Swap article
            newTitle = TitleToDisplayName(newTitle);

            // Replace non-descriptive characters with spaces
            newTitle = Regex.Replace(newTitle, @"[\.:;\+\-\*]", " ");

            // Remove other non-descriptive characters completely
            newTitle = Regex.Replace(newTitle, @"[\(\)\[\]'`,""\#\$\?]", "");

            // Equalize: Common characters with words of the same meaning
            newTitle = Regex.Replace(newTitle, @"\s(and|und|en|et|y)\s", " & ");

            // Equalize: Roman Numbers To Numeric
            newTitle = Regex.Replace(newTitle, @"\sII($|\s)", @" 2$1", RegexOptions.IgnoreCase);
            newTitle = Regex.Replace(newTitle, @"\sIII($|\s)", @" 3$1", RegexOptions.IgnoreCase);
            newTitle = Regex.Replace(newTitle, @"\sIV($|\s)", @" 4$1", RegexOptions.IgnoreCase);
            newTitle = Regex.Replace(newTitle, @"\sV($|\s)", @" 5$1", RegexOptions.IgnoreCase);
            newTitle = Regex.Replace(newTitle, @"\sVI($|\s)", @" 6$1", RegexOptions.IgnoreCase);
            newTitle = Regex.Replace(newTitle, @"\sVI($|\s)", @" 6$1", RegexOptions.IgnoreCase);
            newTitle = Regex.Replace(newTitle, @"\sVII($|\s)", @" 7$1", RegexOptions.IgnoreCase);
            newTitle = Regex.Replace(newTitle, @"\sVIII($|\s)", @" 8$1", RegexOptions.IgnoreCase);
            newTitle = Regex.Replace(newTitle, @"\sIX($|\s)", @" 9$1", RegexOptions.IgnoreCase);

            // Remove double spaces and trim
            newTitle = trimSpaces(newTitle);
            // return the cleaned title
            return newTitle;
        }

        /// <summary>
        /// Removes multiple spaces and replaces them with one space   
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string trimSpaces(string input) {
            return Regex.Replace(input, @"\s{2,}", " ").Trim();
        }

        /// <summary>
        /// Checks if the foldername is a multipart marker.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool isFolderMultipart(string name) {
            Regex expr = new Regex(@"^(cd|dvd)\s*\d+$", RegexOptions.IgnoreCase);
            return expr.Match(name).Success;
        }

        /// <summary>
        /// Checks if the foldername is ambigious (non descriptive)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool isFolderAmbiguous(string name) {
            // Conditions:
            // a) Name is too short
            // b) video_ts folder (dvd subfolder)
            // c) multipart folder (subfolder)
            return (name.Length == 1 || name.ToLower() == "bdmv" || name.ToLower() == "stream" || name.ToLower() == "playlist"
                || name.ToLower() == "clipinf" || name.ToLower() == "backup" || name.ToLower() == "video_ts" || isFolderMultipart(name));
        }

        /// <summary>
        /// Checks if a given directory is a DVD container.
        /// </summary>
        /// <param name="directory">the directory to check</param>
        /// <returns>True if the directory contains a DVD</returns>
        public static bool isDvdContainer(DirectoryInfo directory) {
            FileInfo[] files = directory.GetFiles("video_ts.ifo");
            return (files.Length == 1);
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
            if (path.Length < 4)
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
            while (isFolderAmbiguous(dirLevel.Name) && dirLevel.Root != dirLevel)
                dirLevel = dirLevel.Parent;

            return dirLevel;
        }

        /// <summary>
        /// Check if the file is classified as sample
        /// </summary>
        /// <param name="file">file to check</param>
        /// <returns>True if file is a sample file</returns>
        public static bool isSampleFile(FileInfo file) {
            // Set sample max size in bytes
            long sampleMaxSize = long.Parse(MovingPicturesCore.SettingsManager["importer_sample_maxsize"].Value.ToString()) * 1024 * 1024;
            // Create the sample filter regular expression
            Regex expr = new Regex(MovingPicturesCore.SettingsManager["importer_sample_keyword"].Value.ToString(), RegexOptions.IgnoreCase);
            // Return result of given conditions         
            return ((file.Length < sampleMaxSize) && expr.Match(file.Name).Success);
        }

        // Cached Dictionary for GetVideoFileCount()
        private static Dictionary<string, int> folderCountCache;

        /// <summary>
        /// Get the number of video files (excluding sample files) that are in a folder
        /// </summary>
        /// <param name="folder">the directory to count video files in</param>
        /// <returns>total number of files found in the folder</returns>
        public static int GetVideoFileCount(DirectoryInfo folder) {
            return GetVideoFileCount(folder, true);
        }

        /// <summary>
        /// Get the number of video files (excluding sample files) that are in a folder
        /// </summary>
        /// <param name="folder">the directory to count video files in</param>
        /// <param name="cached">If the count for this folder was already calculated this session return the cache value, set to false to disable cache</param>
        /// <returns>total number of files found in the folder</returns>
        public static int GetVideoFileCount(DirectoryInfo folder, bool cached) {
            // If there's no cache object, create it
            if (folderCountCache == null)
                folderCountCache = new Dictionary<string, int>();

            // if we have already scanned this folder move on
            // todo: remove the cache or let it expire when the directory is changed
            if (folderCountCache.ContainsKey(folder.FullName) && cached)
                return folderCountCache[folder.FullName];

            // count the number of non-sample video files in the folder
            int rtn = 0;
            FileInfo[] fileList = folder.GetFiles("*");
            foreach (FileInfo currFile in fileList) {
                // Loop through files having valid video extensions
                // NOTE: MediaPortal Dependency!
                if (IsMediaPortalVideoFile(currFile))
                    if (!isSampleFile(currFile))
                        rtn++;
            }

            // Save to cache and return count
            folderCountCache[folder.FullName] = rtn;
            return rtn;
        }

        // NOTE: MediaPortal Dependency!
        // Checks if file has valid video extensions (as specified by media portal
        public static bool IsMediaPortalVideoFile(FileInfo file) {
            foreach (string currExt in MediaPortal.Util.Utils.VideoExtensions) {
                if (file.Extension.ToLower() == currExt)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a folder contains a maximum amount of video files
        /// This is used to determine if a folder is dedicated to one movie
        /// </summary>
        /// <param name="folder">the directory to check</param>
        /// <param name="expectedCount">maximum count</param>
        /// <returns>True if folder is dedicated</returns>
        public static bool isFolderDedicated(DirectoryInfo folder, int expectedCount) {
            return (GetVideoFileCount(folder) <= expectedCount);
        }

        #region DirectShowLib

        /// <summary>
        /// Get Disc ID as a string
        /// </summary>
        /// <param name="path">CD/DVD path</param>
        /// <returns>Disc ID</returns>
        public static string GetDiscIdString(string path) {
            long id = GetDiscId(path);
            if (id != 0)
                return Convert.ToString(id, 16); // HEX

            return null;
        }

        /// <summary>
        /// Get Disc ID as 64-bit signed integer
        /// </summary>
        /// <param name="path">CD/DVD path</param>
        /// <returns>Disc ID</returns>
        public static long GetDiscId(string path) {
            long discID = 0;
            logger.Debug("Generating DiscId for: " + path);
            try {
                IDvdInfo2 dvdInfo = (IDvdInfo2)new DVDNavigator();
                dvdInfo.GetDiscID(path, out discID);
            }
            catch (Exception e) {
                logger.Error("Error while retrieving disc id for: " + path, e);
            }
            return discID;
        }

        #endregion

    }
}
