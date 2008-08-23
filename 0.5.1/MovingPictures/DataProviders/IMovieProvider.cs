using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    public interface IMovieProvider {
        List<DBMovieInfo> Get(string movieTitle);
        void Update(DBMovieInfo movie);
    }

    public interface ICoverArtProvider {
        bool GetArtwork(DBMovieInfo movie);
    }

    public interface IBackdropProvider {
        bool GetBackdrop(DBMovieInfo movie);
    }
}
