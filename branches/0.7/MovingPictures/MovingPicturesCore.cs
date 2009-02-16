using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Cornerstone.Database;
using MediaPortal.Configuration;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.DataProviders;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Profile;
using MediaPortal.Services;
using NLog;
using NLog.Config;
using NLog.Targets;
using MediaPortal.Plugins.MovingPictures.Properties;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;

namespace MediaPortal.Plugins.MovingPictures {
    public delegate void WorkerDelegate();
    public delegate void TrackableWorkerDelegate(ProgressDelegate progress);
    public delegate void ProgressDelegate(string actionName, int percentDone);

    public class MovingPicturesCore {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static event ProgressDelegate InitializeProgress;

        private const string dbFileName = "movingpictures.db3";
        private const string logFileName = "movingpictures.log";
        private const string oldLogFileName = "movingpictures.old.log";

        private static float loadingProgress;
        private static float loadingTotal;
        private static string loadingProgressDescription;

        #region Properties & Events

        // The MovieImporter object that should be used by all components of the plugin
        public static MovieImporter Importer {
            get {
                if (importer == null)
                    importer = new MovieImporter();

                return importer;
            }
        } private static MovieImporter importer;

        // The DatabaseManager that should be used by all components of the plugin.       
        public static DatabaseManager DatabaseManager {
            get {
                if (databaseManager == null)
                    initDB();

                return databaseManager;
            }
        }  private static DatabaseManager databaseManager;

        // The SettingsManager that should be used by all components of the plugin.
        public static SettingsManager SettingsManager {
            get {
                if (settingsManager == null)
                    initSettings();

                return settingsManager;
            }
        } private static SettingsManager settingsManager;

        public static DataProviderManager DataProviderManager {
            get {
                return DataProviderManager.GetInstance();
            }
        }

        // Settings from Media Portal
        // Instead of calling this line whenever we need some MP setting we only define it once
        // There isn't really a central MePo settings manager (or is there?)
        public static Settings MediaPortalSettings {
            get {
                Settings mpSettings = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"));
                return mpSettings;
            }
        }

        #endregion

        #region Public Methods

        static MovingPicturesCore() {
            initLogger();
        }

        public static void Initialize() {
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            logger.Info("Moving Pictures (" + ver.Major + "." + ver.Minor + "." + ver.Build + ":" + ver.Revision + ")");
            logger.Info("Plugin Launched");

            DatabaseMaintenanceManager.MaintenanceProgress += new ProgressDelegate(DatabaseMaintenanceManager_MaintenanceProgress);

            // setup the data structures sotring our list of startup actions
            // we use this setup so we can easily add new tasks without having to 
            // tweak any magic numbers for the progress bar / loading screen
            List<WorkerDelegate> initActions = new List<WorkerDelegate>();
            Dictionary<WorkerDelegate, string> actionDescriptions = new Dictionary<WorkerDelegate, string>();
            WorkerDelegate newAction;

            newAction = new WorkerDelegate(initDB);
            actionDescriptions.Add(newAction, "Initializing Database...");
            initActions.Add(newAction);

            newAction = new WorkerDelegate(initAdditionalSettings);
            actionDescriptions.Add(newAction, "Initializing Path Settings...");
            initActions.Add(newAction);

            newAction = new WorkerDelegate(DatabaseMaintenanceManager.UpdateDateAddedFields);
            actionDescriptions.Add(newAction, "Updating sorting metadata...");
            initActions.Add(newAction);

            newAction = new WorkerDelegate(DatabaseMaintenanceManager.RemoveInvalidFiles);
            actionDescriptions.Add(newAction, "Checking for deleted movies...");
            initActions.Add(newAction);

            newAction = new WorkerDelegate(DatabaseMaintenanceManager.RemoveInvalidMovies);
            actionDescriptions.Add(newAction, "Removing invalid movie entries...");
            initActions.Add(newAction);

            newAction = new WorkerDelegate(DatabaseMaintenanceManager.RemoveOrphanArtwork);
            actionDescriptions.Add(newAction, "Removing missing artwork...");
            initActions.Add(newAction);

            newAction = new WorkerDelegate(DatabaseMaintenanceManager.UpdateImportPaths);
            actionDescriptions.Add(newAction, "Updating import paths...");
            initActions.Add(newAction);

            newAction = new WorkerDelegate(DatabaseMaintenanceManager.UpdateMissingDiskInfoProperties);
            actionDescriptions.Add(newAction, "Updating disk information...");
            initActions.Add(newAction);

            newAction = new WorkerDelegate(DatabaseMaintenanceManager.UpdateUserSettings);
            actionDescriptions.Add(newAction, "Updating user settings...");
            initActions.Add(newAction);

            newAction = new WorkerDelegate(checkVersionInfo);
            actionDescriptions.Add(newAction, "Initializing Version Information...");
            initActions.Add(newAction);

            newAction = new WorkerDelegate(DataProviderManager.Initialize);
            actionDescriptions.Add(newAction, "Initializing Data Provider Manager...");
            initActions.Add(newAction);

            newAction = new WorkerDelegate(DeviceManager.StartMonitor);
            actionDescriptions.Add(newAction, "Starting Device Monitor...");
            initActions.Add(newAction);


            // load all the above actions and notify any listeners of our progress
            loadingProgress = 0;
            loadingTotal = initActions.Count;
            foreach (WorkerDelegate currAction in initActions) {
                if (InitializeProgress != null) InitializeProgress(actionDescriptions[currAction], (int)(loadingProgress * 100 / loadingTotal));
                loadingProgressDescription = actionDescriptions[currAction];

                currAction();
                loadingProgress++;
            }

            if (InitializeProgress != null) InitializeProgress("Done!", 100);
        }

