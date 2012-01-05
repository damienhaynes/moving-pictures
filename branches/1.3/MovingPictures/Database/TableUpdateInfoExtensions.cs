using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Database;

namespace MediaPortal.Plugins.MovingPictures.Database {
    public static class TableUpdateInfoExtensions {
        public static bool RatingChanged(this TableUpdateInfo self) {
            return self.UpdatedFields.Contains(DBField.GetField(typeof(DBUserMovieSettings), "UserRating"));
        }

        public static bool WatchedCountChanged(this TableUpdateInfo self) {
            return self.UpdatedFields.Contains(DBField.GetField(typeof(DBUserMovieSettings), "WatchedCount"));
        }

    }
}
