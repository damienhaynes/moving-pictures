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
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.remoteFilteringHelpLink = new System.Windows.Forms.LinkLabel();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.label12 = new System.Windows.Forms.Label();
            this.parentalContolsButton = new System.Windows.Forms.Button();
            this.defineCategoriesButton = new System.Windows.Forms.Button();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.filterMenuButton = new System.Windows.Forms.Button();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.defaultFilterCombo = new Cornerstone.GUI.Controls.FilterComboBox();
            this.defaultFilterCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.categoriesCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.passwordTextBox = new Cornerstone.GUI.Controls.SettingsTextBox();
            this.parentalControlsCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.enableDeleteCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.sortFieldComboBox = new Cornerstone.GUI.Controls.SettingsComboBox();
            this.remoteControlCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
            this.watchedPercentTextBox = new Cornerstone.GUI.Controls.SettingsTextBox();
            this.homeScreenTextBox = new Cornerstone.GUI.Controls.SettingsTextBox();
            this.allowRescanInGuiCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
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
            this.label1.Size = new System.Drawing.Size(103, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Layout && Visuals:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(3, 162);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Behavior:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(123, 66);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(131, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "By default show movies in";
            // 
            // defaultViewComboBox
            // 
            this.defaultViewComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.defaultViewComboBox.FormattingEnabled = true;
            this.defaultViewComboBox.Items.AddRange(new object[] {
            "Last Used View",
            "List View",
            "Poster View",
            "Large Poster View",
            "Filmstrip View",
            "Coverflow View"});
            this.defaultViewComboBox.Location = new System.Drawing.Point(261, 63);
            this.defaultViewComboBox.Name = "defaultViewComboBox";
            this.defaultViewComboBox.Size = new System.Drawing.Size(128, 21);
            this.defaultViewComboBox.TabIndex = 3;
            this.defaultViewComboBox.SelectedIndexChanged += new System.EventHandler(this.defaultViewComboBox_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(123, 129);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(147, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Plug-in name in home screen:";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Location = new System.Drawing.Point(9, 153);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(540, 3);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "groupBox4";
            // 
            // helpButton
            // 
            this.helpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.helpButton.BackgroundImage = global::MediaPortal.Plugins.MovingPictures.Properties.Resources.help;
            this.helpButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.helpButton.Location = new System.Drawing.Point(537, 0);
            this.helpButton.Margin = new System.Windows.Forms.Padding(0);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(23, 23);
            this.helpButton.TabIndex = 16;
            this.helpButton.UseVisualStyleBackColor = true;
            this.helpButton.Click += new System.EventHandler(this.helpButton_Click);
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
            this.label5.Location = new System.Drawing.Point(124, 169);
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
            this.panel1.Location = new System.Drawing.Point(258, 162);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(291, 49);
            this.panel1.TabIndex = 20;
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.doNothingRadioButton);
            this.panel2.Controls.Add(this.playDVDradioButton);
            this.panel2.Controls.Add(this.displayDvdDetailsRadioButton);
            this.panel2.Location = new System.Drawing.Point(258, 226);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(291, 73);
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
            this.label6.Location = new System.Drawing.Point(124, 231);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(114, 13);
            this.label6.TabIndex = 22;
            this.label6.Text = "When DVD is inserted:";
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Location = new System.Drawing.Point(119, 217);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(428, 3);
            this.groupBox4.TabIndex = 23;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "groupBox4";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Location = new System.Drawing.Point(9, 394);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(540, 3);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox4";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Location = new System.Drawing.Point(119, 305);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(428, 3);
            this.groupBox2.TabIndex = 24;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "groupBox2";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(124, 315);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(185, 13);
            this.label7.TabIndex = 25;
            this.label7.Text = "Mark movie as watched after viewing";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(353, 315);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(18, 13);
            this.label8.TabIndex = 27;
            this.label8.Text = "%";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(3, 404);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(56, 13);
            this.label9.TabIndex = 28;
            this.label9.Text = "Filtering:";
            // 
            // remoteFilteringHelpLink
            // 
            this.remoteFilteringHelpLink.AutoSize = true;
            this.remoteFilteringHelpLink.Location = new System.Drawing.Point(299, 471);
            this.remoteFilteringHelpLink.Name = "remoteFilteringHelpLink";
            this.remoteFilteringHelpLink.Size = new System.Drawing.Size(68, 13);
            this.remoteFilteringHelpLink.TabIndex = 30;
            this.remoteFilteringHelpLink.TabStop = true;
            this.remoteFilteringHelpLink.Text = "What is this?";
            this.remoteFilteringHelpLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.remoteFilteringHelpLink_LinkClicked);
            // 
            // groupBox6
            // 
            this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox6.Location = new System.Drawing.Point(119, 339);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(428, 3);
            this.groupBox6.TabIndex = 24;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "groupBox2";
            // 
            // groupBox7
            // 
            this.groupBox7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox7.Location = new System.Drawing.Point(9, 493);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(540, 3);
            this.groupBox7.TabIndex = 17;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "groupBox4";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(3, 505);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(108, 13);
            this.label12.TabIndex = 41;
            this.label12.Text = "Parental Controls:";
            // 
            // parentalContolsButton
            // 
            this.parentalContolsButton.Location = new System.Drawing.Point(127, 528);
            this.parentalContolsButton.Name = "parentalContolsButton";
            this.parentalContolsButton.Size = new System.Drawing.Size(144, 23);
            this.parentalContolsButton.TabIndex = 43;
            this.parentalContolsButton.Text = "Define Unrestricted Movies";
            this.parentalContolsButton.UseVisualStyleBackColor = true;
            this.parentalContolsButton.Click += new System.EventHandler(this.parentalControlsButton_Click);
            // 
            // defineCategoriesButton
            // 
            this.defineCategoriesButton.Location = new System.Drawing.Point(128, 26);
            this.defineCategoriesButton.Name = "defineCategoriesButton";
            this.defineCategoriesButton.Size = new System.Drawing.Size(163, 23);
            this.defineCategoriesButton.TabIndex = 45;
            this.defineCategoriesButton.Text = "Define Movie Categories";
            this.defineCategoriesButton.UseVisualStyleBackColor = true;
            this.defineCategoriesButton.Click += new System.EventHandler(this.categoriesMenuButton_Click);
            // 
            // groupBox10
            // 
            this.groupBox10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox10.Location = new System.Drawing.Point(119, 461);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(428, 3);
            this.groupBox10.TabIndex = 36;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "groupBox10";
            // 
            // filterMenuButton
            // 
            this.filterMenuButton.Location = new System.Drawing.Point(127, 431);
            this.filterMenuButton.Name = "filterMenuButton";
            this.filterMenuButton.Size = new System.Drawing.Size(111, 23);
            this.filterMenuButton.TabIndex = 47;
            this.filterMenuButton.Text = "Modify Filters Menu";
            this.filterMenuButton.UseVisualStyleBackColor = true;
            this.filterMenuButton.Click += new System.EventHandler(this.filterMenuButton_Click);
            // 
            // groupBox9
            // 
            this.groupBox9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox9.Location = new System.Drawing.Point(121, 117);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(428, 3);
            this.groupBox9.TabIndex = 24;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "groupBox9";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(124, 93);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(93, 13);
            this.label11.TabIndex = 33;
            this.label11.Text = "By default sort by";
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Location = new System.Drawing.Point(122, 54);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(428, 3);
            this.groupBox5.TabIndex = 25;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "groupBox5";
            // 
            // defaultFilterCombo
            // 
            this.defaultFilterCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.defaultFilterCombo.FormattingEnabled = true;
            this.defaultFilterCombo.Location = new System.Drawing.Point(255, 401);
            this.defaultFilterCombo.Name = "defaultFilterCombo";
            this.defaultFilterCombo.RestrictSelectionToLeafNodes = true;
            this.defaultFilterCombo.Size = new System.Drawing.Size(134, 21);
            this.defaultFilterCombo.TabIndex = 52;
            this.defaultFilterCombo.SelectedIndexChanged += new System.EventHandler(this.defaultFilterCombo_SelectedIndexChanged);
            // 
            // defaultFilterCheckBox
            // 
            this.defaultFilterCheckBox.AutoSize = true;
            this.defaultFilterCheckBox.IgnoreSettingName = true;
            this.defaultFilterCheckBox.Location = new System.Drawing.Point(127, 403);
            this.defaultFilterCheckBox.Name = "defaultFilterCheckBox";
            this.defaultFilterCheckBox.Setting = null;
            this.defaultFilterCheckBox.Size = new System.Drawing.Size(122, 17);
            this.defaultFilterCheckBox.TabIndex = 50;
            this.defaultFilterCheckBox.Text = "On startup filter by:";
            this.defaultFilterCheckBox.UseVisualStyleBackColor = true;
            this.defaultFilterCheckBox.CheckedChanged += new System.EventHandler(this.defaultFilterCheckBox_CheckedChanged);
            // 
            // categoriesCheckBox
            // 
            this.categoriesCheckBox.AutoSize = true;
            this.categoriesCheckBox.IgnoreSettingName = true;
            this.categoriesCheckBox.Location = new System.Drawing.Point(127, 3);
            this.categoriesCheckBox.Name = "categoriesCheckBox";
            this.categoriesCheckBox.Setting = null;
            this.categoriesCheckBox.Size = new System.Drawing.Size(166, 17);
            this.categoriesCheckBox.TabIndex = 49;
            this.categoriesCheckBox.Text = "Organize movies by category";
            this.categoriesCheckBox.UseVisualStyleBackColor = true;
            this.categoriesCheckBox.CheckedChanged += new System.EventHandler(this.categoriesCheckBox_CheckedChanged);
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(431, 503);
            this.passwordTextBox.MaxLength = 4;
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.PasswordChar = '●';
            this.passwordTextBox.Setting = null;
            this.passwordTextBox.Size = new System.Drawing.Size(51, 21);
            this.passwordTextBox.TabIndex = 44;
            this.passwordTextBox.TextChanged += new System.EventHandler(this.passwordTextBox_TextChanged);
            this.passwordTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.passwordTextBox_KeyPress);
            // 
            // parentalControlsCheckBox
            // 
            this.parentalControlsCheckBox.AutoSize = true;
            this.parentalControlsCheckBox.IgnoreSettingName = true;
            this.parentalControlsCheckBox.Location = new System.Drawing.Point(127, 505);
            this.parentalControlsCheckBox.Name = "parentalControlsCheckBox";
            this.parentalControlsCheckBox.Setting = null;
            this.parentalControlsCheckBox.Size = new System.Drawing.Size(284, 17);
            this.parentalControlsCheckBox.TabIndex = 42;
            this.parentalControlsCheckBox.Text = "Require following password to view restricted movies:";
            this.parentalControlsCheckBox.UseVisualStyleBackColor = true;
            this.parentalControlsCheckBox.CheckedChanged += new System.EventHandler(this.parentalControlsCheckBox_CheckedChanged);
            // 
            // enableDeleteCheckBox
            // 
            this.enableDeleteCheckBox.AutoSize = true;
            this.enableDeleteCheckBox.IgnoreSettingName = false;
            this.enableDeleteCheckBox.Location = new System.Drawing.Point(127, 348);
            this.enableDeleteCheckBox.Name = "enableDeleteCheckBox";
            this.enableDeleteCheckBox.Setting = null;
            this.enableDeleteCheckBox.Size = new System.Drawing.Size(281, 17);
            this.enableDeleteCheckBox.TabIndex = 40;
            this.enableDeleteCheckBox.Text = "Allow user to delete files from the GUI context menu.";
            this.enableDeleteCheckBox.UseVisualStyleBackColor = true;
            // 
            // sortFieldComboBox
            // 
            this.sortFieldComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sortFieldComboBox.EnumType = null;
            this.sortFieldComboBox.FormattingEnabled = true;
            this.sortFieldComboBox.Location = new System.Drawing.Point(223, 90);
            this.sortFieldComboBox.Name = "sortFieldComboBox";
            this.sortFieldComboBox.Setting = null;
            this.sortFieldComboBox.Size = new System.Drawing.Size(121, 21);
            this.sortFieldComboBox.TabIndex = 36;
            // 
            // remoteControlCheckBox
            // 
            this.remoteControlCheckBox.AutoSize = true;
            this.remoteControlCheckBox.IgnoreSettingName = false;
            this.remoteControlCheckBox.Location = new System.Drawing.Point(127, 470);
            this.remoteControlCheckBox.Name = "remoteControlCheckBox";
            this.remoteControlCheckBox.Setting = null;
            this.remoteControlCheckBox.Size = new System.Drawing.Size(163, 17);
            this.remoteControlCheckBox.TabIndex = 29;
            this.remoteControlCheckBox.Text = "Use Remote Control Filtering";
            this.remoteControlCheckBox.UseVisualStyleBackColor = true;
            // 
            // watchedPercentTextBox
            // 
            this.watchedPercentTextBox.Location = new System.Drawing.Point(318, 312);
            this.watchedPercentTextBox.Name = "watchedPercentTextBox";
            this.watchedPercentTextBox.Setting = null;
            this.watchedPercentTextBox.Size = new System.Drawing.Size(29, 21);
            this.watchedPercentTextBox.TabIndex = 26;
            // 
            // homeScreenTextBox
            // 
            this.homeScreenTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.homeScreenTextBox.Location = new System.Drawing.Point(294, 126);
            this.homeScreenTextBox.Name = "homeScreenTextBox";
            this.homeScreenTextBox.Setting = null;
            this.homeScreenTextBox.Size = new System.Drawing.Size(221, 21);
            this.homeScreenTextBox.TabIndex = 5;
            // 
            // allowRescanInGuiCheckBox
            // 
            this.allowRescanInGuiCheckBox.AutoSize = true;
            this.allowRescanInGuiCheckBox.IgnoreSettingName = true;
            this.allowRescanInGuiCheckBox.Location = new System.Drawing.Point(127, 371);
            this.allowRescanInGuiCheckBox.Name = "allowRescanInGuiCheckBox";
            this.allowRescanInGuiCheckBox.Setting = null;
            this.allowRescanInGuiCheckBox.Size = new System.Drawing.Size(392, 17);
            this.allowRescanInGuiCheckBox.TabIndex = 53;
            this.allowRescanInGuiCheckBox.Text = "Allow user to send a movie back to the importer from the GUI context menu.";
            this.allowRescanInGuiCheckBox.UseVisualStyleBackColor = true;
            // 
            // GUISettingsPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.allowRescanInGuiCheckBox);
            this.Controls.Add(this.defaultFilterCombo);
            this.Controls.Add(this.defaultFilterCheckBox);
            this.Controls.Add(this.categoriesCheckBox);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox9);
            this.Controls.Add(this.filterMenuButton);
            this.Controls.Add(this.groupBox10);
            this.Controls.Add(this.defineCategoriesButton);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.parentalContolsButton);
            this.Controls.Add(this.parentalControlsCheckBox);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.groupBox7);
            this.Controls.Add(this.enableDeleteCheckBox);
            this.Controls.Add(this.sortFieldComboBox);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.remoteFilteringHelpLink);
            this.Controls.Add(this.remoteControlCheckBox);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.watchedPercentTextBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.groupBox6);
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
            this.Size = new System.Drawing.Size(560, 556);
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
        private System.Windows.Forms.LinkLabel remoteFilteringHelpLink;
        private Cornerstone.GUI.Controls.SettingCheckBox enableDeleteCheckBox;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Label label12;
        private Cornerstone.GUI.Controls.SettingCheckBox parentalControlsCheckBox;
        private System.Windows.Forms.Button parentalContolsButton;
        private Cornerstone.GUI.Controls.SettingsTextBox passwordTextBox;
        private System.Windows.Forms.Button defineCategoriesButton;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.Button filterMenuButton;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.Label label11;
        private Cornerstone.GUI.Controls.SettingsComboBox sortFieldComboBox;
        private System.Windows.Forms.GroupBox groupBox5;
        private Cornerstone.GUI.Controls.SettingCheckBox categoriesCheckBox;
        private Cornerstone.GUI.Controls.SettingCheckBox defaultFilterCheckBox;
        private Cornerstone.GUI.Controls.FilterComboBox defaultFilterCombo;
        private Cornerstone.GUI.Controls.SettingCheckBox allowRescanInGuiCheckBox;
    }
}