        static void DatabaseMaintenanceManager_MaintenanceProgress(string actionName, int percentDone) {
            int baseProgress = (int)(loadingProgress * 100 / loadingTotal);
            if (InitializeProgress != null) InitializeProgress(loadingProgressDescription, baseProgress + (int)((float)percentDone / loadingTotal));
        }

        public static void Shutdown() {
            DeviceManager.StopMonitor();
            importer.Stop();
            settingsManager.Shutdown();

            importer = null;
            settingsManager = null;
            databaseManager = null;

            logger.Info("Plugin Closed");
        }

        #endregion

        #region Private Methods

        // Initializes the database connection to the Movies Plugin database
        private static void initDB() {
            if (databaseManager != null)
                return;

            string fullDBFileName = Config.GetFile(Config.Dir.Database, dbFileName);
            databaseManager = new DatabaseManager(fullDBFileName);

            // check that we at least have a default user
            List<DBUser> users = DBUser.GetAll();
            if (users.Count == 0) {
                DBUser defaultUser = new DBUser();
                defaultUser.Name = "Default User";
                defaultUser.Commit();
            }
        }

        // Initiallizes the SettingsManager and adds any settings that do not already exist
        private static void initSettings() {
            settingsManager = new SettingsManager(DatabaseManager);

            settingsManager.LoadSettingsFile(Properties.Resources.InitialSettings, false);
        }

        private static void initLogger() {
            LoggingConfiguration config = new LoggingConfiguration();

            try {
                FileInfo logFile = new FileInfo(Config.GetFile(Config.Dir.Log, logFileName));
                if (logFile.Exists) {
                    if (File.Exists(Config.GetFile(Config.Dir.Log, oldLogFileName)))
                        File.Delete(Config.GetFile(Config.Dir.Log, oldLogFileName));

                    logFile.CopyTo(Config.GetFile(Config.Dir.Log, oldLogFileName));
                    logFile.Delete();
                }
            }
            catch (Exception) { }


            FileTarget fileTarget = new FileTarget();
            fileTarget.FileName = Config.GetFile(Config.Dir.Log, logFileName);
            fileTarget.Layout = "${date:format=dd-MMM-yyyy HH\\:mm\\:ss} " +
                                "${level:fixedLength=true:padding=5} " +
                                "[${logger:fixedLength=true:padding=20:shortName=true}]: ${message} " +
                                "${exception:format=tostring}";

            config.AddTarget("file", fileTarget);

            // Get current Log Level from MediaPortal 
            LogLevel logLevel;
            Settings xmlreader = MediaPortalSettings;
            switch ((Level)xmlreader.GetValueAsInt("general", "loglevel", 0)) {
                case Level.Error:
                    logLevel = LogLevel.Error;
                    break;
                case Level.Warning:
                    logLevel = LogLevel.Warn;
                    break;
                case Level.Information:
                    logLevel = LogLevel.Info;
                    break;
                case Level.Debug:
                default:
                    logLevel = LogLevel.Debug;
                    break;
            }

            #if DEBUG
            logLevel = LogLevel.Debug;
            #endif

            LoggingRule rule = new LoggingRule("*", logLevel, fileTarget);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;
        }

        private static void initAdditionalSettings() {
            if (((String)SettingsManager["cover_art_folder"].Value).Trim().Equals(""))
                SettingsManager["cover_art_folder"].Value = Config.GetFolder(Config.Dir.Thumbs) + "\\MovingPictures\\Covers\\FullSize";

            // create the covers folder if it doesn't already exist
            if (!Directory.Exists((string)SettingsManager["cover_art_folder"].Value))
                Directory.CreateDirectory((string)SettingsManager["cover_art_folder"].Value);

            if (((String)SettingsManager["cover_thumbs_folder"].Value).Trim().Equals(""))
                SettingsManager["cover_thumbs_folder"].Value = Config.GetFolder(Config.Dir.Thumbs) + "\\MovingPictures\\Covers\\Thumbs";

            // create the thumbs folder if it doesn't already exist
            if (!Directory.Exists((string)SettingsManager["cover_thumbs_folder"].Value))
                Directory.CreateDirectory((string)SettingsManager["cover_thumbs_folder"].Value);

            if (((String)SettingsManager["backdrop_folder"].Value).Trim().Equals(""))
                SettingsManager["backdrop_folder"].Value = Config.GetFolder(Config.Dir.Thumbs) + "\\MovingPictures\\Backdrops\\FullSize";

            // create the backdrop folder if it doesn't already exist
            if (!Directory.Exists((string)SettingsManager["backdrop_folder"].Value))
                Directory.CreateDirectory((string)SettingsManager["backdrop_folder"].Value);

            if (((String)SettingsManager["backdrop_thumbs_folder"].Value).Trim().Equals(""))
                SettingsManager["backdrop_thumbs_folder"].Value = Config.GetFolder(Config.Dir.Thumbs) + "\\MovingPictures\\Backdrops\\Thumbs";

            // create the backdrop thumbs folder if it doesn't already exist
            if (!Directory.Exists((string)SettingsManager["backdrop_thumbs_folder"].Value))
                Directory.CreateDirectory((string)SettingsManager["backdrop_thumbs_folder"].Value);
        }

        private static void checkVersionInfo() {
            // check if the version changed, and update the DB accordingly
            Version realVer = Assembly.GetExecutingAssembly().GetName().Version;

            if (realVer > GetDBVersionNumber()) {
                SettingsManager["version"].Value = realVer.ToString();
                SettingsManager["version"].Commit();

                SettingsManager["source_manager_init_done"].Value = false;
                SettingsManager["source_manager_init_done"].Commit();
            }
        }

        public static Version GetDBVersionNumber() {
            return new Version((string)SettingsManager["version"].Value);
        }

        #endregion
    }
}
