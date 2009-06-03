using Cornerstone.Database.Tables;
namespace Cornerstone.GUI.Dialogs {
    partial class AttributeTypeEditor {
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
            Cornerstone.GUI.Controls.FieldProperty fieldProperty1 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty2 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty3 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty4 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty5 = new Cornerstone.GUI.Controls.FieldProperty();
            this.button1 = new System.Windows.Forms.Button();
            this.attrDescrList = new Cornerstone.GUI.Controls.DBObjectEditor();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(248, 238);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // attrDescrList
            // 
            this.attrDescrList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.attrDescrList.DatabaseObject = null;
            fieldProperty1.DisplayName = "Name";
            fieldProperty1.FieldName = "Name";
            fieldProperty2.DisplayName = "Table";
            fieldProperty2.FieldName = "Table";
            fieldProperty2.Visible = false;
            fieldProperty3.DisplayName = "Data Type";
            fieldProperty3.FieldName = "ValueType";
            fieldProperty4.DisplayName = "Selection Mode";
            fieldProperty4.FieldName = "SelectionMode";
            fieldProperty5.DisplayName = "Default";
            fieldProperty5.FieldName = "Default";
            this.attrDescrList.FieldDisplaySettings.FieldProperties.Add(fieldProperty1);
            this.attrDescrList.FieldDisplaySettings.FieldProperties.Add(fieldProperty2);
            this.attrDescrList.FieldDisplaySettings.FieldProperties.Add(fieldProperty3);
            this.attrDescrList.FieldDisplaySettings.FieldProperties.Add(fieldProperty4);
            this.attrDescrList.FieldDisplaySettings.FieldProperties.Add(fieldProperty5);
            this.attrDescrList.Location = new System.Drawing.Point(13, 13);
            this.attrDescrList.Name = "attrDescrList";
            this.attrDescrList.Size = new System.Drawing.Size(311, 93);
            this.attrDescrList.TabIndex = 0;
            this.attrDescrList.FieldDisplaySettings.Table = typeof(DBAttrDescription);
            // 
            // AttributeTypeEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(336, 273);
            this.ControlBox = false;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.attrDescrList);
            this.MaximizeBox = false;
            this.Name = "AttributeTypeEditor";
            this.ShowInTaskbar = false;
            this.Text = "AttributeTypeEditor";
            this.Load += new System.EventHandler(this.AttributeTypeEditor_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Cornerstone.GUI.Controls.DBObjectEditor attrDescrList;
        private System.Windows.Forms.Button button1;
    }
}
