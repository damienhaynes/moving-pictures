namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    partial class AboutPane {
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
            this.advancedSettingsButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.versionLabel = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.codeLabel = new System.Windows.Forms.LinkLabel();
            this.forumLabel = new System.Windows.Forms.LinkLabel();
            this.websiteLabel = new System.Windows.Forms.LinkLabel();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // advancedSettingsButton
            // 
            this.advancedSettingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.advancedSettingsButton.Location = new System.Drawing.Point(348, 488);
            this.advancedSettingsButton.Name = "advancedSettingsButton";
            this.advancedSettingsButton.Size = new System.Drawing.Size(122, 23);
            this.advancedSettingsButton.TabIndex = 0;
            this.advancedSettingsButton.Text = "Advanced Settings";
            this.advancedSettingsButton.UseVisualStyleBackColor = true;
            this.advancedSettingsButton.Click += new System.EventHandler(this.advancedSettingsButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(240, 39);
            this.label1.TabIndex = 1;
            this.label1.Text = "Moving Pictures";
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Location = new System.Drawing.Point(7, 39);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(42, 13);
            this.versionLabel.TabIndex = 2;
            this.versionLabel.Text = "Version";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.versionLabel);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(473, 100);
            this.panel1.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.codeLabel);
            this.groupBox1.Controls.Add(this.forumLabel);
            this.groupBox1.Controls.Add(this.websiteLabel);
            this.groupBox1.Location = new System.Drawing.Point(3, 388);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(467, 94);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Online Resources";
            // 
            // codeLabel
            // 
            this.codeLabel.AutoSize = true;
            this.codeLabel.Location = new System.Drawing.Point(6, 40);
            this.codeLabel.Name = "codeLabel";
            this.codeLabel.Size = new System.Drawing.Size(68, 13);
            this.codeLabel.TabIndex = 2;
            this.codeLabel.TabStop = true;
            this.codeLabel.Text = "Google Code";
            this.codeLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.codeLabel_LinkClicked);
            // 
            // forumLabel
            // 
            this.forumLabel.AutoSize = true;
            this.forumLabel.Location = new System.Drawing.Point(6, 63);
            this.forumLabel.Name = "forumLabel";
            this.forumLabel.Size = new System.Drawing.Size(37, 13);
            this.forumLabel.TabIndex = 1;
            this.forumLabel.TabStop = true;
            this.forumLabel.Text = "Forum";
            this.forumLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.forumLabel_LinkClicked);
            // 
            // websiteLabel
            // 
            this.websiteLabel.AutoSize = true;
            this.websiteLabel.Location = new System.Drawing.Point(6, 17);
            this.websiteLabel.Name = "websiteLabel";
            this.websiteLabel.Size = new System.Drawing.Size(75, 13);
            this.websiteLabel.TabIndex = 0;
            this.websiteLabel.TabStop = true;
            this.websiteLabel.Text = "Website / Wiki";
            this.websiteLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.websiteLabel_LinkClicked);
            // 
            // AboutPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.advancedSettingsButton);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "AboutPane";
            this.Size = new System.Drawing.Size(473, 514);
            this.Load += new System.EventHandler(this.AboutPane_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button advancedSettingsButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.LinkLabel forumLabel;
        private System.Windows.Forms.LinkLabel websiteLabel;
        private System.Windows.Forms.LinkLabel codeLabel;
    }
}
