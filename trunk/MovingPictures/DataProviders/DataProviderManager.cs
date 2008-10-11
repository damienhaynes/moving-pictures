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

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    public class DataProviderManager {
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

            loadProvidersFromDatabase();
            loadMissingDefaultProviders();
            normalizePriorities();

            //DBSetting alreadyInitedSetting = MovingPicturesCore.SettingsManager["internal_init_dp_manager_done"];
            //bool alreadyInited = (bool) alreadyInitedSetting.Value;

            //if (!alreadyInited) {
            //    restoreToDefaults();
            //    alreadyInitedSetting.Value = "true";
            //    alreadyInitedSetting.Commit();
            //}

        }

        #endregion

        #region DataProvider Management Functionality

        public void ChangePriority(DBSourceInfo source, DataType type, bool raise) {
            if (source.IsDisabled(type))
                return;
            
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
            foreach (DBSourceInfo currSource in DBSourceInfo.GetAll()) 
                addToLists(currSource);

            detailSources.Sort(sorters[DataType.DETAILS]);
            coverSources.Sort(sorters[DataType.COVERS]);
            backdropSources.Sort(sorters[DataType.BACKDROPS]);
        }

        private void loadMissingDefaultProviders() {
            addSource(typeof(LocalProvider));
            
            addSource(typeof(ScriptableProvider), Resources.IMDb);
            addSource(typeof(ScriptableProvider), Resources.MovieMeter);
            addSource(typeof(ScriptableProvider), Resources.OFDb);
            addSource(typeof(ScriptableProvider), Resources.Allocine);

            addSource(typeof(MeligroveProvider));
            addSource(typeof(ScriptableProvider), Resources.IMPAwards);
        }

        private void addSource(Type providerType, string scriptContents) {
            IScriptableMovieProvider newProvider = (IScriptableMovieProvider)Activator.CreateInstance(providerType);

            DBScriptInfo newScript = new DBScriptInfo();
            newScript.Contents = scriptContents;
            
            // checkif we already have this script in memory. If we do, return,
            // or if in debug mode, reload the contents
            foreach (DBSourceInfo currSource in allSources)
                foreach (DBScriptInfo currScript in currSource.Scripts) {
                    if (currScript.Equals(newScript)) {
                        if (DebugMode) {
                            currScript.Contents = scriptContents;
                            currScript.Reload();
                            return;
                        }
                        else return;
                    }
                }


            // build the source information
            DBSourceInfo newSource = new DBSourceInfo();
            newSource.ProviderType = providerType;
            newSource.Scripts.Add(newScript);
            newSource.SelectedScript = newScript;

            // add and commit
            addToLists(newSource);
            newScript.Commit();
            newSource.Commit();
        }

        private void addSource(Type providerType) {
            foreach (DBSourceInfo currSource in allSources)
                if (currSource.ProviderType == providerType)
                    return;

            DBSourceInfo newSource = new DBSourceInfo();
            newSource.ProviderType = providerType;
            newSource.Commit();
            addToLists(newSource);
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
            detailSources[0].Provider.Update(movie);            
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
