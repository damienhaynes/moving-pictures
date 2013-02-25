using Cornerstone.Database.CustomTypes;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.Database;

namespace MediaPortal.Plugins.MovingPictures.MainUI.NewMovieBrowser {
    public interface IBrowserState {

        /// <summary>
        /// Filters required by this browser state. This does not include global filters such as
        /// the parental controls filter or the remote control filter.
        /// </summary>
        DynamicList<IFilter<DBMovieInfo>> Filters { get; set; }

        /// <summary>
        /// The category that specifies which movies are displayed in this state. Can be set
        /// to null if there is no owning category.
        /// </summary>
        DBNode<DBMovieInfo> Category { get; set; }
    }
}
