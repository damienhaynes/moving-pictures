﻿using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.Properties;
using System.Reflection;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Plugins.MovingPictures.SignatureBuilders;
using System.Collections.ObjectModel;
using NLog;
using System.IO;
using Cornerstone.Tools.Translate;
using System.Globalization;
using System.Threading;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    public class DataProviderManager {
        public enum AddSourceResult { 
            SUCCESS,          // successfully added the source
            FAILED,           // general failure, usually a parsing error or duplicate unscripted source
            SUCCESS_REPLACED, // success, but replaced existing version, this will fail when not in debug mode
            FAILED_VERSION,   // version conflict
            FAILED_DATE       // published date conflict
        }     

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static DataProviderManager instance = null;
        private static String lockObj = "";

        Dictionary<DataType, DBSourceInfoComparer> sorters;
        private DatabaseManager dbManager = null;

        #region Properties
        public ReadOnlyCollection<DBSourceInfo> MovieDetailSources {
            get {
                return detailSources.AsReadOnly(); 
            }
        }
        private List<DBSourceInfo> detailSources;

        public ReadOnlyCollection<DBSourceInfo> CoverSources {
            get {
                return coverSources.AsReadOnly(); 
            }
        }
        private List<DBSourceInfo> coverSources;

        public ReadOnlyCollection<DBSourceInfo> BackdropSources {
            get {
                return backdropSources.AsReadOnly(); 
            }
        }
        private List<DBSourceInfo> backdropSources;

        public ReadOnlyCollection<DBSourceInfo> AllSources {
            get { return allSources.AsReadOnly(); }
        }
        private List<DBSourceInfo> allSources;

        public bool DebugMode {
            get { return debugMode; }
            set {
                if (debugMode != value) {
                    debugMode = value;
                    foreach (DBSourceInfo currSource in allSources) {
                        if (currSource.Provider is IScriptableMovieProvider)
                            ((IScriptableMovieProvider)currSource.Provider).DebugMode = value;
                    }
                }
            }
        } 
        private bool debugMode;

        private bool updateOnly;

        #endregion

        #region Constructors

        public static void Initialize() {
            DataProviderManager.GetInstance();
        }

        public static DataProviderManager GetInstance() {
            lock (lockObj) {
                if (instance == null)
                    instance = new DataProviderManager();
            }
            return instance;
        }

        private DataProviderManager() {
            dbManager = MovingPicturesCore.DatabaseManager;
            
            detailSources = new List<DBSourceInfo>();
            coverSources = new List<DBSourceInfo>();
            backdropSources = new List<DBSourceInfo>();
            allSources = new List<DBSourceInfo>();

            sorters = new Dictionary<DataType, DBSourceInfoComparer>();
            sorters[DataType.DETAILS] = new DBSourceInfoComparer(DataType.DETAILS);
            sorters[DataType.COVERS] = new DBSourceInfoComparer(DataType.COVERS);
            sorters[DataType.BACKDROPS] = new DBSourceInfoComparer(DataType.BACKDROPS);

            debugMode = MovingPicturesCore.Settings.DataSourceDebugActive;

            logger.Info("DataProviderManager Starting");
            loadProvidersFromDatabase();

            // if we have already done an initial load, set an internal flag to do updates only
            // when loading internal scripts. We dont want to load in previously deleted scripts
            // during the internal provider loading process.
            updateOnly = MovingPicturesCore.Settings.DataProvidersInitialized;
            LoadInternalProviders();

            updateOnly = false;
            MovingPicturesCore.Settings.DataProvidersInitialized = true;
        }

        #endregion

        #region Automatic Management Functionailty

        public HashSet<CultureInfo> GetAvailableLanguages() {
            HashSet<CultureInfo> results = new HashSet<CultureInfo>();

            foreach (DBSourceInfo currSource in detailSources) {
                try {
                    if (currSource.Provider.LanguageCode != "")
                        results.Add(new CultureInfo(currSource.Provider.LanguageCode));
                }
                catch (Exception e) {
                    if (e is ThreadAbortException)
                        throw e;
                }
            }

            return results;
        }

        public void AutoArrangeDataProviders() {
            if (MovingPicturesCore.Settings.DataProviderManagementMethod != "auto")
                return;
            
            string languageCode;
            try { languageCode = MovingPicturesCore.Settings.DataProviderAutoLanguage; }
            catch (ArgumentException) {
                languageCode = "en";
            }

            ArrangeDataProviders(languageCode);
        }

        public void ArrangeDataProviders(string languageCode) {
            foreach (DataType currType in Enum.GetValues(typeof(DataType))) {
                int nextRank = 10;
                foreach (DBSourceInfo currSource in getEditableList(currType)) {                    
                    // special case for themoviedb provider. should always be used for details - supports all languages
                    if (currSource.Provider is TheMovieDbProvider && currType == DataType.DETAILS) {
                        currSource.SetPriority(currType, 1);
                        currSource.Commit();
                    }

                    // special case for imdb provider. should always be used as a last resort details provider
                    else if (currSource.IsScriptable() && ((ScriptableProvider)currSource.Provider).ScriptID == 874902 && currType == DataType.DETAILS) {
                        if (languageCode != "en") {
                            currSource.SetPriority(currType, 98);
                            currSource.Commit();
                        }
                        else {
                            currSource.SetPriority(currType, 2);
                            currSource.Commit();
                        }
                    }

                    // special case for themoviedb provider. should always be used for covers and backdrops
                    else if (currSource.Provider is TheMovieDbProvider && (currType == DataType.COVERS || currType == DataType.BACKDROPS)) {
                        currSource.SetPriority(currType, 2);
                        currSource.Commit();
                    }

                    // special case for fanart.tv provider. should always be used for covers and backdrops
                    else if ((currType == DataType.COVERS || currType == DataType.BACKDROPS) && currSource.Provider is FanartTVProvider) {
                        currSource.SetPriority(currType, 3);
                        currSource.Commit();
                    }

                    // not a generic language script and not for the selected language, disable
                    else if (currSource.Provider.LanguageCode != "" &&
                             currSource.Provider.LanguageCode != "various" &&
                             currSource.Provider.LanguageCode != languageCode) {

                        currSource.SetPriority(currType, -1);
                        currSource.Commit();
                    }

                    // valid script, enable
                    else {
                        if (currSource.Provider is LocalProvider) {
                            currSource.SetPriority(currType, 0);
                            currSource.Commit();
                        }
                        else if (currSource.Provider is MyVideosProvider) {
                            currSource.SetPriority(currType, 99);
                            currSource.Commit();
                        }
                        else if (currSource.Provider.LanguageCode == "" || currSource.Provider.LanguageCode == "various") {
                            currSource.SetPriority(currType, 50 + nextRank++);
                            currSource.Commit();
                        }
                        else {
                            currSource.SetPriority(currType, nextRank++);
                            currSource.Commit();
                        }
                    }
                }

                // sort and normalize
                getEditableList(currType).Sort(sorters[currType]);
                normalizePriorities();
            }

        }

        #endregion

        #region DataProvider Management Functionality

        public void ChangePriority(DBSourceInfo source, DataType type, bool raise) {
            if (source.IsDisabled(type)) {
                if (raise)
                    SetDisabled(source, type, false);
                else return;
            }
            
            // grab the correct list 
            List<DBSourceInfo> sourceList = getEditableList(type);
            
            // make sure the specified source is in our list
            if (!sourceList.Contains(source))
                return;

            if (source.GetPriority(type) == null) {
                logger.Error("No priority set for " + type.ToString());
                return;
            }

            // make sure our index is in sync
            int index = sourceList.IndexOf(source);
            int oldPriority = (int) source.GetPriority(type);
            if (index != oldPriority)
                logger.Warn("Priority and List.IndexOf out of sync...");

            // raise priority 
            if (raise) {
                if (source.GetPriority(type) > 0) {
                    source.SetPriority(type, oldPriority - 1);
                    sourceList[index - 1].SetPriority(type, oldPriority);

                    source.Commit();
                    sourceList[index - 1].Commit();
                }
            }

            // lower priority
            else {
                if (source.GetPriority(type) < sourceList.Count - 1 && 
                    sourceList[index + 1].GetPriority(type) != -1) {

                    source.SetPriority(type, oldPriority + 1);
                    sourceList[index + 1].SetPriority(type, oldPriority);

                    source.Commit();
                    sourceList[index + 1].Commit();
                }
            }

            // resort the list
            lock (sourceList) sourceList.Sort(sorters[type]);
        }

        public void SetDisabled(DBSourceInfo source, DataType type, bool disable) {
            if (disable) {
                source.SetPriority(type, -1);
                source.Commit();
            }
            else
                source.SetPriority(type, int.MaxValue);

            getEditableList(type).Sort(sorters[type]);
            normalizePriorities();
        }

        private List<DBSourceInfo> getEditableList(DataType type) {
            List<DBSourceInfo> sourceList = null;
            switch (type) {
                case DataType.DETAILS:
                    sourceList = detailSources;
                    break;
                case DataType.COVERS:
                    sourceList = coverSources;
                    break;
                case DataType.BACKDROPS:
                    sourceList = backdropSources;
                    break;
            }

            return sourceList;
        }

        public ReadOnlyCollection<DBSourceInfo> GetList(DataType type) {
            return getEditableList(type).AsReadOnly();
        }


        #endregion

        #region DataProvider Loading Logic

        private void loadProvidersFromDatabase() {
            logger.Info("Loading existing data sources...");

            foreach (DBSourceInfo currSource in DBSourceInfo.GetAll())
                updateListsWith(currSource);

            detailSources.Sort(sorters[DataType.DETAILS]);
            coverSources.Sort(sorters[DataType.COVERS]);
            backdropSources.Sort(sorters[DataType.BACKDROPS]);
        }

        public void LoadInternalProviders() {
            logger.Info("Checking internal scripts for updates...");

            AddSource(typeof(LocalProvider));
            AddSource(typeof(ScriptableProvider), Resources.Script_IMDb);
            AddSource(typeof(TheMovieDbProvider));
            AddSource(typeof(TraktTVProvider));
            AddSource(typeof(FanartTVProvider));
            AddSource(typeof(MovieMeterProvider));
            AddSource(typeof(ScriptableProvider), Resources.Script_OFDb);
            AddSource(typeof(ScriptableProvider), Resources.Script_MovieMaze);
            AddSource(typeof(ScriptableProvider), Resources.Script_Allocine);
            AddSource(typeof(ScriptableProvider), Resources.Script_MyMoviesItalian);
            AddSource(typeof(ScriptableProvider), Resources.Script_FilmWeb);
            AddSource(typeof(ScriptableProvider), Resources.Script_Scope);
            AddSource(typeof(ScriptableProvider), Resources.Script_Kinopoisk);
            AddSource(typeof(ScriptableProvider), Resources.Script_Alpacine);
            AddSource(typeof(ScriptableProvider), Resources.Script_Sratim);
            AddSource(typeof(ScriptableProvider), Resources.Script_FilmAffinity);
            AddSource(typeof(ScriptableProvider), Resources.Script_CSFD);
            AddSource(typeof(ScriptableProvider), Resources.Script_MyMoviesLocal);
            AddSource(typeof(ScriptableProvider), Resources.Script_XBMC);
            AddSource(typeof(ScriptableProvider), Resources.Script_Filmtipset);
            AddSource(typeof(ScriptableProvider), Resources.Script_Ptgate);
            AddSource(typeof(ScriptableProvider), Resources.Script_Daum);
            AddSource(typeof(ScriptableProvider), Resources.Script_kvikmyndir);
            AddSource(typeof(ScriptableProvider), Resources.Script_EmberMediaManager);            
            AddSource(typeof(MyVideosProvider));

            // remove the impawards script (requested by site owner)
            DBSourceInfo impSource = DBSourceInfo.GetFromScriptID(874903);
            if (impSource != null) {
                logger.Warn("IMPAwards script has been disabled at the website operators request. Very sorry!");
                RemoveSource(impSource);
            }

            normalizePriorities();
        }

        public AddSourceResult AddSource(Type providerType, string scriptContents) {
            return AddSource(providerType, scriptContents, false);
        }
        
        public AddSourceResult AddSource(Type providerType, string scriptContents, bool active) {
            IScriptableMovieProvider newProvider = (IScriptableMovieProvider)Activator.CreateInstance(providerType);

            DBScriptInfo newScript = new DBScriptInfo();
            newScript.Contents = scriptContents;
            
            // if a provider can't be created based on this script we have a bad script file.
            if (newScript.Provider == null)
                return AddSourceResult.FAILED;
            
            // check if we already have this script in memory.
            foreach (DBSourceInfo currSource in allSources)
                // if some version of the script is already in the database
                if (currSource.IsScriptable() && ((IScriptableMovieProvider)currSource.Provider).ScriptID == newScript.Provider.ScriptID) {
                    bool uniqueDate = newScript.Provider.Published != null;
                    foreach (DBScriptInfo currScript in currSource.Scripts) {
                        if (uniqueDate && 
                            currScript.Provider.Published != null &&
                            currScript.Provider.Published.Value.Equals(newScript.Provider.Published.Value))

                            uniqueDate = false;
                        
                        // check if the same version is already loaded
                        if (currScript.Equals(newScript)) {
                            if (DebugMode) {
                                logger.Warn("Script version number already loaded. Reloading because in Debug Mode.");
                                currScript.Contents = scriptContents;
                                currScript.Reload();
                                updateListsWith(currSource);
                                currScript.Commit();
                                normalizePriorities();
                                return AddSourceResult.SUCCESS_REPLACED;
                            }
                            else {
                                logger.Debug("Script already loaded.");
                                return AddSourceResult.FAILED_VERSION;
                            }
                        }
                    }

                    // if the date is unique, go ahead and add the new script
                    if (uniqueDate || DebugMode) {
                        currSource.Scripts.Add(newScript);
                        currSource.SelectedScript = newScript;
                        updateListsWith(currSource);
                        newScript.Commit();
                        currSource.Commit();
                        normalizePriorities();

                        if (uniqueDate)
                            return AddSourceResult.SUCCESS;
                        else
                            return AddSourceResult.SUCCESS_REPLACED;
                    }
                    else {
                        logger.Error("Script failed to load, publish date is not unique.");
                        return AddSourceResult.FAILED_DATE;
                    }
                }

            // if there was nothing to update, and we are not looking to add new data sources, quit
            if (updateOnly)
                return AddSourceResult.SUCCESS;

            // build the new source information
            DBSourceInfo newSource = new DBSourceInfo();
            newSource.ProviderType = providerType;
            newSource.Scripts.Add(newScript);
            newSource.SelectedScript = newScript;
            
            // add and commit
            updateListsWith(newSource);
            newScript.Commit();
            newSource.Commit();
            normalizePriorities();

            // if not set to active, disable the new source by default
            if (!active) 
                foreach (DataType currType in Enum.GetValues(typeof(DataType)))
                    SetDisabled(newSource, currType, true);

            return AddSourceResult.SUCCESS;
        }

        public AddSourceResult AddSource(Type providerType) {
            // internal scripts dont need to be updated, so just quit
            // if we dont need to reload everything
            if (updateOnly) return AddSourceResult.FAILED;

            foreach (DBSourceInfo currSource in allSources)
                if (currSource.ProviderType == providerType)
                    return AddSourceResult.FAILED;

            DBSourceInfo newSource = new DBSourceInfo();
            newSource.ProviderType = providerType;
            newSource.Commit();
            updateListsWith(newSource);
            normalizePriorities();

            return AddSourceResult.SUCCESS;
        }

        public void RemoveSource(DBSourceInfo source) {
            if (source == null)
                return;

            foreach (DataType currType in Enum.GetValues(typeof(DataType)))
                lock (getEditableList(currType)) 
                    getEditableList(currType).Remove(source);

            lock (allSources) allSources.Remove(source);
            source.Delete();
        }

        private void updateListsWith(DBSourceInfo newSource) {
            if (newSource.ProviderType == null) {
               logger.Info("Removing invalid provider.");
               newSource.Delete();
               return;
            }

            lock (allSources) 
                if (!allSources.Contains(newSource)) 
                    allSources.Add(newSource);

            lock (backdropSources) {
                if (newSource.Provider.ProvidesBackdrops && !backdropSources.Contains(newSource))
                    backdropSources.Add(newSource);
                else if (!newSource.Provider.ProvidesBackdrops && backdropSources.Contains(newSource))
                    backdropSources.Remove(newSource);
            }

            lock (coverSources) {
                if (newSource.Provider.ProvidesCoverArt && !coverSources.Contains(newSource))
                    coverSources.Add(newSource);
                else if (!newSource.Provider.ProvidesCoverArt && coverSources.Contains(newSource))
                    coverSources.Remove(newSource);
            }

            lock (detailSources) {
                if (newSource.Provider.ProvidesMoviesDetails && !detailSources.Contains(newSource))
                    detailSources.Add(newSource);
                else if (!newSource.Provider.ProvidesMoviesDetails && detailSources.Contains(newSource))
                    detailSources.Remove(newSource);
            }
        }

        // reinitializes all the priorities based on existing list order.
        // should be called after new items have been added to the list or an
        // tiem has been ignored
        private void normalizePriorities() {
            foreach (DataType currType in Enum.GetValues(typeof(DataType))) {
                int count = 0;
                foreach (DBSourceInfo currSource in getEditableList(currType)) {
                    if (currSource.GetPriority(currType) != count && currSource.GetPriority(currType) != -1) {
                        currSource.SetPriority(currType, count);
                        currSource.Commit();
                    }
                    count++;

                }
            }
        }

        #endregion

        #region Data Loading Methods

        public List<DBMovieInfo> Get(MovieSignature movieSignature) {
            List<DBSourceInfo> sources;
            lock (detailSources) sources = new List<DBSourceInfo>(detailSources);

            // Try each datasource (ordered by their priority) to get results
            List<DBMovieInfo> results = new List<DBMovieInfo>();
            foreach (DBSourceInfo currSource in sources) {
                if (currSource.IsDisabled(DataType.DETAILS))
                    continue;

                // if we have reached the minimum number of possible matches required, we are done
                if (results.Count >= MovingPicturesCore.Settings.MinimumMatches &&
                    MovingPicturesCore.Settings.MinimumMatches != 0)
                    break;

                // search with the current provider
                List<DBMovieInfo> newResults = currSource.Provider.Get(movieSignature);

                // tag the results with the current source
                foreach (DBMovieInfo currMovie in newResults)
                    currMovie.PrimarySource = currSource;

                // add results to our total result list and log what we found
                results.AddRange(newResults);
                logger.Debug("SEARCH: Title='{0}', Provider='{1}', Version={2}, Number of Results={3}", movieSignature.Title, currSource.Provider.Name, currSource.Provider.Version, newResults.Count);
            }

            return results;
        }

        public void Update(DBMovieInfo movie) {
            List<DBSourceInfo> sources;
            lock (detailSources) sources = new List<DBSourceInfo>(detailSources);
            
            // unlock the movie fields for the first iteration
            movie.ProtectExistingValuesFromCopy(false);
            
            // first update from the primary source of this data
            int providerCount = 0;
            if (movie.PrimarySource != null && movie.PrimarySource.Provider != null) {
                UpdateResults success = movie.PrimarySource.Provider.Update(movie);
                logger.Debug("UPDATE: Title='{0}', Provider='{1}', Version={2}, Result={3}", movie.Title, movie.PrimarySource.Provider.Name, movie.PrimarySource.Provider.Version, success.ToString());
                providerCount++;
            }

            foreach (DBSourceInfo currSource in sources) {
                if (movie.IsFullyPopulated()) {
                    logger.Debug("UPDATE: All fields are populated. Done updating '" + movie.Title + "'.");
                    break;
                }

                if (currSource.IsDisabled(DataType.DETAILS))
                    continue;

                if (currSource == movie.PrimarySource)
                    continue;

                providerCount++;

                if (providerCount <= MovingPicturesCore.Settings.DataProviderRequestLimit || MovingPicturesCore.Settings.DataProviderRequestLimit == 0) {
                    UpdateResults success = currSource.Provider.Update(movie);
                    logger.Debug("UPDATE: Title='{0}', Provider='{1}', Version={2}, Result={3}", movie.Title, currSource.Provider.Name, currSource.Provider.Version, success.ToString());
                }
                else {
                    // stop update
                    break;
                }

                if (MovingPicturesCore.Settings.UseTranslator) {
                    movie.Translate();
                }
            }
        }

        public bool GetArtwork(DBMovieInfo movie) {
            // if we have already hit our limit for the number of covers to load, quit
            if (movie.AlternateCovers.Count >= MovingPicturesCore.Settings.MaxCoversPerMovie)
                return true;
            
            List<DBSourceInfo> sources;
            lock (coverSources) sources = new List<DBSourceInfo>(coverSources);

            foreach (DBSourceInfo currSource in sources) {
                if (currSource.IsDisabled(DataType.COVERS))
                    continue;

                bool success = currSource.Provider.GetArtwork(movie);
                if (success) return true;
            }

            return false;
        }

        public bool GetBackdrop(DBMovieInfo movie) {
            List<DBSourceInfo> sources;
            lock (backdropSources) sources = new List<DBSourceInfo>(backdropSources);
            
            foreach (DBSourceInfo currSource in sources) {
                if (currSource.IsDisabled(DataType.BACKDROPS))
                    continue;

                bool success = currSource.Provider.GetBackdrop(movie);
                if (success) return true;
            }

            return false;
        }

        #endregion
    }

}
