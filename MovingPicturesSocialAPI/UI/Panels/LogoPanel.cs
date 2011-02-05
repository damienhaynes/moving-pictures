using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MovingPicturesSocialAPI.UI.Panels {
    public partial class LogoPanel : UserControl {
        public LogoPanel() {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e) {
            this.OnClick(e);
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e) {
            this.OnMouseEnter(e);
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e) {
            this.OnMouseLeave(e);
        }
    }
}
