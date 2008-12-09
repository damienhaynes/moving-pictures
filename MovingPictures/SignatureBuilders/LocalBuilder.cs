using System;
using System.IO;
using System.Text.RegularExpressions;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.SignatureBuilders {
    class LocalBuilder : ISignatureBuilder {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public MovieSignature UpdateSignature(MovieSignature signature) {
            if (signature.LocalMedia == null)
                return signature;

            DirectoryInfo dir = new DirectoryInfo(signature.Path);
            bool preferFolder = (bool)MovingPicturesCore.SettingsManager["importer_prefer_foldername"].Value;
            int filecount = signature.LocalMedia.Count;

            string source;
            int year;

            if ((Utility.isFolderDedicated(dir, filecount) && (preferFolder || (filecount > 1)) ||
                signature.File.ToLower() == "video_ts.ifo" || // DVD
                signature.File.ToLower() == "index.bdmv" // Blu-Ray
                )) {
                
                // Use foldername
                source = signature.Folder;
      
                // If the foldername is a volume use the media label                
                if (Utility.IsDriveRoot(source))
                    source = signature.LocalMedia[0].MediaLabel;
            }
            else {
                // Use filename
                if (filecount > 1)
                    source = Utility.RemoveFileStackMarkers(signature.File);
                else
                    source = Utility.RemoveFileExtension(signature.File);
            }

            // If there are periods or underscores, 
            // assume the period is replacement for spaces.
            source = Regex.Replace(source, @"[\._]", " ");

            // Phase #2: Cleaning (remove noise)
            source = removeNoise(source);

            // Phase #3: Year detection
            signature.Title = extractYearFromTitle(source, out year);
            signature.Year = year;

            return signature;
        }

        // Filters "noise" from the input string
        private static string removeNoise(string input) {
            Regex expr = new Regex(MovingPicturesCore.SettingsManager["importer_filter"].Value.ToString(), RegexOptions.IgnoreCase);
            string denoisedTitle = expr.Replace(input, "");
            denoisedTitle = Utility.trimSpaces(denoisedTitle);
            return denoisedTitle;
        }

        // Separates the year from the title string (if applicable)
        private static string extractYearFromTitle(string input, out int year) {
            string rtn = input;
            year = 0;

            // if there is a four digit number that looks like a year, parse it out
            Regex expr = new Regex(@"(^.+)[\[\(]?([0-9]{4})[\]\)]?($|.+)");
            Match match = expr.Match(rtn);
            if (match.Success) {
                // set the year
                year = int.Parse(match.Groups[2].Value);
                // check if it's really a year value
                if (year > 1900 && year < DateTime.Now.Year + 2)
                    // clean the possible left overs from the title
                    rtn = match.Groups[1].Value.TrimEnd('(', '[');
                else
                    // year check failed so reset it to 0
                    year = 0;
            }

            // trim and return the title
            return rtn.Trim();
        }



    }
}
