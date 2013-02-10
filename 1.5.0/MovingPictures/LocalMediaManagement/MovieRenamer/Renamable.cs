using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieRenamer {
    public abstract class Renamable {

        public DBMovieInfo Movie {
            get { return _movie; }
        } protected DBMovieInfo _movie;

        /// <summary>
        /// Value to track over time if this rename operation has been approved. 
        /// </summary>
        public bool Approved {
            get { return _approved; }
            set { _approved = value; }
        } protected bool _approved = true;

        public string OriginalName {
            get { return _originalName; }
        } protected string _originalName;

        public string NewName {
            get { return _newName; }
            set { _newName = value; }
        } protected string _newName;

        public string FinalNewName {
            get { return _finalNewName; }
            set { _finalNewName = value; }
        } protected string _finalNewName;

        public bool Rename() {
            if (Approved)
                return RenameWorker();
            
            return true;
        }

        protected abstract bool RenameWorker();
        
    }
}
