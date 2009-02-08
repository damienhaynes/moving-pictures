namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    partial class SearchStringPopup {
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
            this.cancelButton = new System.Windows.Forms.Button();
            this.descriptionLabel = new System.Windows.Forms.TextBox();
            this.fileListBox = new System.Windows.Forms.ListBox();
            this.searchStrTextBox = new System.Windows.Forms.TextBox();
            this.searchStrLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(316, 156);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 3;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(397, 156);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionLabel.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.descriptionLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.descriptionLabel.Location = new System.Drawing.Point(12, 12);
            this.descriptionLabel.Multiline = true;
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.ReadOnly = true;
            this.descriptionLabel.Size = new System.Drawing.Size(461, 30);
            this.descriptionLabel.TabIndex = 5;
            this.descriptionLabel.Text = "Please enter a new search string for the following file(s). If these files do not" +
                " belong together you should click \"Cancel\" and split them using the Split button" +
                " on the Media Importer.";
            // 
            // fileListBox
            // 
            this.fileListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.fileListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.fileListBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileListBox.FormattingEnabled = true;
            this.fileListBox.ItemHeight = 14;
            this.fileListBox.Location = new System.Drawing.Point(12, 49);
            this.fileListBox.Name = "fileListBox";
            this.fileListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.fileListBox.Size = new System.Drawing.Size(461, 44);
            this.fileListBox.TabIndex = 6;
            // 
            // searchStrTextBox
            // 
            this.searchStrTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.searchStrTextBox.Location = new System.Drawing.Point(12, 122);
            this.searchStrTextBox.Name = "searchStrTextBox";
            this.searchStrTextBox.Size = new System.Drawing.Size(460, 20);
            this.searchStrTextBox.TabIndex = 7;
            // 
            // searchStrLabel
            // 
            this.searchStrLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.searchStrLabel.AutoSize = true;
            this.searchStrLabel.Location = new System.Drawing.Point(12, 106);
            this.searchStrLabel.Name = "searchStrLabel";
            this.searchStrLabel.Size = new System.Drawing.Size(71, 13);
            this.searchStrLabel.TabIndex = 8;
            this.searchStrLabel.Text = "Search String";
            // 
            // SearchStringPopup
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(485, 191);
            this.Controls.Add(this.searchStrTextBox);
            this.Controls.Add(this.searchStrLabel);
            this.Controls.Add(this.fileListBox);
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Name = "SearchStringPopup";
            this.ShowInTaskbar = false;
            this.Text = "New Search String";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox descriptionLabel;
        private System.Windows.Forms.ListBox fileListBox;
        private System.Windows.Forms.TextBox searchStrTextBox;
        private System.Windows.Forms.Label searchStrLabel;
    }
}