using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MovingPicturesSocialAPI.UI.Panels;
using MovingPicturesSocialAPI.Data;

namespace MovingPicturesSocialAPI.UI {
    public partial class MpsLoginForm : Form {

        public MpsUser ValidatedUser {
            get;
            private set;
        }

        public string ApiUrl {
            get {
                return _apiUrl;
            }

            set {
                _apiUrl = value;
                newUserPanel.ApiUrl = value;
                existingUserPanel.ApiUrl = value;

            }
        } string _apiUrl = MpsAPI.DefaultUrl;

        private LoginNewUserPanel newUserPanel;
        private LoginExistingUserPanel existingUserPanel;
        private LoginSuccessPanel successPanel;
           
        public MpsLoginForm() {
            InitializeComponent();

            newUserPanel = new LoginNewUserPanel();
            newUserPanel.Visible = false;
            newUserPanel.Location = new System.Drawing.Point(10, 100);
            Controls.Add(newUserPanel);

            existingUserPanel = new LoginExistingUserPanel();
            existingUserPanel.Visible = false;
            existingUserPanel.Location = new System.Drawing.Point(10, 100);
            Controls.Add(existingUserPanel);

            successPanel = new LoginSuccessPanel();
            successPanel.Visible = false;
            successPanel.Location = new System.Drawing.Point(10, 100);
            Controls.Add(successPanel);

            welcomePanel.ActionSelected += new LoginWelcomePanel.ActionSelectedDelegate(welcomePanel_ActionSelected);
            newUserPanel.ActionSelected += new LoginNewUserPanel.ActionSelectedDelegate(newUserPanel_ActionSelected);
            existingUserPanel.ActionSelected += new LoginExistingUserPanel.ActionSelectedDelegate(existingUserPanel_ActionSelected);
        }

        // handle actions triggered by the welome panel
        void welcomePanel_ActionSelected(Panels.LoginWelcomePanel.Action action) {
            welcomePanel.Visible = false;
            
            switch (action) {
                case LoginWelcomePanel.Action.EXISTING_USER:
                    existingUserPanel.Visible = true;
                    break;
                case LoginWelcomePanel.Action.NEW_USER:
                    newUserPanel.Visible = true;
                    break;
            }
        }

        // handle actions triggered by the new user panel
        void newUserPanel_ActionSelected(LoginNewUserPanel.Action action) {
            newUserPanel.Visible = false;

            switch (action) {
                case LoginNewUserPanel.Action.BACK:
                    welcomePanel.Visible = true;                    
                    break;
                case LoginNewUserPanel.Action.VALIDATED:
                    successPanel.Visible = true;
                    ValidatedUser = newUserPanel.ValidatedUser;
                    break;
            }
        }

        // handle actions triggered by the existing user login panel
        void existingUserPanel_ActionSelected(LoginExistingUserPanel.Action action) {
            existingUserPanel.Visible = false;

            switch (action) {
                case LoginExistingUserPanel.Action.BACK:
                    welcomePanel.Visible = true;
                    break;
                case LoginExistingUserPanel.Action.VALIDATED:
                    successPanel.Visible = true;
                    ValidatedUser = existingUserPanel.ValidatedUser;
                    break;
            }
        }

    }
}
