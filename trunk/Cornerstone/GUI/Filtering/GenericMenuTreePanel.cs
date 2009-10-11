﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using Cornerstone.Database.Tables;
using Cornerstone.GUI.Controls;
using System.Threading;
using Cornerstone.Database;
using Cornerstone.Properties;
using Cornerstone.GUI.Dialogs;
using NLog;

namespace Cornerstone.GUI.Filtering {
    internal struct NodeModifiedDetails {
        public int Index;
        public TreeNode Parent;
    }

    public partial class GenericMenuTreePanel<T> : UserControl, IMenuTreePanel, IFieldDisplaySettingsOwner
        where T : DatabaseTable {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Dictionary<DBNode<T>, TreeNode> treeNodeLookup = new Dictionary<DBNode<T>,TreeNode>();
        private HashSet<DBNode<T>> updatingNodes = new HashSet<DBNode<T>>();
        private Dictionary<DBNode<T>, NodeModifiedDetails> modificationDetails = new Dictionary<DBNode<T>, NodeModifiedDetails>();
        private Stack<DBNode<T>> pendingModification = new Stack<DBNode<T>>();
        private Stack<DBNode<T>> finishedModification = new Stack<DBNode<T>>();

        TreeNode previousNode = null;
        Color previousForeColor;
        Color previousBackColor;

        Font bold;
        Font regular;

        public GenericMenuTreePanel() {
            InitializeComponent();

            ImageList imageList = new ImageList();
            imageList.Images.Add("error", Resources.bullet_error);
            imageList.Images.Add("folder", Resources.folder);
            imageList.Images.Add("filterFolder", Resources.folder_filter);
            imageList.Images.Add("endNode", Resources.bullet_go);
            imageList.Images.Add("filteredItem", Resources.bullet_black);

            treeView.ImageList = imageList;

            bold = new Font(treeView.Font.Name, treeView.Font.Size, FontStyle.Bold, treeView.Font.Unit);
            regular = new Font(treeView.Font.Name, treeView.Font.Size, FontStyle.Regular, treeView.Font.Unit);

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

        #region IMenuTreePanel Members

        public event DBNodeEventHandler SelectedNodeChanged;

        [Browsable(false)]
        public DatabaseManager DBManager {
            get {
                return _dbManager;
            }
            set {
                _dbManager = value;
            }
        } private DatabaseManager _dbManager;

        #endregion


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
            AddNodePopup popup = new AddNodePopup();
            popup.Table = typeof(T);

            DBNode<T> node = null;
            if (treeView.SelectedNode.Tag is DBNode<T>)
                node = (DBNode<T>)treeView.SelectedNode.Tag;
            if (node.DynamicNode)
                popup.ForceTopLevel = true;
            
            popup.ShowDialog();

            if (popup.CloseState == AddNodePopup.CloseStateEnum.ADD_BASIC) {
                addEmptyNode(popup.AddToRoot);
            } else if (popup.CloseState == AddNodePopup.CloseStateEnum.ADD_DYNAMIC) {
                addDynamicNode(popup.DynamicFilteringField, popup.DynamicFilteringRelation, popup.AddToRoot);
            }
        }

        private void removeNodeButton_Click(object sender, EventArgs e) {
            removeNode();
        }

        private void addEmptyNode(bool forceRoot) {
            if (_menu == null)
                return;

            // build internal node object
            DBNode<T> newNode = new DBNode<T>();
            newNode.Name = "New Node " + count;
            count++;

            // build treenode object (for the UI)
            TreeNode treeNode = createTreeNode(newNode);

            // find the appropriate parent
            TreeNode parentTreeNode = treeView.SelectedNode;
            DBNode<T> parent = parentTreeNode == null ? null : (DBNode<T>)parentTreeNode.Tag;

            while (parentTreeNode != null && (parent.AutoGenerated || parent.DynamicNode)) {
                parentTreeNode = parentTreeNode.Parent;
                parent = parentTreeNode == null ? null : (DBNode<T>)parentTreeNode.Tag;
            }
            
            // add the new node and tree node to the existing heirarchies.
            treeView.BeginUpdate();
            if (parentTreeNode != null && !forceRoot) {
                parent.Children.Add(newNode);
                newNode.Parent = parent;
                
                parentTreeNode.Nodes.Add(treeNode);
                parentTreeNode.Expand();
                setVisualProperties(parentTreeNode);
            }
            else {
                _menu.RootNodes.Add(newNode);
                treeView.Nodes.Add(treeNode);
            }

            treeView.EndUpdate();
        }

        private void addDynamicNode(DBField field, DBRelation relation, bool forceRoot) {
            if (_menu == null)
                return;

            SuspendLayout();

            // build internal node object
            DBNode<T> newNode = new DBNode<T>();
            newNode.Name = field.FriendlyName;
            newNode.BasicFilteringField = field;
            newNode.BasicFilteringRelation = relation;
            newNode.DynamicNode = true;
            newNode.DBManager = DBManager;
            
            count++;
            
            // build treenode object (for the UI)
            asyncInput = newNode;
            forceNewNodeRoot = forceRoot;
            ProgressPopup p = new ProgressPopup(new WorkerDelegate(createTreeNodeAsyncWorker));
            p.Owner = FindForm();
            p.Text = "Building Menu Tree";
            p.WorkComplete += new WorkCompleteDelegate(addDynamicNode_TreeNodeCreated);
            p.ShowDialogDelayed(300);
            
        }

        void addDynamicNode_TreeNodeCreated() {
            if (InvokeRequired) {
                Invoke(new WorkerDelegate(addDynamicNode_TreeNodeCreated));
                return;
            }

            logger.Debug("addDynamicNode_TreeNodeCreated");
            TreeNode treeNode = asyncOutput;
            DBNode<T> newNode = treeNode.Tag as DBNode<T>;

            // find the appropriate parent
            TreeNode parentTreeNode = treeView.SelectedNode;
            DBNode<T> parent = parentTreeNode == null ? null : (DBNode<T>)parentTreeNode.Tag;

            while (parentTreeNode != null && (parent.AutoGenerated || parent.DynamicNode)) {
                parentTreeNode = parentTreeNode.Parent;
                parent = parentTreeNode == null ? null : (DBNode<T>)parentTreeNode.Tag;
            }

            // add the new node and tree node to the existing heirarchies.
            treeView.BeginUpdate();
            if (parentTreeNode != null && !forceNewNodeRoot) {
                parent.Children.Add(newNode);
                newNode.Parent = parent;

                parentTreeNode.Nodes.Add(treeNode);
                parentTreeNode.Expand();
                setVisualProperties(parentTreeNode);
            }
            else {
                _menu.RootNodes.Add(newNode);
                treeView.Nodes.Add(treeNode);
            }

            treeView.EndUpdate();
            ResumeLayout();
        }

        private DBNode<T> asyncInput;
        private TreeNode asyncOutput;
        private bool forceNewNodeRoot;
        private void createTreeNodeAsyncWorker() {
            asyncOutput = createTreeNode(asyncInput);
        }

        private void removeNode() {
            if (treeView.SelectedNode != null) {
                DBNode<T> selectedNode = treeView.SelectedNode.Tag as DBNode<T>;
                if (selectedNode == null || selectedNode.AutoGenerated) return;

                if (treeView.SelectedNode.Parent == null) {
                    _menu.RootNodes.Remove(selectedNode);
                    treeView.Nodes.Remove(treeView.SelectedNode);
                }
                else {
                    ((DBNode<T>)treeView.SelectedNode.Parent.Tag).Children.Remove(selectedNode);
                    setVisualProperties(treeView.SelectedNode.Parent);
                    treeView.SelectedNode.Parent.Nodes.Remove(treeView.SelectedNode);
                }

                selectedNode.Delete();
            }
        }

        private void RepopulateTree() {
            if (_menu == null) return;

            // stop drawing and clear tree
            treeView.BeginUpdate();
            treeView.Nodes.Clear();

            // add root nodes. children will be recursively added as well
            _menu.RootNodes.Sort();
            foreach (DBNode<T> currNode in _menu.RootNodes) 
                treeView.Nodes.Add(createTreeNode(currNode));
            
            // restrart drawing
            treeView.ExpandAll();
            treeView.EndUpdate();
        }

        private TreeNode createTreeNode(DBNode<T> node) {
            TreeNode treeNode;
            if (treeNodeLookup.ContainsKey(node))
                return treeNodeLookup[node];
            else {
                treeNode = new TreeNode(node.Name);
                treeNode.Tag = node;
                treeNodeLookup[node] = treeNode;
            }

            node.UpdateDynamicNode();
            node.Children.Sort();

            setVisualProperties(treeNode);
            updateFilteredItems(treeNode, false);

            // add any missing children
            foreach (DBNode<T> currSubNode in node.Children) {
                TreeNode child = createTreeNode(currSubNode);
                if (!treeNode.Nodes.Contains(child))
                    treeNode.Nodes.Add(child);
            }

            treeNode.Collapse();
            node.Modified += new DBNodeEventHandler(nodeModified);

            return treeNode;
        }

        private void updateFilteredItems(TreeNode treeNode, bool forcePopulation) {            
            DBNode<T> node = treeNode.Tag as DBNode<T>;

            // make sure we have an updatable node and we are not already processing
            if (node == null) return;
            if (updatingNodes.Contains(node)) return;
            updatingNodes.Add(node);

            // if this is a dynamic node, update the children at model level
            if (node.DynamicNode) 
                node.UpdateDynamicNode();

            // if the node is not expanded and is a leaf node, don't worry about populating it for now
            if (!forcePopulation && !treeNode.IsExpanded && node.Children.Count == 0 && node.Filter != null) {
                if (treeNode.Nodes.Count == 0)
                    treeNode.Nodes.Add(new TreeNode("dummy"));

                updatingNodes.Remove(node);
                return;
            }

            // if we are populating the node and there is a dummy node in there, get rid of it
            if (treeNode.Nodes.Count == 1 && treeNode.Nodes[0].Tag == null)
                treeNode.Nodes.Clear();

            // if this is a dynamic node, update the children and remove any that are
            // no longer needed
            if (node.DynamicNode) {
                // using a slightly slower method here to preserve sorted order

                //List<TreeNode> subNodesToRemove = new List<TreeNode>();
                //foreach (TreeNode currSubNode in treeNode.Nodes)
                //    if (!node.Children.Contains(currSubNode.Tag as DBNode<T>))
                //        subNodesToRemove.Add(currSubNode);

                //foreach (TreeNode currSubNode in subNodesToRemove)
                //    treeNode.Nodes.Remove(currSubNode);
                treeNode.Nodes.Clear();

                // add any missing children
                foreach (DBNode<T> currSubNode in node.Children) {
                    TreeNode child = createTreeNode(currSubNode);
                    if (!treeNode.Nodes.Contains(child))
                        treeNode.Nodes.Add(child);
                }
            }

            // if this node has children, delegate to sub node processing and exit
            if (node.Children.Count > 0) {
                foreach (DBNode<T> currSubNode in node.Children)
                    if (treeNodeLookup.ContainsKey(currSubNode))
                        updateFilteredItems(treeNodeLookup[currSubNode], false);

                updatingNodes.Remove(node);
                return;
            }

            // get list of existing leaf nodes
            List<TreeNode> filteredItemsToRemove = new List<TreeNode>();
            foreach (TreeNode currSubNode in treeNode.Nodes) {
                if (currSubNode.Tag == null || currSubNode.Tag is T)
                    filteredItemsToRemove.Add(currSubNode);
            }

            // and get rid of them
            foreach (TreeNode currSubNode in filteredItemsToRemove)
                treeNode.Nodes.Remove(currSubNode);

            // then readd add all leaf nodes
            node.DBManager = DBManager;
            if (node.Children.Count == 0 && node.Filter != null) {
                foreach (T currItem in node.GetFilteredItems().OrderBy((item) => item.ToString())) {
                    TreeNode itemNode = new TreeNode(currItem.ToString());
                    itemNode.Tag = currItem;
                    treeNode.Nodes.Add(itemNode);
                    setVisualProperties(itemNode);

                }
            }

            updatingNodes.Remove(node);
        }

        private void setVisualProperties(TreeNode treeNode) {
            DBNode<T> node = null;
            if (treeNode.Tag is DBNode<T>) {
                node = (DBNode<T>)treeNode.Tag;
                treeNode.Text = node.Name;
            }

            if (node != null && node.Children.Count > 0 && node.Filter != null) {
                treeNode.ImageKey = "filterFolder";
                treeNode.SelectedImageKey = "filterFolder";
            }
            else if (node != null && node.Children.Count > 0) {
                treeNode.ImageKey = "folder";
                treeNode.SelectedImageKey = "folder";
            }
            else if (node != null && node.Filter != null) {
                treeNode.ImageKey = "endNode";
                treeNode.SelectedImageKey = "endNode";
            }
            else if (node != null) {
                treeNode.ImageKey = "error";
                treeNode.SelectedImageKey = "error";
            }
            else {
                treeNode.ImageKey = "filteredItem";
                treeNode.SelectedImageKey = "filteredItem";
            }

            // mark as bold if the node has children
            if (node != null && node.Children.Count > 0)
                treeNode.NodeFont = bold;
            else
                treeNode.NodeFont = regular;

            // mark a node in red if it has no children and no filters attached
            // mark a node teal if it has children and a filter attaches as a 
            // visual reminder that the node will be doing some filtering
            if (node != null && node.Children.Count == 0 && node.Filter == null)
                treeNode.ForeColor = Color.Red;
            else if (node != null && node.DynamicNode)
                treeNode.ForeColor = Color.Teal;
            else if (treeNode.Tag is T)
                treeNode.ForeColor = Color.DarkGray;
            else
                treeNode.ForeColor = treeView.ForeColor;
        }

        private void updateButtonStates() {
            DBNode<T> node = null;
            if (treeView.SelectedNode.Tag is DBNode<T>) 
                node = (DBNode<T>)treeView.SelectedNode.Tag;

            if (node == null || node.AutoGenerated) {
                addNodeButton.Enabled = false;
                removeNodeButton.Enabled = false;
                convertToRegularMenuItemButton.Enabled = false;
            }
            else if (node.DynamicNode) {
                addNodeButton.Enabled = true;
                removeNodeButton.Enabled = true;
                convertToRegularMenuItemButton.Enabled = true;
            }
            else {
                addNodeButton.Enabled = true;
                removeNodeButton.Enabled = true;
                convertToRegularMenuItemButton.Enabled = false;
            }
        }

        void nodeModified(IDBNode node, Type type) {
            if (modificationDetails.ContainsKey((DBNode<T>)node))
                return;

            if (treeNodeLookup.ContainsKey((DBNode<T>)node) && this.Visible) {
                // store modification details
                TreeNode treeNode =  treeNodeLookup[(DBNode<T>)node];
                NodeModifiedDetails details = new NodeModifiedDetails();
                details.Index = treeNode.Index;
                details.Parent = treeNode.Parent;
                modificationDetails[(DBNode<T>)node] = details;
                pendingModification.Push((DBNode<T>)node);

                // temporarily remove the node
                if (details.Parent == null)
                    treeView.Nodes.Remove(treeNode);
                else
                    details.Parent.Nodes.Remove(treeNode);

                // add a dummy node so the user doesnt freak out

                // update the node in a new thread while displaying a progress dialog
                ProgressPopup popup = new ProgressPopup(new WorkerDelegate(nodeModifiedWorker));
                popup.Owner = FindForm();
                popup.Text = "Updating Menu Items";
                popup.WorkComplete += new WorkCompleteDelegate(modifyingNode_Complete);
                popup.ShowDialogDelayed(300);
            }
        }

        private void nodeModifiedWorker() {
            DBNode<T> node = pendingModification.Pop();

            setVisualProperties(treeNodeLookup[node]);
            updateFilteredItems(treeNodeLookup[node], false);

            finishedModification.Push(node);

        }

        private void modifyingNode_Complete() {
            if (InvokeRequired) {
                Invoke(new WorkerDelegate(modifyingNode_Complete));
                return;
            }

            if (finishedModification.Count == 0)
                return;

            DBNode<T> node = finishedModification.Pop();
            TreeNode treeNode = treeNodeLookup[node];
            NodeModifiedDetails details = modificationDetails[node];

            // re-add the node
            try {
                if (details.Parent == null)
                    treeView.Nodes.Insert(details.Index, treeNode);
                else
                    details.Parent.Nodes.Insert(details.Index, treeNode);
            }
            catch (Exception e) {
                if (e is ThreadAbortException)
                    throw e;
            }


            treeView.SelectedNode = treeNode;
            modificationDetails.Remove(node);
        }

        private void treeView_ItemDrag(object sender, ItemDragEventArgs e) {
            TreeNode node = e.Item as TreeNode;

            if (node != null && node.Tag is DBNode<T> && !((DBNode<T>)node.Tag).AutoGenerated) {
                DoDragDrop(e.Item, DragDropEffects.Move);
            }
        }

        private void treeView_DragDrop(object sender, DragEventArgs e) {
            // reset highlight color for last highlighted node
            if (previousNode != null) {
                previousNode.ForeColor = previousForeColor;
                previousNode.BackColor = previousBackColor;
            }

            // grab the item being moved
            TreeNode movedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
            if (movedNode == null) return;

            // grab the new parent node
            Point pt = treeView.PointToClient(new Point(e.X, e.Y));
            TreeNode newParent = treeView.GetNodeAt(pt);

            // grab the old parent
            TreeNode oldParent = movedNode.Parent;

            if (oldParent == newParent || newParent == movedNode)
                return;

            // remove from old parent node
            if (oldParent != null) {
                oldParent.Nodes.Remove(movedNode);
                ((DBNode<T>)oldParent.Tag).Children.Remove((DBNode<T>)movedNode.Tag);
                setVisualProperties(oldParent);
            }
            // or remove from treeview root if necessary
            else {
                treeView.Nodes.Remove(movedNode);
                _menu.RootNodes.Remove((DBNode<T>)movedNode.Tag);
            }

            // move to the root
            if (newParent == null) {
                _menu.RootNodes.Add((DBNode<T>)movedNode.Tag);
                ((DBNode<T>)movedNode.Tag).Parent = null;
            }
            // or move to new node if necessary
            else {
                ((DBNode<T>)newParent.Tag).Children.Add((DBNode<T>)movedNode.Tag);
                ((DBNode<T>)movedNode.Tag).Parent = (DBNode<T>)newParent.Tag;
            }

            // update the node in a new thread while displaying a progress dialog
            this.movedNode = movedNode;
            this.newParent = newParent;
            ProgressPopup popup = new ProgressPopup(new WorkerDelegate(updateMovedNodeDragDrop));
            popup.Owner = FindForm();
            popup.Text = "Updating Moved Menu Item";
            popup.WorkComplete += new WorkCompleteDelegate(updateMovedNode_Complete);
            popup.ShowDialogDelayed(300);
        }

        TreeNode movedNode;
        TreeNode newParent;
        private void updateMovedNodeDragDrop() {
            setVisualProperties(movedNode);
            updateFilteredItems(movedNode, false);          
        }

        private void updateMovedNode_Complete() {
            if (InvokeRequired) {
                Invoke(new WorkerDelegate(updateMovedNode_Complete));
                return;
            }

            // move to the root
            if (newParent == null)
                try {
                    treeView.Nodes.Add(movedNode);
                }
                catch (Exception ) { }
            // or move to new node if necessary
            else {
                try {
                    newParent.Nodes.Add(movedNode);
                    setVisualProperties(newParent);
                }
                catch (Exception) { }
            }
        }

        private void treeView_DragOver(object sender, DragEventArgs e) {
            // grab the node currently being hovered over
            Point pt = treeView.PointToClient(new Point(e.X, e.Y));
            TreeNode newParent = treeView.GetNodeAt(pt);

            // grab the item being moved
            TreeNode selectedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
            if (selectedNode == null) return;

            // if hovering over itself a child, or own parent, dont accept drops
            if (newParent == selectedNode || newParent == selectedNode.Parent || isChild(selectedNode, newParent)) {
                e.Effect = DragDropEffects.None;

                // reset previous node highlight color
                if (previousNode != null) {
                    previousNode.ForeColor = previousForeColor;
                    previousNode.BackColor = previousBackColor;
                }

                return;
            }
            else
                e.Effect = DragDropEffects.Move;

            // reset previous node highlight color
            if (previousNode != null && previousNode != newParent) {
                previousNode.ForeColor = previousForeColor;
                previousNode.BackColor = previousBackColor;
            }

            // store reversion settings and highlight the node currently hovering over
            if (newParent != null && previousNode != newParent) {
                previousNode = newParent;
                previousForeColor = newParent.ForeColor;
                previousBackColor = newParent.BackColor;
                newParent.BackColor = SystemColors.Highlight;
                newParent.ForeColor = SystemColors.HighlightText;

            }

        }

        private void treeView_DragEnter(object sender, DragEventArgs e) {
            e.Effect = DragDropEffects.Move;
        }

        private bool isChild(TreeNode parent, TreeNode child) {
            foreach (TreeNode currNode in parent.Nodes) {
                if (currNode == child)
                    return true;

                if (isChild(currNode, child))
                    return true;
            }

            return false;
        }

        private void treeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e) {
            if (e.CancelEdit || e.Label == null)
                return;

            ((DBNode<T>)treeView.SelectedNode.Tag).Name = e.Label;
        }

