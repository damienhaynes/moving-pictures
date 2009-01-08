namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    partial class GUISettingsPane {
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.defaultViewComboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.homeScreenTextBox = new Cornerstone.GUI.Controls.SettingsTextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.helpButton = new System.Windows.Forms.Button();
            this.playMovieRadioButton = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.displayDetailsRadioButton = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.doNothingRadioButton = new System.Windows.Forms.RadioButton();
            this.playDVDradioButton = new System.Windows.Forms.RadioButton();
            this.displayDvdDetailsRadioButton = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.watchedPercentTextBox = new Cornerstone.GUI.Controls.SettingsTextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.remoteControlCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Visuals:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(4, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Behavior:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(125, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(156, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Start GUI with this view active:";
            // 
            // defaultViewComboBox
            // 
            this.defaultViewComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.defaultViewComboBox.FormattingEnabled = true;
            this.defaultViewComboBox.Items.AddRange(new object[] {
            "List View",
            "Poster View",
            "Large Poster View",
            "Filmstrip View"});
            this.defaultViewComboBox.Location = new System.Drawing.Point(296, 1);
            this.defaultViewComboBox.Name = "defaultViewComboBox";
            this.defaultViewComboBox.Size = new System.Drawing.Size(128, 21);
            this.defaultViewComboBox.TabIndex = 3;
            this.defaultViewComboBox.SelectedIndexChanged += new System.EventHandler(this.defaultViewComboBox_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(125, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(147, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Plug-in name in home screen:";
            // 
            // homeScreenTextBox
            // 
            this.homeScreenTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.homeScreenTextBox.Location = new System.Drawing.Point(296, 28);
            this.homeScreenTextBox.Name = "homeScreenTextBox";
            this.homeScreenTextBox.Setting = null;
            this.homeScreenTextBox.Size = new System.Drawing.Size(173, 21);
            this.homeScreenTextBox.TabIndex = 5;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Location = new System.Drawing.Point(10, 54);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(492, 3);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "groupBox4";
            // 
            // helpButton
            // 
            this.helpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.helpButton.BackgroundImage = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.help;
            this.helpButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.helpButton.Location = new System.Drawing.Point(489, 0);
            this.helpButton.Margin = new System.Windows.Forms.Padding(0);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(23, 23);
            this.helpButton.TabIndex = 16;
            this.helpButton.UseVisualStyleBackColor = true;
            // 
            // playMovieRadioButton
            // 
            this.playMovieRadioButton.AutoSize = true;
            this.playMovieRadioButton.Location = new System.Drawing.Point(3, 5);
            this.playMovieRadioButton.Name = "playMovieRadioButton";
            this.playMovieRadioButton.Size = new System.Drawing.Size(76, 17);
            this.playMovieRadioButton.TabIndex = 17;
            this.playMovieRadioButton.TabStop = true;
            this.playMovieRadioButton.Text = "Play movie";
            this.playMovieRadioButton.UseVisualStyleBackColor = true;
            this.playMovieRadioButton.CheckedChanged += new System.EventHandler(this.playMovieRadioButton_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(125, 70);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(114, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "When movie is clicked:";
            // 
            // displayDetailsRadioButton
            // 
            this.displayDetailsRadioButton.AutoSize = true;
            this.displayDetailsRadioButton.Location = new System.Drawing.Point(3, 28);
            this.displayDetailsRadioButton.Name = "displayDetailsRadioButton";
            this.displayDetailsRadioButton.Size = new System.Drawing.Size(127, 17);
            this.displayDetailsRadioButton.TabIndex = 19;
            this.displayDetailsRadioButton.TabStop = true;
            this.displayDetailsRadioButton.Text = "Display movie  details";
            this.displayDetailsRadioButton.UseVisualStyleBackColor = true;
            this.displayDetailsRadioButton.CheckedChanged += new System.EventHandler(this.displayDetailsRadioButton_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.playMovieRadioButton);
            this.panel1.Controls.Add(this.displayDetailsRadioButton);
            this.panel1.Location = new System.Drawing.Point(259, 63);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(243, 49);
            this.panel1.TabIndex = 20;
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.doNothingRadioButton);
            this.panel2.Controls.Add(this.playDVDradioButton);
            this.panel2.Controls.Add(this.displayDvdDetailsRadioButton);
            this.panel2.Location = new System.Drawing.Point(259, 127);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(243, 73);
            this.panel2.TabIndex = 21;
            // 
            // doNothingRadioButton
            // 
            this.doNothingRadioButton.AutoSize = true;
            this.doNothingRadioButton.Location = new System.Drawing.Point(3, 50);
            this.doNothingRadioButton.Name = "doNothingRadioButton";
            this.doNothingRadioButton.Size = new System.Drawing.Size(77, 17);
            this.doNothingRadioButton.TabIndex = 20;
            this.doNothingRadioButton.TabStop = true;
            this.doNothingRadioButton.Text = "Do nothing";
            this.doNothingRadioButton.UseVisualStyleBackColor = true;
            this.doNothingRadioButton.CheckedChanged += new System.EventHandler(this.doNothingRadioButton_CheckedChanged);
            // 
            // playDVDradioButton
            // 
            this.playDVDradioButton.AutoSize = true;
            this.playDVDradioButton.Location = new System.Drawing.Point(3, 3);
            this.playDVDradioButton.Name = "playDVDradioButton";
            this.playDVDradioButton.Size = new System.Drawing.Size(76, 17);
            this.playDVDradioButton.TabIndex = 17;
            this.playDVDradioButton.TabStop = true;
            this.playDVDradioButton.Text = "Play movie";
            this.playDVDradioButton.UseVisualStyleBackColor = true;
            this.playDVDradioButton.CheckedChanged += new System.EventHandler(this.playDVDradioButton_CheckedChanged);
            // 
            // displayDvdDetailsRadioButton
            // 
            this.displayDvdDetailsRadioButton.AutoSize = true;
            this.displayDvdDetailsRadioButton.Location = new System.Drawing.Point(3, 27);
            this.displayDvdDetailsRadioButton.Name = "displayDvdDetailsRadioButton";
            this.displayDvdDetailsRadioButton.Size = new System.Drawing.Size(127, 17);
            this.displayDvdDetailsRadioButton.TabIndex = 19;
            this.displayDvdDetailsRadioButton.TabStop = true;
            this.displayDvdDetailsRadioButton.Text = "Display movie  details";
            this.displayDvdDetailsRadioButton.UseVisualStyleBackColor = true;
            this.displayDvdDetailsRadioButton.CheckedChanged += new System.EventHandler(this.displayDvdDetailsRadioButton_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(125, 132);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(114, 13);
            this.label6.TabIndex = 22;
            this.label6.Text = "When DVD is inserted:";
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Location = new System.Drawing.Point(120, 118);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(380, 3);
            this.groupBox4.TabIndex = 23;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "groupBox4";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Location = new System.Drawing.Point(10, 240);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(492, 3);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox4";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Location = new System.Drawing.Point(120, 206);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(380, 3);
            this.groupBox2.TabIndex = 24;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "groupBox2";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(125, 216);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(185, 13);
            this.label7.TabIndex = 25;
            this.label7.Text = "Mark movie as watched after viewing";
            // 
            // watchedPercentTextBox
            // 
            this.watchedPercentTextBox.Location = new System.Drawing.Point(319, 213);
            this.watchedPercentTextBox.Name = "watchedPercentTextBox";
            this.watchedPercentTextBox.Setting = null;
            this.watchedPercentTextBox.Size = new System.Drawing.Size(29, 21);
            this.watchedPercentTextBox.TabIndex = 26;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(354, 216);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(18, 13);
            this.label8.TabIndex = 27;
            this.label8.Text = "%";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(4, 250);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(56, 13);
            this.label9.TabIndex = 28;
            this.label9.Text = "Filtering:";
            // 
            // remoteControlCheckBox
            // 
            this.remoteControlCheckBox.AutoSize = true;
            this.remoteControlCheckBox.IgnoreSettingName = false;
            this.remoteControlCheckBox.Location = new System.Drawing.Point(128, 250);
            this.remoteControlCheckBox.Name = "remoteControlCheckBox";
            this.remoteControlCheckBox.Setting = null;
            this.remoteControlCheckBox.Size = new System.Drawing.Size(163, 17);
            this.remoteControlCheckBox.TabIndex = 29;
            this.remoteControlCheckBox.Text = "Use Remote Control Filtering";
            this.remoteControlCheckBox.UseVisualStyleBackColor = true;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(315, 252);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(68, 13);
            this.linkLabel1.TabIndex = 30;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "What is this?";
            // 
            // GUISettingsPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.remoteControlCheckBox);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.watchedPercentTextBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.homeScreenTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.defaultViewComboBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(460, 100);
            this.Name = "GUISettingsPane";
            this.Size = new System.Drawing.Size(512, 446);
            this.Load += new System.EventHandler(this.GUISettingsPane_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox defaultViewComboBox;
        private System.Windows.Forms.Label label4;
        private Cornerstone.GUI.Controls.SettingsTextBox homeScreenTextBox;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button helpButton;
        private System.Windows.Forms.RadioButton playMovieRadioButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton displayDetailsRadioButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton playDVDradioButton;
        private System.Windows.Forms.RadioButton displayDvdDetailsRadioButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton doNothingRadioButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label7;
        private Cornerstone.GUI.Controls.SettingsTextBox watchedPercentTextBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private Cornerstone.GUI.Controls.SettingCheckBox remoteControlCheckBox;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}
