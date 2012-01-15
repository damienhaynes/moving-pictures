namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    partial class DataProviderSetupPopup {
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
            this.autoDataSourcesPanel1 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.AutoDataSourcesPanel();
            this.okButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // autoDataSourcesPanel1
            // 
            this.autoDataSourcesPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.autoDataSourcesPanel1.AutoCommit = true;
            this.autoDataSourcesPanel1.Location = new System.Drawing.Point(13, 13);
            this.autoDataSourcesPanel1.Name = "autoDataSourcesPanel1";
            this.autoDataSourcesPanel1.Size = new System.Drawing.Size(433, 44);
            this.autoDataSourcesPanel1.TabIndex = 0;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(371, 63);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // DataProviderSetupPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(458, 96);
            this.ControlBox = false;
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.autoDataSourcesPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "DataProviderSetupPopup";
            this.ShowInTaskbar = false;
            this.Text = "Language Configuration";
            this.Load += new System.EventHandler(this.DataProviderSetupPopup_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private AutoDataSourcesPanel autoDataSourcesPanel1;
        private System.Windows.Forms.Button okButton;

    }
}