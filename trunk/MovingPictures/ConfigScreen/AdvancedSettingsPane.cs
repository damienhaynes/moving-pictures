using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.Database;
using System.Collections;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;
using Cornerstone.Database;
using Cornerstone.Database.Tables;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class AdvancedSettingsPane : UserControl {
        private SettingsManager settings;
        Dictionary<string, TreeNode> groups = new Dictionary<string, TreeNode>();
        
        public AdvancedSettingsPane() {
            InitializeComponent();

            // if we are in designer, break to prevent errors with rendering, it cant access the DB...
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;
        }

        public void populateTree(SettingsManager settings) {
            this.settings = settings;

            // stop redrawing and clear tree
            advancedSettingsTreeView.BeginUpdate();
            advancedSettingsTreeView.Nodes.Clear();

            // loop through all settings and add nodes for groups and settings accordingly.
            // maintain a dictionary of groups to ensure on one listing per group.
            foreach (DBSetting currSetting in settings.Values) {
                string groupKey = "";
                TreeNode parentNode = null;

                // loop through all parent groups of this setting and if no node has been made for any
                // pof them yet, go ahead and make a node
                foreach (string currGroup in currSetting.Grouping) {
                    groupKey += currGroup + '|';
                    if (!groups.ContainsKey(groupKey)) {
                        // create new node and place in TreeView
                        TreeNode newNode = new TreeNode(currGroup);
                        if (parentNode == null)
                            advancedSettingsTreeView.Nodes.Add(newNode);
                        else
                            parentNode.Nodes.Add(newNode);

                        newNode.BackColor = Color.LightGray;
                        newNode.NodeFont = new Font(this.Font.Name, this.Font.Size,
                                                    this.Font.Style, this.Font.Unit);

                        // update parentNode var (*since we arer walking up group chain), 
                        // and store node in our dictionary.
                        parentNode = newNode;
                        groups.Add(groupKey, newNode);
                    } else
                        parentNode = groups[groupKey];
                }

                // create a node for this setting an place it under it's parent group
                TreeNode settingNode = new TreeNode(currSetting.Name);
                settingNode.Tag = currSetting;
                
                if (parentNode == null)
                    advancedSettingsTreeView.Nodes.Add(settingNode);
                else
                    parentNode.Nodes.Add(settingNode);
            }

            // restrart drawing
            advancedSettingsTreeView.ExpandAll();
            advancedSettingsTreeView.EndUpdate();
            
        }

        private void advancedSettingsTreeView_DoubleClick(object sender, EventArgs e) {
            TreeNode selectedNode = advancedSettingsTreeView.SelectedNode;
            
            // if the user selected a settings node
            if (selectedNode != null && 
                selectedNode.Tag != null && 
                selectedNode.Tag.GetType() == typeof(DBSetting) ) {

                // launch the dialog to modify the selected setting
                SettingPopup newPopup = new SettingPopup();
                newPopup.Setting = (DBSetting) selectedNode.Tag;
                newPopup.ShowDialog();
            }
        }
    }
}
