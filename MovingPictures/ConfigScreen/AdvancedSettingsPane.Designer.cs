namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    partial class AdvancedSettingsPane {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdvancedSettingsPane));
            this.mainGroupBox = new System.Windows.Forms.GroupBox();
            this.advancedSettingsTreeView = new System.Windows.Forms.TreeView();
            this.infoLabel = new System.Windows.Forms.TextBox();
            this.setDescriptionLabel = new System.Windows.Forms.Label();
            this.updateSettingButton = new System.Windows.Forms.Button();
            this.setValueTextBox = new System.Windows.Forms.TextBox();
            this.detailsGroupBox = new System.Windows.Forms.GroupBox();
            this.mainGroupBox.SuspendLayout();
            this.detailsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainGroupBox
            // 
            this.mainGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mainGroupBox.Controls.Add(this.advancedSettingsTreeView);
            this.mainGroupBox.Controls.Add(this.infoLabel);
            this.mainGroupBox.Location = new System.Drawing.Point(4, 4);
            this.mainGroupBox.Name = "mainGroupBox";
            this.mainGroupBox.Size = new System.Drawing.Size(526, 436);
            this.mainGroupBox.TabIndex = 0;
            this.mainGroupBox.TabStop = false;
            // 
            // advancedSettingsTreeView
            // 
            this.advancedSettingsTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.advancedSettingsTreeView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.advancedSettingsTreeView.FullRowSelect = true;
            this.advancedSettingsTreeView.Indent = 0;
            this.advancedSettingsTreeView.Location = new System.Drawing.Point(7, 85);
            this.advancedSettingsTreeView.Name = "advancedSettingsTreeView";
            this.advancedSettingsTreeView.ShowLines = false;
            this.advancedSettingsTreeView.ShowPlusMinus = false;
            this.advancedSettingsTreeView.ShowRootLines = false;
            this.advancedSettingsTreeView.Size = new System.Drawing.Size(513, 345);
            this.advancedSettingsTreeView.TabIndex = 3;
            this.advancedSettingsTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.advancedSettingsTreeView_AfterSelect);
            // 
            // infoLabel
            // 
            this.infoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.infoLabel.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.infoLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.infoLabel.Location = new System.Drawing.Point(6, 9);
            this.infoLabel.Multiline = true;
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.ReadOnly = true;
            this.infoLabel.Size = new System.Drawing.Size(514, 70);
            this.infoLabel.TabIndex = 2;
            this.infoLabel.Text = resources.GetString("infoLabel.Text");
            // 
            // setDescriptionLabel
            // 
            this.setDescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.setDescriptionLabel.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.setDescriptionLabel.Location = new System.Drawing.Point(6, 16);
            this.setDescriptionLabel.Name = "setDescriptionLabel";
            this.setDescriptionLabel.Size = new System.Drawing.Size(516, 52);
            this.setDescriptionLabel.TabIndex = 7;
            // 
            // updateSettingButton
            // 
            this.updateSettingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.updateSettingButton.Enabled = false;
            this.updateSettingButton.Location = new System.Drawing.Point(445, 69);
            this.updateSettingButton.Name = "updateSettingButton";
            this.updateSettingButton.Size = new System.Drawing.Size(75, 23);
            this.updateSettingButton.TabIndex = 6;
            this.updateSettingButton.Text = "Update";
            this.updateSettingButton.UseVisualStyleBackColor = true;
            this.updateSettingButton.Click += new System.EventHandler(this.updateSettingButton_Click);
            // 
            // setValueTextBox
            // 
            this.setValueTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.setValueTextBox.Enabled = false;
            this.setValueTextBox.Location = new System.Drawing.Point(6, 71);
            this.setValueTextBox.Name = "setValueTextBox";
            this.setValueTextBox.Size = new System.Drawing.Size(433, 20);
            this.setValueTextBox.TabIndex = 5;
            this.setValueTextBox.TextChanged += new System.EventHandler(this.setValueTextBox_TextChanged);
            this.setValueTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.setValueTextBox_KeyUp);
            // 
            // detailsGroupBox
            // 
            this.detailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.detailsGroupBox.Controls.Add(this.updateSettingButton);
            this.detailsGroupBox.Controls.Add(this.setDescriptionLabel);
            this.detailsGroupBox.Controls.Add(this.setValueTextBox);
            this.detailsGroupBox.Location = new System.Drawing.Point(4, 446);
            this.detailsGroupBox.Name = "detailsGroupBox";
            this.detailsGroupBox.Size = new System.Drawing.Size(526, 100);
            this.detailsGroupBox.TabIndex = 1;
            this.detailsGroupBox.TabStop = false;
            this.detailsGroupBox.Text = "Setting Details";
            // 
            // AdvancedSettingsPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.detailsGroupBox);
            this.Controls.Add(this.mainGroupBox);
            this.Name = "AdvancedSettingsPane";
            this.Size = new System.Drawing.Size(533, 549);
            this.Tag = "";
            this.mainGroupBox.ResumeLayout(false);
            this.mainGroupBox.PerformLayout();
            this.detailsGroupBox.ResumeLayout(false);
            this.detailsGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }                

        #endregion

        private System.Windows.Forms.GroupBox mainGroupBox;
        private System.Windows.Forms.TextBox infoLabel;
        private System.Windows.Forms.TreeView advancedSettingsTreeView;
        private System.Windows.Forms.TextBox setValueTextBox;
        private System.Windows.Forms.Button updateSettingButton;
        private System.Windows.Forms.Label setDescriptionLabel;
        private System.Windows.Forms.GroupBox detailsGroupBox;

    }
}
