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
            Cornerstone.GUI.Controls.FieldDisplaySettings fieldDisplaySettings1 = new Cornerstone.GUI.Controls.FieldDisplaySettings();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty1 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty2 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty3 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty4 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty5 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty6 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty7 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty8 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty9 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty10 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty11 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty12 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty13 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty14 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty15 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty16 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty17 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty18 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty19 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty20 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty21 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty22 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty23 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty24 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty25 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty26 = new Cornerstone.GUI.Controls.FieldProperty();
            this.okButton = new System.Windows.Forms.Button();
            this.filterEditorPane1 = new Cornerstone.GUI.Filtering.FilterEditorPane();
            this.helpButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(609, 345);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // filterEditorPane1
            // 
            this.filterEditorPane1.DBManager = null;
            this.filterEditorPane1.DisplayName = "movies";
            this.filterEditorPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            fieldProperty1.ColumnWidth = 150;
            fieldProperty1.DisplayName = "Title";
            fieldProperty1.FieldName = "Title";
            fieldProperty2.ColumnWidth = null;
            fieldProperty2.DisplayName = "Alternate Titles";
            fieldProperty2.FieldName = "AlternateTitles";
            fieldProperty2.Visible = false;
            fieldProperty3.ColumnWidth = null;
            fieldProperty3.DisplayName = "Sort By";
            fieldProperty3.FieldName = "SortBy";
            fieldProperty3.Visible = false;
            fieldProperty4.ColumnWidth = null;
            fieldProperty4.DisplayName = "Directors";
            fieldProperty4.FieldName = "Directors";
            fieldProperty4.Visible = false;
            fieldProperty5.ColumnWidth = null;
            fieldProperty5.DisplayName = "Writers";
            fieldProperty5.FieldName = "Writers";
            fieldProperty5.Visible = false;
            fieldProperty6.ColumnWidth = null;
            fieldProperty6.DisplayName = "Actors";
            fieldProperty6.FieldName = "Actors";
            fieldProperty6.Visible = false;
            fieldProperty7.ColumnWidth = 40;
            fieldProperty7.DisplayName = "Year";
            fieldProperty7.FieldName = "Year";
            fieldProperty8.ColumnWidth = null;
            fieldProperty8.DisplayName = "Genres";
            fieldProperty8.FieldName = "Genres";
            fieldProperty8.Visible = false;
            fieldProperty9.ColumnWidth = null;
            fieldProperty9.DisplayName = "Certification";
            fieldProperty9.FieldName = "Certification";
            fieldProperty9.Visible = false;
            fieldProperty10.ColumnWidth = null;
            fieldProperty10.DisplayName = "Language";
            fieldProperty10.FieldName = "Language";
            fieldProperty10.Visible = false;
            fieldProperty11.ColumnWidth = null;
            fieldProperty11.DisplayName = "Tagline";
            fieldProperty11.FieldName = "Tagline";
            fieldProperty11.Visible = false;
            fieldProperty12.ColumnWidth = null;
            fieldProperty12.DisplayName = "Summary";
            fieldProperty12.FieldName = "Summary";
            fieldProperty12.Visible = false;
            fieldProperty13.ColumnWidth = null;
            fieldProperty13.DisplayName = "Score";
            fieldProperty13.FieldName = "Score";
            fieldProperty13.Visible = false;
            fieldProperty14.ColumnWidth = null;
            fieldProperty14.DisplayName = "Popularity";
            fieldProperty14.FieldName = "Popularity";
            fieldProperty14.Visible = false;
            fieldProperty15.ColumnWidth = null;
            fieldProperty15.DisplayName = "Date Added";
            fieldProperty15.FieldName = "DateAdded";
            fieldProperty15.Visible = false;
            fieldProperty16.ColumnWidth = null;
            fieldProperty16.DisplayName = "Runtime";
            fieldProperty16.FieldName = "Runtime";
            fieldProperty16.Visible = false;
            fieldProperty17.ColumnWidth = null;
            fieldProperty17.DisplayName = "Movie Xml ID";
            fieldProperty17.FieldName = "MovieXmlID";
            fieldProperty17.Visible = false;
            fieldProperty18.ColumnWidth = null;
            fieldProperty18.DisplayName = "Imdb ID";
            fieldProperty18.FieldName = "ImdbID";
            fieldProperty18.Visible = false;
            fieldProperty19.ColumnWidth = null;
            fieldProperty19.DisplayName = "Alternate Covers";
            fieldProperty19.FieldName = "AlternateCovers";
            fieldProperty19.Visible = false;
            fieldProperty20.ColumnWidth = null;
            fieldProperty20.DisplayName = "Cover Full Path";
            fieldProperty20.FieldName = "CoverFullPath";
            fieldProperty20.Visible = false;
            fieldProperty21.ColumnWidth = null;
            fieldProperty21.DisplayName = "Cover Thumb Full Path";
            fieldProperty21.FieldName = "CoverThumbFullPath";
            fieldProperty21.Visible = false;
            fieldProperty22.ColumnWidth = null;
            fieldProperty22.DisplayName = "Backdrop Full Path";
            fieldProperty22.FieldName = "BackdropFullPath";
            fieldProperty22.Visible = false;
            fieldProperty23.ColumnWidth = null;
            fieldProperty23.DisplayName = "Details URL";
            fieldProperty23.FieldName = "DetailsURL";
            fieldProperty23.Visible = false;
            fieldProperty24.ColumnWidth = null;
            fieldProperty24.DisplayName = "Primary Source";
            fieldProperty24.FieldName = "PrimarySource";
            fieldProperty24.Visible = false;
            fieldProperty25.ColumnWidth = null;
            fieldProperty25.DisplayName = "Original Directory Name";
            fieldProperty25.FieldName = "OriginalDirectoryName";
            fieldProperty25.Visible = false;
            fieldProperty26.ColumnWidth = null;
            fieldProperty26.DisplayName = "Mps Id";
            fieldProperty26.FieldName = "MpsId";
            fieldProperty26.Visible = false;
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty1);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty2);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty3);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty4);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty5);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty6);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty7);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty8);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty9);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty10);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty11);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty12);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty13);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty14);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty15);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty16);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty17);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty18);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty19);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty20);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty21);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty22);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty23);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty24);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty25);
            fieldDisplaySettings1.FieldProperties.Add(fieldProperty26);
            fieldDisplaySettings1.Table = typeof(MediaPortal.Plugins.MovingPictures.Database.DBMovieInfo);
            this.filterEditorPane1.FieldDisplaySettings = fieldDisplaySettings1;
            this.filterEditorPane1.Location = new System.Drawing.Point(0, 0);
            this.filterEditorPane1.Name = "filterEditorPane1";
            this.filterEditorPane1.Size = new System.Drawing.Size(710, 376);
            this.filterEditorPane1.TabIndex = 0;
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
            // MovieFilterEditorPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(710, 376);
            this.ControlBox = false;
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




    }
}