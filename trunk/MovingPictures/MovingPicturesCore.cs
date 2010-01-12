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
using Cornerstone.Database.Tables;
using Cornerstone.GUI.Dialogs;
using Cornerstone.Tools;
using MediaPortal.Plugins.MovingPictures.BackgroundProcesses;
using System.Threading;
using MovingPicturesSocialAPI;

namespace MediaPortal.Plugins.MovingPictures {
    public class MovingPicturesCore {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static event ProgressDelegate InitializeProgress;

        private const string dbFileName = "movingpictures.db3";
        private const string logFileName = "movingpictures.log";
        private const string oldLogFileName = "movingpictures.old.log";

        private static float loadingProgress;
        private static float loadingTotal;
        private static string loadingProgressDescription;

        private static object importerLock = new Object();
        private static object dbLock = new Object();
        private static object settingsLock = new Object();
        private static object processLock = new Object();
        private static object socialAPILock = new Object();

        #region Properties & Events

        // The MovieImporter object that should be used by all components of the plugin
        public static MovieImporter Importer {
            get {
                lock (importerLock) {
                    if (_importer == null)
                        _importer = new MovieImporter();

                    return _importer;
                }
            }
        } private static MovieImporter _importer;

        // The DatabaseManager that should be used by all components of the plugin.       
        public static DatabaseManager DatabaseManager {
            get {
                lock (dbLock) {
                    if (_databaseManager == null)
                        initDB();

                    return _databaseManager;
                }
            }
        }  private static DatabaseManager _databaseManager;

        // The SettingsManager that should be used by all components of the plugin.
        public static MovingPicturesSettings Settings {
            get {
                lock (settingsLock) {
                    if (_settings == null)
                        _settings = new MovingPicturesSettings(DatabaseManager);

                    return _settings;
                }
            }
        } private static MovingPicturesSettings _settings = null;
        
        public static DataProviderManager DataProviderManager {
            get {
                return DataProviderManager.GetInstance();
            }
        }

        public static BackgroundProcessManager ProcessManager {
            get {
                lock (processLock) {
                    if (_processManager == null)
                        _processManager = new BackgroundProcessManager();

                    return _processManager;
                }
            }
        } private static BackgroundProcessManager _processManager = null;

        // Settings from Media Portal
        // Instead of calling this line whenever we need some MP setting we only define it once
        // There isn't really a central MePo settings manager (or is there?)
        public static MediaPortal.Profile.Settings MediaPortalSettings {
            get {
                MediaPortal.Profile.Settings mpSettings = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"));
                return mpSettings;
            }
        }

        // The MpsAPI object that should be used by all components of the plugin.
        public static MovingPicturesSocialAPI.MpsAPI SocialAPI {
            get {
                lock (socialAPILock) {
                    if (_socialAPI == null) {
                        _socialAPI = new MpsAPI(Settings.SocialUsername
                            , Settings.SocialPassword
                            , Settings.SocialURLBase + "api/1.0/");
                    }
                    return _socialAPI;
                }
            }
        } private static MovingPicturesSocialAPI.MpsAPI _socialAPI = null;

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

            newAction = new WorkerDelegate(initAdditionalSettings);
            actionDescriptions.Add(newAction, "Initializing Path Settings...");
            initActions.Add(newAction);

            newAction = new WorkerDelegate(DatabaseMaintenanceManager.UpdateImportPaths);
            actionDescriptions.Add(newAction, "Updating Import Paths...");
            initActions.Add(newAction);

            // only perform the following tasks (one time) when we are upgrading from a previous version
            if (MovingPicturesCore.GetDBVersionNumber() < ver) {
                
                // local media
                newAction = new WorkerDelegate(DatabaseMaintenanceManager.PerformFileInformationUpgradeCheck);
                actionDescriptions.Add(newAction, "Performing File Information Upgrade Check...");
                initActions.Add(newAction);

                // movies
                newAction = new WorkerDelegate(DatabaseMaintenanceManager.PerformMovieInformationUpgradeCheck);
                actionDescriptions.Add(newAction, "Performing Movie Information Upgrade Check...");
                initActions.Add(newAction);

            }            

            newAction = new WorkerDelegate(checkVersionInfo);
            actionDescriptions.Add(newAction, "Initializing Version Information...");
            initActions.Add(newAction);

            newAction = new WorkerDelegate(DataProviderManager.Initialize);
            actionDescriptions.Add(newAction, "Initializing Data Provider Manager...");
            initActions.Add(newAction);

            newAction = new WorkerDelegate(DatabaseMaintenanceManager.VerifyMovieInformation);
            actionDescriptions.Add(newAction, "Updating Movie Information...");
            initActions.Add(newAction);

            newAction = new WorkerDelegate(DatabaseMaintenanceManager.VerifyFilterMenu);
            actionDescriptions.Add(newAction, "Updating Filtering Menu...");
            initActions.Add(newAction);

            newAction = new WorkerDelegate(DatabaseMaintenanceManager.VerifyCategoryMenu);
            actionDescriptions.Add(newAction, "Updating Categories Menu...");
            initActions.Add(newAction);            

