using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPortal.Plugins.MovingPictures.Database {
    public static class LocalMediaListExtensions {
        /// <summary>
        /// Returns true if any of the video files in the list have subtitles.
        /// </summary>
        public static bool HasSubtitles(this IList<DBLocalMedia> mediaList) {
            foreach (DBLocalMedia currMedia in mediaList)
                if (currMedia.HasSubtitles) return true;

            return false;
        }

        /// <summary>
        /// Returns true if none of the video files in the list are offline.
        /// </summary>
        public static bool IsAvailable(this IList<DBLocalMedia> mediaList) {
            foreach (DBLocalMedia currMedia in mediaList)
                if (!currMedia.IsAvailable) return false;

            return true;
        }

    }
}
