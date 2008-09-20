using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.Properties;
using System.Reflection;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    public class DataProviderManager {
        private static DataProviderManager instance = null;
        private DatabaseManager dbManager = null;
        
        private List<DBSourceInfo> detailSources;
        private List<DBSourceInfo> coverSources;
        private List<DBSourceInfo> backdropSources;
        private List<DBSourceInfo> allSources;

        public static DataProviderManager GetInstance() {
            if (instance == null)
                instance = new DataProviderManager();

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

        private void loadProvidersFromDatabase() {
            allSources = DBSourceInfo.GetAll();
            foreach (DBSourceInfo currSource in allSources) 
                add(currSource);
        }

        private void loadMissingDefaultProviders() {
            foreach (Type currType in Assembly.GetExecutingAssembly().GetTypes()) {
                if (!currType.IsClass)
                    continue;

                // check if the found type is a scriptable movie provider
                bool found = false;
                foreach (Type currInterface in currType.GetInterfaces())
                    if (currInterface == typeof(IScriptableMovieProvider)) {
                        found = true;

                        IScriptableMovieProvider newProvider = (IScriptableMovieProvider) Activator.CreateInstance(currType);
                        List<string> defaultScripts = newProvider.GetDefaultScripts();

                        // TODO: build list of possible scraper engines here

                        // load any default scripts if they are not already loaded.
                        foreach (string script in defaultScripts) {
                            DBScriptInfo scriptInfo = new DBScriptInfo();
                            scriptInfo.Contents = script;
                            
                            bool alreadyLoaded = false;
                            foreach (DBSourceInfo currSource in allSources)
                                if (currSource.Scripts.Contains(scriptInfo)) {
                                    alreadyLoaded = true; 
                                    break;
                                }

                            if (!alreadyLoaded) {
                                DBSourceInfo newSource = new DBSourceInfo();
                                newSource.ProviderType = currType;
                                newSource.Scripts.Add(scriptInfo);
                                newSource.SelectedScript = scriptInfo;
                                add(newSource);
                            }
                        }
                    }

                if (found) continue;

                // check if the found type is a regular movie provider
                foreach (Type currInterface in currType.GetInterfaces())
                    if (currInterface == typeof(IMovieProvider)) {
                        bool alreadyLoaded = false;
                        foreach (DBSourceInfo currSource in allSources) 
                            if (currSource.ProviderType == currType)
                                alreadyLoaded = true;

                        if (alreadyLoaded) break;

                        DBSourceInfo newSource = new DBSourceInfo();
                        newSource.ProviderType = currType;
                        add(newSource);
                    }
            }
        }

        private void add(DBSourceInfo newSource) {
            allSources.Add(newSource);
            if (newSource.Provider.ProvidesBackdrops)
                backdropSources.Add(newSource);

            if (newSource.Provider.ProvidesCoverArt)
                coverSources.Add(newSource);

            if (newSource.Provider.ProvidesMoviesDetails)
                detailSources.Add(newSource);
        }
        
        public List<DBMovieInfo> Get(string movieTitle) {
            return detailSources[0].Provider.Get(movieTitle);
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
