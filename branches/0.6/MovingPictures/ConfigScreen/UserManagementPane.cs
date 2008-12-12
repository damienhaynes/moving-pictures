using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.Database;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class UserManagementPane : UserControl {
        List<DBUser> users = new List<DBUser>();
        
        public UserManagementPane() {
            InitializeComponent();

            // if we are in designer, break to prevent errors with rendering, it cant access the DB...
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            // grab all existing users
            users = DBUser.GetAll();
            userBindingSource.DataSource = users;

            if (userBindingSource.Count == 1)
                removeSourceButton.Enabled = false;
        }

        private void usersBindingSource_ListChanged(object sender, ListChangedEventArgs e) {
            if (e.ListChangedType == ListChangedType.ItemAdded ||
                e.ListChangedType == ListChangedType.ItemChanged) {

                DBUser changedObj = (DBUser)userBindingSource[e.NewIndex];
                changedObj.Commit();
            }
        }

        private void addSourceButton_Click(object sender, EventArgs e) {
            userBindingSource.Add(new DBUser());

            if (userBindingSource.Count > 1)
                removeSourceButton.Enabled = true;

            // need to add user movie settings object for all mvoies
        }

        private void removeSourceButton_Click(object sender, EventArgs e) {
            if (userBindingSource.Current != null && userBindingSource.Count > 1) {
                ((DBUser)userBindingSource.Current).Delete();
                userBindingSource.RemoveCurrent();
            }

            if (userBindingSource.Count == 1)
                removeSourceButton.Enabled = false;

        }
    }
}
