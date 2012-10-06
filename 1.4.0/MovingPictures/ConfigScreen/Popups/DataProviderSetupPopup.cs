using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    public partial class DataProviderSetupPopup : Form {
        public DataProviderSetupPopup() {
            InitializeComponent();
            autoDataSourcesPanel1.AutoCommit = false;
        }

        private void okButton_Click(object sender, EventArgs e) {
            autoDataSourcesPanel1.Commit();
            Close();
        }

        private void DataProviderSetupPopup_Load(object sender, EventArgs e) {
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
