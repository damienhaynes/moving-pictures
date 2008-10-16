using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Plugins.MovingPictures.Database;
namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    partial class MovieImporterPane {
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.importerGroupBox = new System.Windows.Forms.GroupBox();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.matchesTabPage = new System.Windows.Forms.TabPage();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.approveButton = new System.Windows.Forms.ToolStripButton();
            this.rescanButton = new System.Windows.Forms.ToolStripButton();
            this.splitJoinButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ignoreButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.filterSplitButton = new System.Windows.Forms.ToolStripSplitButton();
            this.allMatchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.processingMatchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unapprovedMatchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.approvedCommitedMatchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unapprovedGrid = new System.Windows.Forms.DataGridView();
            this.statusColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.unapprovedLocalMediaColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unapprovedPossibleMatchesColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.sourcesTabPage = new System.Windows.Forms.TabPage();
            this.settingsTabPage = new System.Windows.Forms.TabPage();
            this.comingSoon1 = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.countProgressLabel = new System.Windows.Forms.Label();
            this.currentTaskDesc = new System.Windows.Forms.Label();
            this.dataGridViewComboBoxColumn1 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.settingsButton = new System.Windows.Forms.ToolStripSplitButton();
            this.unignoreAllFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restartImporterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataSourceList1 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.DataSourcePane();
            this.unapprovedMatchesBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.importerGroupBox.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.matchesTabPage.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.unapprovedGrid)).BeginInit();
            this.sourcesTabPage.SuspendLayout();
            this.settingsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.unapprovedMatchesBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // importerGroupBox
            // 
            this.importerGroupBox.Controls.Add(this.tabControl);
            this.importerGroupBox.Controls.Add(this.progressBar);
            this.importerGroupBox.Controls.Add(this.countProgressLabel);
            this.importerGroupBox.Controls.Add(this.currentTaskDesc);
            this.importerGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.importerGroupBox.Location = new System.Drawing.Point(0, 0);
            this.importerGroupBox.Name = "importerGroupBox";
            this.importerGroupBox.Size = new System.Drawing.Size(642, 427);
            this.importerGroupBox.TabIndex = 0;
            this.importerGroupBox.TabStop = false;
            this.importerGroupBox.Text = "Media Importer";
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl.Controls.Add(this.matchesTabPage);
            this.tabControl.Controls.Add(this.sourcesTabPage);
            this.tabControl.Controls.Add(this.settingsTabPage);
            this.tabControl.Location = new System.Drawing.Point(6, 20);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(630, 355);
            this.tabControl.TabIndex = 4;
            // 
            // matchesTabPage
            // 
            this.matchesTabPage.Controls.Add(this.toolStrip1);
            this.matchesTabPage.Controls.Add(this.unapprovedGrid);
            this.matchesTabPage.Location = new System.Drawing.Point(4, 25);
            this.matchesTabPage.Name = "matchesTabPage";
            this.matchesTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.matchesTabPage.Size = new System.Drawing.Size(622, 326);
            this.matchesTabPage.TabIndex = 0;
            this.matchesTabPage.Text = "Matches";
            this.matchesTabPage.UseVisualStyleBackColor = true;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filterSplitButton,
            this.approveButton,
            this.rescanButton,
            this.splitJoinButton,
            this.toolStripSeparator1,
            this.ignoreButton,
            this.toolStripSeparator2,
            this.settingsButton});
            this.toolStrip1.Location = new System.Drawing.Point(3, 3);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.toolStrip1.Size = new System.Drawing.Size(616, 26);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // approveButton
            // 
            this.approveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.approveButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.tick;
            this.approveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.approveButton.Name = "approveButton";
            this.approveButton.Size = new System.Drawing.Size(23, 23);
            this.approveButton.Text = "toolStripButton1";
            this.approveButton.ToolTipText = "Approve Selected File(s)";
            this.approveButton.Click += new System.EventHandler(this.approveButton_Click);
            // 
            // rescanButton
            // 
            this.rescanButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.rescanButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.arrow_rotate_clockwise;
            this.rescanButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.rescanButton.Name = "rescanButton";
            this.rescanButton.Size = new System.Drawing.Size(23, 23);
            this.rescanButton.Text = "toolStripButton1";
            this.rescanButton.ToolTipText = "Rescan Selected File(s) with Custom Search String";
            this.rescanButton.Click += new System.EventHandler(this.rescanButton_Click);
            // 
            // splitJoinButton
            // 
            this.splitJoinButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.splitJoinButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.arrow_divide;
            this.splitJoinButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.splitJoinButton.Name = "splitJoinButton";
            this.splitJoinButton.Size = new System.Drawing.Size(23, 23);
            this.splitJoinButton.Text = "toolStripButton1";
            this.splitJoinButton.ToolTipText = "Split Selected File Group";
            this.splitJoinButton.Click += new System.EventHandler(this.splitJoinButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 26);
            // 
            // ignoreButton
            // 
            this.ignoreButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ignoreButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.cross;
            this.ignoreButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ignoreButton.Name = "ignoreButton";
            this.ignoreButton.Size = new System.Drawing.Size(23, 23);
            this.ignoreButton.Text = "toolStripButton2";
            this.ignoreButton.ToolTipText = "Ignore Selected File(s)";
            this.ignoreButton.Click += new System.EventHandler(this.ignoreButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 26);
            // 
            // filterSplitButton
            // 
            this.filterSplitButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.filterSplitButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allMatchesToolStripMenuItem,
            this.processingMatchesToolStripMenuItem,
            this.unapprovedMatchesToolStripMenuItem,
            this.approvedCommitedMatchesToolStripMenuItem});
            this.filterSplitButton.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.filterSplitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.filterSplitButton.Name = "filterSplitButton";
            this.filterSplitButton.Size = new System.Drawing.Size(119, 23);
            this.filterSplitButton.Text = "All Matches";
            this.filterSplitButton.ToolTipText = "Filter Match List";
            this.filterSplitButton.Visible = false;
            // 
            // allMatchesToolStripMenuItem
            // 
            this.allMatchesToolStripMenuItem.Name = "allMatchesToolStripMenuItem";
            this.allMatchesToolStripMenuItem.Size = new System.Drawing.Size(336, 24);
            this.allMatchesToolStripMenuItem.Text = "All Matches";
            // 
            // processingMatchesToolStripMenuItem
            // 
            this.processingMatchesToolStripMenuItem.Name = "processingMatchesToolStripMenuItem";
            this.processingMatchesToolStripMenuItem.Size = new System.Drawing.Size(336, 24);
            this.processingMatchesToolStripMenuItem.Text = "Processing Matches";
            // 
            // unapprovedMatchesToolStripMenuItem
            // 
            this.unapprovedMatchesToolStripMenuItem.Name = "unapprovedMatchesToolStripMenuItem";
            this.unapprovedMatchesToolStripMenuItem.Size = new System.Drawing.Size(336, 24);
            this.unapprovedMatchesToolStripMenuItem.Text = "Unapproved Matches";
            // 
            // approvedCommitedMatchesToolStripMenuItem
            // 
            this.approvedCommitedMatchesToolStripMenuItem.Name = "approvedCommitedMatchesToolStripMenuItem";
            this.approvedCommitedMatchesToolStripMenuItem.Size = new System.Drawing.Size(336, 24);
            this.approvedCommitedMatchesToolStripMenuItem.Text = "Approved/Commited Matches";
            // 
            // unapprovedGrid
            // 
            this.unapprovedGrid.AllowUserToAddRows = false;
            this.unapprovedGrid.AllowUserToDeleteRows = false;
            this.unapprovedGrid.AllowUserToResizeRows = false;
            this.unapprovedGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.unapprovedGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.unapprovedGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.unapprovedGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.statusColumn,
            this.unapprovedLocalMediaColumn,
            this.unapprovedPossibleMatchesColumn});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.unapprovedGrid.DefaultCellStyle = dataGridViewCellStyle2;
            this.unapprovedGrid.Location = new System.Drawing.Point(0, 31);
            this.unapprovedGrid.Name = "unapprovedGrid";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.unapprovedGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.unapprovedGrid.RowHeadersVisible = false;
            this.unapprovedGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.unapprovedGrid.Size = new System.Drawing.Size(621, 295);
            this.unapprovedGrid.TabIndex = 0;
            this.unapprovedGrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.unapprovedGrid_DataError);
            this.unapprovedGrid.SelectionChanged += new System.EventHandler(this.unapprovedGrid_SelectionChanged);
            // 
            // statusColumn
            // 
            this.statusColumn.HeaderText = "";
            this.statusColumn.Name = "statusColumn";
            this.statusColumn.Width = 20;
            // 
            // unapprovedLocalMediaColumn
            // 
            this.unapprovedLocalMediaColumn.DataPropertyName = "LocalMediaString";
            this.unapprovedLocalMediaColumn.HeaderText = "File(s)";
            this.unapprovedLocalMediaColumn.Name = "unapprovedLocalMediaColumn";
            this.unapprovedLocalMediaColumn.ReadOnly = true;
            this.unapprovedLocalMediaColumn.Width = 200;
            // 
            // unapprovedPossibleMatchesColumn
            // 
            this.unapprovedPossibleMatchesColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.unapprovedPossibleMatchesColumn.DataPropertyName = "Selected";
            this.unapprovedPossibleMatchesColumn.HeaderText = "Possible Matches";
            this.unapprovedPossibleMatchesColumn.Name = "unapprovedPossibleMatchesColumn";
            // 
            // sourcesTabPage
            // 
            this.sourcesTabPage.Controls.Add(this.dataSourceList1);
            this.sourcesTabPage.Location = new System.Drawing.Point(4, 25);
            this.sourcesTabPage.Name = "sourcesTabPage";
            this.sourcesTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.sourcesTabPage.Size = new System.Drawing.Size(622, 326);
            this.sourcesTabPage.TabIndex = 5;
            this.sourcesTabPage.Text = "Data Sources";
            this.sourcesTabPage.UseVisualStyleBackColor = true;
            // 
            // settingsTabPage
            // 
            this.settingsTabPage.Controls.Add(this.comingSoon1);
            this.settingsTabPage.Location = new System.Drawing.Point(4, 25);
            this.settingsTabPage.Name = "settingsTabPage";
            this.settingsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.settingsTabPage.Size = new System.Drawing.Size(622, 326);
            this.settingsTabPage.TabIndex = 3;
            this.settingsTabPage.Text = "Settings";
            this.settingsTabPage.UseVisualStyleBackColor = true;
            // 
            // comingSoon1
            // 
            this.comingSoon1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.comingSoon1.AutoSize = true;
            this.comingSoon1.Location = new System.Drawing.Point(273, 157);
            this.comingSoon1.Name = "comingSoon1";
            this.comingSoon1.Size = new System.Drawing.Size(77, 13);
            this.comingSoon1.TabIndex = 0;
            this.comingSoon1.Text = "Coming soon...";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(6, 394);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(626, 27);
            this.progressBar.TabIndex = 0;
            // 
            // countProgressLabel
            // 
            this.countProgressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.countProgressLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.countProgressLabel.Location = new System.Drawing.Point(539, 379);
            this.countProgressLabel.Name = "countProgressLabel";
            this.countProgressLabel.Size = new System.Drawing.Size(93, 13);
            this.countProgressLabel.TabIndex = 3;
            this.countProgressLabel.Text = "(0/99)";
            this.countProgressLabel.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.countProgressLabel.Visible = false;
            // 
            // currentTaskDesc
            // 
            this.currentTaskDesc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.currentTaskDesc.AutoSize = true;
            this.currentTaskDesc.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.currentTaskDesc.Location = new System.Drawing.Point(3, 378);
            this.currentTaskDesc.Name = "currentTaskDesc";
            this.currentTaskDesc.Size = new System.Drawing.Size(115, 13);
            this.currentTaskDesc.TabIndex = 1;
            this.currentTaskDesc.Text = "Currently Processing ...";
            this.currentTaskDesc.Visible = false;
            // 
            // dataGridViewComboBoxColumn1
            // 
            this.dataGridViewComboBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewComboBoxColumn1.DataPropertyName = "Selected";
            this.dataGridViewComboBoxColumn1.HeaderText = "Possible Matches";
            this.dataGridViewComboBoxColumn1.Name = "dataGridViewComboBoxColumn1";
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "LocalMediaString";
            this.dataGridViewTextBoxColumn1.HeaderText = "File(s)";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Width = 200;
            // 
            // settingsButton
            // 
            this.settingsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.settingsButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.unignoreAllFilesToolStripMenuItem,
            this.restartImporterToolStripMenuItem});
            this.settingsButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.cog;
            this.settingsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(32, 23);
            this.settingsButton.ToolTipText = "Advanced Actions";
            // 
            // unignoreAllFilesToolStripMenuItem
            // 
            this.unignoreAllFilesToolStripMenuItem.Name = "unignoreAllFilesToolStripMenuItem";
            this.unignoreAllFilesToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.unignoreAllFilesToolStripMenuItem.Text = "Unignore All Files";
            this.unignoreAllFilesToolStripMenuItem.Click += new System.EventHandler(this.unignoreAllFilesToolStripMenuItem_Click);
            // 
            // restartImporterToolStripMenuItem
            // 
            this.restartImporterToolStripMenuItem.Name = "restartImporterToolStripMenuItem";
            this.restartImporterToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.restartImporterToolStripMenuItem.Text = "Restart Importer";
            this.restartImporterToolStripMenuItem.Click += new System.EventHandler(this.restartImporterToolStripMenuItem_Click);
            // 
            // dataSourceList1
            // 
            this.dataSourceList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataSourceList1.Location = new System.Drawing.Point(3, 3);
            this.dataSourceList1.Name = "dataSourceList1";
            this.dataSourceList1.Size = new System.Drawing.Size(616, 320);
            this.dataSourceList1.TabIndex = 0;
            // 
            // unapprovedMatchesBindingSource
            // 
            this.unapprovedMatchesBindingSource.DataSource = typeof(MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieMatch);
            this.unapprovedMatchesBindingSource.ListChanged += new System.ComponentModel.ListChangedEventHandler(this.unapprovedMatchesBindingSource_ListChanged);
            // 
            // MovieImporterPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.importerGroupBox);
            this.MinimumSize = new System.Drawing.Size(422, 250);
            this.Name = "MovieImporterPane";
            this.Size = new System.Drawing.Size(642, 427);
            this.Load += new System.EventHandler(this.MovieImporterPane_Load);
            this.HandleDestroyed += new System.EventHandler(this.MovieImporterPane_HandleDestroyed);
            this.importerGroupBox.ResumeLayout(false);
            this.importerGroupBox.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.matchesTabPage.ResumeLayout(false);
            this.matchesTabPage.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.unapprovedGrid)).EndInit();
            this.sourcesTabPage.ResumeLayout(false);
            this.settingsTabPage.ResumeLayout(false);
            this.settingsTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.unapprovedMatchesBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox importerGroupBox;
        private BindingSource unapprovedMatchesBindingSource;
        private DataGridViewComboBoxColumn dataGridViewComboBoxColumn1;
        private Label countProgressLabel;
        private Label currentTaskDesc;
        private ProgressBar progressBar;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private TabControl tabControl;
        private TabPage matchesTabPage;
        private ToolStrip toolStrip1;
        private ToolStripButton approveButton;
        private ToolStripButton rescanButton;
        private ToolStripButton splitJoinButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton ignoreButton;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSplitButton filterSplitButton;
        private ToolStripMenuItem allMatchesToolStripMenuItem;
        private ToolStripMenuItem processingMatchesToolStripMenuItem;
        private ToolStripMenuItem unapprovedMatchesToolStripMenuItem;
        private ToolStripMenuItem approvedCommitedMatchesToolStripMenuItem;
        private DataGridView unapprovedGrid;
        private DataGridViewImageColumn statusColumn;
        private DataGridViewTextBoxColumn unapprovedLocalMediaColumn;
        private DataGridViewComboBoxColumn unapprovedPossibleMatchesColumn;
        private TabPage sourcesTabPage;
        private DataSourcePane dataSourceList1;
        private TabPage settingsTabPage;
        private Label comingSoon1;
        private ToolStripSplitButton settingsButton;
        private ToolStripMenuItem unignoreAllFilesToolStripMenuItem;
        private ToolStripMenuItem restartImporterToolStripMenuItem;
    }
}
