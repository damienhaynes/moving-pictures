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
            this.dataSourcePane1 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.DataSourcePane();
            this.SuspendLayout();
            // 
            // dataSourcePane1
            // 
            this.dataSourcePane1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataSourcePane1.Location = new System.Drawing.Point(0, 0);
            this.dataSourcePane1.Name = "dataSourcePane1";
            this.dataSourcePane1.Size = new System.Drawing.Size(389, 142);
            this.dataSourcePane1.TabIndex = 0;
            // 
            // DataSourcesPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(389, 142);
            this.Controls.Add(this.dataSourcePane1);
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
    }
}