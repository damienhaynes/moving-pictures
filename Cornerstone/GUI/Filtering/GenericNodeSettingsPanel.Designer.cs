namespace Cornerstone.GUI.Filtering {
    partial class GenericNodeSettingsPanel<T> {
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
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.addEditFilterButton = new System.Windows.Forms.Button();
            this.removeFilterButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Node Settings:";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.nameTextBox.Location = new System.Drawing.Point(235, 3);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(212, 21);
            this.nameTextBox.TabIndex = 2;
            this.nameTextBox.Leave += new System.EventHandler(this.nameTextBox_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(126, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 13);
            this.label3.TabIndex = 28;
            this.label3.Text = "The node is labeled:";
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(129, 30);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(321, 3);
            this.groupBox1.TabIndex = 31;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // addEditFilterButton
            // 
            this.addEditFilterButton.Location = new System.Drawing.Point(129, 39);
            this.addEditFilterButton.Name = "addEditFilterButton";
            this.addEditFilterButton.Size = new System.Drawing.Size(85, 23);
            this.addEditFilterButton.TabIndex = 32;
            this.addEditFilterButton.Text = "Add Filter";
            this.addEditFilterButton.UseVisualStyleBackColor = true;
            this.addEditFilterButton.Click += new System.EventHandler(this.addEditFilterButton_Click);
            // 
            // removeFilterButton
            // 
            this.removeFilterButton.Enabled = false;
            this.removeFilterButton.Location = new System.Drawing.Point(220, 39);
            this.removeFilterButton.Name = "removeFilterButton";
            this.removeFilterButton.Size = new System.Drawing.Size(93, 23);
            this.removeFilterButton.TabIndex = 33;
            this.removeFilterButton.Text = "Remove Filter";
            this.removeFilterButton.UseVisualStyleBackColor = true;
            this.removeFilterButton.Click += new System.EventHandler(this.removeFilterButton_Click);
            // 
            // GenericNodeSettingsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.removeFilterButton);
            this.Controls.Add(this.addEditFilterButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "GenericNodeSettingsPanel";
            this.Size = new System.Drawing.Size(454, 66);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button addEditFilterButton;
        private System.Windows.Forms.Button removeFilterButton;
    }
}
