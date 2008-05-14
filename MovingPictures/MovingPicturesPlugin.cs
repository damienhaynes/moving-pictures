using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using MediaPortal.GUI.Library;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.DataProviders;
using System.Xml;
using MediaPortal.Plugins.MovingPictures.ConfigScreen;
using MediaPortal.Configuration;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using System.Threading;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;
using System.ComponentModel;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Reflection;

namespace MediaPortal.Plugins.MovingPictures {
    public class MovingPicturesPlugin: ISetupForm {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private const string dbFileName = "movingpictures.db3";
        private const string logFileName = "movingpictures.log";

        // THe MovieImporter object that should be used by all components of
        // the plugin.
        public static MovieImporter Importer {
            get {
                if (importer == null)
                    importer = new MovieImporter();

                return importer;
            }
        }
        private static MovieImporter importer;

        // The DatabaseManager that should be used by all components of the plugin.       
        public static DatabaseManager DatabaseManager {
            get {
                if (databaseManager == null) 
                    initDB();

                return databaseManager;
            }
        } 
        private static DatabaseManager databaseManager;

        // The SettingsManager that should be used by all components of the plugin.
        public static SettingsManager SettingsManager {
            get {
                if (settingsManager == null)
                    initSettings();

                return settingsManager;
            }
        }
        private static SettingsManager settingsManager;

        public static IMovieProvider MovieProvider {
            get {
                if (_dataProvider == null)
                    _dataProvider = new MovieXMLProvider();

                return _dataProvider;
            }
        } private static IMovieProvider _dataProvider = null;

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
            fileTarget.Layout = "${date:format=dd-MMM-yyyy HH\\:mm\\:ss} " +
                                "${level:fixedLength=true:padding=5} " +
                                "[${logger:fixedLength=true:padding=20:shortName=true}]: ${message}";

            config.AddTarget("file", fileTarget);

            LoggingRule rule = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config; 
        }

        private void initAdditionalSettings() {
            if (((String)SettingsManager["cover_art_folder"].Value).Trim().Equals(""))
                SettingsManager["cover_art_folder"].Value = Config.GetFolder(Config.Dir.Thumbs) + "\\MovingPictures\\Covers";

            // create the covers folder if it doesn't already exist
            if (!Directory.Exists((string)SettingsManager["cover_art_folder"].Value))
                Directory.CreateDirectory((string)SettingsManager["cover_art_folder"].Value);

            if (((String)SettingsManager["cover_thumbs_folder"].Value).Trim().Equals(""))
                SettingsManager["cover_thumbs_folder"].Value = Config.GetFolder(Config.Dir.Thumbs) + "\\MovingPictures\\Thumbs";

            // create the thumbs folder if it doesn't already exist
            if (!Directory.Exists((string)SettingsManager["cover_thumbs_folder"].Value))
                Directory.CreateDirectory((string)SettingsManager["cover_thumbs_folder"].Value);
        }

        public MovingPicturesPlugin() {
            initLogger();
            
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            logger.Info("Moving Pictures (" + ver.Major + "." + ver.Minor + "." + ver.Build + ":" + ver.Revision +")");
            logger.Info("Plugin Lanched");

            initDB();
            initAdditionalSettings();
        }

        ~MovingPicturesPlugin() {
            logger.Info("Plugin Closed");
        }

        #region ISetupForm Members

        // Returns the name of the plugin which is shown in the plugin menu
        public string PluginName() {
            return "MoviesPlugin";
        }

        // Returns the description of the plugin is shown in the plugin menu
        public string Description() {
            return "Movies Plugin";
        }

        // Returns the author of the plugin which is shown in the plugin menu
        public string Author() {
            return "John";
        }

        // show the setup dialog
        public void ShowPlugin() {
            //Thread.Sleep(1000);
            MoviesPluginConfig configScr = new MoviesPluginConfig();
            //List<DBMovieInfo> movieList = DBMovieInfo.GetAll();
            configScr.ShowDialog();
        }

        // Indicates whether plugin can be enabled/disabled
        public bool CanEnable() {
            return true;
        }

        // get ID of windowplugin belonging to this setup
        public int GetWindowId() {
            return 96742;
        }

        // Indicates if plugin is enabled by default;
        public bool DefaultEnabled() {
            return true;
        }

        // indicates if a plugin has its own setup screen
        public bool HasSetup() {
            return true;
        }

        /// <summary>
        /// If the plugin should have its own button on the main menu of Media Portal then it
        /// should return true to this method, otherwise if it should not be on home
        /// it should return false
        /// </summary>
        /// <param name="strButtonText">text the button should have</param>
        /// <param name="strButtonImage">image for the button, or empty for default</param>
        /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
        /// <param name="strPictureImage">subpicture for the button or empty for none</param>
        /// <returns>true  : plugin needs its own button on home
        ///          false : plugin does not need its own button on home</returns>
        public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage) {
            strButtonText = String.Empty;
            strButtonImage = String.Empty;
            strButtonImageFocus = String.Empty;
            strPictureImage = String.Empty;
            return false;
        }
        #endregion

    }
}
