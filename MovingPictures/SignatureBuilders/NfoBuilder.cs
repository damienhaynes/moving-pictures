using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.SignatureBuilders {
    public class NfoBuilder : ISignatureBuilder {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public MovieSignature UpdateSignature(MovieSignature signature) {
            bool scanNFO = (bool)MovingPicturesCore.SettingsManager["importer_nfoscan"].Value;

            // Scan for NFO files 
            if (scanNFO && signature.Path != null && signature.LocalMedia != null) {
                DirectoryInfo dir = new DirectoryInfo(signature.Path);
                if (Utility.isFolderDedicated(dir, signature.LocalMedia.Count)) {
                    // Scans base directory for all NFO extensions (Dedicated Folder)
                    signature.ImdbId = fileScanner(dir);                  
                }
                else {
                    // Scans base directory for specific filenames (Shared Folder)
                    string fileName = Utility.RemoveFileStackMarkers(signature.File);
                    signature.ImdbId = fileScanner(dir, fileName);
                }
            }
            
            // If there's no ImdbId in the signature return the signature immediatly
            if (String.IsNullOrEmpty(signature.ImdbId))
                return signature;

            // Fill signature information from IMDB
            string detailsPage = getImdbDetailsPage(signature.ImdbId);
            if (String.IsNullOrEmpty(signature.ImdbId))
                return signature;

            // See if we get a Title and Year from the title node
            Regex expr = new Regex(@"<title>([^\(]+?)\((\d{4})[\/IVX]*\).*?</title>", RegexOptions.IgnoreCase);
            Match details = expr.Match(detailsPage);
            if (details.Success) {
                signature.Title = details.Groups[1].Value;
                signature.Year = int.Parse(details.Groups[2].Value);
                logger.Debug("Lookup Imdbid={0}: Title= '{1}', Year= {2}", signature.ImdbId, details.Groups[1], details.Groups[2]);
            }
            else {
                logger.Debug("Lookup failed for Imdbid={0}", signature.ImdbId);
            }

            return signature;
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
            string nfoExt = MovingPicturesCore.SettingsManager["importer_nfoext"].Value.ToString();
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
                    if (imdbid != string.Empty)
                        return imdbid;
                }

            }

            // we found nothing so return empty
            return string.Empty;
        }

        /// <summary>
        /// Extract an ImdbID out of a textfile
        /// </summary>
        /// <param name="filePath">full path to the file</param>
        /// <returns>ImdbID</returns>
        public static string parseFile(string filePath) {
            logger.Info("Parsing NFO file: {0}", filePath);

            // Read the nfo file content into a string
            string s = File.ReadAllText(filePath);
            // Check for the existance of a imdb id 
            Regex rxIMDB = new Regex(@"tt\d{7}", RegexOptions.IgnoreCase);
            Match match = rxIMDB.Match(s);

            // If success return the id, on failure return empty. 
            if (match.Success) {
                s = match.Value;
                logger.Debug("ImdbID Found: {0}", s);
            }
            else {
                s = string.Empty;
                logger.Debug("No ImdbID Found.");
            }

            // return the string
            return s;
        }

        private static string getImdbDetailsPage(string ImdbId) {
            String sData = string.Empty;
            string url = "http://www.imdb.com/title/" + ImdbId;

            int tryCount = 0;
            int maxRetries = 3;
            int timeout = 5000;
            int timeoutIncrement = 1000;

            while (sData == string.Empty) {
                try {
                    // builds the request and retrieves the respones from the url
                    tryCount++;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Timeout = timeout + (timeoutIncrement * tryCount);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    // converts the resulting stream to a string for easier use
                    Stream resultData = response.GetResponseStream();
                    StreamReader reader = new StreamReader(resultData, Encoding.UTF8, true);
                    sData = reader.ReadToEnd().Replace('\0', ' ');
                    sData = HttpUtility.HtmlDecode(sData);
                    resultData.Close();
                    reader.Close();
                    response.Close();
                }
                catch (WebException e) {
                    if (tryCount == maxRetries) {
                        logger.ErrorException("Error connecting to imdb.com Reached retry limit of " + maxRetries, e);
                        return null;
                    }
                }
            }

            return sData;
        }

    }
}
