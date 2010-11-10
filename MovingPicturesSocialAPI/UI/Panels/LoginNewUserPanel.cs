using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MovingPicturesSocialAPI.Data;
using System.Threading;
using CookComputing.XmlRpc;

namespace MovingPicturesSocialAPI.UI.Panels {
    public partial class LoginNewUserPanel : UserControl {
        public enum Action { BACK, VALIDATED }

        public event ActionSelectedDelegate ActionSelected;
        public delegate void ActionSelectedDelegate(Action action);

        private delegate void InvokeDelegate();
        private delegate void InvokeDelegateEx(Exception ex);

        private Color defaultLabelColor;
        private bool verified = false;
        private MpsAPI api = null;

        private bool usernameAvailable = true;
        private string lastCheckedName = "";

        public MpsUser ValidatedUser {
            get;
            private set;
        }

        public string ApiUrl {
            get;
            set;
        }

        public LoginNewUserPanel() {
            InitializeComponent();

            ApiUrl = null;
        }

        private void CreateUser() {
            // set the status label to inform the user of whats happening
            statusLabel.ForeColor = defaultLabelColor;
            statusLabel.Text = "Creating new account...";
            statusLabel.Visible = true;

            // disable the log in button so the user doesnt double tap it
            createAccountOkButton.Enabled = false;

            ThreadStart actions = delegate {
                try { 
                    string username = usernameTextBox.Text;
                    string password = passwordTextBox.Text;

                    bool success = MpsAPI.CreateUser(username, password, emailTextBox.Text, "en", privateCheckBox.Checked, "http://social-dev.moving-pictures.tv/api/1.0/");

                    if (success) {
                        api = MpsAPI.Login(username, password, ApiUrl);
                        ValidatedUser = api.User;
                    }
                    else {
                        api = null;
                        ValidatedUser = null;
                    }
                }
                catch (Exception ex) {                    
                    UnexpectedError(ex);
                    return;
                }

                CreateUserDone();
            };

            Thread verifyLoginThread = new Thread(actions);
            verifyLoginThread.Name = "Authentication Thread";
            verifyLoginThread.IsBackground = true;
            verifyLoginThread.Start();
        }

        private void CreateUserDone() {
            if (InvokeRequired) {
                Invoke(new InvokeDelegate(CreateUserDone));
                return;
            }

            if (api == null) {
                statusLabel.ForeColor = Color.Red;
                statusLabel.Text = "Unexpected Error.";
                statusLabel.Visible = true;

                createAccountOkButton.Text = "Sign In";
                createAccountOkButton.Enabled = true;
                verified = false;
            }
            else {
                statusLabel.ForeColor = Color.Green;
                statusLabel.Text = "Account successfully created!";
                statusLabel.Visible = true;

                createAccountOkButton.Text = "OK";
                createAccountOkButton.Enabled = true;
                verified = true;

                if (ActionSelected != null) ActionSelected(Action.VALIDATED);            
            }
        }

        private void UnexpectedError(Exception ex) {
            if (InvokeRequired) {
                Invoke(new InvokeDelegateEx(UnexpectedError), new object[] { ex });
                return;
            }

            createAccountOkButton.Text = "Create Account";
            createAccountOkButton.Enabled = true;
            verified = false;

            statusLabel.ForeColor = Color.Red;
            if (ex is XmlRpcServerException && ((XmlRpcServerException)ex).Message == "Not Found")
                statusLabel.Text = "Unable to connect to server!";
            else if (ex is UsernameAlreadyExistsException) {
                statusLabel.Text = "Selected username is unavailable!";
            }
            else if (ex is RequiredFieldMissingException) {
                statusLabel.Text = "Missing information: " + ((RequiredFieldMissingException)ex).FieldName;
            }
            else 
                statusLabel.Text = "Unexpected Error: " + ex.Message;
        }

        private void SetStatus() {
            statusLabel.Visible = false;

            if (verified)
                return;

            if (usernameTextBox.Text.Length == 0 || 
                emailTextBox.Text.Length == 0 ||
                passwordTextBox.Text.Length == 0 ||
                verifyPasswordTextBox.Text.Length == 0) 

                createAccountOkButton.Enabled = false;
            else
                createAccountOkButton.Enabled = true;

            if ((passwordTextBox.Text != verifyPasswordTextBox.Text) && verifyPasswordTextBox.Text != "") {
                statusLabel.ForeColor = Color.Red;
                statusLabel.Visible = true;
                statusLabel.Text = "Passwords do not match!";
                createAccountOkButton.Enabled = false;
            }

            CheckUsername();
        }

        private void CheckUsername() {
            // no error if we are editing the username
            if (usernameTextBox.Focused || usernameTextBox.Text == lastCheckedName) {
                
                if (usernameTextBox.Text == lastCheckedName)
                    UsernameChecked();

                return;
            }

            ThreadStart actions = delegate {
                lastCheckedName = usernameTextBox.Text;
                try { usernameAvailable = MpsAPI.IsUsernameAvailable(usernameTextBox.Text, "http://social-dev.moving-pictures.tv/api/1.0/"); }
                catch (Exception ex) {
                    UnexpectedError(ex);
                    return;
                }

                UsernameChecked();
            };

            Thread verifyLoginThread = new Thread(actions);
            verifyLoginThread.Name = "Username Check Thread";
            verifyLoginThread.IsBackground = true;
            verifyLoginThread.Start();
        }

        private void UsernameChecked() {
            if (InvokeRequired) {
                Invoke(new InvokeDelegate(UsernameChecked));
                return;
            }

            if (!usernameAvailable) {
                statusLabel.ForeColor = Color.Red;
                statusLabel.Visible = true;
                statusLabel.Text = "Selected username is unavailable!";
                createAccountOkButton.Enabled = false;
            }
        }

        private void backButton_Click(object sender, EventArgs e) {
            SetStatus();

            if (ActionSelected != null) ActionSelected(Action.BACK);
        }

        private void usernameTextBox_TextChanged(object sender, EventArgs e) {
            SetStatus();
        }

        private void passwordTextBox_TextChanged(object sender, EventArgs e) {
            SetStatus();
        }

        private void verifyPasswordTextBox_TextChanged(object sender, EventArgs e) {
            SetStatus();
        }

        private void passwordTextBox_FocusChanged(object sender, EventArgs e) {
            SetStatus();
        }

        private void verifyPasswordTextBox_FocusChanged(object sender, EventArgs e) {
            SetStatus();
        }

        private void LoginNewUserPanel_Load(object sender, EventArgs e) {
            defaultLabelColor = statusLabel.ForeColor;
        }

        private void LoginNewUserPanel_VisibleChanged(object sender, EventArgs e) {
            if (Visible) {
                SetStatus();

                usernameTextBox.Focus();
                usernameTextBox.Text = "";
                emailTextBox.Text = "";
                passwordTextBox.Text = "";
                verifyPasswordTextBox.Text = "";
                privateCheckBox.Checked = false;

                ValidatedUser = null;
                api = null;

                if (ParentForm != null) {                    
                    this.ParentForm.AcceptButton = createAccountOkButton;
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

        private void createAccountOkButton_Click(object sender, EventArgs e) {
            if (!verified)
                CreateUser();
            else
                if (ActionSelected != null) ActionSelected(Action.VALIDATED);            
        }

        private void usernameTextBox_Leave(object sender, EventArgs e) {
            SetStatus();
        }
    }
}
