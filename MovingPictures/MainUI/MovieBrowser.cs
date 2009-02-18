using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;
using Cornerstone.Database.Tables;
using Cornerstone.Database;
using System.Collections.ObjectModel;
using Cornerstone.Database.CustomTypes;
using MediaPortal.Plugins.MovingPictures.MainUI.Filters;

namespace MediaPortal.Plugins.MovingPictures.MainUI {
    public enum BrowserViewMode { LIST, SMALLICON, LARGEICON, FILMSTRIP, DETAILS }
    
    public class MovieBrowser {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // lookup for GUIListItems that have been created for DBMovieInfo objects
        private Dictionary<DBMovieInfo, GUIListItem> listItems;

        private MovingPicturesSkinSettings skinSettings;

        #region Properties

        // The currenty selected movie.
        public DBMovieInfo SelectedMovie {
            get {
                return selectedMovie;
            }

            set {
                if (selectedMovie == value)
                    return;

                selectedMovie = value;

                // log the change
                if (selectedMovie != null)
                    logger.Debug("SelectedMovie changed: " + selectedMovie.Title);
                else
                    logger.Debug("SelectedMovie changed: null");

                // notify any listeners
                if (SelectionChanged != null)
                    SelectionChanged(selectedMovie);

                // set the facade selection
                SyncToFacade();
            }
        }
        private DBMovieInfo selectedMovie = null;

        /// <summary>
        /// The current view mode the browser is in. Generally this refers to either
        /// the facade mode or the details page.
        /// </summary>
        public BrowserViewMode CurrentView {
            get {
                return currentView;
            }

            set {
                // update the state variables
                if (currentView != value)
                    previousView = currentView;
                currentView = value;

                ReapplyView();
            }
        }
        private BrowserViewMode currentView;

        /// <summary>
        /// The previous view mode the browser was in before the current view mode was set.
        /// </summary>
        public BrowserViewMode PreviousView {
            get { return previousView; }
        }
        private BrowserViewMode previousView;

        /// <summary>
        /// The default view mode to start the plug-in in, as defined by the user.
        /// </summary>
        public BrowserViewMode DefaultView {
            get {
                string defaultView = ((string)MovingPicturesCore.SettingsManager["default_view"].Value).Trim().ToLower();
                if (defaultView.Equals("list"))
                    return BrowserViewMode.LIST;
                else if (defaultView.Equals("thumbs"))
                    return BrowserViewMode.SMALLICON;
                else if (defaultView.Equals("largethumbs"))
                    return BrowserViewMode.LARGEICON;
                else if (defaultView.Equals("filmstrip"))
                    return BrowserViewMode.FILMSTRIP;
                else {
                    logger.Warn("The DEFAULT_VIEW setting contains an invalid value. Defaulting to List View.");
                    return BrowserViewMode.LIST;
                }
            }
        }

        /// <summary>
        /// The facade control linked to this movie browser.
        /// </summary>
        public GUIFacadeControl Facade {
            get { return facade; }

            set {
                facade = value;
                ReloadFacade();
            }
        }
        public GUIFacadeControl facade;

        
        /// <summary>
        /// All movies available in the movie browser, including those not currently
        /// displayed due to active filters.
        /// </summary>
        public ReadOnlyCollection<DBMovieInfo> AllMovies {
            get { return allMovies.AsReadOnly(); }
        }
        private List<DBMovieInfo> allMovies;

        /// <summary>
        /// All movies currently visible in the movie browser.
        /// </summary>
        public ReadOnlyCollection<DBMovieInfo> FilteredMovies {
            get {
                if (filteredMovies == null)
                    ReapplyFilters();
                return filteredMovies.AsReadOnly(); 
            }
        }
        List<DBMovieInfo> filteredMovies = null;

        /// <summary>
        /// All filters that are active on the movie browser.
        /// </summary>
         public DynamicList<IBrowserFilter> ActiveFilters {
            get {
                if (activeFilters == null) {
                    activeFilters = new DynamicList<IBrowserFilter>();
                    activeFilters.Changed += new ChangedEventHandler(onActiveFiltersChanged);

                    watchedFilters = new DynamicList<IBrowserFilter>();

                }
                return activeFilters; 
            }
        }
        private DynamicList<IBrowserFilter> activeFilters = null;
        private DynamicList<IBrowserFilter> watchedFilters = null;

