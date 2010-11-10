namespace MovingPicturesSocialAPI.UI {
    partial class LoginForm {
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
            this.welcomePanel = new MovingPicturesSocialAPI.UI.Panels.LoginWelcomePanel();
            this.splashPanel1 = new MovingPicturesSocialAPI.UI.Panels.SplashPanel();
            this.SuspendLayout();
            // 
            // welcomePanel
            // 
            this.welcomePanel.Location = new System.Drawing.Point(10, 100);
            this.welcomePanel.Name = "welcomePanel";
            this.welcomePanel.Size = new System.Drawing.Size(292, 171);
            this.welcomePanel.TabIndex = 5;
            // 
            // splashPanel1
            // 
            this.splashPanel1.Location = new System.Drawing.Point(16, 13);
            this.splashPanel1.Name = "splashPanel1";
            this.splashPanel1.Size = new System.Drawing.Size(257, 78);
            this.splashPanel1.TabIndex = 4;
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(313, 301);
            this.Controls.Add(this.welcomePanel);
            this.Controls.Add(this.splashPanel1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.ShowIcon = false;
            this.Text = "Moving Pictures Social";
            this.ResumeLayout(false);

        }

        #endregion

        private Panels.SplashPanel splashPanel1;
        private Panels.LoginWelcomePanel welcomePanel;

    }
}