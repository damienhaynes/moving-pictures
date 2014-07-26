using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using System.Collections;
using NLog;
using MediaPortal.GUI.Library;

namespace MediaPortal.Plugins.MovingPictures.MainUI {

    /// <summary>
    /// enum of all possible sort fields
    /// </summary>
    public enum SortingFields {
        Title = 1,
        DateAdded = 2,
        Year = 3,
        Certification = 4,
        Language = 5,
        Score = 6,
        Runtime = 7,
        FilePath = 8,
        ReleaseDate = 9,
        FileSize = 10
    }

    public enum SortingDirections {
        Ascending,
        Descending
    }

    public class Sort {
        public static string GetFriendlySortName(SortingFields field) {
            switch (field) {
                case SortingFields.Title:
                    return Translation.Title;
                case SortingFields.DateAdded:
                    return Translation.DateAdded;
                case SortingFields.Year:
                    return Translation.Year;
                case SortingFields.Certification:
                    return Translation.Certification;
                case SortingFields.Language:
                    return Translation.Language;
                case SortingFields.Score:
                    return Translation.Score;
                case SortingFields.Runtime:
                    return Translation.Runtime;
                case SortingFields.FilePath:
                    return Translation.FilePath;
                case SortingFields.ReleaseDate:
                    return Translation.ReleaseDate;
                case SortingFields.FileSize:
                    return Translation.FileSize;
                default:
                    return "";
            }
        }

        public static SortingDirections GetLastSortDirection(SortingFields field) {
            bool ascending;
            switch (field) {
                case SortingFields.Title:
                    ascending = DBSortPreferences.Instance.SortTitleAscending;
                    break;
                case SortingFields.DateAdded:
                    ascending = DBSortPreferences.Instance.SortDateAddedAscending;
                    break;
                case SortingFields.ReleaseDate:
                    ascending = DBSortPreferences.Instance.SortReleaseDateAscending;
                    break;
                case SortingFields.Year:
                    ascending = DBSortPreferences.Instance.SortYearAscending;
                    break;
                case SortingFields.Certification:
                    ascending = DBSortPreferences.Instance.SortCertificationAscending;
                    break;
                case SortingFields.Language:
                    ascending = DBSortPreferences.Instance.SortLanguageAscending;
                    break;
                case SortingFields.Score:
                    ascending = DBSortPreferences.Instance.SortScoreAscending;
                    break;
                case SortingFields.Runtime:
                    ascending = DBSortPreferences.Instance.SortRuntimeAscending;
                    break;
                case SortingFields.FilePath:
                    ascending = DBSortPreferences.Instance.SortFilePathAscending;
                    break;
                case SortingFields.FileSize:
                    ascending = DBSortPreferences.Instance.SortFileSizeAscending;
                    break;
                default:
                    ascending = true;
                    break;
            }
            if (ascending) return SortingDirections.Ascending;
            else return SortingDirections.Descending;
        }

        public static void StoreLastSortDirection(SortingFields field, SortingDirections sortDirection) {

            bool isAscending = sortDirection == SortingDirections.Ascending;

            switch (field) {
                case SortingFields.Title:
                    DBSortPreferences.Instance.SortTitleAscending = isAscending;
                    break;

                case SortingFields.DateAdded:
                    DBSortPreferences.Instance.SortDateAddedAscending = isAscending;
                    break;

                case SortingFields.ReleaseDate:
                    DBSortPreferences.Instance.SortReleaseDateAscending = isAscending;
                    break;

                case SortingFields.Year:
                    DBSortPreferences.Instance.SortYearAscending = isAscending;
                    break;

                case SortingFields.Certification:
                    DBSortPreferences.Instance.SortCertificationAscending = isAscending;
                    break;

                case SortingFields.Language:
                    DBSortPreferences.Instance.SortLanguageAscending = isAscending;
                    break;

                case SortingFields.Score:
                    DBSortPreferences.Instance.SortScoreAscending = isAscending;
                    break;

                case SortingFields.Runtime:
                    DBSortPreferences.Instance.SortRuntimeAscending = isAscending;
                    break;

                case SortingFields.FilePath:
                    DBSortPreferences.Instance.SortFilePathAscending = isAscending;
                    break;

                case SortingFields.FileSize:
                    DBSortPreferences.Instance.SortFileSizeAscending = isAscending;
                    break;

                default:
                    break;
            }
            if (DBSortPreferences.Instance.CommitNeeded)
                DBSortPreferences.Instance.Commit();
        }
    }


