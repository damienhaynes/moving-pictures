using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;
using MediaPortal.Plugins.MovingPictures.DataProviders;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class ArtworkSettingsPane : UserControl {
        public ArtworkSettingsPane() {
            InitializeComponent();
            
            // if we are in designer, break to prevent errors with rendering, it cant access the DB...
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            coverFromMovieFolderCheckBox.Setting = MovingPicturesCore.Settings["local_cover_from_movie_folder"];
            coverFromMovieFolderCheckBox.IgnoreSettingName = true;
            coverPatternTextBox.Setting = MovingPicturesCore.Settings["local_moviefolder_coverart_pattern"];

            backdropFromMovieFolderCheckBox.Setting = MovingPicturesCore.Settings["local_backdrop_from_movie_folder"];
            backdropFromMovieFolderCheckBox.IgnoreSettingName = true;
            backdropPatternTextBox.Setting = MovingPicturesCore.Settings["local_moviefolder_backdrop_pattern"];

            coverCountTextBox.Setting = MovingPicturesCore.Settings["max_covers_per_movie"];
        }

        private void ArtworkSettingsPane_Load(object sender, EventArgs e) {
            updateGUI();
        }

        private void coverFromMovieFolderCheckBox_CheckedChanged(object sender, EventArgs e) {
            updateGUI();
        }

        private void backdropFromMovieFolderCheckBox_CheckedChanged(object sender, EventArgs e) {
            updateGUI();
        }

        private void updateGUI() {
            coverPatternTextBox.Enabled = coverFromMovieFolderCheckBox.Checked;
            backdropPatternTextBox.Enabled = backdropFromMovieFolderCheckBox.Checked;
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
    }
}
