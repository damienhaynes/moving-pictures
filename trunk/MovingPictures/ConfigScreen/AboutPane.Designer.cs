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
            this.SuspendLayout();
            // 
            // advancedSettingsButton
            // 
            this.advancedSettingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.advancedSettingsButton.Location = new System.Drawing.Point(167, 218);
            this.advancedSettingsButton.Name = "advancedSettingsButton";
            this.advancedSettingsButton.Size = new System.Drawing.Size(122, 23);
            this.advancedSettingsButton.TabIndex = 0;
            this.advancedSettingsButton.Text = "Advanced Settings";
            this.advancedSettingsButton.UseVisualStyleBackColor = true;
            this.advancedSettingsButton.Click += new System.EventHandler(this.advancedSettingsButton_Click);
            // 
            // AboutPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.advancedSettingsButton);
            this.Name = "AboutPane";
            this.Size = new System.Drawing.Size(304, 257);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button advancedSettingsButton;
    }
}
