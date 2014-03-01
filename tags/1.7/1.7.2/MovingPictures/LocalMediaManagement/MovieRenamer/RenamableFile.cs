using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieRenamer {
    public class RenamableFile: Renamable {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Properties

        public DBLocalMedia LocalMedia {
            get { return _localMedia; }
        } private DBLocalMedia _localMedia = null;

        public FileInfo OriginalFile {
            get { return _originalFile; }
        } private FileInfo _originalFile = null;

        #endregion

        public RenamableFile(DBMovieInfo movie, DBLocalMedia localMedia) {
            _movie = movie;
            _localMedia = localMedia;
            _originalName = localMedia.FullPath;
            _originalFile = localMedia.File;
        }

        public RenamableFile(DBMovieInfo movie, FileInfo originalFile) {
            _movie = movie;
            _originalFile = originalFile;
            _originalName = originalFile.FullName;
        }

        protected override bool RenameWorker() {
            // make sure this job is approved
            if (!Approved) {
                logger.Warn("Tried renaming an unapproved file!");
                return false;
            }

            //if not already set, store the original filename for reversion purposes
            if (_localMedia != null && _localMedia.OriginalFileName == String.Empty) {
                _localMedia.OriginalFileName = Path.GetFileNameWithoutExtension(_originalName);
                _localMedia.Commit();
            }

            // rename the file!
            try {
                File.Move(OriginalName, NewName);
            }
            catch (Exception e) {
                logger.ErrorException("Unexpected error in file rename process!", e);
                return false;
            }
            
            return true;
        }
    }
}
