using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database;
using Cornerstone.Database.Tables;

namespace MediaPortal.Plugins.MovingPictures.Database {
    [DBTableAttribute("sort_preferences")]
    public class DBSortPreferences : MovingPicturesDBTable {

        #region Database Fields
        [DBFieldAttribute(Default="True")]
        public bool SortTitleAscending {
            get { return _sortTitleAscending; }
            set {
                if (_sortTitleAscending != value) {
                    _sortTitleAscending = value;
                    commitNeeded = true;
                }
            }
        } private bool _sortTitleAscending;

        [DBFieldAttribute(Default = "False")]
        public bool SortDateAddedAscending {
            get { return _sortDateAddedAscending; }
            set {
                if (_sortDateAddedAscending != value) {
                    _sortDateAddedAscending = value;
                    commitNeeded = true;
                }
            }
        } private bool _sortDateAddedAscending;

        [DBFieldAttribute(Default = "False")]
        public bool SortYearAscending {
            get { return _sortYearAscending; }
            set {
                if (_sortYearAscending != value) {
                    _sortYearAscending = value;
                    commitNeeded = true;
                }
            }
        } private bool _sortYearAscending;

        [DBFieldAttribute(Default = "True")]
        public bool SortCertificationAscending {
            get { return _sortCertificationAscending; }
            set {
                if (_sortCertificationAscending != value) {
                    _sortCertificationAscending = value;
                    commitNeeded = true;
                }
            }
        } private bool _sortCertificationAscending;

        [DBFieldAttribute(Default = "True")]
        public bool SortLanguageAscending {
            get { return _sortLanguageAscending; }
            set {
                if (_sortLanguageAscending != value) {
                    _sortLanguageAscending = value;
                    commitNeeded = true;
                }
            }
        } private bool _sortLanguageAscending;

        [DBFieldAttribute(Default = "False")]
        public bool SortScoreAscending {
            get { return _sortScoreAscending; }
            set {
                if (_sortScoreAscending != value) {
                    _sortScoreAscending = value;
                    commitNeeded = true;
                }
            }
        } private bool _sortScoreAscending;

        [DBFieldAttribute(Default = "False")]
        public bool SortPopularityAscending {
            get { return _sortPopularityAscending; }
            set {
                if (_sortPopularityAscending != value) {
                    _sortPopularityAscending = value;
                    commitNeeded = true;
                }
            }
        } private bool _sortPopularityAscending;

        [DBFieldAttribute(Default = "True")]
        public bool SortRuntimeAscending {
            get { return _sortRuntimeAscending; }
            set {
                if (_sortRuntimeAscending != value) {
                    _sortRuntimeAscending = value;
                    commitNeeded = true;
                }
            }
        } private bool _sortRuntimeAscending;

        [DBFieldAttribute(Default = "True")]
        public bool SortFilePathAscending {
            get { return _sortFilePathAscending; }
            set {
                if (_sortFilePathAscending != value) {
                    _sortFilePathAscending = value;
                    commitNeeded = true;
                }
            }
        } private bool _sortFilePathAscending;

        #endregion

        static DBSortPreferences instance;


        #region Database Management Methods


        public static DBSortPreferences Instance {
            get {
                if (instance == null) {
                    var all = MovingPicturesCore.DatabaseManager.Get<DBSortPreferences>(null);

                    if (all.Count > 0)
                        instance = all[0];
                    else {
                        instance = new DBSortPreferences();
                        instance.Commit();
                    }
                }
                return instance;
            }
        }


        #endregion
    }
}
