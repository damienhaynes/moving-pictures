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
            this.panel2 = new System.Windows.Forms.Panel();
            this.unapprovedGrid = new System.Windows.Forms.DataGridView();
            this.statusColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.unapprovedLocalMediaColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unapprovedPossibleMatchesColumn = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.PotentialMatchColumn();
            this.progressPanel = new System.Windows.Forms.Panel();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.currentTaskDesc = new System.Windows.Forms.Label();
            this.countProgressLabel = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.filterSplitButton = new System.Windows.Forms.ToolStripSplitButton();
            this.allMatchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.processingMatchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unapprovedMatchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.approvedCommitedMatchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.approveButton = new System.Windows.Forms.ToolStripButton();
            this.manualAssignButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.rescanButton = new System.Windows.Forms.ToolStripButton();
            this.splitJoinButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ignoreButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsButton = new System.Windows.Forms.ToolStripSplitButton();
            this.ignoredFileManagerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restartImporterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.automaticMediaInfoMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpButton = new System.Windows.Forms.ToolStripButton();
            this.dataGridViewComboBoxColumn1 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unapprovedMatchesBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.displayDataProviderTagsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.importerGroupBox.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.unapprovedGrid)).BeginInit();
            this.progressPanel.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.unapprovedMatchesBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // importerGroupBox
            // 
            this.importerGroupBox.Controls.Add(this.panel2);
            this.importerGroupBox.Controls.Add(this.toolStrip1);
            this.importerGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.importerGroupBox.Location = new System.Drawing.Point(0, 0);
            this.importerGroupBox.Name = "importerGroupBox";
            this.importerGroupBox.Size = new System.Drawing.Size(642, 427);
            this.importerGroupBox.TabIndex = 0;
            this.importerGroupBox.TabStop = false;
            this.importerGroupBox.Text = "Media Importer";
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.unapprovedGrid);
            this.panel2.Controls.Add(this.progressPanel);
            this.panel2.Location = new System.Drawing.Point(3, 45);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(636, 379);
            this.panel2.TabIndex = 7;
            // 
            // unapprovedGrid
            // 
            this.unapprovedGrid.AllowUserToAddRows = false;
            this.unapprovedGrid.AllowUserToDeleteRows = false;
            this.unapprovedGrid.AllowUserToResizeRows = false;
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
            this.unapprovedGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.unapprovedGrid.Location = new System.Drawing.Point(0, 0);
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
            this.unapprovedGrid.Size = new System.Drawing.Size(636, 345);
            this.unapprovedGrid.TabIndex = 4;
            this.unapprovedGrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.unapprovedGrid_DataError_1);
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
            this.unapprovedPossibleMatchesColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // progressPanel
            // 
            this.progressPanel.Controls.Add(this.progressBar);
            this.progressPanel.Controls.Add(this.currentTaskDesc);
            this.progressPanel.Controls.Add(this.countProgressLabel);
            this.progressPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressPanel.Location = new System.Drawing.Point(0, 345);
            this.progressPanel.Name = "progressPanel";
            this.progressPanel.Size = new System.Drawing.Size(636, 34);
            this.progressPanel.TabIndex = 6;
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(7, 21);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(622, 10);
            this.progressBar.TabIndex = 0;
            // 
            // currentTaskDesc
            // 
            this.currentTaskDesc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.currentTaskDesc.AutoSize = true;
            this.currentTaskDesc.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.currentTaskDesc.Location = new System.Drawing.Point(4, 5);
            this.currentTaskDesc.Name = "currentTaskDesc";
            this.currentTaskDesc.Size = new System.Drawing.Size(115, 13);
            this.currentTaskDesc.TabIndex = 1;
            this.currentTaskDesc.Text = "Currently Processing ...";
            this.currentTaskDesc.Visible = false;
            // 
            // countProgressLabel
            // 
            this.countProgressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.countProgressLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.countProgressLabel.Location = new System.Drawing.Point(536, 6);
            this.countProgressLabel.Name = "countProgressLabel";
            this.countProgressLabel.Size = new System.Drawing.Size(93, 13);
            this.countProgressLabel.TabIndex = 3;
            this.countProgressLabel.Text = "(0/99)";
            this.countProgressLabel.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.countProgressLabel.Visible = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filterSplitButton,
            this.approveButton,
            this.manualAssignButton,
            this.toolStripSeparator3,
            this.rescanButton,
            this.splitJoinButton,
            this.toolStripSeparator1,
            this.ignoreButton,
            this.toolStripSeparator2,
            this.settingsButton,
            this.helpButton});
            this.toolStrip1.Location = new System.Drawing.Point(3, 16);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.toolStrip1.Size = new System.Drawing.Size(636, 26);
            this.toolStrip1.TabIndex = 5;
            this.toolStrip1.Text = "toolStrip1";
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
            this.filterSplitButton.Size = new System.Drawing.Size(118, 23);
            this.filterSplitButton.Text = "All Matches";
            this.filterSplitButton.ToolTipText = "Filter Match List";
            this.filterSplitButton.Visible = false;
            // 
            // allMatchesToolStripMenuItem
            // 
            this.allMatchesToolStripMenuItem.Name = "allMatchesToolStripMenuItem";
            this.allMatchesToolStripMenuItem.Size = new System.Drawing.Size(319, 24);
            this.allMatchesToolStripMenuItem.Text = "All Matches";
            // 
            // processingMatchesToolStripMenuItem
            // 
            this.processingMatchesToolStripMenuItem.Name = "processingMatchesToolStripMenuItem";
            this.processingMatchesToolStripMenuItem.Size = new System.Drawing.Size(319, 24);
            this.processingMatchesToolStripMenuItem.Text = "Processing Matches";
            // 
            // unapprovedMatchesToolStripMenuItem
            // 
            this.unapprovedMatchesToolStripMenuItem.Name = "unapprovedMatchesToolStripMenuItem";
            this.unapprovedMatchesToolStripMenuItem.Size = new System.Drawing.Size(319, 24);
            this.unapprovedMatchesToolStripMenuItem.Text = "Unapproved Matches";
            // 
            // approvedCommitedMatchesToolStripMenuItem
            // 
            this.approvedCommitedMatchesToolStripMenuItem.Name = "approvedCommitedMatchesToolStripMenuItem";
            this.approvedCommitedMatchesToolStripMenuItem.Size = new System.Drawing.Size(319, 24);
            this.approvedCommitedMatchesToolStripMenuItem.Text = "Approved/Commited Matches";
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
            // manualAssignButton
            // 
            this.manualAssignButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.manualAssignButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.page_white_edit;
            this.manualAssignButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.manualAssignButton.Name = "manualAssignButton";
            this.manualAssignButton.Size = new System.Drawing.Size(23, 23);
            this.manualAssignButton.Text = "manualAssignButton";
            this.manualAssignButton.ToolTipText = "Add as Blank (Editable) Movie";
            this.manualAssignButton.Click += new System.EventHandler(this.manualAssignButton_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 26);
            // 
            // rescanButton
            // 
            this.rescanButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.rescanButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.find;
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
            // settingsButton
            // 
            this.settingsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.settingsButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ignoredFileManagerToolStripMenuItem,
            this.restartImporterToolStripMenuItem,
            this.toolStripSeparator4,
            this.automaticMediaInfoMenuItem,
            this.displayDataProviderTagsMenuItem});
            this.settingsButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.cog;
            this.settingsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(32, 23);
            this.settingsButton.ToolTipText = "Advanced Actions";
            this.settingsButton.ButtonClick += new System.EventHandler(this.settingsButton_ButtonClick);
            // 
            // ignoredFileManagerToolStripMenuItem
            // 
            this.ignoredFileManagerToolStripMenuItem.Name = "ignoredFileManagerToolStripMenuItem";
            this.ignoredFileManagerToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.ignoredFileManagerToolStripMenuItem.Text = "Manage Ignored Files";
            this.ignoredFileManagerToolStripMenuItem.Click += new System.EventHandler(this.ignoredFileManagerToolStripMenuItem_Click);
            // 
            // restartImporterToolStripMenuItem
            // 
            this.restartImporterToolStripMenuItem.Name = "restartImporterToolStripMenuItem";
            this.restartImporterToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.restartImporterToolStripMenuItem.Text = "Restart Importer";
            this.restartImporterToolStripMenuItem.Click += new System.EventHandler(this.restartImporterToolStripMenuItem_Click);
            // 
            // automaticMediaInfoMenuItem
            // 
            this.automaticMediaInfoMenuItem.Name = "automaticMediaInfoMenuItem";
            this.automaticMediaInfoMenuItem.Size = new System.Drawing.Size(254, 22);
            this.automaticMediaInfoMenuItem.Text = "Automatically Retrieve MediaInfo";
            this.automaticMediaInfoMenuItem.Click += new System.EventHandler(this.automaticMediaInfoMenuItem_Click);
            // 
            // helpButton
            // 
            this.helpButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.helpButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.helpButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.help;
            this.helpButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(23, 23);
            this.helpButton.ToolTipText = "Help";
            this.helpButton.Click += new System.EventHandler(this.helpButton_Click);
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
            // unapprovedMatchesBindingSource
            // 
            this.unapprovedMatchesBindingSource.DataSource = typeof(MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieMatch);
            this.unapprovedMatchesBindingSource.ListChanged += new System.ComponentModel.ListChangedEventHandler(this.unapprovedMatchesBindingSource_ListChanged);
            // 
            // alwaysDisplayDataProviderTagsToolStripMenuItem
            // 
            this.displayDataProviderTagsMenuItem.Name = "alwaysDisplayDataProviderTagsToolStripMenuItem";
            this.displayDataProviderTagsMenuItem.Size = new System.Drawing.Size(254, 22);
            this.displayDataProviderTagsMenuItem.Text = "Always Display Data Provider Tags";
            this.displayDataProviderTagsMenuItem.Click += new System.EventHandler(this.alwaysDisplayDataProviderTagsToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(251, 6);
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
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.unapprovedGrid)).EndInit();
            this.progressPanel.ResumeLayout(false);
            this.progressPanel.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
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
        private Panel progressPanel;
        private ToolStrip toolStrip1;
        private ToolStripSplitButton filterSplitButton;
        private ToolStripMenuItem allMatchesToolStripMenuItem;
        private ToolStripMenuItem processingMatchesToolStripMenuItem;
        private ToolStripMenuItem unapprovedMatchesToolStripMenuItem;
        private ToolStripMenuItem approvedCommitedMatchesToolStripMenuItem;
        private ToolStripButton approveButton;
        private ToolStripButton manualAssignButton;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripButton rescanButton;
        private ToolStripButton splitJoinButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton ignoreButton;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSplitButton settingsButton;
        private ToolStripMenuItem ignoredFileManagerToolStripMenuItem;
        private ToolStripMenuItem restartImporterToolStripMenuItem;
        private DataGridView unapprovedGrid;
        private Panel panel2;
        private ToolStripButton helpButton;
        private ToolStripMenuItem automaticMediaInfoMenuItem;
        private DataGridViewImageColumn statusColumn;
        private DataGridViewTextBoxColumn unapprovedLocalMediaColumn;
        private Controls.PotentialMatchColumn unapprovedPossibleMatchesColumn;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem displayDataProviderTagsMenuItem;
    }
}
