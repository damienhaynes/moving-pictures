using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    interface IMovieProvider {
        List<DBMovieInfo> Get(string movieTitle);
        void Update(DBMovieInfo movie);
        bool GetArtwork(DBMovieInfo movie);
    }
}
