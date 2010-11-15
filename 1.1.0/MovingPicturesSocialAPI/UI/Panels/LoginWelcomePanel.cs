using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MovingPicturesSocialAPI.UI.Panels {
    public partial class LoginWelcomePanel : UserControl {
        public enum Action { NEW_USER, EXISTING_USER }
        
        public event ActionSelectedDelegate ActionSelected;
        public delegate void ActionSelectedDelegate(Action action);
                
        public LoginWelcomePanel() {
            InitializeComponent();
        }

        private void existingAccountButton_Click(object sender, EventArgs e) {
            if (ActionSelected != null) ActionSelected(Action.EXISTING_USER);
        }

        private void newAccountButton_Click(object sender, EventArgs e) {
            if (ActionSelected != null) ActionSelected(Action.NEW_USER);
        }
    }
}
