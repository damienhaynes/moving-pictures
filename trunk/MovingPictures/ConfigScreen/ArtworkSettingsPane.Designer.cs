namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    partial class ArtworkSettingsPane {
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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.coverDataSources = new System.Windows.Forms.Button();
            this.backdropSourcesButton = new System.Windows.Forms.Button();
            this.coverCountTextBox = new Cornerstone.GUI.Controls.SettingsTextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.backdropPatternTextBox = new Cornerstone.GUI.Controls.SettingsTextBox();
            this.coverPatternTextBox = new Cornerstone.GUI.Controls.SettingsTextBox();
            this.backdropFromMovieFolderCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.helpButton1 = new System.Windows.Forms.Button();
            this.coverFromMovieFolderCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.groupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox
            // 
            this.groupBox.Controls.Add(this.groupBox2);
            this.groupBox.Controls.Add(this.coverDataSources);
            this.groupBox.Controls.Add(this.backdropSourcesButton);
            this.groupBox.Controls.Add(this.coverCountTextBox);
            this.groupBox.Controls.Add(this.groupBox1);
            this.groupBox.Controls.Add(this.label1);
            this.groupBox.Controls.Add(this.backdropPatternTextBox);
            this.groupBox.Controls.Add(this.coverPatternTextBox);
            this.groupBox.Controls.Add(this.backdropFromMovieFolderCheckBox);
            this.groupBox.Controls.Add(this.helpButton1);
            this.groupBox.Controls.Add(this.coverFromMovieFolderCheckBox);
            this.groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox.Location = new System.Drawing.Point(0, 0);
            this.groupBox.MinimumSize = new System.Drawing.Size(400, 147);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(576, 147);
            this.groupBox.TabIndex = 0;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "Artwork";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Location = new System.Drawing.Point(12, 104);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(552, 3);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "groupBox2";
            // 
            // coverDataSources
            // 
            this.coverDataSources.Location = new System.Drawing.Point(6, 113);
            this.coverDataSources.Name = "coverDataSources";
            this.coverDataSources.Size = new System.Drawing.Size(131, 23);
            this.coverDataSources.TabIndex = 9;
            this.coverDataSources.Text = "Cover Art Data Sources";
            this.coverDataSources.UseVisualStyleBackColor = true;
            this.coverDataSources.Click += new System.EventHandler(this.coverDataSources_Click);
            // 
            // backdropSourcesButton
            // 
            this.backdropSourcesButton.Location = new System.Drawing.Point(143, 113);
            this.backdropSourcesButton.Name = "backdropSourcesButton";
            this.backdropSourcesButton.Size = new System.Drawing.Size(139, 23);
            this.backdropSourcesButton.TabIndex = 8;
            this.backdropSourcesButton.Text = "Backdrop Data Sources";
            this.backdropSourcesButton.UseVisualStyleBackColor = true;
            this.backdropSourcesButton.Click += new System.EventHandler(this.backdropSourcesButton_Click);
            // 
            // coverCountTextBox
            // 
            this.coverCountTextBox.Location = new System.Drawing.Point(275, 17);
            this.coverCountTextBox.Name = "coverCountTextBox";
            this.coverCountTextBox.Setting = null;
            this.coverCountTextBox.Size = new System.Drawing.Size(24, 20);
            this.coverCountTextBox.TabIndex = 5;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Location = new System.Drawing.Point(12, 43);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(552, 3);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(255, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Maximum Number of Covers to Download per Movie:";
            // 
            // backdropPatternTextBox
            // 
            this.backdropPatternTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.backdropPatternTextBox.Location = new System.Drawing.Point(243, 78);
            this.backdropPatternTextBox.Name = "backdropPatternTextBox";
            this.backdropPatternTextBox.Setting = null;
            this.backdropPatternTextBox.Size = new System.Drawing.Size(327, 20);
            this.backdropPatternTextBox.TabIndex = 3;
            // 
            // coverPatternTextBox
            // 
            this.coverPatternTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.coverPatternTextBox.Location = new System.Drawing.Point(243, 52);
            this.coverPatternTextBox.Name = "coverPatternTextBox";
            this.coverPatternTextBox.Setting = null;
            this.coverPatternTextBox.Size = new System.Drawing.Size(327, 20);
            this.coverPatternTextBox.TabIndex = 0;
            // 
            // backdropFromMovieFolderCheckBox
            // 
            this.backdropFromMovieFolderCheckBox.AutoSize = true;
            this.backdropFromMovieFolderCheckBox.IgnoreSettingName = false;
            this.backdropFromMovieFolderCheckBox.Location = new System.Drawing.Point(6, 80);
            this.backdropFromMovieFolderCheckBox.Name = "backdropFromMovieFolderCheckBox";
            this.backdropFromMovieFolderCheckBox.Setting = null;
            this.backdropFromMovieFolderCheckBox.Size = new System.Drawing.Size(231, 17);
            this.backdropFromMovieFolderCheckBox.TabIndex = 4;
            this.backdropFromMovieFolderCheckBox.Text = "Search movie folders for backdrops named:";
            this.backdropFromMovieFolderCheckBox.UseVisualStyleBackColor = true;
            this.backdropFromMovieFolderCheckBox.CheckedChanged += new System.EventHandler(this.backdropFromMovieFolderCheckBox_CheckedChanged);
            // 
            // helpButton1
            // 
            this.helpButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.helpButton1.BackgroundImage = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.help;
            this.helpButton1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.helpButton1.Location = new System.Drawing.Point(547, 13);
            this.helpButton1.Margin = new System.Windows.Forms.Padding(0);
            this.helpButton1.Name = "helpButton1";
            this.helpButton1.Size = new System.Drawing.Size(23, 23);
            this.helpButton1.TabIndex = 2;
            this.helpButton1.UseVisualStyleBackColor = true;
            // 
            // coverFromMovieFolderCheckBox
            // 
            this.coverFromMovieFolderCheckBox.AutoSize = true;
            this.coverFromMovieFolderCheckBox.IgnoreSettingName = false;
            this.coverFromMovieFolderCheckBox.Location = new System.Drawing.Point(6, 54);
            this.coverFromMovieFolderCheckBox.Name = "coverFromMovieFolderCheckBox";
            this.coverFromMovieFolderCheckBox.Setting = null;
            this.coverFromMovieFolderCheckBox.Size = new System.Drawing.Size(223, 17);
            this.coverFromMovieFolderCheckBox.TabIndex = 1;
            this.coverFromMovieFolderCheckBox.Text = "Search movie folders for cover art named:";
            this.coverFromMovieFolderCheckBox.UseVisualStyleBackColor = true;
            this.coverFromMovieFolderCheckBox.CheckedChanged += new System.EventHandler(this.coverFromMovieFolderCheckBox_CheckedChanged);
            // 
            // ArtworkSettingsPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox);
            this.MinimumSize = new System.Drawing.Size(400, 147);
            this.Name = "ArtworkSettingsPane";
            this.Size = new System.Drawing.Size(576, 147);
            this.Load += new System.EventHandler(this.ArtworkSettingsPane_Load);
            this.groupBox.ResumeLayout(false);
            this.groupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox;
        private Cornerstone.GUI.Controls.SettingCheckBox coverFromMovieFolderCheckBox;
        private Cornerstone.GUI.Controls.SettingsTextBox coverPatternTextBox;
        private System.Windows.Forms.Button helpButton1;
        private Cornerstone.GUI.Controls.SettingCheckBox backdropFromMovieFolderCheckBox;
        private Cornerstone.GUI.Controls.SettingsTextBox backdropPatternTextBox;
        private System.Windows.Forms.Label label1;
        private Cornerstone.GUI.Controls.SettingsTextBox coverCountTextBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button coverDataSources;
        private System.Windows.Forms.Button backdropSourcesButton;
    }
}