        public GUIListItemMovieComparer.SortingFields CurrentSortField { get; set; }
        public GUIListItemMovieComparer.SortingDirections CurrentSortDirection { get; set; }

        /// <summary>
        /// Delegate to be called to clear focus of all on screen controls. This
        /// is unfortunately necessary due to MPs poor handling of focus changes.
        /// </summary>
        public ClearFocusDelegate ClearFocusAction {
            set {
                _clearFocusAction = value;
            }
        }
        public delegate void ClearFocusDelegate();
        private ClearFocusDelegate _clearFocusAction;

        #endregion

        #region Events

        public delegate void ContentsChangedDelegate();
        public delegate void SelectionChangedDelegate(DBMovieInfo obj);
        public delegate void ViewChangedDelegate(BrowserViewMode previousView, BrowserViewMode currentView);
        
        public event SelectionChangedDelegate SelectionChanged;
        public event ContentsChangedDelegate ContentsChanged;
        public event ViewChangedDelegate ViewChanged;
        
        #endregion

        #region Public Methods

        public MovieBrowser(MovingPicturesSkinSettings skinSettings) {
            this.skinSettings = skinSettings;

            // setup listeners for new or removed movies
            MovingPicturesCore.DatabaseManager.ObjectDeleted += new DatabaseManager.ObjectAffectedDelegate(onMovieDeleted);
            MovingPicturesCore.DatabaseManager.ObjectInserted += new DatabaseManager.ObjectAffectedDelegate(onMovieAdded);

            listItems = new Dictionary<DBMovieInfo, GUIListItem>();

            loadMovies();
            initSortingDefaults();
        }

        /// <summary>
        /// Rotates the current view.
        /// </summary>
        public void CycleView() {
            if (CurrentView == BrowserViewMode.DETAILS)
                return;

            BrowserViewMode newView = CurrentView;

            do {
                // rotate view until one is available
                if (newView == BrowserViewMode.FILMSTRIP)
                    newView = BrowserViewMode.LIST;
                else
                    newView++;

            } while (!skinSettings.IsViewAvailable(newView));

            previousView = CurrentView;
            CurrentView = newView;
        }

        /// <summary>
        /// Reapplies the current view to the GUI.
        /// </summary>
        public void ReapplyView() {
            logger.Debug("Setting view mode to  " + currentView.ToString() + ".");

            if (facade == null)
                return;

            switch (currentView) {
                case BrowserViewMode.LIST:
                    facade.View = GUIFacadeControl.ViewMode.List;
                    break;
                case BrowserViewMode.SMALLICON:
                    facade.View = GUIFacadeControl.ViewMode.SmallIcons;
                    break;
                case BrowserViewMode.LARGEICON:
                    facade.View = GUIFacadeControl.ViewMode.LargeIcons;
                    break;
                case BrowserViewMode.FILMSTRIP:
                    facade.View = GUIFacadeControl.ViewMode.Filmstrip;
                    break;
                case BrowserViewMode.DETAILS:
                    if (_clearFocusAction != null) _clearFocusAction();

                    facade.Visible = false;
                    facade.ListView.Visible = false;
                    facade.ThumbnailView.Visible = false;
                    facade.FilmstripView.Visible = false;
                    break;
            }

            // if we are leaving details view, set focus back on the facade
            if (previousView == BrowserViewMode.DETAILS) {
                if (_clearFocusAction != null) _clearFocusAction();

                facade.Focus = true;
                facade.Visible = true;
            }

            // sync the selected item and facade as appropriate
            switch (currentView) {
                // if in details view, we dont want to update the facade because it's not in
                // use and we just want to keep the selected movie displayed.
                case BrowserViewMode.DETAILS:
                    break;

                // in filmstrip mode, we cant set the selected movie on the facade, so we sync
                // from whatever is currently selected in the facade
                case BrowserViewMode.FILMSTRIP:
                    SyncFromFacade();
                    break;

                case BrowserViewMode.LIST:
                case BrowserViewMode.SMALLICON:
                case BrowserViewMode.LARGEICON:
                    // if coming from details view, sync from the facade because the selected
                    // movie could no longer be in the facade based on actions in details screen
                    if (previousView == BrowserViewMode.DETAILS)
                        SyncFromFacade();
                    else
                        // otherwise just update the facade with the currently selected movie
                        SyncToFacade();
                    break;
            }

            if (ViewChanged != null) ViewChanged(previousView, currentView);
        }

