using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;

using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;
using MediaPortal.Plugins.MovingPictures.Properties;
using Cornerstone.Database;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class ImportPathsPane : UserControl {
        private List<DBImportPath> paths = new List<DBImportPath>();
        private BindingSource pathBindingSource;
        
        public ImportPathsPane() {
            InitializeComponent();

            // if we are in designer, break to prevent errors with rendering, it cant access the DB...
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            // grab all user defined paths
            paths = DBImportPath.GetAllUserDefined();

            // set up the binding for the on screen control
            pathBindingSource = new BindingSource();
            pathBindingSource.DataSource = paths;
            pathBindingSource.ListChanged += new ListChangedEventHandler(pathBindingSource_ListChanged);

            // assign the bound list of paths to the control
            pathsGridView.AutoGenerateColumns = false;
            pathsGridView.DataSource = pathBindingSource;

            // link the checkbox to db settings
            importDvdCheckBox.Setting = MovingPicturesCore.SettingsManager["importer_disc_enabled"];

            this.HandleDestroyed += new EventHandler(ImportPathsPane_HandleDestroyed);
        }

        private void addTooltips() {
            foreach (DataGridViewRow currRow in pathsGridView.Rows) {
                DBImportPath path = (DBImportPath) currRow.DataBoundItem;

                string toolTipText = path.FullPath + "\n" + "Drive Type: " + path.GetDriveType().ToString();
                if (path.IsRemovable)
                    toolTipText += " (Removable)\nOnline: " + path.IsAvailable;
                
                currRow.Cells[0].ToolTipText = toolTipText;
            }
        }

        // Commits new and existing itmes on addition or modification.
        void pathBindingSource_ListChanged(object sender, ListChangedEventArgs e) {
            if (e.ListChangedType != ListChangedType.ItemDeleted) {

                DBImportPath changedObj = (DBImportPath)pathBindingSource[e.NewIndex];
                changedObj.Commit();
            }
        }

        // If this pane has been destroyed, that means the Config screen has shut down, so 
        // the importer also should be  stopped.
        void ImportPathsPane_HandleDestroyed(object sender, System.EventArgs e) {
            pathsGridView.EndEdit();
        }
                
        private void addSourceButton_Click(object sender, EventArgs e) {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            DialogResult result = folderDialog.ShowDialog();
            if (result == DialogResult.OK) {
                DBImportPath newPath = new DBImportPath();
                newPath.FullPath = folderDialog.SelectedPath;
                if (!newPath.System) {
                    pathBindingSource.Add(newPath);
                    MovingPicturesCore.Importer.RestartScanner();
                    addTooltips();
                }
                else {
                    newPath = null;
                    MessageBox.Show("Importing from this drive is controlled through the setting 'Enable Import Paths For Optical Drives'", "Not Allowed!");
                }
            }
            
        }

        private void removeSourceButton_Click(object sender, EventArgs e) {
            if (pathBindingSource.Current != null) {
                DialogResult result = MessageBox.Show("This will remove from Moving Pictures all movies retrieved from this import path, are you sure?", "Warning!", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes) {
                    ((DBImportPath)pathBindingSource.Current).Delete();
                    pathBindingSource.RemoveCurrent();

                    MovingPicturesCore.Importer.DoFileMaintenance();
                    MovingPicturesCore.Importer.RestartScanner();
                    
                }
            }
        }

        private void helpButton_Click(object sender, EventArgs e) {
            // show the help content fo the Media Sources section
            Help.ShowHelp(ParentForm, MovingPicturesCore.SettingsManager["help_file"].StringValue, HelpNavigator.Topic, "2_MediaSources.htm");
        }

        private void ImportPathsPane_Load(object sender, EventArgs e) {
            addTooltips();
        }
    }
}
