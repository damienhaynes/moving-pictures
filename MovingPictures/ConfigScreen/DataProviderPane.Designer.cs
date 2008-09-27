namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    partial class DataProviderPane {
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
            Cornerstone.GUI.Controls.FieldProperty fieldProperty1 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty2 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty3 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty4 = new Cornerstone.GUI.Controls.FieldProperty();
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.detailsPage = new System.Windows.Forms.TabPage();
            this.coversPage = new System.Windows.Forms.TabPage();
            this.backdropsPage = new System.Windows.Forms.TabPage();
            this.dbObjectEditor = new Cornerstone.GUI.Controls.DBObjectEditor();
            this.groupBox.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.detailsPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox
            // 
            this.groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox.Controls.Add(this.tabControl);
            this.groupBox.Location = new System.Drawing.Point(3, 3);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(444, 343);
            this.groupBox.TabIndex = 0;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "Data Sources";
            // 
            // tabControl
            // 
            this.tabControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl.Controls.Add(this.detailsPage);
            this.tabControl.Controls.Add(this.coversPage);
            this.tabControl.Controls.Add(this.backdropsPage);
            this.tabControl.Location = new System.Drawing.Point(7, 20);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(431, 317);
            this.tabControl.TabIndex = 0;
            // 
            // detailsPage
            // 
            this.detailsPage.Controls.Add(this.dbObjectEditor);
            this.detailsPage.Location = new System.Drawing.Point(4, 25);
            this.detailsPage.Name = "detailsPage";
            this.detailsPage.Padding = new System.Windows.Forms.Padding(3);
            this.detailsPage.Size = new System.Drawing.Size(423, 288);
            this.detailsPage.TabIndex = 0;
            this.detailsPage.Text = "Movie Details";
            this.detailsPage.UseVisualStyleBackColor = true;
            // 
            // coversPage
            // 
            this.coversPage.Location = new System.Drawing.Point(4, 25);
            this.coversPage.Name = "coversPage";
            this.coversPage.Padding = new System.Windows.Forms.Padding(3);
            this.coversPage.Size = new System.Drawing.Size(423, 288);
            this.coversPage.TabIndex = 1;
            this.coversPage.Text = "Covers";
            this.coversPage.UseVisualStyleBackColor = true;
            // 
            // backdropsPage
            // 
            this.backdropsPage.Location = new System.Drawing.Point(4, 25);
            this.backdropsPage.Name = "backdropsPage";
            this.backdropsPage.Padding = new System.Windows.Forms.Padding(3);
            this.backdropsPage.Size = new System.Drawing.Size(423, 288);
            this.backdropsPage.TabIndex = 2;
            this.backdropsPage.Text = "Backdrops";
            this.backdropsPage.UseVisualStyleBackColor = true;
            // 
            // dbObjectEditor
            // 
            this.dbObjectEditor.DatabaseObject = null;
            fieldProperty1.DisplayName = "Full Path";
            fieldProperty1.FieldName = "FullPath";
            fieldProperty2.DisplayName = "Part";
            fieldProperty2.FieldName = "Part";
            fieldProperty3.DisplayName = "Ignored";
            fieldProperty3.FieldName = "Ignored";
            fieldProperty4.DisplayName = "Import Path";
            fieldProperty4.FieldName = "ImportPath";
            this.dbObjectEditor.FieldProperties.Add(fieldProperty1);
            this.dbObjectEditor.FieldProperties.Add(fieldProperty2);
            this.dbObjectEditor.FieldProperties.Add(fieldProperty3);
            this.dbObjectEditor.FieldProperties.Add(fieldProperty4);
            this.dbObjectEditor.Location = new System.Drawing.Point(7, 7);
            this.dbObjectEditor.Name = "dbObjectEditor";
            this.dbObjectEditor.Size = new System.Drawing.Size(410, 275);
            this.dbObjectEditor.TabIndex = 0;
            this.dbObjectEditor.Table = typeof(MediaPortal.Plugins.MovingPictures.Database.DBLocalMedia);
            // 
            // DataProviderPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox);
            this.Name = "DataProviderPane";
            this.Size = new System.Drawing.Size(450, 349);
            this.groupBox.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.detailsPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage detailsPage;
        private System.Windows.Forms.TabPage coversPage;
        private System.Windows.Forms.TabPage backdropsPage;
        private Cornerstone.GUI.Controls.DBObjectEditor dbObjectEditor;
    }
}
