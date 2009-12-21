using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;
using MediaPortal.Plugins.MovingPictures.DataProviders;
using System.IO;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {
    public class MovieRenamer {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        //Dictionary used to hold key->value to decode the MovingPicturesCore.Settings.FileRenameString.
        private Dictionary<string, string> paramList = new Dictionary<string, string>();

        public MovieRenamer() {
            //FileMultipartString must contain a pound sign. The pound sign is replaced by the part number of the movie. If this does not exist in the FileMultipartString, append it.
            if (!MovingPicturesCore.Settings.FileMultipartString.Contains('#')) { MovingPicturesCore.Settings.FileMultipartString = MovingPicturesCore.Settings.FileMultipartString + "#"; }
            //FileRenameString must contain ${moviepart}. This is where the movie part string is added to the file name. If this does not exist in the FileRenameString, append it.
            if (!MovingPicturesCore.Settings.FileRenameString.Contains(@"${moviepart}")) { MovingPicturesCore.Settings.FileRenameString = MovingPicturesCore.Settings.FileRenameString + @"${moviepart}"; }
        }

        public void StartRename(DBMovieInfo movieInfo) {
            int mediaCounter = 0;
            foreach (DBLocalMedia localMediaItem in movieInfo.LocalMedia) {
                //Rebuild paramList for file. 
                this.BuildParamList(movieInfo, localMediaItem);

                //replaceVars switches out the FileRenameString with the vaules in the paramList
                string newFileName = replaceVars(MovingPicturesCore.Settings.FileRenameString, paramList);

                string fileName = Path.GetFileNameWithoutExtension(localMediaItem.FullPath);
                string fileExt = Path.GetExtension(localMediaItem.FullPath);
                string filePath = Path.GetDirectoryName(localMediaItem.FullPath);

                //set the current file name as the original file name in the DB if OriginalFileName is not already set.
                if (localMediaItem.OriginalFileName == String.Empty) { localMediaItem.OriginalFileName = fileName; }

                //Attempt to rename the file based on newFileName. If this is a multi part file ' Part #' will be put at the end of the file.
                try {
                    //Remove forbidden filename characters.
                    newFileName = Regex.Replace(newFileName, @"[?:\/*""<>|]", "");
                    //if multipart file
                    if (movieInfo.LocalMedia.Count > 1) {
                        newFileName = newFileName.Replace("#", (mediaCounter + 1).ToString());
                    }
                    else {
                        newFileName = newFileName.Replace(MovingPicturesCore.Settings.FileMultipartString, "");
                    }

                    //missing variables could introduce some unwanted spaces in file names. removes these.
                    newFileName = newFileName.Trim();
                    newFileName = newFileName.Replace("  ", " ");

                    File.Move(localMediaItem.FullPath, filePath + "\\" + newFileName + fileExt);

                    //look for additional files with the same name.
                    DirectoryInfo dirInfo = new DirectoryInfo(Path.GetDirectoryName(localMediaItem.FullPath));
                    foreach (FileInfo fileInfo in dirInfo.GetFiles(localMediaItem.OriginalFileName + ".*")) {
                        File.Move(fileInfo.FullName, fileInfo.DirectoryName + "\\" + newFileName + fileInfo.Extension);
                    }
                }
                catch (Exception nameChangeError) {
                    logger.Error("Failed to rename file: " + fileName + " to file name: " + newFileName + " Error was: " + nameChangeError.Message);
                }
                mediaCounter++;
            }
        }

        //method for reverting a file back to its original file name.
        public void UndoRename(DBMovieInfo movieInfo) {
            foreach (DBLocalMedia localMediaItem in movieInfo.LocalMedia) {
                string fileName = Path.GetFileNameWithoutExtension(localMediaItem.FullPath);
                string fileExt = Path.GetExtension(localMediaItem.FullPath);
                string filePath = Path.GetDirectoryName(localMediaItem.FullPath);


                //Attempt to rename the file based on localMediaItem.OriginalFileName.
                try {
                    File.Move(localMediaItem.FullPath, filePath + "\\" + localMediaItem.OriginalFileName + fileExt);

                    //look for additional files with the same name.
                    DirectoryInfo dirInfo = new DirectoryInfo(Path.GetDirectoryName(localMediaItem.FullPath));
                    foreach (FileInfo fileInfo in dirInfo.GetFiles(fileName + ".*")) {
                        File.Move(fileInfo.FullName, fileInfo.DirectoryName + "\\" + localMediaItem.OriginalFileName + fileInfo.Extension);
                    }
                    //Once here, the file rename has worked. Remove original file name from the DB.
                    localMediaItem.OriginalFileName = String.Empty;
                }
                catch (Exception nameChangeError) {
                    logger.Error("Failed to rename file: " + fileName + " to file name: " + localMediaItem.OriginalFileName + " Error was: " + nameChangeError.Message);
                }
            }
        }

        private void BuildParamList(DBMovieInfo movieInfo, DBLocalMedia localMediaItem) {
            //rebuild paramList for the spacific file.
            paramList.Clear();
            if (movieInfo.Title != null) paramList["movie.title"] = movieInfo.Title;
            if (movieInfo.Year != 0) paramList["movie.year"] = movieInfo.Year.ToString();
            if (movieInfo.ImdbID != null) paramList["movie.imdb_id"] = movieInfo.ImdbID;
            if (movieInfo.SortBy != null) paramList["movie.sortby"] = movieInfo.SortBy;
            if (movieInfo.Runtime != 0) paramList["movie.runtime"] = movieInfo.Runtime.ToString();
            if (movieInfo.Score.ToString() != null) paramList["movie.score"] = movieInfo.Score.ToString();
            if (movieInfo.Language != null) paramList["movie.language"] = movieInfo.Language;
            if (movieInfo.Certification != null) paramList["movie.certification"] = movieInfo.Certification;
            if (localMediaItem.AudioChannels != null) paramList["movie.audioahannels"] = localMediaItem.AudioChannels;
            if (localMediaItem.AudioCodec != null) paramList["movie.audiocodec"] = localMediaItem.AudioCodec;
            if (localMediaItem.VideoAspectRatio != null) paramList["movie.videoaspectratio"] = localMediaItem.VideoAspectRatio;
            if (localMediaItem.VideoCodec != null) paramList["movie.audioahannels"] = localMediaItem.AudioChannels;
            if (localMediaItem.AudioChannels != null) paramList["movie.videocodec"] = localMediaItem.VideoCodec;
            if (localMediaItem.VideoFrameRate != 0) paramList["movie.videoframerate"] = localMediaItem.VideoFrameRate.ToString();
            if (localMediaItem.VideoHeight != 0) paramList["movie.videoheight"] = localMediaItem.VideoHeight.ToString();
            if (localMediaItem.VideoResolution != null) paramList["movie.videoresolution"] = localMediaItem.VideoResolution;
            if (localMediaItem.VideoWidth != 0) paramList["movie.videowidth"] = localMediaItem.VideoWidth.ToString();
            if (MovingPicturesCore.Settings.FileMultipartString != null) paramList["moviepart"] = MovingPicturesCore.Settings.FileMultipartString;
        }

        //method replaces encoded file rename string to contain movie specific information.
        private string replaceVars(string input, Dictionary<string, string> paramList) {
            StringBuilder strBuild = new StringBuilder(input);
            int offset = 0;

            //find occurances of text like: ${movie.title} this will be replaced by the value in the dictionary (paramList)
            System.Text.RegularExpressions.Regex variablePattern = new System.Text.RegularExpressions.Regex(@"\$\{(?<item>[^}]+)\}");
            System.Text.RegularExpressions.MatchCollection matches = variablePattern.Matches(input);
            foreach (System.Text.RegularExpressions.Match currMatch in matches) {
                string varName = "";
                string value = string.Empty;

                // get rid of the escaped variable string
                strBuild.Remove(currMatch.Index + offset, currMatch.Length);

                // grab details for this parse
                varName = currMatch.Groups["item"].Value;
                paramList.TryGetValue(varName, out value);

                // if there is no variable for what was passed in we are done
                if (value == string.Empty || value == null) {
                    offset -= currMatch.Length;
                    continue;
                }
                strBuild.Insert(currMatch.Index + offset, value);
                offset = offset - currMatch.Length + value.Length;
            }
            return strBuild.ToString();
        }
    }
}
