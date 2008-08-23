namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.DesignMode {
    partial class DBObjectListFieldEditorDialog {
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
            this.fieldList = new System.Windows.Forms.ListView();
            this.fieldColumn = new System.Windows.Forms.ColumnHeader();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.fieldsLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // fieldList
            // 
            this.fieldList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.fieldList.CheckBoxes = true;
            this.fieldList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.fieldColumn});
            this.fieldList.FullRowSelect = true;
            this.fieldList.GridLines = true;
            this.fieldList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.fieldList.HideSelection = false;
            this.fieldList.Location = new System.Drawing.Point(13, 25);
            this.fieldList.MultiSelect = false;
            this.fieldList.Name = "fieldList";
            this.fieldList.Size = new System.Drawing.Size(182, 317);
            this.fieldList.TabIndex = 0;
            this.fieldList.UseCompatibleStateImageBehavior = false;
            this.fieldList.View = System.Windows.Forms.View.Details;
            this.fieldList.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.fieldList_ItemChecked);
            this.fieldList.SelectedIndexChanged += new System.EventHandler(this.fieldList_SelectedIndexChanged);
            // 
            // fieldColumn
            // 
            this.fieldColumn.Text = "Database Field";
            this.fieldColumn.Width = 161;
            // 
            // propertyGrid
            // 
            this.propertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid.Location = new System.Drawing.Point(201, 13);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(279, 329);
            this.propertyGrid.TabIndex = 1;
            this.propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
            // 
            // fieldsLabel
            // 
            this.fieldsLabel.AutoSize = true;
            this.fieldsLabel.Location = new System.Drawing.Point(12, 9);
            this.fieldsLabel.Name = "fieldsLabel";
            this.fieldsLabel.Size = new System.Drawing.Size(37, 13);
            this.fieldsLabel.TabIndex = 2;
            this.fieldsLabel.Text = "Fields:";
            // 
            // DBObjectListFieldEditorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 355);
            this.Controls.Add(this.fieldsLabel);
            this.Controls.Add(this.propertyGrid);
            this.Controls.Add(this.fieldList);
            this.Name = "DBObjectListFieldEditorDialog";
            this.Text = "DatabaseTableTypeEditorDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView fieldList;
        private System.Windows.Forms.ColumnHeader fieldColumn;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.Label fieldsLabel;
    }
}