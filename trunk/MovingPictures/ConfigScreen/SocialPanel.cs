using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MovingPicturesSocialAPI.UI;
using Cornerstone.GUI.Dialogs;
using MediaPortal.Plugins.MovingPictures.Database;
using MovingPicturesSocialAPI.Data;
using NLog;
using System.Diagnostics;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;
using Cornerstone.GUI;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class SocialPanel : UserControl {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public SocialPanel() {
            InitializeComponent();

            restrictSyncedMoviesCheckBox.Setting = MovingPicturesCore.Settings["mps_restrict_synched_movies"];

            UpdateControls();
        }

        private void ConnectMps() {
            MpsLoginForm loginForm = new MpsLoginForm();
            loginForm.ApiUrl = MovingPicturesCore.Settings.SocialUrl;

            DialogResult result = loginForm.ShowDialog();
            if (result == DialogResult.OK) {
                MovingPicturesCore.Settings.SocialEnabled = true;
                MovingPicturesCore.Settings.SocialUsername = loginForm.ValidatedUser.Name;
                MovingPicturesCore.Settings.SocialHashedPassword = loginForm.ValidatedUser.HashedPassword;
            }
        }

        private void DisconnectMps() {
            DialogResult response = MessageBox.Show("Are you sure you want to disconnect this computer\nfrom Moving Pictures Social?", "Moving Pictures Social", MessageBoxButtons.YesNo);
            if (response == DialogResult.Yes) {
                MovingPicturesCore.Settings.SocialEnabled = false;
                MovingPicturesCore.Settings.SocialUsername = "";
                MovingPicturesCore.Settings.SocialHashedPassword = "";
            }
        }


        private void Sync() {
            ProgressPopup popup = new ProgressPopup(new TrackableWorkerDelegate(MovingPicturesCore.Social.Synchronize));
            popup.Owner = this.ParentForm;
            popup.ShowDialog();
        }

        private void OpenUserPage() {
            string url = String.Format("{0}u/{1}", MovingPicturesCore.Settings.SocialURLBase, MovingPicturesCore.Settings.SocialUsername);
            ProcessStartInfo processInfo = new ProcessStartInfo(url);
            Process.Start(processInfo);
        }

        private void TogglePrivateProfile() {
            ProgressPopup popup = new ProgressPopup(new WorkerDelegate(() => {
                MovingPicturesCore.Social.SocialAPI.UpdateUser("", "en", !publicProfileCheckBox.Checked);
            }));

            popup.Owner = this.ParentForm;
            popup.ShowDialog();
        }

        private void LaunchSyncFilterDialog() {
            MovieFilterEditorPopup popup = new MovieFilterEditorPopup();

            // attach the filter, show the popup, and if necisarry, save the results
            popup.FilterPane.AttachedFilter = MovingPicturesCore.Settings.SocialSyncFilter;
            popup.ShowDialog();
            MovingPicturesCore.Settings.SocialSyncFilter.Commit();
        }

        private void UpdateControls() {
            restrictMoviesButton.Enabled = restrictSyncedMoviesCheckBox.Checked;

            if (!MovingPicturesCore.Settings.SocialEnabled) {
                statusLabel.Text = "Not connected to Moving Pictures Social!";
                statusLabel.ForeColor = Color.Red;

                userLinkLabel.Visible = false;

                accountButton.Text = "Setup Account";

                if (MovingPicturesCore.Settings.SocialEnabled)
                    publicProfileCheckBox.Checked = !MovingPicturesCore.Social.SocialAPI.User.PrivateProfile;
                else
                    publicProfileCheckBox.Checked = false;

                restrictMoviesButton.Enabled = false;
                restrictSyncedMoviesCheckBox.Enabled = false;
                publicProfileCheckBox.Enabled = false;
                syncButton.Enabled = false;
            }
            else {
                statusLabel.Text = "Currently linked to:";
                statusLabel.ForeColor = Label.DefaultForeColor;

                userLinkLabel.Visible = true;
                userLinkLabel.Text = MovingPicturesCore.Settings.SocialUsername;

                accountButton.Text = "Disconnect Account";

                restrictSyncedMoviesCheckBox.Enabled = true;
                publicProfileCheckBox.Enabled = true;
                syncButton.Enabled = true;
            }

            // temporarily disable private profile
            publicProfileCheckBox.Enabled = false;
        }

        private void accountButton_Click(object sender, EventArgs e) {
            if (MovingPicturesCore.Settings.SocialEnabled)
                DisconnectMps();
            else
                ConnectMps();

            UpdateControls();
        }

        private void syncButton_Click(object sender, EventArgs e) {
            Sync();
        }

        private void restrictSyncedMoviesCheckBox_CheckedChanged(object sender, EventArgs e) {
            UpdateControls();
        }

        private void restrictMoviesButton_Click(object sender, EventArgs e) {
            LaunchSyncFilterDialog();
        }

        private void publicProfileCheckBox_CheckedChanged(object sender, EventArgs e) {
            TogglePrivateProfile();
        }

        private void userLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            OpenUserPage();
        }
    }
}
