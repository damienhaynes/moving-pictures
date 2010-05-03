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
                logger.Debug("Registering with MPS as ", txtRegisterUsername.Text);
                bool bSuccess = MpsAPI.UserCreate(
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
                    MovingPicturesCore.Settings.SocialSyncRequired = true;

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
                    , MovingPicturesCore.Settings["socialurlbase"].Value.ToString() + "api/1.0/");

                if (api.UserCheckAuthentication()) {
                    logger.Debug("Login succeeded for user {0}.  Linking with MPS", txtLinkUsername.Text);
                    MovingPicturesCore.Settings.SocialUsername = txtLinkUsername.Text;
                    MovingPicturesCore.Settings.SocialPassword = HashMPSPassword(txtLinkPassword.Text);
                    MovingPicturesCore.Settings.SocialSyncRequired = true;

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
                List<MovingPicturesSocialAPI.MovieDTO> mpsMovies = new List<MovingPicturesSocialAPI.MovieDTO>();

                int count = 0;
                foreach (var movie in allMovies) {
                    count++;
                    MovingPicturesSocialAPI.MovieDTO mpsMovie = new MovingPicturesSocialAPI.MovieDTO();
                    string directors = "";
                    string actors = "";
                    string writers = "";
                    string genres = "";
                    foreach (var person in movie.Directors) {
                        directors += "|" + person;
                    }
                    foreach (var person in movie.Actors) {
                        actors += "|" + person;
                    }
                    foreach (var person in movie.Writers) {
                        writers += "|" + person;
                    }
                    foreach (var genre in movie.Genres) {
                        genres += "|" + genre;
                    }

                    mpsMovie.SourceName = movie.PrimarySource.Provider.Name;
                    mpsMovie.SourceId = movie.GetSourceMovieInfo(movie.PrimarySource).Identifier;
                    mpsMovie.Title = movie.Title;
                    mpsMovie.Year = movie.Year.ToString();
                    mpsMovie.Certification = movie.Certification;
                    mpsMovie.Language = movie.Language;
                    mpsMovie.Tagline = movie.Tagline;
                    mpsMovie.Summary = movie.Summary;
                    mpsMovie.Score = movie.Score.ToString();
                    mpsMovie.Popularity = movie.Popularity.ToString();
                    mpsMovie.Runtime = movie.Runtime.ToString();
                    mpsMovie.Genres = genres;
                    mpsMovie.Directors = directors;
                    mpsMovie.Cast = actors;
                    mpsMovie.TranslatedTitle = movie.Title;
                    mpsMovie.Locale = movie.PrimarySource.Provider.LanguageCode;
                    mpsMovies.Add(mpsMovie);

                    if (progress != null)
                        progress("Syncing All Movies to MPS", (int)(count * 100 / allMovies.Count));
                    if (mpsMovies.Count >= 100) {
                        MovingPicturesCore.Social.SocialAPI.MovieAddToCollectionWithData(mpsMovies);
                        mpsMovies.Clear();
                    }
                }
                MovingPicturesCore.Social.SocialAPI.MovieAddToCollectionWithData(mpsMovies);
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
