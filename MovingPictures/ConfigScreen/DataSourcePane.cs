using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.Database;
using System.Collections.ObjectModel;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class DataSourcePane : UserControl {
        public DataSourcePane() {
            InitializeComponent();
        }

        private void DataSourcePane_Load(object sender, EventArgs e) {
            if (!DesignMode) {
                reloadList(); 
            }
        }

        // load the data source list
        private void reloadList() {
            // initialize list editing
            listView.BeginUpdate();
            listView.Items.Clear();

            // add all items
            ReadOnlyCollection<DBSourceInfo> sources = MovingPicturesCore.DataProviderManager.MovieDetailSources;
            foreach (DBSourceInfo currSource in sources) {
                ListViewItem newItem = new ListViewItem();
                
                ListViewItem.ListViewSubItem nameItem = new ListViewItem.ListViewSubItem(newItem, currSource.Name);
                ListViewItem.ListViewSubItem versionItem = new ListViewItem.ListViewSubItem(newItem, currSource.SelectedScript.Version);
                //ListViewItem.ListViewSubItem languageItem = new ListViewItem.ListViewSubItem(newItem, currSource.Provider);
                
                newItem.SubItems.Add(nameItem);
                newItem.SubItems.Add(versionItem);

                newItem.Tag = currSource;
                listView.Items.Add(newItem);
            }

            // end list editing
            listView.EndUpdate();
        }

        
    }
}
