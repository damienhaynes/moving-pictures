using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;
using System.Reflection;
using System.Diagnostics;
using MediaPortal.Plugins.MovingPictures.Properties;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class AboutPane : UserControl {
        public AboutPane() {
            InitializeComponent();

        }

        private void advancedSettingsButton_Click(object sender, EventArgs e) {
            AdvancedSettingsPopup popup = new AdvancedSettingsPopup();
            popup.Owner = ParentForm;
            popup.ShowDialog();
        }

        private void websiteLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            ProcessStartInfo process = new ProcessStartInfo("http://www.moving-pictures.tv");
            Process.Start(process);
        }

        private void codeLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            ProcessStartInfo process = new ProcessStartInfo("http://moving-pictures.googlecode.com");
            Process.Start(process);
        }

        private void forumLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            ProcessStartInfo process = new ProcessStartInfo("http://forum.team-mediaportal.com/moving-pictures-284/");
            Process.Start(process);
        }

        private void getInvolvedButton_Click(object sender, EventArgs e) {
            ProcessStartInfo processInfo = new ProcessStartInfo(Resources.GetInvolvedURL);
            Process.Start(processInfo);
        }

        private void donateButton_Click(object sender, EventArgs e) {
            ProcessStartInfo processInfo = new ProcessStartInfo(Resources.DonateURL);
            Process.Start(processInfo);
        }

    }
}
