using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.Database;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    public partial class ScriptVersionPopup : Form {
        private DBSourceInfo source;
        
        public ScriptVersionPopup(DBSourceInfo source) {
            InitializeComponent();

            this.source = source;
        }

        private void center() {
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

        private void ScriptVersionPopup_Load(object sender, EventArgs e) {
            if (!DesignMode) {
                sourceNameLabel.Text = source.Provider.Name;

                // populate the list
                foreach (DBScriptInfo currScript in source.Scripts) {
                    ListViewItem currItem = new ListViewItem(currScript.Provider.Version);
                    ListViewItem.ListViewSubItem publishedItem;
                    if (currScript.Provider.Published != null)
                        publishedItem = new ListViewItem.ListViewSubItem(currItem, currScript.Provider.Published.Value.ToShortDateString());
                    else
                        publishedItem = new ListViewItem.ListViewSubItem(currItem, "");

                    currItem.SubItems.Add(publishedItem);

                    currItem.Tag = currScript;
                    this.listView.Items.Add(currItem);

                    // select the currently active version
                    if (currItem.Tag == source.SelectedScript)
                        currItem.Selected = true;
                }
            }

            center();
        }

        private void okButton_Click(object sender, EventArgs e) {
            source.SelectedScript = (DBScriptInfo) listView.SelectedItems[0].Tag;
            source.Commit();
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e) {
            this.Close();
        }
    }
}
