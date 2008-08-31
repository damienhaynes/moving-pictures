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
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.advancedSettingsTreeView = new System.Windows.Forms.TreeView();
            this.infoLabel = new System.Windows.Forms.TextBox();
            this.groupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox
            // 
            this.groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox.Controls.Add(this.advancedSettingsTreeView);
            this.groupBox.Controls.Add(this.infoLabel);
            this.groupBox.Location = new System.Drawing.Point(4, 4);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(526, 542);
            this.groupBox.TabIndex = 0;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "Advanced Settings";
            // 
            // advancedSettingsTreeView
            // 
            this.advancedSettingsTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.advancedSettingsTreeView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.advancedSettingsTreeView.FullRowSelect = true;
            this.advancedSettingsTreeView.Indent = 0;
            this.advancedSettingsTreeView.Location = new System.Drawing.Point(7, 96);
            this.advancedSettingsTreeView.Name = "advancedSettingsTreeView";
            this.advancedSettingsTreeView.ShowLines = false;
            this.advancedSettingsTreeView.ShowPlusMinus = false;
            this.advancedSettingsTreeView.ShowRootLines = false;
            this.advancedSettingsTreeView.Size = new System.Drawing.Size(513, 440);
            this.advancedSettingsTreeView.TabIndex = 3;
            this.advancedSettingsTreeView.DoubleClick += new System.EventHandler(this.advancedSettingsTreeView_DoubleClick);
            // 
            // infoLabel
            // 
            this.infoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.infoLabel.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.infoLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.infoLabel.Location = new System.Drawing.Point(6, 19);
            this.infoLabel.Multiline = true;
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.ReadOnly = true;
            this.infoLabel.Size = new System.Drawing.Size(514, 70);
            this.infoLabel.TabIndex = 2;
            this.infoLabel.Text = resources.GetString("infoLabel.Text");
            // 
            // AdvancedSettingsPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox);
            this.Name = "AdvancedSettingsPane";
            this.Size = new System.Drawing.Size(533, 549);
            this.Tag = "";
            this.groupBox.ResumeLayout(false);
            this.groupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.TextBox infoLabel;
        private System.Windows.Forms.TreeView advancedSettingsTreeView;

    }
}
