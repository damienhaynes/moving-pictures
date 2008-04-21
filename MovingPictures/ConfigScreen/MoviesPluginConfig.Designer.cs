using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;
namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    partial class MoviesPluginConfig {
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
            this.importSettingsTab = new System.Windows.Forms.TabPage();
            this.movieImporterPane1 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.MovieImporterPane();
            this.importPathsListView1 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.ImportPathsPane();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.advancedSettingsPane = new MediaPortal.Plugins.MovingPictures.ConfigScreen.AdvancedSettingsPane();
            this.userManagementPane1 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.UserManagementPane();
            this.mainTabControl.SuspendLayout();
            this.importSettingsTab.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTabControl
            // 
            this.mainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTabControl.Controls.Add(this.importSettingsTab);
            this.mainTabControl.Controls.Add(this.tabPage2);
            this.mainTabControl.Location = new System.Drawing.Point(12, 12);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new System.Drawing.Size(570, 551);
            this.mainTabControl.TabIndex = 0;
            // 
            // importSettingsTab
            // 
            this.importSettingsTab.Controls.Add(this.movieImporterPane1);
            this.importSettingsTab.Controls.Add(this.importPathsListView1);
            this.importSettingsTab.Location = new System.Drawing.Point(4, 22);
            this.importSettingsTab.Name = "importSettingsTab";
            this.importSettingsTab.Padding = new System.Windows.Forms.Padding(3);
            this.importSettingsTab.Size = new System.Drawing.Size(562, 525);
            this.importSettingsTab.TabIndex = 0;
            this.importSettingsTab.Text = "Import Settings";
            this.importSettingsTab.UseVisualStyleBackColor = true;
            // 
            // movieImporterPane1
            // 
            this.movieImporterPane1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.movieImporterPane1.Location = new System.Drawing.Point(3, 200);
            this.movieImporterPane1.MinimumSize = new System.Drawing.Size(422, 250);
            this.movieImporterPane1.Name = "movieImporterPane1";
            this.movieImporterPane1.Size = new System.Drawing.Size(556, 319);
            this.movieImporterPane1.TabIndex = 1;
            // 
            // importPathsListView1
            // 
            this.importPathsListView1.Dock = System.Windows.Forms.DockStyle.Top;
            this.importPathsListView1.Location = new System.Drawing.Point(3, 3);
            this.importPathsListView1.MinimumSize = new System.Drawing.Size(524, 150);
            this.importPathsListView1.Name = "importPathsListView1";
            this.importPathsListView1.Size = new System.Drawing.Size(556, 191);
            this.importPathsListView1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.advancedSettingsPane);
            this.tabPage2.Controls.Add(this.userManagementPane1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(562, 525);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Additional Settings";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // allSettingsPane1
            // 
            this.advancedSettingsPane.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.advancedSettingsPane.Location = new System.Drawing.Point(8, 161);
            this.advancedSettingsPane.Name = "allSettingsPane1";
            this.advancedSettingsPane.Size = new System.Drawing.Size(548, 347);
            this.advancedSettingsPane.TabIndex = 1;
            // 
            // userManagementPane1
            // 
            this.userManagementPane1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.userManagementPane1.Location = new System.Drawing.Point(8, 6);
            this.userManagementPane1.Name = "userManagementPane1";
            this.userManagementPane1.Size = new System.Drawing.Size(548, 149);
            this.userManagementPane1.TabIndex = 0;
            // 
            // MoviesPluginConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(594, 575);
            this.Controls.Add(this.mainTabControl);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "MoviesPluginConfig";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Movies Plugin Config";
            this.mainTabControl.ResumeLayout(false);
            this.importSettingsTab.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage importSettingsTab;
        private System.Windows.Forms.TabPage tabPage2;
        private ImportPathsPane importPathsListView1;
        private MovieImporterPane movieImporterPane1;
        private UserManagementPane userManagementPane1;
        private AdvancedSettingsPane advancedSettingsPane;
    }
}