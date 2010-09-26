using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Extensions;
using Cornerstone.Database.Tables;

namespace Cornerstone.Tools {
    
    /// <summary>
    /// Static DateTime Helper class
    /// </summary>
    public static class DateTimeHelper {

        #region DateTime Helper Methods

        public static DateTime Yesterday {
            get {
                return DateTime.Today.AddDays(-1);
            }
        }

        public static DateTime ThisWeek {
            get {
                return DateTime.Today.GetStartOfWeek();
            }
        }

        public static DateTime LastWeek {
            get {
                return DateTime.Today.AddDays(-7).GetStartOfWeek();
            }
        }

        public static DateTime TwoWeeksAgo {
            get {
                return DateTime.Today.AddDays(-14).GetStartOfWeek();
            }
        }

        public static DateTime ThreeWeeksAgo {
            get {
                return DateTime.Today.AddDays(-21).GetStartOfWeek();
            }
        }

        public static DateTime ThisMonth {
            get {
                return DateTime.Today.GetStartOfMonth();
            }
        }

        public static DateTime LastMonth {
            get {
                return DateTime.Today.GetStartOfMonth().AddMonths(-1);
            }
        }

        public static DateTime TwoMonthsAgo {
            get {
                return DateTime.Today.GetStartOfMonth().AddMonths(-2);
            }
        }

        public static DateTime ThreeMonthsAgo {
            get {
                return DateTime.Today.GetStartOfMonth().AddMonths(-3);
            }
        }

        public static DateTime ThisYear {
            get {
                return DateTime.Today.GetStartOfYear();
            }
        }

        public static DateTime LastYear {
            get {
                return DateTime.Today.GetStartOfYear().AddYears(-1);
            }
        }

        #endregion       

    }
}
