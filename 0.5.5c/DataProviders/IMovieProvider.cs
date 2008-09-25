using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    public interface IMovieProvider {
        bool ProvidesMoviesDetails { get; }
        List<DBMovieInfo> Get(string movieTitle);
        void Update(DBMovieInfo movie);
    }

    public interface ICoverArtProvider {
        bool ProvidesCoverArt { get; }
        bool GetArtwork(DBMovieInfo movie);
    }

    public interface IBackdropProvider {
        bool ProvidesBackdrops { get; }
        bool GetBackdrop(DBMovieInfo movie);
    }
}
