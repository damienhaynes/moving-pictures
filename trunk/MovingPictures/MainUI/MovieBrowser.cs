using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;
using Cornerstone.Database.Tables;
using Cornerstone.Database;
using Cornerstone.MP.Extensions;
using System.Collections.ObjectModel;
using Cornerstone.Database.CustomTypes;
using Cornerstone.Collections;
using MediaPortal.Plugins.MovingPictures.MainUI.Filters;

namespace MediaPortal.Plugins.MovingPictures.MainUI {
    public enum BrowserViewMode { INIT, LIST, SMALLICON, LARGEICON, FILMSTRIP, DETAILS, CATEGORIES }
    
    public class MovieBrowser {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // lookup for GUIListItems that have been created for DBMovieInfo objects
        private Dictionary<DatabaseTable, GUIListItem> listItems;
        private Dictionary<DBNode<DBMovieInfo>, HashSet<DBMovieInfo>> availableMovies;
        private Dictionary<DBNode<DBMovieInfo>, HashSet<DBMovieInfo>> possibleMovies;

        private MovingPicturesSkinSettings skinSettings;
        private FilterUpdatedDelegate<DBMovieInfo> filterUpdatedDelegate;

        bool updatingFiltering = false;
        bool refreshFacade = false;
        DateTime refreshToday = DateTime.Today;
        Timer refreshFacadeTimer;
        private readonly object syncRefresh = new object();

        #region Properties

        /// <summary>
        /// Gets an object that can be used to synchronize access to the movie browser
        /// </summary>
        public object SyncRoot {
            get {
                return syncRoot;
            }
        } private readonly object syncRoot = new object();

        public bool AutoRefresh { get; set; }

        // The currently selected movie.
        public DBMovieInfo SelectedMovie {
            get {
                return selectedMovie;
            }
            set {
                lock (SyncRoot) {
                    
                    // do nothing when the movie is already the selected one
                    if (selectedMovie == value) {
                        return;
                    }
                    
                    // set the selected movie
                    selectedMovie = value;

                    // resets the selected movie to the first movie in the list
                    if (selectedMovie == null) {
                        selectedMovie = facade.SyncToFacade<DBMovieInfo>(null, out selectedIndex);
                    }
                  
                }

                // log the change
                if (selectedMovie != null) {
                    logger.Debug("SelectedMovie changed: " + selectedMovie.Title);
                }
                else {
                    logger.Debug("SelectedMovie changed: NULL");
                }

                // notify any listeners
                if (MovieSelectionChanged != null)
                    MovieSelectionChanged(selectedMovie);
            }
        }
        private DBMovieInfo selectedMovie = null;

        // The currently selected facade index
        public int SelectedIndex {
            get {
                return selectedIndex;
            }
            set {
                if (Facade.Count > 1) {
                    if (value < Facade.Count)
                        Facade.SelectIndex(value);
                    else
                        Facade.SelectIndex(Facade.Count - 1);
                }
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
                
                // Set view and reload it
                currentView = value;
                ReloadView();
               
            }
        } private BrowserViewMode currentView; 

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
                // disable filter events
                updatingFiltering = true;

                if (_currentNode != null) {

                    // remove filters of the current node
                    removeFilters(_currentNode);

                    // if we are moving to the parent category set the current node
                    // as the selected node before we reload the facade
                    if (value == _currentNode.Parent) {
                        _selectedNode = _currentNode;

                        // if required reset the selectedMovie
                        if (MovingPicturesCore.Settings.ResetSelectedMovieWhenSwitchingCategories) {
                            selectedMovie = null;
                        }
                    }
                }

                // set the new node as current node
                _currentNode = value;

                // prepare settings for the new node
                if (_currentNode != null) {
                    // add current node filters
                    addFilters(value);

                    // setup the sorting
                    if (_currentNode != null) {
                        DBMovieNodeSettings nodeSettings = (DBMovieNodeSettings)_currentNode.AdditionalSettings;
                        if (!nodeSettings.UseDefaultSorting) {
                            // using category settings
                            CurrentSortField = nodeSettings.SortField;
                            CurrentSortDirection = nodeSettings.SortDirection;
                        }
                        else {
                            // using default settings
                            CurrentSortField = DefaultSortField;
                            CurrentSortDirection = Sort.GetLastSortDirection(CurrentSortField);
                        }
                    }

                }

