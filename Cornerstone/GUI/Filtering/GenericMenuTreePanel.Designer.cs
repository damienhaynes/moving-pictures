namespace Cornerstone.GUI.Filtering {
    partial class GenericMenuTreePanel<T> {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }

            if (disposing)
                removeEventHandlers();

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.treeView = new System.Windows.Forms.TreeView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.addNodeButton = new System.Windows.Forms.ToolStripButton();
            this.removeNodeButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.moveUpButton = new System.Windows.Forms.ToolStripButton();
            this.moveDownButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.advancedButton = new System.Windows.Forms.ToolStripSplitButton();
            this.convertToRegularMenuItemButton = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.AllowDrop = true;
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeView.HideSelection = false;
            this.treeView.LabelEdit = true;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(190, 340);
            this.treeView.TabIndex = 0;
            this.treeView.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeView_AfterLabelEdit);
            this.treeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView_BeforeExpand);
            this.treeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeView_DragDrop);
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            this.treeView.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeView_DragEnter);
            this.treeView.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeView_BeforeLabelEdit);
            this.treeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView_BeforeSelect);
            this.treeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeView_KeyDown);
            this.treeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView_ItemDrag);
            this.treeView.DragOver += new System.Windows.Forms.DragEventHandler(this.treeView_DragOver);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Right;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNodeButton,
            this.removeNodeButton,
            this.toolStripSeparator2,
            this.moveUpButton,
            this.moveDownButton,
            this.toolStripSeparator1,
            this.advancedButton});
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.toolStrip1.Location = new System.Drawing.Point(190, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(33, 340);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // addNodeButton
            // 
            this.addNodeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.addNodeButton.Image = global::Cornerstone.Properties.Resources.list_add;
            this.addNodeButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.addNodeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addNodeButton.Name = "addNodeButton";
            this.addNodeButton.Size = new System.Drawing.Size(31, 20);
            this.addNodeButton.Text = "Add Menu Item";
            this.addNodeButton.Click += new System.EventHandler(this.addNodeButton_Click);
            // 
            // removeNodeButton
            // 
            this.removeNodeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.removeNodeButton.Image = global::Cornerstone.Properties.Resources.list_remove;
            this.removeNodeButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.removeNodeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.removeNodeButton.Name = "removeNodeButton";
            this.removeNodeButton.Size = new System.Drawing.Size(31, 20);
            this.removeNodeButton.Text = "Remove Node";
            this.removeNodeButton.ToolTipText = "Remove Menu Item";
            this.removeNodeButton.Click += new System.EventHandler(this.removeNodeButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(31, 6);
            // 
            // moveUpButton
            // 
            this.moveUpButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.moveUpButton.Image = global::Cornerstone.Properties.Resources.arrow_up;
            this.moveUpButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.moveUpButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.moveUpButton.Name = "moveUpButton";
            this.moveUpButton.Size = new System.Drawing.Size(31, 20);
            this.moveUpButton.Text = "Move Category Up";
            this.moveUpButton.Click += new System.EventHandler(this.moveUpButton_Click);
            // 
            // moveDownButton
            // 
            this.moveDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.moveDownButton.Image = global::Cornerstone.Properties.Resources.arrow_down;
            this.moveDownButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.moveDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.moveDownButton.Name = "moveDownButton";
            this.moveDownButton.Size = new System.Drawing.Size(31, 20);
            this.moveDownButton.Text = "Move Category Down";
            this.moveDownButton.ToolTipText = "Move Category Down";
            this.moveDownButton.Click += new System.EventHandler(this.moveDownButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(31, 6);
            // 
            // advancedButton
            // 
            this.advancedButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.advancedButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.convertToRegularMenuItemButton});
            this.advancedButton.Image = global::Cornerstone.Properties.Resources.advanced;
            this.advancedButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.advancedButton.Name = "advancedButton";
            this.advancedButton.Size = new System.Drawing.Size(31, 20);
            this.advancedButton.Text = "toolStripButton1";
            this.advancedButton.ToolTipText = "Advanced Options";
            this.advancedButton.ButtonClick += new System.EventHandler(this.advancedButton_ButtonClick);
            // 
            // convertToRegularMenuItemButton
            // 
            this.convertToRegularMenuItemButton.Name = "convertToRegularMenuItemButton";
            this.convertToRegularMenuItemButton.Size = new System.Drawing.Size(234, 22);
            this.convertToRegularMenuItemButton.Text = "Convert to Regular Menu Item";
            this.convertToRegularMenuItemButton.Click += new System.EventHandler(this.convertToRegularMenuItemToolStripMenuItem_Click);
            // 
            // GenericMenuTreePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.treeView);
            this.Controls.Add(this.toolStrip1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "GenericMenuTreePanel";
            this.Size = new System.Drawing.Size(223, 340);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.ToolStripButton removeNodeButton;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton addNodeButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSplitButton advancedButton;
        private System.Windows.Forms.ToolStripMenuItem convertToRegularMenuItemButton;
        private System.Windows.Forms.ToolStripButton moveDownButton;
        private System.Windows.Forms.ToolStripButton moveUpButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}
