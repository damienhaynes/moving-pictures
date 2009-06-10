using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Cornerstone.Database.Tables;
using Cornerstone.GUI.Controls;

namespace Cornerstone.GUI.Filtering {
    public partial class GenericMenuTreePanel<T> : UserControl, IMenuTreePanel, IFieldDisplaySettingsOwner
        where T : DatabaseTable {

        private Dictionary<DBNode<T>, TreeNode> treeNodeLookup;

        public GenericMenuTreePanel() {
            InitializeComponent();
        }

        #region IFieldDisplaySettingsOwner Members

        [Category("Cornerstone Settings")]
        [Description("Manage the type of database table this control connects to and which fields should be displayed.")]
        public FieldDisplaySettings FieldDisplaySettings {
            get {
                if (_fieldSettings == null) {
                    _fieldSettings = new FieldDisplaySettings();
                    _fieldSettings.Owner = this;
                }

                return _fieldSettings;
            }

            set {
                _fieldSettings = value;
                _fieldSettings.Owner = this;
            }
        } private FieldDisplaySettings _fieldSettings = null;

        public void OnFieldPropertiesChanged() {
            //throw new NotImplementedException();
        }

        #endregion

        #region IMenuTreePanel Members

        public IDBMenu Menu {
            get { return _menu; }
            set { 
                _menu = (DBMenu<T>)value;
                RepopulateTree();
            }
        } private DBMenu<T> _menu;

        #endregion

        private static int count = 0;
        private void addNodeButton_Click(object sender, EventArgs e) {
            if (_menu == null)
                return;

            // build internal node object
            DBNode<T> newNode = new DBNode<T>();
            newNode.Name = "New Node " + count;
            count++;

            // build treenode object (for the UI)
            TreeNode treeNode = new TreeNode(newNode.Name);
            treeNode.Tag = newNode;

            // add the new node and tree node to the existing heirarchies.
            treeView1.BeginUpdate();
            if (treeView1.SelectedNode != null) {
                ((DBNode<T>)treeView1.SelectedNode.Tag).Children.Add(newNode);
                treeView1.SelectedNode.Nodes.Add(treeNode);
                treeView1.SelectedNode.Expand();
            }
            else {
                _menu.RootNodes.Add(newNode);
                treeView1.Nodes.Add(treeNode);
            }
            treeView1.EndUpdate();

            _menu.Commit();
        }

        private void removeNodeButton_Click(object sender, EventArgs e) {
            if (treeView1.SelectedNode != null) {
                DBNode<T> selectedNode = (DBNode<T>)treeView1.SelectedNode.Tag;

                if (treeView1.SelectedNode.Parent == null)
                    _menu.RootNodes.Remove(selectedNode);
                else 
                    ((DBNode<T>)treeView1.SelectedNode.Parent.Tag).Children.Remove(selectedNode);
            }

            _menu.Commit();
            RepopulateTree();
        }

        private void RepopulateTree() {
            if (_menu == null) return;

            // stop drawing and clear tree
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();

            foreach (DBNode<T> currNode in _menu.RootNodes) {
                addNode(treeView1.Nodes, currNode);
            }

            // restrart drawing
            treeView1.ExpandAll();
            treeView1.EndUpdate();
        }

        private void addNode(TreeNodeCollection nodeCollection, DBNode<T> node) {
            TreeNode treeNode = new TreeNode(node.Name);
            treeNode.Tag = node;

            nodeCollection.Add(treeNode);
            foreach (DBNode<T> currSubNode in node.Children) {
                addNode(treeNode.Nodes, currSubNode);
            }
        }

    }

    public interface IMenuTreePanel {
        IDBMenu Menu {
            get;
            set;
        }
    }
}
