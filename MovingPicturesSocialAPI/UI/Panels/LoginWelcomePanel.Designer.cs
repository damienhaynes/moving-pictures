namespace MovingPicturesSocialAPI.UI.Panels {
    partial class LoginWelcomePanel {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginWelcomePanel));
            this.label1 = new System.Windows.Forms.Label();
            this.existingAccountButton = new System.Windows.Forms.Button();
            this.newAccountButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(289, 116);
            this.label1.TabIndex = 6;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // existingAccountButton
            // 
            this.existingAccountButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.existingAccountButton.Location = new System.Drawing.Point(131, 145);
            this.existingAccountButton.Name = "existingAccountButton";
            this.existingAccountButton.Size = new System.Drawing.Size(154, 23);
            this.existingAccountButton.TabIndex = 5;
            this.existingAccountButton.Text = "Login to Existing Account";
            this.existingAccountButton.UseVisualStyleBackColor = true;
            this.existingAccountButton.Click += new System.EventHandler(this.existingAccountButton_Click);
            // 
            // newAccountButton
            // 
            this.newAccountButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.newAccountButton.Location = new System.Drawing.Point(0, 145);
            this.newAccountButton.Name = "newAccountButton";
            this.newAccountButton.Size = new System.Drawing.Size(125, 23);
            this.newAccountButton.TabIndex = 1;
            this.newAccountButton.Text = "Create New Account";
            this.newAccountButton.UseVisualStyleBackColor = true;
            this.newAccountButton.Click += new System.EventHandler(this.newAccountButton_Click);
            // 
            // LoginWelcomePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.existingAccountButton);
            this.Controls.Add(this.newAccountButton);
            this.Name = "LoginWelcomePanel";
            this.Size = new System.Drawing.Size(292, 171);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button existingAccountButton;
        private System.Windows.Forms.Button newAccountButton;
    }
}
