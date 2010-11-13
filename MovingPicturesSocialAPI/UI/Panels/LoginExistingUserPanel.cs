using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using MovingPicturesSocialAPI.Data;
using CookComputing.XmlRpc;

namespace MovingPicturesSocialAPI.UI.Panels {
    public partial class LoginExistingUserPanel : UserControl {
        public enum Action { BACK, VALIDATED }

        public event ActionSelectedDelegate ActionSelected;
        public delegate void ActionSelectedDelegate(Action action);

        private delegate void InvokeDelegate();
        private delegate void InvokeDelegateEx(Exception ex);

        private Color defaultLabelColor;
        private bool verified = false;
        private MpsAPI api = null;

        public MpsUser ValidatedUser {
            get;
            private set;
        }

        public string ApiUrl {
            get;
            set;
        }

        public LoginExistingUserPanel() {
            InitializeComponent();
            ApiUrl = null;
        }

        private void VerifyLogin() {
            // set the status label to inform the user of whats happening
            statusLabel.ForeColor = defaultLabelColor;
            statusLabel.Text = "Verifying login credentials...";
            statusLabel.Visible = true;

            // disable the log in button so the user doesnt double tap it
            signInOkButton.Enabled = false;

            ThreadStart actions = delegate {
                try { api = MpsAPI.Login(usernameTextBox.Text, MpsAPI.HashPassword(passwordTextBox.Text), ApiUrl); }
                catch (Exception ex) {                    
                    LoginFailed(ex);
                    return;
                }

                LoginDone();
            };

            Thread verifyLoginThread = new Thread(actions);
            verifyLoginThread.Name = "Authentication Thread";
            verifyLoginThread.IsBackground = true;
            verifyLoginThread.Start();
        }

        private void LoginDone() {
            if (InvokeRequired) {
                Invoke(new InvokeDelegate(LoginDone));
                return;
            }

            if (api == null) {
                statusLabel.ForeColor = Color.Red;
                statusLabel.Text = "Invalid username or password.";
                statusLabel.Visible = true;

                signInOkButton.Text = "Sign In";
                signInOkButton.Enabled = true;
                verified = false;
            }
            else {
                statusLabel.ForeColor = Color.Green;
                statusLabel.Text = "Account successfully validated!";
                statusLabel.Visible = true;

                signInOkButton.Text = "OK";
                signInOkButton.Enabled = true;
                verified = true;

                ValidatedUser = api.User;
                if (ActionSelected != null) ActionSelected(Action.VALIDATED);  
            }
        }

        private void LoginFailed(Exception ex) {
            if (InvokeRequired) {
                Invoke(new InvokeDelegateEx(LoginFailed), new object[] { ex });
                return;
            }

            signInOkButton.Text = "Sign In";
            signInOkButton.Enabled = true;
            verified = false;

            statusLabel.ForeColor = Color.Red;
            if (ex is XmlRpcServerException && ((XmlRpcServerException)ex).Message == "Not Found")
                statusLabel.Text = "Unable to connect to server!";
            else
                statusLabel.Text = "Unexpected Error: " + ex.Message;
        }

        private void ClearStatus() {
            statusLabel.Visible = false;

            if (verified) {                
                signInOkButton.Text = "Sign In";
                verified = false;
            }

            if (usernameTextBox.Text.Length == 0 || passwordTextBox.Text.Length == 0) 
                signInOkButton.Enabled = false;
            else
                signInOkButton.Enabled = true;


        }

        private void backButton_Click(object sender, EventArgs e) {
            ClearStatus();

            if (ActionSelected != null) ActionSelected(Action.BACK);
        }

        private void signInOkButton_Click(object sender, EventArgs e) {
            if (!verified)
                VerifyLogin();
            else 
                if (ActionSelected != null) ActionSelected(Action.VALIDATED);            
        }

        private void usernameTextBox_TextChanged(object sender, EventArgs e) {
            ClearStatus();
        }

        private void passwordTextBox_TextChanged(object sender, EventArgs e) {
            ClearStatus();
        }

        private void LoginExistingUserPanel_Load(object sender, EventArgs e) {
            defaultLabelColor = statusLabel.ForeColor;
        }

        private void LoginExistingUserPanel_VisibleChanged(object sender, EventArgs e) {
            if (Visible) {
                ClearStatus();

                usernameTextBox.Focus();
                usernameTextBox.Text = "";
                passwordTextBox.Text = "";

                ValidatedUser = null;
                api = null;

                if (ParentForm != null) {                    
                    this.ParentForm.AcceptButton = signInOkButton;
                    this.ParentForm.CancelButton = backButton;
                    backButton.DialogResult = DialogResult.None;
                }
            }
            else {
                if (ParentForm != null) {
                    this.ParentForm.AcceptButton = null;
                    this.ParentForm.CancelButton = null;
                }
            }
        }
    }
}
