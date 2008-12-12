using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using NLog;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using System.Threading;

namespace MediaPortal.Plugins.MovingPictures.Database {
    [DBTableAttribute("import_path")]
    public class DBImportPath: MovingPicturesDBTable  {
        private static Logger logger = LogManager.GetCurrentClassLogger();
                
        public DBImportPath()
            : base() {
            dirInfo = null;
        }

        public override void AfterDelete() {
        }
        
        public DirectoryInfo Directory {
            get { return dirInfo; }
            
            set {
                dirInfo = value;
                commitNeeded = true;
            }
        } 
        private DirectoryInfo dirInfo;

        #region Database Fields

        [DBFieldAttribute(FieldName="path")]
        public string FullPath {
            get { 
                if (dirInfo == null)
                    return "";
                
                return dirInfo.FullName;
            }

            set {
                if (value.Trim() == "")
                    dirInfo = null;
                else
                    dirInfo = new DirectoryInfo(value);

                commitNeeded = true;
            }
        }


        [DBFieldAttribute(Default = "false")]
        public bool Removable {
            get { return removable; }
            set {
                removable = value;
                commitNeeded = true;
            }
        } private bool removable;

        #endregion

        #region Public Methods

        public List<DBLocalMedia> GetAllLocalMedia() {
            return GetLocalMedia(false);
        }

        public List<DBLocalMedia> GetNewLocalMedia() {
            return GetLocalMedia(true);
        }

        public static List<FileInfo> getFilesRecursive(DirectoryInfo inputDir) {
            List<FileInfo> fileList = new List<FileInfo>();
            DirectoryInfo[] childDirs = new DirectoryInfo[] { };

            try {
                fileList.AddRange(inputDir.GetFiles("*"));
                childDirs = inputDir.GetDirectories();
            }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;

                logger.Error("Error while retrieving files/directories for: " + inputDir.FullName, e);
            }

            foreach (DirectoryInfo currChildDir in childDirs) {
                try {
                    if ((currChildDir.Attributes & FileAttributes.System) == 0) 
                        fileList.AddRange(getFilesRecursive(currChildDir));
                    else 
                        logger.Debug("Rejecting directory " + currChildDir.FullName + " because it is flagged as a System folder.");
                }
                catch (Exception e) {
                    if (e.GetType() == typeof(ThreadAbortException))
                        throw e;
                    logger.Error("Error during attribute check for: " + currChildDir.FullName, e);
                }
            }

            return fileList;
        }    


        public List<DBLocalMedia> GetLocalMedia(bool returnOnlyNew) {
            if (Directory == null)
                return null;

            List<DBLocalMedia> rtn = new List<DBLocalMedia>();

            // grab the list of files and parse out appropriate ones based on extension
            try {
                List<FileInfo> fileList = getFilesRecursive(Directory);
                foreach (FileInfo currFile in fileList) {
                    DBLocalMedia newFile = DBLocalMedia.Get(currFile.FullName);
                    foreach (string currExt in MediaPortal.Util.Utils.VideoExtensions) {
                        if (currFile.Extension.ToLower().Equals(currExt.ToLower())) {

                            // if this file is in the database continue if we only want new files
                            if (newFile.ID != null && returnOnlyNew) 
                                break;
                            
                            // good extension for new file, so add it
                            logger.Debug("Pulling new file " + currFile.Name + " from import path.");
                            newFile.ImportPath = this;
                            rtn.Add(newFile);
                            break;
                        }
                    }
                }
            }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;
                logger.Error("Error scanning " + Directory.FullName, e);
            }

            return rtn;
        }

        public override string ToString() {
            return FullPath;
        }        


        #endregion

        // Unfortunately static methods don't work quite right with inheritance, there is
        // no way to determine the superclass, so these can't be placed in the base class.
        // If you have an idea of how to move this to DatabaseTable.cs, by all means let
        // me know.
        #region Database Management Methods

        // Gets the cooresponding Setting based on the given record ID.
        public static DBImportPath Get(int id) {
            return MovingPicturesCore.DatabaseManager.Get<DBImportPath>(id);
        }

        public static List<DBImportPath> GetAll() {
            return MovingPicturesCore.DatabaseManager.Get<DBImportPath>(null);
        }

        #endregion

    }
}