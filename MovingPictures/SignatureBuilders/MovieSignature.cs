using System;
using System.Collections.Generic;
using System.IO;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;

namespace MediaPortal.Plugins.MovingPictures.SignatureBuilders {
    
    /// <summary>
    /// a movie signature object that is used as input to a dataprovider.
    /// </summary>
    public class MovieSignature {

        #region Signature properties

        public string Title     = null; // ex. "Pirates of Silicon Valley"
        public int? Year        = null; // ex. 1999
        public string ImdbId    = null; // ex. "tt0168122"

        public string DiscId {
            get {
                if (LocalMedia != null)
                    discid = LocalMedia[0].DiscId;
                return discid;
            }
            set { discid = value; }
        } private string discid = null;

        public List<DBLocalMedia> LocalMedia = null; // LocalMedia collection

        #region Read-only

        // base folder
        public string Folder {
            get {
                if (folder == null)
                    updatePropertiesFromLocalMedia();

                return folder;
            }
        } private string folder = null;

        // base file
        public string File {
            get {
                if (file == null)
                    updatePropertiesFromLocalMedia();

                return file;
            }
        } private string file = null;

        // path of base folder
        public string Path {
            get {
                if (path == null)
                    updatePropertiesFromLocalMedia();

                return path;
            }
        } private string path = null;
             
        #endregion

        #endregion

        #region Ctor

        public MovieSignature() {

        }

        public MovieSignature(List<DBLocalMedia> localMedia) {
            LocalMedia = localMedia;
        }

        public MovieSignature(string title) {
            Title = title;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Updates the File, Folder and Path property using the LocalMedia data
        /// </summary>
        private void updatePropertiesFromLocalMedia() {
            if (LocalMedia != null) {
                DirectoryInfo baseFolder = Utility.GetMovieBaseDirectory(LocalMedia[0].File.Directory);
                folder = baseFolder.Name;
                file = LocalMedia[0].File.Name;
                path = baseFolder.FullName;
            }
        }

        #endregion

        #region Overrides

        public override string ToString() {
            return String.Format("Path= {0} || Folder= {1}|| File= {2} || Title= \"{3}\" || Year= {4} || DiscId= {5} || ImdbId= {6} || ",
            this.Path, this.Folder, this.File, this.Title, this.Year.ToString(), this.DiscId, this.ImdbId);
        }

        #endregion

    }
}
