using Cornerstone.GUI.Controls;
using System.Windows.Forms;
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
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label label7;
            this.helpButton1 = new System.Windows.Forms.Button();
            this.detailsButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.autoApproveTrackBar = new System.Windows.Forms.TrackBar();
            this.nfoAutoApproveCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.nfoExtTextBox = new Cornerstone.GUI.Controls.SettingsTextBox();
            this.nfoScannerCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.folderGroupingCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.preferFolderCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.settingAutoApproveAlternateTitle = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.coverDataSources = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.backdropSourcesButton = new System.Windows.Forms.Button();
            this.coverFromMovieFolderCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.coverCountTextBox = new Cornerstone.GUI.Controls.SettingsTextBox();
            this.backdropFromMovieFolderCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.coverPatternTextBox = new Cornerstone.GUI.Controls.SettingsTextBox();
            this.backdropPatternTextBox = new Cornerstone.GUI.Controls.SettingsTextBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.autoRescanCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.rescanIntervalTextBox = new Cornerstone.GUI.Controls.SettingsTextBox();
            this.rescanHelpLinkLabel = new System.Windows.Forms.LinkLabel();
            this.autoDataSourcesPanel1 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.AutoDataSourcesPanel();
            label6 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.autoApproveTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            label6.Location = new System.Drawing.Point(4, 314);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(51, 13);
            label6.TabIndex = 29;
            label6.Text = "Rescan:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(427, 318);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(48, 13);
            label7.TabIndex = 32;
            label7.Text = "minutes.";
            // 
            // helpButton1
            // 
            this.helpButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.helpButton1.BackgroundImage = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.help;
            this.helpButton1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.helpButton1.Location = new System.Drawing.Point(575, 0);
            this.helpButton1.Margin = new System.Windows.Forms.Padding(0);
            this.helpButton1.Name = "helpButton1";
            this.helpButton1.Size = new System.Drawing.Size(23, 23);
            this.helpButton1.TabIndex = 11;
            this.helpButton1.UseVisualStyleBackColor = true;
            this.helpButton1.Click += new System.EventHandler(this.helpButton1_Click);
            // 
            // detailsButton
            // 
            this.detailsButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.detailsButton.Location = new System.Drawing.Point(127, 417);
            this.detailsButton.Name = "detailsButton";
            this.detailsButton.Size = new System.Drawing.Size(157, 23);
            this.detailsButton.TabIndex = 10;
            this.detailsButton.Text = "Movie Details Data Sources";
            this.detailsButton.UseVisualStyleBackColor = true;
            this.detailsButton.Click += new System.EventHandler(this.detailsButton_Click);
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
            this.groupBox4.Size = new System.Drawing.Size(466, 3);
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
            this.groupBox1.Size = new System.Drawing.Size(466, 3);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox4";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Location = new System.Drawing.Point(120, 408);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(466, 3);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "groupBox4";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Location = new System.Drawing.Point(8, 207);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(578, 3);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "groupBox4";
            // 
            // autoApproveTrackBar
            // 
            this.autoApproveTrackBar.LargeChange = 1;
            this.autoApproveTrackBar.Location = new System.Drawing.Point(237, 0);
            this.autoApproveTrackBar.Maximum = 3;
            this.autoApproveTrackBar.Name = "autoApproveTrackBar";
            this.autoApproveTrackBar.Size = new System.Drawing.Size(72, 45);
            this.autoApproveTrackBar.TabIndex = 0;
            this.autoApproveTrackBar.Scroll += new System.EventHandler(this.autoApproveTrackBar_Scroll);
            // 
            // nfoAutoApproveCheckBox
            // 
            this.nfoAutoApproveCheckBox.AutoSize = true;
            this.nfoAutoApproveCheckBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nfoAutoApproveCheckBox.IgnoreSettingName = true;
            this.nfoAutoApproveCheckBox.Location = new System.Drawing.Point(147, 184);
            this.nfoAutoApproveCheckBox.Name = "nfoAutoApproveCheckBox";
            this.nfoAutoApproveCheckBox.Setting = null;
            this.nfoAutoApproveCheckBox.Size = new System.Drawing.Size(247, 17);
            this.nfoAutoApproveCheckBox.TabIndex = 9;
            this.nfoAutoApproveCheckBox.Text = "Force auto-approval based on info file details.";
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
            this.nfoScannerCheckBox.IgnoreSettingName = true;
            this.nfoScannerCheckBox.Location = new System.Drawing.Point(128, 135);
            this.nfoScannerCheckBox.Name = "nfoScannerCheckBox";
            this.nfoScannerCheckBox.Setting = null;
            this.nfoScannerCheckBox.Size = new System.Drawing.Size(317, 17);
            this.nfoScannerCheckBox.TabIndex = 7;
            this.nfoScannerCheckBox.Text = "Scan movie folder for info files with the following extensions:";
            this.nfoScannerCheckBox.UseVisualStyleBackColor = true;
            this.nfoScannerCheckBox.CheckedChanged += new System.EventHandler(this.nfoScannerCheckBox_CheckedChanged);
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
            // settingAutoApproveAlternateTitle
            // 
            this.settingAutoApproveAlternateTitle.AutoSize = true;
            this.settingAutoApproveAlternateTitle.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.settingAutoApproveAlternateTitle.IgnoreSettingName = true;
            this.settingAutoApproveAlternateTitle.Location = new System.Drawing.Point(128, 48);
            this.settingAutoApproveAlternateTitle.Name = "settingAutoApproveAlternateTitle";
            this.settingAutoApproveAlternateTitle.Setting = null;
            this.settingAutoApproveAlternateTitle.Size = new System.Drawing.Size(184, 17);
            this.settingAutoApproveAlternateTitle.TabIndex = 15;
            this.settingAutoApproveAlternateTitle.Text = "Auto Approve on Alternate Titles";
            this.settingAutoApproveAlternateTitle.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(4, 223);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 26;
            this.label3.Text = "Artwork:";
            // 
            // coverDataSources
            // 
            this.coverDataSources.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.coverDataSources.Location = new System.Drawing.Point(290, 417);
            this.coverDataSources.Name = "coverDataSources";
            this.coverDataSources.Size = new System.Drawing.Size(131, 23);
            this.coverDataSources.TabIndex = 25;
            this.coverDataSources.Text = "Cover Art Data Sources";
            this.coverDataSources.UseVisualStyleBackColor = true;
            this.coverDataSources.Click += new System.EventHandler(this.coverDataSources_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(125, 225);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(254, 13);
            this.label4.TabIndex = 21;
            this.label4.Text = "Maximum number of covers to download per movie:";
            // 
            // backdropSourcesButton
            // 
            this.backdropSourcesButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.backdropSourcesButton.Location = new System.Drawing.Point(427, 417);
            this.backdropSourcesButton.Name = "backdropSourcesButton";
            this.backdropSourcesButton.Size = new System.Drawing.Size(139, 23);
            this.backdropSourcesButton.TabIndex = 24;
            this.backdropSourcesButton.Text = "Backdrop Data Sources";
            this.backdropSourcesButton.UseVisualStyleBackColor = true;
            this.backdropSourcesButton.Click += new System.EventHandler(this.backdropSourcesButton_Click);
            // 
            // coverFromMovieFolderCheckBox
            // 
            this.coverFromMovieFolderCheckBox.AutoSize = true;
            this.coverFromMovieFolderCheckBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.coverFromMovieFolderCheckBox.IgnoreSettingName = true;
            this.coverFromMovieFolderCheckBox.Location = new System.Drawing.Point(127, 257);
            this.coverFromMovieFolderCheckBox.Name = "coverFromMovieFolderCheckBox";
            this.coverFromMovieFolderCheckBox.Setting = null;
            this.coverFromMovieFolderCheckBox.Size = new System.Drawing.Size(229, 17);
            this.coverFromMovieFolderCheckBox.TabIndex = 17;
            this.coverFromMovieFolderCheckBox.Text = "Search movie folders for cover art named:";
            this.coverFromMovieFolderCheckBox.UseVisualStyleBackColor = true;
            this.coverFromMovieFolderCheckBox.CheckedChanged += new System.EventHandler(this.coverFromMovieFolderCheckBox_CheckedChanged);
            // 
            // coverCountTextBox
            // 
            this.coverCountTextBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.coverCountTextBox.Location = new System.Drawing.Point(397, 222);
            this.coverCountTextBox.Name = "coverCountTextBox";
            this.coverCountTextBox.Setting = null;
            this.coverCountTextBox.Size = new System.Drawing.Size(24, 21);
            this.coverCountTextBox.TabIndex = 20;
            // 
            // backdropFromMovieFolderCheckBox
            // 
            this.backdropFromMovieFolderCheckBox.AutoSize = true;
            this.backdropFromMovieFolderCheckBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.backdropFromMovieFolderCheckBox.IgnoreSettingName = true;
            this.backdropFromMovieFolderCheckBox.Location = new System.Drawing.Point(127, 283);
            this.backdropFromMovieFolderCheckBox.Name = "backdropFromMovieFolderCheckBox";
            this.backdropFromMovieFolderCheckBox.Setting = null;
            this.backdropFromMovieFolderCheckBox.Size = new System.Drawing.Size(234, 17);
            this.backdropFromMovieFolderCheckBox.TabIndex = 19;
            this.backdropFromMovieFolderCheckBox.Text = "Search movie folders for backdrops named:";
            this.backdropFromMovieFolderCheckBox.UseVisualStyleBackColor = true;
            this.backdropFromMovieFolderCheckBox.CheckedChanged += new System.EventHandler(this.backdropFromMovieFolderCheckBox_CheckedChanged);
            // 
            // coverPatternTextBox
            // 
            this.coverPatternTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.coverPatternTextBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.coverPatternTextBox.Location = new System.Drawing.Point(379, 255);
            this.coverPatternTextBox.Name = "coverPatternTextBox";
            this.coverPatternTextBox.Setting = null;
            this.coverPatternTextBox.Size = new System.Drawing.Size(207, 21);
            this.coverPatternTextBox.TabIndex = 16;
            // 
            // backdropPatternTextBox
            // 
            this.backdropPatternTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.backdropPatternTextBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.backdropPatternTextBox.Location = new System.Drawing.Point(379, 281);
            this.backdropPatternTextBox.Name = "backdropPatternTextBox";
            this.backdropPatternTextBox.Setting = null;
            this.backdropPatternTextBox.Size = new System.Drawing.Size(207, 21);
            this.backdropPatternTextBox.TabIndex = 18;
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Location = new System.Drawing.Point(120, 246);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(466, 3);
            this.groupBox5.TabIndex = 14;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "groupBox4";
            // 
            // groupBox7
            // 
            this.groupBox7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox7.Location = new System.Drawing.Point(8, 342);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(578, 3);
            this.groupBox7.TabIndex = 15;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "groupBox4";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(5, 358);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(85, 13);
            this.label5.TabIndex = 27;
            this.label5.Text = "Data Sources:";
            // 
            // groupBox6
            // 
            this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox6.Location = new System.Drawing.Point(8, 308);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(578, 3);
            this.groupBox6.TabIndex = 16;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "groupBox4";
            // 
            // autoRescanCheckBox
            // 
            this.autoRescanCheckBox.AutoSize = true;
            this.autoRescanCheckBox.IgnoreSettingName = true;
            this.autoRescanCheckBox.Location = new System.Drawing.Point(127, 317);
            this.autoRescanCheckBox.Name = "autoRescanCheckBox";
            this.autoRescanCheckBox.Setting = null;
            this.autoRescanCheckBox.Size = new System.Drawing.Size(256, 17);
            this.autoRescanCheckBox.TabIndex = 30;
            this.autoRescanCheckBox.Text = "Automatically rescan remote import paths every";
            this.autoRescanCheckBox.UseVisualStyleBackColor = true;
            this.autoRescanCheckBox.CheckedChanged += new System.EventHandler(this.autoRescanCheckBox_CheckedChanged);
            // 
            // rescanIntervalTextBox
            // 
            this.rescanIntervalTextBox.Location = new System.Drawing.Point(389, 315);
            this.rescanIntervalTextBox.Name = "rescanIntervalTextBox";
            this.rescanIntervalTextBox.Setting = null;
            this.rescanIntervalTextBox.Size = new System.Drawing.Size(32, 21);
            this.rescanIntervalTextBox.TabIndex = 31;
            // 
            // rescanHelpLinkLabel
            // 
            this.rescanHelpLinkLabel.AutoSize = true;
            this.rescanHelpLinkLabel.Location = new System.Drawing.Point(477, 318);
            this.rescanHelpLinkLabel.Name = "rescanHelpLinkLabel";
            this.rescanHelpLinkLabel.Size = new System.Drawing.Size(79, 13);
            this.rescanHelpLinkLabel.TabIndex = 33;
            this.rescanHelpLinkLabel.TabStop = true;
            this.rescanHelpLinkLabel.Text = "Do I need this?";
            this.rescanHelpLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.rescanHelpLinkLabel_LinkClicked);
            // 
            // autoDataSourcesPanel1
            // 
            this.autoDataSourcesPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.autoDataSourcesPanel1.AutoCommit = true;
            this.autoDataSourcesPanel1.Location = new System.Drawing.Point(128, 358);
            this.autoDataSourcesPanel1.Name = "autoDataSourcesPanel1";
            this.autoDataSourcesPanel1.Size = new System.Drawing.Size(458, 44);
            this.autoDataSourcesPanel1.TabIndex = 28;
            // 
            // ImporterSettingsPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.rescanHelpLinkLabel);
            this.Controls.Add(label7);
            this.Controls.Add(this.rescanIntervalTextBox);
            this.Controls.Add(this.autoRescanCheckBox);
            this.Controls.Add(label6);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.autoDataSourcesPanel1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.groupBox7);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.coverDataSources);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.backdropSourcesButton);
            this.Controls.Add(this.coverFromMovieFolderCheckBox);
            this.Controls.Add(this.coverCountTextBox);
            this.Controls.Add(this.backdropFromMovieFolderCheckBox);
            this.Controls.Add(this.coverPatternTextBox);
            this.Controls.Add(this.backdropPatternTextBox);
            this.Controls.Add(this.settingAutoApproveAlternateTitle);
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
            this.Controls.Add(this.nfoScannerCheckBox);
            this.Controls.Add(this.folderGroupingCheckBox);
            this.Controls.Add(this.preferFolderCheckBox);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(460, 250);
            this.Name = "ImporterSettingsPane";
            this.Size = new System.Drawing.Size(598, 449);
            this.Load += new System.EventHandler(this.ImporterSettingsPane_Load);
            ((System.ComponentModel.ISupportInitialize)(this.autoApproveTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TrackBar autoApproveTrackBar;
        private System.Windows.Forms.Label label1;
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
        private SettingCheckBox settingAutoApproveAlternateTitle;
        private Label label3;
        private Button coverDataSources;
        private Label label4;
        private Button backdropSourcesButton;
        private SettingCheckBox coverFromMovieFolderCheckBox;
        private SettingsTextBox coverCountTextBox;
        private SettingCheckBox backdropFromMovieFolderCheckBox;
        private SettingsTextBox coverPatternTextBox;
        private SettingsTextBox backdropPatternTextBox;
        private GroupBox groupBox5;
        private GroupBox groupBox7;
        private Label label5;
        private AutoDataSourcesPanel autoDataSourcesPanel1;
        private GroupBox groupBox6;
        private SettingCheckBox autoRescanCheckBox;
        private SettingsTextBox rescanIntervalTextBox;
        private LinkLabel rescanHelpLinkLabel;
    }
}