        /// <summary>
        /// Reapplies all existing filters to the movies in the browser. This should be
        /// called if an existing filter has been modified.
        /// </summary>
        public void ReapplyFilters() {
            // (re)initialize the filtered movie list
            filteredMovies = new List<DBMovieInfo>();
            filteredMovies.Clear();
            filteredMovies.AddRange(allMovies);

            // trim it down to satisfy the filters.
            foreach (IBrowserFilter currFilter in ActiveFilters)
                filteredMovies = currFilter.Filter(filteredMovies);
        }

        #endregion

        #region Core MovieBrowser Methods

        // An initial load of all movies in the database.
        private void loadMovies() {
            // clear the list
            allMovies = new List<DBMovieInfo>();
            
            // populate the list
            List<DBMovieInfo> movies = DBMovieInfo.GetAll();
            foreach (DBMovieInfo currMovie in movies)
                allMovies.Add(currMovie);

            if (ContentsChanged != null)
                ContentsChanged();
        }

        // Sets the initial settings for how movies should be sorted on launch.
        private void initSortingDefaults() {
            // set default sort method
            string defaultSortField = ((string)MovingPicturesCore.SettingsManager["default_sort_field"].Value).Trim();
            string defaultSortDirection = ((string)MovingPicturesCore.SettingsManager["default_sort_direction"].Value).Trim();

            try {
                CurrentSortField = (GUIListItemMovieComparer.SortingFields)Enum.Parse(typeof(GUIListItemMovieComparer.SortingFields), defaultSortField, true);
            }
            catch {
                logger.Error("Invalid Sort Field provided: {0}.  Defaulting to Title", defaultSortField);
                CurrentSortField = GUIListItemMovieComparer.SortingFields.Title;
            }

            try {
                CurrentSortDirection = (GUIListItemMovieComparer.SortingDirections)Enum.Parse(typeof(GUIListItemMovieComparer.SortingDirections), defaultSortDirection, true);
            }
            catch {
                logger.Error("Invalid Sort Direction provided: {0}.  Defaulting to ascending", defaultSortDirection);
                CurrentSortDirection = GUIListItemMovieComparer.SortingDirections.Ascending;
            }
        }

        // Listens for newly added movies from the database manager.
        private void onMovieAdded(DatabaseTable obj) {
            // if this is not a movie object, break
            if (obj.GetType() != typeof(DBMovieInfo))
                return;

            // if this item is already in the list exit. This should never really happen though...  
            if (allMovies.Contains((DBMovieInfo)obj)) {
                logger.Warn("Received multiple \"added\" messages for " + (DBMovieInfo)obj);
                return;
            }

            // add movie to the list, update the facade and log the action
            // a full update of the facade is neccisary because of the possibility
            // that the new item should be filtered out by the ActiveFilters
            logger.Info("Adding " + ((DBMovieInfo)obj).Title + " to movie browser.");
            allMovies.Add((DBMovieInfo)obj);
            ReapplyFilters();
            ReloadFacade();

            if (ContentsChanged != null)
                ContentsChanged();
        }

        // Listens for newly removed items from the database manager.
        private void onMovieDeleted(DatabaseTable obj) {
            // if this is not a movie object, break
            if (obj.GetType() != typeof(DBMovieInfo))
                return;

            // if this item is not in the list exit. This should never really happen though...  
            if (!allMovies.Contains((DBMovieInfo)obj)) {
                logger.Warn("Received multiple \"removed\" messages for " + (DBMovieInfo)obj);
                return;
            }

            // remove movie from master list and filtered list
            logger.Info("Removing " + ((DBMovieInfo)obj).Title + " from list.");
            DBMovieInfo movie = (DBMovieInfo)obj;
            allMovies.Remove(movie);
            filteredMovies.Remove(movie);

            // update the facade to reflect the changes
            ReloadFacade();

            if (ContentsChanged != null)
                ContentsChanged();
        }

