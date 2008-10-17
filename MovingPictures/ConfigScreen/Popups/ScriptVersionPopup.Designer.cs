namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    partial class ScriptVersionPopup {
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
            this.label1 = new System.Windows.Forms.Label();
            this.sourceNameLabel = new System.Windows.Forms.Label();
            this.listView = new System.Windows.Forms.ListView();
            this.versionHeader = new System.Windows.Forms.ColumnHeader();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(9, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(180, 28);
            this.label1.TabIndex = 0;
            this.label1.Text = "Please select the version of the script you wish to use.";
            // 
            // sourceNameLabel
            // 
            this.sourceNameLabel.AutoSize = true;
            this.sourceNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sourceNameLabel.Location = new System.Drawing.Point(8, 9);
            this.sourceNameLabel.Name = "sourceNameLabel";
            this.sourceNameLabel.Size = new System.Drawing.Size(52, 20);
            this.sourceNameLabel.TabIndex = 1;
            this.sourceNameLabel.Text = "IMDb";
            // 
            // listView
            // 
            this.listView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.versionHeader});
            this.listView.FullRowSelect = true;
            this.listView.GridLines = true;
            this.listView.Location = new System.Drawing.Point(12, 60);
            this.listView.MultiSelect = false;
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(170, 126);
            this.listView.TabIndex = 2;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            // 
            // versionHeader
            // 
            this.versionHeader.Text = "Version";
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(48, 193);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(52, 23);
            this.okButton.TabIndex = 3;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(106, 193);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // ScriptVersionPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(194, 226);
            this.ControlBox = false;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.listView);
            this.Controls.Add(this.sourceNameLabel);
            this.Name = "ScriptVersionPopup";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Data Source Version";
            this.Load += new System.EventHandler(this.ScriptVersionPopup_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label sourceNameLabel;
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.ColumnHeader versionHeader;
        private System.Windows.Forms.Button cancelButton;
    }
}