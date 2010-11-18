using MediaPortal.Plugins.MovingPictures.Database;
using Cornerstone.GUI.Filtering;
namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    partial class MovieFilterEditorPopup {
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
            Cornerstone.GUI.Controls.FieldDisplaySettings fieldDisplaySettings2 = new Cornerstone.GUI.Controls.FieldDisplaySettings();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty27 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty28 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty29 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty30 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty31 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty32 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty33 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty34 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty35 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty36 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty37 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty38 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty39 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty40 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty41 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty42 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty43 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty44 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty45 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty46 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty47 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty48 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty49 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty50 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty51 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty52 = new Cornerstone.GUI.Controls.FieldProperty();
            this.okButton = new System.Windows.Forms.Button();
            this.helpButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.filterEditorPane1 = new Cornerstone.GUI.Filtering.FilterEditorPane();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(544, 345);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // helpButton
            // 
            this.helpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.helpButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.helpButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.help;
            this.helpButton.Location = new System.Drawing.Point(677, 6);
            this.helpButton.Margin = new System.Windows.Forms.Padding(0);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(23, 23);
            this.helpButton.TabIndex = 18;
            this.helpButton.UseVisualStyleBackColor = true;
            this.helpButton.Click += new System.EventHandler(this.helpButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(625, 345);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // filterEditorPane1
            // 
            this.filterEditorPane1.DBManager = null;
            this.filterEditorPane1.DisplayName = "movies";
            this.filterEditorPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            fieldProperty27.ColumnWidth = 150;
            fieldProperty27.DisplayName = "Title";
            fieldProperty27.FieldName = "Title";
            fieldProperty28.ColumnWidth = null;
            fieldProperty28.DisplayName = "Alternate Titles";
            fieldProperty28.FieldName = "AlternateTitles";
            fieldProperty28.Visible = false;
            fieldProperty29.ColumnWidth = null;
            fieldProperty29.DisplayName = "Sort By";
            fieldProperty29.FieldName = "SortBy";
            fieldProperty29.Visible = false;
            fieldProperty30.ColumnWidth = null;
            fieldProperty30.DisplayName = "Directors";
            fieldProperty30.FieldName = "Directors";
            fieldProperty30.Visible = false;
            fieldProperty31.ColumnWidth = null;
            fieldProperty31.DisplayName = "Writers";
            fieldProperty31.FieldName = "Writers";
            fieldProperty31.Visible = false;
            fieldProperty32.ColumnWidth = null;
            fieldProperty32.DisplayName = "Actors";
            fieldProperty32.FieldName = "Actors";
            fieldProperty32.Visible = false;
            fieldProperty33.ColumnWidth = 40;
            fieldProperty33.DisplayName = "Year";
            fieldProperty33.FieldName = "Year";
            fieldProperty34.ColumnWidth = null;
            fieldProperty34.DisplayName = "Genres";
            fieldProperty34.FieldName = "Genres";
            fieldProperty34.Visible = false;
            fieldProperty35.ColumnWidth = null;
            fieldProperty35.DisplayName = "Certification";
            fieldProperty35.FieldName = "Certification";
            fieldProperty35.Visible = false;
            fieldProperty36.ColumnWidth = null;
            fieldProperty36.DisplayName = "Language";
            fieldProperty36.FieldName = "Language";
            fieldProperty36.Visible = false;
            fieldProperty37.ColumnWidth = null;
            fieldProperty37.DisplayName = "Tagline";
            fieldProperty37.FieldName = "Tagline";
            fieldProperty37.Visible = false;
            fieldProperty38.ColumnWidth = null;
            fieldProperty38.DisplayName = "Summary";
            fieldProperty38.FieldName = "Summary";
            fieldProperty38.Visible = false;
            fieldProperty39.ColumnWidth = null;
            fieldProperty39.DisplayName = "Score";
            fieldProperty39.FieldName = "Score";
            fieldProperty39.Visible = false;
            fieldProperty40.ColumnWidth = null;
            fieldProperty40.DisplayName = "Popularity";
            fieldProperty40.FieldName = "Popularity";
            fieldProperty40.Visible = false;
            fieldProperty41.ColumnWidth = null;
            fieldProperty41.DisplayName = "Date Added";
            fieldProperty41.FieldName = "DateAdded";
            fieldProperty41.Visible = false;
            fieldProperty42.ColumnWidth = null;
            fieldProperty42.DisplayName = "Runtime";
            fieldProperty42.FieldName = "Runtime";
            fieldProperty42.Visible = false;
            fieldProperty43.ColumnWidth = null;
            fieldProperty43.DisplayName = "Movie Xml ID";
            fieldProperty43.FieldName = "MovieXmlID";
            fieldProperty43.Visible = false;
            fieldProperty44.ColumnWidth = null;
            fieldProperty44.DisplayName = "Imdb ID";
            fieldProperty44.FieldName = "ImdbID";
            fieldProperty44.Visible = false;
            fieldProperty45.ColumnWidth = null;
            fieldProperty45.DisplayName = "Alternate Covers";
            fieldProperty45.FieldName = "AlternateCovers";
            fieldProperty45.Visible = false;
            fieldProperty46.ColumnWidth = null;
            fieldProperty46.DisplayName = "Cover Full Path";
            fieldProperty46.FieldName = "CoverFullPath";
            fieldProperty46.Visible = false;
            fieldProperty47.ColumnWidth = null;
            fieldProperty47.DisplayName = "Cover Thumb Full Path";
            fieldProperty47.FieldName = "CoverThumbFullPath";
            fieldProperty47.Visible = false;
            fieldProperty48.ColumnWidth = null;
            fieldProperty48.DisplayName = "Backdrop Full Path";
            fieldProperty48.FieldName = "BackdropFullPath";
            fieldProperty48.Visible = false;
            fieldProperty49.ColumnWidth = null;
            fieldProperty49.DisplayName = "Details URL";
            fieldProperty49.FieldName = "DetailsURL";
            fieldProperty49.Visible = false;
            fieldProperty50.ColumnWidth = null;
            fieldProperty50.DisplayName = "Primary Source";
            fieldProperty50.FieldName = "PrimarySource";
            fieldProperty50.Visible = false;
            fieldProperty51.ColumnWidth = null;
            fieldProperty51.DisplayName = "Original Directory Name";
            fieldProperty51.FieldName = "OriginalDirectoryName";
            fieldProperty51.Visible = false;
            fieldProperty52.ColumnWidth = null;
            fieldProperty52.DisplayName = "Mps Id";
            fieldProperty52.FieldName = "MpsId";
            fieldProperty52.Visible = false;
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty27);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty28);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty29);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty30);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty31);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty32);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty33);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty34);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty35);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty36);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty37);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty38);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty39);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty40);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty41);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty42);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty43);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty44);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty45);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty46);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty47);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty48);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty49);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty50);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty51);
            fieldDisplaySettings2.FieldProperties.Add(fieldProperty52);
            fieldDisplaySettings2.Table = typeof(MediaPortal.Plugins.MovingPictures.Database.DBMovieInfo);
            this.filterEditorPane1.FieldDisplaySettings = fieldDisplaySettings2;
            this.filterEditorPane1.Location = new System.Drawing.Point(0, 0);
            this.filterEditorPane1.Name = "filterEditorPane1";
            this.filterEditorPane1.Size = new System.Drawing.Size(710, 376);
            this.filterEditorPane1.TabIndex = 0;
            // 
            // MovieFilterEditorPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(710, 376);
            this.ControlBox = false;
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.filterEditorPane1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MovieFilterEditorPopup";
            this.ShowInTaskbar = false;
            this.Text = "Filter Editor";
            this.Load += new System.EventHandler(this.TesterFrame_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private FilterEditorPane filterEditorPane1;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button helpButton;
        private System.Windows.Forms.Button cancelButton;




    }
}