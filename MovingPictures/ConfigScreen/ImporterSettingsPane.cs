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

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class ImporterSettingsPane : UserControl {
        DBSetting autoApproval;
        
        public ImporterSettingsPane() {
            InitializeComponent();

            // if we are in designer, break to prevent errors with rendering, it cant access the DB...
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            autoApproval = MovingPicturesCore.SettingsManager["importer_autoapprove"];

            strictYearCheckBox.Setting = MovingPicturesCore.SettingsManager["importer_strict_year"];
            preferFolderCheckBox.Setting = MovingPicturesCore.SettingsManager["importer_prefer_foldername"];
            folderGroupingCheckBox.Setting = MovingPicturesCore.SettingsManager["importer_groupfolder"];
            nfoExtTextBox.Setting = MovingPicturesCore.SettingsManager["importer_nfoext"];

            nfoScannerCheckBox.Setting = MovingPicturesCore.SettingsManager["importer_nfoscan"]; 
            nfoScannerCheckBox.IgnoreSettingName = true;

            nfoAutoApproveCheckBox.Setting = MovingPicturesCore.SettingsManager["importer_autoimdb"];
            nfoAutoApproveCheckBox.IgnoreSettingName = true;
        }

        private void ImporterSettingsPane_Load(object sender, EventArgs e) {
            if (DesignMode)
                return;

            autoApproveTrackBar.Value = (int)autoApproval.Value;

            updateGUI();
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
        }

    }
}
