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
    public partial class FollwitPanel : UserControl {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        bool updatingControls = false;

        public FollwitPanel() {
            InitializeComponent();

            restrictSyncedMoviesCheckBox.Setting = MovingPicturesCore.Settings["mps_restrict_synched_movies"];

            MovingPicturesCore.Follwit.StatusChanged += new FollwitConnector.StatusChangedDelegate(Follwit_StatusChanged);

            UpdateControls();
        }

        private void ConnectFollwit() {
            LoginForm loginForm = new LoginForm();
            loginForm.ApiUrl = MovingPicturesCore.Settings.FollwitUrl;

            DialogResult result = loginForm.ShowDialog();
            if (result == DialogResult.OK) {
                MovingPicturesCore.Settings.FollwitEnabled = true;
                MovingPicturesCore.Settings.FollwitUsername = loginForm.ValidatedUser.Name;
                MovingPicturesCore.Settings.FollwitHashedPassword = loginForm.ValidatedUser.HashedPassword;

                MovingPicturesCore.Follwit.Reconnect();
                DialogResult response = MessageBox.Show("Do you want to synchronize your collection with follw.it now?", "follw.it", MessageBoxButtons.YesNo);
                if (response == DialogResult.Yes) {
                    Sync();
                }
            }
        }

        private void DisconnectFollwit() {
            DialogResult response = MessageBox.Show("Are you sure you want to disconnect this computer\nfrom follw.it?", "follw.it", MessageBoxButtons.YesNo);
            if (response == DialogResult.Yes) {
                MovingPicturesCore.Settings.FollwitEnabled = false;
                MovingPicturesCore.Settings.FollwitUsername = "";
                MovingPicturesCore.Settings.FollwitHashedPassword = "";
            }
        }


        private void Sync() {
            ProgressPopup popup = new ProgressPopup(new TrackableWorkerDelegate(MovingPicturesCore.Follwit.Synchronize));
            popup.Owner = this.ParentForm;
            popup.ShowDialog();
        }

        private void OpenUserPage() {
            string url = String.Format("{0}u/{1}", MovingPicturesCore.Settings.FollwitURLBase, MovingPicturesCore.Settings.FollwitUsername);
            ProcessStartInfo processInfo = new ProcessStartInfo(url);
            Process.Start(processInfo);
        }

        private void TogglePrivateProfile() {
            if (updatingControls)
                return;

            ProgressPopup popup = new ProgressPopup(new WorkerDelegate(() => {
                MovingPicturesCore.Follwit.FollwitApi.UpdateUser("", "", !publicProfileCheckBox.Checked);
            }));

            popup.Owner = this.ParentForm;
            popup.ShowDialog();
        }

        private void LaunchSyncFilterDialog() {
            MovieFilterEditorPopup popup = new MovieFilterEditorPopup();

            // attach the filter, show the popup, and if necisarry, save the results
            popup.FilterPane.AttachedFilter = MovingPicturesCore.Settings.FollwitSyncFilter;
            DialogResult result = popup.ShowDialog();
            if (result == DialogResult.OK) {
                MovingPicturesCore.Settings.FollwitSyncFilter.Commit();
                Sync();
            }
            else
                MovingPicturesCore.Settings.FollwitSyncFilter.Revert();

        }

        private void UpdateControls() {
            updatingControls = true;

            restrictMoviesButton.Enabled = restrictSyncedMoviesCheckBox.Checked;

            if (!MovingPicturesCore.Settings.FollwitEnabled) {
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

                if (MovingPicturesCore.Follwit.FollwitApi == null) {
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
                    userLinkLabel.Text = MovingPicturesCore.Settings.FollwitUsername;
                }


                accountButton.Text = "Disconnect Account";

                try { publicProfileCheckBox.Checked = !MovingPicturesCore.Follwit.FollwitApi.User.PrivateProfile; }
                catch { }

                restrictSyncedMoviesCheckBox.Enabled = true;
                publicProfileCheckBox.Enabled = true;
                syncButton.Enabled = true;
            }

            updatingControls = false;
        }

        private void Follwit_StatusChanged(FollwitConnector.StatusEnum status) {
            if (InvokeRequired) {
                Invoke(new FollwitConnector.StatusChangedDelegate(Follwit_StatusChanged), new object[] { status });
                return;
            }

            UpdateControls();
        }

        private void accountButton_Click(object sender, EventArgs e) {
            if (MovingPicturesCore.Settings.FollwitEnabled)
                DisconnectFollwit();
            else
                ConnectFollwit();

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
            ProcessStartInfo processInfo = new ProcessStartInfo(MovingPicturesCore.Settings.FollwitURLBase);
            Process.Start(processInfo);
        }

        private void logoPanel1_MouseEnter(object sender, EventArgs e) {
            Cursor = Cursors.Hand;
        }

        private void logoPanel1_MouseLeave(object sender, EventArgs e) {
            Cursor = Cursors.Arrow;
        }

        private void retryLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            ProgressPopup popup = new ProgressPopup(new WorkerDelegate(delegate { MovingPicturesCore.Follwit.Reconnect(); }));
            UpdateControls();

            if (MovingPicturesCore.Follwit.FollwitApi == null) {
                MessageBox.Show(this, "Unable to reconnect to follw.it!", "Error");
            }
        }
    }
}
