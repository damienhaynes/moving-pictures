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
            ProgressPopup popup = new Cornerstone.GUI.Dialogs.ProgressPopup(new TrackableWorkerDelegate(SyncWorker));
            popup.Owner = this.ParentForm;
            popup.ShowDialog();
        }

        private void SyncWorker(ProgressDelegate progress) {
            try {
                List<DBMovieInfo> allMovies = DBMovieInfo.GetAll();
                logger.Debug("Syncing {0} movies", allMovies.Count);

                allMovies = allMovies.OrderBy(m => m.DateAdded).ToList();

                List<MpsMovie> mpsMovies = new List<MpsMovie>();

                int count = 0;
                foreach (var movie in allMovies) {
                    count++;
                    MpsMovie mpsMovie = MovingPicturesCore.Social.MovieToMPSMovie(movie);
                    if (mpsMovie.Resources.Length > 1) {
                        logger.Debug("Adding {0} to movies to be synced", movie.Title);
                        mpsMovies.Add(mpsMovie);
                    }
                    else {
                        logger.Debug("Skipping {0} because it doesn't have source information", movie.Title);
                    }

                    if (progress != null)
                        progress("Syncing All Movies to MPS", (int)(count * 100 / allMovies.Count));

                    if (mpsMovies.Count >= 100 || count == allMovies.Count) {
                        logger.Debug("Sending batch of {0} movies", mpsMovies.Count);
                        MovingPicturesCore.Social.SocialAPI.AddMoviesToCollection(ref mpsMovies);

                        // update MpsId on the DBMovieInfo object
                        foreach (MpsMovie mpsMovieDTO in mpsMovies) {
                            DBMovieInfo m = DBMovieInfo.Get(mpsMovieDTO.InternalId);
                            if (m != null) {
                                m.MpsId = mpsMovieDTO.MovieId;
                                m.Commit();
                            }
                        }

                        mpsMovies.Clear();
                    }
                }
            }
            catch (Exception ex) {
                logger.ErrorException("", ex);
            }
        }

        private void OpenUserPage() {
            string url = String.Format("{0}u/{1}", MovingPicturesCore.Settings.SocialURLBase, MovingPicturesCore.Settings.SocialUsername);
            ProcessStartInfo processInfo = new ProcessStartInfo(url);
            Process.Start(processInfo);
        }

        private void UpdateControls() {
            restrictMoviesButton.Enabled = restrictSyncedMoviesCheckBox.Checked;

            if (!MovingPicturesCore.Settings.SocialEnabled) {
                statusLabel.Text = "Not connected to Moving Pictures Social!";
                statusLabel.ForeColor = Color.Red;

                userLinkLabel.Visible = false;

                accountButton.Text = "Setup Account";

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

        }

        private void publicProfileCheckBox_CheckedChanged(object sender, EventArgs e) {

        }

        private void userLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            OpenUserPage();
        }
    }
}
