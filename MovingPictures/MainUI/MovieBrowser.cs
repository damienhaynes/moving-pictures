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
    public enum BrowserViewMode { LIST, SMALLICON, LARGEICON, FILMSTRIP, DETAILS, CATEGORIES }
    
    public class MovieBrowser {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // lookup for GUIListItems that have been created for DBMovieInfo objects
        private Dictionary<DatabaseTable, GUIListItem> listItems;
        
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
                    // log the change
                    if (value != null)
                        logger.Debug("SelectedMovie changed: " + value.Title);
                    else {
                        selectedIndex = 0;
                        logger.Debug("SelectedMovie changed: null");
                    }

                    selectedMovie = value;
                    SyncToFacade();

                    // notify any listeners
                    if (SelectionChanged != null)
                        SelectionChanged(selectedMovie);

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
                // if the skin doesnt support the categories facade, dont set it
                if (value == BrowserViewMode.CATEGORIES && _categoriesFacade == null)
                    return;

                // update the state variables
                if (currentView != value)
                    previousView = currentView;

                logger.Debug("CurrentView changed: {0}", value);
                currentView = value;

                // if we are in the category view reload the category facade
                if (currentView == BrowserViewMode.CATEGORIES)
                    ReloadCategoriesFacade();
                else // otherwise reload the movie facade                    
                    ReloadMovieFacade();
                

                ReapplyView();
               
            }
        } private BrowserViewMode currentView = BrowserViewMode.CATEGORIES; // starting default is CATEGORIES

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
        /// The facade control linked to this movie browser used for movie content.
        /// </summary>
        public GUIFacadeControl Facade {
            get { return facade; }
            set { facade = value;}
        }
        public GUIFacadeControl facade;

        /// <summary>
        /// The facade control linked to this movie browser used for categories.
        /// </summary>
        public GUIFacadeControl CategoriesFacade {
            get { return _categoriesFacade; }
            set { _categoriesFacade = value;  }
        }
        public GUIFacadeControl _categoriesFacade;

        /// <summary>
        /// Returns true if the skin supports categories and a categories menu has been defined.
        /// </summary>
        public bool CategoriesAvailable {
            get {
                return _categoriesFacade != null && 
                       MovingPicturesCore.Settings.CategoriesEnabled && 
                       CategoriesMenu.RootNodes.Count > 0;
            }
        }

        /// <summary>
        /// Returns the internal Categories Menu database object.
        /// </summary>
        public DBMenu<DBMovieInfo> CategoriesMenu {
            get { return MovingPicturesCore.Settings.CategoriesMenu; }
        }

        /// <summary>
        /// Returns the current Categories Menu Node. The children of this node should be displayed
        /// to the user. If at the root of the categories menu, this will return null. 
        /// </summary>
        public DBNode<DBMovieInfo> CurrentNode {
            set {
                // set correct sort for this node
                if (value != null) {
                    DBMovieNodeSettings nodeSettings = (DBMovieNodeSettings)value.AdditionalSettings;
                    if (!nodeSettings.UseDefaultSorting) {
                        CurrentSortField = nodeSettings.SortField;
                        CurrentSortDirection = nodeSettings.SortDirection;
                    }
                    else {
                        CurrentSortField = DefaultSortField;
                        CurrentSortDirection = Sort.GetLastSortDirection(DefaultSortField);
                    }
                }

                updatingFiltering = true;

                // if required reset the selectedMovie
                if (MovingPicturesCore.Settings.ResetSelectedMovieWhenSwitchingCategories) {
                    selectedMovie = null;
                }

                // remove previous node filter
                if (_currentNode != null && _currentNode.Filter != null)
                    Filters.Remove(_currentNode.Filter);

                // add current node filter
                if (value != null && value.Filter != null) 
                    Filters.Add(value.Filter);

                updatingFiltering = false;
                ReapplyFilters();

                // if we are moving to the parent category set the current node
                // as the selected node before we reload the facade
                if (_currentNode != null && value == _currentNode.Parent)
                    _selectedNode = _currentNode;

                _currentNode = value;
                ReloadCategoriesFacade();
            }
            get { return _currentNode; }
        } public DBNode<DBMovieInfo> _currentNode;

        /// <summary>
        /// Returns the Categories Menu Node currently highlighted in the browser. This is a child of the
        /// CurrentNode. If no node is highlighted (for example if a movie is highlighted instead) this
        /// will return null.
        /// </summary>
        public DBNode<DBMovieInfo> SelectedNode {
            get { return _selectedNode; }
            set {
                if (_selectedNode != value) {
                    // log the change
                    if (value != null)
                        logger.Debug("SelectedNode changed: " + value.Name);
                    else {
                        logger.Debug("SelectedNode changed: null");
                    }

                    _selectedNode = value;

                    // notify any listeners
                    if (SelectionChanged != null)
                        SelectionChanged(selectedMovie);
                }
            }
        } public DBNode<DBMovieInfo> _selectedNode;

        public IList<DBNode<DBMovieInfo>> SubNodes {
            get {
                if (CurrentNode != null)
                    return CurrentNode.Children;
                else if (CategoriesMenu != null)
                    return CategoriesMenu.RootNodes;
                else return null;
            }
        }

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

        public SortingFields DefaultSortField { get; set; }
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

            listItems = new Dictionary<DatabaseTable, GUIListItem>();
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
            logger.Debug("ReapplyView: {0}", currentView.ToString());

            if (facade == null)
                return;

            if (currentView != BrowserViewMode.CATEGORIES && _categoriesFacade != null) {
                _categoriesFacade.Visible = false;
                if (_categoriesFacade.ListView != null) _categoriesFacade.ListView.Visible = false;
                if (_categoriesFacade.ThumbnailView != null) _categoriesFacade.ThumbnailView.Visible = false;
                if (_categoriesFacade.FilmstripView != null) _categoriesFacade.FilmstripView.Visible = false;
            }

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
                case BrowserViewMode.CATEGORIES:
                    logger.Debug("Categories Facade Visible");
                    if (_clearFocusAction != null) _clearFocusAction();

                    facade.Visible = false;
                    facade.ListView.Visible = false;
                    facade.ThumbnailView.Visible = false;
                    facade.FilmstripView.Visible = false;
                    
                    _categoriesFacade.Focus = true;
                    _categoriesFacade.Visible = true;
                    if (_categoriesFacade.ListView != null) _categoriesFacade.ListView.Visible = true;
                    if (_categoriesFacade.ThumbnailView != null) _categoriesFacade.ThumbnailView.Visible = true;
                    if (_categoriesFacade.FilmstripView != null) _categoriesFacade.FilmstripView.Visible = true;
                    break;
            }

            // if we are leaving details view, set focus back on the facade
            if (previousView == BrowserViewMode.DETAILS || previousView == BrowserViewMode.CATEGORIES) {
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
                DefaultSortField = (SortingFields)Enum.Parse(typeof(SortingFields), defaultSortField, true);
            }
            catch {
                logger.Error("Invalid Sort Field provided: {0}.  Defaulting to Title", defaultSortField);
                DefaultSortField = SortingFields.Title;
            }

            CurrentSortField = DefaultSortField;
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
            ReloadMovieFacade();
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
            ReloadMovieFacade();
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
            ReloadMovieFacade();
            onContentsChanged();
        }

        private void onFilterUpdated(IFilter<DBMovieInfo> obj) {
            logger.Debug("OnFilterUpdated: " + obj);
            ReapplyFilters();
            ReloadMovieFacade();

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

        public void ReloadCategoriesFacade() {
            if (!CategoriesAvailable || SubNodes.Count == 0)
                return;

            CategoriesFacade.Clear();
            if (CategoriesFacade.ListView != null) CategoriesFacade.ListView.Clear();

            foreach (DBNode<DBMovieInfo> currNode in SubNodes) {
                //logger.Debug("add category node: " + currNode.Name);
                addCategoryNodeToFacade(currNode);
            }

            int? desiredIndex = null;
            if (_selectedNode != null) {
                // Find the selected category
                for (int i = 0; i < _categoriesFacade.Count; i++) {
                    if (_categoriesFacade[i].TVTag == _selectedNode) {
                        // we found the selected category so we break the loop
                        desiredIndex = i;
                        break;
                    }
                }
            }

            // if no index was found pick the first selection
            int newIndex = 0;
            if (desiredIndex == null)
                SelectedNode = _categoriesFacade[newIndex].TVTag as DBNode<DBMovieInfo>;
            else
                newIndex = (int)desiredIndex;

            // set the required index in the facade
            if (_categoriesFacade.SelectedListItemIndex != newIndex)
                _categoriesFacade.SelectedListItemIndex = newIndex;
        }

        // populates the facade with the currently filtered list items
        public void ReloadMovieFacade() {
            if (facade == null)
                return;

            // clear and populate the facade
            facade.Clear();
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
            if (facade == null || facade.Count == 0)
                return;

            int? desiredIndex = null;
            for (int i = 0; i < facade.Count; i++) {
                // set the desired index to the first movie (not a group header)
                if (desiredIndex == null && facade[i].TVTag != null) {
                    desiredIndex = i;
                    // if we found the first movie and we don't have 
                    // a movie selected we set the selected movie and return
                    if (selectedMovie == null) {
                        SelectedMovie = facade[i].TVTag as DBMovieInfo;
                        return;
                    }
                }

                // otherwise look for the correct movie
                if (selectedMovie != null && facade[i].TVTag == selectedMovie) {  
                    // we found the selected movie so we break the loop
                    desiredIndex = i;
                    break;
                }

                // if we didn't found the movie when we are at the end of the loop
                // we select the first movie using the desired index
                if (selectedMovie != null && (i == facade.Count-1)) {
                    SelectedMovie = facade[(int)desiredIndex].TVTag as DBMovieInfo;
                    return;
                }
            }


            // if we found a new index update the selected Index
            if (desiredIndex != null && desiredIndex != selectedIndex) { 
                selectedIndex = (int)desiredIndex;
             
                logger.Debug("SyncToFacade() SelectedIndex={0}", selectedIndex);
            }

            // set the index in the facade
            if (facade.SelectedListItemIndex != selectedIndex) {
                facade.SelectedListItemIndex = selectedIndex;

                // if we are in the filmstrip view also send a message
                if (CurrentView == BrowserViewMode.FILMSTRIP) {
                    GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, facade.WindowId, 0, facade.FilmstripView.GetID, selectedIndex, 0, null);
                    GUIGraphicsContext.SendMessage(msg);
                    logger.Debug("Sending a selection postcard to FilmStrip.");
                }
            }
           
        }

        /// <summary>
        /// Updates the selected movie linked to the selected index on the facade
        /// </summary>
        public void SyncFromFacade() {
            // if nothing is selected in the facade,
            if (facade == null || facade.SelectedListItem == null)
                return;

            // if the selected index has changed update and log
            if (selectedIndex != facade.SelectedListItemIndex) {
                selectedIndex = facade.SelectedListItemIndex;
                logger.Debug("SyncFromFacade() SelectedIndex={0}", selectedIndex);
            } 
            
            // update the selected movie object 
            DBMovieInfo selectedMovieInFacade = facade.SelectedListItem.TVTag as DBMovieInfo;
            SelectedMovie = facade.SelectedListItem.TVTag as DBMovieInfo;               
        }

        // triggered when a movie was selected on the facade
        public void onMovieItemSelected(GUIListItem item, GUIControl parent) {
            // if this is not a message from the facade, exit
            if (parent != facade && parent != facade.FilmstripView && parent != facade.ThumbnailView &&
                parent != facade.ListView) return;

            SyncFromFacade();
        }

        public void onCategoryNodeSelected(GUIListItem item, GUIControl parent) {
            // if this is not a message from the facade, exit
            if (parent != _categoriesFacade && parent != _categoriesFacade.ListView) return;
            SelectedNode = (DBNode<DBMovieInfo>)item.TVTag;
        }

        // adds the given movie to the facade and creates a GUIListItem if neccesary
        private void addCategoryNodeToFacade(DBNode<DBMovieInfo> newNode) {
            if (newNode == null || _categoriesFacade == null)
                return;

            // if needed, create a new GUIListItem
            if (!listItems.ContainsKey(newNode)) {
                GUIListItem currItem = new GUIListItem();
                currItem.Label = newNode.Name;
                //currItem.IconImage = newMovie.CoverThumbFullPath.Trim();
                //currItem.IconImageBig = newMovie.CoverThumbFullPath.Trim();
                currItem.TVTag = newNode;
                currItem.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(onCategoryNodeSelected);

                listItems[newNode] = currItem;
            }

            // add the listitem
            _categoriesFacade.Add(listItems[newNode]);
            //UpdateListColors(newMovie);
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

        /// <summary>
        /// Updates the color properties of the GUIListItem object for this movie
        /// </summary>
        /// <param name="movie"></param>
        public void UpdateListColors(DBMovieInfo movie) {
            if (!listItems.ContainsKey(movie))
                return;
            
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
        /// Triggers RefreshCoverArt() on the GUIListItem for this movie
        /// </summary>         
        /// <param name="movie"></param>
        public void RefreshArtwork(DBMovieInfo movie) {
            if (!listItems.ContainsKey(movie))
                return;
             
            // Refresh the list item object              
            listItems[movie].RefreshCoverArt();
        }          

        #endregion

    }
}
