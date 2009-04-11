using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Plugins.MovingPictures.Database;

namespace MediaPortal.Plugins.MovingPictures.MainUI {
    public class GroupHeaders {
        private static MovieBrowser Browser;

        public static void AddGroupHeaders(MovieBrowser browser) {
            Browser = browser;
            for (int i = Browser.Facade.Count - 1; i >= 0; i--) {
                string priorGroupName = "";
                string thisGroupName = "";
                
                DBMovieInfo thisMovie = ((DBMovieInfo)Browser.Facade[i].TVTag);
                thisGroupName = DetermineGroupName(thisMovie);

                if (i > 0) {
                    DBMovieInfo priorMovie = ((DBMovieInfo)Browser.Facade[i - 1].TVTag);
                    priorGroupName = DetermineGroupName(priorMovie);
                }
                else {
                    priorGroupName = "";
                }
                
                if (priorGroupName != thisGroupName) {
                    // if this movie's group differs from the prior, insert a group header
                    InsertGroupHeader(i, thisGroupName);
                }
            }
        }

        private static string DetermineGroupName(DBMovieInfo movie) {
            
            switch (Browser.CurrentSortField) {
                case SortingFields.Title:
                    // Either the first character of the title, or the word "Numeric"
                    // should we have a category for special characters like numeric has?
                    string groupName = "";
                    if (movie.SortBy.Trim().Length > 0)
                        groupName = movie.SortBy.Trim().Substring(0, 1).ToUpper();
                    int iTemp;
                    if (int.TryParse(groupName, out iTemp))
                        groupName = "Numeric";
                    return groupName;

                case SortingFields.DateAdded:
                    return GetDateGroupName(movie.DateAdded.Date);
                case SortingFields.Year:
                    return movie.Year.ToString();
                case SortingFields.Certification:
                    return movie.Certification.Trim().ToUpper();
                case SortingFields.Language:
                    return movie.Language.Trim().ToUpper();
                case SortingFields.Score:
                    return movie.Score.ToString();
                case SortingFields.Runtime:
                    return "";
                    break;
                case SortingFields.FilePath:
                    if (movie.LocalMedia.Count > 0)
                        return movie.LocalMedia[0].File.Directory.ToString();
                    else
                        return "";
                default:
                    return "";
            }
        }

        private static string GetDateGroupName(DateTime date) {
            DateTime todayDate = DateTime.Today;
            if (date > todayDate)
                return Translation.Future;

            if (date == todayDate)
                return Translation.Today;
            if (date == todayDate.AddDays(-1))
                return Translation.Yesterday;

            if (date >= GetStartOfWeek(todayDate))
                return Translation.GetByName(date.DayOfWeek.ToString());


            if (date >= GetStartOfWeek(todayDate.AddDays(-7)))
                return Translation.LastWeek;
            if (date >= GetStartOfWeek(todayDate.AddDays(-14)))
                return Translation.TwoWeeksAgo;
            if (date >= GetStartOfWeek(todayDate.AddDays(-21)))
                return Translation.ThreeWeeksAgo;

            if (date >= GetStartOfMonth(todayDate).AddMonths(-1))
                return Translation.LastMonth;
            if (date >= GetStartOfMonth(todayDate).AddMonths(-2))
                return Translation.TwoMonthsAgo;
            if (date >= GetStartOfMonth(todayDate).AddMonths(-3))
                return Translation.ThreeMonthsAgo;
            if (date >= GetStartOfYear(todayDate))
                return Translation.EarlierThisYear;
            if (date >= GetStartOfYear(todayDate).AddYears(-1))
                return Translation.LastYear;

            return Translation.Older;
        }

        private static DateTime GetStartOfWeek(DateTime date) {
            DayOfWeek fdow = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            DayOfWeek today = date.DayOfWeek;
            DateTime sow = date.AddDays(-(today - fdow)).Date;
            return sow;
        }
        private static DateTime GetStartOfMonth(DateTime date) {
            return new DateTime(date.Year, date.Month, 1);
        }
        private static DateTime GetStartOfYear(DateTime date) {
            return new DateTime(date.Year, 1, 1);
        }

        private static void InsertGroupHeader(int index, string groupLabel) {
            // create label text
            string labelText = "--- " + groupLabel + " ";
            float textwidth = 0; float textheight = 0;
            while (textwidth <= Browser.Facade.ListView.Width) {
                GUIFontManager.GetFont(Browser.Facade.ListView.FontName).GetTextExtent(labelText, ref textwidth, ref textheight);
                labelText += "-";
            }
            // remove 1 char
            labelText = labelText.Substring(0, labelText.Length - 1);

            GUIListItem groupItem = new GUIListItem();
            groupItem.Label = labelText;
            groupItem.IsRemote = true;

            Browser.Facade.Insert(index, groupItem);

            groupItem.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(Browser.onFacadeItemSelected);
        }
    }
}
