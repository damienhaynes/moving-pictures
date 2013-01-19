using Cornerstone.GUI.Dialogs;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class MovingPicturesConfig : Form
    {
        #region MediaPortal XML Constants
        const string cMovingPicturesSection = "MovingPictures";
        const string cConfigScreenWidth = "ConfigScreenWidth";
        const string cConfigScreenHeight = "ConfigScreenHeight";
        const string cConfigPositionX = "ConfigPositionX";
        const string cConfigPositionY = "ConfigPositionY";
        #endregion

        public MovingPicturesConfig() {
            InitializeComponent();

            #if DEBUG
            ShowInTaskbar = true;
            #endif

            // if we are in designer, break to prevent errors with rendering, it cant access the DB...
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;
        }

        private void MoviesPluginConfig_Load(object sender, EventArgs e) {

            // load previous saved screen dimmensions and position
            if (MovingPicturesCore.Settings.ConfigScreenWidth != 0) this.Width = MovingPicturesCore.Settings.ConfigScreenWidth;
            if (MovingPicturesCore.Settings.ConfigScreenHeight != 0) this.Height = MovingPicturesCore.Settings.ConfigScreenHeight;
            if (MovingPicturesCore.Settings.ConfigScreenPositionX != 0) this.Left = MovingPicturesCore.Settings.ConfigScreenPositionX;
            if (MovingPicturesCore.Settings.ConfigScreenPositionX != 0) this.Top = MovingPicturesCore.Settings.ConfigScreenPositionY;

            // if we start on the first tab, we need to
            // manually flip to the second tab (media importer) very quickly 
            // to force it to load into memory
            // mainTabControl.SelectedIndex = 1;
            // mainTabControl.SelectedIndex = 0;

            mainTabControl.SelectedIndex = 1;
        }

        private void MovingPicturesConfig_FormClosing(object sender, FormClosedEventArgs e) {
            if (!DesignMode) {
                MovingPicturesCore.Importer.Stop();
                ProgressPopup popup = new ProgressPopup(new WorkerDelegate(movieManagerPane1.Commit));
                popup.Owner = this;
                popup.ShowDialog();
            }


            MovingPicturesCore.Settings.ConfigScreenWidth = this.Width;
            MovingPicturesCore.Settings.ConfigScreenHeight = this.Height;
            MovingPicturesCore.Settings.ConfigScreenPositionX = this.Left;
            MovingPicturesCore.Settings.ConfigScreenPositionY = this.Top;
        }

        private void MovingPicturesConfig_Shown(object sender, EventArgs e) {
            if (MovingPicturesCore.Settings.DataProviderManagementMethod == "undefined") {
                MovingPicturesCore.DataProviderManager.ArrangeDataProviders("en");
                DataProviderSetupPopup popup = new DataProviderSetupPopup();
                popup.Owner = this;
                popup.ShowDialog();
            }
        }        

    }
}