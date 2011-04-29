namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    partial class AdvancedSettingsPopup {
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
            this.advancedSettingsWarningPane1 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.AdvancedSettingsWarningPane();
            this.SuspendLayout();
            // 
            // advancedSettingsWarningPane1
            // 
            this.advancedSettingsWarningPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.advancedSettingsWarningPane1.Location = new System.Drawing.Point(0, 0);
            this.advancedSettingsWarningPane1.Name = "advancedSettingsWarningPane1";
            this.advancedSettingsWarningPane1.Size = new System.Drawing.Size(395, 440);
            this.advancedSettingsWarningPane1.TabIndex = 0;
            // 
            // AdvancedSettingsPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(395, 440);
            this.Controls.Add(this.advancedSettingsWarningPane1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AdvancedSettingsPopup";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Advanced Settings";
            this.Load += new System.EventHandler(this.AdvancedSettingsPopup_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private AdvancedSettingsWarningPane advancedSettingsWarningPane1;
    }
}