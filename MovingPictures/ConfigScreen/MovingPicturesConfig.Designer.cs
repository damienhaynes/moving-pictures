using Cornerstone.GUI;
namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    partial class MovingPicturesConfig {
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
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.managerTab = new System.Windows.Forms.TabPage();
            this.movieManagerPane1 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.MovieManagerPane();
            this.importSettingsTab = new System.Windows.Forms.TabPage();
            this.movieImporterPane1 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.MovieImporterPane();
            this.importPathsListView1 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.ImportPathsPane();
            this.settingsTab = new System.Windows.Forms.TabPage();
            this.advancedSettingsWarningPane1 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.AdvancedSettingsWarningPane();
            this.mainTabControl.SuspendLayout();
            this.managerTab.SuspendLayout();
            this.importSettingsTab.SuspendLayout();
            this.settingsTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTabControl
            // 
            this.mainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTabControl.Controls.Add(this.managerTab);
            this.mainTabControl.Controls.Add(this.importSettingsTab);
            this.mainTabControl.Controls.Add(this.settingsTab);
            this.mainTabControl.Location = new System.Drawing.Point(12, 12);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new System.Drawing.Size(568, 585);
            this.mainTabControl.TabIndex = 0;
            // 
            // managerTab
            // 
            this.managerTab.Controls.Add(this.movieManagerPane1);
            this.managerTab.Location = new System.Drawing.Point(4, 22);
            this.managerTab.Name = "managerTab";
            this.managerTab.Padding = new System.Windows.Forms.Padding(3);
            this.managerTab.Size = new System.Drawing.Size(560, 559);
            this.managerTab.TabIndex = 2;
            this.managerTab.Text = "Movie Manager";
            this.managerTab.UseVisualStyleBackColor = true;
            // 
            // movieManagerPane1
            // 
            this.movieManagerPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.movieManagerPane1.Location = new System.Drawing.Point(3, 3);
            this.movieManagerPane1.Name = "movieManagerPane1";
            this.movieManagerPane1.Size = new System.Drawing.Size(554, 553);
            this.movieManagerPane1.TabIndex = 0;
            // 
            // importSettingsTab
            // 
            this.importSettingsTab.Controls.Add(this.movieImporterPane1);
            this.importSettingsTab.Controls.Add(this.importPathsListView1);
            this.importSettingsTab.Location = new System.Drawing.Point(4, 22);
            this.importSettingsTab.Name = "importSettingsTab";
            this.importSettingsTab.Padding = new System.Windows.Forms.Padding(3);
            this.importSettingsTab.Size = new System.Drawing.Size(560, 559);
            this.importSettingsTab.TabIndex = 0;
            this.importSettingsTab.Text = "Movie Importer";
            this.importSettingsTab.UseVisualStyleBackColor = true;
            // 
            // movieImporterPane1
            // 
            this.movieImporterPane1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.movieImporterPane1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.movieImporterPane1.Location = new System.Drawing.Point(6, 190);
            this.movieImporterPane1.MinimumSize = new System.Drawing.Size(422, 250);
            this.movieImporterPane1.Name = "movieImporterPane1";
            this.movieImporterPane1.Size = new System.Drawing.Size(548, 349);
            this.movieImporterPane1.TabIndex = 1;
            // 
            // importPathsListView1
            // 
            this.importPathsListView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.importPathsListView1.Location = new System.Drawing.Point(6, 15);
            this.importPathsListView1.MinimumSize = new System.Drawing.Size(524, 150);
            this.importPathsListView1.Name = "importPathsListView1";
            this.importPathsListView1.Size = new System.Drawing.Size(548, 157);
            this.importPathsListView1.TabIndex = 0;
            // 
            // settingsTab
            // 
            this.settingsTab.Controls.Add(this.advancedSettingsWarningPane1);
            this.settingsTab.Location = new System.Drawing.Point(4, 22);
            this.settingsTab.Name = "settingsTab";
            this.settingsTab.Padding = new System.Windows.Forms.Padding(3);
            this.settingsTab.Size = new System.Drawing.Size(560, 559);
            this.settingsTab.TabIndex = 1;
            this.settingsTab.Text = "Advanced Settings";
            this.settingsTab.UseVisualStyleBackColor = true;
            // 
            // advancedSettingsWarningPane1
            // 
            this.advancedSettingsWarningPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.advancedSettingsWarningPane1.Location = new System.Drawing.Point(3, 3);
            this.advancedSettingsWarningPane1.Name = "advancedSettingsWarningPane1";
            this.advancedSettingsWarningPane1.Size = new System.Drawing.Size(554, 553);
            this.advancedSettingsWarningPane1.TabIndex = 0;
            // 
            // MovingPicturesConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(592, 609);
            this.Controls.Add(this.mainTabControl);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "MovingPicturesConfig";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Moving Pictures Configuration";
            this.Load += new System.EventHandler(this.MoviesPluginConfig_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MovingPicturesConfig_FormClosing);
            this.mainTabControl.ResumeLayout(false);
            this.managerTab.ResumeLayout(false);
            this.importSettingsTab.ResumeLayout(false);
            this.settingsTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage importSettingsTab;
        private System.Windows.Forms.TabPage settingsTab;
        private ImportPathsPane importPathsListView1;
        private MovieImporterPane movieImporterPane1;
        private System.Windows.Forms.TabPage managerTab;
        private MovieManagerPane movieManagerPane1;
        private AdvancedSettingsWarningPane advancedSettingsWarningPane1;
    }
}