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

namespace MediaPortal.Plugins.MovingPictures {
    public class MovingPicturesCore {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private const string dbFileName = "movingpictures.db3";
        private const string logFileName = "movingpictures.log";

        #region Properties
        
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

        #endregion

        #region Public Methods

        static MovingPicturesCore() {
            initLogger();
        }

        public static bool Initialize()
        {
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            logger.Info("Moving Pictures (" + ver.Major + "." + ver.Minor + "." + ver.Build + ":" + ver.Revision + ")");
            logger.Info("Plugin Launched");

            initDB();
            initAdditionalSettings();
            Importer.VerifyUserMovieSettings();

            DataProviderManager.GetInstance();

            return true;
        }

        public static void Shutdown()
        {
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
                if (logFile.Exists)
                    logFile.Delete();
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
            Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"));
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

        #endregion
    }
}