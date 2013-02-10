namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.MovieManager {
    partial class FileDetailsSubPane {
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
            Cornerstone.GUI.Controls.FieldProperty fieldProperty17 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty18 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty19 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty20 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty21 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty22 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty23 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty24 = new Cornerstone.GUI.Controls.FieldProperty();
            Cornerstone.GUI.Controls.FieldProperty fieldProperty25 = new Cornerstone.GUI.Controls.FieldProperty();
            this.fileList = new System.Windows.Forms.ListBox();
            this.playbackOrderToolStrip = new System.Windows.Forms.ToolStrip();
            this.fileUpButton = new System.Windows.Forms.ToolStripButton();
            this.fileDownButton = new System.Windows.Forms.ToolStripButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.fileDetailsList = new Cornerstone.GUI.Controls.DBObjectEditor();
            this.playbackOrderToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // fileList
            // 
            this.fileList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.fileList.FormattingEnabled = true;
            this.fileList.Location = new System.Drawing.Point(0, 24);
            this.fileList.Name = "fileList";
            this.fileList.Size = new System.Drawing.Size(381, 56);
            this.fileList.TabIndex = 16;
            this.fileList.SelectedIndexChanged += new System.EventHandler(this.fileList_SelectedIndexChanged);
            // 
            // playbackOrderToolStrip
            // 
            this.playbackOrderToolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.playbackOrderToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.playbackOrderToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.playbackOrderToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileUpButton,
            this.fileDownButton});
            this.playbackOrderToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.playbackOrderToolStrip.Location = new System.Drawing.Point(384, 29);
            this.playbackOrderToolStrip.Name = "playbackOrderToolStrip";
            this.playbackOrderToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.playbackOrderToolStrip.Size = new System.Drawing.Size(24, 47);
            this.playbackOrderToolStrip.TabIndex = 17;
            this.playbackOrderToolStrip.Text = "toolStrip1";
            // 
            // fileUpButton
            // 
            this.fileUpButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.fileUpButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.arrow_up;
            this.fileUpButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.fileUpButton.Margin = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.fileUpButton.Name = "fileUpButton";
            this.fileUpButton.Size = new System.Drawing.Size(22, 20);
            this.fileUpButton.Text = "toolStripButton1";
            this.fileUpButton.ToolTipText = "Change Playback Order";
            this.fileUpButton.Click += new System.EventHandler(this.fileUpButton_Click);
            // 
            // fileDownButton
            // 
            this.fileDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.fileDownButton.Image = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.arrow_down;
            this.fileDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.fileDownButton.Name = "fileDownButton";
            this.fileDownButton.Size = new System.Drawing.Size(22, 20);
            this.fileDownButton.Text = "toolStripButton2";
            this.fileDownButton.ToolTipText = "Change Playback Order";
            this.fileDownButton.Click += new System.EventHandler(this.fileDownButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Playback Order";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 19;
            this.label2.Text = "File Information";
            // 
            // fileDetailsList
            // 
            this.fileDetailsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.fileDetailsList.DatabaseObject = null;
            fieldProperty17.DisplayName = "Full Path";
            fieldProperty17.FieldName = "FullPath";
            fieldProperty17.ReadOnly = true;
            fieldProperty18.DisplayName = "Part";
            fieldProperty18.FieldName = "Part";
            fieldProperty18.Visible = false;
            fieldProperty19.DisplayName = "Ignored";
            fieldProperty19.FieldName = "Ignored";
            fieldProperty19.Visible = false;
            fieldProperty20.DisplayName = "Import Path";
            fieldProperty20.FieldName = "ImportPath";
            fieldProperty20.ReadOnly = true;
            fieldProperty21.DisplayName = "Duration";
            fieldProperty21.FieldName = "Duration";
            fieldProperty21.ReadOnly = true;
            fieldProperty22.DisplayName = "Media/Disk Label";
            fieldProperty22.FieldName = "MediaLabel";
            fieldProperty23.DisplayName = "Volume Serial";
            fieldProperty23.FieldName = "VolumeSerial";
            fieldProperty23.ReadOnly = true;
            fieldProperty23.Visible = false;
            fieldProperty24.DisplayName = "Disc Id";
            fieldProperty24.FieldName = "DiscId";
            fieldProperty24.ReadOnly = true;
            fieldProperty24.Visible = false;
            fieldProperty25.DisplayName = "File Hash";
            fieldProperty25.FieldName = "FileHash";
            fieldProperty25.ReadOnly = true;
            fieldProperty25.Visible = false;
            this.fileDetailsList.FieldDisplaySettings.FieldProperties.Add(fieldProperty17);
            this.fileDetailsList.FieldDisplaySettings.FieldProperties.Add(fieldProperty18);
            this.fileDetailsList.FieldDisplaySettings.FieldProperties.Add(fieldProperty19);
            this.fileDetailsList.FieldDisplaySettings.FieldProperties.Add(fieldProperty20);
            this.fileDetailsList.FieldDisplaySettings.FieldProperties.Add(fieldProperty21);
            this.fileDetailsList.FieldDisplaySettings.FieldProperties.Add(fieldProperty22);
            this.fileDetailsList.FieldDisplaySettings.FieldProperties.Add(fieldProperty23);
            this.fileDetailsList.FieldDisplaySettings.FieldProperties.Add(fieldProperty24);
            this.fileDetailsList.FieldDisplaySettings.FieldProperties.Add(fieldProperty25);
            this.fileDetailsList.Location = new System.Drawing.Point(0, 111);
            this.fileDetailsList.Name = "fileDetailsList";
            this.fileDetailsList.Size = new System.Drawing.Size(408, 320);
            this.fileDetailsList.TabIndex = 15;
            this.fileDetailsList.FieldDisplaySettings.Table = typeof(MediaPortal.Plugins.MovingPictures.Database.DBLocalMedia);
            // 
            // FileDetailsSubPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.fileList);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.playbackOrderToolStrip);
            this.Controls.Add(this.fileDetailsList);
            this.Name = "FileDetailsSubPane";
            this.Size = new System.Drawing.Size(408, 431);
            this.playbackOrderToolStrip.ResumeLayout(false);
            this.playbackOrderToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox fileList;
        private Cornerstone.GUI.Controls.DBObjectEditor fileDetailsList;
        private System.Windows.Forms.ToolStrip playbackOrderToolStrip;
        private System.Windows.Forms.ToolStripButton fileUpButton;
        private System.Windows.Forms.ToolStripButton fileDownButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;

    }
}
