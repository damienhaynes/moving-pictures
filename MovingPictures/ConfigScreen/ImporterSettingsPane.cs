using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.DataProviders;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;
using MediaPortal.Plugins.MovingPictures.Properties;
using System.Diagnostics;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class ImporterSettingsPane : UserControl {
        DBSetting autoApproval;
        
        public ImporterSettingsPane() {
            InitializeComponent();

            // if we are in designer, break to prevent errors with rendering, it cant access the DB...
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            autoApproval = MovingPicturesCore.Settings["importer_autoapprove"];

            settingAutoApproveAlternateTitle.Setting = MovingPicturesCore.Settings["importer_autoapprove_alternate_titles"];
            settingAutoApproveAlternateTitle.IgnoreSettingName = true;

            preferFolderCheckBox.Setting = MovingPicturesCore.Settings["importer_prefer_foldername"];
            folderGroupingCheckBox.Setting = MovingPicturesCore.Settings["importer_groupfolder"];
            nfoExtTextBox.Setting = MovingPicturesCore.Settings["importer_nfoext"];

            nfoScannerCheckBox.Setting = MovingPicturesCore.Settings["importer_nfoscan"]; 
            nfoScannerCheckBox.IgnoreSettingName = true;

            nfoAutoApproveCheckBox.Setting = MovingPicturesCore.Settings["importer_autoimdb"];
            nfoAutoApproveCheckBox.IgnoreSettingName = true;

            coverFromMovieFolderCheckBox.Setting = MovingPicturesCore.Settings["local_cover_from_movie_folder"];
            coverFromMovieFolderCheckBox.IgnoreSettingName = true;
            coverPatternTextBox.Setting = MovingPicturesCore.Settings["local_moviefolder_coverart_pattern"];

            backdropFromMovieFolderCheckBox.Setting = MovingPicturesCore.Settings["local_backdrop_from_movie_folder"];
            backdropFromMovieFolderCheckBox.IgnoreSettingName = true;
            backdropPatternTextBox.Setting = MovingPicturesCore.Settings["local_moviefolder_backdrop_pattern"];

            coverCountTextBox.Setting = MovingPicturesCore.Settings["max_covers_per_movie"];
            
            autoRescanCheckBox.Setting = MovingPicturesCore.Settings["importer_rescan_enabled"];
            rescanIntervalTextBox.Setting = MovingPicturesCore.Settings["importer_rescan_interval"];

            MovingPicturesCore.Settings.SettingChanged += new Cornerstone.Database.SettingChangedDelegate(Settings_SettingChanged);
        }

        private void ImporterSettingsPane_Load(object sender, EventArgs e) {
            if (DesignMode)
                return;

            autoApproveTrackBar.Value = (int)autoApproval.Value;

            // this is a conditional that is true under vista. the trackbar backcolor is wrong under
            // vista just haven't figured out the right color yet....
            if (Environment.OSVersion.Version.Major == 6)
                autoApproveTrackBar.BackColor = SystemColors.Window;

            updateGUI();
        }

        void Settings_SettingChanged(Cornerstone.Database.Tables.DBSetting setting, object oldValue) {
            if (setting.Key == "dataprovider_management") {
                updateGUI();
            }
        }


        private void coverFromMovieFolderCheckBox_CheckedChanged(object sender, EventArgs e) {
            updateGUI();
        }

        private void backdropFromMovieFolderCheckBox_CheckedChanged(object sender, EventArgs e) {
            updateGUI();
        }

        private void coverDataSources_Click(object sender, EventArgs e) {
            DataSourcesPopup popup = new DataSourcesPopup();
            popup.DataSourcePane.DisplayType = DataType.COVERS;
            popup.Owner = ParentForm;
            popup.ShowDialog();
        }

        private void backdropSourcesButton_Click(object sender, EventArgs e) {
            DataSourcesPopup popup = new DataSourcesPopup();
            popup.DataSourcePane.DisplayType = DataType.BACKDROPS;
            popup.Owner = ParentForm;
            popup.ShowDialog();
        }

        private void autoApproveTrackBar_Scroll(object sender, EventArgs e) {
            autoApproval.Value = autoApproveTrackBar.Value;
            autoApproval.Commit();
            
            Point pos = Cursor.Position;
            pos.Y += 25;

            switch (autoApproveTrackBar.Value) {
                case 0:
                    Help.ShowPopup(this, "Low", pos);
                    break;
                case 1:
                    Help.ShowPopup(this, "Medium", pos);
                    break;
                case 2:
                    Help.ShowPopup(this, "High", pos);
                    break;
                case 3:
                    Help.ShowPopup(this, "Reckless!", pos);
                    break;
            }

            
        }

        private void nfoScannerCheckBox_CheckedChanged(object sender, EventArgs e) {
            updateGUI();
        }

        private void detailsButton_Click(object sender, EventArgs e) {
            DataSourcesPopup popup = new DataSourcesPopup();
            popup.DataSourcePane.DisplayType = DataType.DETAILS;
            popup.Owner = ParentForm;
            popup.ShowDialog();
        }

        private void updateGUI() {
            if (nfoScannerCheckBox.Checked) {
                nfoExtTextBox.Enabled = true;
                nfoAutoApproveCheckBox.Enabled = true;
            }
            else {
                nfoExtTextBox.Enabled = false;
                nfoAutoApproveCheckBox.Enabled = false;
            }

            rescanIntervalTextBox.Enabled = autoRescanCheckBox.Checked;            

            if (!DesignMode) {
                coverPatternTextBox.Enabled = coverFromMovieFolderCheckBox.Checked;
                backdropPatternTextBox.Enabled = backdropFromMovieFolderCheckBox.Checked;
            }

            bool manualManagement = MovingPicturesCore.Settings.DataProviderManagementMethod == "manual";

            coverDataSources.Enabled = manualManagement;
            backdropSourcesButton.Enabled = manualManagement;
            detailsButton.Enabled = manualManagement;

        }

        private void helpButton1_Click(object sender, EventArgs e) {
            ProcessStartInfo processInfo = new ProcessStartInfo(Resources.ImporterSettingsHelpURL);
            Process.Start(processInfo);
        }

        private void autoRescanCheckBox_CheckedChanged(object sender, EventArgs e) {
            updateGUI();
        }

        private void rescanHelpLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Help.ShowPopup(this, GUISettingsPane.RescanHelpMessage, Cursor.Position);
        }

    }
}
