using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class AboutPane : UserControl {
        public AboutPane() {
            InitializeComponent();
        }

        private void advancedSettingsButton_Click(object sender, EventArgs e) {
            AdvancedSettingsPopup popup = new AdvancedSettingsPopup();
            popup.Owner = ParentForm;
            popup.ShowDialog();
        }
    }
}
