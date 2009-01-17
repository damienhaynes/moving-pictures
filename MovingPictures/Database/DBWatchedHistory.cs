using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database.Tables;
using Cornerstone.Database;

namespace MediaPortal.Plugins.MovingPictures.Database {
    [DBTableAttribute("watched_history")]
    public class DBWatchedHistory : MovingPicturesDBTable {

        #region Database Fields

        [DBFieldAttribute]
        public DBUser User {
            get { return _user; }
            set {
                _user = value;
                commitNeeded = true;
            }
        } private DBUser _user;


        [DBFieldAttribute]
        public DBMovieInfo Movie {
            get { return _movie; }
            set {
                _movie = value;
                commitNeeded = true;
            }
        } private DBMovieInfo _movie;


        [DBFieldAttribute(FieldName = "date_watched")]
        public DateTime DateWatched {
            get { return _dateWatched; }

            set {
                _dateWatched = value;
                commitNeeded = true;
            }
        } private DateTime _dateWatched;



        #endregion
        
        public static void AddWatchedHistory(DBMovieInfo movie, DBUser user) {
            DBWatchedHistory history = new DBWatchedHistory();
            history.DateWatched = DateTime.Now;
            history.Movie = movie;
            history.User = user;

            movie.WatchedHistory.Add(history);
            history.Commit();
            movie.Commit();
        }
    }
}
