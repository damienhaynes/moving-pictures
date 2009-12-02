using System;
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
using Cornerstone.Database.CustomTypes;

namespace Cornerstone.GUI.Filtering {
    internal struct NodeModifiedDetails {
        public int Index;
        public TreeNode Parent;
    }

    public delegate string TranslationParserDelegate(string input);

    public partial class GenericMenuTreePanel<T> : UserControl, IMenuTreePanel, IFieldDisplaySettingsOwner
        where T : DatabaseTable {

        private enum DropPositionEnum { Before, Inside, After, None }

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Dictionary<DBNode<T>, TreeNode> treeNodeLookup = new Dictionary<DBNode<T>,TreeNode>();
        private HashSet<DBNode<T>> updatingNodes = new HashSet<DBNode<T>>();
        private Dictionary<DBNode<T>, NodeModifiedDetails> modificationDetails = new Dictionary<DBNode<T>, NodeModifiedDetails>();
        private Stack<DBNode<T>> pendingModification = new Stack<DBNode<T>>();
        private Stack<DBNode<T>> finishedModification = new Stack<DBNode<T>>();

        TreeNode movingNode = null;       // node that is being dragged around by a drag and drop op
        TreeNode highlightedNode = null;  // node that is currently highlighted due to a drag and drop op
        TreeNode previousNode = null;     // last node to be hovered over in drag and drop op
        TreeNode targetNode = null;      // current node to be hovered over in drag and drop op

        DropPositionEnum dropPosition;
        Color originalForeColor;
        Color originalBackColor;

        Font bold;
        Font regular;

        bool dragBarVisible = false;
        bool rebuildingNode = false;

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

        public TranslationParserDelegate TranslationParser {
            get { return _translationParser; }
            set { _translationParser = value; }
        } TranslationParserDelegate _translationParser = null;

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
            if (treeView.SelectedNode != null && treeView.SelectedNode.Tag is DBNode<T>)
                node = (DBNode<T>)treeView.SelectedNode.Tag;
            if (node == null || node.DynamicNode)
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
                parent.Children.Normalize(true);
                newNode.Parent = parent;
                
                parentTreeNode.Nodes.Add(treeNode);
                parentTreeNode.Expand();
                setVisualProperties(parentTreeNode);
            }
            else {
                _menu.RootNodes.Add(newNode);
                _menu.RootNodes.Normalize(true);
                treeView.Nodes.Add(treeNode);
            }

            treeView.SelectedNode = treeNode;
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
                parent.Children.Normalize(true);
                newNode.Parent = parent;

