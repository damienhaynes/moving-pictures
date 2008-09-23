using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.Properties;
using System.Reflection;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    public class DataProviderManager {
        private static DataProviderManager instance = null;
        private static String lockObj = "";
        
        private DatabaseManager dbManager = null;

        private List<DBSourceInfo> detailSources;
        private List<DBSourceInfo> coverSources;
        private List<DBSourceInfo> backdropSources;
        private List<DBSourceInfo> allSources;

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

            loadProvidersFromDatabase();
            loadMissingDefaultProviders();

            //DBSetting alreadyInitedSetting = MovingPicturesCore.SettingsManager["internal_init_dp_manager_done"];
            //bool alreadyInited = (bool) alreadyInitedSetting.Value;

            //if (!alreadyInited) {
            //    restoreToDefaults();
            //    alreadyInitedSetting.Value = "true";
            //    alreadyInitedSetting.Commit();
            //}

        }

        #region DataProvider Loading Logic

        private void loadProvidersFromDatabase() {
            foreach (DBSourceInfo currSource in DBSourceInfo.GetAll()) 
                addToLists(currSource);
        }

        private void loadMissingDefaultProviders() {
            addSource(typeof(ScriptableProvider), Resources.IMDb, 1);
            // addSource(typeof(MovieXMLProvider), 2);

            addSource(typeof(LocalProvider), 1);
            addSource(typeof(MeligroveProvider), 2);
            addSource(typeof(ScriptableProvider), Resources.IMPAwards, 3);
        }

        private void addSource(Type providerType, string script, int priority) {
            IScriptableMovieProvider newProvider = (IScriptableMovieProvider)Activator.CreateInstance(providerType);

            DBScriptInfo scriptInfo = new DBScriptInfo();
            scriptInfo.Contents = script;

            foreach (DBSourceInfo currSource in allSources)
                if (currSource.Scripts.Contains(scriptInfo)) {
                    return;
                }

            // build the source information
            DBSourceInfo newSource = new DBSourceInfo();
            newSource.ProviderType = providerType;
            newSource.Scripts.Add(scriptInfo);
            newSource.SelectedScript = scriptInfo;

            // add and commit
            addToLists(newSource);
            scriptInfo.Commit();
            newSource.Commit();
        }

        private void addSource(Type providerType, int priority) {
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

        #endregion

        public List<DBMovieInfo> Get(MovieSignature movieSignature) {
            return detailSources[0].Provider.Get(movieSignature);
        }

        public void Update(DBMovieInfo movie) {
            detailSources[0].Provider.Update(movie);            
        }

        public bool GetArtwork(DBMovieInfo movie) {
            foreach (DBSourceInfo currSource in coverSources) {
                bool success = currSource.Provider.GetArtwork(movie);
                if (success) return true;
            }

            return false;
        }

        public bool GetBackdrop(DBMovieInfo movie) {
            foreach (DBSourceInfo currSource in backdropSources) {
                bool success = currSource.Provider.GetBackdrop(movie);
                if (success) return true; 
            }

            return false;
        }
    }
}
