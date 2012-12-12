namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    partial class AutoDataSourcesPanel {
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
            this.label1 = new System.Windows.Forms.Label();
            this.autoRadioButton = new System.Windows.Forms.RadioButton();
            this.languageComboBox = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.manualRadioButton = new System.Windows.Forms.RadioButton();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(375, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "language.";
            // 
            // autoRadioButton
            // 
            this.autoRadioButton.AutoSize = true;
            this.autoRadioButton.Location = new System.Drawing.Point(3, 1);
            this.autoRadioButton.Name = "autoRadioButton";
            this.autoRadioButton.Size = new System.Drawing.Size(216, 17);
            this.autoRadioButton.TabIndex = 4;
            this.autoRadioButton.TabStop = true;
            this.autoRadioButton.Text = "Automatically retrieve movie data in the";
            this.autoRadioButton.UseVisualStyleBackColor = true;
            this.autoRadioButton.CheckedChanged += new System.EventHandler(this.autoRadioButton_CheckedChanged);
            // 
            // languageComboBox
            // 
            this.languageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.languageComboBox.FormattingEnabled = true;
            this.languageComboBox.Location = new System.Drawing.Point(225, 0);
            this.languageComboBox.Name = "languageComboBox";
            this.languageComboBox.Size = new System.Drawing.Size(144, 21);
            this.languageComboBox.TabIndex = 3;
            this.languageComboBox.SelectedIndexChanged += new System.EventHandler(this.languageComboBox_SelectedIndexChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.languageComboBox);
            this.panel1.Controls.Add(this.manualRadioButton);
            this.panel1.Controls.Add(this.autoRadioButton);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(433, 44);
            this.panel1.TabIndex = 6;
            // 
            // manualRadioButton
            // 
            this.manualRadioButton.AutoSize = true;
            this.manualRadioButton.Location = new System.Drawing.Point(3, 25);
            this.manualRadioButton.Name = "manualRadioButton";
            this.manualRadioButton.Size = new System.Drawing.Size(208, 17);
            this.manualRadioButton.TabIndex = 6;
            this.manualRadioButton.TabStop = true;
            this.manualRadioButton.Text = "Manually manage movie data sources.";
            this.manualRadioButton.UseVisualStyleBackColor = true;
            this.manualRadioButton.CheckedChanged += new System.EventHandler(this.manualRadioButton_CheckedChanged);
            // 
            // AutoDataSourcesPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "AutoDataSourcesPanel";
            this.Size = new System.Drawing.Size(433, 44);
            this.Load += new System.EventHandler(this.AutoDataSourcesPanel_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton autoRadioButton;
        private System.Windows.Forms.ComboBox languageComboBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton manualRadioButton;
    }
}
