using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Database;
using MediaPortal.Plugins.MovingPictures.Database;
using System.Text.RegularExpressions;
using System.IO;
using NLog;
using System.Threading;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieRenamer {
    public class MovieRenamer {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Dictionary<DBMovieInfo, string> baseFileNameLookup = new Dictionary<DBMovieInfo, string>();
        private Dictionary<DBMovieInfo, string> newDirectoryNameLookup = new Dictionary<DBMovieInfo, string>();

        public MovieRenamer() {
            // if matching patterns are invalid fix / reinitialize them
            if (!MovingPicturesCore.Settings.FileMultipartString.Contains('#')) { MovingPicturesCore.Settings.FileMultipartString = MovingPicturesCore.Settings.FileMultipartString + "#"; }
            if (!MovingPicturesCore.Settings.FileRenameString.Contains(@"${moviepart}")) { MovingPicturesCore.Settings.FileRenameString = MovingPicturesCore.Settings.FileRenameString + @"${moviepart}"; }
        }

        
        public bool Rename(DBMovieInfo movie) {
            return GetRenameActionList(movie).RenameApprovedItems();
        }

        public bool Revert(DBMovieInfo movie) {
            // attempt to rename the folder if needed
            try {
                if (movie.OriginalDirectoryName != String.Empty) {
                    DirectoryInfo originalDir = Utility.GetMovieBaseDirectory(movie.LocalMedia[0].File.Directory);
                    int lastDirSeperator = originalDir.FullName.LastIndexOf(Path.DirectorySeparatorChar);
                    string newDirName = originalDir.FullName.Remove(lastDirSeperator + 1) + movie.OriginalDirectoryName;

                    if (originalDir.FullName != newDirName) {
                        originalDir.MoveTo(newDirName);
                        movie.OriginalDirectoryName = String.Empty;
                        movie.Commit();
                    }
                }
            }
            catch (Exception e) {
                logger.ErrorException("Failed to revert renaming of directory: " + movie.LocalMedia[0].File.DirectoryName, e);
                return false;
            }

            // wait a sec for directory changes to propagate.
            Thread.Sleep(100);

            
            foreach (DBLocalMedia currMedia in movie.LocalMedia) {
                if (currMedia.OriginalFileName == String.Empty)
                    continue;

                string fileName = Path.GetFileNameWithoutExtension(currMedia.FullPath);
                string fileExt = Path.GetExtension(currMedia.FullPath);
                string filePath = currMedia.File.DirectoryName;

                // attempt to rename the file based on localMediaItem.OriginalFileName.
                try {
                    File.Move(currMedia.FullPath, filePath + "\\" + currMedia.OriginalFileName + fileExt);

                    // look for additional files with the same name.
                    foreach (FileInfo currSubFile in currMedia.File.Directory.GetFiles(fileName + ".*")) {
                        File.Move(currSubFile.FullName, filePath + "\\" + currMedia.OriginalFileName + currSubFile.Extension);
                        Thread.Sleep(100);
                    }

                    // remove original file name from the DB.
                    currMedia.OriginalFileName = String.Empty;
                }
                catch (Exception e) {
                    logger.ErrorException("Failed to revert renaming of file: " + fileName, e);
                    return false;
                }

            }
            return true;
        }

        // Returns a list of files and directories ready to be renamed, based on this movie.
        // To perform a rename on this list call list.RenameApprovedItems();
        public List<Renamable> GetRenameActionList(DBMovieInfo movie) {
            List<Renamable> renamableList = new List<Renamable>();

            // if this is a disk or a ripped disk, we dont want to rename any files
            if (movie.LocalMedia[0].ImportPath.IsOpticalDrive ||
                movie.LocalMedia[0].IsBluray ||
                movie.LocalMedia[0].IsDVD ||
                movie.LocalMedia[0].IsHDDVD) {

                return renamableList;
            }

            foreach (DBLocalMedia currFile in movie.LocalMedia) {
                string primaryExtension = Path.GetExtension(currFile.FullPath);
                string path = Path.GetDirectoryName(currFile.FullPath);
                string newFileName = GetNewFileName(currFile);

                // generate primary renamable file for the current video file
                if (MovingPicturesCore.Settings.RenameFiles) {
                    RenamableFile primaryRenamable = new RenamableFile(movie, currFile);
                    primaryRenamable.NewName = path + @"\" + newFileName + primaryExtension;
                    renamableList.Add(primaryRenamable);
                }

                // generate secondary renamable files for subtitles, etc
                if (MovingPicturesCore.Settings.RenameSecondaryFiles) {
                    renamableList.AddRange(GetSecondaryFiles(currFile));
                }
            }

            // generate a renamable object for the directory as needed
            if (MovingPicturesCore.Settings.RenameFolders) {
                RenamableDirectory renamableDir = GetDirectoryRenamable(movie);
                if (renamableDir != null) renamableList.Add(renamableDir);
            }

            renamableList.UpdateFinalFilenames();
            return renamableList;
        }

        private List<Renamable> GetSecondaryFiles(DBLocalMedia mainFile) {
            List<Renamable> secondaryFiles = new List<Renamable>();

            // grab information about files to be renamed
            DirectoryInfo targetDir = new DirectoryInfo(mainFile.File.DirectoryName);
            string[] fileExtensions = MovingPicturesCore.Settings.Rename_SecondaryFileTypes.Split('|');
            
            // loop through our possible files and flag any exist for renaming
            foreach(string currFileExt in fileExtensions) {
                string originalNameBase = targetDir + @"\" + Path.GetFileNameWithoutExtension(mainFile.FullPath);
                string newNameBase = targetDir + @"\" + GetNewFileName(mainFile);
                FileInfo newFile = new FileInfo(originalNameBase + currFileExt);
                
                if (newFile.Exists) {
                    RenamableFile newRenamable = new RenamableFile(mainFile.AttachedMovies[0], newFile);                   
                    newRenamable.NewName = newNameBase + currFileExt;
                    secondaryFiles.Add(newRenamable);
                }
            }

            return secondaryFiles;
        }

        private RenamableDirectory GetDirectoryRenamable(DBMovieInfo movie) {
            if (!movie.LocalMedia.IsAvailable())
                return null;

            DirectoryInfo movieDir = movie.LocalMedia[0].File.Directory;
            bool dedicatedFolder = Utility.isFolderDedicated(movieDir, movie.LocalMedia.Count);
            
            if (!dedicatedFolder) 
                return null;

            // make sure we are not in a video_ts, hddvd_ts, etc folder
            DirectoryInfo originalDirectory = Utility.GetMovieBaseDirectory(movieDir);

            // and build a renamable object for the folder.
            RenamableDirectory renamableDir = new RenamableDirectory(movie, originalDirectory);
            renamableDir.NewName = GetNewDirectoryName(movie);
            return renamableDir;
        }

        // method replaces encoded file rename string to contain movie specific information.
        private string GetNewFileName(DBLocalMedia file) {
           
            // use existing or generate new base filename
            string baseFileName;
            if (baseFileNameLookup.ContainsKey(file.AttachedMovies[0]))
                baseFileName = baseFileNameLookup[file.AttachedMovies[0]];
            else {
                // parse variables in pattern for base filename
                string pattern = MovingPicturesCore.Settings.FileRenameString;
                baseFileName = ReplaceVariables(file.AttachedMovies[0], pattern);

                // store the new base filename for reuse later
                baseFileNameLookup[file.AttachedMovies[0]] = baseFileName;
            }
            
            // add multipart information as needed
            string newFileName = AddPartInfo(baseFileName, file);
         
            // remove characters not accepted by the OS in a filename
            newFileName = Regex.Replace(newFileName, @"[?:\/*""<>|]", "");

            // remove any unwanted double spacing due to missing variables
            newFileName = newFileName.Trim();
            newFileName = newFileName.Replace("  ", " ");

            return newFileName;
        }

        private string GetNewDirectoryName(DBMovieInfo movie) {
            DirectoryInfo movieDir = movie.LocalMedia[0].File.Directory;
            DirectoryInfo baseDir = Utility.GetMovieBaseDirectory(movieDir);

            // parse variables in pattern for new folder name
            string newFolderName = ReplaceVariables(movie, MovingPicturesCore.Settings.DirectoryRenameString);

            // remove characters not accepted by the OS in a filename
            newFolderName = Regex.Replace(newFolderName, @"[?:\/*""<>|]", "");

            // remove any unwanted double spacing due to missing variables
            newFolderName = newFolderName.Trim();
            newFolderName = newFolderName.Replace("  ", " ");

            // remove the original base dir for this movie and add the new one
            int lastDirSeperator = baseDir.FullName.LastIndexOf(Path.DirectorySeparatorChar);
            return baseDir.FullName.Remove(lastDirSeperator + 1) + newFolderName;   
        }

        // add part information as needed
        private string AddPartInfo(string baseFileName, DBLocalMedia file) {
            string newFileName = baseFileName;            
            DBMovieInfo movie = file.AttachedMovies[0];
            int part = movie.LocalMedia.IndexOf(file) + 1;

            // either add the correct part number or remove the part substring as needed
            if (movie.LocalMedia.Count > 1)
                newFileName = newFileName.Replace("#", part.ToString());
            else 
                newFileName = newFileName.Replace(MovingPicturesCore.Settings.FileMultipartString, "");

            return newFileName;
        }

        private string ReplaceVariables(DBMovieInfo movie, string pattern) {
            Dictionary<string, string> mapping = getVariableMapping(movie);

            // initialize our new filename and perform regex lookup to locate variable patterns
            StringBuilder newFileName = new StringBuilder(pattern);
            Regex variableRegex = new Regex(@"\$\{(?<item>[^}]+)\}");
            MatchCollection matches = variableRegex.Matches(pattern);

            // loop through all variables and replace with actual value from movie object
            int replacementOffset = 0;
            foreach (System.Text.RegularExpressions.Match currMatch in matches) {

                // remove the current variable from the new filename
                newFileName.Remove(currMatch.Index + replacementOffset, currMatch.Length);

                // try to find a value for the variable
                string value;
                bool found = mapping.TryGetValue(currMatch.Groups["item"].Value, out value);

                // if there is no variable for what was passed move on to the next variable
                if (!found) {
                    replacementOffset -= currMatch.Length;
                    continue;
                }

                // insert value of variable that was matched and store the offset
                newFileName.Insert(currMatch.Index + replacementOffset, value);
                replacementOffset = replacementOffset - currMatch.Length + value.Length;
            }

            return newFileName.ToString();
        }

        // create the dictionary to map variables to the correct values for this movie
        private Dictionary<string, string> getVariableMapping(DBMovieInfo movie) {
            // load replacement strings
            Dictionary<string, string> replacementStrings = new Dictionary<string, string>();
            foreach (DBField currField in DBField.GetFieldList(typeof(DBMovieInfo))) {
                if (currField.AutoUpdate && currField.GetValue(movie) != null)
                    replacementStrings["movie." + currField.FieldName] = currField.GetValue(movie).ToString().Trim();
            }

            // add the replacement string for multipart movies
            if (MovingPicturesCore.Settings.FileMultipartString != null) 
                replacementStrings["moviepart"] = MovingPicturesCore.Settings.FileMultipartString;

            return replacementStrings;
        }
    }
}
