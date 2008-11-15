using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Configuration;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    public class MyVideosProvider: IMovieProvider {
        #region IMovieProvider Members

        public string Name {
            get { return "MyVideos (Local)"; }
        }

        public string Version {
            get { return "Internal"; }
        }

        public string Author {
            get { return "Moving Pictures Team"; }
        }

        public string Description {
            get { return "Retrieves movie cover artwork previously downloaded via MyVideos."; }
        }

        public string Language {
            get { return ""; }
        }

        public bool ProvidesMoviesDetails {
            get { return false; }
        }

        public bool ProvidesCoverArt {
            get { return true; }
        }

        public bool ProvidesBackdrops {
            get { return false; }
        }

        public List<DBMovieInfo> Get(MovieSignature movieSignature) {
            throw new NotImplementedException();
        }

        public UpdateResults Update(DBMovieInfo movie) {
            throw new NotImplementedException();
        }

        public bool GetArtwork(DBMovieInfo movie) {
            string myVideoCoversFolder = Config.GetFolder(Config.Dir.Thumbs) + "\\Videos\\Title";
            
            Regex cleaner = new Regex("[\\\\/:*?\"<>|]");
            string cleanTitle = cleaner.Replace(movie.Title, "_");

            string filename = myVideoCoversFolder + "\\" + cleanTitle + "L.jpg";

            if (System.IO.File.Exists(filename)) 
                return movie.AddCoverFromFile(filename);
            
            return false;
        }

        public bool GetBackdrop(DBMovieInfo movie) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
