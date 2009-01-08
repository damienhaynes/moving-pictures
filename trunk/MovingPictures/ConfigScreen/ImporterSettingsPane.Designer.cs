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
            this.helpButton1 = new System.Windows.Forms.Button();
            this.detailsButton = new System.Windows.Forms.Button();
            this.nfoAutoApproveCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.nfoExtTextBox = new Cornerstone.GUI.Controls.SettingsTextBox();
            this.nfoScannerCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.preferFolderCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.folderGroupingCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.strictYearCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.autoApproveTrackBar = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.autoApproveTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // helpButton1
            // 
            this.helpButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.helpButton1.BackgroundImage = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.help;
            this.helpButton1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.helpButton1.Location = new System.Drawing.Point(437, 0);
            this.helpButton1.Margin = new System.Windows.Forms.Padding(0);
            this.helpButton1.Name = "helpButton1";
            this.helpButton1.Size = new System.Drawing.Size(23, 23);
            this.helpButton1.TabIndex = 11;
            this.helpButton1.UseVisualStyleBackColor = true;
            // 
            // detailsButton
            // 
            this.detailsButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.detailsButton.Location = new System.Drawing.Point(128, 216);
            this.detailsButton.Name = "detailsButton";
            this.detailsButton.Size = new System.Drawing.Size(157, 23);
            this.detailsButton.TabIndex = 10;
            this.detailsButton.Text = "Movie Details Data Sources";
            this.detailsButton.UseVisualStyleBackColor = true;
            this.detailsButton.Click += new System.EventHandler(this.detailsButton_Click);
            // 
            // nfoAutoApproveCheckBox
            // 
            this.nfoAutoApproveCheckBox.AutoSize = true;
            this.nfoAutoApproveCheckBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nfoAutoApproveCheckBox.IgnoreSettingName = false;
            this.nfoAutoApproveCheckBox.Location = new System.Drawing.Point(147, 184);
            this.nfoAutoApproveCheckBox.Name = "nfoAutoApproveCheckBox";
            this.nfoAutoApproveCheckBox.Setting = null;
            this.nfoAutoApproveCheckBox.Size = new System.Drawing.Size(211, 17);
            this.nfoAutoApproveCheckBox.TabIndex = 9;
            this.nfoAutoApproveCheckBox.Text = "Auto approve based on info file details";
            this.nfoAutoApproveCheckBox.UseVisualStyleBackColor = true;
            // 
            // nfoExtTextBox
            // 
            this.nfoExtTextBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nfoExtTextBox.Location = new System.Drawing.Point(147, 158);
            this.nfoExtTextBox.Name = "nfoExtTextBox";
            this.nfoExtTextBox.Setting = null;
            this.nfoExtTextBox.Size = new System.Drawing.Size(158, 21);
            this.nfoExtTextBox.TabIndex = 8;
            // 
            // nfoScannerCheckBox
            // 
            this.nfoScannerCheckBox.AutoSize = true;
            this.nfoScannerCheckBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nfoScannerCheckBox.IgnoreSettingName = false;
            this.nfoScannerCheckBox.Location = new System.Drawing.Point(128, 135);
            this.nfoScannerCheckBox.Name = "nfoScannerCheckBox";
            this.nfoScannerCheckBox.Setting = null;
            this.nfoScannerCheckBox.Size = new System.Drawing.Size(317, 17);
            this.nfoScannerCheckBox.TabIndex = 7;
            this.nfoScannerCheckBox.Text = "Scan movie folder for info files with the following extensions:";
            this.nfoScannerCheckBox.UseVisualStyleBackColor = true;
            this.nfoScannerCheckBox.CheckedChanged += new System.EventHandler(this.nfoScannerCheckBox_CheckedChanged);
            // 
            // preferFolderCheckBox
            // 
            this.preferFolderCheckBox.AutoSize = true;
            this.preferFolderCheckBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.preferFolderCheckBox.IgnoreSettingName = false;
            this.preferFolderCheckBox.Location = new System.Drawing.Point(128, 71);
            this.preferFolderCheckBox.Name = "preferFolderCheckBox";
            this.preferFolderCheckBox.Setting = null;
            this.preferFolderCheckBox.Size = new System.Drawing.Size(213, 17);
            this.preferFolderCheckBox.TabIndex = 4;
            this.preferFolderCheckBox.Text = "Prefer Folder Name for Movie Matching";
            this.preferFolderCheckBox.UseVisualStyleBackColor = true;
            // 
            // folderGroupingCheckBox
            // 
            this.folderGroupingCheckBox.AutoSize = true;
            this.folderGroupingCheckBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.folderGroupingCheckBox.IgnoreSettingName = false;
            this.folderGroupingCheckBox.Location = new System.Drawing.Point(128, 103);
            this.folderGroupingCheckBox.Name = "folderGroupingCheckBox";
            this.folderGroupingCheckBox.Setting = null;
            this.folderGroupingCheckBox.Size = new System.Drawing.Size(208, 17);
            this.folderGroupingCheckBox.TabIndex = 3;
            this.folderGroupingCheckBox.Text = "Always Group Files in the Same Folder";
            this.folderGroupingCheckBox.UseVisualStyleBackColor = true;
            // 
            // strictYearCheckBox
            // 
            this.strictYearCheckBox.AutoSize = true;
            this.strictYearCheckBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.strictYearCheckBox.IgnoreSettingName = false;
            this.strictYearCheckBox.Location = new System.Drawing.Point(128, 48);
            this.strictYearCheckBox.Name = "strictYearCheckBox";
            this.strictYearCheckBox.Setting = null;
            this.strictYearCheckBox.Size = new System.Drawing.Size(237, 17);
            this.strictYearCheckBox.TabIndex = 2;
            this.strictYearCheckBox.Text = "Ignore Possible Matches with Incorrect Year";
            this.strictYearCheckBox.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(125, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Auto Approval Rate:";
            // 
            // autoApproveTrackBar
            // 
            this.autoApproveTrackBar.LargeChange = 3;
            this.autoApproveTrackBar.Location = new System.Drawing.Point(237, 0);
            this.autoApproveTrackBar.Maximum = 3;
            this.autoApproveTrackBar.Name = "autoApproveTrackBar";
            this.autoApproveTrackBar.Size = new System.Drawing.Size(72, 42);
            this.autoApproveTrackBar.TabIndex = 0;
            this.autoApproveTrackBar.Scroll += new System.EventHandler(this.autoApproveTrackBar_Scroll);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(4, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Movie Details:";
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Location = new System.Drawing.Point(120, 94);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(328, 3);
            this.groupBox4.TabIndex = 13;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "groupBox4";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Location = new System.Drawing.Point(120, 126);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(328, 3);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox4";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Location = new System.Drawing.Point(120, 207);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(328, 3);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "groupBox4";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Location = new System.Drawing.Point(8, 245);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(440, 3);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "groupBox4";
            // 
            // ImporterSettingsPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.helpButton1);
            this.Controls.Add(this.detailsButton);
            this.Controls.Add(this.autoApproveTrackBar);
            this.Controls.Add(this.nfoAutoApproveCheckBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nfoExtTextBox);
            this.Controls.Add(this.strictYearCheckBox);
            this.Controls.Add(this.nfoScannerCheckBox);
            this.Controls.Add(this.folderGroupingCheckBox);
            this.Controls.Add(this.preferFolderCheckBox);
            this.MinimumSize = new System.Drawing.Size(460, 250);
            this.Name = "ImporterSettingsPane";
            this.Size = new System.Drawing.Size(460, 250);
            this.Load += new System.EventHandler(this.ImporterSettingsPane_Load);
            ((System.ComponentModel.ISupportInitialize)(this.autoApproveTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar autoApproveTrackBar;
        private System.Windows.Forms.Label label1;
        private Cornerstone.GUI.Controls.SettingCheckBox strictYearCheckBox;
        private Cornerstone.GUI.Controls.SettingCheckBox preferFolderCheckBox;
        private Cornerstone.GUI.Controls.SettingCheckBox folderGroupingCheckBox;
        private Cornerstone.GUI.Controls.SettingCheckBox nfoScannerCheckBox;
        private Cornerstone.GUI.Controls.SettingsTextBox nfoExtTextBox;
        private Cornerstone.GUI.Controls.SettingCheckBox nfoAutoApproveCheckBox;
        private System.Windows.Forms.Button detailsButton;
        private System.Windows.Forms.Button helpButton1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
    }
}
