using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.Properties;
using System.Reflection;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using System.Collections.ObjectModel;
using NLog;
using System.IO;

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

            debugMode = (bool) MovingPicturesCore.SettingsManager["source_manager_debug"].Value;

            logger.Info("DataProviderManager Starting");
            loadProvidersFromDatabase();

            // if we have already done an initial load, set an internal flag to do updates only
            // when loading internal scripts. We dont want to load in previously deleted scripts
            // during the internal provider loading process.
            DBSetting alreadyInitedSetting = MovingPicturesCore.SettingsManager["source_manager_init_done"];
            updateOnly = (bool)alreadyInitedSetting.Value;
            LoadInternalProviders();
            updateOnly = false;

            if (!(bool)alreadyInitedSetting.Value) {
                alreadyInitedSetting.Value = true;
                alreadyInitedSetting.Commit();
            }

            turnOffMotech();
        }

        private void turnOffMotech() {
            foreach (DBSourceInfo currSource in coverSources) {
                if (currSource.IsScriptable() && currSource.SelectedScript.Provider.ScriptID == 874904) {
                    SetDisabled(currSource, DataType.COVERS, true);
                    return;
                }
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
            sourceList.Sort(sorters[type]);
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
                addToLists(currSource);

            detailSources.Sort(sorters[DataType.DETAILS]);
            coverSources.Sort(sorters[DataType.COVERS]);
            backdropSources.Sort(sorters[DataType.BACKDROPS]);
        }

        public void LoadInternalProviders() {
            logger.Info("Checking internal scripts for updates...");

            AddSource(typeof(LocalProvider));
            
            AddSource(typeof(ScriptableProvider), Resources.IMDb);
            AddSource(typeof(ScriptableProvider), Resources.MovieMeter);
            AddSource(typeof(ScriptableProvider), Resources.OFDb);
            AddSource(typeof(ScriptableProvider), Resources.Allocine);

            AddSource(typeof(MeligroveProvider));
            AddSource(typeof(ScriptableProvider), Resources.IMPAwards);
            AddSource(typeof(ScriptableProvider), Resources.MoTechPosters);
            
            AddSource(typeof(MyVideosProvider));

            normalizePriorities();
        }

        public AddSourceResult AddSource(Type providerType, string scriptContents) {
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
                                return AddSourceResult.SUCCESS_REPLACED;
                            }
                            else {
                                logger.Info("Script already loaded.");
                                return AddSourceResult.FAILED_VERSION;
                            }
                        }
                    }

                    // if the date is unique, go ahead and add the new script
                    if (uniqueDate) {
                        currSource.Scripts.Add(newScript);
                        currSource.SelectedScript = newScript;
                        newScript.Commit();
                        currSource.Commit();
                        return AddSourceResult.SUCCESS;
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
            addToLists(newSource);
            newScript.Commit();
            newSource.Commit();

            return AddSourceResult.SUCCESS;
        }

        public AddSourceResult AddSource(Type providerType) {
            foreach (DBSourceInfo currSource in allSources)
                if (currSource.ProviderType == providerType)
                    return AddSourceResult.FAILED;

            DBSourceInfo newSource = new DBSourceInfo();
            newSource.ProviderType = providerType;
            newSource.Commit();
            addToLists(newSource);

            return AddSourceResult.SUCCESS;
        }

        public void RemoveSource(DBSourceInfo source) {
            foreach (DataType currType in Enum.GetValues(typeof(DataType)))
                getEditableList(currType).Remove(source);

            allSources.Remove(source);
            source.Delete();
        }

        private void addToLists(DBSourceInfo newSource) {
            allSources.Add(newSource);
            if (newSource.Provider.ProvidesBackdrops)
                backdropSources.Add(newSource);

            if (newSource.Provider.ProvidesCoverArt)
                coverSources.Add(newSource);

            if (newSource.Provider.ProvidesMoviesDetails)
                detailSources.Add(newSource);
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
            return detailSources[0].Provider.Get(movieSignature);
        }

        public void Update(DBMovieInfo movie) {
            foreach(DBSourceInfo currSource in detailSources) {
                if (currSource.IsDisabled(DataType.DETAILS))
                    continue;

                UpdateResults result = currSource.Provider.Update(movie);
                if (result == UpdateResults.SUCCESS)
                    break;
            }
        }

        public bool GetArtwork(DBMovieInfo movie) {
            foreach (DBSourceInfo currSource in coverSources) {
                if (currSource.IsDisabled(DataType.COVERS))
                    continue;

                bool success = currSource.Provider.GetArtwork(movie);
                if (success) return true;
            }

            return false;
        }

        public bool GetBackdrop(DBMovieInfo movie) {
            foreach (DBSourceInfo currSource in backdropSources) {
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