        // When a new filter is added to or removed update our listeners and reload the facade
        private void onActiveFiltersChanged(object sender, EventArgs e) {
            foreach (IBrowserFilter currFilter in watchedFilters) 
                currFilter.Updated -= new FilterUpdatedDelegate(onFilterUpdated);

            foreach (IBrowserFilter currFilter in activeFilters)
                currFilter.Updated += new FilterUpdatedDelegate(onFilterUpdated);

            watchedFilters = new DynamicList<IBrowserFilter>();
            watchedFilters.AddRange(activeFilters);
            
            ReapplyFilters();
            ReloadFacade();

            if (ContentsChanged != null)
                ContentsChanged();
        }

        private void onFilterUpdated(IBrowserFilter obj) {
            logger.Debug("OnFilterUpdated: " + obj);
            ReapplyFilters();
            ReloadFacade();

            if (ContentsChanged != null)
                ContentsChanged();
        }

        #endregion

        // these methods should eventually be extracted to a super class allowing
        // for the movie browser to be reused by other GUIs for other HTPC apps.
        #region Facade Management Methods

        // populates the facade with the currently filtered list items
        public void ReloadFacade() {
            if (facade == null)
                return;

            // clear and populate the facade
            if (facade.ListView != null) facade.ListView.Clear();
            if (facade.ThumbnailView != null) facade.ThumbnailView.Clear();
            if (facade.FilmstripView != null) facade.FilmstripView.Clear();

            foreach (DBMovieInfo currMovie in FilteredMovies) 
                addMovieToFacade(currMovie);
            
            // sort it using our basic sorter
            facade.Sort(new GUIListItemMovieComparer(this.CurrentSortField, this.CurrentSortDirection));

            // reapply the current selection
            SyncToFacade();
        }

        // try to set the selected movie in the facade
        public void SyncToFacade() {
            if (facade == null || selectedMovie == null)
                return;

            int? desiredIndex = null;
            for (int i = 0; i < facade.Count; i++) {
                if (facade[i].TVTag == selectedMovie) {
                    desiredIndex = i;
                    break;
                }
            }

            if (desiredIndex != null && desiredIndex != facade.SelectedListItemIndex) 
                facade.SelectedListItemIndex = (int)desiredIndex;
        }

        public void SyncFromFacade() {
            if (facade == null)
                return;

            // if we already are on this movie, exit
            if (selectedMovie == facade.SelectedListItem.TVTag as DBMovieInfo)
                return;

            DBMovieInfo selectedMovieInFacade = facade.SelectedListItem.TVTag as DBMovieInfo;
            if (selectedMovieInFacade != null)
                SelectedMovie = selectedMovieInFacade;
        }

        // adds the given movie to the facade and creates a GUIListItem if neccesary
        private void addMovieToFacade(DBMovieInfo newMovie) {
            if (newMovie == null || facade == null)
                return;

            // if needed, create a new GUIListItem
            if (!listItems.ContainsKey(newMovie)) {
                GUIListItem currItem = new GUIListItem();
                currItem.Label = newMovie.Title;
                currItem.IconImage = newMovie.CoverThumbFullPath.Trim();
                currItem.IconImageBig = newMovie.CoverThumbFullPath.Trim();
                currItem.TVTag = newMovie;
                currItem.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(onFacadeItemSelected);
                listItems[newMovie] = currItem;
            }

            // add the listitem
            facade.Add(listItems[newMovie]);
        }

        // triggered when a selection change was made on the facade
        private void onFacadeItemSelected(GUIListItem item, GUIControl parent) {
            // if this is not a message from the facade, exit
            if (parent != facade && parent != facade.FilmstripView &&
                parent != facade.ThumbnailView && parent != facade.ListView)
                return;

            SyncFromFacade();
        }

        #endregion

    }

