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
            this.coverDataSources = new System.Windows.Forms.Button();
            this.backdropSourcesButton = new System.Windows.Forms.Button();
            this.coverCountTextBox = new Cornerstone.GUI.Controls.SettingsTextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.backdropPatternTextBox = new Cornerstone.GUI.Controls.SettingsTextBox();
            this.coverPatternTextBox = new Cornerstone.GUI.Controls.SettingsTextBox();
            this.backdropFromMovieFolderCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.coverFromMovieFolderCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.SuspendLayout();
            // 
            // coverDataSources
            // 
            this.coverDataSources.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.coverDataSources.Location = new System.Drawing.Point(127, 97);
            this.coverDataSources.Name = "coverDataSources";
            this.coverDataSources.Size = new System.Drawing.Size(131, 23);
            this.coverDataSources.TabIndex = 9;
            this.coverDataSources.Text = "Cover Art Data Sources";
            this.coverDataSources.UseVisualStyleBackColor = true;
            this.coverDataSources.Click += new System.EventHandler(this.coverDataSources_Click);
            // 
            // backdropSourcesButton
            // 
            this.backdropSourcesButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.backdropSourcesButton.Location = new System.Drawing.Point(264, 97);
            this.backdropSourcesButton.Name = "backdropSourcesButton";
            this.backdropSourcesButton.Size = new System.Drawing.Size(139, 23);
            this.backdropSourcesButton.TabIndex = 8;
            this.backdropSourcesButton.Text = "Backdrop Data Sources";
            this.backdropSourcesButton.UseVisualStyleBackColor = true;
            this.backdropSourcesButton.Click += new System.EventHandler(this.backdropSourcesButton_Click);
            // 
            // coverCountTextBox
            // 
            this.coverCountTextBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.coverCountTextBox.Location = new System.Drawing.Point(397, 3);
            this.coverCountTextBox.Name = "coverCountTextBox";
            this.coverCountTextBox.Setting = null;
            this.coverCountTextBox.Size = new System.Drawing.Size(24, 21);
            this.coverCountTextBox.TabIndex = 5;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Location = new System.Drawing.Point(120, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(424, 3);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(125, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(254, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Maximum number of covers to download per movie:";
            // 
            // backdropPatternTextBox
            // 
            this.backdropPatternTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.backdropPatternTextBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.backdropPatternTextBox.Location = new System.Drawing.Point(379, 62);
            this.backdropPatternTextBox.Name = "backdropPatternTextBox";
            this.backdropPatternTextBox.Setting = null;
            this.backdropPatternTextBox.Size = new System.Drawing.Size(163, 21);
            this.backdropPatternTextBox.TabIndex = 3;
            // 
            // coverPatternTextBox
            // 
            this.coverPatternTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.coverPatternTextBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.coverPatternTextBox.Location = new System.Drawing.Point(379, 36);
            this.coverPatternTextBox.Name = "coverPatternTextBox";
            this.coverPatternTextBox.Setting = null;
            this.coverPatternTextBox.Size = new System.Drawing.Size(163, 21);
            this.coverPatternTextBox.TabIndex = 0;
            // 
            // backdropFromMovieFolderCheckBox
            // 
            this.backdropFromMovieFolderCheckBox.AutoSize = true;
            this.backdropFromMovieFolderCheckBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.backdropFromMovieFolderCheckBox.IgnoreSettingName = false;
            this.backdropFromMovieFolderCheckBox.Location = new System.Drawing.Point(127, 64);
            this.backdropFromMovieFolderCheckBox.Name = "backdropFromMovieFolderCheckBox";
            this.backdropFromMovieFolderCheckBox.Setting = null;
            this.backdropFromMovieFolderCheckBox.Size = new System.Drawing.Size(234, 17);
            this.backdropFromMovieFolderCheckBox.TabIndex = 4;
            this.backdropFromMovieFolderCheckBox.Text = "Search movie folders for backdrops named:";
            this.backdropFromMovieFolderCheckBox.UseVisualStyleBackColor = true;
            this.backdropFromMovieFolderCheckBox.CheckedChanged += new System.EventHandler(this.backdropFromMovieFolderCheckBox_CheckedChanged);
            // 
            // coverFromMovieFolderCheckBox
            // 
            this.coverFromMovieFolderCheckBox.AutoSize = true;
            this.coverFromMovieFolderCheckBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.coverFromMovieFolderCheckBox.IgnoreSettingName = false;
            this.coverFromMovieFolderCheckBox.Location = new System.Drawing.Point(127, 38);
            this.coverFromMovieFolderCheckBox.Name = "coverFromMovieFolderCheckBox";
            this.coverFromMovieFolderCheckBox.Setting = null;
            this.coverFromMovieFolderCheckBox.Size = new System.Drawing.Size(229, 17);
            this.coverFromMovieFolderCheckBox.TabIndex = 1;
            this.coverFromMovieFolderCheckBox.Text = "Search movie folders for cover art named:";
            this.coverFromMovieFolderCheckBox.UseVisualStyleBackColor = true;
            this.coverFromMovieFolderCheckBox.CheckedChanged += new System.EventHandler(this.coverFromMovieFolderCheckBox_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(4, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Artwork:";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Location = new System.Drawing.Point(120, 88);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(424, 3);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "groupBox3";
            // 
            // ArtworkSettingsPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.coverDataSources);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.backdropSourcesButton);
            this.Controls.Add(this.coverFromMovieFolderCheckBox);
            this.Controls.Add(this.coverCountTextBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.backdropFromMovieFolderCheckBox);
            this.Controls.Add(this.coverPatternTextBox);
            this.Controls.Add(this.backdropPatternTextBox);
            this.MinimumSize = new System.Drawing.Size(550, 130);
            this.Name = "ArtworkSettingsPane";
            this.Size = new System.Drawing.Size(555, 130);
            this.Load += new System.EventHandler(this.ArtworkSettingsPane_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Cornerstone.GUI.Controls.SettingCheckBox coverFromMovieFolderCheckBox;
        private Cornerstone.GUI.Controls.SettingsTextBox coverPatternTextBox;
        private Cornerstone.GUI.Controls.SettingCheckBox backdropFromMovieFolderCheckBox;
        private Cornerstone.GUI.Controls.SettingsTextBox backdropPatternTextBox;
        private System.Windows.Forms.Label label1;
        private Cornerstone.GUI.Controls.SettingsTextBox coverCountTextBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button coverDataSources;
        private System.Windows.Forms.Button backdropSourcesButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox3;
    }
}
