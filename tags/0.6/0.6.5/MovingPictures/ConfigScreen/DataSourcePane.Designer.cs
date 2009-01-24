namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    partial class DataSourcePane {
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
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataSourcePane));
            this.listView = new System.Windows.Forms.ListView();
            this.sourceColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.versionColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.languageColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.topToolStrip = new System.Windows.Forms.ToolStrip();
            this.scriptTypeDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            this.movieDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.coversToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backdropsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.addButton = new System.Windows.Forms.ToolStripButton();
            this.removeDropDown = new System.Windows.Forms.ToolStripSplitButton();
            this.disableSelectedDataSourceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteSelectedDataSourceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.raisePriorityButton = new System.Windows.Forms.ToolStripButton();
            this.lowerPriorityButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsButton = new System.Windows.Forms.ToolStripSplitButton();
            this.selectScriptVersionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reloadDefaultSourcesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.enableDebugModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugIcon = new System.Windows.Forms.ToolStripLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.publishedHeader = new System.Windows.Forms.ColumnHeader();
            this.topToolStrip.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView
            // 
            this.listView.AllowDrop = true;
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.sourceColumnHeader,
            this.versionColumnHeader,
            this.languageColumnHeader,
            this.publishedHeader});
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.FullRowSelect = true;
            this.listView.GridLines = true;
            this.listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView.HideSelection = false;
            this.listView.Location = new System.Drawing.Point(0, 26);
            this.listView.MultiSelect = false;
            this.listView.Name = "listView";
            this.listView.ShowItemToolTips = true;
            this.listView.Size = new System.Drawing.Size(510, 273);
            this.listView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listView.TabIndex = 5;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            // 
            // sourceColumnHeader
            // 
            this.sourceColumnHeader.Text = "Source";
            this.sourceColumnHeader.Width = 109;
            // 
            // versionColumnHeader
            // 
            this.versionColumnHeader.Text = "Version";
            this.versionColumnHeader.Width = 49;
            // 
            // languageColumnHeader
            // 
            this.languageColumnHeader.Text = "Language";
            this.languageColumnHeader.Width = 134;
            // 
            // topToolStrip
            // 
            this.topToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.topToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.scriptTypeDropDown,
            this.toolStripSeparator4,
            this.addButton,
            this.removeDropDown,
            this.toolStripSeparator1,
            this.raisePriorityButton,
            this.lowerPriorityButton,
            this.toolStripSeparator2,
            this.settingsButton,
            this.debugIcon});
            this.topToolStrip.Location = new System.Drawing.Point(0, 0);
            this.topToolStrip.Name = "topToolStrip";
            this.topToolStrip.Size = new System.Drawing.Size(510, 26);
            this.topToolStrip.TabIndex = 3;
            this.topToolStrip.Text = "toolStrip";
            // 
            // scriptTypeDropDown
            // 
            this.scriptTypeDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.scriptTypeDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.movieDetailsToolStripMenuItem,
            this.coversToolStripMenuItem,
            this.backdropsToolStripMenuItem});
            this.scriptTypeDropDown.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.scriptTypeDropDown.Image = ((System.Drawing.Image)(resources.GetObject("scriptTypeDropDown.Image")));
            this.scriptTypeDropDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.scriptTypeDropDown.Name = "scriptTypeDropDown";
            this.scriptTypeDropDown.Size = new System.Drawing.Size(132, 23);
            this.scriptTypeDropDown.Text = "Movie Details";
            // 
            // movieDetailsToolStripMenuItem
            // 
            this.movieDetailsToolStripMenuItem.Name = "movieDetailsToolStripMenuItem";
            this.movieDetailsToolStripMenuItem.Size = new System.Drawing.Size(204, 24);
            this.movieDetailsToolStripMenuItem.Text = "Movie Details";
            this.movieDetailsToolStripMenuItem.Click += new System.EventHandler(this.movieDetailsToolStripMenuItem_Click);
            // 
            // coversToolStripMenuItem
            // 
            this.coversToolStripMenuItem.Name = "coversToolStripMenuItem";
            this.coversToolStripMenuItem.Size = new System.Drawing.Size(204, 24);
            this.coversToolStripMenuItem.Text = "Covers";
            this.coversToolStripMenuItem.Click += new System.EventHandler(this.coversToolStripMenuItem_Click);
            // 
            // backdropsToolStripMenuItem
            // 
            this.backdropsToolStripMenuItem.Name = "backdropsToolStripMenuItem";
            this.backdropsToolStripMenuItem.Size = new System.Drawing.Size(204, 24);
            this.backdropsToolStripMenuItem.Text = "Backdrops";
            this.backdropsToolStripMenuItem.Click += new System.EventHandler(this.backdropsToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 26);
            // 
            // addButton
            // 
            this.addButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.addButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.list_add;
            this.addButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(23, 23);
            this.addButton.Text = "Add New Script";
            this.addButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // removeDropDown
            // 
            this.removeDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.removeDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.disableSelectedDataSourceToolStripMenuItem,
            this.deleteSelectedDataSourceToolStripMenuItem});
            this.removeDropDown.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.list_remove;
            this.removeDropDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.removeDropDown.Name = "removeDropDown";
            this.removeDropDown.Size = new System.Drawing.Size(32, 23);
            this.removeDropDown.Text = "Disable Selected Data Source";
            this.removeDropDown.ButtonClick += new System.EventHandler(this.disableButton_Click);
            // 
            // disableSelectedDataSourceToolStripMenuItem
            // 
            this.disableSelectedDataSourceToolStripMenuItem.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.list_remove;
            this.disableSelectedDataSourceToolStripMenuItem.Name = "disableSelectedDataSourceToolStripMenuItem";
            this.disableSelectedDataSourceToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.disableSelectedDataSourceToolStripMenuItem.Text = "Disable Selected Data Source";
            this.disableSelectedDataSourceToolStripMenuItem.Click += new System.EventHandler(this.disableButton_Click);
            // 
            // deleteSelectedDataSourceToolStripMenuItem
            // 
            this.deleteSelectedDataSourceToolStripMenuItem.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.cross;
            this.deleteSelectedDataSourceToolStripMenuItem.Name = "deleteSelectedDataSourceToolStripMenuItem";
            this.deleteSelectedDataSourceToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.deleteSelectedDataSourceToolStripMenuItem.Text = "Delete Selected Data Source";
            this.deleteSelectedDataSourceToolStripMenuItem.Click += new System.EventHandler(this.removeButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 26);
            // 
            // raisePriorityButton
            // 
            this.raisePriorityButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.raisePriorityButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.arrow_up;
            this.raisePriorityButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.raisePriorityButton.Name = "raisePriorityButton";
            this.raisePriorityButton.Size = new System.Drawing.Size(23, 23);
            this.raisePriorityButton.Text = "toolStripButton1";
            this.raisePriorityButton.ToolTipText = "Raise Priority of Selected Script";
            this.raisePriorityButton.Click += new System.EventHandler(this.raisePriorityButton_Click);
            // 
            // lowerPriorityButton
            // 
            this.lowerPriorityButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.lowerPriorityButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.arrow_down;
            this.lowerPriorityButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.lowerPriorityButton.Name = "lowerPriorityButton";
            this.lowerPriorityButton.Size = new System.Drawing.Size(23, 23);
            this.lowerPriorityButton.Text = "toolStripButton2";
            this.lowerPriorityButton.ToolTipText = "Lower Priority of Selected Script";
            this.lowerPriorityButton.Click += new System.EventHandler(this.lowerPriorityButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 26);
            // 
            // settingsButton
            // 
            this.settingsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.settingsButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectScriptVersionToolStripMenuItem,
            this.reloadDefaultSourcesToolStripMenuItem,
            this.toolStripSeparator3,
            this.enableDebugModeToolStripMenuItem});
            this.settingsButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.cog;
            this.settingsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(32, 23);
            this.settingsButton.ToolTipText = "Advanced Actions";
            this.settingsButton.ButtonClick += new System.EventHandler(this.settingsButton_ButtonClick);
            // 
            // selectScriptVersionToolStripMenuItem
            // 
            this.selectScriptVersionToolStripMenuItem.Name = "selectScriptVersionToolStripMenuItem";
            this.selectScriptVersionToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.selectScriptVersionToolStripMenuItem.Text = "Select Version of Source";
            this.selectScriptVersionToolStripMenuItem.Click += new System.EventHandler(this.selectScriptVersionToolStripMenuItem_Click);
            // 
            // reloadDefaultSourcesToolStripMenuItem
            // 
            this.reloadDefaultSourcesToolStripMenuItem.Name = "reloadDefaultSourcesToolStripMenuItem";
            this.reloadDefaultSourcesToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.reloadDefaultSourcesToolStripMenuItem.Text = "Reload Default Sources";
            this.reloadDefaultSourcesToolStripMenuItem.Click += new System.EventHandler(this.reloadDefaultSourcesToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(198, 6);
            // 
            // enableDebugModeToolStripMenuItem
            // 
            this.enableDebugModeToolStripMenuItem.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.bug;
            this.enableDebugModeToolStripMenuItem.Name = "enableDebugModeToolStripMenuItem";
            this.enableDebugModeToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.enableDebugModeToolStripMenuItem.Text = "Enable Debug Mode";
            this.enableDebugModeToolStripMenuItem.Click += new System.EventHandler(this.toggleDebugModeToolStripMenuItem_Click);
            // 
            // debugIcon
            // 
            this.debugIcon.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.debugIcon.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.bug;
            this.debugIcon.Margin = new System.Windows.Forms.Padding(0, 1, 5, 2);
            this.debugIcon.Name = "debugIcon";
            this.debugIcon.Size = new System.Drawing.Size(16, 23);
            this.debugIcon.ToolTipText = "Advanced Data Provider Debugging Active";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.listView);
            this.panel1.Controls.Add(this.topToolStrip);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(510, 299);
            this.panel1.TabIndex = 6;
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "xml";
            this.openFileDialog.Filter = "Moving Pictures Script Files|*.xml|All Files|*.*";
            // 
            // publishedHeader
            // 
            this.publishedHeader.Text = "Published";
            this.publishedHeader.Width = 82;
            // 
            // DataSourcePane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "DataSourcePane";
            this.Size = new System.Drawing.Size(510, 299);
            this.Load += new System.EventHandler(this.DataSourcePane_Load);
            this.topToolStrip.ResumeLayout(false);
            this.topToolStrip.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader sourceColumnHeader;
        private System.Windows.Forms.ColumnHeader versionColumnHeader;
        private System.Windows.Forms.ColumnHeader languageColumnHeader;
        private System.Windows.Forms.ToolStrip topToolStrip;
        private System.Windows.Forms.ToolStripButton addButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSplitButton settingsButton;
        private System.Windows.Forms.ToolStripMenuItem selectScriptVersionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableDebugModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reloadDefaultSourcesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripDropDownButton scriptTypeDropDown;
        private System.Windows.Forms.ToolStripMenuItem movieDetailsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem coversToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backdropsToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripLabel debugIcon;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ToolStripSplitButton removeDropDown;
        private System.Windows.Forms.ToolStripMenuItem disableSelectedDataSourceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteSelectedDataSourceToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton raisePriorityButton;
        private System.Windows.Forms.ToolStripButton lowerPriorityButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ColumnHeader publishedHeader;
    }
}
