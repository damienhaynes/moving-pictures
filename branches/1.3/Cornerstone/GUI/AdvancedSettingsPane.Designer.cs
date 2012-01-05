namespace Cornerstone.GUI {
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
            this.mainGroupBox = new System.Windows.Forms.GroupBox();
            this.advancedSettingsTreeView = new System.Windows.Forms.TreeView();
            this.setDescriptionLabel = new System.Windows.Forms.Label();
            this.updateSettingButton = new System.Windows.Forms.Button();
            this.setValueTextBox = new System.Windows.Forms.TextBox();
            this.detailsGroupBox = new System.Windows.Forms.GroupBox();
            this.moreInfoLinkLabel = new System.Windows.Forms.LinkLabel();
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
            this.mainGroupBox.Location = new System.Drawing.Point(4, 4);
            this.mainGroupBox.Name = "mainGroupBox";
            this.mainGroupBox.Size = new System.Drawing.Size(421, 282);
            this.mainGroupBox.TabIndex = 0;
            this.mainGroupBox.TabStop = false;
            // 
            // advancedSettingsTreeView
            // 
            this.advancedSettingsTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.advancedSettingsTreeView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.advancedSettingsTreeView.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.advancedSettingsTreeView.FullRowSelect = true;
            this.advancedSettingsTreeView.Indent = 0;
            this.advancedSettingsTreeView.Location = new System.Drawing.Point(7, 13);
            this.advancedSettingsTreeView.Name = "advancedSettingsTreeView";
            this.advancedSettingsTreeView.ShowPlusMinus = false;
            this.advancedSettingsTreeView.ShowRootLines = false;
            this.advancedSettingsTreeView.Size = new System.Drawing.Size(408, 263);
            this.advancedSettingsTreeView.TabIndex = 3;
            this.advancedSettingsTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.advancedSettingsTreeView_AfterSelect);
            // 
            // setDescriptionLabel
            // 
            this.setDescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.setDescriptionLabel.BackColor = System.Drawing.SystemColors.Control;
            this.setDescriptionLabel.Location = new System.Drawing.Point(6, 16);
            this.setDescriptionLabel.Name = "setDescriptionLabel";
            this.setDescriptionLabel.Size = new System.Drawing.Size(411, 52);
            this.setDescriptionLabel.TabIndex = 7;
            // 
            // updateSettingButton
            // 
            this.updateSettingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.updateSettingButton.Enabled = false;
            this.updateSettingButton.Location = new System.Drawing.Point(340, 69);
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
            this.setValueTextBox.Size = new System.Drawing.Size(328, 20);
            this.setValueTextBox.TabIndex = 5;
            this.setValueTextBox.TextChanged += new System.EventHandler(this.setValueTextBox_TextChanged);
            this.setValueTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.setValueTextBox_KeyUp);
            // 
            // detailsGroupBox
            // 
            this.detailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.detailsGroupBox.Controls.Add(this.updateSettingButton);
            this.detailsGroupBox.Controls.Add(this.moreInfoLinkLabel);
            this.detailsGroupBox.Controls.Add(this.setDescriptionLabel);
            this.detailsGroupBox.Controls.Add(this.setValueTextBox);
            this.detailsGroupBox.Location = new System.Drawing.Point(4, 292);
            this.detailsGroupBox.Name = "detailsGroupBox";
            this.detailsGroupBox.Size = new System.Drawing.Size(421, 100);
            this.detailsGroupBox.TabIndex = 1;
            this.detailsGroupBox.TabStop = false;
            this.detailsGroupBox.Text = "Setting Details";
            // 
            // moreInfoLinkLabel
            // 
            this.moreInfoLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.moreInfoLinkLabel.AutoSize = true;
            this.moreInfoLinkLabel.Location = new System.Drawing.Point(359, 54);
            this.moreInfoLinkLabel.Name = "moreInfoLinkLabel";
            this.moreInfoLinkLabel.Size = new System.Drawing.Size(52, 13);
            this.moreInfoLinkLabel.TabIndex = 8;
            this.moreInfoLinkLabel.TabStop = true;
            this.moreInfoLinkLabel.Text = "More Info";
            this.moreInfoLinkLabel.Visible = false;
            this.moreInfoLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.moreInfoLinkLabel_LinkClicked);
            // 
            // AdvancedSettingsPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.detailsGroupBox);
            this.Controls.Add(this.mainGroupBox);
            this.Name = "AdvancedSettingsPane";
            this.Size = new System.Drawing.Size(428, 395);
            this.Tag = "";
            this.mainGroupBox.ResumeLayout(false);
            this.detailsGroupBox.ResumeLayout(false);
            this.detailsGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }                

        #endregion

        private System.Windows.Forms.GroupBox mainGroupBox;
        private System.Windows.Forms.TreeView advancedSettingsTreeView;
        private System.Windows.Forms.TextBox setValueTextBox;
        private System.Windows.Forms.Button updateSettingButton;
        private System.Windows.Forms.Label setDescriptionLabel;
        private System.Windows.Forms.GroupBox detailsGroupBox;
        private System.Windows.Forms.LinkLabel moreInfoLinkLabel;

    }
}
