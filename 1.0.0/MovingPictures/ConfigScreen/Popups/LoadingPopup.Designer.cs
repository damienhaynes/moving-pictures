namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    partial class LoadingPopup {
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
            this.splashPane1 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.SplashPane();
            this.SuspendLayout();
            // 
            // splashPane1
            // 
            this.splashPane1.BackColor = System.Drawing.Color.White;
            this.splashPane1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splashPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splashPane1.Location = new System.Drawing.Point(0, 0);
            this.splashPane1.Name = "splashPane1";
            this.splashPane1.Progress = 0;
            this.splashPane1.ShowProgressComponents = false;
            this.splashPane1.Size = new System.Drawing.Size(511, 112);
            this.splashPane1.Status = "Initializing...";
            this.splashPane1.TabIndex = 0;
            // 
            // LoadingPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(511, 112);
            this.Controls.Add(this.splashPane1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "LoadingPopup";
            this.ShowInTaskbar = false;
            this.Text = "LoadingPopup";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.LoadingPopup_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private SplashPane splashPane1;
    }
}