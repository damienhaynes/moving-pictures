using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    public partial class CoverPopup : Form {
        public CoverPopup() {
            InitializeComponent();
        }

        public CoverPopup(Image image) {
            InitializeComponent();
            pictureBox.Image = image;
        }

        private void pictureBox_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void CoverPopup_KeyPress(object sender, KeyPressEventArgs e) {
            this.Close();
        }

        private void CoverPopup_Deactivate(object sender, EventArgs e) {
            this.Close();
        }

        // if we have been launched as a dialog and we have an owner, center
        // on the owning form.
        private void CoverPopup_Shown(object sender, EventArgs e) {

        }

        private void CoverPopup_Load(object sender, EventArgs e) {
            if (Owner == null)
                return;

            Point center = new Point();
            center.X = Owner.Location.X + (Owner.Width / 2);
            center.Y = Owner.Location.Y + (Owner.Height / 2);

            Point newLocation = new Point();
            newLocation.X = center.X - (Width / 2);
            newLocation.Y = center.Y - (Height / 2);

            Location = newLocation;
        }


    }
}
