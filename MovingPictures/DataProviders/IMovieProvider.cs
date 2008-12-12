using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    public interface IMovieProvider {
        string Name { get; }
        string Version { get; }
        string Author { get; }
        string Description { get; }
        string Language { get; }

        bool ProvidesMoviesDetails { get; }
        bool ProvidesCoverArt { get; }
        bool ProvidesBackdrops { get; }

        List<DBMovieInfo> Get(MovieSignature movieSignature);
        UpdateResults Update(DBMovieInfo movie);
        bool GetArtwork(DBMovieInfo movie);
        bool GetBackdrop(DBMovieInfo movie);
    }

    public interface IScriptableMovieProvider : IMovieProvider {
        int ScriptID { get; }
        DateTime? Published { get; }

        bool Load(string script);
        bool DebugMode { get; set; }
    }

    public enum DataType { DETAILS, COVERS, BACKDROPS }
    public enum UpdateResults { SUCCESS, FAILED_NEED_ID, FAILED }
}
