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
            Cornerstone.GUI.Controls.FieldDisplaySettings fieldDisplaySettings3 = new Cornerstone.GUI.Controls.FieldDisplaySettings();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty49 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty50 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty51 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty52 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty53 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty54 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty55 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty56 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty57 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty58 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty59 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty60 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty61 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty62 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty63 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty64 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty65 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty66 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty67 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty68 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty69 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty70 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty71 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty72 = new Cornerstone.GUI.Controls.FieldProperty();
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
            fieldProperty49.ColumnWidth = 150;
            fieldProperty49.DisplayName = "Title";
            fieldProperty49.FieldName = "Title";
            fieldProperty50.ColumnWidth = null;
            fieldProperty50.DisplayName = "Alternate Titles";
            fieldProperty50.FieldName = "AlternateTitles";
            fieldProperty50.Visible = false;
            fieldProperty51.ColumnWidth = null;
            fieldProperty51.DisplayName = "Sort By";
            fieldProperty51.FieldName = "SortBy";
            fieldProperty51.Visible = false;
            fieldProperty52.ColumnWidth = null;
            fieldProperty52.DisplayName = "Directors";
            fieldProperty52.FieldName = "Directors";
            fieldProperty52.Visible = false;
            fieldProperty53.ColumnWidth = null;
            fieldProperty53.DisplayName = "Writers";
            fieldProperty53.FieldName = "Writers";
            fieldProperty53.Visible = false;
            fieldProperty54.ColumnWidth = null;
            fieldProperty54.DisplayName = "Actors";
            fieldProperty54.FieldName = "Actors";
            fieldProperty54.Visible = false;
            fieldProperty55.ColumnWidth = 40;
            fieldProperty55.DisplayName = "Year";
            fieldProperty55.FieldName = "Year";
            fieldProperty56.ColumnWidth = null;
            fieldProperty56.DisplayName = "Genres";
            fieldProperty56.FieldName = "Genres";
            fieldProperty56.Visible = false;
            fieldProperty57.ColumnWidth = null;
            fieldProperty57.DisplayName = "Certification";
            fieldProperty57.FieldName = "Certification";
            fieldProperty57.Visible = false;
            fieldProperty58.ColumnWidth = null;
            fieldProperty58.DisplayName = "Language";
            fieldProperty58.FieldName = "Language";
            fieldProperty58.Visible = false;
            fieldProperty59.ColumnWidth = null;
            fieldProperty59.DisplayName = "Tagline";
            fieldProperty59.FieldName = "Tagline";
            fieldProperty59.Visible = false;
            fieldProperty60.ColumnWidth = null;
            fieldProperty60.DisplayName = "Summary";
            fieldProperty60.FieldName = "Summary";
            fieldProperty60.Visible = false;
            fieldProperty61.ColumnWidth = null;
            fieldProperty61.DisplayName = "Score";
            fieldProperty61.FieldName = "Score";
            fieldProperty61.Visible = false;
            fieldProperty62.ColumnWidth = null;
            fieldProperty62.DisplayName = "Popularity";
            fieldProperty62.FieldName = "Popularity";
            fieldProperty62.Visible = false;
            fieldProperty63.ColumnWidth = null;
            fieldProperty63.DisplayName = "Date Added";
            fieldProperty63.FieldName = "DateAdded";
            fieldProperty63.Visible = false;
            fieldProperty64.ColumnWidth = null;
            fieldProperty64.DisplayName = "Runtime";
            fieldProperty64.FieldName = "Runtime";
            fieldProperty64.Visible = false;
            fieldProperty65.ColumnWidth = null;
            fieldProperty65.DisplayName = "Movie Xml ID";
            fieldProperty65.FieldName = "MovieXmlID";
            fieldProperty65.Visible = false;
            fieldProperty66.ColumnWidth = null;
            fieldProperty66.DisplayName = "Imdb ID";
            fieldProperty66.FieldName = "ImdbID";
            fieldProperty66.Visible = false;
            fieldProperty67.ColumnWidth = null;
            fieldProperty67.DisplayName = "Alternate Covers";
            fieldProperty67.FieldName = "AlternateCovers";
            fieldProperty67.Visible = false;
            fieldProperty68.ColumnWidth = null;
            fieldProperty68.DisplayName = "Cover Full Path";
            fieldProperty68.FieldName = "CoverFullPath";
            fieldProperty68.Visible = false;
            fieldProperty69.ColumnWidth = null;
            fieldProperty69.DisplayName = "Cover Thumb Full Path";
            fieldProperty69.FieldName = "CoverThumbFullPath";
            fieldProperty69.Visible = false;
            fieldProperty70.ColumnWidth = null;
            fieldProperty70.DisplayName = "Backdrop Full Path";
            fieldProperty70.FieldName = "BackdropFullPath";
            fieldProperty70.Visible = false;
            fieldProperty71.ColumnWidth = null;
            fieldProperty71.DisplayName = "Details URL";
            fieldProperty71.FieldName = "DetailsURL";
            fieldProperty71.Visible = false;
            fieldProperty72.ColumnWidth = null;
            fieldProperty72.DisplayName = "Primary Source";
            fieldProperty72.FieldName = "PrimarySource";
            fieldProperty72.Visible = false;
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty49);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty50);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty51);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty52);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty53);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty54);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty55);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty56);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty57);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty58);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty59);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty60);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty61);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty62);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty63);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty64);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty65);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty66);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty67);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty68);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty69);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty70);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty71);
            fieldDisplaySettings3.FieldProperties.Add(fieldProperty72);
            fieldDisplaySettings3.Table = typeof(MediaPortal.Plugins.MovingPictures.Database.DBMovieInfo);
            this.filterEditorPane1.FieldDisplaySettings = fieldDisplaySettings3;
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