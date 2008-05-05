using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    public partial class SettingPopup : Form {
        Color validColor;
        Color invalidColor;
        

        public SettingPopup() {
            InitializeComponent();

            invalidColor = Color.Red;
            validColor = valueTextBox.ForeColor;
        }

        // Sets the DBSetting this popup will be modifying
        public DBSetting Setting { 
            get {
                return _setting;
            }
            set {
                _setting = value;
                updateDialog();
            }
        } DBSetting _setting;

        private void updateDialog() {
            if (Setting == null)
                return;

            // Set the title of the dialog
            string title = "";
            for (int i = 0; i < Setting.Grouping.Count; i++) {
                title += Setting.Grouping[i];
                if (i+1 < Setting.Grouping.Count)
                    title += " | ";
            }
            this.Text = title;

            // set the setting name
            settingNameLabel.Text = Setting.Name;

            // set the description
            descriptionLabel.Text = Setting.Description;

            // set the value
            valueTextBox.Text = Setting.StringValue;
        }

        // returns true if the value in the text box is a valid value for the setting
        // represented by this popup.
        private bool valueIsValid() {
            return Setting.Validate(valueTextBox.Text);
        }

        // updates the color of the textbox text based on if the value is valid for the current setting
        private void valueTextBox_TextChanged(object sender, EventArgs e) {
            if (valueIsValid()) {
                valueTextBox.ForeColor = validColor;
                okButton.Enabled = true;
            } else {
                valueTextBox.ForeColor = invalidColor;
                okButton.Enabled = false;
            }
        }

        private void okButton_Click(object sender, EventArgs e) {
            if (valueIsValid()) {
                Setting.Value = valueTextBox.Text;
                MovingPicturesPlugin.DatabaseManager.Commit(Setting);
            }

            Close();
        }
    }
}
