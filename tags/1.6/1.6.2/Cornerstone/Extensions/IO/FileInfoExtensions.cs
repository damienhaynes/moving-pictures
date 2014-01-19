using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using NLog;

namespace Cornerstone.Extensions.IO {

    public static class FileInfoExtensions {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets a value indicating whether this file is accessible (for reading)
        /// </summary>
        /// <param name="self"></param>
        /// <returns>True if accessible</returns>
        public static bool IsLocked(this FileInfo self) {
            FileStream stream = null;
            try {
                stream = self.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException) {
                return true;
            }
            finally {
                if (stream != null) stream.Close();
            }

            return false;
        }

        /// <summary>
        /// Calculates a unique hash for the contents of the file.
        /// Use this method to compute hashes of large files.
        /// </summary>
        /// <param name="self"></param>
        /// <returns>a unique hash or null when error</returns>
        public static string ComputeSmartHash(this FileInfo self) {
            string hexHash = null;
            byte[] bytes = null;
            try {
                using (Stream input = self.OpenRead()) {
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
                    bytes = BitConverter.GetBytes(lhash);
                    Array.Reverse(bytes);

                    // convert to hexadecimal string
                    hexHash = bytes.ToHexString();
                }
            }
            catch (Exception e) {
                logger.DebugException("Error computing smart hash: ", e);
            }
            return hexHash;
        }

        /// <summary>
        /// Generates a SHA1-Hash from a given filepath
        /// </summary>
        /// <param name="filePath">path to the file</param>
        /// <returns>hash as an hexadecimal string </returns>
        public static string ComputeSHA1Hash(this FileInfo self) {
            string hashHex = null;
            if (self.Exists) {
                Stream file = null;
                try {
                    file = self.OpenRead();
                    HashAlgorithm hashObj = new SHA1Managed();
                    byte[] hash = hashObj.ComputeHash(file);
                    hashHex = hash.ToHexString();
                    logger.Debug("SHA1: Success, File='{0}', Hash='{1}'", self.FullName, hashHex);
                }
                catch (Exception e) {
                    if (e.GetType() == typeof(ThreadAbortException))
                        throw e;

                    logger.DebugException("SHA1: Failed, File='" + self.FullName + "' ", e);
                }
                finally {
                    if (file != null)
                        file.Close();
                }
            }
            else {
                // File does not exist
                logger.Debug("SHA1: Failed, File='{0}', Reason='File is not available'", self.FullName);
            }

            // Return
            return hashHex;
        }
    
    }
}
