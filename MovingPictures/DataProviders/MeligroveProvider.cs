using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;
using System.Net;
using System.IO;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    class MeligroveProvider: IMovieProvider {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        // drect url access to a backdrop for a movie, given it's imdb id
        private const string meligroveURL = "http://www.meligrove.com/images/posters/movies/jpg/"; // %imdb%.jpg

        public string Name {
            get {
                return "meligrove.com";
            }
        }
        
        public bool ProvidesMoviesDetails {
            get { return false; }
        }

        public bool ProvidesCoverArt {
            get { return false; }
        }

        public bool ProvidesBackdrops {
            get { return true; }
        }

        public bool GetBackdrop(DBMovieInfo movie) {
            if (movie == null)
                return false;

            // if we already have a backdrop move on for now
            if (movie.BackdropFullPath.Trim().Length > 0)
                return true;
            
            // an imdb tag is required for this dataprovider. if it is not set, fail
            if (movie.ImdbID.Trim().Length == 0)
                return false;

            // try to grab a backdrop from meligrove
            ArtworkLoadStatus status = movie.AddBackdropFromURL(meligroveURL + movie.ImdbID.Trim() + ".jpg"); 

            if (movie.BackdropFullPath.Trim().Length > 0)
                return true;
            
            return false;
        }

        public List<DBMovieInfo> Get(string movieTitle) {
            throw new NotImplementedException();
        }

        public void Update(DBMovieInfo movie) {
            throw new NotImplementedException();
        }

        public bool GetArtwork(DBMovieInfo movie) {
            throw new NotImplementedException();
        }

    }
}
