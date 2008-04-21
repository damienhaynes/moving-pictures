using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables {
    [DBTableAttribute("import_path")]
    class DBImportPath: MoviesPluginDBTable  {
        private static Logger logger = LogManager.GetCurrentClassLogger();
                
        public DBImportPath()
            : base() {
            dirInfo = null;
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
        
        public List<DBLocalMedia> GetLocalMedia(bool returnOnlyNew) {
            if (Directory == null)
                return null;

            List<DBLocalMedia> rtn = new List<DBLocalMedia>();

            // grab the list of files and parse out appropriate ones based on extension
            try {
                FileInfo[] fileList = Directory.GetFiles("*", SearchOption.AllDirectories);
                foreach (FileInfo currFile in fileList) {
                    foreach (string currExt in MediaPortal.Util.Utils.VideoExtensions) {
                        if (currFile.Extension == currExt) {
                            DBLocalMedia newFile = DBLocalMedia.Get(currFile.FullName);
                            
                            // if this file is in the database continue if we only want new files
                            if (newFile != null && returnOnlyNew)
                                continue;

                            if (newFile == null) {
                                newFile = new DBLocalMedia();
                                newFile.File = currFile;
                            }

                            rtn.Add(newFile);
                            break;
                        }
                    }
                }
            }
            catch (Exception e) {
                logger.Error("Error scanning " + Directory.FullName, e);
            }

            return rtn;
        }

        //public List<FileInfo> GetNewFiles() {
        //    List<FileInfo> fileOnDisk = GetAllFiles();
        //    List<DBLocalMedia> filesInDB = DBLocalMedia.GetAll();

        //    foreach(DBMed
        //}

        #endregion

        // Unfortunately static methods don't work quite right with inheritance, there is
        // no way to determine the superclass, so these can't be placed in the base class.
        // If you have an idea of how to move this to DatabaseTable.cs, by all means let
        // me know.
        #region Database Management Methods

        // Gets the cooresponding Setting based on the given record ID.
        public static DBImportPath Get(int id) {
            return MovingPicturesPlugin.DatabaseManager.Get<DBImportPath>(id);
        }

        public static List<DBImportPath> GetAll() {
            return MovingPicturesPlugin.DatabaseManager.Get<DBImportPath>(null);
        }

        #endregion

    }
}
