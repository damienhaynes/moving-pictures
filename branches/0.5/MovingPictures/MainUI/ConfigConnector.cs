﻿using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Plugins.MovingPictures.ConfigScreen;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.MainUI {
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
            return "John";
        }

        // show the setup dialog                                                                                                               
        public void ShowPlugin() {
            try {
                MovingPicturesCore.Initialize();

                MovingPicturesConfig configScr = new MovingPicturesConfig();
                configScr.ShowDialog();

                MovingPicturesCore.Shutdown();
            }
            catch (Exception e) {
                logger.ErrorException("Unexpected error from the Configuration Screen!", e);
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
            strButtonText = "Moving Pictures";
            strButtonImage = String.Empty;
            strButtonImageFocus = String.Empty;
            strPictureImage = String.Empty;
            return true;
        }
        #endregion                                                                                                                             
                                                                                                                                               

    }
}
