namespace Cornerstone.GUI.Filtering {
    partial class AddNodePopup {
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
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.emptyNodeRadioButton = new System.Windows.Forms.RadioButton();
            this.fieldComboBox = new System.Windows.Forms.ComboBox();
            this.dynamicNodeRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rootCheckBox = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(283, 124);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 0;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(202, 124);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.rootCheckBox);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.emptyNodeRadioButton);
            this.panel1.Controls.Add(this.fieldComboBox);
            this.panel1.Controls.Add(this.dynamicNodeRadioButton);
            this.panel1.Location = new System.Drawing.Point(13, 13);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(345, 105);
            this.panel1.TabIndex = 2;
            // 
            // emptyNodeRadioButton
            // 
            this.emptyNodeRadioButton.AutoSize = true;
            this.emptyNodeRadioButton.Location = new System.Drawing.Point(4, 28);
            this.emptyNodeRadioButton.Name = "emptyNodeRadioButton";
            this.emptyNodeRadioButton.Size = new System.Drawing.Size(141, 17);
            this.emptyNodeRadioButton.TabIndex = 2;
            this.emptyNodeRadioButton.Text = "Add an empty menu item";
            this.emptyNodeRadioButton.UseVisualStyleBackColor = true;
            this.emptyNodeRadioButton.CheckedChanged += new System.EventHandler(this.emptyNodeRadioButton_CheckedChanged);
            // 
            // fieldComboBox
            // 
            this.fieldComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.fieldComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fieldComboBox.FormattingEnabled = true;
            this.fieldComboBox.Location = new System.Drawing.Point(174, 3);
            this.fieldComboBox.Name = "fieldComboBox";
            this.fieldComboBox.Size = new System.Drawing.Size(168, 21);
            this.fieldComboBox.TabIndex = 1;
            // 
            // dynamicNodeRadioButton
            // 
            this.dynamicNodeRadioButton.AutoSize = true;
            this.dynamicNodeRadioButton.Checked = true;
            this.dynamicNodeRadioButton.Location = new System.Drawing.Point(4, 4);
            this.dynamicNodeRadioButton.Name = "dynamicNodeRadioButton";
            this.dynamicNodeRadioButton.Size = new System.Drawing.Size(164, 17);
            this.dynamicNodeRadioButton.TabIndex = 0;
            this.dynamicNodeRadioButton.TabStop = true;
            this.dynamicNodeRadioButton.Text = "Add a dynamic menu item for:";
            this.dynamicNodeRadioButton.UseVisualStyleBackColor = true;
            this.dynamicNodeRadioButton.CheckedChanged += new System.EventHandler(this.dynamicNodeRadioButton_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(4, 62);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(338, 3);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // rootCheckBox
            // 
            this.rootCheckBox.AutoSize = true;
            this.rootCheckBox.Location = new System.Drawing.Point(4, 81);
            this.rootCheckBox.Name = "rootCheckBox";
            this.rootCheckBox.Size = new System.Drawing.Size(161, 17);
            this.rootCheckBox.TabIndex = 4;
            this.rootCheckBox.Text = "Add Menu Item to Top Level";
            this.rootCheckBox.UseVisualStyleBackColor = true;
            // 
            // AddNodePopup
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(370, 159);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddNodePopup";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "New Menu Item";
            this.Load += new System.EventHandler(this.AddNodePopup_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton dynamicNodeRadioButton;
        private System.Windows.Forms.RadioButton emptyNodeRadioButton;
        private System.Windows.Forms.ComboBox fieldComboBox;
        private System.Windows.Forms.CheckBox rootCheckBox;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}