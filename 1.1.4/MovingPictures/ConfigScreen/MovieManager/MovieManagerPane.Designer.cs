using System.Windows.Forms;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MovieManagerPane));
            this.coverPanel = new System.Windows.Forms.Panel();
            this.artworkProgressBar = new System.Windows.Forms.ProgressBar();
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
            this.deleteCoverButton = new System.Windows.Forms.ToolStripButton();
            this.coverArtFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.movieToolStrip = new System.Windows.Forms.ToolStrip();
            this.detailsViewDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            this.movieDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.updateMediaInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendToImporterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteMovieButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.refreshMovieButton = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.unwatchedToggleButton = new System.Windows.Forms.ToolStripButton();
            this.watchedToggleButton = new System.Windows.Forms.ToolStripButton();
            this.playMovieButton = new System.Windows.Forms.ToolStripButton();
            this.movieListBox = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.titleLabel = new System.Windows.Forms.Label();
            this.movieDetailsSubPane = new MediaPortal.Plugins.MovingPictures.ConfigScreen.MovieManager.MovieDetailsSubPane();
            this.fileDetailsSubPane = new MediaPortal.Plugins.MovingPictures.ConfigScreen.MovieManager.FileDetailsSubPane();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.titleSortByFieldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dateDateAddedFieldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameFilesAndFoldersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.revertToOriginalNamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.coverPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.coverImage)).BeginInit();
            this.coverToolStrip.SuspendLayout();
            this.movieToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // coverPanel
            // 
            this.coverPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.coverPanel.Controls.Add(this.artworkProgressBar);
            this.coverPanel.Controls.Add(this.coverImage);
            this.coverPanel.Controls.Add(this.resolutionLabel);
            this.coverPanel.Controls.Add(this.coverNumLabel);
            this.coverPanel.Controls.Add(this.coverToolStrip);
            this.coverPanel.Location = new System.Drawing.Point(3, 294);
            this.coverPanel.Name = "coverPanel";
            this.coverPanel.Size = new System.Drawing.Size(177, 303);
            this.coverPanel.TabIndex = 5;
            // 
            // artworkProgressBar
            // 
            this.artworkProgressBar.Location = new System.Drawing.Point(0, 23);
            this.artworkProgressBar.Name = "artworkProgressBar";
            this.artworkProgressBar.Size = new System.Drawing.Size(176, 18);
            this.artworkProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.artworkProgressBar.TabIndex = 5;
            this.artworkProgressBar.Visible = false;
            // 
            // coverImage
            // 
            this.coverImage.BackColor = System.Drawing.SystemColors.ControlDark;
            this.coverImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.coverImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.coverImage.Location = new System.Drawing.Point(0, 25);
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
            this.resolutionLabel.Location = new System.Drawing.Point(90, 284);
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
            this.coverNumLabel.Location = new System.Drawing.Point(0, 286);
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
            this.deleteCoverButton});
            this.coverToolStrip.Location = new System.Drawing.Point(0, 0);
            this.coverToolStrip.Name = "coverToolStrip";
            this.coverToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
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
            this.refreshArtButton.Click += new System.EventHandler(this.refreshArtButton_Click);
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
            this.loadCoverArtFromFileToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.loadCoverArtFromFileToolStripMenuItem.Text = "Load Cover Art From File";
            this.loadCoverArtFromFileToolStripMenuItem.Click += new System.EventHandler(this.loadCoverArtFromFileToolStripMenuItem_Click);
            // 
            // loadCoverArtFromURLToolStripMenuItem
            // 
            this.loadCoverArtFromURLToolStripMenuItem.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.world_add;
            this.loadCoverArtFromURLToolStripMenuItem.Name = "loadCoverArtFromURLToolStripMenuItem";
            this.loadCoverArtFromURLToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.loadCoverArtFromURLToolStripMenuItem.Text = "Load Cover Art From URL";
            this.loadCoverArtFromURLToolStripMenuItem.Click += new System.EventHandler(this.loadCoverArtFromURLToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // deleteCoverButton
            // 
            this.deleteCoverButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.deleteCoverButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.cross;
            this.deleteCoverButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.deleteCoverButton.Name = "deleteCoverButton";
            this.deleteCoverButton.Size = new System.Drawing.Size(23, 22);
            this.deleteCoverButton.Text = "toolStripButton1";
            this.deleteCoverButton.ToolTipText = "Delete Current Cover Art";
            this.deleteCoverButton.Click += new System.EventHandler(this.deleteCoverButton_Click);
            // 
            // coverArtFileDialog
            // 
            this.coverArtFileDialog.Filter = "\"Image Files|*.jpg;*.png;*.bmp;*.gif\"";
            // 
            // movieToolStrip
            // 
            this.movieToolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.movieToolStrip.AutoSize = false;
            this.movieToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.movieToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.movieToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.detailsViewDropDown,
            this.advancedButton,
            this.toolStripSeparator6,
            this.deleteMovieButton,
            this.toolStripSeparator5,
            this.refreshMovieButton,
            this.toolStripSeparator4,
            this.unwatchedToggleButton,
            this.watchedToggleButton,
            this.playMovieButton});
            this.movieToolStrip.Location = new System.Drawing.Point(192, 33);
            this.movieToolStrip.Name = "movieToolStrip";
            this.movieToolStrip.Size = new System.Drawing.Size(451, 25);
            this.movieToolStrip.TabIndex = 14;
            this.movieToolStrip.Text = "toolStrip1";
            // 
            // detailsViewDropDown
            // 
            this.detailsViewDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.detailsViewDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.movieDetailsToolStripMenuItem,
            this.fileDetailsToolStripMenuItem});
            this.detailsViewDropDown.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.detailsViewDropDown.Image = ((System.Drawing.Image)(resources.GetObject("detailsViewDropDown.Image")));
            this.detailsViewDropDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.detailsViewDropDown.Name = "detailsViewDropDown";
            this.detailsViewDropDown.Size = new System.Drawing.Size(115, 22);
            this.detailsViewDropDown.Text = "Movie Details";
            // 
            // movieDetailsToolStripMenuItem
            // 
            this.movieDetailsToolStripMenuItem.Name = "movieDetailsToolStripMenuItem";
            this.movieDetailsToolStripMenuItem.Size = new System.Drawing.Size(171, 24);
            this.movieDetailsToolStripMenuItem.Text = "Movie Details";
            this.movieDetailsToolStripMenuItem.Click += new System.EventHandler(this.movieDetailsToolStripMenuItem_Click);
            // 
            // fileDetailsToolStripMenuItem
            // 
            this.fileDetailsToolStripMenuItem.Name = "fileDetailsToolStripMenuItem";
            this.fileDetailsToolStripMenuItem.Size = new System.Drawing.Size(171, 24);
            this.fileDetailsToolStripMenuItem.Text = "File Details";
            this.fileDetailsToolStripMenuItem.Click += new System.EventHandler(this.fileDetailsToolStripMenuItem_Click);
            // 
            // advancedButton
            // 
            this.advancedButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.advancedButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.advancedButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.updateMediaInfoToolStripMenuItem,
            this.sendToImporterToolStripMenuItem,
            this.toolStripMenuItem1,
            this.renameToolStripMenuItem});
            this.advancedButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.cog;
            this.advancedButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.advancedButton.Name = "advancedButton";
            this.advancedButton.Size = new System.Drawing.Size(29, 22);
            this.advancedButton.Text = "toolStripDropDownButton1";
            this.advancedButton.ToolTipText = "Advanced Tools";
            // 
            // updateMediaInfoToolStripMenuItem
            // 
            this.updateMediaInfoToolStripMenuItem.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.page_gear;
            this.updateMediaInfoToolStripMenuItem.Name = "updateMediaInfoToolStripMenuItem";
            this.updateMediaInfoToolStripMenuItem.Size = new System.Drawing.Size(281, 22);
            this.updateMediaInfoToolStripMenuItem.Text = "Update MediaInfo";
            this.updateMediaInfoToolStripMenuItem.Click += new System.EventHandler(this.updateMediaInfoToolStripMenuItem_Click);
            // 
            // sendToImporterToolStripMenuItem
            // 
            this.sendToImporterToolStripMenuItem.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.arrow_down;
            this.sendToImporterToolStripMenuItem.Name = "sendToImporterToolStripMenuItem";
            this.sendToImporterToolStripMenuItem.Size = new System.Drawing.Size(281, 22);
            this.sendToImporterToolStripMenuItem.Text = "Send To Importer";
            this.sendToImporterToolStripMenuItem.Click += new System.EventHandler(this.sendToImporterToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
            // 
            // deleteMovieButton
            // 
            this.deleteMovieButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.deleteMovieButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.deleteMovieButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.cross;
            this.deleteMovieButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.deleteMovieButton.Name = "deleteMovieButton";
            this.deleteMovieButton.Size = new System.Drawing.Size(23, 22);
            this.deleteMovieButton.Text = "toolStripDropDownButton1";
            this.deleteMovieButton.ToolTipText = "Remove Movie and Ignore Files";
            this.deleteMovieButton.Click += new System.EventHandler(this.deleteMovieButton_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // refreshMovieButton
            // 
            this.refreshMovieButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.refreshMovieButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.refreshMovieButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.arrow_rotate_clockwise;
            this.refreshMovieButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.refreshMovieButton.Name = "refreshMovieButton";
            this.refreshMovieButton.Size = new System.Drawing.Size(32, 22);
            this.refreshMovieButton.Text = " Selec";
            this.refreshMovieButton.ToolTipText = "Refresh Movie Info From Internet";
            this.refreshMovieButton.ButtonClick += new System.EventHandler(this.refreshMovieButton_Click);
            this.refreshMovieButton.DropDownOpening += new System.EventHandler(this.refreshMovieButton_DropDownOpening);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // unwatchedToggleButton
            // 
            this.unwatchedToggleButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.unwatchedToggleButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.unwatchedToggleButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.eye_grey;
            this.unwatchedToggleButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.unwatchedToggleButton.Name = "unwatchedToggleButton";
            this.unwatchedToggleButton.Size = new System.Drawing.Size(23, 22);
            this.unwatchedToggleButton.Text = "toolStripButton1";
            this.unwatchedToggleButton.ToolTipText = "Mark Selected Movies As Unwatched";
            this.unwatchedToggleButton.Click += new System.EventHandler(this.unwatchedToggleButton_Click);
            // 
            // watchedToggleButton
            // 
            this.watchedToggleButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.watchedToggleButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.watchedToggleButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.eye;
            this.watchedToggleButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.watchedToggleButton.Name = "watchedToggleButton";
            this.watchedToggleButton.Size = new System.Drawing.Size(23, 22);
            this.watchedToggleButton.Text = "toolStripButton1";
            this.watchedToggleButton.ToolTipText = "Mark Selected Movies As Watched";
            this.watchedToggleButton.Click += new System.EventHandler(this.watchedToggleButton_Click);
            // 
            // playMovieButton
            // 
            this.playMovieButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.playMovieButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.playMovieButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.MediaPlay;
            this.playMovieButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.playMovieButton.Name = "playMovieButton";
            this.playMovieButton.Size = new System.Drawing.Size(23, 22);
            this.playMovieButton.Text = "toolStripButton1";
            this.playMovieButton.ToolTipText = "Play Movie in Default Player";
            this.playMovieButton.Click += new System.EventHandler(this.playMovieButton_Click);
            // 
            // movieListBox
            // 
            this.movieListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.movieListBox.BackColor = System.Drawing.SystemColors.Window;
            this.movieListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.movieListBox.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.movieListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.movieListBox.FullRowSelect = true;
            this.movieListBox.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.movieListBox.HideSelection = false;
            this.movieListBox.Location = new System.Drawing.Point(3, 3);
            this.movieListBox.Name = "movieListBox";
            this.movieListBox.Size = new System.Drawing.Size(178, 288);
            this.movieListBox.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.movieListBox.TabIndex = 2;
            this.movieListBox.UseCompatibleStateImageBehavior = false;
            this.movieListBox.View = System.Windows.Forms.View.Details;
            this.movieListBox.SelectedIndexChanged += new System.EventHandler(this.movieTree_AfterSelect);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 160;
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.Location = new System.Drawing.Point(192, 4);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(58, 29);
            this.titleLabel.TabIndex = 16;
            this.titleLabel.Text = "Title";
            // 
            // movieDetailsSubPane
            // 
            this.movieDetailsSubPane.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.movieDetailsSubPane.DatabaseObject = null;
            this.movieDetailsSubPane.Location = new System.Drawing.Point(192, 62);
            this.movieDetailsSubPane.Name = "movieDetailsSubPane";
            this.movieDetailsSubPane.Size = new System.Drawing.Size(451, 527);
            this.movieDetailsSubPane.TabIndex = 17;
            this.movieDetailsSubPane.Table = typeof(MediaPortal.Plugins.MovingPictures.Database.DBMovieInfo);
            // 
            // fileDetailsSubPane
            // 
            this.fileDetailsSubPane.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.fileDetailsSubPane.DatabaseObject = null;
            this.fileDetailsSubPane.Location = new System.Drawing.Point(192, 62);
            this.fileDetailsSubPane.Name = "fileDetailsSubPane";
            this.fileDetailsSubPane.Size = new System.Drawing.Size(451, 527);
            this.fileDetailsSubPane.TabIndex = 18;
            this.fileDetailsSubPane.Table = typeof(MediaPortal.Plugins.MovingPictures.Database.DBMovieInfo);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.titleSortByFieldToolStripMenuItem,
            this.dateDateAddedFieldToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(169, 22);
            this.toolStripMenuItem1.Text = "Update Sorting";
            // 
            // titleSortByFieldToolStripMenuItem
            // 
            this.titleSortByFieldToolStripMenuItem.Name = "titleSortByFieldToolStripMenuItem";
            this.titleSortByFieldToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.titleSortByFieldToolStripMenuItem.Text = "Title (Sort By Field)";
            this.titleSortByFieldToolStripMenuItem.Click += new System.EventHandler(this.updateTitleSortingMenuItem_Click);
            // 
            // dateDateAddedFieldToolStripMenuItem
            // 
            this.dateDateAddedFieldToolStripMenuItem.Name = "dateDateAddedFieldToolStripMenuItem";
            this.dateDateAddedFieldToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.dateDateAddedFieldToolStripMenuItem.Text = "Date (Date Added Field)";
            this.dateDateAddedFieldToolStripMenuItem.Click += new System.EventHandler(this.updateDateSortingMenuItem_Click);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renameFilesAndFoldersToolStripMenuItem,
            this.revertToOriginalNamesToolStripMenuItem});
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.renameToolStripMenuItem.Text = "Rename";
            // 
            // renameFilesAndFoldersToolStripMenuItem
            // 
            this.renameFilesAndFoldersToolStripMenuItem.Name = "renameFilesAndFoldersToolStripMenuItem";
            this.renameFilesAndFoldersToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.renameFilesAndFoldersToolStripMenuItem.Text = "Rename Files and Folders";
            this.renameFilesAndFoldersToolStripMenuItem.Click += new System.EventHandler(this.updateFileNameToolStripMenuItem_Click);
            // 
            // revertToOriginalNamesToolStripMenuItem
            // 
            this.revertToOriginalNamesToolStripMenuItem.Name = "revertToOriginalNamesToolStripMenuItem";
            this.revertToOriginalNamesToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.revertToOriginalNamesToolStripMenuItem.Text = "Revert to Original Names";
            this.revertToOriginalNamesToolStripMenuItem.Click += new System.EventHandler(this.returnToOriginalFileNameToolStripMenuItem_Click);
            // 
            // MovieManagerPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.movieToolStrip);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.coverPanel);
            this.Controls.Add(this.movieListBox);
            this.Controls.Add(this.movieDetailsSubPane);
            this.Controls.Add(this.fileDetailsSubPane);
            this.Name = "MovieManagerPane";
            this.Size = new System.Drawing.Size(647, 597);
            this.Load += new System.EventHandler(this.MovieManagerPane_Load);
            this.coverPanel.ResumeLayout(false);
            this.coverPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.coverImage)).EndInit();
            this.coverToolStrip.ResumeLayout(false);
            this.coverToolStrip.PerformLayout();
            this.movieToolStrip.ResumeLayout(false);
            this.movieToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ListView movieListBox;
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
        private System.Windows.Forms.ToolStripButton deleteCoverButton;
        private System.Windows.Forms.ToolStrip movieToolStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton playMovieButton;
        private System.Windows.Forms.ToolStripButton deleteMovieButton;
        private ColumnHeader columnHeader1;
        private ProgressBar artworkProgressBar;
        private ToolStripSplitButton refreshMovieButton;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripDropDownButton detailsViewDropDown;
        private ToolStripMenuItem movieDetailsToolStripMenuItem;
        private ToolStripMenuItem fileDetailsToolStripMenuItem;
        private Label titleLabel;
        private MediaPortal.Plugins.MovingPictures.ConfigScreen.MovieManager.MovieDetailsSubPane movieDetailsSubPane;
        private MediaPortal.Plugins.MovingPictures.ConfigScreen.MovieManager.FileDetailsSubPane fileDetailsSubPane;
        private ToolStripDropDownButton advancedButton;
        private ToolStripMenuItem updateMediaInfoToolStripMenuItem;
        private ToolStripMenuItem sendToImporterToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripButton watchedToggleButton;
        private ToolStripButton unwatchedToggleButton;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem titleSortByFieldToolStripMenuItem;
        private ToolStripMenuItem dateDateAddedFieldToolStripMenuItem;
        private ToolStripMenuItem renameToolStripMenuItem;
        private ToolStripMenuItem renameFilesAndFoldersToolStripMenuItem;
        private ToolStripMenuItem revertToOriginalNamesToolStripMenuItem;
    }
}
