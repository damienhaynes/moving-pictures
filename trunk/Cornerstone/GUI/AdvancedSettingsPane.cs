using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using Cornerstone.Database;
using Cornerstone.Database.Tables;

namespace Cornerstone.GUI {
    public partial class AdvancedSettingsPane : UserControl {
        private SettingsManager settings;
        private DBSetting selectedSetting;
        private Color validColor;
        private Color invalidColor;
        Dictionary<string, TreeNode> groups = new Dictionary<string, TreeNode>();
        
        public AdvancedSettingsPane() {
            InitializeComponent();

            invalidColor = Color.Red;
            validColor = setValueTextBox.ForeColor;

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
            foreach (DBSetting currSetting in settings.AllSettings) {
                string groupKey = "";
                TreeNode parentNode = null;

                // loop through all parent groups of this setting and if no node has been made for any
                // of them yet, go ahead and make a node
                foreach (string currGroup in currSetting.Grouping) {
                    groupKey += currGroup + '|';
                    if (!groups.ContainsKey(groupKey)) {
                        // create new node and place in TreeView
                        TreeNode newNode = new TreeNode(currGroup);
                        if (parentNode == null)
                            advancedSettingsTreeView.Nodes.Add(newNode);
                        else
                            parentNode.Nodes.Add(newNode);

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
                settingNode.NodeFont = new Font(this.Font.Name, this.Font.Size,
                                                FontStyle.Regular, this.Font.Unit);

                
                if (parentNode == null)
                    advancedSettingsTreeView.Nodes.Add(settingNode);
                else
                    parentNode.Nodes.Add(settingNode);
            }

            // restrart drawing
            advancedSettingsTreeView.ExpandAll();
            advancedSettingsTreeView.EndUpdate();
            
        }

        // if the user single clicks on an item, show the value of that item
        private void advancedSettingsTreeView_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e) {           
            TreeNode selectedNode = advancedSettingsTreeView.SelectedNode;                        
            // if the user selected a settings node
            if (selectedNode != null &&
                selectedNode.Tag != null &&
                selectedNode.Tag.GetType() == typeof(DBSetting)) 
            {

                setValueTextBox.Enabled = true;
                selectedSetting = (DBSetting)selectedNode.Tag;
                setValueTextBox.Text = selectedSetting.StringValue;
                setDescriptionLabel.Text = selectedSetting.Description;
            } 
            else {
                setValueTextBox.Enabled = false;
                setDescriptionLabel.Text = string.Empty;
                setValueTextBox.Text = String.Empty;
            }

            updateSettingButton.Enabled = false;
        }

        // if the value is valid and the user hits enter, go ahead and save the value.
        void setValueTextBox_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e) {
            if (e.KeyValue == 13 && valueIsValid()) 
                commitSetting();
        }

        // returns true if the value in the text box is a valid value for the setting
        // represented by this popup.
        private bool valueIsValid() {
            return selectedSetting.Validate(setValueTextBox.Text);
        }

        private void setValueTextBox_TextChanged(object sender, EventArgs e) {
            if (valueIsValid()) {
                setValueTextBox.ForeColor = validColor;
                updateSettingButton.Enabled = true;
            } else {
                setValueTextBox.ForeColor = invalidColor;
                updateSettingButton.Enabled = false;
            }
        }

        private void updateSettingButton_Click(object sender, EventArgs e) {
            if (valueIsValid()) 
                commitSetting();
        }

        private void commitSetting() {
            selectedSetting.Value = setValueTextBox.Text;
            selectedSetting.Commit();
            updateSettingButton.Enabled = false;
        }
    }
}
