namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    partial class SocialPane {
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
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabStartWizard = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.btnStartNext = new System.Windows.Forms.Button();
            this.radStartLink = new System.Windows.Forms.RadioButton();
            this.radStartCreate = new System.Windows.Forms.RadioButton();
            this.tabCreateAccount = new System.Windows.Forms.TabPage();
            this.btnRegisterBack = new System.Windows.Forms.Button();
            this.txtRegisterEmail = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.btnRegisterNext = new System.Windows.Forms.Button();
            this.chkRegisterPrivateProfile = new System.Windows.Forms.CheckBox();
            this.txtRegisterConfirmPassword = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.txtRegisterPassword = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtRegisterUsername = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.tabLinkAccount = new System.Windows.Forms.TabPage();
            this.btnLinkBack = new System.Windows.Forms.Button();
            this.btnLinkNext = new System.Windows.Forms.Button();
            this.txtLinkPassword = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtLinkUsername = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tabAlreadyLinked = new System.Windows.Forms.TabPage();
            this.btnLinkedRemove = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnLinkedSyncNow = new System.Windows.Forms.Button();
            this.linkLinkedUsername = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.tabControl1.SuspendLayout();
            this.tabStartWizard.SuspendLayout();
            this.tabCreateAccount.SuspendLayout();
            this.tabLinkAccount.SuspendLayout();
            this.tabAlreadyLinked.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabStartWizard);
            this.tabControl1.Controls.Add(this.tabCreateAccount);
            this.tabControl1.Controls.Add(this.tabLinkAccount);
            this.tabControl1.Controls.Add(this.tabAlreadyLinked);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(528, 535);
            this.tabControl1.TabIndex = 4;
            this.tabControl1.TabStop = false;
            // 
            // tabStartWizard
            // 
            this.tabStartWizard.Controls.Add(this.label2);
            this.tabStartWizard.Controls.Add(this.btnStartNext);
            this.tabStartWizard.Controls.Add(this.radStartLink);
            this.tabStartWizard.Controls.Add(this.radStartCreate);
            this.tabStartWizard.Location = new System.Drawing.Point(4, 22);
            this.tabStartWizard.Name = "tabStartWizard";
            this.tabStartWizard.Size = new System.Drawing.Size(520, 509);
            this.tabStartWizard.TabIndex = 3;
            this.tabStartWizard.Text = "tabStartWizard";
            this.tabStartWizard.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(409, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Moving Pictures Social is an online community which you can link to Moving Pictur" +
                "es.";
            // 
            // btnStartNext
            // 
            this.btnStartNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartNext.AutoSize = true;
            this.btnStartNext.Location = new System.Drawing.Point(439, 222);
            this.btnStartNext.Name = "btnStartNext";
            this.btnStartNext.Size = new System.Drawing.Size(75, 23);
            this.btnStartNext.TabIndex = 2;
            this.btnStartNext.Text = "Next";
            this.btnStartNext.UseVisualStyleBackColor = true;
            this.btnStartNext.Click += new System.EventHandler(this.btnStartNext_Click);
            // 
            // radStartLink
            // 
            this.radStartLink.AutoSize = true;
            this.radStartLink.Location = new System.Drawing.Point(7, 97);
            this.radStartLink.Name = "radStartLink";
            this.radStartLink.Size = new System.Drawing.Size(257, 17);
            this.radStartLink.TabIndex = 1;
            this.radStartLink.Text = "I already have a Moving Pictures Social account.";
            this.radStartLink.UseVisualStyleBackColor = true;
            // 
            // radStartCreate
            // 
            this.radStartCreate.AutoSize = true;
            this.radStartCreate.Checked = true;
            this.radStartCreate.Location = new System.Drawing.Point(7, 74);
            this.radStartCreate.Name = "radStartCreate";
            this.radStartCreate.Size = new System.Drawing.Size(346, 17);
            this.radStartCreate.TabIndex = 0;
            this.radStartCreate.TabStop = true;
            this.radStartCreate.Text = "I do not have a Moving Pictures Social account.  Create one for me.";
            this.radStartCreate.UseVisualStyleBackColor = true;
            // 
            // tabCreateAccount
            // 
            this.tabCreateAccount.Controls.Add(this.btnRegisterBack);
            this.tabCreateAccount.Controls.Add(this.txtRegisterEmail);
            this.tabCreateAccount.Controls.Add(this.label16);
            this.tabCreateAccount.Controls.Add(this.btnRegisterNext);
            this.tabCreateAccount.Controls.Add(this.chkRegisterPrivateProfile);
            this.tabCreateAccount.Controls.Add(this.txtRegisterConfirmPassword);
            this.tabCreateAccount.Controls.Add(this.label15);
            this.tabCreateAccount.Controls.Add(this.txtRegisterPassword);
            this.tabCreateAccount.Controls.Add(this.label13);
            this.tabCreateAccount.Controls.Add(this.txtRegisterUsername);
            this.tabCreateAccount.Controls.Add(this.label14);
            this.tabCreateAccount.Controls.Add(this.label10);
            this.tabCreateAccount.Location = new System.Drawing.Point(4, 22);
            this.tabCreateAccount.Name = "tabCreateAccount";
            this.tabCreateAccount.Padding = new System.Windows.Forms.Padding(3);
            this.tabCreateAccount.Size = new System.Drawing.Size(520, 509);
            this.tabCreateAccount.TabIndex = 2;
            this.tabCreateAccount.Text = "tabCreateAccount";
            this.tabCreateAccount.UseVisualStyleBackColor = true;
            // 
            // btnRegisterBack
            // 
            this.btnRegisterBack.AutoSize = true;
            this.btnRegisterBack.Location = new System.Drawing.Point(6, 222);
            this.btnRegisterBack.Name = "btnRegisterBack";
            this.btnRegisterBack.Size = new System.Drawing.Size(75, 23);
            this.btnRegisterBack.TabIndex = 15;
            this.btnRegisterBack.Text = "Back";
            this.btnRegisterBack.UseVisualStyleBackColor = true;
            this.btnRegisterBack.Click += new System.EventHandler(this.btnRegisterBack_Click);
            // 
            // txtRegisterEmail
            // 
            this.txtRegisterEmail.Location = new System.Drawing.Point(103, 142);
            this.txtRegisterEmail.Name = "txtRegisterEmail";
            this.txtRegisterEmail.Size = new System.Drawing.Size(108, 20);
            this.txtRegisterEmail.TabIndex = 4;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(6, 150);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(32, 13);
            this.label16.TabIndex = 14;
            this.label16.Text = "Email";
            // 
            // btnRegisterNext
            // 
            this.btnRegisterNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRegisterNext.AutoSize = true;
            this.btnRegisterNext.Location = new System.Drawing.Point(439, 222);
            this.btnRegisterNext.Name = "btnRegisterNext";
            this.btnRegisterNext.Size = new System.Drawing.Size(75, 23);
            this.btnRegisterNext.TabIndex = 6;
            this.btnRegisterNext.Text = "Next";
            this.btnRegisterNext.UseVisualStyleBackColor = true;
            this.btnRegisterNext.Click += new System.EventHandler(this.btnRegisterNext_Click);
            // 
            // chkRegisterPrivateProfile
            // 
            this.chkRegisterPrivateProfile.AutoSize = true;
            this.chkRegisterPrivateProfile.Location = new System.Drawing.Point(103, 168);
            this.chkRegisterPrivateProfile.Name = "chkRegisterPrivateProfile";
            this.chkRegisterPrivateProfile.Size = new System.Drawing.Size(149, 17);
            this.chkRegisterPrivateProfile.TabIndex = 5;
            this.chkRegisterPrivateProfile.Text = "Make My Account Private";
            this.chkRegisterPrivateProfile.UseVisualStyleBackColor = true;
            // 
            // txtRegisterConfirmPassword
            // 
            this.txtRegisterConfirmPassword.Location = new System.Drawing.Point(103, 116);
            this.txtRegisterConfirmPassword.Name = "txtRegisterConfirmPassword";
            this.txtRegisterConfirmPassword.Size = new System.Drawing.Size(108, 20);
            this.txtRegisterConfirmPassword.TabIndex = 3;
            this.txtRegisterConfirmPassword.UseSystemPasswordChar = true;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(6, 124);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(91, 13);
            this.label15.TabIndex = 10;
            this.label15.Text = "Confirm Password";
            // 
            // txtRegisterPassword
            // 
            this.txtRegisterPassword.Location = new System.Drawing.Point(103, 90);
            this.txtRegisterPassword.Name = "txtRegisterPassword";
            this.txtRegisterPassword.Size = new System.Drawing.Size(108, 20);
            this.txtRegisterPassword.TabIndex = 2;
            this.txtRegisterPassword.UseSystemPasswordChar = true;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 98);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(53, 13);
            this.label13.TabIndex = 8;
            this.label13.Text = "Password";
            // 
            // txtRegisterUsername
            // 
            this.txtRegisterUsername.Location = new System.Drawing.Point(103, 64);
            this.txtRegisterUsername.Name = "txtRegisterUsername";
            this.txtRegisterUsername.Size = new System.Drawing.Size(108, 20);
            this.txtRegisterUsername.TabIndex = 1;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 72);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(55, 13);
            this.label14.TabIndex = 6;
            this.label14.Text = "Username";
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 16);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(368, 13);
            this.label10.TabIndex = 2;
            this.label10.Text = "Enter the information below to create a new Moving Pictures Social account.";
            // 
            // tabLinkAccount
            // 
            this.tabLinkAccount.Controls.Add(this.btnLinkBack);
            this.tabLinkAccount.Controls.Add(this.btnLinkNext);
            this.tabLinkAccount.Controls.Add(this.txtLinkPassword);
            this.tabLinkAccount.Controls.Add(this.label8);
            this.tabLinkAccount.Controls.Add(this.txtLinkUsername);
            this.tabLinkAccount.Controls.Add(this.label7);
            this.tabLinkAccount.Controls.Add(this.label6);
            this.tabLinkAccount.Location = new System.Drawing.Point(4, 22);
            this.tabLinkAccount.Name = "tabLinkAccount";
            this.tabLinkAccount.Padding = new System.Windows.Forms.Padding(3);
            this.tabLinkAccount.Size = new System.Drawing.Size(520, 509);
            this.tabLinkAccount.TabIndex = 1;
            this.tabLinkAccount.Text = "tabLinkAccount";
            this.tabLinkAccount.UseVisualStyleBackColor = true;
            // 
            // btnLinkBack
            // 
            this.btnLinkBack.AutoSize = true;
            this.btnLinkBack.Location = new System.Drawing.Point(6, 222);
            this.btnLinkBack.Name = "btnLinkBack";
            this.btnLinkBack.Size = new System.Drawing.Size(75, 23);
            this.btnLinkBack.TabIndex = 17;
            this.btnLinkBack.Text = "Back";
            this.btnLinkBack.UseVisualStyleBackColor = true;
            this.btnLinkBack.Click += new System.EventHandler(this.btnLinkBack_Click);
            // 
            // btnLinkNext
            // 
            this.btnLinkNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLinkNext.AutoSize = true;
            this.btnLinkNext.Location = new System.Drawing.Point(439, 222);
            this.btnLinkNext.Name = "btnLinkNext";
            this.btnLinkNext.Size = new System.Drawing.Size(75, 23);
            this.btnLinkNext.TabIndex = 16;
            this.btnLinkNext.Text = "Next";
            this.btnLinkNext.UseVisualStyleBackColor = true;
            this.btnLinkNext.Click += new System.EventHandler(this.btnLinkNext_Click);
            // 
            // txtLinkPassword
            // 
            this.txtLinkPassword.Location = new System.Drawing.Point(75, 118);
            this.txtLinkPassword.Name = "txtLinkPassword";
            this.txtLinkPassword.Size = new System.Drawing.Size(100, 20);
            this.txtLinkPassword.TabIndex = 5;
            this.txtLinkPassword.UseSystemPasswordChar = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(13, 126);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 13);
            this.label8.TabIndex = 4;
            this.label8.Text = "Password";
            // 
            // txtLinkUsername
            // 
            this.txtLinkUsername.Location = new System.Drawing.Point(75, 92);
            this.txtLinkUsername.Name = "txtLinkUsername";
            this.txtLinkUsername.Size = new System.Drawing.Size(100, 20);
            this.txtLinkUsername.TabIndex = 3;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 100);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(55, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "Username";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 25);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(446, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "If you already have a Moving Pictures Social account, enter the information below" +
                " to link to it.";
            // 
            // tabAlreadyLinked
            // 
            this.tabAlreadyLinked.Controls.Add(this.btnLinkedRemove);
            this.tabAlreadyLinked.Controls.Add(this.groupBox2);
            this.tabAlreadyLinked.Location = new System.Drawing.Point(4, 22);
            this.tabAlreadyLinked.Name = "tabAlreadyLinked";
            this.tabAlreadyLinked.Padding = new System.Windows.Forms.Padding(3);
            this.tabAlreadyLinked.Size = new System.Drawing.Size(520, 509);
            this.tabAlreadyLinked.TabIndex = 0;
            this.tabAlreadyLinked.Text = "tabAlreadyLinked";
            this.tabAlreadyLinked.UseVisualStyleBackColor = true;
            // 
            // btnLinkedRemove
            // 
            this.btnLinkedRemove.AutoSize = true;
            this.btnLinkedRemove.Location = new System.Drawing.Point(194, 254);
            this.btnLinkedRemove.Name = "btnLinkedRemove";
            this.btnLinkedRemove.Size = new System.Drawing.Size(216, 23);
            this.btnLinkedRemove.TabIndex = 27;
            this.btnLinkedRemove.Text = "Remove Moving Pictures Social";
            this.btnLinkedRemove.UseVisualStyleBackColor = true;
            this.btnLinkedRemove.Click += new System.EventHandler(this.btnLinkedRemove_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnLinkedSyncNow);
            this.groupBox2.Controls.Add(this.linkLinkedUsername);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(6, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(436, 92);
            this.groupBox2.TabIndex = 26;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Current Status";
            // 
            // btnLinkedSyncNow
            // 
            this.btnLinkedSyncNow.Location = new System.Drawing.Point(256, 45);
            this.btnLinkedSyncNow.Name = "btnLinkedSyncNow";
            this.btnLinkedSyncNow.Size = new System.Drawing.Size(163, 23);
            this.btnLinkedSyncNow.TabIndex = 28;
            this.btnLinkedSyncNow.Text = "Sync Movie Collection Now";
            this.btnLinkedSyncNow.UseVisualStyleBackColor = true;
            this.btnLinkedSyncNow.Click += new System.EventHandler(this.btnLinkedSyncNow_Click);
            // 
            // linkLinkedUsername
            // 
            this.linkLinkedUsername.AutoSize = true;
            this.linkLinkedUsername.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLinkedUsername.Location = new System.Drawing.Point(9, 42);
            this.linkLinkedUsername.Name = "linkLinkedUsername";
            this.linkLinkedUsername.Size = new System.Drawing.Size(97, 24);
            this.linkLinkedUsername.TabIndex = 27;
            this.linkLinkedUsername.TabStop = true;
            this.linkLinkedUsername.Text = "Username";
            this.linkLinkedUsername.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLinkedUsername_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(394, 13);
            this.label1.TabIndex = 26;
            this.label1.Text = "Moving Pictures is currently linked to the following Moving Pictures Social accou" +
                "nt";
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // SocialPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Name = "SocialPane";
            this.Size = new System.Drawing.Size(528, 535);
            this.Load += new System.EventHandler(this.SocialPane_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabStartWizard.ResumeLayout(false);
            this.tabStartWizard.PerformLayout();
            this.tabCreateAccount.ResumeLayout(false);
            this.tabCreateAccount.PerformLayout();
            this.tabLinkAccount.ResumeLayout(false);
            this.tabLinkAccount.PerformLayout();
            this.tabAlreadyLinked.ResumeLayout(false);
            this.tabAlreadyLinked.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabAlreadyLinked;
        private System.Windows.Forms.TabPage tabLinkAccount;
        private System.Windows.Forms.TextBox txtLinkPassword;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtLinkUsername;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TabPage tabCreateAccount;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btnRegisterNext;
        private System.Windows.Forms.CheckBox chkRegisterPrivateProfile;
        private System.Windows.Forms.TextBox txtRegisterConfirmPassword;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox txtRegisterPassword;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtRegisterUsername;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtRegisterEmail;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnLinkedSyncNow;
        private System.Windows.Forms.LinkLabel linkLinkedUsername;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnLinkedRemove;
        private System.Windows.Forms.TabPage tabStartWizard;
        private System.Windows.Forms.Button btnStartNext;
        private System.Windows.Forms.RadioButton radStartLink;
        private System.Windows.Forms.RadioButton radStartCreate;
        private System.Windows.Forms.Button btnRegisterBack;
        private System.Windows.Forms.Button btnLinkBack;
        private System.Windows.Forms.Button btnLinkNext;
        private System.Windows.Forms.Label label2;


    }
}
