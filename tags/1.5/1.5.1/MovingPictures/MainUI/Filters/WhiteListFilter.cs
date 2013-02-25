using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.Database;

namespace MediaPortal.Plugins.MovingPictures.MainUI.Filters {
    public class WhiteListFilter: IFilter<DBMovieInfo> {
        public event FilterUpdatedDelegate<DBMovieInfo> Updated;

        private readonly HashSet<DBMovieInfo> whiteList;

        public WhiteListFilter(IEnumerable<DBMovieInfo> movies) {
            whiteList = new HashSet<DBMovieInfo>();   
            whiteList.UnionWith(movies);
        }

        public HashSet<DBMovieInfo> Filter(ICollection<DBMovieInfo> input) {
            var filtered = new HashSet<DBMovieInfo>(input);
            filtered.IntersectWith(whiteList);
            return filtered;
        }

        public HashSet<DBMovieInfo> Filter(ICollection<DBMovieInfo> input, bool forceActive) {
            return Filter(input);
        }

        public bool Active {
            get { return true; }
        }
    }
}
