using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;
using System.Text.RegularExpressions;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    public class LocalProvider : IBackdropProvider {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private DBMovieInfo movie;

        public bool GetBackdrop(DBMovieInfo movie) {
            this.movie = movie;

            // if we already have a backdrop move on for now
            if (movie.BackdropFullPath.Trim().Length > 0)
                return false;

            // grab our settings
            string pattern = MovingPicturesCore.SettingsManager["local_backdrop_pattern"].StringValue;
            string backdropFolderPath = MovingPicturesCore.SettingsManager["backdrop_folder"].StringValue;
            
            // try to create our filename
            Regex parser = new Regex("%(.*?)%", RegexOptions.IgnoreCase);
            string filename = parser.Replace(pattern, new MatchEvaluator(dbNameParser)).Trim().ToLower();

            // grab the list of files in the backdrop folder and see if any match
            DirectoryInfo backdropFolder = null;
            try {
                backdropFolder = new DirectoryInfo(backdropFolderPath);
                FileInfo[] fileList = backdropFolder.GetFiles("*", SearchOption.TopDirectoryOnly);
                foreach (FileInfo currFile in fileList) {
                    if (currFile.Name.ToLower().Equals(filename)) {
                        movie.BackdropFullPath = currFile.FullName;
                        return true;
                    }
                }
            }
            catch (Exception e) {
                logger.Error("Error scanning " + (backdropFolder == null ? "folder" : backdropFolder.FullName) + " for Backdrops.", e);
            }

            return false;
        }

        private string dbNameParser(Match match) {
            // try to grab the field object
            string fieldName = match.Value.Substring(1, match.Length - 2);
            DBField field = DBField.GetFieldByDBName(typeof(DBMovieInfo), fieldName);
            
            // if no dice, the user probably entered an invalid string.
            if (field == null) {
                logger.Error("Error parsing \"" + match.Value + "\" from local_backdrop_pattern advanced setting. Not a database field name.");
                return match.Value;
            }

            return field.GetValue(movie).ToString();
        }

    }
}
