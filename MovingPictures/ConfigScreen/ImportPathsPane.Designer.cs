namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    partial class ImportPathsPane {
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
            this.pathsGridView = new System.Windows.Forms.DataGridView();
            this.pathColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pathsGroupBox = new System.Windows.Forms.GroupBox();
            this.importDvdCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.addSourceButton = new System.Windows.Forms.ToolStripButton();
            this.removeSourceButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.helpButton = new System.Windows.Forms.ToolStripButton();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.pathsGridView)).BeginInit();
            this.pathsGroupBox.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // pathsGridView
            // 
            this.pathsGridView.AllowUserToAddRows = false;
            this.pathsGridView.AllowUserToDeleteRows = false;
            this.pathsGridView.AllowUserToResizeRows = false;
            this.pathsGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pathsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.pathsGridView.ColumnHeadersVisible = false;
            this.pathsGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.pathColumn});
            this.pathsGridView.Location = new System.Drawing.Point(3, 39);
            this.pathsGridView.MultiSelect = false;
            this.pathsGridView.Name = "pathsGridView";
            this.pathsGridView.RowHeadersVisible = false;
            this.pathsGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.pathsGridView.Size = new System.Drawing.Size(497, 76);
            this.pathsGridView.TabIndex = 0;
            // 
            // pathColumn
            // 
            this.pathColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.pathColumn.DataPropertyName = "FullPath";
            this.pathColumn.HeaderText = "Watch Folders";
            this.pathColumn.Name = "pathColumn";
            this.pathColumn.ReadOnly = true;
            // 
            // pathsGroupBox
            // 
            this.pathsGroupBox.Controls.Add(this.importDvdCheckBox);
            this.pathsGroupBox.Controls.Add(this.groupBox1);
            this.pathsGroupBox.Controls.Add(this.label1);
            this.pathsGroupBox.Controls.Add(this.toolStrip);
            this.pathsGroupBox.Controls.Add(this.pathsGridView);
            this.pathsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pathsGroupBox.Location = new System.Drawing.Point(0, 0);
            this.pathsGroupBox.Name = "pathsGroupBox";
            this.pathsGroupBox.Size = new System.Drawing.Size(530, 155);
            this.pathsGroupBox.TabIndex = 1;
            this.pathsGroupBox.TabStop = false;
            this.pathsGroupBox.Text = "Media Sources";
            // 
            // importDvdCheckBox
            // 
            this.importDvdCheckBox.AutoSize = true;
            this.importDvdCheckBox.Location = new System.Drawing.Point(7, 131);
            this.importDvdCheckBox.Name = "importDvdCheckBox";
            this.importDvdCheckBox.Setting = null;
            this.importDvdCheckBox.Size = new System.Drawing.Size(192, 17);
            this.importDvdCheckBox.TabIndex = 8;
            this.importDvdCheckBox.Text = "Automatically Import Inserted DVDs";
            this.importDvdCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Location = new System.Drawing.Point(5, 121);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(520, 3);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(353, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Videos in the following folders will be processed up by the Media Importer:";
            // 
            // toolStrip
            // 
            this.toolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addSourceButton,
            this.removeSourceButton,
            this.toolStripSeparator1,
            this.helpButton});
            this.toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.toolStrip.Location = new System.Drawing.Point(503, 39);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(24, 77);
            this.toolStrip.TabIndex = 4;
            this.toolStrip.Text = "toolStrip1";
            // 
            // addSourceButton
            // 
            this.addSourceButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.addSourceButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.list_add;
            this.addSourceButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addSourceButton.Name = "addSourceButton";
            this.addSourceButton.Size = new System.Drawing.Size(30, 20);
            this.addSourceButton.Text = "toolStripButton1";
            this.addSourceButton.ToolTipText = "Add Watch Folder";
            this.addSourceButton.Click += new System.EventHandler(this.addSourceButton_Click);
            // 
            // removeSourceButton
            // 
            this.removeSourceButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.removeSourceButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.list_remove;
            this.removeSourceButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.removeSourceButton.Name = "removeSourceButton";
            this.removeSourceButton.Size = new System.Drawing.Size(30, 20);
            this.removeSourceButton.Text = "toolStripButton2";
            this.removeSourceButton.ToolTipText = "Remove Watch Folder";
            this.removeSourceButton.Click += new System.EventHandler(this.removeSourceButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(30, 6);
            // 
            // helpButton
            // 
            this.helpButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.helpButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.help;
            this.helpButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(30, 20);
            this.helpButton.Text = "toolStripButton3";
            this.helpButton.ToolTipText = "Help";
            this.helpButton.Click += new System.EventHandler(this.helpButton_Click);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn1.DataPropertyName = "FullPath";
            this.dataGridViewTextBoxColumn1.HeaderText = "Watch Folders";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // ImportPathsPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pathsGroupBox);
            this.MinimumSize = new System.Drawing.Size(530, 155);
            this.Name = "ImportPathsPane";
            this.Size = new System.Drawing.Size(530, 155);
            ((System.ComponentModel.ISupportInitialize)(this.pathsGridView)).EndInit();
            this.pathsGroupBox.ResumeLayout(false);
            this.pathsGroupBox.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView pathsGridView;
        private System.Windows.Forms.GroupBox pathsGroupBox;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton addSourceButton;
        private System.Windows.Forms.ToolStripButton removeSourceButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton helpButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn pathColumn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private Cornerstone.GUI.Controls.SettingCheckBox importDvdCheckBox;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    }
}
