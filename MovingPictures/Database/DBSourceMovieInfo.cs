using Cornerstone.Database;
using Cornerstone.Database.Tables;
using NLog;
using System;
using System.Collections.Generic;

namespace MediaPortal.Plugins.MovingPictures.Database
{
    [DBTable("source_movie_info")]
    public class DBSourceMovieInfo: MovingPicturesDBTable{
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [DBField]
        public DBSourceInfo Source {
            get { return source; }
            set {
                source = value;
                commitNeeded = true;
            }
        } private DBSourceInfo source;

        public int? ScriptID {
            get {
                if (source.IsScriptable())
                    return source.SelectedScript.Provider.ScriptID;

                return null;
            }
        }

        [DBField]
        public DBMovieInfo Movie {
            get { return movie; }
            set {
                movie = value;
                commitNeeded = true;
            }
        } private DBMovieInfo movie;

        [DBField(Default = null)]
        public String Identifier {
            get { return identifier; }
            set {
                identifier = value;
                commitNeeded = true;
            }
        } private String identifier;

        #region Database Management Methods

        public static DBSourceMovieInfo Get(int id) {
            return MovingPicturesCore.DatabaseManager.Get<DBSourceMovieInfo>(id);
        }

        public static List<DBSourceMovieInfo> GetAll() {
            return MovingPicturesCore.DatabaseManager.Get<DBSourceMovieInfo>(null);
        }

        #endregion

        #region Static Methods

        public static DBSourceMovieInfo Get(DBMovieInfo movie, DBSourceInfo source) {
            foreach (DBSourceMovieInfo currInfo in movie.SourceMovieInfo)
                if (currInfo.Source == source)
                    return currInfo;

            return null;
        }

        public static DBSourceMovieInfo Get(DBMovieInfo movie, int scriptID) {
            return Get(movie, DBSourceInfo.GetFromScriptID(scriptID));
        }

        public static DBSourceMovieInfo GetOrCreate(DBMovieInfo movie, DBSourceInfo source) {
            DBSourceMovieInfo rtn = Get(movie, source);
            if (rtn != null)
                return rtn;

            rtn = new DBSourceMovieInfo();
            rtn.Movie = movie;
            rtn.Source = source;

            // if this is the IMDb data source, populate the id with the imdb_id field
            if (rtn.ScriptID == 874902 && !string.IsNullOrEmpty(movie.ImdbID) && movie.ImdbID.Trim().Length >= 9)
                rtn.Identifier = movie.ImdbID;

            movie.SourceMovieInfo.Add(rtn);
            return rtn;
        }

        public static DBSourceMovieInfo GetOrCreate(DBMovieInfo movie, int scriptID) {
            return GetOrCreate(movie, DBSourceInfo.GetFromScriptID(scriptID));
        }

        #endregion

        public override string ToString() {
            return "DBSourceMovieInfo [title='" + (Movie == null ? "???" : Movie.Title) + "',  identifier='" + (identifier == null ? "???" : identifier) + "', provider='" + (Source == null || Source.Provider == null ? "???" : Source.Provider.Name) + "']";
        }

    }
}
