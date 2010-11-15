using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NLog;

namespace Cornerstone.Extensions.IO {
    public static class DirectoryInfoExtensions {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets a value indicating wether this directory is available
        /// </summary>
        /// <param name="self"></param>
        /// <returns>True if available</returns>
        public static bool IsAccessible(this DirectoryInfo self) {
            if (self.Exists) {

                if (!self.IsReparsePoint())
                    return true;

                try {
                    self.GetDirectories();
                    // directory access successful, directory is available
                    return true;
                }
                // ignore the exception, failure means it is not available 
                catch (DirectoryNotFoundException) { }
            }
            return false;
        }

        /// <summary>
        /// Get all files from directory and it's subdirectories.
        /// </summary>
        /// <param name="inputDir"></param>
        /// <returns></returns>
        public static List<FileInfo> GetFilesRecursive(this DirectoryInfo self) {
            List<FileInfo> fileList = new List<FileInfo>();
            DirectoryInfo[] subdirectories = new DirectoryInfo[] { };

            try {
                fileList.AddRange(self.GetFiles("*"));
                subdirectories = self.GetDirectories();
            }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;

                logger.Debug("Error while retrieving files/directories for: {0} {1}", self.FullName, e);
            }

            foreach (DirectoryInfo subdirectory in subdirectories) {
                try {
                    if ((subdirectory.Attributes & FileAttributes.System) == 0)
                        fileList.AddRange(GetFilesRecursive(subdirectory));
                    else
                        logger.Debug("Skipping directory {0} because it is flagged as a System folder.", subdirectory.FullName);
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
        /// Get the largest file from a directory matching the specified file mask
        /// </summary>
        /// <param name="fileMask">the filemask to match</param>
        /// <returns>path to the largest file or null if no file was found or an error occured</returns>
        public static string GetLargestFile(this DirectoryInfo self, string fileMask) {
            string largestFile = null;
            long largestSize = 0;
            try {
                FileInfo[] files = self.GetFiles(fileMask);
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

                logger.ErrorException("Error while retrieving files for: " + self.FullName, e);
            }
            return largestFile;
        }

    }
}
