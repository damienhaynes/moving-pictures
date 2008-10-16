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
using NLog;
using System.IO;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class DataSourcePane : UserControl {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<DBSourceInfo, ListViewItem> listItemLookup;

        [ReadOnly(true)]
        public DataType DisplayType {
            set {
                if (DesignMode) {
                    displayType = DataType.DETAILS;
                    return;
                }

                if ((listView != null && listView.Items != null && listView.Items.Count != 0) && displayType == value)
                    return;

                displayType = value;

                switch (displayType) {
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

                listView.ListViewItemSorter = new ListViewItemComparer(displayType);
                reloadList();
            }

            get { return displayType; }
        }
        private DataType displayType;

        public DataSourcePane() {
            InitializeComponent();
            listItemLookup = new Dictionary<DBSourceInfo, ListViewItem>();
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

                ListViewItem.ListViewSubItem nameItem = new ListViewItem.ListViewSubItem(newItem, currSource.Provider.Name);
                ListViewItem.ListViewSubItem versionItem = new ListViewItem.ListViewSubItem(newItem, currSource.Provider.Version);

                newItem.SubItems.Add(nameItem);
                newItem.SubItems.Add(versionItem);

                newItem.Tag = currSource;
                listView.Items.Add(newItem);
                listItemLookup[currSource] = newItem;
            }

            // end list editing
            listView.Sort();
            repaintListItems();
            listView.EndUpdate();
        }

        private void repaintListItems() {
            foreach (DBSourceInfo currSource in listItemLookup.Keys) {
               
                if (currSource.IsDisabled(DisplayType))
                    listItemLookup[currSource].ForeColor = Color.LightGray;
                else
                    listItemLookup[currSource].ForeColor = Color.Black;

            }

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

        
        private void raisePriorityButton_Click(object sender, EventArgs e) {
            foreach (ListViewItem currItem in listView.SelectedItems) {
                DBSourceInfo source = (DBSourceInfo)currItem.Tag;
                MovingPicturesCore.DataProviderManager.ChangePriority(source, DisplayType, true);
            }

            listView.Sort();
            repaintListItems();
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

            listView.Sort();
            repaintListItems();
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

            Thread newThread = new Thread(new ThreadStart(reinitScrapers));
            newThread.Start();
        }

        private void reinitScrapers() {
            DBSetting setting = MovingPicturesCore.SettingsManager["source_manager_debug"];
            MovingPicturesCore.DataProviderManager.DebugMode = (bool)setting.Value;
        }

        private void addButton_Click(object sender, EventArgs e) {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK) {
                logger.Info("New script specified by user: " + openFileDialog.FileName);

                // grab the contents of the file and try to add it to the manager
                StreamReader reader = new StreamReader(openFileDialog.FileName);
                string script = reader.ReadToEnd();
                MovingPicturesCore.DataProviderManager.AddSource(typeof(ScriptableProvider), script);

                reloadList();
            }
        }

        private void removeButton_Click(object sender, EventArgs e) {
            DialogResult result = MessageBox.Show(
                "This will PERMANENTLY REMOVE the selected data source!\n" +
                "This action is irreversable. Normally the best choice is\n" +
                "to disable rather than remove a data source. Are you sure\n" +
                "you want to continue?", "Warning!", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
                foreach (ListViewItem currItem in listView.SelectedItems) {
                    DBSourceInfo source = (DBSourceInfo)currItem.Tag;
                    MovingPicturesCore.DataProviderManager.RemoveSource(source);
                    listView.Items.Remove(currItem);
                }
        }

        private void reloadDefaultSourcesToolStripMenuItem_Click(object sender, EventArgs e) {
            ProgressPopup.WorkerDelegate worker = new ProgressPopup.WorkerDelegate(MovingPicturesCore.DataProviderManager.LoadInternalProviders);
            ProgressPopup popup = new ProgressPopup(worker);
            popup.Owner = this.ParentForm;
            popup.ShowDialog();

            reloadList();
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
