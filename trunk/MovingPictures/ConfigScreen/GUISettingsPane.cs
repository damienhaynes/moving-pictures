using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.MainUI.Filters;
using MediaPortal.Plugins.MovingPictures.Properties;
using System.Diagnostics;
using MediaPortal.Plugins.MovingPictures.MainUI;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;
using MediaPortal.Plugins.MovingPictures.Database;

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

            homeScreenTextBox.Setting = MovingPicturesCore.Settings["home_name"];
            watchedPercentTextBox.Setting = MovingPicturesCore.Settings["gui_watch_percentage"];
            remoteControlCheckBox.Setting = MovingPicturesCore.Settings["enable_rc_filter"];
            enableDeleteCheckBox.Setting = MovingPicturesCore.Settings["enable_delete_movie"];
            parentalControlsCheckBox.Setting = MovingPicturesCore.Settings["enable_parental_controls"];
            passwordTextBox.Setting = MovingPicturesCore.Settings["parental_controls_password"];

            passwordTextBox.Enabled = parentalControlsCheckBox.Checked;
            parentalContolsButton.Enabled = parentalControlsCheckBox.Checked;

            sortFieldComboBox.Setting = MovingPicturesCore.Settings["default_sort_field"];
            sortFieldComboBox.EnumType = typeof(SortingFields);

            clickGoesToDetails = MovingPicturesCore.Settings["click_to_details"];
            dvdInsertedAction = MovingPicturesCore.Settings["on_disc_loaded"];
            defaultView = MovingPicturesCore.Settings["default_view"];
            watchedFilterStartsOn = MovingPicturesCore.Settings["start_watched_filter_on"];


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

        private void helpButton_Click(object sender, EventArgs e) {
            ProcessStartInfo processInfo = new ProcessStartInfo(Resources.GuiSettingsHelpURL);
            Process.Start(processInfo);
        }

        private void remoteFilteringHelpLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            ProcessStartInfo processInfo = new ProcessStartInfo(Resources.RemoteFilteringHelpURL);
            Process.Start(processInfo);
        }

        private void parentalControlsButton_Click(object sender, EventArgs e) {
            FilterConfigPopup popup = new FilterConfigPopup();

            // grab or create the filter object attached to the parental controls
            DBFilter<DBMovieInfo> filter;
            string filterID = MovingPicturesCore.Settings.ParentalContolsFilterID;
            if (filterID == "null") {
                filter = new DBFilter<DBMovieInfo>();
                filter.Name = "Parental Controls Filter";
                MovingPicturesCore.DatabaseManager.Commit(filter);
                MovingPicturesCore.Settings.ParentalContolsFilterID = filter.ID.ToString();
            }
            else {
                filter = MovingPicturesCore.DatabaseManager.Get<DBFilter<DBMovieInfo>>(int.Parse(filterID));
            }

            // attach the filter, show the popup, and if necisarry, save the results
            popup.FilterPane.AttachedFilter = filter;
            popup.ShowDialog();
            filter.Commit();
        }

        private void parentalControlsCheckBox_CheckedChanged(object sender, EventArgs e) {
            passwordTextBox.Enabled = parentalControlsCheckBox.Checked;
            parentalContolsButton.Enabled = parentalControlsCheckBox.Checked;
        }

        private void button1_Click(object sender, EventArgs e) {
            MenuEditorPopup popup = new MenuEditorPopup();
            popup.MenuTree.FieldDisplaySettings.Table = typeof(DBMovieInfo);

            // grab or create the menu object for categories
            DBMenu<DBMovieInfo> menu;
            string menuID = MovingPicturesCore.Settings.CategoriesMenuID;
            if (menuID == "null") {
                menu = new DBMenu<DBMovieInfo>();
                menu.Name = "Categories Menu";
                MovingPicturesCore.DatabaseManager.Commit(menu);
                MovingPicturesCore.Settings.CategoriesMenuID = menu.ID.ToString();
            }
            else {
                menu = MovingPicturesCore.DatabaseManager.Get<DBMenu<DBMovieInfo>>(int.Parse(menuID));
            }

            popup.MenuTree.Menu = menu;
            popup.ShowDialog();
        }
    }
}
