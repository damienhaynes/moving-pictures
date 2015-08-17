using Cornerstone.MP;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.MainUI {
    public class MovingPicturesSkinSettings: SkinSettings {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Creates a SkinSettings object based on a Moving Pictures skin file.
        /// </summary>
        /// <param name="skinFileName"></param>
        public MovingPicturesSkinSettings(string skinFileName)
            : base(skinFileName) {
        }

        #region Available Views

        /// <summary>
        /// Returns true if the specified view is implemented by the current skin.
        /// </summary>
        public bool IsViewAvailable(BrowserViewMode view) {
            switch (view) {
                case BrowserViewMode.FILMSTRIP:
                    return FilmstripViewAvailable;
                case BrowserViewMode.COVERFLOW:
                    return CoverFlowViewAvailable;
                case BrowserViewMode.LARGEICON:
                    return LargeIconViewAvailable;
                case BrowserViewMode.SMALLICON:
                    return IconViewAvailable;
                case BrowserViewMode.LIST:
                    return ListViewAvailable;
                case BrowserViewMode.DETAILS:
                    return DetailsViewAvailable;
                default:
                    logger.Warn("No \"IsViewAvailable\" skin setting for " + view.ToString() + " view.");
                    return true;
            }
        }

        /// <summary>
        /// True if the current skin has implemented the list view.
        /// </summary>
        [SkinSetting("#list.available", true)]
        public bool ListViewAvailable {
            get { return _listViewAvailable; }
            private set { _listViewAvailable = value; }
        }
        private bool _listViewAvailable;

        /// <summary>
        /// True if the current skin has implemented the small icon view.
        /// </summary>
        [SkinSetting("#smallicons.available", true)]
        public bool IconViewAvailable {
            get { return _iconViewAvailable; }
            private set { _iconViewAvailable = value; }
        }
        private bool _iconViewAvailable;

        /// <summary>
        /// True if the current skin has implemented the large icon view.
        /// </summary>
        [SkinSetting("#largeicons.available", true)]
        public bool LargeIconViewAvailable {
            get { return _largeIconViewAvailable; }
            private set { _largeIconViewAvailable = value; }
        }
        private bool _largeIconViewAvailable;

        /// <summary>
        /// True if the current skin has implemented the filmstrip view.
        /// </summary>
        [SkinSetting("#filmstrip.available", true)]
        public bool FilmstripViewAvailable {
            get { return _filmstripViewAvailable; }
            private set { _filmstripViewAvailable = value; }
        }
        private bool _filmstripViewAvailable;

        /// <summary>
        /// True if the current skin has implemented the coverflow view.
        /// </summary>
        [SkinSetting("#coverflow.available", false)]
        public bool CoverFlowViewAvailable
        {
            get { return _coverflowViewAvailable; }
            private set { _coverflowViewAvailable = value; }
        }
        private bool _coverflowViewAvailable;

        /// <summary>
        /// True if the current skin has implemented the details view.
        /// </summary>
        [SkinSetting("#details.available", false)]
        public bool DetailsViewAvailable
        {
            get { return _detailsViewAvailable; }
            private set { _detailsViewAvailable = value; }
        }
        private bool _detailsViewAvailable;

        #endregion

        #region Backdrop Usage

        /// <summary>
        /// Returns true if the backdrop should be displayed for the specified view.
        /// </summary>
        public bool UseBackdrop(BrowserViewMode view) {
            switch (view) {
                case BrowserViewMode.FILMSTRIP:
                    return UseBackdropInFilmstripView;
                case BrowserViewMode.COVERFLOW:
                    return UseBackdropInCoverFlowView;
                case BrowserViewMode.LARGEICON:
                    return UseBackdropInLargeIconView;
                case BrowserViewMode.SMALLICON:
                    return UseBackdropInIconView;
                case BrowserViewMode.LIST:
                    return UseBackdropInListView;
                case BrowserViewMode.DETAILS:
                    return UseBackdropInDetailsView;
                case BrowserViewMode.CATEGORIES:
                    return UseBackdropInCategoriesView;
                default:
                    logger.Warn("No \"UseBackdrop\" skin setting for " + view.ToString() + " view.");
                    return true;
            }
        }

        /// <summary>
        /// True if the backdrop should be used in the list view.
        /// </summary>
        [SkinSetting("#list.backdrop.used", true)]
        public bool UseBackdropInListView {
            get { return _useBackdropInListView; }
            private set { _useBackdropInListView = value; }
        }
        private bool _useBackdropInListView;

        /// <summary>
        /// True if the backdrop should be used in the small icon view.
        /// </summary>
        [SkinSetting("#smallicons.backdrop.used", true)]
        public bool UseBackdropInIconView {
            get { return _useBackdropInIconView; }
            private set { _useBackdropInIconView = value; }
        }
        private bool _useBackdropInIconView;

        /// <summary>
        /// True if the backdrop should be used in the large icon view.
        /// </summary>
        [SkinSetting("#largeicons.backdrop.used", true)]
        public bool UseBackdropInLargeIconView {
            get { return _useBackdropInLargeIconView; }
            private set { _useBackdropInLargeIconView = value; }
        }
        private bool _useBackdropInLargeIconView;

        /// <summary>
        /// True if the backdrop should be used in the filmstrip view.
        /// </summary>
        [SkinSetting("#filmstrip.backdrop.used", true)]
        public bool UseBackdropInFilmstripView {
            get { return _useBackdropInFilmstripView; }
            private set { _useBackdropInFilmstripView = value; }
        }
        private bool _useBackdropInFilmstripView;

        /// <summary>
        /// True if the backdrop should be used in the coverflow view.
        /// </summary>
        [SkinSetting("#coverflow.backdrop.used", true)]
        public bool UseBackdropInCoverFlowView
        {
            get { return _useBackdropInCoverFlowView; }
            private set { _useBackdropInCoverFlowView = value; }
        }
        private bool _useBackdropInCoverFlowView;

        /// <summary>
        /// True if the backdrop should be used in the details view.
        /// </summary>
        [SkinSetting("#details.backdrop.used", true)]
        public bool UseBackdropInDetailsView {
            get { return _useBackdropInDetailsView; }
            private set { _useBackdropInDetailsView = value; }
        }
        private bool _useBackdropInDetailsView;

        /// <summary>
        /// True if the backdrop should be used in the categories view.
        /// </summary>
        [SkinSetting("#categories.backdrop.used", true)]
        public bool UseBackdropInCategoriesView {
            get { return _useBackdropInCategoriesView; }
            private set { _useBackdropInCategoriesView = value; }
        }
        private bool _useBackdropInCategoriesView;

        #endregion
    }
}
