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
        private FilterUpdatedDelegate<DBMovieInfo> filterUpdatedDelegate;

        bool updatingFiltering = false;

        #region Properties

        // The currently selected movie.
        public DBMovieInfo SelectedMovie {
            get {
                return selectedMovie;
            }
            set {
                if (selectedMovie != value) {
                    updateSelectedMovie(value);
                    SyncToFacade();
                }
            }
        }
        private DBMovieInfo selectedMovie = null;

        // The currently selected facade index
        public int SelectedIndex {
            get {
                return selectedIndex;
            }
            set {
                selectedIndex = value;
            }
        } private int selectedIndex = 0;

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
                ReloadFacade();
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
                string defaultView = MovingPicturesCore.Settings.DefaultView.Trim().ToLower();
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
        public ICollection<DBMovieInfo> FilteredMovies {
            get {
                if (filteredMovies == null)
                    ReapplyFilters();
                return filteredMovies; 
            }
        }
        HashSet<DBMovieInfo> filteredMovies = null;

        /// <summary>
        /// All filters that are active on the movie browser.
        /// </summary>
        public DynamicList<IFilter<DBMovieInfo>> Filters {
            get {
                if (activeFilters == null) {
                    activeFilters = new DynamicList<IFilter<DBMovieInfo>>();
                    activeFilters.Changed += new ChangedEventHandler(onFiltersChanged);

                    watchedFilters = new DynamicList<IFilter<DBMovieInfo>>();

                }
                return activeFilters; 
            }
        }
        private DynamicList<IFilter<DBMovieInfo>> activeFilters = null;
        private List<IFilter<DBMovieInfo>> watchedFilters = null;

        public DBNode<DBMovieInfo> FilterNode {
            get { return _filterNode; }
            set {
                updatingFiltering = true;

                if (_filterNode != null) removeFilters(_filterNode);
                if (value != null) addFilters(value);

                _filterNode = value;
                
                updatingFiltering = false;
                onFiltersChanged(null, null);
            }
        } private DBNode<DBMovieInfo> _filterNode;

        public SortingFields CurrentSortField { get; set; }
        public SortingDirections CurrentSortDirection { get; set; }


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
            filterUpdatedDelegate = new FilterUpdatedDelegate<DBMovieInfo>(onFilterUpdated);

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

                // Sync
                case BrowserViewMode.FILMSTRIP:
                case BrowserViewMode.LIST:
                case BrowserViewMode.SMALLICON:
                case BrowserViewMode.LARGEICON:
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
            filteredMovies = new HashSet<DBMovieInfo>();
            filteredMovies.Clear();

            // trim it down to satisfy the filters.
            bool first = true;
            foreach (IFilter<DBMovieInfo> currFilter in Filters) {
                if (first)
                    filteredMovies = currFilter.Filter(allMovies);
                else 
                    filteredMovies = currFilter.Filter(filteredMovies);

                first = false;
            }
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
            
            onContentsChanged();
        }

        // Sets the initial settings for how movies should be sorted on launch.
        private void initSortingDefaults() {
            // set default sort method
            string defaultSortField = MovingPicturesCore.Settings.DefaultSortField.Trim();

            try {
                CurrentSortField = (SortingFields)Enum.Parse(typeof(SortingFields), defaultSortField, true);
            }
            catch {
                logger.Error("Invalid Sort Field provided: {0}.  Defaulting to Title", defaultSortField);
                CurrentSortField = SortingFields.Title;
            }

            CurrentSortDirection = Sort.GetLastSortDirection(CurrentSortField);
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
            // a full update of the facade is necessary because of the possibility
            // that the new item should be filtered out by the ActiveFilters
            logger.Info("Adding " + ((DBMovieInfo)obj).Title + " to movie browser.");
            allMovies.Add((DBMovieInfo)obj);
            ReapplyFilters();
            ReloadFacade();
            onContentsChanged();
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
            onContentsChanged();
        }

        private void removeFilters(DBNode<DBMovieInfo> node) {
            foreach (IFilter<DBMovieInfo> currFilter in node.GetAllFilters())
                if (Filters.Contains(currFilter))
                    Filters.Remove(currFilter);
        }

        private void addFilters(DBNode<DBMovieInfo> node) {
            foreach (IFilter<DBMovieInfo> currFilter in node.GetAllFilters())
                if (!Filters.Contains(currFilter))
                    Filters.Add(currFilter);
        }

        // When a new filter is added to or removed update our listeners and reload the facade
        private void onFiltersChanged(object sender, EventArgs e) {
            if (updatingFiltering)
                return;

            foreach (IFilter<DBMovieInfo> currFilter in watchedFilters)
                currFilter.Updated -= filterUpdatedDelegate;

            foreach (IFilter<DBMovieInfo> currFilter in activeFilters)
                currFilter.Updated += filterUpdatedDelegate;

            watchedFilters = new DynamicList<IFilter<DBMovieInfo>>();
            watchedFilters.AddRange(activeFilters);
            
            ReapplyFilters();
            ReloadFacade();
            onContentsChanged();
        }

        private void onFilterUpdated(IFilter<DBMovieInfo> obj) {
            logger.Debug("OnFilterUpdated: " + obj);
            ReapplyFilters();
            ReloadFacade();

            // in case the current movie is no longer in the list
            SyncFromFacade();
            onContentsChanged();
        }

        private void onContentsChanged() {
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

            if (MovingPicturesCore.Settings.AllowGrouping && CurrentView == BrowserViewMode.LIST) {
                GroupHeaders.AddGroupHeaders(this);
            }

            // reapply the current selection
            SyncToFacade();
        }

        /// <summary>
        /// Updates the selected index on the facade linked to the selected movie
        /// </summary>
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

            int setIndex = selectedIndex;
            if (desiredIndex != null && desiredIndex != selectedIndex)
                setIndex = (int)desiredIndex;

            facade.SelectedListItemIndex = setIndex;
            logger.Debug("SyncToFacade() SelectedIndex={0}", setIndex);

            // if we are in the filmstrip view also send a message
            if (CurrentView == BrowserViewMode.FILMSTRIP) {
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, facade.WindowId, 0, facade.FilmstripView.GetID, setIndex, 0, null);
                GUIGraphicsContext.SendMessage(msg);
                logger.Debug("Sending a selection postcard to FilmStrip.");
            }

           
        }

        /// <summary>
        /// Updates the selected movie linked to the selected index on the facade
        /// </summary>
        public void SyncFromFacade() {
            if (facade == null)
                return;

            // if nothing is selected in the facade, exit
            if (facade.SelectedListItem == null)                
                return;

            selectedIndex = facade.SelectedListItemIndex;
            logger.Debug("SyncFromFacade() SelectedIndex={0}", selectedIndex);

            DBMovieInfo selectedMovieInFacade = facade.SelectedListItem.TVTag as DBMovieInfo;            
            if (selectedMovie != selectedMovieInFacade)
                updateSelectedMovie(selectedMovieInFacade);
        }

        // triggered when a movie was selected on the facade
        public void onMovieItemSelected(GUIListItem item, GUIControl parent) {
            // if this is not a message from the facade, exit
            if (parent != facade && parent != facade.FilmstripView && parent != facade.ThumbnailView &&
                parent != facade.ListView) return;

            SyncFromFacade();
        }

        /// <summary>
        /// Updates the selected movie
        /// </summary>
        /// <param name="movie"></param>
        private void updateSelectedMovie(DBMovieInfo movie) {
            selectedMovie = movie;

            // log the change
            if (selectedMovie != null)
                logger.Debug("SelectedMovie changed: " + selectedMovie.Title);
            else
                logger.Debug("SelectedMovie changed: null");

            // notify any listeners
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
                currItem.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(onMovieItemSelected);

                listItems[newMovie] = currItem;
            }

            // add the listitem
            facade.Add(listItems[newMovie]);
            UpdateListColors(newMovie);
        }

        public void UpdateListColors(DBMovieInfo movie) {
            if (!listItems.ContainsKey(movie)) return;

            GUIListItem currItem = listItems[movie];
            currItem.IsRemote = false;
            currItem.IsPlayed = false;

            if (!movie.LocalMedia[0].IsAvailable) {
                // remoteColor
                currItem.IsRemote = true;
            }
            else if (movie.ActiveUserSettings.WatchedCount > 0) {
                // playedColor
                currItem.IsPlayed = true;
            }            
        }

        /// <summary>
        /// Navigates the selected movie to the first movie in the list.
        /// </summary>
        public void JumpToBeginningOfList() {
            if (CurrentView == BrowserViewMode.LIST) {
                // go to the first item that is not a group header
                for (int i = 0; i < Facade.ListView.ListItems.Count; i++) {
                    if (Facade.ListView.ListItems[i].TVTag != null) {
                        Facade.SelectedListItemIndex = i;
                        break;
                    }
                }
            }
            else
                Facade.SelectedListItemIndex = 0;
        }
        #endregion

    }
}
