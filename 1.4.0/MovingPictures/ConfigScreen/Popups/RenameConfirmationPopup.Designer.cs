namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    partial class RenameConfirmationPopup {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.renameButton = new System.Windows.Forms.Button();
            this.skipButton = new System.Windows.Forms.Button();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.settingsDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            this.renameFoldersMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameFilesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameSecondaryFilesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.movieTitleLabel = new System.Windows.Forms.ToolStripLabel();
            this.confirmAllCheckBox = new System.Windows.Forms.CheckBox();
            this.fileListView = new System.Windows.Forms.ListView();
            this.originalNameColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.newNameColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.warningLabel = new System.Windows.Forms.Label();
            this.warningPanel = new System.Windows.Forms.Panel();
            this.retryLinkLabel = new System.Windows.Forms.LinkLabel();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.listPanel = new System.Windows.Forms.Panel();
            this.toolStrip.SuspendLayout();
            this.warningPanel.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.listPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // renameButton
            // 
            this.renameButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.renameButton.Location = new System.Drawing.Point(608, 3);
            this.renameButton.Name = "renameButton";
            this.renameButton.Size = new System.Drawing.Size(75, 23);
            this.renameButton.TabIndex = 0;
            this.renameButton.Text = "Rename";
            this.renameButton.UseVisualStyleBackColor = true;
            this.renameButton.Click += new System.EventHandler(this.renameButton_Click);
            // 
            // skipButton
            // 
            this.skipButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.skipButton.Location = new System.Drawing.Point(689, 3);
            this.skipButton.Name = "skipButton";
            this.skipButton.Size = new System.Drawing.Size(75, 23);
            this.skipButton.TabIndex = 1;
            this.skipButton.Text = "Skip";
            this.skipButton.UseVisualStyleBackColor = true;
            this.skipButton.Click += new System.EventHandler(this.skipButton_Click);
            // 
            // toolStrip
            // 
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsDropDown,
            this.movieTitleLabel});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(776, 25);
            this.toolStrip.TabIndex = 2;
            this.toolStrip.Text = "toolStrip1";
            // 
            // settingsDropDown
            // 
            this.settingsDropDown.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.settingsDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.settingsDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renameFoldersMenuItem,
            this.renameFilesMenuItem,
            this.renameSecondaryFilesMenuItem});
            this.settingsDropDown.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.cog;
            this.settingsDropDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.settingsDropDown.Name = "settingsDropDown";
            this.settingsDropDown.Size = new System.Drawing.Size(29, 22);
            this.settingsDropDown.Text = "toolStripDropDownButton1";
            // 
            // renameFoldersMenuItem
            // 
            this.renameFoldersMenuItem.CheckOnClick = true;
            this.renameFoldersMenuItem.Name = "renameFoldersMenuItem";
            this.renameFoldersMenuItem.Size = new System.Drawing.Size(288, 22);
            this.renameFoldersMenuItem.Text = "Include Folders When Renaming";
            this.renameFoldersMenuItem.Click += new System.EventHandler(this.renameFoldersMenuItem_Click);
            // 
            // renameFilesMenuItem
            // 
            this.renameFilesMenuItem.CheckOnClick = true;
            this.renameFilesMenuItem.Name = "renameFilesMenuItem";
            this.renameFilesMenuItem.Size = new System.Drawing.Size(288, 22);
            this.renameFilesMenuItem.Text = "Include Video Files When Renaming";
            this.renameFilesMenuItem.Click += new System.EventHandler(this.renameFilesMenuItem_Click);
            // 
            // renameSecondaryFilesMenuItem
            // 
            this.renameSecondaryFilesMenuItem.CheckOnClick = true;
            this.renameSecondaryFilesMenuItem.Name = "renameSecondaryFilesMenuItem";
            this.renameSecondaryFilesMenuItem.Size = new System.Drawing.Size(288, 22);
            this.renameSecondaryFilesMenuItem.Text = "Include Secondary Files When Renaming";
            this.renameSecondaryFilesMenuItem.Click += new System.EventHandler(this.includeSecondaryFilesMenuItem_Click);
            // 
            // movieTitleLabel
            // 
            this.movieTitleLabel.Name = "movieTitleLabel";
            this.movieTitleLabel.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.movieTitleLabel.Size = new System.Drawing.Size(86, 22);
            this.movieTitleLabel.Text = "Movie Title";
            // 
            // confirmAllCheckBox
            // 
            this.confirmAllCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.confirmAllCheckBox.AutoSize = true;
            this.confirmAllCheckBox.Location = new System.Drawing.Point(12, 9);
            this.confirmAllCheckBox.Name = "confirmAllCheckBox";
            this.confirmAllCheckBox.Size = new System.Drawing.Size(171, 17);
            this.confirmAllCheckBox.TabIndex = 3;
            this.confirmAllCheckBox.Text = "Do this for the next {0} movies.";
            this.confirmAllCheckBox.UseVisualStyleBackColor = true;
            // 
            // fileListView
            // 
            this.fileListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.fileListView.CheckBoxes = true;
            this.fileListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.originalNameColumnHeader,
            this.newNameColumnHeader});
            this.fileListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.fileListView.Location = new System.Drawing.Point(12, 3);
            this.fileListView.Name = "fileListView";
            this.fileListView.ShowItemToolTips = true;
            this.fileListView.Size = new System.Drawing.Size(752, 128);
            this.fileListView.TabIndex = 4;
            this.fileListView.UseCompatibleStateImageBehavior = false;
            this.fileListView.View = System.Windows.Forms.View.Details;
            this.fileListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.fileListView_ItemChecked);
            // 
            // originalNameColumnHeader
            // 
            this.originalNameColumnHeader.Text = "Original Name";
            this.originalNameColumnHeader.Width = 377;
            // 
            // newNameColumnHeader
            // 
            this.newNameColumnHeader.Text = "New Name";
            this.newNameColumnHeader.Width = 370;
            // 
            // warningLabel
            // 
            this.warningLabel.AutoSize = true;
            this.warningLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.warningLabel.ForeColor = System.Drawing.Color.Red;
            this.warningLabel.Location = new System.Drawing.Point(485, 0);
            this.warningLabel.Name = "warningLabel";
            this.warningLabel.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.warningLabel.Size = new System.Drawing.Size(249, 13);
            this.warningLabel.TabIndex = 5;
            this.warningLabel.Text = "The files for this movie are not currently available.";
            // 
            // warningPanel
            // 
            this.warningPanel.Controls.Add(this.warningLabel);
            this.warningPanel.Controls.Add(this.retryLinkLabel);
            this.warningPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.warningPanel.Location = new System.Drawing.Point(0, 162);
            this.warningPanel.Name = "warningPanel";
            this.warningPanel.Size = new System.Drawing.Size(776, 18);
            this.warningPanel.TabIndex = 6;
            // 
            // retryLinkLabel
            // 
            this.retryLinkLabel.AutoSize = true;
            this.retryLinkLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.retryLinkLabel.Location = new System.Drawing.Point(734, 0);
            this.retryLinkLabel.Name = "retryLinkLabel";
            this.retryLinkLabel.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.retryLinkLabel.Size = new System.Drawing.Size(42, 13);
            this.retryLinkLabel.TabIndex = 6;
            this.retryLinkLabel.TabStop = true;
            this.retryLinkLabel.Text = "Retry";
            // 
            // buttonPanel
            // 
            this.buttonPanel.Controls.Add(this.skipButton);
            this.buttonPanel.Controls.Add(this.renameButton);
            this.buttonPanel.Controls.Add(this.confirmAllCheckBox);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = new System.Drawing.Point(0, 180);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(776, 38);
            this.buttonPanel.TabIndex = 7;
            // 
            // listPanel
            // 
            this.listPanel.Controls.Add(this.fileListView);
            this.listPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listPanel.Location = new System.Drawing.Point(0, 25);
            this.listPanel.Name = "listPanel";
            this.listPanel.Size = new System.Drawing.Size(776, 137);
            this.listPanel.TabIndex = 8;
            // 
            // RenameConfirmationPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(776, 218);
            this.Controls.Add(this.listPanel);
            this.Controls.Add(this.warningPanel);
            this.Controls.Add(this.buttonPanel);
            this.Controls.Add(this.toolStrip);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RenameConfirmationPopup";
            this.ShowIcon = false;
            this.Text = "Confirm Renaming";
            this.Load += new System.EventHandler(this.RenameConfirmationPopup_Load);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.warningPanel.ResumeLayout(false);
            this.warningPanel.PerformLayout();
            this.buttonPanel.ResumeLayout(false);
            this.buttonPanel.PerformLayout();
            this.listPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button renameButton;
        private System.Windows.Forms.Button skipButton;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripDropDownButton settingsDropDown;
        private System.Windows.Forms.ToolStripMenuItem renameFilesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameFoldersMenuItem;
        private System.Windows.Forms.CheckBox confirmAllCheckBox;
        private System.Windows.Forms.ListView fileListView;
        private System.Windows.Forms.ColumnHeader originalNameColumnHeader;
        private System.Windows.Forms.ColumnHeader newNameColumnHeader;
        private System.Windows.Forms.Label warningLabel;
        private System.Windows.Forms.Panel warningPanel;
        private System.Windows.Forms.LinkLabel retryLinkLabel;
        private System.Windows.Forms.Panel buttonPanel;
        private System.Windows.Forms.Panel listPanel;
        private System.Windows.Forms.ToolStripLabel movieTitleLabel;
        private System.Windows.Forms.ToolStripMenuItem renameSecondaryFilesMenuItem;
    }
}