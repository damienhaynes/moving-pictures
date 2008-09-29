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
            this.iconColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.sourceColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.versionColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.languageColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.sideToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.topToolStrip = new System.Windows.Forms.ToolStrip();
            this.addButton = new System.Windows.Forms.ToolStripButton();
            this.removeButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
            this.selectScriptVersionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reloadDefaultSourcesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.enableDebugModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.movieDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.coversToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backdropsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.sideToolStrip.SuspendLayout();
            this.topToolStrip.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView
            // 
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.iconColumnHeader,
            this.sourceColumnHeader,
            this.versionColumnHeader,
            this.languageColumnHeader});
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.FullRowSelect = true;
            this.listView.GridLines = true;
            this.listView.Location = new System.Drawing.Point(0, 26);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(486, 273);
            this.listView.TabIndex = 5;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            // 
            // iconColumnHeader
            // 
            this.iconColumnHeader.Text = "";
            this.iconColumnHeader.Width = 21;
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
            // sideToolStrip
            // 
            this.sideToolStrip.Dock = System.Windows.Forms.DockStyle.Right;
            this.sideToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.sideToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripButton2,
            this.toolStripSeparator2,
            this.toolStripButton3});
            this.sideToolStrip.Location = new System.Drawing.Point(486, 0);
            this.sideToolStrip.Name = "sideToolStrip";
            this.sideToolStrip.Padding = new System.Windows.Forms.Padding(0, 45, 1, 0);
            this.sideToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.sideToolStrip.Size = new System.Drawing.Size(24, 299);
            this.sideToolStrip.TabIndex = 4;
            this.sideToolStrip.Text = "toolStrip2";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.arrow_up;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(21, 20);
            this.toolStripButton1.Text = "toolStripButton1";
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.arrow_down;
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(21, 20);
            this.toolStripButton2.Text = "toolStripButton2";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(21, 6);
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.ignored;
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(21, 20);
            this.toolStripButton3.Text = "toolStripButton3";
            // 
            // topToolStrip
            // 
            this.topToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.topToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1,
            this.toolStripSeparator4,
            this.addButton,
            this.removeButton,
            this.toolStripSeparator1,
            this.toolStripSplitButton1});
            this.topToolStrip.Location = new System.Drawing.Point(0, 0);
            this.topToolStrip.Name = "topToolStrip";
            this.topToolStrip.Size = new System.Drawing.Size(486, 26);
            this.topToolStrip.TabIndex = 3;
            this.topToolStrip.Text = "toolStrip";
            // 
            // addButton
            // 
            this.addButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.addButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.list_add;
            this.addButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(23, 23);
            this.addButton.Text = "toolStripButton1";
            // 
            // removeButton
            // 
            this.removeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.removeButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.list_remove;
            this.removeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(23, 23);
            this.removeButton.Text = "removeButton";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 26);
            // 
            // toolStripSplitButton1
            // 
            this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectScriptVersionToolStripMenuItem,
            this.reloadDefaultSourcesToolStripMenuItem,
            this.toolStripSeparator3,
            this.enableDebugModeToolStripMenuItem});
            this.toolStripSplitButton1.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.cog;
            this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            this.toolStripSplitButton1.Size = new System.Drawing.Size(32, 23);
            this.toolStripSplitButton1.Text = "toolStripSplitButton1";
            // 
            // selectScriptVersionToolStripMenuItem
            // 
            this.selectScriptVersionToolStripMenuItem.Name = "selectScriptVersionToolStripMenuItem";
            this.selectScriptVersionToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.selectScriptVersionToolStripMenuItem.Text = "Select Version of Selected Source";
            // 
            // reloadDefaultSourcesToolStripMenuItem
            // 
            this.reloadDefaultSourcesToolStripMenuItem.Name = "reloadDefaultSourcesToolStripMenuItem";
            this.reloadDefaultSourcesToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.reloadDefaultSourcesToolStripMenuItem.Text = "Reload Default Sources";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(242, 6);
            // 
            // enableDebugModeToolStripMenuItem
            // 
            this.enableDebugModeToolStripMenuItem.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.bug;
            this.enableDebugModeToolStripMenuItem.Name = "enableDebugModeToolStripMenuItem";
            this.enableDebugModeToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.enableDebugModeToolStripMenuItem.Text = "Enable Debug Mode";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.movieDetailsToolStripMenuItem,
            this.coversToolStripMenuItem,
            this.backdropsToolStripMenuItem});
            this.toolStripDropDownButton1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(132, 23);
            this.toolStripDropDownButton1.Text = "Movie Details";
            // 
            // movieDetailsToolStripMenuItem
            // 
            this.movieDetailsToolStripMenuItem.Name = "movieDetailsToolStripMenuItem";
            this.movieDetailsToolStripMenuItem.Size = new System.Drawing.Size(204, 24);
            this.movieDetailsToolStripMenuItem.Text = "Movie Details";
            // 
            // coversToolStripMenuItem
            // 
            this.coversToolStripMenuItem.Name = "coversToolStripMenuItem";
            this.coversToolStripMenuItem.Size = new System.Drawing.Size(204, 24);
            this.coversToolStripMenuItem.Text = "Covers";
            // 
            // backdropsToolStripMenuItem
            // 
            this.backdropsToolStripMenuItem.Name = "backdropsToolStripMenuItem";
            this.backdropsToolStripMenuItem.Size = new System.Drawing.Size(204, 24);
            this.backdropsToolStripMenuItem.Text = "Backdrops";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.listView);
            this.panel1.Controls.Add(this.topToolStrip);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(486, 299);
            this.panel1.TabIndex = 6;
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 26);
            // 
            // DataSourcePane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.sideToolStrip);
            this.Name = "DataSourcePane";
            this.Size = new System.Drawing.Size(510, 299);
            this.Load += new System.EventHandler(this.DataSourcePane_Load);
            this.sideToolStrip.ResumeLayout(false);
            this.sideToolStrip.PerformLayout();
            this.topToolStrip.ResumeLayout(false);
            this.topToolStrip.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader iconColumnHeader;
        private System.Windows.Forms.ColumnHeader sourceColumnHeader;
        private System.Windows.Forms.ColumnHeader versionColumnHeader;
        private System.Windows.Forms.ColumnHeader languageColumnHeader;
        private System.Windows.Forms.ToolStrip sideToolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStrip topToolStrip;
        private System.Windows.Forms.ToolStripButton addButton;
        private System.Windows.Forms.ToolStripButton removeButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton1;
        private System.Windows.Forms.ToolStripMenuItem selectScriptVersionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableDebugModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reloadDefaultSourcesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem movieDetailsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem coversToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backdropsToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
    }
}
