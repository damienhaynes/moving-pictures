namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    partial class SingleMovieImporterPopup {
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
            this.components = new System.ComponentModel.Container();
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
            this.fileListBox = new System.Windows.Forms.ListBox();
            this.possibleMatchesCombo = new System.Windows.Forms.ComboBox();
            this.rescanButton = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.okButton = new System.Windows.Forms.Button();
            this.descriptionLabel = new System.Windows.Forms.TextBox();
            this.detailsLabel = new System.Windows.Forms.Label();
            this.comboLabel = new System.Windows.Forms.Label();
            this.filesLabel = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.movieDetails = new Cornerstone.GUI.Controls.DBObjectEditor();
            this.statusStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // fileListBox
            // 
            this.fileListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.fileListBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileListBox.FormattingEnabled = true;
            this.fileListBox.ItemHeight = 14;
            this.fileListBox.Location = new System.Drawing.Point(2, 67);
            this.fileListBox.Name = "fileListBox";
            this.fileListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.fileListBox.Size = new System.Drawing.Size(340, 44);
            this.fileListBox.TabIndex = 9;
            // 
            // possibleMatchesCombo
            // 
            this.possibleMatchesCombo.FormattingEnabled = true;
            this.possibleMatchesCombo.Location = new System.Drawing.Point(2, 130);
            this.possibleMatchesCombo.Name = "possibleMatchesCombo";
            this.possibleMatchesCombo.Size = new System.Drawing.Size(313, 21);
            this.possibleMatchesCombo.TabIndex = 10;
            this.possibleMatchesCombo.SelectedIndexChanged += new System.EventHandler(this.possibleMatchesCombo_SelectedIndexChanged);
            // 
            // rescanButton
            // 
            this.rescanButton.BackgroundImage = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.arrow_rotate_clockwise;
            this.rescanButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.rescanButton.Location = new System.Drawing.Point(321, 130);
            this.rescanButton.Name = "rescanButton";
            this.rescanButton.Size = new System.Drawing.Size(21, 21);
            this.rescanButton.TabIndex = 11;
            this.toolTip1.SetToolTip(this.rescanButton, "Rescan for possible matches using a new search string.");
            this.rescanButton.UseVisualStyleBackColor = true;
            this.rescanButton.Click += new System.EventHandler(this.rescanButton_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 315);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(371, 22);
            this.statusStrip1.TabIndex = 12;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(152, 17);
            this.statusLabel.Text = "Retrieving Possible Matches...";
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(264, 267);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(78, 23);
            this.okButton.TabIndex = 13;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.descriptionLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.descriptionLabel.Location = new System.Drawing.Point(3, 4);
            this.descriptionLabel.Multiline = true;
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.ReadOnly = true;
            this.descriptionLabel.Size = new System.Drawing.Size(335, 44);
            this.descriptionLabel.TabIndex = 14;
            this.descriptionLabel.Text = "Please select the movie from the drop-down list that matches the files listed bel" +
                "ow. To search for additional possible matches, type a movie title in the drop-do" +
                "wn box and click the refresh button.";
            // 
            // detailsLabel
            // 
            this.detailsLabel.AutoSize = true;
            this.detailsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.detailsLabel.Location = new System.Drawing.Point(3, 154);
            this.detailsLabel.Name = "detailsLabel";
            this.detailsLabel.Size = new System.Drawing.Size(88, 13);
            this.detailsLabel.TabIndex = 17;
            this.detailsLabel.Text = "Movie Details:";
            // 
            // comboLabel
            // 
            this.comboLabel.AutoSize = true;
            this.comboLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboLabel.Location = new System.Drawing.Point(3, 114);
            this.comboLabel.Name = "comboLabel";
            this.comboLabel.Size = new System.Drawing.Size(45, 13);
            this.comboLabel.TabIndex = 18;
            this.comboLabel.Text = "Movie:";
            // 
            // filesLabel
            // 
            this.filesLabel.AutoSize = true;
            this.filesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.filesLabel.Location = new System.Drawing.Point(3, 51);
            this.filesLabel.Name = "filesLabel";
            this.filesLabel.Size = new System.Drawing.Size(37, 13);
            this.filesLabel.TabIndex = 19;
            this.filesLabel.Text = "Files:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.possibleMatchesCombo);
            this.panel1.Controls.Add(this.fileListBox);
            this.panel1.Controls.Add(this.movieDetails);
            this.panel1.Controls.Add(this.descriptionLabel);
            this.panel1.Controls.Add(this.filesLabel);
            this.panel1.Controls.Add(this.okButton);
            this.panel1.Controls.Add(this.comboLabel);
            this.panel1.Controls.Add(this.detailsLabel);
            this.panel1.Controls.Add(this.rescanButton);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(345, 296);
            this.panel1.TabIndex = 20;
            // 
            // movieDetails
            // 
            this.movieDetails.DatabaseObject = null;
            fieldProperty1.DisplayName = "Name";
            fieldProperty1.FieldName = "Name";
            fieldProperty1.ReadOnly = true;
            fieldProperty1.Visible = false;
            fieldProperty2.DisplayName = "Sort Name";
            fieldProperty2.FieldName = "SortName";
            fieldProperty2.Visible = false;
            fieldProperty3.DisplayName = "Directors";
            fieldProperty3.FieldName = "Directors";
            fieldProperty3.ReadOnly = true;
            fieldProperty4.DisplayName = "Writers";
            fieldProperty4.FieldName = "Writers";
            fieldProperty4.Visible = false;
            fieldProperty5.DisplayName = "Actors";
            fieldProperty5.FieldName = "Actors";
            fieldProperty5.Visible = false;
            fieldProperty6.DisplayName = "Year";
            fieldProperty6.FieldName = "Year";
            fieldProperty6.ReadOnly = true;
            fieldProperty7.DisplayName = "Genres";
            fieldProperty7.FieldName = "Genres";
            fieldProperty7.Visible = false;
            fieldProperty8.DisplayName = "Certification";
            fieldProperty8.FieldName = "Certification";
            fieldProperty8.Visible = false;
            fieldProperty9.DisplayName = "Language";
            fieldProperty9.FieldName = "Language";
            fieldProperty9.Visible = false;
            fieldProperty10.DisplayName = "Tagline";
            fieldProperty10.FieldName = "Tagline";
            fieldProperty10.Visible = false;
            fieldProperty11.DisplayName = "Summary";
            fieldProperty11.FieldName = "Summary";
            fieldProperty11.Visible = false;
            fieldProperty12.DisplayName = "Score";
            fieldProperty12.FieldName = "Score";
            fieldProperty12.Visible = false;
            fieldProperty13.DisplayName = "Trailer Link";
            fieldProperty13.FieldName = "TrailerLink";
            fieldProperty13.Visible = false;
            fieldProperty14.DisplayName = "Poster Url";
            fieldProperty14.FieldName = "PosterUrl";
            fieldProperty14.Visible = false;
            fieldProperty15.DisplayName = "Runtime";
            fieldProperty15.FieldName = "Runtime";
            fieldProperty15.ReadOnly = true;
            fieldProperty16.DisplayName = "Movie Xml ID";
            fieldProperty16.FieldName = "MovieXmlID";
            fieldProperty16.Visible = false;
            fieldProperty17.DisplayName = "Imdb ID";
            fieldProperty17.FieldName = "ImdbID";
            fieldProperty17.ReadOnly = true;
            fieldProperty18.DisplayName = "Local Media";
            fieldProperty18.FieldName = "LocalMedia";
            fieldProperty18.Visible = false;
            fieldProperty19.DisplayName = "Alternate Covers";
            fieldProperty19.FieldName = "AlternateCovers";
            fieldProperty19.Visible = false;
            fieldProperty20.DisplayName = "Cover Full Path";
            fieldProperty20.FieldName = "CoverFullPath";
            fieldProperty20.Visible = false;
            fieldProperty21.DisplayName = "Cover Thumb Full Path";
            fieldProperty21.FieldName = "CoverThumbFullPath";
            fieldProperty21.Visible = false;
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty1);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty2);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty3);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty4);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty5);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty6);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty7);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty8);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty9);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty10);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty11);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty12);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty13);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty14);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty15);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty16);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty17);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty18);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty19);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty20);
            this.movieDetails.FieldDisplaySettings.FieldProperties.Add(fieldProperty21);
            this.movieDetails.Location = new System.Drawing.Point(2, 170);
            this.movieDetails.Name = "movieDetails";
            this.movieDetails.Size = new System.Drawing.Size(340, 91);
            this.movieDetails.TabIndex = 16;
            this.movieDetails.FieldDisplaySettings.Table = typeof(MediaPortal.Plugins.MovingPictures.Database.DBMovieInfo);
            // 
            // SingleMovieImporterPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(371, 337);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "SingleMovieImporterPopup";
            this.Text = "Single Movie Importer";
            this.Load += new System.EventHandler(this.SingleMovieImporterPopup_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox fileListBox;
        private System.Windows.Forms.ComboBox possibleMatchesCombo;
        private System.Windows.Forms.Button rescanButton;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.TextBox descriptionLabel;
        private Cornerstone.GUI.Controls.DBObjectEditor movieDetails;
        private System.Windows.Forms.Label detailsLabel;
        private System.Windows.Forms.Label comboLabel;
        private System.Windows.Forms.Label filesLabel;
        private System.Windows.Forms.Panel panel1;
    }
}