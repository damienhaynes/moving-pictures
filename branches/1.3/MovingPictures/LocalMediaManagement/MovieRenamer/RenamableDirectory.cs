using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using System.IO;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieRenamer {
    public class RenamableDirectory: Renamable {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public DirectoryInfo OriginalDirectory {
            get { return _originalDirectory; }
        } private DirectoryInfo _originalDirectory = null;


        public RenamableDirectory(DBMovieInfo movie, DirectoryInfo directory) {
            _movie = movie;
            _originalDirectory = directory;
            _originalName = directory.FullName;
        }

        protected override bool RenameWorker() {
            // make sure this job is approved
            if (!Approved) {
                logger.Warn("Tried renaming an unapproved directory!");
                return false;
            }

            //if not already set, store the original filename for reversion purposes
            if (_movie.OriginalDirectoryName == String.Empty) {
                _movie.OriginalDirectoryName = Path.GetFileNameWithoutExtension(_originalName);
            }

            // rename the file!
            try {
                if (OriginalName != NewName)
                    Directory.Move(OriginalName, NewName);
            }
            catch (Exception e) {
                logger.ErrorException("Unexpected error in directory rename process!", e);
                return false;
            }

            return true;
        }
    }
}
