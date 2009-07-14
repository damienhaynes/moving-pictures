using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {

    /// <summary>
    /// Video Formats
    /// </summary>
    public enum VideoFormat {
        NotSupported, // used for file types that are not supported
        Unknown, // used for images
        DVD,
        Bluray,
        HDDVD,
        SVCD,
        File, // used for all valid 'standalone' video files that do not have a specific video format
    }

    /// <summary>
    /// Extension methods for the VideoFormat ENUM
    /// </summary>
    public static class VideoFormatExtensions {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets the path 'signature' of this video format.
        /// </summary>
        public static string PathEndsWith(this VideoFormat self) {
            switch (self) {
                case VideoFormat.DVD:
                    return @"\video_ts\video_ts.ifo";
                case VideoFormat.Bluray:
                    return @"\bdmv\index.bdmv";
                case VideoFormat.HDDVD:
                    return @"\adv_obj\discid.dat"; // or adv_obj\vplst000.xpl ?
                case VideoFormat.SVCD:
                    return @"\vcd\entries.vcd";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Gets a value indicating wether the path validates for the current video format
        /// </summary>
        /// <param name="path">path to validate</param>
        /// <returns>True if the given validates for this video format</returns>
        public static bool Validate(this VideoFormat self, string path) {
            switch (self) {
                case VideoFormat.Bluray:
                case VideoFormat.DVD:
                case VideoFormat.HDDVD:
                case VideoFormat.SVCD:
                    return (path.EndsWith(self.PathEndsWith(), StringComparison.OrdinalIgnoreCase));
                case VideoFormat.Unknown:
                    return Utility.IsImageFile(path);
                case VideoFormat.File:
                    try {
                        if (Utility.IsMediaPortalVideoFile(path)) {
                            FileInfo fileInfo = new FileInfo(path);

                            string ext = fileInfo.Extension.ToLower();
                            string name = fileInfo.Name.ToLower(); ;

                            // DVD: Non-Standalone content is invalid
                            if (ext == ".vob" && Regex.Match(name, @"(video_ts|vts_).+", RegexOptions.IgnoreCase).Success)
                                return false;

                            // DVD: Filter ifo's that are not called video_ts.ifo
                            // but allow them when we don't have a video_ts.ifo in the same folder
                            if (ext == ".ifo" && name != "video_ts.ifo")
                                if (System.IO.File.Exists(path.ToLower().Replace(name, "video_ts.ifo")))
                                    return false;

                            // Bluray: the only valid bluray file would already passed the method, we filter the rest
                            if (ext == ".bdmv")
                                return false;

                            // HD-DVD/(S)VCD: .dat files other than discid.dat should be ignored
                            if (ext == ".dat" && name != "discid.dat")
                                return false;

                            string dirName = fileInfo.Directory.Name;

                            // Bluray: m2ts files sitting in a stream folder are part of a bluray disc
                            if (ext == ".m2ts" && dirName.Equals("stream", StringComparison.OrdinalIgnoreCase))
                                return false;

                            // HD-DVD: evo files sitting in a hvdvd_ts folder are part of a hddvd disc
                            if (ext == ".evo" && dirName.Equals("hvdvd_ts", StringComparison.OrdinalIgnoreCase))
                                return false;

                            // if we made it this far we have a winner
                            return true;
                        }
                    }
                    catch (Exception e) {
                        if (e is ThreadAbortException)
                            throw e;

                        logger.ErrorException("An error occured while validating '" + path + "' as a video file.", e);
                    }
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets a value indicating wether this video format exists on the specified drive
        /// </summary>
        /// <param name="driveletter">the drive to check</param>
        /// <returns>True if the format exists on the drive</returns>
        public static bool PathExistsOnDrive(this VideoFormat self, string driveletter) {
            if (self == VideoFormat.Unknown || self == VideoFormat.File || self == VideoFormat.NotSupported)
                return false;
            else
                return System.IO.File.Exists(self.GenerateVideoPathOnDrive(driveletter));
        }

        /// <summary>
        /// Generates the full path to the video file on the specified drive.
        /// </summary>
        /// <param name="driveletter">the drive to use in the generated path</param>
        /// <returns>Full path to the entry file</returns>
        public static string GenerateVideoPathOnDrive(this VideoFormat self, string driveletter) {
            return DeviceManager.GetDriveLetter(driveletter) + self.PathEndsWith();       
        }

        /// <summary>
       /// Gets the path to the main feature file for this video format
       /// </summary>
       /// <param name="self"></param>
       /// <param name="path">actual entry path for this video format</param>
       /// <returns>full path to to main feature file</returns>
        public static string GetMainFeatureFilePath(this VideoFormat self, string path) {
            string dir;
            switch (self) {
                case VideoFormat.Bluray:
                    dir = path.ToLower().Replace("index.bdmv", @"STREAM\");
                    return Utility.GetLargestFileInDirectory(new DirectoryInfo(dir), "*.m2ts");
                case VideoFormat.HDDVD:
                    dir = path.ToLower().Replace(@"adv_obj\discid.dat", @"HVDVD_TS\");
                    return Utility.GetLargestFileInDirectory(new DirectoryInfo(dir), "*.evo");
                case VideoFormat.DVD:
                    return path.ToLower().Replace(@"\video_ts.ifo", @"\vts_01_0.ifo");
                default:
                    return path;
            }
        }

        /// <summary>
        /// Gets the MediaInfo for the given videopath
        /// </summary>
        /// <param name="self"></param>
        /// <param name="videoPath">path to the video</param>
        /// <returns>MediaInfoWrapper object for this video</returns>
        public static MediaInfoWrapper GetMediaInfo(this VideoFormat self, string videoPath) {
            string mainFeatureFile;
            if (self == VideoFormat.DVD) {
                // because dvds have multiple files
                // we must build a list of all IFO files, and loop through them.
                // the best file wins
                List<string> files = new List<string>();
                files.AddRange(Directory.GetFiles(Path.GetDirectoryName(videoPath), "*.ifo"));
                mainFeatureFile = VideoUtility.FindFeatureFilm(files);
            }
            else {
                mainFeatureFile = self.GetMainFeatureFilePath(videoPath);
            }

            if (videoPath != mainFeatureFile)
                logger.Debug("Format={0}, FeatureFilmFile='{1}'", self, mainFeatureFile);

            return new MediaInfoWrapper(mainFeatureFile);
        }
    }

    /// <summary>
    /// Utility class for video related methods
    /// </summary>
    public static class VideoUtility {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Returns the videoformat that matches the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>NotSupported if the videoformat is not recognized</returns>
        public static VideoFormat GetVideoFormat(string path) {
            foreach (VideoFormat format in Enum.GetValues(typeof(VideoFormat))) {
                if (format.Validate(path))
                    return format;
            }
            return VideoFormat.NotSupported;
        }

        /// <summary>
        /// Returns the video path (in case of a known video disc format) or just the driveletter
        /// </summary>
        /// <param name="driveletter">drive that will be checked</param>
        /// <returns>path or driveletter</returns>
        public static string GetVideoPath(string driveletter) {
            foreach (VideoFormat format in Enum.GetValues(typeof(VideoFormat))) {
                if (format.PathExistsOnDrive(driveletter))
                    return format.GenerateVideoPathOnDrive(driveletter);
            }

            return DeviceManager.GetDriveLetter(driveletter);
        }

        /// <summary>
        /// Gets a value indicating wether the supplied path validates as a video file
        /// </summary>
        /// <param name="path">filepath to validate</param>
        /// <returns>True if the file validates as a video file</returns>
        public static bool IsVideoFile(string path) {
            VideoFormat format = GetVideoFormat(path);
            return (GetVideoFormat(path) != VideoFormat.NotSupported);
        }

        public static bool IsVideoFile(FileInfo fileInfo) {
            return IsVideoFile(fileInfo.FullName);
        }

        /// <summary>
        /// Gets a value indicating wether the supplied path validates as a video DISC format (DVD, Bluray, HDDVD or SVCD)
        /// </summary>
        /// <param name="path">filepath to validate</param>
        /// <returns>True if the path validates as a video disc</returns>
        public static bool IsVideoDisc(string path) {
            VideoFormat format = GetVideoFormat(path);
            return (format != VideoFormat.NotSupported && format != VideoFormat.Unknown && format != VideoFormat.File);
        }


        /// <summary>
        /// Check if the file is classified as a video sample file
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
        /// Get all video files from directory and it's subdirectories.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static List<FileInfo> GetVideoFilesRecursive(DirectoryInfo directory) {
            List<FileInfo> fileList = Utility.GetFilesRecursive(directory);
            List<FileInfo> videoFileList = new List<FileInfo>();
            foreach (FileInfo file in fileList) {
                if (IsVideoFile(file))
                    videoFileList.Add(file);
            }
            return videoFileList;
        }

        /// <summary>
        /// Returns a MediaInfoWrapper for the given video path
        /// </summary>
        /// <param name="videoPath">path to the video</param>
        /// <returns>MediaInfoWrapper object</returns>
        public static MediaInfoWrapper GetMediaInfo(string videoPath) {
            VideoFormat format = GetVideoFormat(videoPath);
            return format.GetMediaInfo(videoPath);
        }

        /// <summary>
        /// Finds the most optimal file in a collection of files.
        /// Uses the aspect ratio, resolution, audio channel count, and duration to find the file
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public static string FindFeatureFilm(List<string> files) {
            if (files.Count == 1) return files[0];

            Dictionary<string, MediaInfoWrapper> mediaInfos = new Dictionary<string, MediaInfoWrapper>();
            foreach (string file in files) {
                mediaInfos.Add(file, new Database.MediaInfoWrapper(file));
            }

            // first filter out the fullscreen files if there are widescreen files present
            List<string> potentialFiles = new List<string>();
            foreach (var mediaInfo in mediaInfos) {
                if (mediaInfo.Value.AspectRatio == "widescreen")
                    potentialFiles.Add(mediaInfo.Key);
            }
            if (potentialFiles.Count == 0) potentialFiles.AddRange(files);
            if (potentialFiles.Count == 1) return potentialFiles[0];

            // next filter out by the highest resolution

            // find max height
            int maxHeight = 0;
            foreach (string file in potentialFiles) {
                if (mediaInfos[file].Height > maxHeight)
                    maxHeight = mediaInfos[file].Height;
            }

            // remove everything that is not max height
            for (int i = potentialFiles.Count - 1; i >= 0; i--) {
                if (mediaInfos[potentialFiles[i]].Height != maxHeight)
                    potentialFiles.RemoveAt(i);
            }
            if (potentialFiles.Count == 1) return potentialFiles[0];

            // next filter by audio channel count
            // find max audio channel count
            int maxChannelCount = 0;
            foreach (string file in potentialFiles) {
                if (mediaInfos[file].AudioChannels > maxChannelCount)
                    maxChannelCount = mediaInfos[file].AudioChannels;
            }

            // remove everything that is not max channel count
            for (int i = potentialFiles.Count - 1; i >= 0; i--) {
                if (mediaInfos[potentialFiles[i]].AudioChannels != maxChannelCount)
                    potentialFiles.RemoveAt(i);
            }

            // find max duration
            int maxDuration = 0;
            foreach (string file in potentialFiles) {
                if (mediaInfos[file].Duration > maxDuration)
                    maxDuration = mediaInfos[file].Duration;
            }
            // remove everything that is not max duration
            for (int i = potentialFiles.Count - 1; i >= 0; i--) {
                if (mediaInfos[potentialFiles[i]].Duration != maxDuration)
                    potentialFiles.RemoveAt(i);
            }

            return potentialFiles[0];
        }

    }

}
