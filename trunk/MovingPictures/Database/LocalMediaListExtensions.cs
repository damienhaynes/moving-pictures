using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPortal.Plugins.MovingPictures.Database {
    public static class LocalMediaListExtensions {
        /// <summary>
        /// Returns true if any of the objects in the list have subtitles.
        /// </summary>
        public static bool HasSubtitles(this IList<DBLocalMedia> mediaList) {
            bool rtn = false;
            foreach(DBLocalMedia currMedia in mediaList)  
                if (currMedia.HasSubtitles) rtn = true;

            return true;
        }
    }
}
