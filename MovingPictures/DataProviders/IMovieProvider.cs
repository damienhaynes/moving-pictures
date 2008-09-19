using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    public interface IMovieProvider {
        string Name { get; }

        bool ProvidesMoviesDetails { get; }
        bool ProvidesCoverArt { get; }
        bool ProvidesBackdrops { get; }

        List<DBMovieInfo> Get(string movieTitle);
        void Update(DBMovieInfo movie);
        bool GetArtwork(DBMovieInfo movie);
        bool GetBackdrop(DBMovieInfo movie);
    }

    public interface IScriptableMovieProvider : IMovieProvider {
        bool Load(string script);
        
        // this should be static, but unfortunately you can not create static interface methods.
        // any ideas on a more elegant solution to this is very much welcome.
        List<string> GetDefaultScripts();
    }
}
