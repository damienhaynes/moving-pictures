using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.SignatureBuilders {
    
    /// <summary>
    /// The Local Builder fills the signature properties by using the filesystem names
    /// </summary>
    class LocalBuilder : ISignatureBuilder {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static Dictionary<string, string> replacements;

        static LocalBuilder() {
            // todo: design logic to reload these strings when there's a configurable setting for the user
            replacements = new Dictionary<string, string>();
            replacements.Add(@"(?<!\b\w)\.", " ");  // Replace dots that are not part of acronyms with spaces
            replacements.Add(@"_", " ");  // Replace underscores with spaces
            replacements.Add(@"tt\d{7}", ""); // Removes imdb numbers
        }

        // Filters "noise" from the input string
        private static string removeNoise(string input) {
            Regex expr = new Regex(MovingPicturesCore.Settings.NoiseFilter, RegexOptions.IgnoreCase);
            string denoisedTitle = expr.Replace(input, "");
            denoisedTitle = Utility.TrimSpaces(denoisedTitle);
            return denoisedTitle;
        }

        // Separates the year from the title string (if applicable)
        private static string extractYearFromTitle(string input, out int year) {
            string rtn = input;
            year = 0;

            // if there is a four digit number that looks like a year, parse it out
            Regex expr = new Regex(@"^(.*)[\[\(]?(19\d{2}|20\d{2})[\]\)]?($|.+)");
            Match match = expr.Match(rtn);
            if (match.Success) {
                rtn = match.Groups[1].Value.TrimEnd('(', '['); // leading title string
                year = int.Parse(match.Groups[2].Value);
                if (rtn.Trim() == string.Empty)
                    rtn = match.Groups[3].Value.TrimEnd('(', '['); // trailing title string
            }

            // If the title becomes 0 length, undo this method's processing.
            if (rtn.Trim().Length == 0) { rtn = input; year = 0; return rtn; } else { return rtn.Trim(); }
        }

        #region ISignatureBuilder Members

        public SignatureBuilderResult UpdateSignature(MovieSignature signature) {
            if (signature.LocalMedia == null)
                return SignatureBuilderResult.INCONCLUSIVE;

            DirectoryInfo dir = new DirectoryInfo(signature.Path);
            string fullPath = signature.LocalMedia[0].FullPath; 
            int fileCount = signature.LocalMedia.Count;
            string source = string.Empty;

            if (Utility.isFolderDedicated(dir, fileCount) && (MovingPicturesCore.Settings.PreferFolderName || fileCount > 1) && signature.Folder.Length > 1 || signature.LocalMedia[0].IsVideoDisc) {
                
                // Use foldername
                source = signature.Folder;
                
                // If the foldername is a volume use the media label                
                if (Utility.IsDriveRoot(source))
                    source = signature.LocalMedia[0].MediaLabel;

            } else {
                // Use filename
                if (fileCount > 1)
                    source = Utility.GetFileNameWithoutExtensionAndStackMarkers(signature.File);
                else
                    source = Path.GetFileNameWithoutExtension(signature.File);
            }

            // Detect IMDB ID in the source string, and put it in the signature on success
            Match match = Regex.Match(source, @"tt\d{7}", RegexOptions.IgnoreCase);
            if (match.Success) signature.ImdbId = match.Value;

            // Execute configured string replacements
            foreach (KeyValuePair<string, string> replacement in replacements)
                source = Regex.Replace(source, replacement.Key, replacement.Value);

            // Remove noise characters/words
            // todo: combine this into replacements when there's a configurable setting for the user
            source = removeNoise(source);

            // Detect year in a title string
            int year;
            signature.Title = extractYearFromTitle(source, out year);
            signature.Year = year;      

            return SignatureBuilderResult.INCONCLUSIVE;
        }

        #endregion

    }
}
