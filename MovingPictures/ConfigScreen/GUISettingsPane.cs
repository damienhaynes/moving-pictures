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
using Cornerstone.GUI.Dialogs;
using Cornerstone.GUI;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class GUISettingsPane : UserControl {

        private DBMenu<DBMovieInfo> categoriesMenu;
        private DBMenu<DBMovieInfo> filtersMenu;
        
        DBSetting clickGoesToDetails;
        DBSetting dvdInsertedAction;
        DBSetting defaultView;

        MenuEditorPopup menuEditorPopup = null;

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
            categoriesCheckBox.Setting = MovingPicturesCore.Settings["enable_categories"];

            passwordTextBox.Enabled = parentalControlsCheckBox.Checked;
            parentalContolsButton.Enabled = parentalControlsCheckBox.Checked;
            defineCategoriesButton.Enabled = categoriesCheckBox.Checked;
 
            sortFieldComboBox.Setting = MovingPicturesCore.Settings["default_sort_field"];
            sortFieldComboBox.EnumType = typeof(SortingFields);

            clickGoesToDetails = MovingPicturesCore.Settings["click_to_details"];
            dvdInsertedAction = MovingPicturesCore.Settings["on_disc_loaded"];
            defaultView = MovingPicturesCore.Settings["default_view"];


        }

        private void GUISettingsPane_Load(object sender, EventArgs e) {
            if (DesignMode)
                return;

            // default view init
            if (defaultView.StringValue.Equals("lastused"))
                defaultViewComboBox.SelectedIndex = 0;
            else if (defaultView.StringValue.Equals("list"))
                defaultViewComboBox.SelectedIndex = 1;
            else if (defaultView.StringValue.Equals("thumbs"))
                defaultViewComboBox.SelectedIndex = 2;
            else if (defaultView.StringValue.Equals("largethumbs"))
                defaultViewComboBox.SelectedIndex = 3;
            else if (defaultView.StringValue.Equals("filmstrip"))
                defaultViewComboBox.SelectedIndex = 4;

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
                defaultView.Value = "lastused";
            if (defaultViewComboBox.SelectedIndex == 1)
                defaultView.Value = "list";
            if (defaultViewComboBox.SelectedIndex == 2)
                defaultView.Value = "thumbs";
            if (defaultViewComboBox.SelectedIndex == 3)
                defaultView.Value = "largethumbs";
            if (defaultViewComboBox.SelectedIndex == 4)
                defaultView.Value = "filmstrip";
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
            MovieFilterEditorPopup popup = new MovieFilterEditorPopup();

            // attach the filter, show the popup, and if necisarry, save the results
            popup.FilterPane.AttachedFilter = MovingPicturesCore.Settings.ParentalControlsFilter;
            popup.ShowHelpButton = true;
            popup.HelpAction = new HelpActionDelegate(delegate {
                ProcessStartInfo processInfo = new ProcessStartInfo(Resources.ParentalControlsURL);
                Process.Start(processInfo);
            });

            popup.ShowDialog();
            MovingPicturesCore.Settings.ParentalControlsFilter.Commit();
        }

        private void parentalControlsCheckBox_CheckedChanged(object sender, EventArgs e) {
            passwordTextBox.Enabled = parentalControlsCheckBox.Checked;
            parentalContolsButton.Enabled = parentalControlsCheckBox.Checked;
        }

        private void categoriesMenuButton_Click(object sender, EventArgs e) {
            menuEditorPopup = new MenuEditorPopup();
            menuEditorPopup.MenuTree.FieldDisplaySettings.Table = typeof(DBMovieInfo);

            categoriesMenu = null;
            ProgressPopup loadingPopup = new ProgressPopup(new WorkerDelegate(loadCategoriesMenu));
            loadingPopup.Owner = FindForm();
            loadingPopup.Text = "Loading Menu...";
            loadingPopup.ShowDialog();
       
            menuEditorPopup.ShowDialog();
            menuEditorPopup = null;

            MovingPicturesCore.DatabaseManager.BeginTransaction();
            ProgressPopup savingPopup = new ProgressPopup(new WorkerDelegate(categoriesMenu.Commit));
            savingPopup.Owner = FindForm();
            savingPopup.Text = "Saving Menu...";
            savingPopup.ShowDialog();
            MovingPicturesCore.DatabaseManager.EndTransaction();
        }

        private void loadCategoriesMenu() {
            // grab or create the menu object for categories
            string menuID = MovingPicturesCore.Settings.CategoriesMenuID;
            if (menuID == "null") {
                categoriesMenu = new DBMenu<DBMovieInfo>();
                categoriesMenu.Name = "Categories Menu";
                MovingPicturesCore.DatabaseManager.Commit(categoriesMenu);
                MovingPicturesCore.Settings.CategoriesMenuID = categoriesMenu.ID.ToString();
            }
            else {
                categoriesMenu = MovingPicturesCore.DatabaseManager.Get<DBMenu<DBMovieInfo>>(int.Parse(menuID));
            }

            menuEditorPopup.MenuTree.Menu = categoriesMenu;
        }


        // This event occurs after the KeyDown event and can be used to prevent
        // characters from entering the control.
        private void passwordTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e) {
            if ((!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))) {
                e.Handled = true; // suppress

                Point pos = passwordTextBox.Location;
                pos.Y += 25;
                Help.ShowPopup(this, "Password must be a four digit number.", PointToScreen(pos));
            }
        }

        private void passwordTextBox_TextChanged(object sender, EventArgs e) {
            if (passwordTextBox.Text.Length < 4)
                passwordTextBox.ForeColor = Color.Red;
            else
                passwordTextBox.ForeColor = SystemColors.ControlText;
        }

        private void filterMenuButton_Click(object sender, EventArgs e) {
            menuEditorPopup = new MenuEditorPopup();
            menuEditorPopup.MenuTree.FieldDisplaySettings.Table = typeof(DBMovieInfo);

            categoriesMenu = null;
            ProgressPopup loadingPopup = new ProgressPopup(new WorkerDelegate(loadFiltersMenu));
            loadingPopup.Owner = FindForm();
            loadingPopup.Text = "Loading Menu...";
            loadingPopup.ShowDialog();

            menuEditorPopup.ShowMovieNodeSettings = false;
            menuEditorPopup.ShowDialog();
            menuEditorPopup = null;

            MovingPicturesCore.DatabaseManager.BeginTransaction();
            ProgressPopup savingPopup = new ProgressPopup(new WorkerDelegate(filtersMenu.Commit));
            savingPopup.Owner = FindForm();
            savingPopup.Text = "Saving Menu...";
            savingPopup.ShowDialog();
            MovingPicturesCore.DatabaseManager.EndTransaction();
        }

        private void loadFiltersMenu() {
            filtersMenu = MovingPicturesCore.Settings.FilterMenu;
            menuEditorPopup.MenuTree.Menu = filtersMenu;
        }

        private void categoriesCheckBox_CheckedChanged(object sender, EventArgs e) {
            defineCategoriesButton.Enabled = categoriesCheckBox.Checked;
        }
    }
}
