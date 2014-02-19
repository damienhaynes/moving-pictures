using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Database.CustomTypes;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.Database;

namespace MediaPortal.Plugins.MovingPictures.MainUI.NewMovieBrowser {
    public class MovieCollectionState: IBrowserState {
        public enum ViewMode { List, SmallIcon, LargeIcon, FilmStrip, Coverflow }

        public DynamicList<IFilter<DBMovieInfo>> Filters {
            get;
            set;
        }

        public DBNode<DBMovieInfo> Category {
            get;
            set;
        }
        
        /// <summary>
        /// The facade view mode to use when displaying the movie collection.
        /// </summary>
        public ViewMode View { get; set; }

        /// <summary>
        /// The movie selected in this browser state.
        /// </summary>
        public DBMovieInfo SelectedMovie { get; set; }

    }
}