                // enable filter events and reapply all filters
                updatingFiltering = false;
                ReapplyFilters();                

                // evaluate where we are
                if ( SubNodes == null || SubNodes.Count == 0) {
                    // we don't have any sub categories to show so we switch to 
                    // the default view if we are not in that view
                    if (CurrentView != DefaultView)
                        CurrentView = DefaultView;
                }
                else if (CurrentView != BrowserViewMode.CATEGORIES) {
                    // we have sub categories to show so switch to 
                    // category view if we are not in that view
                    CurrentView = BrowserViewMode.CATEGORIES;
                }
                else {
                    // we are already in the correct view so 
                    // we only have to reload the facade
                    ReloadFacade();
                }

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
                // log the change
                if (value != null)
                    logger.Debug("SelectedNode changed: " + value.Name);
                else {
                    logger.Debug("SelectedNode changed: null");
                }

                _selectedNode = value;

                // notify any listeners
                if (NodeSelectionChanged != null)
                    NodeSelectionChanged(_selectedNode);
            }
        } public DBNode<DBMovieInfo> _selectedNode;

        public List<DBNode<DBMovieInfo>> SubNodes {
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
                lock (syncRefresh) {
                    if (filteredMovies == null)
                        ReapplyFilters();
                }

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
        public delegate void MovieSelectionChangedDelegate(DBMovieInfo obj);
        public delegate void NodeSelectionChangedDelegate(DBNode<DBMovieInfo> obj);
        public delegate void ViewChangedDelegate(BrowserViewMode previousView, BrowserViewMode currentView);
        
        public event MovieSelectionChangedDelegate MovieSelectionChanged;
        public event NodeSelectionChangedDelegate NodeSelectionChanged;
        public event ContentsChangedDelegate ContentsChanged;
        public event ViewChangedDelegate ViewChanged;
        
        #endregion

        #region Public Methods

        public MovieBrowser(MovingPicturesSkinSettings skinSettings) {
            this.skinSettings = skinSettings;

            AutoRefresh = false;

            // setup listeners for new or removed movies
            MovingPicturesCore.DatabaseManager.ObjectDeleted += new DatabaseManager.ObjectAffectedDelegate(onMovieDeleted);
            MovingPicturesCore.DatabaseManager.ObjectInserted += new DatabaseManager.ObjectAffectedDelegate(onMovieAdded);
            MovingPicturesCore.DatabaseManager.ObjectUpdated += new DatabaseManager.ObjectAffectedDelegate(onMovieUpdated);

            listItems = new Dictionary<DatabaseTable, GUIListItem>();
            availableMovies = new Dictionary<DBNode<DBMovieInfo>, HashSet<DBMovieInfo>>();
            possibleMovies = new Dictionary<DBNode<DBMovieInfo>, HashSet<DBMovieInfo>>();

            filterUpdatedDelegate = new FilterUpdatedDelegate<DBMovieInfo>(onFilterUpdated);

            // load all movies once
            allMovies = DBMovieInfo.GetAll();
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
        /// Reloads the current view to the GUI.
        /// </summary>
        public void ReloadView() {
            logger.Debug("CurrentView: {0}", currentView.ToString());

            // Clear focus
            if (_clearFocusAction != null) _clearFocusAction();

            // Facade visibility
            if (currentView != BrowserViewMode.CATEGORIES && _categoriesFacade != null) {
                _categoriesFacade.Focus(false);
                _categoriesFacade.Visible(false);
            }
            
            if (currentView == BrowserViewMode.CATEGORIES || currentView == BrowserViewMode.DETAILS) {
                facade.Focus(false);
                facade.Visible(false);
            }

            // Switch facade view
            switch (currentView) {
                case BrowserViewMode.CATEGORIES:
                    _categoriesFacade.View = GUIFacadeControl.ViewMode.List;
                    break;
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
            }

            // Reload facade content
            ReloadFacade();

            // Set facade visibility and focus 
            if (CurrentView == BrowserViewMode.CATEGORIES) {
                _categoriesFacade.Focus = true;
                _categoriesFacade.Visible = true;
            }
            else if (CurrentView != BrowserViewMode.DETAILS) {
                facade.Focus = true;
                facade.Visible = true;
            }

            // Update listeners that the view changed
            if (ViewChanged != null) ViewChanged(previousView, currentView);
        }

        /// <summary>
        /// Reapplies all existing filters to the movies in the browser. This should be
        /// called if an existing filter has been modified.
        /// </summary>
        public void ReapplyFilters() {
            lock (syncRefresh) {

                // (re)initialize the filtered movie list
                filteredMovies = new HashSet<DBMovieInfo>();

                // clear the available movie cache because it partly is 
                // derived from filtered movies
                availableMovies.Clear();

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
        }

        #endregion

        #region Core MovieBrowser Methods

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

            onMovieContentsChange();
        }

        private void onMovieUpdated(DatabaseTable obj) {
            // if this is not a movie or related localmedia object, break
            if (obj.GetType() != typeof(DBMovieInfo)) {
                if (obj.GetType() != typeof(DBLocalMedia))
                    return;
                else if (((DBLocalMedia)obj).AttachedMovies.Count == 0)
                    return;
            }

            onMovieContentsChange();
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

            onMovieContentsChange();
        }

        private void onMovieContentsChange() {
            
            // flag that we need a refresh
            // and clear our cached movie lists
            lock (syncRefresh) {
                
                refreshFacade = true;
                filteredMovies = null;
                possibleMovies.Clear();

                // if we are in the details screen we don't have to set a timer to perform the reload
                if (!AutoRefresh || CurrentView == BrowserViewMode.DETAILS)
                    return;

                // Initiate a refresh to be performed 5 seconds after the last update
                if (refreshFacadeTimer == null) {
                    refreshFacadeTimer = new Timer(RefreshFacade, null, 5000, Timeout.Infinite);
                }
                else {
                    refreshFacadeTimer.Change(5000, Timeout.Infinite);
                }
            }
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
        }

        private void onFilterUpdated(IFilter<DBMovieInfo> obj) {
            logger.Debug("OnFilterUpdated: " + obj);
            ReapplyFilters();
            ReloadFacade();        
        }

        private void onContentsChanged() {
            if (ContentsChanged != null)
                ContentsChanged();
        }

        #endregion

        // these methods should eventually be extracted to a super class allowing
        // for the movie browser to be reused by other GUIs for other HTPC apps.
        #region Facade Management Methods

        // reloads one of the facades depending on view settings
        public void ReloadFacade() {
            if (CurrentView == BrowserViewMode.CATEGORIES) {
                ReloadCategoriesFacade();
            }
            else if (CurrentView != BrowserViewMode.DETAILS) {
                ReloadMovieFacade();
            }            

            // content has changed
            onContentsChanged();
        }

        /// <summary>
        /// Reloads the facade as result from a delayed refresh
        /// </summary>
        /// <param name="state"></param>
        private void RefreshFacade(object state) {
            // If we are in details view we don't need to do a reload
            if (!AutoRefresh || CurrentView == BrowserViewMode.DETAILS )
                return;

            // Check if we still need to reload
            lock (syncRefresh) {
                if (!refreshFacade)
                    return;
            }
            
            // Reload
            logger.Debug("Refreshing Visible Facade After Receiving Movie Updates");
            ReloadFacade();
        }


        // populates the category facade
        public void ReloadCategoriesFacade() {
            if (!CategoriesAvailable || SubNodes.Count == 0)
                return;

            CategoriesFacade.ClearAll();

            // Refresh the facade
            lock (syncRefresh) {
                refreshFacade = false;
                if (FilteredMovies.Count > 0) {
                    SubNodes.Sort();
                    foreach (DBNode<DBMovieInfo> currNode in SubNodes) {
                        if (HasAvailableMovies(currNode))
                            addCategoryNodeToFacade(currNode);
                    }
                }
            }

            // Sync to facade
            int index;
            DBNode<DBMovieInfo> node = _categoriesFacade.SyncToFacade<DBNode<DBMovieInfo>>(SelectedNode, out index);
            if (index == 0) SelectedNode = node;
        }

        // populates the facade with the currently filtered list items
        public void ReloadMovieFacade() {
            if (facade == null)
                return;

            // clear and populate the facade
            facade.ClearAll();

            lock (syncRefresh) {
                refreshFacade = false;

                foreach (DBMovieInfo currMovie in FilteredMovies) 
                    addMovieToFacade(currMovie);

                // sort it using our basic sorter
                facade.Sort(new GUIListItemMovieComparer(this.CurrentSortField, this.CurrentSortDirection));

                if (MovingPicturesCore.Settings.AllowGrouping && CurrentView == BrowserViewMode.LIST) {
                    GroupHeaders.AddGroupHeaders(this);
                }
            }

            // reapply the current selection
            SelectedMovie = facade.SyncToFacade<DBMovieInfo>(selectedMovie, out selectedIndex);        
        }

        // triggered when a movie was selected on the facade
        public void onMovieItemSelected(GUIListItem item, GUIControl parent) {
            if (!facade.IsRelated(parent) || facade.SelectedListItem != item)
                return;

            selectedIndex = facade.SelectedListItemIndex;
            SelectedMovie = facade.SelectedListItem.TVTag as DBMovieInfo;
        }

        public void onCategoryNodeSelected(GUIListItem item, GUIControl parent) {
            // if this is not a message from the facade, exit
            if (!_categoriesFacade.IsRelated(parent) || _categoriesFacade.SelectedListItem != item)
                return;

            SelectedNode = _categoriesFacade.SelectedListItem.TVTag as DBNode<DBMovieInfo>;
        }

        // adds the given movie to the facade and creates a GUIListItem if neccesary
        private void addCategoryNodeToFacade(DBNode<DBMovieInfo> newNode) {
            if (newNode == null || _categoriesFacade == null)
                return;

            // if needed, create a new GUIListItem
            if (!listItems.ContainsKey(newNode)) {
                GUIListItem currItem = new GUIListItem();
                
                // Try to parse the category name and execute  translations if needed
                currItem.Label = Translation.ParseString(newNode.Name);
                //currItem.IconImage = newMovie.CoverThumbFullPath.Trim();
                //currItem.IconImageBig = newMovie.CoverThumbFullPath.Trim();
                currItem.TVTag = newNode;
                currItem.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(onCategoryNodeSelected);

                listItems[newNode] = currItem;
            }

            // add the listitem
            _categoriesFacade.Add(listItems[newNode]);
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
        /// Gets the GUIListItem object associated with this movie
        /// </summary>
        /// <param name="movie"></param>
        /// <returns></returns>
        public GUIListItem GetMovieListItem(DBMovieInfo movie) {
            if (!listItems.ContainsKey(movie))
                return null;

            return listItems[movie];
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
        /// Returns a value indicating wether this node has available movies
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool HasAvailableMovies(DBNode<DBMovieInfo> node) {
            lock (syncRefresh) {
                HashSet<DBMovieInfo> results = GetAvailableMovies(node);
                return (results.Count > 0);
            }
        }

        /// <summary>
        /// Returns a list of available movies for this node (includes active filters)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public HashSet<DBMovieInfo> GetAvailableMovies(DBNode<DBMovieInfo> node) {
            lock (syncRefresh) {
                if (FilteredMovies.Count == 0) {
                    availableMovies[node] = new HashSet<DBMovieInfo>();
                }
                else if (!availableMovies.ContainsKey(node)) {
                    HashSet<DBMovieInfo> nodeResults = new HashSet<DBMovieInfo>(GetPossibleMovies(node));
                    // intersect with the filtered movie list meaning that what is 
                    // not in the filtered movies list will be removed from the base list
                    nodeResults.IntersectWith(FilteredMovies);
                    availableMovies[node] = nodeResults;
                }
            }
            return availableMovies[node];
        }

        /// <summary>
        /// Returns a list of possible movies for this node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public HashSet<DBMovieInfo> GetPossibleMovies(DBNode<DBMovieInfo> node) {
            lock (syncRefresh) {
                if (!possibleMovies.ContainsKey(node)) {
                    possibleMovies[node] = node.GetPossibleFilteredItems();
                }
            }
            return possibleMovies[node];
        }

        // TODO replace commented out code with a special "midnight" timer that calls the OnMovieContentsChanged event (effectively clearing the cache once every day at midnight)
        // filters using relative date criteria need a refresh when the day changes
        // if (refreshToday < DateTime.Today) {
        //    refreshToday = DateTime.Today;
        //}

        #endregion

    }
}
