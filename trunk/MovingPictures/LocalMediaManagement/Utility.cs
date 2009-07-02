using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DirectShowLib;
using DirectShowLib.Dvd;
using Cornerstone.Tools;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {

    public enum VideoDiscFormat
    {
        [Description("")]
        Unknown,
        [Description(@"\video_ts\video_ts.ifo")]
        DVD,
        [Description(@"\bdmv\index.bdmv")]
        Bluray,
        [Description(@"\adv_obj\discid.dat")] // or adv_obj\vplst000.xpl ?
        HDDVD,
        [Description(@"\vcd\entries.vcd")]
        SVCD       
    }
    
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
        /// Get all video files from directory and it's subdirectories.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static List<FileInfo> GetVideoFilesRecursive(DirectoryInfo directory) {
            List<FileInfo> fileList = GetFilesRecursive(directory);
            List<FileInfo> videoFileList = new List<FileInfo>();
            foreach (FileInfo file in fileList) {
                if (Utility.IsVideoFile(file))
                    videoFileList.Add(file);
            }
            return videoFileList;
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
        private const string rxStackPatterns = @"(\W*" + rxStackKeywords + @"\W*([a-c]|\d+|i+))|[\(\[]\d(of|-)\d[\)\]]$";

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

        #region String Modification / Regular Expressions Methods

        // Regular expression pattern that matches a selection of non-word characters
        private const string rxMatchNonWordCharacters = "[\\~`!@#$%^&*\\(\\)_\\+-={}|\\[\\]\\\\:\";'<>?,./]";

        /// <summary>
        /// Filters non descriptive words/characters from a title so that only keywords remain.
        /// </summary>
        /// <param name="title"></param>
        /// <returns>keywords string</returns>
        public static string TitleToKeywords(string title) {

            // Remove articles and non-descriptive words
            string newTitle = Regex.Replace(title, @"\b(" + MovingPicturesCore.Settings.ArticlesForRemoval + @")\b", "", RegexOptions.IgnoreCase);
            newTitle = Regex.Replace(newTitle, @"\b(and|or|of|und|en|et|y)\b", "", RegexOptions.IgnoreCase);

            // Replace non-word characters with spaces
            newTitle = Regex.Replace(newTitle, rxMatchNonWordCharacters, " ");

            // Remove double spaces and trim
            newTitle = TrimSpaces(newTitle);

            // return the keywords
            return newTitle;
        }

        /// <summary>
        /// Converts a movie title to the display name.
        /// </summary>
        /// <example>
        /// Changes "Movie, The" into "The Movie"
        /// </example>
        /// <param name="title"></param>
        /// <returns>display name</returns>
        public static string TitleToDisplayName(string title) {
            Regex expr = new Regex(@"(.+?)(?:, (" + MovingPicturesCore.Settings.ArticlesForRemoval + @"))?\s*$", RegexOptions.IgnoreCase);
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
            Regex expr = new Regex(@"^{" + MovingPicturesCore.Settings.ArticlesForRemoval + @")\s(.+)", RegexOptions.IgnoreCase);
            return expr.Replace(title, "$2, $1").Trim();
        }

        /// <summary>
        /// Converts a title string to a common format to be used in comparison.
        /// </summary>
        /// <param name="title">the original title</param>
        /// <returns>the normalized title</returns>
        public static string NormalizeTitle(string title) {
            if (title == null)
                return "";

            // Convert title to lowercase culture invariant
            string newTitle = title.ToLowerInvariant();

            // Swap article
            newTitle = TitleToDisplayName(newTitle);

            // Replace non-descriptive characters with spaces
            newTitle = Regex.Replace(newTitle, rxMatchNonWordCharacters, " ");

            // Equalize: Convert to base character string
            newTitle = RemoveDiacritics(newTitle);

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
            newTitle = Regex.Replace(newTitle, @"\s(1)$","");

            // Remove double spaces and trim
            newTitle = TrimSpaces(newTitle);

            // return the cleaned title
            return newTitle;
        }

        /// <summary>
        /// Translates characters to their base form.
        /// </summary>
        /// <example>
        /// characters: ë, é, è
        /// result: e
        /// </example>
        /// <remarks>
        /// source: http://blogs.msdn.com/michkap/archive/2007/05/14/2629747.aspx
        /// </remarks>
        public static string RemoveDiacritics(string title) {
            string stFormD = title.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            for (int ich = 0; ich < stFormD.Length; ich++) {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark) {
                    sb.Append(stFormD[ich]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }


        /// <summary>
        /// Removes multiple spaces and replaces them with one space   
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string TrimSpaces(string input) {
            return Regex.Replace(input, @"\s{2,}", " ").Trim();
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
        /// Checks if the file is a valid video file.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static bool IsVideoFile(FileInfo fileInfo) {
            try {
                string fullPath = fileInfo.FullName;

                // Video Disc Standards Are OK
                if (IsVideoDiscPath(fullPath))
                    return true;

                // Check files that pass the MediaPortal Video Extension list
                if (IsMediaPortalVideoFile(fileInfo)) {
                    string ext = fileInfo.Extension.ToLower();
                    string name = fileInfo.Name.ToLower(); ;

                    // DVD: Non-Standalone content is invalid
                    if (ext == ".vob" && Regex.Match(name, @"(video_ts|vts_).+", RegexOptions.IgnoreCase).Success)
                        return false;

                    // DVD: Filter ifo's that are not called video_ts.ifo
                    // but allow them when we don't have a video_ts.ifo in the same folder
                    if (ext == ".ifo" && name != "video_ts.ifo")
                        if (File.Exists(fullPath.ToLower().Replace(name, "video_ts.ifo")))
                            return false;

                    // Bluray: the only valid bluray file would already passed the method, we filter the rest
                    if (ext == ".bdmv")
                        return false;

                    // Bluray: m2ts files sitting in a stream folder are part of a bluray disc
                    if (ext == ".m2ts" && fileInfo.Directory.Name.Equals("stream", StringComparison.OrdinalIgnoreCase))
                        return false;

                    // HD-DVD: evo files sitting in a hvdvd_ts folder are part of a hddvd disc
                    if (ext == ".evo" && fileInfo.Directory.Name.Equals("hvdvd_ts", StringComparison.OrdinalIgnoreCase))
                        return false;

                    // HD-DVD/(S)VCD: .dat files other than discid.dat should be ignored
                    if (ext == ".dat" && name != "discid.dat")
                        return false;

                    // if we made it this far we have a winner
                    return true;
                }
            }
            catch (Exception e) {
                if (e is ThreadAbortException)
                    throw e;

                logger.ErrorException("An error occured while validating '" + fileInfo.ToString() + "' as a video file.", e);
            }

            // we did not pass so return false
            return false;
        }

        /// <summary>
        /// Returns the Video Disc Type
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static VideoDiscFormat GetVideoDiscFormat(string path) {
            foreach (VideoDiscFormat format in Enum.GetValues(typeof(VideoDiscFormat))) {
                if (format != VideoDiscFormat.Unknown) {
                    if (path.EndsWith(GetEnumValueDescription(format),StringComparison.OrdinalIgnoreCase))
                        return format;
                }
            }
            return VideoDiscFormat.Unknown;
        }

        /// <summary>
        /// Check if the path specified is a video disc standard
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsVideoDiscPath(string path) {
            return GetVideoDiscFormat(path) != VideoDiscFormat.Unknown;
        }

        /// <summary>
        /// Returns the full path to the video disc or null if it doesn't find one.
        /// </summary>
        /// <param name="drive"></param>
        /// <returns></returns>
        public static string GetVideoDiscPath(string drive) {
           string path;
           foreach (VideoDiscFormat format in Enum.GetValues(typeof(VideoDiscFormat))) {
               if (format != VideoDiscFormat.Unknown) {
                   path = DeviceManager.GetDriveLetter(drive) + GetEnumValueDescription(format);
                   bool pathExists = File.Exists(path);
                   logger.Debug("Video Disc Check: Format={0}, Path='{1}', Result={2}", format.ToString(), path, pathExists);
                   if (pathExists) {
                       logger.Info("Detected Video Disc: {0}", format.ToString());
                       return path;
                   }
               }
           }
           logger.Info("Detected Video Disc: {0}", VideoDiscFormat.Unknown.ToString());
           return null;
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
        /// Check if the file is classified as sample
        /// </summary>
        /// <param name="file">file to check</param>
        /// <returns>True if file is a sample file</returns>
        public static bool isSampleFile(FileInfo file) {
            try {
                // Set sample max size in bytes
                long sampleMaxSize = MovingPicturesCore.Settings.MaxSampleFilesize * 1024 * 1024;
                // Create the sample filter regular expression
                Regex expr = new Regex(MovingPicturesCore.Settings.SampleRegExFilter, RegexOptions.IgnoreCase);
                // Return result of given conditions         
                return ((file.Length < sampleMaxSize) && expr.Match(file.Name).Success);
            }
            catch (Exception e) {
                if (e is ThreadAbortException)
                    throw e;

                logger.Warn("Sample file check failed: {0}", e.Message);
                return false;
            }
        }

        /// <summary>
        /// Get the number of video files (excluding sample files) that are in a folder
        /// </summary>
        /// <param name="folder">the directory to count video files in</param>
        /// <returns>total number of files found in the folder</returns>
        public static int GetVideoFileCount(DirectoryInfo folder) {
            int rtn = 0;
            FileInfo[] fileList = folder.GetFiles("*");
            foreach (FileInfo currFile in fileList) {
                // count the number of non-sample video files in the folder
                if (IsVideoFile(currFile))
                    if (!isSampleFile(currFile))
                        rtn++;
            }

            // Return count
            return rtn;
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

        #endregion
        
        /// <summary>
        /// Get a WebGrabber instance with the default moving pictures settings
        /// </summary>
        /// <param name="url">url to resource</param>
        /// <returns>webgrabber instance</returns>
        public static WebGrabber GetWebGrabberInstance(string url) {
            WebGrabber grabber = new WebGrabber(url);
            grabber.MaxRetries = MovingPicturesCore.Settings.MaxTimeouts;
            grabber.Timeout = MovingPicturesCore.Settings.TimeoutLength;
            grabber.TimeoutIncrement = MovingPicturesCore.Settings.TimeoutIncrement;
            return grabber;
        }

        #region Video Methods

        /// <summary>
        /// Returns the main feature stream from a video disc (bypassing menu)
        /// </summary>
        /// <param name="entryPath">entry path to the disc</param>
        /// <param name="format">the video disc format</param>
        /// <returns>path to the stream file</returns>
        public static string GetMainFeatureStreamFromVideoDisc(string entryPath, VideoDiscFormat format) {   
            if (entryPath == null)
                return null;

            string dir;
            switch (format) {
                case VideoDiscFormat.Bluray:
                    dir = entryPath.ToLower().Replace("index.bdmv", @"STREAM\");
                    return GetLargestFileInDirectory(new DirectoryInfo(dir), "*.m2ts");
                case VideoDiscFormat.HDDVD:
                    dir = entryPath.ToLower().Replace(@"adv_obj\discid.dat", @"HVDVD_TS\");
                    return GetLargestFileInDirectory(new DirectoryInfo(dir), "*.evo");
                case VideoDiscFormat.DVD:
                    return entryPath.ToLower().Replace(@"\video_ts.ifo", @"\vts_01_0.ifo");
                default:
                    return null;
            }
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
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;

                logger.Error("Error while retrieving disc id for: " + path, e);
            }
            return discID;
        }

        #endregion

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
