using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Database.CustomTypes;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.Database;

namespace MediaPortal.Plugins.MovingPictures.MainUI.NewMovieBrowser {
    public class MovieDetailsState: IBrowserState {

        public DynamicList<IFilter<DBMovieInfo>> Filters {
            get;
            set;
        }

        public DBNode<DBMovieInfo> Category {
            get;
            set;
        }
    }
}
