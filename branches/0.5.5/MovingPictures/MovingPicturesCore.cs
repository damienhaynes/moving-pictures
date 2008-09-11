using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MediaPortal.Configuration;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;
using MediaPortal.Plugins.MovingPictures.DataProviders;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
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

        public static IMovieProvider MovieProvider {
            get {
                if (dataProvider == null)
                    dataProvider = new MovieXMLProvider();

                return dataProvider;
            }
        } private static IMovieProvider dataProvider = null;

        public static ICoverArtProvider CoverProvider {
            get {
                if (coverProvider == null)
                    coverProvider = new MovieXMLProvider();

                return coverProvider;
            }
        } private static ICoverArtProvider coverProvider = null;

        public static IBackdropProvider BackdropProvider {
            get {
                if (backdropProvider == null)
                    backdropProvider = new MovieBackdropsProvider();

                return backdropProvider;
            }
        } private static IBackdropProvider backdropProvider = null;

        #endregion

        #region Public Methods

        public static bool Initialize()
        {
            initLogger();

            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            logger.Info("Moving Pictures (" + ver.Major + "." + ver.Minor + "." + ver.Build + ":" + ver.Revision + ")");
            logger.Info("Plugin Launched");

            initDB();
            initAdditionalSettings();


            // testing
            ScriptableDataProvider imdbProvider = new ScriptableDataProvider(Properties.Resources.imdb);
            List<object> resultList = imdbProvider.Execute("search", "Back to the Future");
            imdbProvider.Execute("details", resultList[0]);

            return true;
        }

        public static void Shutdown()
        {
            importer.Stop();
            settingsManager.Shutdown();
            
            dataProvider = null;
            coverProvider = null;
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

            FileTarget fileTarget = new FileTarget();
            fileTarget.FileName = Config.GetFile(Config.Dir.Log, logFileName);
            fileTarget.DeleteOldFileOnStartup = true;
            fileTarget.Layout = "${date:format=dd-MMM-yyyy HH\\:mm\\:ss} " +
                                "${level:fixedLength=true:padding=5} " +
                                "[${logger:fixedLength=true:padding=20:shortName=true}]: ${message} " +
                                "${exception:format=tostring}";

            config.AddTarget("file", fileTarget);

            LoggingRule rule = new LoggingRule("*", LogLevel.Debug, fileTarget);
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
