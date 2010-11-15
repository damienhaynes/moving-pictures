using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MovingPicturesSocialAPI.UI.Panels {
    public partial class LoginSuccessPanel : UserControl {
        public LoginSuccessPanel() {
            InitializeComponent();
        }

        private void LoginSuccessPanel_Load(object sender, EventArgs e) {
            ParentForm.AcceptButton = okButton;
        }

        private void okButton_Click(object sender, EventArgs e) {
            ParentForm.Close();
        }
    }
}
