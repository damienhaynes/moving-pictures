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
using Win32.Utils.Cd;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class MovingPicturesConfig : Form {
        
        private DeviceVolumeMonitor deviceMonitor;
     
        public MovingPicturesConfig() {
            InitializeComponent();

            // if we are in designer, break to prevent errors with rendering, it cant access the DB...
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            // todo: possible MePo dependancy, check if this is mediaportal customized or common library.
            deviceMonitor = new DeviceVolumeMonitor(this.Handle);
            deviceMonitor.OnVolumeInserted += new DeviceVolumeAction(onVolumeInserted);
            deviceMonitor.OnVolumeRemoved += new DeviceVolumeAction(onVolumeRemoved);
            deviceMonitor.AsynchronousEvents = true;
            deviceMonitor.Enabled = true;

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

        private void MovingPicturesConfig_FormClosing(object sender, FormClosedEventArgs e) {
            if (!DesignMode) {
                ProgressPopup popup = new ProgressPopup(new ProgressPopup.WorkerDelegate(movieManagerPane1.Commit));
                popup.Owner = this;
                popup.ShowDialog();
            }
        }


        private void onVolumeRemoved(int bitMask) {
            string driveLetter = deviceMonitor.MaskToLogicalPaths(bitMask);
            // Notify the importer
            MovingPicturesCore.Importer.OnVolumeRemoved(driveLetter);
        }


        private void onVolumeInserted(int bitMask) {
            string driveLetter = deviceMonitor.MaskToLogicalPaths(bitMask);
            // Notify the importer
            MovingPicturesCore.Importer.OnVolumeInserted(driveLetter);
        }

    }
}