        private void treeView_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.F2) {
                treeView.SelectedNode.BeginEdit();
            }
            else if (e.KeyCode == Keys.Delete) {
                removeNode();
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e) {
            if (treeView.SelectedNode.Tag is DBNode<T>) {
                if (SelectedNodeChanged != null)
                    SelectedNodeChanged((IDBNode)treeView.SelectedNode.Tag, typeof(T));
            }

            updateButtonStates();
        }

        private void treeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e) {
            if (treeView.SelectedNode.Tag is T) {
                e.CancelEdit = true;
            }
        }

        private void convertToRegularMenuItemToolStripMenuItem_Click(object sender, EventArgs e) {            
            if (treeView.SelectedNode == null || !(treeView.SelectedNode.Tag is DBNode<T>)) 
                return;

            DBNode<T> node = treeView.SelectedNode.Tag as DBNode<T>;
            if (!node.DynamicNode)
                return;

            DialogResult result = MessageBox.Show("This will permanently change this dynamic menu item\n" +
                            "to a standard menu item. This means sub elements will no\n" +
                            "longer be automatically generated. This also means you\n" +
                            "can manually modify the menu items.\n\n" +
                            "Do you want to continue?", "Change to Normal Menu Item?", MessageBoxButtons.YesNo);

            if (result == DialogResult.No)
                return;

            node.DynamicNode = false;
            setVisualProperties(treeNodeLookup[node]);

            foreach (DBNode<T> currSubNode in node.Children) {
                currSubNode.AutoGenerated = false;
                setVisualProperties(treeNodeLookup[currSubNode]);
            }
        }

        private void advancedButton_ButtonClick(object sender, EventArgs e) {
            advancedButton.ShowDropDown();
        }

        private void treeView_BeforeSelect(object sender, TreeViewCancelEventArgs e) {
            if (e.Node.Tag is T) {
                if (SelectedNodeChanged != null)
                    SelectedNodeChanged(null, typeof(T));
            }
        }

        private void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e) {
            DBNode<T> node = e.Node.Tag as DBNode<T>;
            if (node == null)
                return;

            if (node.Children.Count == 0) {
                updateFilteredItems(e.Node, true);
            }
        }

        private void moveUpButton_Click(object sender, EventArgs e) {
            if (treeView.SelectedNode == null || !(treeView.SelectedNode.Tag is DBNode<T>))
                return;

            // grab the dbnode
            DBNode<T> node = treeView.SelectedNode.Tag as DBNode<T>;
            
            // and the collection it belongs to
            List<DBNode<T>> collection;
            if (node.Parent == null) collection = _menu.RootNodes;
            else collection = node.Parent.Children;

            // attempt to move the node in the collection and if successful reflect the change on the GUI
            if (collection.MoveUp(node, true)) {
                if (node.Parent != null)
                    node.Parent.Commit();
                else
                    _menu.Commit();

                TreeNode treeNode = treeView.SelectedNode;
                
                TreeNodeCollection treeCollection;
                if (treeNode.Parent == null) treeCollection = treeView.Nodes;
                else treeCollection = treeNode.Parent.Nodes;

                int newIndex = treeCollection.IndexOf(treeNode) - 1;
                treeCollection.RemoveAt(newIndex + 1);
                treeCollection.Insert(newIndex, treeNode);

                treeView.SelectedNode = treeNode;
            }
        }

        private void moveDownButton_Click(object sender, EventArgs e) {
            if (treeView.SelectedNode == null || !(treeView.SelectedNode.Tag is DBNode<T>))
                return;

            // grab the dbnode
            DBNode<T> node = treeView.SelectedNode.Tag as DBNode<T>;

            // and the collection it belongs to
            List<DBNode<T>> collection;
            if (node.Parent == null) collection = _menu.RootNodes;
            else collection = node.Parent.Children;

            // attempt to move the node in the collection and if successful 
            // commit and reflect the change on the GUI
            if (collection.MoveDown(node, true)) {
                if (node.Parent != null)
                    node.Parent.Commit();
                else
                    _menu.Commit();

                TreeNode treeNode = treeView.SelectedNode;

                TreeNodeCollection treeCollection;
                if (treeNode.Parent == null) treeCollection = treeView.Nodes;
                else treeCollection = treeNode.Parent.Nodes;

                int newIndex = treeCollection.IndexOf(treeNode) + 1;
                treeCollection.RemoveAt(newIndex - 1);
                treeCollection.Insert(newIndex, treeNode);

                treeView.SelectedNode = treeNode;
            }
        }   
    }

    public interface IMenuTreePanel {
        event DBNodeEventHandler SelectedNodeChanged;

        IDBMenu Menu {
            get;
            set;
        }

        DatabaseManager DBManager {
            get;
            set;
        }
    }
}
