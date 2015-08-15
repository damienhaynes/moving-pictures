using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Plugins.MovingPictures.ConfigScreen;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using NLog;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;
using MediaPortal.Configuration;

namespace MediaPortal.Plugins.MovingPictures.MainUI {
    //MediaPortal.Plugins.MovingPictures.Properties.Resources.mp_config_icon
    [PluginIcons("MediaPortal.Plugins.MovingPictures.Resources.Images.icon_normal.png", "MediaPortal.Plugins.MovingPictures.Resources.Images.icon_faded.png")]
    public class ConfigConnector: ISetupForm {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region ISetupForm Members

        // Returns the name of the plugin which is shown in the plugin menu                                                                    
        public string PluginName() {
            return "Moving Pictures";
        }

        // Returns the description of the plugin is shown in the plugin menu                                                                   
        public string Description() {
            return "A comprehensive movie management plug-in.";
        }

        // Returns the author of the plugin which is shown in the plugin menu                                                                  
        public string Author() {
            return "John Conrad (fforde), Armand Pondman (armandp), Damien Haynes (ltfearme)";
        }

        // show the setup dialog                                                                                                               
        public void ShowPlugin() {

            MovingPicturesConfig configScr;

            try {
                LoadingPopup loadingPopup = new LoadingPopup();

                Thread initThread = new Thread(new ThreadStart(MovingPicturesCore.Initialize));
                initThread.IsBackground = true;
                initThread.Start();
                loadingPopup.ShowDialog();
                configScr = new MovingPicturesConfig();
            }
            catch (Exception e) {
                logger.FatalException("Unexpected error from plug-in initialization!", e);
                return;
            }
            
            try {
                configScr.ShowDialog();
            }
            catch (Exception e) {
                MessageBox.Show("There was an unexpected error in the Moving Pictures Configuration screen!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                logger.FatalException("Unexpected error from the Configuration Screen!", e);
                return;
            }

            try {
                MovingPicturesCore.Shutdown();
            }
            catch (Exception e) {
                logger.FatalException("Unexpected error from plug-in shutdown!", e);
            }

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
            strButtonText = MovingPicturesCore.Settings.HomeScreenName;
            strButtonImage = String.Empty;
            strButtonImageFocus = String.Empty;
            strPictureImage = "hover_moving pictures.png";
            return true;
        }
        #endregion                                                                                                                             
                                                                                                                                               

    }
}
