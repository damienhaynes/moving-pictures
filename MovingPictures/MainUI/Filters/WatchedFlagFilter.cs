using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;

namespace MediaPortal.Plugins.MovingPictures.MainUI.Filters {
    public class WatchedFlagFilter: IBrowserFilter {
        public event FilterUpdatedDelegate Updated;

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

        public List<DBMovieInfo> Filter(List<DBMovieInfo> input) {
            if (active) {

                Predicate<DBMovieInfo> unwatched = delegate(DBMovieInfo item) {
                    return item.ActiveUserSettings.Watched == 0;
                };

                // Filter the list with the specified critera
                List<DBMovieInfo> filteredList = input.FindAll(unwatched);
                return filteredList;
            }

            return input;
        }

    }
}
