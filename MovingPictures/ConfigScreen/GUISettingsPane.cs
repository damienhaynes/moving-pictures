using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.MainUI.MovieBrowser;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class GUISettingsPane : UserControl {

        DBSetting clickGoesToDetails;
        DBSetting dvdInsertedAction;
        DBSetting defaultView;
        DBSetting watchedFilterStartsOn;

        public GUISettingsPane() {
            InitializeComponent();

            // if we are in designer, break to prevent errors with rendering, it cant access the DB...
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            homeScreenTextBox.Setting = MovingPicturesCore.SettingsManager["home_name"];
            watchedPercentTextBox.Setting = MovingPicturesCore.SettingsManager["gui_watch_percentage"];
            remoteControlCheckBox.Setting = MovingPicturesCore.SettingsManager["enable_rc_filter"];

            sortFieldComboBox.Setting = MovingPicturesCore.SettingsManager["default_sort_field"];
            sortFieldComboBox.EnumType = typeof(GUIListItemMovieComparer.SortingFields);

            sortDirComboBox.Setting = MovingPicturesCore.SettingsManager["default_sort_direction"];
            sortDirComboBox.EnumType = typeof(GUIListItemMovieComparer.SortingDirections);

            clickGoesToDetails = MovingPicturesCore.SettingsManager["click_to_details"];
            dvdInsertedAction = MovingPicturesCore.SettingsManager["on_disc_loaded"];
            defaultView = MovingPicturesCore.SettingsManager["default_view"];
            watchedFilterStartsOn = MovingPicturesCore.SettingsManager["start_watched_filter_on"];
        }

        private void GUISettingsPane_Load(object sender, EventArgs e) {
            if (DesignMode)
                return;

            // default view init
            if (defaultView.StringValue.Equals("list"))
                defaultViewComboBox.SelectedIndex = 0;
            else if (defaultView.StringValue.Equals("thumbs"))
                defaultViewComboBox.SelectedIndex = 1;
            else if (defaultView.StringValue.Equals("largethumbs"))
                defaultViewComboBox.SelectedIndex = 2;
            else if (defaultView.StringValue.Equals("filmstrip"))
                defaultViewComboBox.SelectedIndex = 3;

            // movie clicked action init
            if ((bool)clickGoesToDetails.Value)
                displayDetailsRadioButton.Checked = true;
            else
                playMovieRadioButton.Checked = true;

            // dvd insertion action init
            if (dvdInsertedAction.StringValue.Equals("PLAY"))
                playDVDradioButton.Checked = true;
            else if (dvdInsertedAction.StringValue.Equals("DETAILS"))
                displayDvdDetailsRadioButton.Checked = true;
            else if (dvdInsertedAction.StringValue.Equals("NOTHING"))
                doNothingRadioButton.Checked = true;

            // configure watched filter settings
            if ((bool)watchedFilterStartsOn.Value)
                watchedComboBox.SelectedIndex = 1;
            else
                watchedComboBox.SelectedIndex = 0;
        }

        #region Movie Clicked Action Checkboxes

        private void playMovieRadioButton_CheckedChanged(object sender, EventArgs e) {
            if (playMovieRadioButton.Checked) {
                clickGoesToDetails.Value = false;
                clickGoesToDetails.Commit();
            }
        }

        private void displayDetailsRadioButton_CheckedChanged(object sender, EventArgs e) {
            if (displayDetailsRadioButton.Checked) {
                clickGoesToDetails.Value = true;
                clickGoesToDetails.Commit();
            }
        }

        #endregion

        #region DVD Inserted Checkboxes

        private void playDVDradioButton_CheckedChanged(object sender, EventArgs e) {
            if (playDVDradioButton.Checked) {
                dvdInsertedAction.Value = "PLAY";
                dvdInsertedAction.Commit();
            }
        }

        private void displayDvdDetailsRadioButton_CheckedChanged(object sender, EventArgs e) {
            if (displayDvdDetailsRadioButton.Checked) {
                dvdInsertedAction.Value = "DETAILS";
                dvdInsertedAction.Commit();
            }
        }

        private void doNothingRadioButton_CheckedChanged(object sender, EventArgs e) {
            if (doNothingRadioButton.Checked) {
                dvdInsertedAction.Value = "NOTHING";
                dvdInsertedAction.Commit();
            }
        }

        #endregion

        private void defaultViewComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (defaultViewComboBox.SelectedIndex == 0)
                defaultView.Value = "list";
            if (defaultViewComboBox.SelectedIndex == 1)
                defaultView.Value = "thumbs";
            if (defaultViewComboBox.SelectedIndex == 2)
                defaultView.Value = "largethumbs";
            if (defaultViewComboBox.SelectedIndex == 3)
                defaultView.Value = "filmstrip";
        }

        private void watchedComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (watchedComboBox.SelectedIndex == 0)
                watchedFilterStartsOn.Value = false;
            if (watchedComboBox.SelectedIndex == 1)
                watchedFilterStartsOn.Value = true;
        }
    }
}