            newAction = new WorkerDelegate(DeviceManager.StartMonitor);
            actionDescriptions.Add(newAction, "Starting Device Monitor...");
            initActions.Add(newAction);

            // load all the above actions and notify any listeners of our progress
            loadingProgress = 0;
            loadingTotal = initActions.Count;
            foreach (WorkerDelegate currAction in initActions) {
                try {
                    if (InitializeProgress != null) InitializeProgress(actionDescriptions[currAction], (int)(loadingProgress * 100 / loadingTotal));
                    loadingProgressDescription = actionDescriptions[currAction];
                    currAction();
                }
                catch (Exception ex) {
                    // don't log error if the init was aborted on purpose
                    if (ex.GetType() == typeof(ThreadAbortException))
                        throw ex;

                    logger.ErrorException("Error: ", ex);
                }
                finally {
                    loadingProgress++;
                }
            }

            if (InitializeProgress != null) InitializeProgress("Done!", 100);
            
            // stop listening
            DatabaseMaintenanceManager.MaintenanceProgress -= new ProgressDelegate(DatabaseMaintenanceManager_MaintenanceProgress);

            // Launch background tasks
            launchBackgroundTasks();

        }

        static void DatabaseMaintenanceManager_MaintenanceProgress(string actionName, int percentDone) {
            int baseProgress = (int)(loadingProgress * 100 / loadingTotal);
            if (InitializeProgress != null) InitializeProgress(loadingProgressDescription, baseProgress + (int)((float)percentDone / loadingTotal));
        }

        public static void Shutdown() {
            DeviceManager.StopMonitor();
            
            // Stop Importer
            if (_importer != null)
                _importer.Stop();

            // Cancel background processes
            if (_processManager != null)
                _processManager.CancelAllProcesses();

            _processManager = null;
            _importer = null;
            _settings = null;
            _databaseManager = null;

            logger.Info("Plugin Closed");
        }

        #endregion

        #region Private Methods

        // Initializes the database connection to the Movies Plugin database
        private static void initDB() {
            if (_databaseManager != null)
                return;

            string fullDBFileName = Config.GetFile(Config.Dir.Database, dbFileName);
            _databaseManager = new DatabaseManager(fullDBFileName);

            // check that we at least have a default user
            List<DBUser> users = DBUser.GetAll();
            if (users.Count == 0) {
                DBUser defaultUser = new DBUser();
                defaultUser.Name = "Default User";
                defaultUser.Commit();
            }

            // add all filter helpers
            _databaseManager.AddFilterHelper<DBMovieInfo>(new FilterHelperDBMovieInfo());
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
            MediaPortal.Profile.Settings xmlreader = MediaPortalSettings;
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

        //for production, replace all references in this method from "SettingsManagerNew" to "SettingsManager"
        private static void initAdditionalSettings() {
            if (Settings.CoverArtFolder.Trim() == "")
                Settings.CoverArtFolder = Config.GetFolder(Config.Dir.Thumbs) + "\\MovingPictures\\Covers\\FullSize";

            // create the covers folder if it doesn't already exist
            if (!Directory.Exists(Settings.CoverArtFolder))
                Directory.CreateDirectory(Settings.CoverArtFolder);

            if (Settings.CoverArtThumbsFolder.Trim() == "")
                Settings.CoverArtThumbsFolder = Config.GetFolder(Config.Dir.Thumbs) + "\\MovingPictures\\Covers\\Thumbs";

            // create the thumbs folder if it doesn't already exist
            if (!Directory.Exists(Settings.CoverArtThumbsFolder))
                Directory.CreateDirectory(Settings.CoverArtThumbsFolder);

            if (Settings.BackdropFolder.Trim() == "")
                Settings.BackdropFolder = Config.GetFolder(Config.Dir.Thumbs) + "\\MovingPictures\\Backdrops\\FullSize";

            // create the backdrop folder if it doesn't already exist
            if (!Directory.Exists(Settings.BackdropFolder))
                Directory.CreateDirectory(Settings.BackdropFolder);

            if (Settings.BackdropThumbsFolder.Trim() == "")
                Settings.BackdropThumbsFolder = Config.GetFolder(Config.Dir.Thumbs) + "\\MovingPictures\\Backdrops\\Thumbs";

            // create the backdrop thumbs folder if it doesn't already exist
            if (!Directory.Exists(Settings.BackdropThumbsFolder))
                Directory.CreateDirectory(Settings.BackdropThumbsFolder);
        }

        private static void launchBackgroundTasks() {
            logger.Info("Launching Background Processes...");
            ProcessManager.StartProcess(new MediaInfoUpdateProcess());
            ProcessManager.StartProcess(new UpdateArtworkProcess());
        }

        private static void checkVersionInfo() {
            // check if the version changed, and update the DB accordingly
            Version realVer = Assembly.GetExecutingAssembly().GetName().Version;

            if (realVer > GetDBVersionNumber()) {
                Settings.Version = realVer.ToString();
                Settings.DataProvidersInitialized = false;
            }
        }

        public static Version GetDBVersionNumber() {
            return new Version(Settings.Version);
        }

        #endregion
    }
}
