using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;
using Cornerstone.GUI.Dialogs;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class MovingPicturesConfig : Form {      
    
        public MovingPicturesConfig() {
            InitializeComponent();

            #if DEBUG
            ShowInTaskbar = true;
            #endif

            // if we are in designer, break to prevent errors with rendering, it cant access the DB...
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;
        }

        private void MoviesPluginConfig_Load(object sender, EventArgs e) {

            // if we start on the first tab, we need to
            // manually flip to the second tab (media importer) very quickly 
            // to force it to load into memory
            // mainTabControl.SelectedIndex = 1;
            // mainTabControl.SelectedIndex = 0;

            mainTabControl.SelectedIndex = 1;
        }

        private void MovingPicturesConfig_FormClosing(object sender, FormClosedEventArgs e) {
            if (!DesignMode) {
                MovingPicturesCore.Importer.Stop();
                ProgressPopup popup = new ProgressPopup(new WorkerDelegate(movieManagerPane1.Commit));
                popup.Owner = this;
                popup.ShowDialog();
            }
        }

        private void MovingPicturesConfig_Shown(object sender, EventArgs e) {
            if (MovingPicturesCore.Settings.DataProviderManagementMethod == "undefined") {
                MovingPicturesCore.DataProviderManager.ArrangeDataProviders("en");
                DataProviderSetupPopup popup = new DataProviderSetupPopup();
                popup.Owner = this;
                popup.ShowDialog();
            }
        }        

    }
}