namespace MovingPicturesSocialAPI.UI.Panels {
    partial class LoginNewUserPanel {
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
            this.backButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.usernameTextBox = new System.Windows.Forms.TextBox();
            this.emailTextBox = new System.Windows.Forms.TextBox();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.verifyPasswordTextBox = new System.Windows.Forms.TextBox();
            this.privateCheckBox = new System.Windows.Forms.CheckBox();
            this.statusLabel = new System.Windows.Forms.Label();
            this.createAccountOkButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // backButton
            // 
            this.backButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.backButton.Location = new System.Drawing.Point(115, 225);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(75, 23);
            this.backButton.TabIndex = 6;
            this.backButton.Text = "Back";
            this.backButton.UseVisualStyleBackColor = true;
            this.backButton.Click += new System.EventHandler(this.backButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Username:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Email:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Password:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 127);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(85, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Verify Password:";
            // 
            // usernameTextBox
            // 
            this.usernameTextBox.Location = new System.Drawing.Point(0, 16);
            this.usernameTextBox.Name = "usernameTextBox";
            this.usernameTextBox.Size = new System.Drawing.Size(289, 20);
            this.usernameTextBox.TabIndex = 1;
            this.usernameTextBox.TextChanged += new System.EventHandler(this.usernameTextBox_TextChanged);
            this.usernameTextBox.Leave += new System.EventHandler(this.usernameTextBox_Leave);
            // 
            // emailTextBox
            // 
            this.emailTextBox.Location = new System.Drawing.Point(0, 55);
            this.emailTextBox.Name = "emailTextBox";
            this.emailTextBox.Size = new System.Drawing.Size(289, 20);
            this.emailTextBox.TabIndex = 2;
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(0, 104);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(289, 20);
            this.passwordTextBox.TabIndex = 3;
            this.passwordTextBox.UseSystemPasswordChar = true;
            this.passwordTextBox.TextChanged += new System.EventHandler(this.passwordTextBox_TextChanged);
            this.passwordTextBox.Enter += new System.EventHandler(this.passwordTextBox_FocusChanged);
            this.passwordTextBox.Leave += new System.EventHandler(this.passwordTextBox_FocusChanged);
            // 
            // verifyPasswordTextBox
            // 
            this.verifyPasswordTextBox.Location = new System.Drawing.Point(0, 143);
            this.verifyPasswordTextBox.Name = "verifyPasswordTextBox";
            this.verifyPasswordTextBox.Size = new System.Drawing.Size(289, 20);
            this.verifyPasswordTextBox.TabIndex = 4;
            this.verifyPasswordTextBox.UseSystemPasswordChar = true;
            this.verifyPasswordTextBox.TextChanged += new System.EventHandler(this.verifyPasswordTextBox_TextChanged);
            this.verifyPasswordTextBox.Enter += new System.EventHandler(this.verifyPasswordTextBox_FocusChanged);
            this.verifyPasswordTextBox.Leave += new System.EventHandler(this.verifyPasswordTextBox_FocusChanged);
            // 
            // privateCheckBox
            // 
            this.privateCheckBox.AutoSize = true;
            this.privateCheckBox.Location = new System.Drawing.Point(168, 178);
            this.privateCheckBox.Name = "privateCheckBox";
            this.privateCheckBox.Size = new System.Drawing.Size(121, 17);
            this.privateCheckBox.TabIndex = 5;
            this.privateCheckBox.Text = "Make Profile Private";
            this.privateCheckBox.UseVisualStyleBackColor = true;
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.statusLabel.Location = new System.Drawing.Point(172, 0);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(111, 13);
            this.statusLabel.TabIndex = 10;
            this.statusLabel.Text = "Registering account...";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.statusLabel.Visible = false;
            // 
            // createAccountOkButton
            // 
            this.createAccountOkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.createAccountOkButton.Enabled = false;
            this.createAccountOkButton.Location = new System.Drawing.Point(196, 225);
            this.createAccountOkButton.Name = "createAccountOkButton";
            this.createAccountOkButton.Size = new System.Drawing.Size(93, 23);
            this.createAccountOkButton.TabIndex = 7;
            this.createAccountOkButton.Text = "Create Account";
            this.createAccountOkButton.UseVisualStyleBackColor = true;
            this.createAccountOkButton.Click += new System.EventHandler(this.createAccountOkButton_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.statusLabel);
            this.panel1.Location = new System.Drawing.Point(6, 204);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(283, 18);
            this.panel1.TabIndex = 12;
            // 
            // LoginNewUserPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.createAccountOkButton);
            this.Controls.Add(this.privateCheckBox);
            this.Controls.Add(this.verifyPasswordTextBox);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.emailTextBox);
            this.Controls.Add(this.usernameTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.backButton);
            this.Name = "LoginNewUserPanel";
            this.Size = new System.Drawing.Size(292, 251);
            this.Load += new System.EventHandler(this.LoginNewUserPanel_Load);
            this.VisibleChanged += new System.EventHandler(this.LoginNewUserPanel_VisibleChanged);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button backButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox usernameTextBox;
        private System.Windows.Forms.TextBox emailTextBox;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.TextBox verifyPasswordTextBox;
        private System.Windows.Forms.CheckBox privateCheckBox;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Button createAccountOkButton;
        private System.Windows.Forms.Panel panel1;
    }
}