                parentTreeNode.Nodes.Add(treeNode);
                parentTreeNode.Expand();
                setVisualProperties(parentTreeNode);
            }
            else {
                _menu.RootNodes.Add(newNode);
                _menu.RootNodes.Normalize(true);
                treeView.Nodes.Add(treeNode);
            }

            treeView.SelectedNode = treeNode;
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
                treeNode = new TreeNode();
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

                string displayName = node.Name;
                if (TranslationParser != null)
                    displayName = TranslationParser(node.Name);

                treeNode.Text = displayName;
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
                rebuildingNode = true;
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

            rebuildingNode = false;

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
            clearDragDropMarkup();

            if (dropPosition == DropPositionEnum.None)
                return;

            // grab the item being moved
            movingNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
            if (movingNode == null) return;

            // grab the target node
            Point pt = treeView.PointToClient(new Point(e.X, e.Y));
            targetNode = treeView.GetNodeAt(pt);

            // grab the old parent
            TreeNode oldParent = movingNode.Parent;

            // remove node from treeview and internal menu system
            if (oldParent != null) {
                oldParent.Nodes.Remove(movingNode);
                ((DBNode<T>)oldParent.Tag).Children.Remove((DBNode<T>)movingNode.Tag);
                setVisualProperties(oldParent);
            }
            else {
                treeView.Nodes.Remove(movingNode);
                _menu.RootNodes.Remove((DBNode<T>)movingNode.Tag);
            }

            // do the heavy lifting in a new thread while displaying a progress dialog
            ProgressPopup popup = new ProgressPopup(new WorkerDelegate(updateMovedNodeDragDrop));
            popup.Owner = FindForm();
            popup.Text = "Updating Moved Menu Item";
            
            // node will be reinserted by the callback method when work is complete
            popup.WorkComplete += new WorkCompleteDelegate(updateMovedNode_Complete);
            popup.ShowDialogDelayed(300);
        }

        private void updateMovedNodeDragDrop() {
            DBNode<T> targetDbNode = ((DBNode<T>)targetNode.Tag);
            DBNode<T> movingDbNode = ((DBNode<T>)movingNode.Tag);
            DBNode<T> parentDbNode = null; 

            // grab the collection of the parent if we are doing a before or after drop
            List<DBNode<T>> parentCollection = null;
            if (dropPosition == DropPositionEnum.Before || dropPosition == DropPositionEnum.After) {
                if (targetNode.Parent == null) {
                    parentCollection = _menu.RootNodes;
                    ((RelationList<DBMenu<T>, DBNode<T>>)parentCollection).CommitNeeded = true;
                }
                else {
                    parentCollection = targetDbNode.Parent.Children;
                    parentDbNode = targetDbNode;
                    ((RelationList<DBNode<T>, DBNode<T>>)parentCollection).CommitNeeded = true;
                }
            }

            // relocate node in internal menu system
            if (dropPosition == DropPositionEnum.Before) {
                parentCollection.Insert(parentCollection.IndexOf(targetDbNode), movingDbNode);
                movingDbNode.Parent = parentDbNode;
                parentCollection.Normalize(true);
            }
            else if (dropPosition == DropPositionEnum.After) {
                parentCollection.Insert(parentCollection.IndexOf(targetDbNode) + 1, movingDbNode);
                movingDbNode.Parent = parentDbNode;
                parentCollection.Normalize(true);
            }
            else {
                movingDbNode.Parent = targetDbNode;
                targetDbNode.Children.Add(movingDbNode);
                targetDbNode.Children.Normalize(true);
                targetDbNode.Children.CommitNeeded = true;
            }

            // rebuild visual properties and children of the moved node
            setVisualProperties(movingNode);
            updateFilteredItems(movingNode, false);          
        }

        private void updateMovedNode_Complete() {
            if (InvokeRequired) {
                Invoke(new WorkerDelegate(updateMovedNode_Complete));
                return;
            }

            // grab the collection of the parent (used if we are doing a before or after drop)
            TreeNodeCollection parentCollection;
            if (targetNode.Parent == null)
                parentCollection = treeView.Nodes;
            else
                parentCollection = targetNode.Parent.Nodes;

            // place the node back in the tree
            try {
                if (dropPosition == DropPositionEnum.Before)
                    parentCollection.Insert(parentCollection.IndexOf(targetNode), movingNode);
                else if (dropPosition == DropPositionEnum.After)
                    parentCollection.Insert(parentCollection.IndexOf(targetNode) + 1, movingNode);
                else {
                    targetNode.Nodes.Add(movingNode);
                }
            }
            catch (Exception) { }
        }

        private void treeView_DragOver(object sender, DragEventArgs e) {
            // grab the item being moved
            movingNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
            if (movingNode == null) return;

            // grab the node currently being hovered over
            Point mousePos = treeView.PointToClient(new Point(e.X, e.Y));
            targetNode = treeView.GetNodeAt(mousePos);

            // bounds for the current node being hovered over (if one exists)
            Point upperLeft = new Point();
            Point bottomRight = new Point();
            if (targetNode != null) {
                upperLeft = new Point(targetNode.Bounds.Left, targetNode.Bounds.Top);
                bottomRight = new Point(targetNode.Bounds.Right, targetNode.Bounds.Bottom);
            }

            // if we are hovering over a real node, it's not a child of the one we are moving, and the parent 
            // of the current node is not dynamic, then it's okay for a border drop (before or after a node)
            bool borderDropOk = targetNode != null &&   
                                targetNode != movingNode &&
                                !isChild(movingNode, targetNode) &&  
                                !((DBNode<T>)targetNode.Tag).AutoGenerated;

            // if we are hovering over a real node that is not the one we are moving or it's parent, child
            // or grandchild and it is not a dynamic node, then it's okay for an inner drop
            bool innerDropOk = targetNode != null &&
                               targetNode != movingNode &&
                               targetNode != movingNode.Parent &&
                               !isChild(movingNode, targetNode) &&
                               !((DBNode<T>)targetNode.Tag).DynamicNode &&
                               !((DBNode<T>)targetNode.Tag).AutoGenerated;


            // determine the action to be taken based on current status
            if (borderDropOk && mousePos.Y < upperLeft.Y + 4)
                dropPosition = DropPositionEnum.Before;
            else if (borderDropOk && !targetNode.IsExpanded && mousePos.Y > bottomRight.Y - 4)
                dropPosition = DropPositionEnum.After;
            else if (innerDropOk)
                dropPosition = DropPositionEnum.Inside;
            else
                dropPosition = DropPositionEnum.None;

            switch (dropPosition) {
                case DropPositionEnum.None:
                    e.Effect = DragDropEffects.None;
                    clearDragDropMarkup();
                    break;
                case DropPositionEnum.Before:
                case DropPositionEnum.After:
                    e.Effect = DragDropEffects.Move;
                    drawDragBar(dropPosition);
                    break;
                case DropPositionEnum.Inside:
                    e.Effect = DragDropEffects.Move;
                    highlightCurrentNode();
                    break;
            }

            previousNode = targetNode;
        }

        private void clearDragDropMarkup() {
            // reset previous node highlight color if needed
            if (highlightedNode != null) {
                highlightedNode.ForeColor = originalForeColor;
                highlightedNode.BackColor = originalBackColor;
                highlightedNode = null;
            }

            // remove the dragbar if needed
            if (dragBarVisible) {
                Refresh();
                dragBarVisible = false;
            }
        }

        private void drawDragBar(DropPositionEnum position) {
            if (previousNode != targetNode)
                Refresh();

            Point upperLeft = new Point(targetNode.Bounds.Left, targetNode.Bounds.Top);
            Point bottomRight = new Point(targetNode.Bounds.Right, targetNode.Bounds.Bottom);

            // reset previous node highlight color
            if (highlightedNode != null) {
                highlightedNode.ForeColor = originalForeColor;
                highlightedNode.BackColor = originalBackColor;
                highlightedNode = null;
            }

            int leftPos, rightPos, vertPos;
            leftPos = upperLeft.X - 22;
            rightPos = treeView.Width - 4;

            if (position == DropPositionEnum.Before) 
                vertPos = upperLeft.Y;
            else 
                vertPos = bottomRight.Y;

            // edge marker
            Point[] LeftTriangle = new Point[5]{
                new Point(leftPos, vertPos - 3), 
                new Point(leftPos, vertPos + 3), 
                new Point(leftPos + 3, vertPos), 
                new Point(leftPos + 3, vertPos - 1), 
                new Point(leftPos, vertPos - 4)};

            Point[] RightTriangle = new Point[5]{
                new Point(rightPos, vertPos - 3),
                new Point(rightPos, vertPos + 3),
                new Point(rightPos - 3, vertPos),
                new Point(rightPos - 3, vertPos - 1),
                new Point(rightPos, vertPos - 4)};

            Graphics g = treeView.CreateGraphics();
            g.FillPolygon(System.Drawing.Brushes.Black, LeftTriangle);
            g.FillPolygon(System.Drawing.Brushes.Black, RightTriangle);
            g.DrawLine(new System.Drawing.Pen(Color.Black, 2),
              new Point(leftPos, vertPos),
              new Point(rightPos, vertPos));

            dragBarVisible = true;
        }

        private void highlightCurrentNode() {
            // reset previous node highlight color
            if (highlightedNode != null && highlightedNode != targetNode) {
                highlightedNode.ForeColor = originalForeColor;
                highlightedNode.BackColor = originalBackColor;
                highlightedNode = null;
            }

            // store reversion settings and highlight the node currently hovering over
            if (highlightedNode == null) {
                highlightedNode = targetNode;
                originalForeColor = targetNode.ForeColor;
                originalBackColor = targetNode.BackColor;
                targetNode.BackColor = SystemColors.Highlight;
                targetNode.ForeColor = SystemColors.HighlightText;

                if (dragBarVisible) {
                    Refresh();
                    dragBarVisible = false;
                }
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

            DBNode<T> node = (DBNode<T>)treeView.SelectedNode.Tag;

            string displayName = node.Name;
            if (TranslationParser != null)
                displayName = TranslationParser(node.Name);

            if (e.Label != displayName)
                node.Name = e.Label;
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
            if (rebuildingNode)
                return;

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

        TranslationParserDelegate TranslationParser {
            get;
            set;
        } 
    }
}
