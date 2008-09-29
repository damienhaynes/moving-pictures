using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    public interface IMovieProvider {
        string Name { get; }
        //string Version { get; }
        //string Author { get; }
        //string Description { get; }

        bool ProvidesMoviesDetails { get; }
        bool ProvidesCoverArt { get; }
        bool ProvidesBackdrops { get; }

        List<DBMovieInfo> Get(MovieSignature movieSignature);
        void Update(DBMovieInfo movie);
        bool GetArtwork(DBMovieInfo movie);
        bool GetBackdrop(DBMovieInfo movie);
    }

    public interface IScriptableMovieProvider : IMovieProvider {
        bool Load(string script);
    }

    public enum DataType { DETAILS, COVERS, BACKDROPS }
}
