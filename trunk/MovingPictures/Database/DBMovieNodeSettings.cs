using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Database.Tables;
using Cornerstone.Database;
using System.IO;
using MediaPortal.Plugins.MovingPictures.MainUI;

namespace MediaPortal.Plugins.MovingPictures.Database {
    public enum MenuBackdropType { RANDOM, MOVIE, FILE }

    [DBTable("movie_node_settings")]
    public class DBMovieNodeSettings: DatabaseTable {
        [DBField]
        public MenuBackdropType BackdropType {
            get { return _backdropType; }
            set { 
                _backdropType = value;
                commitNeeded = true;
            }
        } private MenuBackdropType _backdropType = MenuBackdropType.RANDOM;

        [DBField (Default=null)]
        public DBMovieInfo BackdropMovie {
            get { return _backdropMovie; }
            set {
                _backdropMovie = value;
                commitNeeded = true;
            }
        } private DBMovieInfo _backdropMovie;

        public FileInfo BackdropFile {
            get { return fileInfo; }
            set {
                fileInfo = value;
                commitNeeded = true;
            }
        }
        private FileInfo fileInfo;

        [DBField]
        public string BackdropFilePath {
            get {
                if (fileInfo == null)
                    return "";

                return fileInfo.FullName;
            }

            set {
                if (value.Trim() == "")
                    fileInfo = null;
                else
                    fileInfo = new FileInfo(value);

                if (fileInfo != null && !fileInfo.Exists)
                    fileInfo = null;

                commitNeeded = true;
            }
        }

        [DBField(Default="true")]
        public bool UseDefaultSorting {
            get { return _useDefaultSorting; }
            set {
                _useDefaultSorting = value;
                commitNeeded = true;
            }
        } private bool _useDefaultSorting = true;

        [DBField(Default="Title")]
        public SortingFields SortField {
            get { return _sortingField; }
            set {
                _sortingField = value;
                commitNeeded = true;
            }
        } private SortingFields _sortingField = SortingFields.Title;

        [DBField]
        public SortingDirections SortDirection {
            get { return _sortDirection; }
            set {
                _sortDirection = value;
                commitNeeded = true;
            }
        } private SortingDirections _sortDirection = SortingDirections.Ascending;
        

    }
}
