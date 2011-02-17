using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cornerstone.GUI.Dialogs;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;
using System.Diagnostics;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;
using Cornerstone.GUI;
using Follwit.API.UI;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class SocialPanel : UserControl {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        bool updatingControls = false;

        public SocialPanel() {
            InitializeComponent();

            restrictSyncedMoviesCheckBox.Setting = MovingPicturesCore.Settings["mps_restrict_synched_movies"];

            MovingPicturesCore.Social.StatusChanged += new FollwitConnector.StatusChangedDelegate(Social_StatusChanged);

            UpdateControls();
        }

        private void ConnectMps() {
            LoginForm loginForm = new LoginForm();
            loginForm.ApiUrl = MovingPicturesCore.Settings.SocialUrl;

            DialogResult result = loginForm.ShowDialog();
            if (result == DialogResult.OK) {
                MovingPicturesCore.Settings.SocialEnabled = true;
                MovingPicturesCore.Settings.SocialUsername = loginForm.ValidatedUser.Name;
                MovingPicturesCore.Settings.SocialHashedPassword = loginForm.ValidatedUser.HashedPassword;

                MovingPicturesCore.Social.Reconnect();
                DialogResult response = MessageBox.Show("Do you want to synchronize your collection with follw.it now?", "follw.it", MessageBoxButtons.YesNo);
                if (response == DialogResult.Yes) {
                    Sync();
                }
            }
        }

        private void DisconnectMps() {
            DialogResult response = MessageBox.Show("Are you sure you want to disconnect this computer\nfrom follw.it?", "follw.it", MessageBoxButtons.YesNo);
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
            if (updatingControls)
                return;

            ProgressPopup popup = new ProgressPopup(new WorkerDelegate(() => {
                MovingPicturesCore.Social.FollwitApi.UpdateUser("", "en", !publicProfileCheckBox.Checked);
            }));

            popup.Owner = this.ParentForm;
            popup.ShowDialog();
        }

        private void LaunchSyncFilterDialog() {
            MovieFilterEditorPopup popup = new MovieFilterEditorPopup();

            // attach the filter, show the popup, and if necisarry, save the results
            popup.FilterPane.AttachedFilter = MovingPicturesCore.Settings.SocialSyncFilter;
            DialogResult result = popup.ShowDialog();
            if (result == DialogResult.OK) {
                MovingPicturesCore.Settings.SocialSyncFilter.Commit();
                Sync();
            }
            else
                MovingPicturesCore.Settings.SocialSyncFilter.Revert();

        }

        private void UpdateControls() {
            updatingControls = true;

            restrictMoviesButton.Enabled = restrictSyncedMoviesCheckBox.Checked;

            if (!MovingPicturesCore.Settings.SocialEnabled) {
                statusLabel.Text = "Not connected to follw.it!";
                statusLabel.ForeColor = Color.Red;

                userLinkLabel.Visible = false;
                retryLinkLabel.Visible = false;

                accountButton.Text = "Setup Account";

                publicProfileCheckBox.Checked = false;
                restrictMoviesButton.Enabled = false;
                restrictSyncedMoviesCheckBox.Enabled = false;
                publicProfileCheckBox.Enabled = false;
                syncButton.Enabled = false;
            }
            else {

                if (MovingPicturesCore.Social.FollwitApi == null) {
                    statusLabel.Text = "Unable to connect to follw.it!";
                    statusLabel.ForeColor = Color.Red;

                    userLinkLabel.Visible = false;
                    retryLinkLabel.Visible = true;

                }
                else {
                    statusLabel.Text = "Currently linked to:";
                    statusLabel.ForeColor = Label.DefaultForeColor;

                    userLinkLabel.Visible = true;
                    retryLinkLabel.Visible = false;
                    userLinkLabel.Text = MovingPicturesCore.Settings.SocialUsername;
                }


                accountButton.Text = "Disconnect Account";

                try { publicProfileCheckBox.Checked = !MovingPicturesCore.Social.FollwitApi.User.PrivateProfile; }
                catch { }

                restrictSyncedMoviesCheckBox.Enabled = true;
                publicProfileCheckBox.Enabled = true;
                syncButton.Enabled = true;
            }

            updatingControls = false;
        }

        private void Social_StatusChanged(FollwitConnector.StatusEnum status) {
            if (InvokeRequired) Invoke(new FollwitConnector.StatusChangedDelegate(Social_StatusChanged), new object[] { status });

            UpdateControls();
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

        private void logoPanel1_Click(object sender, EventArgs e) {
            ProcessStartInfo processInfo = new ProcessStartInfo(MovingPicturesCore.Settings.SocialURLBase);
            Process.Start(processInfo);
        }

        private void logoPanel1_MouseEnter(object sender, EventArgs e) {
            Cursor = Cursors.Hand;
        }

        private void logoPanel1_MouseLeave(object sender, EventArgs e) {
            Cursor = Cursors.Arrow;
        }

        private void retryLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            ProgressPopup popup = new ProgressPopup(new WorkerDelegate(delegate { MovingPicturesCore.Social.Reconnect(); }));
            UpdateControls();

            if (MovingPicturesCore.Social.FollwitApi == null) {
                MessageBox.Show(this, "Unable to reconnect to follw.it!", "Error");
            }
        }
    }
}