    public class GUIListItemMovieComparer : IComparer<GUIListItem> {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private SortingFields _sortField;
        private SortingDirections _sortDirection;

        /// <summary>
        /// Constructor for GUIListItemMovieComparer
        /// </summary>
        /// <param name="sortField">The database field to sort by</param>
        /// <param name="sortDirection">The direction to sort by</param>
        public GUIListItemMovieComparer(SortingFields sortField, SortingDirections sortDirection) {
            _sortField = sortField;
            _sortDirection = sortDirection;

            Sort.StoreLastSortDirection(sortField, sortDirection);

            logger.Info("Sort Field: {0} Sort Direction: {1}", sortField, sortDirection);
        }


        public int Compare(GUIListItem x, GUIListItem y) {
            try {

                DBMovieInfo movieX = (DBMovieInfo)x.TVTag;
                DBMovieInfo movieY = (DBMovieInfo)y.TVTag;
                int rtn;

                switch (_sortField) {
                    case SortingFields.DateAdded:
                        rtn = movieX.DateAdded.CompareTo(movieY.DateAdded);
                        break;

                    case SortingFields.ReleaseDate:
                        rtn = movieX.ReleaseDate.CompareTo(movieY.ReleaseDate);
                        break;

                    case SortingFields.Year:
                        rtn = movieX.Year.CompareTo(movieY.Year);
                        break;

                    case SortingFields.Certification:
                        int intX = GetCertificationValue(movieX.Certification);
                        int intY = GetCertificationValue(movieY.Certification);
                        if (intX == 100 && intY == 100)
                            rtn = movieX.Certification.CompareTo(movieY.Certification);
                        else
                            rtn = intX.CompareTo(intY);
                        break;

                    case SortingFields.Language:
                        rtn = movieX.Language.CompareTo(movieY.Language);
                        break;

                    case SortingFields.Score:
                        rtn = movieX.Score.CompareTo(movieY.Score);
                        break;

                    case SortingFields.Runtime:
                        rtn = movieX.Runtime.CompareTo(movieY.Runtime);
                        break;

                    case SortingFields.FilePath:
                        rtn = movieX.LocalMedia[0].FullPath.CompareTo(movieY.LocalMedia[0].FullPath);
                        break;

                    case SortingFields.FileSize:
                        rtn = movieX.LocalMedia[0].FileSize.CompareTo(movieY.LocalMedia[0].FileSize);
                        break;

                    // default to the title field
                    case SortingFields.Title:
                    default:
                        rtn = movieX.SortBy.CompareTo(movieY.SortBy);
                        break;
                }

                // if both items are identical, fallback to using the Title
                if (rtn == 0)
                    rtn = movieX.SortBy.CompareTo(movieY.SortBy);

                // if both items are STILL identical, fallback to using the ID
                if (rtn == 0)
                    rtn = movieX.ID.GetValueOrDefault(0).CompareTo(movieY.ID.GetValueOrDefault(0));

                if (_sortDirection == SortingDirections.Descending)
                    rtn = -rtn;

                return rtn;
            }
            catch {
                return 0;
            }
        }

        private int GetCertificationValue(string certification) {
            switch (certification) {
                case "G":
                    return 1;
                case "PG":
                    return 2;
                case "PG-13":
                    return 3;
                case "R":
                    return 4;
                case "NC-17":
                    return 5;
                default:
                    return 100;
            }
        }
    }
}