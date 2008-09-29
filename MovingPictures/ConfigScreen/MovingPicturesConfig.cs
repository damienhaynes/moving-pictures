using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class MovingPicturesConfig : Form {
        public MovingPicturesConfig() {
            InitializeComponent();

            // if we are in designer, break to prevent errors with rendering, it cant access the DB...
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            advancedSettingsWarningPane1.SettingsPane.populateTree(MovingPicturesCore.SettingsManager);
        }

        private void MoviesPluginConfig_Load(object sender, EventArgs e) {
            // if we start on the first tab, we need to
            // manually flip to the second tab (media importer) very quickly 
            // to force it to load into memory
            // mainTabControl.SelectedIndex = 1;
            // mainTabControl.SelectedIndex = 0;

            mainTabControl.SelectedIndex = 1;
        }
    }
}