namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    partial class ImporterSettingsPane {
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
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.nfoAutoApproveCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.nfoExtTextBox = new Cornerstone.GUI.Controls.SettingsTextBox();
            this.nfoScannerCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.preferFolderCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.folderGroupingCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.strictYearCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.autoApproveTrackBar = new System.Windows.Forms.TrackBar();
            this.detailsButton = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.helpButton1 = new System.Windows.Forms.Button();
            this.groupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.autoApproveTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox
            // 
            this.groupBox.Controls.Add(this.helpButton1);
            this.groupBox.Controls.Add(this.groupBox3);
            this.groupBox.Controls.Add(this.detailsButton);
            this.groupBox.Controls.Add(this.nfoAutoApproveCheckBox);
            this.groupBox.Controls.Add(this.nfoExtTextBox);
            this.groupBox.Controls.Add(this.nfoScannerCheckBox);
            this.groupBox.Controls.Add(this.groupBox2);
            this.groupBox.Controls.Add(this.groupBox1);
            this.groupBox.Controls.Add(this.preferFolderCheckBox);
            this.groupBox.Controls.Add(this.folderGroupingCheckBox);
            this.groupBox.Controls.Add(this.strictYearCheckBox);
            this.groupBox.Controls.Add(this.label1);
            this.groupBox.Controls.Add(this.autoApproveTrackBar);
            this.groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox.Location = new System.Drawing.Point(0, 0);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(391, 304);
            this.groupBox.TabIndex = 0;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "Movie Details";
            // 
            // nfoAutoApproveCheckBox
            // 
            this.nfoAutoApproveCheckBox.AutoSize = true;
            this.nfoAutoApproveCheckBox.IgnoreSettingName = false;
            this.nfoAutoApproveCheckBox.Location = new System.Drawing.Point(29, 224);
            this.nfoAutoApproveCheckBox.Name = "nfoAutoApproveCheckBox";
            this.nfoAutoApproveCheckBox.Setting = null;
            this.nfoAutoApproveCheckBox.Size = new System.Drawing.Size(206, 17);
            this.nfoAutoApproveCheckBox.TabIndex = 9;
            this.nfoAutoApproveCheckBox.Text = "Auto approve based on info file details";
            this.nfoAutoApproveCheckBox.UseVisualStyleBackColor = true;
            // 
            // nfoExtTextBox
            // 
            this.nfoExtTextBox.Location = new System.Drawing.Point(29, 198);
            this.nfoExtTextBox.Name = "nfoExtTextBox";
            this.nfoExtTextBox.Setting = null;
            this.nfoExtTextBox.Size = new System.Drawing.Size(158, 20);
            this.nfoExtTextBox.TabIndex = 8;
            // 
            // nfoScannerCheckBox
            // 
            this.nfoScannerCheckBox.AutoSize = true;
            this.nfoScannerCheckBox.IgnoreSettingName = false;
            this.nfoScannerCheckBox.Location = new System.Drawing.Point(10, 175);
            this.nfoScannerCheckBox.Name = "nfoScannerCheckBox";
            this.nfoScannerCheckBox.Setting = null;
            this.nfoScannerCheckBox.Size = new System.Drawing.Size(307, 17);
            this.nfoScannerCheckBox.TabIndex = 7;
            this.nfoScannerCheckBox.Text = "Scan movie folder for info files with the following extensions:";
            this.nfoScannerCheckBox.UseVisualStyleBackColor = true;
            this.nfoScannerCheckBox.CheckedChanged += new System.EventHandler(this.nfoScannerCheckBox_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Location = new System.Drawing.Point(8, 155);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(375, 3);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "groupBox2";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Location = new System.Drawing.Point(8, 113);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(375, 3);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // preferFolderCheckBox
            // 
            this.preferFolderCheckBox.AutoSize = true;
            this.preferFolderCheckBox.IgnoreSettingName = false;
            this.preferFolderCheckBox.Location = new System.Drawing.Point(9, 90);
            this.preferFolderCheckBox.Name = "preferFolderCheckBox";
            this.preferFolderCheckBox.Setting = null;
            this.preferFolderCheckBox.Size = new System.Drawing.Size(211, 17);
            this.preferFolderCheckBox.TabIndex = 4;
            this.preferFolderCheckBox.Text = "Prefer Folder Name for Movie Matching";
            this.preferFolderCheckBox.UseVisualStyleBackColor = true;
            // 
            // folderGroupingCheckBox
            // 
            this.folderGroupingCheckBox.AutoSize = true;
            this.folderGroupingCheckBox.IgnoreSettingName = false;
            this.folderGroupingCheckBox.Location = new System.Drawing.Point(10, 132);
            this.folderGroupingCheckBox.Name = "folderGroupingCheckBox";
            this.folderGroupingCheckBox.Setting = null;
            this.folderGroupingCheckBox.Size = new System.Drawing.Size(206, 17);
            this.folderGroupingCheckBox.TabIndex = 3;
            this.folderGroupingCheckBox.Text = "Always Group Files in the Same Folder";
            this.folderGroupingCheckBox.UseVisualStyleBackColor = true;
            // 
            // strictYearCheckBox
            // 
            this.strictYearCheckBox.AutoSize = true;
            this.strictYearCheckBox.IgnoreSettingName = false;
            this.strictYearCheckBox.Location = new System.Drawing.Point(9, 67);
            this.strictYearCheckBox.Name = "strictYearCheckBox";
            this.strictYearCheckBox.Setting = null;
            this.strictYearCheckBox.Size = new System.Drawing.Size(234, 17);
            this.strictYearCheckBox.TabIndex = 2;
            this.strictYearCheckBox.Text = "Ignore Possible Matches with Incorrect Year";
            this.strictYearCheckBox.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Auto Approval Rate:";
            // 
            // autoApproveTrackBar
            // 
            this.autoApproveTrackBar.LargeChange = 3;
            this.autoApproveTrackBar.Location = new System.Drawing.Point(115, 19);
            this.autoApproveTrackBar.Maximum = 3;
            this.autoApproveTrackBar.Name = "autoApproveTrackBar";
            this.autoApproveTrackBar.Size = new System.Drawing.Size(72, 42);
            this.autoApproveTrackBar.TabIndex = 0;
            this.autoApproveTrackBar.Scroll += new System.EventHandler(this.autoApproveTrackBar_Scroll);
            // 
            // detailsButton
            // 
            this.detailsButton.Location = new System.Drawing.Point(10, 266);
            this.detailsButton.Name = "detailsButton";
            this.detailsButton.Size = new System.Drawing.Size(157, 23);
            this.detailsButton.TabIndex = 10;
            this.detailsButton.Text = "Movie Details Data Sources";
            this.detailsButton.UseVisualStyleBackColor = true;
            this.detailsButton.Click += new System.EventHandler(this.detailsButton_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Location = new System.Drawing.Point(8, 247);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(375, 3);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "groupBox3";
            // 
            // helpButton1
            // 
            this.helpButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.helpButton1.BackgroundImage = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.help;
            this.helpButton1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.helpButton1.Location = new System.Drawing.Point(358, 15);
            this.helpButton1.Margin = new System.Windows.Forms.Padding(0);
            this.helpButton1.Name = "helpButton1";
            this.helpButton1.Size = new System.Drawing.Size(23, 23);
            this.helpButton1.TabIndex = 11;
            this.helpButton1.UseVisualStyleBackColor = true;
            // 
            // ImporterSettingsPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox);
            this.MinimumSize = new System.Drawing.Size(332, 304);
            this.Name = "ImporterSettingsPane";
            this.Size = new System.Drawing.Size(391, 304);
            this.Load += new System.EventHandler(this.ImporterSettingsPane_Load);
            this.groupBox.ResumeLayout(false);
            this.groupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.autoApproveTrackBar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.TrackBar autoApproveTrackBar;
        private System.Windows.Forms.Label label1;
        private Cornerstone.GUI.Controls.SettingCheckBox strictYearCheckBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private Cornerstone.GUI.Controls.SettingCheckBox preferFolderCheckBox;
        private Cornerstone.GUI.Controls.SettingCheckBox folderGroupingCheckBox;
        private Cornerstone.GUI.Controls.SettingCheckBox nfoScannerCheckBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private Cornerstone.GUI.Controls.SettingsTextBox nfoExtTextBox;
        private Cornerstone.GUI.Controls.SettingCheckBox nfoAutoApproveCheckBox;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button detailsButton;
        private System.Windows.Forms.Button helpButton1;
    }
}
