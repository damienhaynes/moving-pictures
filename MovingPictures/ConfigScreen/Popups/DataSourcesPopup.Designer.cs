namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    partial class DataSourcesPopup {
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
            this.okButton = new System.Windows.Forms.Button();
            this.dataSourcePane1 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.DataSourcePane();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(363, 170);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // dataSourcePane1
            // 
            this.dataSourcePane1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataSourcePane1.Location = new System.Drawing.Point(12, 12);
            this.dataSourcePane1.Name = "dataSourcePane1";
            this.dataSourcePane1.Size = new System.Drawing.Size(426, 152);
            this.dataSourcePane1.TabIndex = 0;
            // 
            // DataSourcesPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 201);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.dataSourcePane1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DataSourcesPopup";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Data Sources";
            this.Load += new System.EventHandler(this.DataSourcesPopup_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private DataSourcePane dataSourcePane1;
        private System.Windows.Forms.Button okButton;
    }
}