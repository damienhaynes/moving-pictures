namespace Cornerstone.GUI.Filtering {
    partial class CriteriaPanel<T> {
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.fieldComboBox = new System.Windows.Forms.ComboBox();
            this.operatorComboBox = new System.Windows.Forms.ComboBox();
            this.valueInputField = new Cornerstone.GUI.Filtering.CriteriaInputField();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 119F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.fieldComboBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.operatorComboBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.valueInputField, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(449, 25);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // fieldComboBox
            // 
            this.fieldComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fieldComboBox.FormattingEnabled = true;
            this.fieldComboBox.Items.AddRange(new object[] {
            "Title",
            "Year",
            "Genre"});
            this.fieldComboBox.Location = new System.Drawing.Point(2, 2);
            this.fieldComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.fieldComboBox.Name = "fieldComboBox";
            this.fieldComboBox.Size = new System.Drawing.Size(115, 21);
            this.fieldComboBox.TabIndex = 1;
            this.fieldComboBox.SelectedIndexChanged += new System.EventHandler(this.fieldComboBox_SelectedIndexChanged);
            // 
            // operatorComboBox
            // 
            this.operatorComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.operatorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.operatorComboBox.FormattingEnabled = true;
            this.operatorComboBox.Items.AddRange(new object[] {
            "is",
            "is not",
            "contains",
            "does not contain",
            "equals",
            "not equal to",
            "is less than",
            "is greater than",
            "is less than or equal to",
            "is greater than or equal to",
            ""});
            this.operatorComboBox.Location = new System.Drawing.Point(121, 2);
            this.operatorComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.operatorComboBox.Name = "operatorComboBox";
            this.operatorComboBox.Size = new System.Drawing.Size(146, 21);
            this.operatorComboBox.TabIndex = 2;
            this.operatorComboBox.SelectedIndexChanged += new System.EventHandler(this.operatorComboBox_SelectedIndexChanged);
            // 
            // valueInputField
            // 
            this.valueInputField.AutoSize = true;
            this.valueInputField.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.valueInputField.Dock = System.Windows.Forms.DockStyle.Fill;
            this.valueInputField.InputType = Cornerstone.GUI.Filtering.CriteriaInputType.STRING;
            this.valueInputField.Location = new System.Drawing.Point(271, 2);
            this.valueInputField.Margin = new System.Windows.Forms.Padding(2);
            this.valueInputField.Name = "valueInputField";
            this.valueInputField.Size = new System.Drawing.Size(176, 21);
            this.valueInputField.TabIndex = 0;
            // 
            // CriteriaPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(400, 25);
            this.Name = "CriteriaPanel";
            this.Size = new System.Drawing.Size(449, 25);
            this.Leave += new System.EventHandler(this.CriteriaPanel_Leave);
            this.Enter += new System.EventHandler(this.CriteriaPanel_Enter);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox fieldComboBox;
        private System.Windows.Forms.ComboBox operatorComboBox;
        public CriteriaInputField valueInputField;
    }
}
