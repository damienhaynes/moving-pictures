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
                    reapplyFilters();
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

        #endregion

        #region Events

        public delegate void SelectionChangedDelegate(DBMovieInfo obj);
        public event SelectionChangedDelegate SelectionChanged;
        
        #endregion

        #region Core MovieBrowser Methods

        public MovieBrowser() {
            // setup listeners for new or removed movies
            MovingPicturesCore.DatabaseManager.ObjectDeleted += new DatabaseManager.ObjectAffectedDelegate(onMovieDeleted);
            MovingPicturesCore.DatabaseManager.ObjectInserted += new DatabaseManager.ObjectAffectedDelegate(onMovieAdded);

            listItems = new Dictionary<DBMovieInfo, GUIListItem>();

            loadMovies();
        }

        // An initial load of all movies in the database.
        private void loadMovies() {
            // clear the list
            allMovies = new List<DBMovieInfo>();
            
            // populate the list
            List<DBMovieInfo> movies = DBMovieInfo.GetAll();
            foreach (DBMovieInfo currMovie in movies)
                allMovies.Add(currMovie);

            if (FilteredMovies.Count > 0)
                SelectedMovie = FilteredMovies[0];
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
            reapplyFilters();
            ReloadFacade();
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
        }

        // When a new filter is added to or removed update our listeners and reload the facade
        private void onActiveFiltersChanged(object sender, EventArgs e) {
            foreach (IBrowserFilter currFilter in watchedFilters) 
                currFilter.Updated -= new FilterUpdatedDelegate(onFilterUpdated);

            foreach (IBrowserFilter currFilter in activeFilters)
                currFilter.Updated += new FilterUpdatedDelegate(onFilterUpdated);

            watchedFilters = new DynamicList<IBrowserFilter>();
            watchedFilters.AddRange(activeFilters);
            
            reapplyFilters();
            ReloadFacade();
        }

        private void onFilterUpdated(IBrowserFilter obj) {
            logger.Debug("OnFilterUpdated: " + obj);
            reapplyFilters();
            ReloadFacade();
        }

        private void reapplyFilters() {
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
            facade.Sort(new GUIListItemMovieComparer());

            // reapply the current selection
            facade.SelectedListItemIndex = facadeIndexOf(SelectedMovie);
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

            // if we already are on this movie, exit
            if (SelectedMovie == item.TVTag)
                return;

            selectedMovie = item.TVTag as DBMovieInfo;

            // log and notify any listeners
            logger.Debug("SelectedMovie changed: " + ((DBMovieInfo)item.TVTag).Title);
            if (SelectionChanged != null)
                SelectionChanged(selectedMovie);
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
        public int Compare(GUIListItem x, GUIListItem y) {
            try {
                DBMovieInfo movieX = (DBMovieInfo)x.TVTag;
                DBMovieInfo movieY = (DBMovieInfo)y.TVTag;

                int rtn = movieX.SortBy.CompareTo(movieY.SortBy);
                if (rtn == 0)
                    rtn = movieX.ID.GetValueOrDefault(0).CompareTo(movieY.ID.GetValueOrDefault(0));

                return rtn;
            }
            catch {
                return 0;
            }
        }
    }
}
