using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using Cornerstone.Database.Tables;

namespace MediaPortal.Plugins.MovingPictures.MainUI.Filters {
    public class WatchedFlagFilter: IFilter<DBMovieInfo> {
        public event FilterUpdatedDelegate<DBMovieInfo> Updated;

        public bool Active {
            get { return active; }
            set {
                if (active != value) {
                    active = value;
                    
                    if (Updated != null)
                        Updated(this);
                }
            }
        }
        private bool active;

        public HashSet<DBMovieInfo> Filter(ICollection<DBMovieInfo> input) {
            HashSet<DBMovieInfo> results = new HashSet<DBMovieInfo>();

            // if we are not active, just return the inputs.
            if (!active) {
                if (input is HashSet<DBMovieInfo>)
                    return (HashSet<DBMovieInfo>)input;

                foreach (DBMovieInfo currItem in input)
                    results.Add(currItem);

                return results;
            }

            Predicate<DBMovieInfo> unwatched = delegate(DBMovieInfo item) {
                return item.ActiveUserSettings.WatchedCount == 0;
            };

            // Filter the list with the specified critera
            foreach (DBMovieInfo currMovie in input) {
                if (currMovie.ActiveUserSettings.WatchedCount == 0)
                    results.Add(currMovie);
            }
            return results;
        }

    }
}