    public class GUIListItemMovieComparer : IComparer<GUIListItem> {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// enum of all possible sort fields
        /// </summary>
        public enum SortingFields {
            Title = 1,
            DateAdded = 2,
            Year = 3,
            Certification = 4,
            Language = 5,
            Score = 6,
            Popularity = 7,
            Runtime = 8,
            FilePath = 9
        }

        public enum SortingDirections{
            Ascending,
            Descending
        }

        private SortingFields _sortField;
        private SortingDirections _sortDirection; 

        /// <summary>
        /// Constructor for GUIListItemMovieComparer
        /// </summary>
        /// <param name="sortField">The database field to sort by</param>
        /// <param name="sortDirection">The direction to sort by</param>
        public GUIListItemMovieComparer(SortingFields sortField, SortingDirections sortDirection) {
            _sortField = sortField;
            _sortDirection = sortDirection;
            logger.Info("Sort Field: {0} Sort Direction: {1}", sortField, sortDirection);
        }

        public int Compare(GUIListItem x, GUIListItem y) {
            try {
                
                DBMovieInfo movieX = (DBMovieInfo)x.TVTag;
                DBMovieInfo movieY = (DBMovieInfo)y.TVTag;
                int rtn;

                switch (_sortField) {
                    case SortingFields.DateAdded:
                        rtn = movieX.DateAdded.CompareTo(movieY.DateAdded);
                        break;

                    case SortingFields.Year:
                        rtn = movieX.Year.CompareTo(movieY.Year);
                        break;

                    case SortingFields.Certification:
                        int intX = GetCertificationValue(movieX.Certification);
                        int intY = GetCertificationValue(movieY.Certification);
                        if (intX == 100 && intY == 100)
                            rtn = movieX.Certification.CompareTo(movieY.Certification);
                        else
                            rtn = intX.CompareTo(intY);
                        break;

                    case SortingFields.Language:
                        rtn = movieX.Language.CompareTo(movieY.Language);
                        break;

                    case SortingFields.Score:
                        rtn = movieX.Score.CompareTo(movieY.Score);
                        break;

                    case SortingFields.Popularity:
                        rtn = movieX.Popularity.CompareTo(movieY.Popularity);
                        break;

                    case SortingFields.Runtime:
                        rtn = movieX.Runtime.CompareTo(movieY.Runtime);
                        break;

                    case SortingFields.FilePath:
                        rtn = movieX.LocalMedia[0].FullPath.CompareTo(movieY.LocalMedia[0].FullPath);
                        break;

                    // default to the title field
                    case SortingFields.Title:
                    default:
                        rtn = movieX.SortBy.CompareTo(movieY.SortBy);
                        break;
                }

                

                // if both items are identical, fallback to using the Title
                if (rtn == 0)
                    rtn = movieX.SortBy.CompareTo(movieY.SortBy);

                // if both items are STILL identical, fallback to using the ID
                if (rtn == 0)
                    rtn = movieX.ID.GetValueOrDefault(0).CompareTo(movieY.ID.GetValueOrDefault(0));

                if (_sortDirection == SortingDirections.Descending)
                    rtn = -rtn; 

                return rtn;
            }
            catch {
                return 0;
            }
        }


        private int GetCertificationValue(string certification) {
            switch (certification) {
                case "G":
                    return 1;
                case "PG":
                    return 2;
                case "PG-13":
                    return 3;
                case "R":
                    return 4;
                case "NC-17":
                    return 5;
                default:
                    return 100;
            }
        }


        public static string GetFriendlySortName(SortingFields field) {

            switch (field) {
                case SortingFields.Title:
                    return Translation.Title;
                case SortingFields.DateAdded:
                    return Translation.DateAdded;
                case SortingFields.Year:
                    return Translation.Year;
                case SortingFields.Certification:
                    return Translation.Certification;
                case SortingFields.Language:
                    return Translation.Language;
                case SortingFields.Score:
                    return Translation.Score;
                case SortingFields.Popularity:
                    return Translation.Popularity;
                case SortingFields.Runtime:
                    return Translation.Runtime;
                case SortingFields.FilePath:
                    return Translation.FilePath;
                default:
                    return "";
            }
        }
    }
}
