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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportPathsPane));
            this.pathsGridView = new System.Windows.Forms.DataGridView();
            this.pathColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Removable = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pathsGroupBox = new System.Windows.Forms.GroupBox();
            this.removeSourceButton = new System.Windows.Forms.Button();
            this.addSourceButton = new System.Windows.Forms.Button();
            this.notesLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pathsGridView)).BeginInit();
            this.pathsGroupBox.SuspendLayout();
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
            this.pathsGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.pathColumn,
            this.Removable});
            this.pathsGridView.Location = new System.Drawing.Point(6, 97);
            this.pathsGridView.MultiSelect = false;
            this.pathsGridView.Name = "pathsGridView";
            this.pathsGridView.RowHeadersVisible = false;
            this.pathsGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.pathsGridView.Size = new System.Drawing.Size(480, 167);
            this.pathsGridView.TabIndex = 0;
            // 
            // pathColumn
            // 
            this.pathColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.pathColumn.DataPropertyName = "FullPath";
            this.pathColumn.HeaderText = "Path";
            this.pathColumn.Name = "pathColumn";
            this.pathColumn.ReadOnly = true;
            // 
            // Removable
            // 
            this.Removable.DataPropertyName = "IsRemovable";
            this.Removable.HeaderText = "Removable";
            this.Removable.Name = "Removable";
            this.Removable.ReadOnly = true;
            // 
            // pathsGroupBox
            // 
            this.pathsGroupBox.Controls.Add(this.removeSourceButton);
            this.pathsGroupBox.Controls.Add(this.addSourceButton);
            this.pathsGroupBox.Controls.Add(this.notesLabel);
            this.pathsGroupBox.Controls.Add(this.pathsGridView);
            this.pathsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pathsGroupBox.Location = new System.Drawing.Point(0, 0);
            this.pathsGroupBox.Name = "pathsGroupBox";
            this.pathsGroupBox.Size = new System.Drawing.Size(530, 270);
            this.pathsGroupBox.TabIndex = 1;
            this.pathsGroupBox.TabStop = false;
            this.pathsGroupBox.Text = "Media Sources";
            // 
            // removeSourceButton
            // 
            this.removeSourceButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.removeSourceButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.list_remove;
            this.removeSourceButton.Location = new System.Drawing.Point(492, 132);
            this.removeSourceButton.Margin = new System.Windows.Forms.Padding(0);
            this.removeSourceButton.Name = "removeSourceButton";
            this.removeSourceButton.Size = new System.Drawing.Size(32, 32);
            this.removeSourceButton.TabIndex = 3;
            this.removeSourceButton.UseVisualStyleBackColor = true;
            this.removeSourceButton.Click += new System.EventHandler(this.removeSourceButton_Click);
            // 
            // addSourceButton
            // 
            this.addSourceButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.addSourceButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.list_add;
            this.addSourceButton.Location = new System.Drawing.Point(492, 97);
            this.addSourceButton.Name = "addSourceButton";
            this.addSourceButton.Size = new System.Drawing.Size(32, 32);
            this.addSourceButton.TabIndex = 2;
            this.addSourceButton.UseVisualStyleBackColor = true;
            this.addSourceButton.Click += new System.EventHandler(this.addSourceButton_Click);
            // 
            // notesLabel
            // 
            this.notesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.notesLabel.Location = new System.Drawing.Point(7, 19);
            this.notesLabel.Name = "notesLabel";
            this.notesLabel.Size = new System.Drawing.Size(517, 72);
            this.notesLabel.TabIndex = 1;
            this.notesLabel.Text = resources.GetString("notesLabel.Text");
            // 
            // ImportPathsPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pathsGroupBox);
            this.MinimumSize = new System.Drawing.Size(530, 150);
            this.Name = "ImportPathsPane";
            this.Size = new System.Drawing.Size(530, 270);
            ((System.ComponentModel.ISupportInitialize)(this.pathsGridView)).EndInit();
            this.pathsGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView pathsGridView;
        private System.Windows.Forms.GroupBox pathsGroupBox;
        private System.Windows.Forms.Label notesLabel;
        private System.Windows.Forms.Button addSourceButton;
        private System.Windows.Forms.Button removeSourceButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn pathColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Removable;
    }
}
