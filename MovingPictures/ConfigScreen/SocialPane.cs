using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MovingPicturesSocialAPI;
using System.Diagnostics;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;
using System.Security.Cryptography;
using Cornerstone.GUI.Dialogs;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class SocialPane : UserControl {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public SocialPane() {
            InitializeComponent();

            // hide tabs at runtime
            tabControl1.SizeMode = TabSizeMode.Fixed;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.Top = -5;
            tabControl1.Left = -5;
            tabControl1.Height = tabControl1.Height + 10;
            tabControl1.Width = tabControl1.Width + 10;
        }

        private void SocialPane_Load(object sender, EventArgs e) {
            if (!DesignMode) {
                if (MovingPicturesCore.Social.HasSocial) {
                    SwitchToTab(this.tabAlreadyLinked);
                }
                else {
                    SwitchToTab(this.tabStartWizard);
                }
            }
        }

        private void SwitchToTab(TabPage tabPage) {
            this.tabControl1.SelectedTab = tabPage;

            if (tabPage == this.tabStartWizard) {
                this.ParentForm.AcceptButton = this.btnStartNext;
            }

            if (tabPage == this.tabCreateAccount) {
                this.ParentForm.AcceptButton = this.btnRegisterNext;
                this.txtRegisterUsername.Text = "";
                this.txtRegisterPassword.Text = "";
                this.txtRegisterConfirmPassword.Text = "";
                this.txtRegisterEmail.Text = "";
                this.chkRegisterPrivateProfile.Checked = false;
            }

            if (tabPage == tabLinkAccount) {
                this.ParentForm.AcceptButton = this.btnLinkNext;
                this.txtLinkUsername.Text = "";
                this.txtLinkPassword.Text = "";
            }

            if (tabPage == tabAlreadyLinked) {
                linkLinkedUsername.Text = MovingPicturesCore.Settings.SocialUsername;
            }
        }

        private void btnStartNext_Click(object sender, EventArgs e) {
            if (radStartCreate.Checked)
                SwitchToTab(this.tabCreateAccount);
            else
                SwitchToTab(this.tabLinkAccount);
        }

        private void btnRegisterBack_Click(object sender, EventArgs e) {
            SwitchToTab(this.tabStartWizard);
        }

        private void btnRegisterNext_Click(object sender, EventArgs e) {
            try {
                // todo: form validation
                logger.Debug("Registering with MPS as " + txtRegisterUsername.Text);
                bool bSuccess = MpsAPI.CreateUser(
                                    txtRegisterUsername.Text
                                    , txtRegisterPassword.Text
                                    , txtRegisterEmail.Text
                                    , "en", "1.0"
                                    , chkRegisterPrivateProfile.Checked
                                    , MovingPicturesCore.Settings["socialurlbase"].Value.ToString() + "api/1.0/"
                                    );
                if (bSuccess) {
                    logger.Debug("MPS Registration successful for user {0}.  Linking with MPS.", txtRegisterUsername.Text);
                    MovingPicturesCore.Settings.SocialUsername = txtRegisterUsername.Text;
                    MovingPicturesCore.Settings.SocialPassword = HashMPSPassword(txtRegisterPassword.Text);

                    SwitchToTab(this.tabAlreadyLinked);
                    MessageBox.Show("Your Moving Pictures Social account was created successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else {
                    logger.Debug("MPS Registration unsuccessful");
                    MessageBox.Show("There was an error creating your Moving Pictures Social account.  Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            catch (UsernameAlreadyExistsException ex) {
                logger.WarnException("", ex);
                MessageBox.Show("Sorry, this username is already in use. Please try a different username.  If you own the username, you can link to your existing Moving Pictures social account."
                    , "Username alerady in use", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (RequiredFieldMissingException ex) {
                logger.WarnException("", ex);
                MessageBox.Show(String.Format("Please complete the {0} field to continue", ex.FieldName)
                    , "Required Field Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex) {
                logger.ErrorException("", ex);
                MessageBox.Show(String.Format("An unexpected error occurred while registering with Moving Pictures Social.\n{0}", ex.Message)
                    , "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnLinkBack_Click(object sender, EventArgs e) {
            SwitchToTab(this.tabStartWizard);
        }

        private void btnLinkNext_Click(object sender, EventArgs e) {
            try {
                MpsAPI api = new MpsAPI(txtLinkUsername.Text, txtLinkPassword.Text
                    , MovingPicturesCore.Social.SocialAPIURL);

                if (api.CheckAuthentication()) {
                    logger.Debug("Login succeeded for user {0}.  Linking with MPS", txtLinkUsername.Text);
                    MovingPicturesCore.Settings.SocialUsername = txtLinkUsername.Text;
                    MovingPicturesCore.Settings.SocialPassword = HashMPSPassword(txtLinkPassword.Text);

                    SwitchToTab(this.tabAlreadyLinked);
                    MessageBox.Show("Moving Pictures has been successfully linked to your Moving Pictures Social account.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else {
                    logger.Debug("Login failed for user {0}", txtLinkUsername.Text);
                    MessageBox.Show("Login Failed.  Check username and password and try again");
                }
            }
            catch (Exception ex) {
                logger.ErrorException("", ex);
                MessageBox.Show(String.Format("An unexpected error occurred while linking with Moving Pictures Social.\n{0}", ex.Message)
                    , "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void linkLinkedUsername_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            // open the default browser with the user's profile
            string url = String.Format("{0}u/{1}"
                , MovingPicturesCore.Settings.SocialURLBase
                , MovingPicturesCore.Settings.SocialUsername
                );
            ProcessStartInfo processInfo = new ProcessStartInfo(url);
            Process.Start(processInfo);
        }

        private void btnLinkedSyncNow_Click(object sender, EventArgs e) {
            ProgressPopup popup = new Cornerstone.GUI.Dialogs.ProgressPopup(new TrackableWorkerDelegate(SyncAllMovies));
            popup.Owner = this.ParentForm;
            popup.ShowDialog();
        }

        private void SyncAllMovies(ProgressDelegate progress) {
            try {
                List<DBMovieInfo> allMovies = DBMovieInfo.GetAll();
                logger.Debug("Syncing {0} movies", allMovies.Count);

                allMovies = allMovies.OrderBy(m => m.DateAdded).ToList();

                List<MovingPicturesSocialAPI.MovieDTO> mpsMovies = new List<MovingPicturesSocialAPI.MovieDTO>();

                int count = 0;
                foreach (var movie in allMovies) {
                    count++;
                    MovingPicturesSocialAPI.MovieDTO mpsMovie = MovingPicturesCore.Social.MovieToMPSMovie(movie);
                    if (mpsMovie.ResourceIds.Length > 1) {
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
                        foreach (MovieDTO mpsMovieDTO in mpsMovies) {
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

        private void btnLinkedRemove_Click(object sender, EventArgs e) {
            DialogResult result = MessageBox.Show("Are you sure you wish to disassociate Moving Pictures with this Moving Pictures Social account?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes) {
                logger.Debug("Removing linked MPS account {0}", MovingPicturesCore.Settings.SocialUsername);
                MovingPicturesCore.Settings.SocialUsername = "";
                MovingPicturesCore.Settings.SocialPassword = "";
                SwitchToTab(this.tabStartWizard);
            }
        }

        private string HashMPSPassword(string password) {
            // salt + hash
            string salt = "52c3a0d0-f793-46fb-a4c0-35a0ff6844c8";
            string saltedPassword = password + salt;
            SHA1CryptoServiceProvider sha1Obj = new SHA1CryptoServiceProvider();
            byte[] bHash = sha1Obj.ComputeHash(Encoding.ASCII.GetBytes(saltedPassword));
            string sHash = "";
            foreach (byte b in bHash) {
                sHash += b.ToString("x2");
            }
            return sHash;
        }

    }
}
