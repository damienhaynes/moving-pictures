using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cornerstone.Extensions {

    public static class DateTimeExtensions {

        /// <summary>
        /// Gets an instance of DateTime representing the start of the week for this instance
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static DateTime GetStartOfWeek(this DateTime self) {
            DayOfWeek firstDayOfWeek = Thread.CurrentThread.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            DayOfWeek today = self.DayOfWeek;
            DateTime startOfWeek = self.AddDays(-(today - firstDayOfWeek)).Date;
            return startOfWeek;
        }

        /// <summary>
        /// Gets a new instance of DateTime representing the start of the month for this instance
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static DateTime GetStartOfMonth(this DateTime self) {
            return new DateTime(self.Year, self.Month, 1);
        }

        /// <summary>
        /// Gets a new instance of DateTime representing the start of the year for this instance
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static DateTime GetStartOfYear(this DateTime self) {
            return new DateTime(self.Year, 1, 1);
        }

    }
}
