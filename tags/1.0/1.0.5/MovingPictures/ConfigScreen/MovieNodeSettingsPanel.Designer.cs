namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    partial class MovieNodeSettingsPanel {
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.fileBrowserButton = new System.Windows.Forms.Button();
            this.backdropMovieCombo = new System.Windows.Forms.ComboBox();
            this.backdropFileTextBox = new System.Windows.Forms.TextBox();
            this.fileBackdropRadioButton = new System.Windows.Forms.RadioButton();
            this.specificBackdropRadioButton = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.randomBackdropRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.sortFieldCombo = new System.Windows.Forms.ComboBox();
            this.sortDirectionCombo = new System.Windows.Forms.ComboBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.customSortRadioButton = new System.Windows.Forms.RadioButton();
            this.defaultSortRadioButton = new System.Windows.Forms.RadioButton();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Display Settings:";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.fileBrowserButton);
            this.panel1.Controls.Add(this.backdropMovieCombo);
            this.panel1.Controls.Add(this.backdropFileTextBox);
            this.panel1.Controls.Add(this.fileBackdropRadioButton);
            this.panel1.Controls.Add(this.specificBackdropRadioButton);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.randomBackdropRadioButton);
            this.panel1.Location = new System.Drawing.Point(127, 7);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(314, 101);
            this.panel1.TabIndex = 3;
            // 
            // fileBrowserButton
            // 
            this.fileBrowserButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fileBrowserButton.Location = new System.Drawing.Point(280, 71);
            this.fileBrowserButton.Name = "fileBrowserButton";
            this.fileBrowserButton.Size = new System.Drawing.Size(31, 23);
            this.fileBrowserButton.TabIndex = 6;
            this.fileBrowserButton.Text = "...";
            this.fileBrowserButton.UseVisualStyleBackColor = true;
            this.fileBrowserButton.Click += new System.EventHandler(this.fileBrowserButton_Click);
            // 
            // backdropMovieCombo
            // 
            this.backdropMovieCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.backdropMovieCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.backdropMovieCombo.FormattingEnabled = true;
            this.backdropMovieCombo.Location = new System.Drawing.Point(97, 46);
            this.backdropMovieCombo.Name = "backdropMovieCombo";
            this.backdropMovieCombo.Size = new System.Drawing.Size(214, 21);
            this.backdropMovieCombo.TabIndex = 5;
            this.backdropMovieCombo.SelectedIndexChanged += new System.EventHandler(this.backdropMovieCombo_SelectedIndexChanged);
            this.backdropMovieCombo.DropDown += new System.EventHandler(this.backdropMovieCombo_DropDown);
            // 
            // backdropFileTextBox
            // 
            this.backdropFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.backdropFileTextBox.Location = new System.Drawing.Point(97, 73);
            this.backdropFileTextBox.Name = "backdropFileTextBox";
            this.backdropFileTextBox.Size = new System.Drawing.Size(177, 21);
            this.backdropFileTextBox.TabIndex = 4;
            this.backdropFileTextBox.TextChanged += new System.EventHandler(this.backdropFileTextBox_TextChanged);
            // 
            // fileBackdropRadioButton
            // 
            this.fileBackdropRadioButton.AutoSize = true;
            this.fileBackdropRadioButton.Location = new System.Drawing.Point(16, 74);
            this.fileBackdropRadioButton.Name = "fileBackdropRadioButton";
            this.fileBackdropRadioButton.Size = new System.Drawing.Size(63, 17);
            this.fileBackdropRadioButton.TabIndex = 3;
            this.fileBackdropRadioButton.TabStop = true;
            this.fileBackdropRadioButton.Text = "this file:";
            this.fileBackdropRadioButton.UseVisualStyleBackColor = true;
            this.fileBackdropRadioButton.CheckedChanged += new System.EventHandler(this.fileBackdropRadioButton_CheckedChanged);
            // 
            // specificBackdropRadioButton
            // 
            this.specificBackdropRadioButton.AutoSize = true;
            this.specificBackdropRadioButton.Location = new System.Drawing.Point(16, 47);
            this.specificBackdropRadioButton.Name = "specificBackdropRadioButton";
            this.specificBackdropRadioButton.Size = new System.Drawing.Size(77, 17);
            this.specificBackdropRadioButton.TabIndex = 2;
            this.specificBackdropRadioButton.TabStop = true;
            this.specificBackdropRadioButton.Text = "this movie:";
            this.specificBackdropRadioButton.UseVisualStyleBackColor = true;
            this.specificBackdropRadioButton.CheckedChanged += new System.EventHandler(this.specificBackdropRadioButton_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(252, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "When this node is highlighted use a backdrop from:";
            // 
            // randomBackdropRadioButton
            // 
            this.randomBackdropRadioButton.AutoSize = true;
            this.randomBackdropRadioButton.Location = new System.Drawing.Point(16, 21);
            this.randomBackdropRadioButton.Name = "randomBackdropRadioButton";
            this.randomBackdropRadioButton.Size = new System.Drawing.Size(204, 17);
            this.randomBackdropRadioButton.TabIndex = 0;
            this.randomBackdropRadioButton.TabStop = true;
            this.randomBackdropRadioButton.Text = "a random movie under this menu item";
            this.randomBackdropRadioButton.UseVisualStyleBackColor = true;
            this.randomBackdropRadioButton.CheckedChanged += new System.EventHandler(this.randomBackdropRadioButton_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Location = new System.Drawing.Point(129, 107);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(315, 3);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Location = new System.Drawing.Point(6, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(438, 3);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "groupBox2";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(128, 113);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Sort movies:";
            // 
            // sortFieldCombo
            // 
            this.sortFieldCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.sortFieldCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sortFieldCombo.FormattingEnabled = true;
            this.sortFieldCombo.Location = new System.Drawing.Point(59, 26);
            this.sortFieldCombo.Name = "sortFieldCombo";
            this.sortFieldCombo.Size = new System.Drawing.Size(159, 21);
            this.sortFieldCombo.TabIndex = 7;
            this.sortFieldCombo.SelectedIndexChanged += new System.EventHandler(this.sortFieldCombo_SelectedIndexChanged);
            // 
            // sortDirectionCombo
            // 
            this.sortDirectionCombo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sortDirectionCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sortDirectionCombo.FormattingEnabled = true;
            this.sortDirectionCombo.Items.AddRange(new object[] {
            "Ascending",
            "Descending"});
            this.sortDirectionCombo.Location = new System.Drawing.Point(224, 26);
            this.sortDirectionCombo.Name = "sortDirectionCombo";
            this.sortDirectionCombo.Size = new System.Drawing.Size(90, 21);
            this.sortDirectionCombo.TabIndex = 8;
            this.sortDirectionCombo.SelectedIndexChanged += new System.EventHandler(this.sortDirectionCombo_SelectedIndexChanged);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.customSortRadioButton);
            this.panel2.Controls.Add(this.defaultSortRadioButton);
            this.panel2.Controls.Add(this.sortFieldCombo);
            this.panel2.Controls.Add(this.sortDirectionCombo);
            this.panel2.Location = new System.Drawing.Point(127, 129);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(317, 55);
            this.panel2.TabIndex = 9;
            // 
            // customSortRadioButton
            // 
            this.customSortRadioButton.AutoSize = true;
            this.customSortRadioButton.Location = new System.Drawing.Point(16, 27);
            this.customSortRadioButton.Name = "customSortRadioButton";
            this.customSortRadioButton.Size = new System.Drawing.Size(37, 17);
            this.customSortRadioButton.TabIndex = 10;
            this.customSortRadioButton.TabStop = true;
            this.customSortRadioButton.Text = "by";
            this.customSortRadioButton.UseVisualStyleBackColor = true;
            this.customSortRadioButton.CheckedChanged += new System.EventHandler(this.customSortRadioButton_CheckedChanged);
            // 
            // defaultSortRadioButton
            // 
            this.defaultSortRadioButton.AutoSize = true;
            this.defaultSortRadioButton.Location = new System.Drawing.Point(16, 3);
            this.defaultSortRadioButton.Name = "defaultSortRadioButton";
            this.defaultSortRadioButton.Size = new System.Drawing.Size(157, 17);
            this.defaultSortRadioButton.TabIndex = 9;
            this.defaultSortRadioButton.TabStop = true;
            this.defaultSortRadioButton.Text = "using the default sort order";
            this.defaultSortRadioButton.UseVisualStyleBackColor = true;
            this.defaultSortRadioButton.CheckedChanged += new System.EventHandler(this.defaultSortRadioButton_CheckedChanged);
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "jpg";
            this.openFileDialog.FileName = "openFileDialog";
            this.openFileDialog.Filter = "Images|*.jpg; *.png|All Files|*.*";
            // 
            // MovieNodeSettingsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "MovieNodeSettingsPanel";
            this.Size = new System.Drawing.Size(444, 190);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton randomBackdropRadioButton;
        private System.Windows.Forms.RadioButton fileBackdropRadioButton;
        private System.Windows.Forms.RadioButton specificBackdropRadioButton;
        private System.Windows.Forms.ComboBox backdropMovieCombo;
        private System.Windows.Forms.TextBox backdropFileTextBox;
        private System.Windows.Forms.Button fileBrowserButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox sortFieldCombo;
        private System.Windows.Forms.ComboBox sortDirectionCombo;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton defaultSortRadioButton;
        private System.Windows.Forms.RadioButton customSortRadioButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}
