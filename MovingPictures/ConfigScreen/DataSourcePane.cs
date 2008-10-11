using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.Database;
using System.Collections.ObjectModel;
using MediaPortal.Plugins.MovingPictures.DataProviders;
using System.Collections;
using Cornerstone.Database.Tables;
using System.Threading;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class DataSourcePane : UserControl {

        public DataType DisplayType {
            set {
                if ((listView != null && listView.Items != null && listView.Items.Count != 0) && displayMode == value)
                    return;

                displayMode = value;

                switch (displayMode) {
                    case DataType.DETAILS:
                        scriptTypeDropDown.Text = scriptTypeDropDown.DropDownItems[0].Text;
                        break;
                    case DataType.COVERS:
                        scriptTypeDropDown.Text = scriptTypeDropDown.DropDownItems[1].Text;
                        break;
                    case DataType.BACKDROPS:
                        scriptTypeDropDown.Text = scriptTypeDropDown.DropDownItems[2].Text;
                        break;
                }

                listView.ListViewItemSorter = new ListViewItemComparer(displayMode);
                reloadList();
            }

            get { return displayMode; }
        }
        private DataType displayMode;

        public DataSourcePane() {
            InitializeComponent();
        }

        private void DataSourcePane_Load(object sender, EventArgs e) {
            if (!DesignMode) {
                DisplayType = DataType.DETAILS;
                updateDebugModeMenuItem();
            }
        }

        // load the data source list
        private void reloadList() {
            // initialize list editing
            listView.BeginUpdate();
            listView.Items.Clear();

            // add all items
            ReadOnlyCollection<DBSourceInfo> sources = MovingPicturesCore.DataProviderManager.GetList(DisplayType);
            foreach (DBSourceInfo currSource in sources) {
                ListViewItem newItem = new ListViewItem();
                newItem.ToolTipText = String.Format("{0}\nAuthor: {1}", currSource.Provider.Description, currSource.Provider.Author);

                if (currSource.IsDisabled(DisplayType))
                    newItem.ForeColor = Color.LightGray;

                ListViewItem.ListViewSubItem nameItem = new ListViewItem.ListViewSubItem(newItem, currSource.Provider.Name);
                ListViewItem.ListViewSubItem versionItem = new ListViewItem.ListViewSubItem(newItem, currSource.Provider.Version);

                newItem.SubItems.Add(nameItem);
                newItem.SubItems.Add(versionItem);

                newItem.Tag = currSource;
                listView.Items.Add(newItem);
            }

            // end list editing
            listView.Sort();
            listView.EndUpdate();
        }

        private void raisePriorityButton_Click(object sender, EventArgs e) {
            foreach (ListViewItem currItem in listView.SelectedItems) {
                DBSourceInfo source = (DBSourceInfo)currItem.Tag;
                MovingPicturesCore.DataProviderManager.ChangePriority(source, DisplayType, true);
            }

            listView.Sort();
        }

        private void lowerPriorityButton_Click(object sender, EventArgs e) {
            foreach (ListViewItem currItem in listView.SelectedItems) {
                DBSourceInfo source = (DBSourceInfo)currItem.Tag;
                MovingPicturesCore.DataProviderManager.ChangePriority(source, DisplayType, false);
            }

            listView.Sort();
        }

        private void disableButton_Click(object sender, EventArgs e) {
            foreach (ListViewItem currItem in listView.SelectedItems) {
                DBSourceInfo source = (DBSourceInfo)currItem.Tag;
                MovingPicturesCore.DataProviderManager.SetDisabled(source, DisplayType, !source.IsDisabled(DisplayType));
            }

            reloadList();
        }


        private void movieDetailsToolStripMenuItem_Click(object sender, EventArgs e) {
            DisplayType = DataType.DETAILS;
        }

        private void coversToolStripMenuItem_Click(object sender, EventArgs e) {
            DisplayType = DataType.COVERS;
        }

        private void backdropsToolStripMenuItem_Click(object sender, EventArgs e) {
            DisplayType = DataType.BACKDROPS;
        }

        private void toggleDebugModeToolStripMenuItem_Click(object sender, EventArgs e) {
            DBSetting setting = MovingPicturesCore.SettingsManager["source_manager_debug"];
            setting.Value = !((bool)setting.Value);
            updateDebugModeMenuItem();

            Thread newThread = new Thread(new ThreadStart(updateProviderManager));
            newThread.Start();
        }

        private void updateProviderManager() {
            DBSetting setting = MovingPicturesCore.SettingsManager["source_manager_debug"];
            MovingPicturesCore.DataProviderManager.DebugMode = (bool)setting.Value;
        }

        private void updateDebugModeMenuItem() {
            bool debugActive = (bool)MovingPicturesCore.SettingsManager["source_manager_debug"].Value;
            if (debugActive) {
                debugIcon.Visible = true;
                enableDebugModeToolStripMenuItem.Text = "Disable Debug Mode";
                enableDebugModeToolStripMenuItem.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.bug;
            }
            else {
                debugIcon.Visible = false;
                enableDebugModeToolStripMenuItem.Text = "Enable Debug Mode";
                enableDebugModeToolStripMenuItem.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.grey_bug;
            }
        }

    }
    class ListViewItemComparer : IComparer {
        DBSourceInfoComparer comparer;

        public ListViewItemComparer(DataType sortType) {
            comparer = new DBSourceInfoComparer(sortType);
        }

        public int Compare(Object x, Object y) {
            return comparer.Compare(((DBSourceInfo)((ListViewItem)x).Tag), ((DBSourceInfo)((ListViewItem)y).Tag));
        } 
    }

}
