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
using Cornerstone.GUI.Dialogs;
using System.Globalization;
using Cornerstone.Tools.Translate;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class DataSourcePane : UserControl {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<DBSourceInfo, ListViewItem> listItemLookup;

        private const string autoTransText = "Automatically Translate Results";

        [ReadOnly(true)]
        public DataType DisplayType {
            set {
                if (DesignMode) {
                    displayType = DataType.DETAILS;
                    return;
                }

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
        private DataType displayType = DataType.DETAILS;

        public DataSourcePane() {
            InitializeComponent();
            listItemLookup = new Dictionary<DBSourceInfo, ListViewItem>();
        }

        private void DataSourcePane_Load(object sender, EventArgs e) {
            if (!DesignMode) {
                // force an update on load
                DisplayType = displayType;
                updateDebugModeMenuItem();
                generatePrioritizeLanguagesList();

                updateTranslationMenuItem();
            }
        }

        private void updateTranslationMenuItem() {
            if (MovingPicturesCore.Settings.UseTranslator) {
                autoTranslateMenuItem.Checked = true;
                autoTranslateMenuItem.Text = autoTransText + ": " + MovingPicturesCore.Settings.TranslationLanguage;
            }
            else {
                autoTranslateMenuItem.Checked = false;
                autoTranslateMenuItem.Text = autoTransText;
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
                ListViewItem newItem = new ListViewItem(currSource.Provider.Name);
                newItem.ToolTipText = String.Format("{0}\nAuthor: {1}", currSource.Provider.Description, currSource.Provider.Author);

                ListViewItem.ListViewSubItem versionItem = new ListViewItem.ListViewSubItem(newItem, currSource.Provider.Version);
                ListViewItem.ListViewSubItem languageItem = new ListViewItem.ListViewSubItem(newItem, currSource.Provider.Language);

                ListViewItem.ListViewSubItem publishedItem;
                if (currSource.IsScriptable() && currSource.SelectedScript.Provider.Published != null) 
                    publishedItem = new ListViewItem.ListViewSubItem(newItem, currSource.SelectedScript.Provider.Published.Value.ToShortDateString());
                else
                    publishedItem = new ListViewItem.ListViewSubItem(newItem, "");

                newItem.SubItems.Add(versionItem);
                newItem.SubItems.Add(languageItem);
                newItem.SubItems.Add(publishedItem);
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
            bool debugActive = MovingPicturesCore.Settings.DataSourceDebugActive;
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

        private void generatePrioritizeLanguagesList() {
            autoLanguageMenuItem.DropDownItems.Clear();
            foreach (CultureInfo currCulture in MovingPicturesCore.DataProviderManager.GetAvailableLanguages()) {
                ToolStripMenuItem newMenuItem = new ToolStripMenuItem(currCulture.DisplayName);
                newMenuItem.Tag = currCulture;
                newMenuItem.Click += new EventHandler(languageMenuItem_Click);
                autoLanguageMenuItem.DropDownItems.Add(newMenuItem);
            }
        }

        private void languageMenuItem_Click(object sender, EventArgs e) {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            string languageCode = ((CultureInfo)(menuItem.Tag)).TwoLetterISOLanguageName;
            MovingPicturesCore.DataProviderManager.ArrangeDataProviders(languageCode);

            reloadList();
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
                if (!source.IsDisabled(DisplayType)) 
                    MovingPicturesCore.DataProviderManager.SetDisabled(source, DisplayType, true);
            }

            listView.Sort();
            listView.SelectedItems.Clear();
            if (listView.Items.Count > 0) listView.Items[0].Selected = true;
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
            MovingPicturesCore.Settings.DataSourceDebugActive = !MovingPicturesCore.Settings.DataSourceDebugActive;
            updateDebugModeMenuItem();

            Thread newThread = new Thread(new ThreadStart(reinitScrapers));
            newThread.Start();
        }

        private void reinitScrapers() {
            DBSetting setting = MovingPicturesCore.Settings["source_manager_debug"];
            MovingPicturesCore.DataProviderManager.DebugMode = (bool)setting.Value;
        }

        private void addButton_Click(object sender, EventArgs e) {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK) {
                logger.Info("New script specified by user: " + openFileDialog.FileName);

                // grab the contents of the file and try
                StreamReader reader = new StreamReader(openFileDialog.FileName);
                string script = reader.ReadToEnd();
                reader.Close();

                // and add it to the manager
                DataProviderManager.AddSourceResult addResult = MovingPicturesCore.DataProviderManager.AddSource(typeof(ScriptableProvider), script);

                if (addResult == DataProviderManager.AddSourceResult.FAILED_VERSION) {
                    MessageBox.Show("A script with this Version and ID is already loaded.", "Load Script Failed");
                }
                else if (addResult == DataProviderManager.AddSourceResult.FAILED_DATE) {
                    MessageBox.Show("This script does not have a unique 'published' date.", "Load Script Failed");
                }
                else if (addResult == DataProviderManager.AddSourceResult.FAILED) {
                    MessageBox.Show("The script is malformed or not a Moving Pictures script.", "Load Script Failed");
                }
                else if (addResult == DataProviderManager.AddSourceResult.SUCCESS_REPLACED) {
                    MessageBox.Show(
                        "Because you are in debug mode, this script has replaced\n" +
                        "an existing script with the same version. If you are a\n" +
                        "developer, please be sure to increment your version number\n" +
                        "and update your release date before distribution.", "Warning");
                }
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
            WorkerDelegate worker = new WorkerDelegate(MovingPicturesCore.DataProviderManager.LoadInternalProviders);
            ProgressPopup popup = new ProgressPopup(worker);
            popup.Owner = this.ParentForm;
            popup.ShowDialog();

            reloadList();
        }

        private void selectScriptVersionToolStripMenuItem_Click(object sender, EventArgs e) {
            if (listView.SelectedItems.Count != 0) 
                foreach (ListViewItem currItem in listView.SelectedItems) 
                    if (((DBSourceInfo)currItem.Tag).IsScriptable()) {
                        ScriptVersionPopup popup = new ScriptVersionPopup((DBSourceInfo)currItem.Tag);
                        popup.Owner = this.ParentForm;
                        popup.ShowDialog();
                    }

            reloadList();
        }

        private void settingsButton_ButtonClick(object sender, EventArgs e) {
            settingsButton.ShowDropDown();
        }

        private void enableToolStripMenuItem_Click(object sender, EventArgs e) {
            foreach (ListViewItem currItem in listView.SelectedItems) {
                DBSourceInfo source = (DBSourceInfo)currItem.Tag;
                if (source.IsDisabled(DisplayType))
                    MovingPicturesCore.DataProviderManager.SetDisabled(source, DisplayType, false);
            }

            listView.Sort();
            repaintListItems();
        }

        private void languageSplitButton_ButtonClick(object sender, EventArgs e) {
            languageSplitButton.ShowDropDown();
        }

        private void autoTranslateMenuItem_Click(object sender, EventArgs e) {
            if (MovingPicturesCore.Settings.UseTranslator) {
                MovingPicturesCore.Settings.UseTranslator = false;
                updateTranslationMenuItem();
                return;
            }

            TranslationPopup popup = new TranslationPopup();
            popup.Owner = FindForm();
            popup.ShowDialog();

            updateTranslationMenuItem();

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
