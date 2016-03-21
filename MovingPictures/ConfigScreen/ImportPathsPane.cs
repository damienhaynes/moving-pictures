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
using System.Diagnostics;
using Cornerstone.GUI.Dialogs;
using System.Threading;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class ImportPathsPane : UserControl {
        private List<DBImportPath> paths = new List<DBImportPath>();
        private BindingSource pathBindingSource;
        private Color normalColor;

        public ImportPathsPane() {
            InitializeComponent();

            // if we are in designer, break to prevent errors with rendering, it cant access the DB...
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;
        }

        private void ImportPathsPane_Load(object sender, EventArgs e) {
            if (DesignMode)
                return;

            // grab all user defined paths
            paths = DBImportPath.GetAllUserDefined();
            
            // get normal row color
            normalColor = pathsGridView.DefaultCellStyle.ForeColor;

            // set up the binding for the on screen control
            pathBindingSource = new BindingSource();
            pathBindingSource.DataSource = paths;
            pathBindingSource.ListChanged += new ListChangedEventHandler(pathBindingSource_ListChanged);

            // assign the bound list of paths to the control
            pathsGridView.AutoGenerateColumns = false;
            pathsGridView.DataSource = pathBindingSource;

            // link the checkbox to db settings
            importDvdCheckBox.Setting = MovingPicturesCore.Settings["importer_disc_enabled"];

            this.HandleDestroyed += new EventHandler(ImportPathsPane_HandleDestroyed);
        }

        // Commits new and existing items on addition or modification.
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
                DBImportPath newPath = DBImportPath.Get(folderDialog.SelectedPath);
                
                if (newPath.GetDriveType() == DriveType.CDRom) {
                    MessageBox.Show("Importing from this drive is controlled through the setting 'Enable Import Paths For Optical Drives'", "Not Allowed!");
                    return;
                }
                
                if (newPath.ID != null) {
                    MessageBox.Show("This import path is already loaded.");
                    return;
                }

                if (newPath.InternallyManaged != true) {
                    pathBindingSource.Add(newPath);
                    MovingPicturesCore.Importer.RestartScanner();
                }

            }
        }

        private void removeSourceButton_Click(object sender, EventArgs e) {
            if (pathBindingSource.Current != null) {
                DialogResult result = MessageBox.Show("This will remove from Moving Pictures all movies retrieved from this import path, are you sure?", "Warning!", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes) {
                    // stop the importer
                    MovingPicturesCore.Importer.Stop(); ;
                    
                    // remove the import path
                    ((DBImportPath)pathBindingSource.Current).Delete();
                    pathBindingSource.RemoveCurrent();
                    
                    // clean the database of the old movies using our progress bar popup
                    ProgressPopup progressPopup = new ProgressPopup(new WorkerDelegate(DatabaseMaintenanceManager.RemoveInvalidFiles));
                    DatabaseMaintenanceManager.MaintenanceProgress += new ProgressDelegate(progressPopup.Progress);
                    progressPopup.Owner = ParentForm;
                    progressPopup.Text = "Removing related movies...";
                    progressPopup.ShowDialog();

                    // restart the importer
                    MovingPicturesCore.Importer.RestartScanner();
                    
                }
            }
        }

        private void helpButton_Click(object sender, EventArgs e) {
            ProcessStartInfo processInfo = new ProcessStartInfo(Resources.MediaSourcesHelpURL);
            Process.Start(processInfo);
        }

        private void manuallyEnterMediaSourceToolStripMenuItem_Click(object sender, EventArgs e) {
            AddImportPathPopup addPopup = new AddImportPathPopup();
            addPopup.Owner = ParentForm;
            DialogResult result = addPopup.ShowDialog();
            if (result == DialogResult.OK) {
                DBImportPath newPath;
                try {
                    newPath = DBImportPath.Get(addPopup.SelectedPath);
                }
                catch (Exception ex){
                    if (ex is ThreadAbortException)
                        throw ex;
                    
                    MessageBox.Show("The path that you have entered is invalid.");
                    return;
                }
                
                if (newPath.Directory == null || !newPath.Directory.Exists) {
                    MessageBox.Show("The path that you have entered is invalid.");
                    return;
                }
                
                if (newPath.GetDriveType() == DriveType.CDRom) {
                    MessageBox.Show("Importing from this drive is controlled through the setting 'Enable Import Paths For Optical Drives'", "Not Allowed!");
                    return;
                }

                if (newPath.ID != null) {
                    MessageBox.Show("This import path is already loaded.");
                    return;
                }

                if (newPath.InternallyManaged != true) {
                    pathBindingSource.Add(newPath);
                    MovingPicturesCore.Importer.RestartScanner();
                }

            }

        }

        private void markAsReplacedToolStripMenuItem_Click(object sender, EventArgs e) {
            if (pathBindingSource.Current != null) {
                DBImportPath importPath = pathBindingSource.Current as DBImportPath;

                string message = "This will mark the import path as replaced. You can use this option to recover from a hardware replacement and are unable to recreate the same import path again. Movies that were previously on this import path will be moved to a new import path once they are detected during a scan. Continue?";
                if (importPath.Replaced) {
                    message = "This wil remove the replaced flag and return the import path back to normal. Continue?";
                }
                
                DialogResult result = MessageBox.Show(message, "Warning!", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes) {
                    // stop the importer
                    MovingPicturesCore.Importer.Stop(); ;

                    // mark as replaced
                    importPath.Replaced = !(importPath.Replaced);
                    importPath.Commit();

                    // restart the importer
                    MovingPicturesCore.Importer.RestartScanner();
                }
            }
        }

        private void pathsGridView_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e) {
            DataGridViewRow row = pathsGridView.Rows[e.RowIndex];
            DBImportPath path = row.DataBoundItem as DBImportPath;

            string toolTipText = path.FullPath + "\n" + "Drive Type: " + path.GetDriveType().ToString();
            if (path.IsRemovable)
                toolTipText += " (Removable)\nOnline: " + path.IsAvailable;
            
            if (path.Replaced) {
                toolTipText += " (Replaced)";
                row.DefaultCellStyle.ForeColor = Color.DarkGray;
            }
            else {
                row.DefaultCellStyle.ForeColor = normalColor;
            }

            // add tooltip text
            row.Cells[0].ToolTipText = toolTipText;
        }
    }
}
