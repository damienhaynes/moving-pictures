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

namespace MediaPortal.Plugins.MovingPictures.MainUI.MovieBrowser {
    public class MovieBrowser {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // lookup for GUIListItems that have been created for DBMovieInfo objects
        private Dictionary<DBMovieInfo, GUIListItem> listItems;

        #region Properties

        // all movies available in the movie browser, including those not currently
        // displayed due to active filters
        public ReadOnlyCollection<DBMovieInfo> AllMovies {
            get { return allMovies.AsReadOnly(); }
        }
        private List<DBMovieInfo> allMovies;

        // All movies currently visible in the mnovie browser.
        public ReadOnlyCollection<DBMovieInfo> FilteredMovies {
            get {
                if (filteredMovies == null)
                    ReapplyFilters();
                return filteredMovies.AsReadOnly(); 
            }
        }
        List<DBMovieInfo> filteredMovies = null;

        // all filters that are active on the movie browser
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


        // the facade linked to this movie browser
        public GUIFacadeControl Facade {
            get { return facade; }

            set {
                facade = value;
                ReloadFacade();
            }
        }
        public GUIFacadeControl facade;

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
                if (facade != null) facade.SelectedListItemIndex = facadeIndexOf(selectedMovie);
            }
        }
        private DBMovieInfo selectedMovie = null;

        public GUIListItemMovieComparer.SortingFields SortField { get; set; }
        public GUIListItemMovieComparer.SortingDirections SortDirection { get; set; }

        #endregion

        #region Events

        public delegate void ContentsChangedDelegate();
        public delegate void SelectionChangedDelegate(DBMovieInfo obj);
        
        public event SelectionChangedDelegate SelectionChanged;
        public event ContentsChangedDelegate ContentsChanged;
        
        #endregion

        #region Core MovieBrowser Methods

        public MovieBrowser() {
            // setup listeners for new or removed movies
            MovingPicturesCore.DatabaseManager.ObjectDeleted += new DatabaseManager.ObjectAffectedDelegate(onMovieDeleted);
            MovingPicturesCore.DatabaseManager.ObjectInserted += new DatabaseManager.ObjectAffectedDelegate(onMovieAdded);

            listItems = new Dictionary<DBMovieInfo, GUIListItem>();

            loadMovies();
            initSortingDefaults();
        }

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
                SortField = (GUIListItemMovieComparer.SortingFields)Enum.Parse(typeof(GUIListItemMovieComparer.SortingFields), defaultSortField, true);
            }
            catch {
                logger.Error("Invalid Sort Field provided: {0}.  Defaulting to Title", defaultSortField);
                SortField = GUIListItemMovieComparer.SortingFields.Title;
            }

            try {
                SortDirection = (GUIListItemMovieComparer.SortingDirections)Enum.Parse(typeof(GUIListItemMovieComparer.SortingDirections), defaultSortDirection, true);
            }
            catch {
                logger.Error("Invalid Sort Direction provided: {0}.  Defaulting to ascending", defaultSortDirection);
                SortDirection = GUIListItemMovieComparer.SortingDirections.Ascending;
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
            facade.Sort(new GUIListItemMovieComparer(this.SortField, this.SortDirection));

            // reapply the current selection
            facade.SelectedListItemIndex = facadeIndexOf(SelectedMovie);
        }

        public void Sync() {
            // if we already are on this movie, exit
            if (selectedMovie == facade.SelectedListItem.TVTag as DBMovieInfo)
                return;

            selectedMovie = facade.SelectedListItem.TVTag as DBMovieInfo;

            // log and notify any listeners
            logger.Debug("SelectedMovie changed: " + selectedMovie.Title);
            if (SelectionChanged != null)
                SelectionChanged(selectedMovie);
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

            Sync();
        }

        // returns the index of the given item in the facade. If it doesn't exist 
        // (maybe filtered out?) then returns 0, the first index
        private int facadeIndexOf(DBMovieInfo movie) {
            for (int i = 0; i < facade.Count; i++) {
                if (facade[i].TVTag == movie)
                    return i;
            }

            return 0;
        }

        #endregion

    }

    public class GUIListItemMovieComparer : IComparer<GUIListItem> {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// enum of all possible sort fields
        /// </summary>
        public enum SortingFields {
            Title,
            DateAdded,
            Year,
            Certification,
            Language,
            Score,
            Popularity,
            Runtime,
            FilePath
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
                        rtn = movieX.ID.GetValueOrDefault(0).CompareTo(movieY.ID.GetValueOrDefault(0));
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
    }
}
