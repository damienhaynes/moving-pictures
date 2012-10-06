using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    public partial class AdvancedSettingsPopup : Form {
        public AdvancedSettingsPopup() {
            InitializeComponent();

            // if we are in designer, break to prevent errors with rendering, it cant access the DB...
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            //for production replace following statement with ==> advancedSettingsWarningPane1.SettingsPane.populateTree(MovingPicturesCore.SettingsManager);
            advancedSettingsWarningPane1.SettingsPane.populateTree(MovingPicturesCore.Settings);
        }

        private void AdvancedSettingsPopup_Load(object sender, EventArgs e) {
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
