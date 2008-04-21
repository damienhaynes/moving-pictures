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
    public partial class MoviesPluginConfig : Form {
        public MoviesPluginConfig() {
            InitializeComponent();

            // if we are in designer, break to prevent errors with rendering, it cant access the DB...
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            advancedSettingsPane.populateTree(MovingPicturesPlugin.SettingsManager);
        }

        private void goButton_Click(object sender, EventArgs e) {
            
        }
    }
}