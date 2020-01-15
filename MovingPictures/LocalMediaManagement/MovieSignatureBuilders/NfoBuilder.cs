using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using NLog;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MediaPortal.Plugins.MovingPictures.SignatureBuilders
{

    /// <summary>
    /// The NFO Signature Builder scans for text-based files containing the imdbid format
    /// and updates the signature with the first occurance of an imdbid.
    /// </summary>
    public class NfoBuilder : ISignatureBuilder {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public SignatureBuilderResult UpdateSignature(MovieSignature signature) {
            bool scanNFO = MovingPicturesCore.Settings.NfoScannerEnabled;

            // Scan for NFO files 
            if (scanNFO && signature.Path != null && signature.LocalMedia != null) {
                DirectoryInfo dir = new DirectoryInfo(signature.Path);
                if (Utility.isFolderDedicated(dir, signature.LocalMedia.Count)) {
                    // Scans base directory for all NFO extensions (Dedicated Folder)
                    signature.ImdbId = fileScanner(dir);                  
                }
                else {
                    // Scans base directory for specific filenames (Shared Folder)
                    string fileName = Utility.GetFileNameWithoutExtensionAndStackMarkers(signature.File);
                    signature.ImdbId = fileScanner(dir, fileName);
                }
            }

            return SignatureBuilderResult.INCONCLUSIVE;
       }


        /// <summary>
        /// Scans the directory for NFO files and returns the first found ImdbID
        /// </summary>
        /// <param name="dir">directory</param>
        /// <returns>ImdbID or empty</returns>
        private static string fileScanner(DirectoryInfo dir) {
            return fileScanner(dir, "*");
        }

        /// <summary>
        /// Scans the directory for (a) specific NFO file(s) and returns the first found ImdbID
        /// </summary>
        /// <param name="dir">directory</param>
        /// <param name="filename">* or filename without extension</param>
        /// <returns>ImdbID or empty</returns>
        public static string fileScanner(DirectoryInfo dir, string filename) {
            string nfoExt = MovingPicturesCore.Settings.NfoScannerFileExtensions;
            Char[] splitters = new Char[] { ',', ';' };
            string[] extensions = nfoExt.Split(splitters);
            string[] mask = new string[extensions.Length];

            // combine the filename/mask
            // with the extension list to create
            // a list of files to look for
            for (int i = 0; i < extensions.Length; i++) {
                string ext = extensions[i].Trim();
                if (ext.Length > 1)
                    mask[i] = filename + "." + ext;
            }

            // iterate through each pattern and get the corresponding files
            foreach (string pattern in mask) {
                // if pattern is null or empty continue to next pattern
                if (string.IsNullOrEmpty(pattern))
                    continue;

                // Get all the files specfied by the current pattern from the directory
                FileInfo[] nfoList = dir.GetFiles(pattern.Trim());
                // If none continue to the next pattern
                if (nfoList.Length == 0)
                    continue;

                // iterate through the list of files and scan them
                foreach (FileInfo file in nfoList) {
                    // scan file and retrieve result
                    string imdbid = parseFile(file.FullName);
                    // if a match is found return the imdb id
                    if (imdbid != null)
                        return imdbid;
                }

            }

            // we found nothing so return empty
            return null;
        }

        /// <summary>
        /// Extract an IMDb ID out of a textfile
        /// </summary>
        /// <param name="filePath">full path to the file</param>
        /// <returns>IMDb ID</returns>
        public static string parseFile(string filePath) {
            logger.Info("Parsing NFO file: {0}", filePath);

            // Read the nfo file content into a string
            string s = File.ReadAllText(filePath);
            // Check for the existance of a IMDb ID 
            Match match = Regex.Match(s, @"tt\d+", RegexOptions.IgnoreCase);

            // If success return the ID, on failure return empty. 
            if (match.Success) {
                s = match.Value;
                logger.Debug("IMDb ID Found: {0}", s);
            }
            else {
                s = null;
                logger.Debug("No IMDb ID Found.");
            }

            // return the string
            return s;
        }
    }
}
