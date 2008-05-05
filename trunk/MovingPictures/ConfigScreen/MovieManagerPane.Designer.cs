namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    partial class MovieManagerPane {
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
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty1 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty2 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty3 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty4 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty5 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty6 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty7 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty8 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty9 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty10 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty11 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty12 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty13 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty14 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty15 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty16 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty17 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty18 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty19 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty20 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty fieldProperty21 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.FieldProperty();
            this.movieTree = new System.Windows.Forms.TreeView();
            this.coverPanel = new System.Windows.Forms.Panel();
            this.coverImage = new System.Windows.Forms.PictureBox();
            this.resolutionLabel = new System.Windows.Forms.Label();
            this.coverNumLabel = new System.Windows.Forms.Label();
            this.coverToolStrip = new System.Windows.Forms.ToolStrip();
            this.previousCoverButton = new System.Windows.Forms.ToolStripButton();
            this.nextCoverButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.zoomButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.refreshArtButton = new System.Windows.Forms.ToolStripButton();
            this.loadNewCoverButton = new System.Windows.Forms.ToolStripSplitButton();
            this.loadCoverArtFromFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadCoverArtFromURLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteButton = new System.Windows.Forms.ToolStripButton();
            this.coverArtFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.dbObjectList1 = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.DBObjectList();
            this.movieTitleTextBox = new MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.DBTextBox();
            this.coverPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.coverImage)).BeginInit();
            this.coverToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // movieTree
            // 
            this.movieTree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.movieTree.BackColor = System.Drawing.SystemColors.Window;
            this.movieTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.movieTree.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.movieTree.FullRowSelect = true;
            this.movieTree.HideSelection = false;
            this.movieTree.Location = new System.Drawing.Point(3, 3);
            this.movieTree.Name = "movieTree";
            this.movieTree.ShowRootLines = false;
            this.movieTree.Size = new System.Drawing.Size(178, 292);
            this.movieTree.TabIndex = 2;
            this.movieTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.movieTree_AfterSelect);
            this.movieTree.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.movieTree_BeforeSelect);
            // 
            // coverPanel
            // 
            this.coverPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.coverPanel.Controls.Add(this.coverImage);
            this.coverPanel.Controls.Add(this.resolutionLabel);
            this.coverPanel.Controls.Add(this.coverNumLabel);
            this.coverPanel.Controls.Add(this.coverToolStrip);
            this.coverPanel.Location = new System.Drawing.Point(3, 311);
            this.coverPanel.Name = "coverPanel";
            this.coverPanel.Size = new System.Drawing.Size(177, 301);
            this.coverPanel.TabIndex = 5;
            // 
            // coverImage
            // 
            this.coverImage.BackColor = System.Drawing.SystemColors.ControlDark;
            this.coverImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.coverImage.Location = new System.Drawing.Point(1, 39);
            this.coverImage.MaximumSize = new System.Drawing.Size(175, 260);
            this.coverImage.Name = "coverImage";
            this.coverImage.Size = new System.Drawing.Size(175, 260);
            this.coverImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.coverImage.TabIndex = 0;
            this.coverImage.TabStop = false;
            this.coverImage.DoubleClick += new System.EventHandler(this.coverImage_DoubleClick);
            // 
            // resolutionLabel
            // 
            this.resolutionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.resolutionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resolutionLabel.Location = new System.Drawing.Point(91, 25);
            this.resolutionLabel.Name = "resolutionLabel";
            this.resolutionLabel.Size = new System.Drawing.Size(87, 11);
            this.resolutionLabel.TabIndex = 3;
            this.resolutionLabel.Text = "419 x 600";
            this.resolutionLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // coverNumLabel
            // 
            this.coverNumLabel.AutoSize = true;
            this.coverNumLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.coverNumLabel.Location = new System.Drawing.Point(1, 27);
            this.coverNumLabel.Name = "coverNumLabel";
            this.coverNumLabel.Size = new System.Drawing.Size(22, 12);
            this.coverNumLabel.TabIndex = 4;
            this.coverNumLabel.Text = "1 / 3";
            // 
            // coverToolStrip
            // 
            this.coverToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.coverToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.previousCoverButton,
            this.nextCoverButton,
            this.toolStripSeparator1,
            this.zoomButton,
            this.toolStripSeparator2,
            this.refreshArtButton,
            this.loadNewCoverButton,
            this.toolStripSeparator3,
            this.deleteButton});
            this.coverToolStrip.Location = new System.Drawing.Point(0, 0);
            this.coverToolStrip.Name = "coverToolStrip";
            this.coverToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.coverToolStrip.Size = new System.Drawing.Size(177, 25);
            this.coverToolStrip.TabIndex = 2;
            // 
            // previousCoverButton
            // 
            this.previousCoverButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.previousCoverButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.resultset_previous;
            this.previousCoverButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.previousCoverButton.Name = "previousCoverButton";
            this.previousCoverButton.Size = new System.Drawing.Size(23, 22);
            this.previousCoverButton.Text = "toolStripButton1";
            this.previousCoverButton.ToolTipText = "Previous Cover";
            this.previousCoverButton.Click += new System.EventHandler(this.previousCoverButton_Click);
            // 
            // nextCoverButton
            // 
            this.nextCoverButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.nextCoverButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.resultset_next;
            this.nextCoverButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.nextCoverButton.Name = "nextCoverButton";
            this.nextCoverButton.Size = new System.Drawing.Size(23, 22);
            this.nextCoverButton.Text = "toolStripButton2";
            this.nextCoverButton.ToolTipText = "Next Cover";
            this.nextCoverButton.Click += new System.EventHandler(this.nextCoverButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // zoomButton
            // 
            this.zoomButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.zoom;
            this.zoomButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.zoomButton.Name = "zoomButton";
            this.zoomButton.Size = new System.Drawing.Size(23, 22);
            this.zoomButton.Text = "toolStripButton1";
            this.zoomButton.ToolTipText = "See Full Size Cover Art";
            this.zoomButton.Click += new System.EventHandler(this.zoomButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // refreshArtButton
            // 
            this.refreshArtButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.refreshArtButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.arrow_rotate_clockwise;
            this.refreshArtButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.refreshArtButton.Name = "refreshArtButton";
            this.refreshArtButton.Size = new System.Drawing.Size(23, 22);
            this.refreshArtButton.Text = "toolStripButton2";
            this.refreshArtButton.ToolTipText = "Refesh Cover Art Selection from Online Data Sources";
            // 
            // loadNewCoverButton
            // 
            this.loadNewCoverButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.loadNewCoverButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadCoverArtFromFileToolStripMenuItem,
            this.loadCoverArtFromURLToolStripMenuItem});
            this.loadNewCoverButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.folder_image;
            this.loadNewCoverButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.loadNewCoverButton.Name = "loadNewCoverButton";
            this.loadNewCoverButton.Size = new System.Drawing.Size(32, 22);
            this.loadNewCoverButton.ToolTipText = "Load Cover Art From File";
            this.loadNewCoverButton.ButtonClick += new System.EventHandler(this.loadNewCoverButton_ButtonClick);
            // 
            // loadCoverArtFromFileToolStripMenuItem
            // 
            this.loadCoverArtFromFileToolStripMenuItem.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.folder_image;
            this.loadCoverArtFromFileToolStripMenuItem.Name = "loadCoverArtFromFileToolStripMenuItem";
            this.loadCoverArtFromFileToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.loadCoverArtFromFileToolStripMenuItem.Text = "Load Cover Art From File";
            this.loadCoverArtFromFileToolStripMenuItem.Click += new System.EventHandler(this.loadCoverArtFromFileToolStripMenuItem_Click);
            // 
            // loadCoverArtFromURLToolStripMenuItem
            // 
            this.loadCoverArtFromURLToolStripMenuItem.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.world_add;
            this.loadCoverArtFromURLToolStripMenuItem.Name = "loadCoverArtFromURLToolStripMenuItem";
            this.loadCoverArtFromURLToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.loadCoverArtFromURLToolStripMenuItem.Text = "Load Cover Art From URL";
            this.loadCoverArtFromURLToolStripMenuItem.Click += new System.EventHandler(this.loadCoverArtFromURLToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // deleteButton
            // 
            this.deleteButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.deleteButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.cross;
            this.deleteButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(23, 22);
            this.deleteButton.Text = "toolStripButton1";
            this.deleteButton.ToolTipText = "Delete Current Cover Art";
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // coverArtFileDialog
            // 
            this.coverArtFileDialog.Filter = "\"Image Files|*.jpg;*.png;*.bmp;*.gif\"";
            // 
            // dbObjectList1
            // 
            this.dbObjectList1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dbObjectList1.DatabaseObject = null;
            fieldProperty1.DisplayName = "Name";
            fieldProperty1.FieldName = "Name";
            fieldProperty1.Visible = false;
            fieldProperty2.DisplayName = "Sort Name";
            fieldProperty2.FieldName = "SortName";
            fieldProperty3.DisplayName = "Directors";
            fieldProperty3.FieldName = "Directors";
            fieldProperty4.DisplayName = "Writers";
            fieldProperty4.FieldName = "Writers";
            fieldProperty5.DisplayName = "Actors";
            fieldProperty5.FieldName = "Actors";
            fieldProperty6.DisplayName = "Year";
            fieldProperty6.FieldName = "Year";
            fieldProperty7.DisplayName = "Genres";
            fieldProperty7.FieldName = "Genres";
            fieldProperty8.DisplayName = "Certification";
            fieldProperty8.FieldName = "Certification";
            fieldProperty9.DisplayName = "Language";
            fieldProperty9.FieldName = "Language";
            fieldProperty10.DisplayName = "Tagline";
            fieldProperty10.FieldName = "Tagline";
            fieldProperty11.DisplayName = "Summary";
            fieldProperty11.FieldName = "Summary";
            fieldProperty12.DisplayName = "Score";
            fieldProperty12.FieldName = "Score";
            fieldProperty13.DisplayName = "Trailer Link";
            fieldProperty13.FieldName = "TrailerLink";
            fieldProperty13.Visible = false;
            fieldProperty14.DisplayName = "Poster Url";
            fieldProperty14.FieldName = "PosterUrl";
            fieldProperty14.Visible = false;
            fieldProperty15.DisplayName = "Runtime";
            fieldProperty15.FieldName = "Runtime";
            fieldProperty16.DisplayName = "Movie Xml ID";
            fieldProperty16.FieldName = "MovieXmlID";
            fieldProperty16.Visible = false;
            fieldProperty17.DisplayName = "IMDb ID";
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
            this.dbObjectList1.FieldProperties.Add(fieldProperty1);
            this.dbObjectList1.FieldProperties.Add(fieldProperty2);
            this.dbObjectList1.FieldProperties.Add(fieldProperty3);
            this.dbObjectList1.FieldProperties.Add(fieldProperty4);
            this.dbObjectList1.FieldProperties.Add(fieldProperty5);
            this.dbObjectList1.FieldProperties.Add(fieldProperty6);
            this.dbObjectList1.FieldProperties.Add(fieldProperty7);
            this.dbObjectList1.FieldProperties.Add(fieldProperty8);
            this.dbObjectList1.FieldProperties.Add(fieldProperty9);
            this.dbObjectList1.FieldProperties.Add(fieldProperty10);
            this.dbObjectList1.FieldProperties.Add(fieldProperty11);
            this.dbObjectList1.FieldProperties.Add(fieldProperty12);
            this.dbObjectList1.FieldProperties.Add(fieldProperty13);
            this.dbObjectList1.FieldProperties.Add(fieldProperty14);
            this.dbObjectList1.FieldProperties.Add(fieldProperty15);
            this.dbObjectList1.FieldProperties.Add(fieldProperty16);
            this.dbObjectList1.FieldProperties.Add(fieldProperty17);
            this.dbObjectList1.FieldProperties.Add(fieldProperty18);
            this.dbObjectList1.FieldProperties.Add(fieldProperty19);
            this.dbObjectList1.FieldProperties.Add(fieldProperty20);
            this.dbObjectList1.FieldProperties.Add(fieldProperty21);
            this.dbObjectList1.Location = new System.Drawing.Point(188, 39);
            this.dbObjectList1.Name = "dbObjectList1";
            this.dbObjectList1.Size = new System.Drawing.Size(466, 571);
            this.dbObjectList1.TabIndex = 8;
            this.dbObjectList1.Table = typeof(MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables.DBMovieInfo);
            // 
            // movieTitleTextBox
            // 
            this.movieTitleTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.movieTitleTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.movieTitleTextBox.DatabaseField = null;
            this.movieTitleTextBox.DatabaseObject = null;
            this.movieTitleTextBox.Font = new System.Drawing.Font("Palatino Linotype", 15.75F, System.Drawing.FontStyle.Bold);
            this.movieTitleTextBox.ForeColor = System.Drawing.Color.Red;
            this.movieTitleTextBox.Location = new System.Drawing.Point(187, 3);
            this.movieTitleTextBox.Name = "movieTitleTextBox";
            this.movieTitleTextBox.Size = new System.Drawing.Size(327, 29);
            this.movieTitleTextBox.TabIndex = 7;
            this.movieTitleTextBox.Text = "Back to the Future";
            // 
            // MovieManagerPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dbObjectList1);
            this.Controls.Add(this.movieTitleTextBox);
            this.Controls.Add(this.coverPanel);
            this.Controls.Add(this.movieTree);
            this.Name = "MovieManagerPane";
            this.Size = new System.Drawing.Size(657, 614);
            this.Load += new System.EventHandler(this.MovieManagerPane_Load);
            this.coverPanel.ResumeLayout(false);
            this.coverPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.coverImage)).EndInit();
            this.coverToolStrip.ResumeLayout(false);
            this.coverToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView movieTree;
        private System.Windows.Forms.ToolStrip coverToolStrip;
        private System.Windows.Forms.ToolStripButton previousCoverButton;
        private System.Windows.Forms.ToolStripButton nextCoverButton;
        private System.Windows.Forms.PictureBox coverImage;
        private System.Windows.Forms.Label resolutionLabel;
        private System.Windows.Forms.Panel coverPanel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton zoomButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton refreshArtButton;
        private System.Windows.Forms.Label coverNumLabel;
        private System.Windows.Forms.ToolStripSplitButton loadNewCoverButton;
        private System.Windows.Forms.ToolStripMenuItem loadCoverArtFromFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadCoverArtFromURLToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog coverArtFileDialog;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton deleteButton;
        private MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.DBTextBox movieTitleTextBox;
        private MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls.DBObjectList dbObjectList1;
    }
}
