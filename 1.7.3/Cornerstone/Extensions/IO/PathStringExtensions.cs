using System.IO;

namespace Cornerstone.Extensions.IO {
    public static class PathStringExtensions {

        /// <summary>
        /// Gets the drive letter for the given path
        /// </summary>
        /// <param name="self"></param>
        /// <returns>a drive letter formatted as 'X:'</returns>
        public static string PathToDriveletter(this string self) {
            // if the path is UNC return null
            if (self.PathIsUnc())
                return null;

            // return the first 2 characters
            if (self.Length > 1)
                return self.Substring(0, 2).ToUpper();
            else // or if only a letter was given add colon
                return self.ToUpper() + ":";
        }

        /// <summary>
        /// Gets a value indicating wether the path is in UNC format.
        /// </summary>
        /// <param name="path">path to check</param>
        /// <returns>True, if it's a UNC path</returns>
        public static bool PathIsUnc(this string self) {
            return self.StartsWith(@"\\");
        }

        /// <summary>
        /// Creates a FileInfo object using the string value as path parameter
        /// </summary>
        /// <param name="self"></param>
        /// <returns>FileInfo object</returns>
        public static FileInfo PathToFileInfo(this string self) {
            return new FileInfo(self);
        }

        /// <summary>
        /// Creates a DirectoryInfo object from a path
        /// </summary>
        /// <param name="self"></param>
        /// <returns>DirectoryInfo object</returns>
        public static DirectoryInfo PathToDirectoryInfo(this string self) {
            return new DirectoryInfo(self);
        }

        /// <summary>
        /// Moves a file to a new location
        /// </summary>
        /// <param name="oldFilePath">Full path of old file</param>
        /// <param name="newFilePath">Full path of where to move new file</param>
        /// <returns>True if move was successful</returns>
        public static bool MoveTo(this string oldFilePath, string newFilePath) {
            try {
                if (File.Exists(newFilePath))
                    return true;

                if (!File.Exists(oldFilePath))
                    return false;

                File.Move(oldFilePath, newFilePath);
            }
            catch {
                return false;
            }
            return true;
        }
    }
}
