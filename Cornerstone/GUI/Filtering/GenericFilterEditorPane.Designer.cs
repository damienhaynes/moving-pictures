namespace Cornerstone.GUI {
    partial class GenericFilterEditorPane<T> {
        /// <summary> 
        /// Required designer variable.
        /// </summary>

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            Cornerstone.GUI.Controls.FieldDisplaySettings fieldDisplaySettings1 = new Cornerstone.GUI.Controls.FieldDisplaySettings();
            Cornerstone.GUI.Controls.FieldDisplaySettings fieldDisplaySettings2 = new Cornerstone.GUI.Controls.FieldDisplaySettings();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
            this.addNewRuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.addNewSubFilterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addExistingSubFilterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.label2 = new System.Windows.Forms.Label();
            this.filterGroupingCombo = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.filterNameTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.toolStrip3 = new System.Windows.Forms.ToolStrip();
            this.blacklistAddButton = new System.Windows.Forms.ToolStripButton();
            this.blacklistRemoveButton = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.whitelistAddButton = new System.Windows.Forms.ToolStripButton();
            this.whitelistRemoveButton = new System.Windows.Forms.ToolStripButton();
            this.blackList = new Cornerstone.GUI.Controls.DBObjectListEditor();
            this.whiteList = new Cornerstone.GUI.Controls.DBObjectListEditor();
            this.criteriaListPanel1 = new Cornerstone.GUI.Filtering.CriteriaListPanel();
            this.toolStrip1.SuspendLayout();
            this.toolStrip3.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSplitButton1,
            this.toolStripButton1});
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.toolStrip1.Location = new System.Drawing.Point(647, 66);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(33, 48);
            this.toolStrip1.TabIndex = 9;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripSplitButton1
            // 
            this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNewRuleToolStripMenuItem,
            this.toolStripSeparator1,
            this.addNewSubFilterToolStripMenuItem,
            this.addExistingSubFilterToolStripMenuItem});
            this.toolStripSplitButton1.Image = global::Cornerstone.Properties.Resources.list_add;
            this.toolStripSplitButton1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            this.toolStripSplitButton1.Size = new System.Drawing.Size(31, 20);
            this.toolStripSplitButton1.Text = "addCriteriaButton";
            this.toolStripSplitButton1.ButtonClick += new System.EventHandler(this.addCriteriaButton_ButtonClick);
            // 
            // addNewRuleToolStripMenuItem
            // 
            this.addNewRuleToolStripMenuItem.Name = "addNewRuleToolStripMenuItem";
            this.addNewRuleToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.addNewRuleToolStripMenuItem.Text = "Add New Rule";
            this.addNewRuleToolStripMenuItem.Click += new System.EventHandler(this.addCriteriaButton_ButtonClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(188, 6);
            // 
            // addNewSubFilterToolStripMenuItem
            // 
            this.addNewSubFilterToolStripMenuItem.Enabled = false;
            this.addNewSubFilterToolStripMenuItem.Name = "addNewSubFilterToolStripMenuItem";
            this.addNewSubFilterToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.addNewSubFilterToolStripMenuItem.Text = "Add New Sub Filter";
            // 
            // addExistingSubFilterToolStripMenuItem
            // 
            this.addExistingSubFilterToolStripMenuItem.Enabled = false;
            this.addExistingSubFilterToolStripMenuItem.Name = "addExistingSubFilterToolStripMenuItem";
            this.addExistingSubFilterToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.addExistingSubFilterToolStripMenuItem.Text = "Add Existing Sub Filter";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::Cornerstone.Properties.Resources.list_remove;
            this.toolStripButton1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(31, 20);
            this.toolStripButton1.Text = "removeCriteriaButton";
            this.toolStripButton1.Click += new System.EventHandler(this.removeCriteriaButton_Click);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(179, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "of these rules:";
            // 
            // filterGroupingCombo
            // 
            this.filterGroupingCombo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.filterGroupingCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.filterGroupingCombo.FormattingEnabled = true;
            this.filterGroupingCombo.Items.AddRange(new object[] {
            "all",
            "any",
            "none"});
            this.filterGroupingCombo.Location = new System.Drawing.Point(125, 3);
            this.filterGroupingCombo.Name = "filterGroupingCombo";
            this.filterGroupingCombo.Size = new System.Drawing.Size(48, 21);
            this.filterGroupingCombo.TabIndex = 7;
            this.filterGroupingCombo.SelectedIndexChanged += new System.EventHandler(this.filterGroupingCombo_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Include items matching";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(166, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "This filter is named:";
            // 
            // filterNameTextBox
            // 
            this.filterNameTextBox.Location = new System.Drawing.Point(272, 6);
            this.filterNameTextBox.Name = "filterNameTextBox";
            this.filterNameTextBox.Size = new System.Drawing.Size(259, 21);
            this.filterNameTextBox.TabIndex = 13;
            this.filterNameTextBox.TextChanged += new System.EventHandler(this.filterNameTextBox_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(3, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(101, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Display Settings:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(3, 45);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(96, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "Matching Rules:";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Location = new System.Drawing.Point(6, 33);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(673, 3);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Location = new System.Drawing.Point(146, 189);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(533, 3);
            this.groupBox2.TabIndex = 18;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "groupBox2";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(139, 13);
            this.label8.TabIndex = 19;
            this.label8.Text = "Always include these items:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(143, 13);
            this.label9.TabIndex = 21;
            this.label9.Text = "Always exclude these items:";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Location = new System.Drawing.Point(146, 324);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(533, 3);
            this.groupBox3.TabIndex = 19;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "groupBox3";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(166, 333);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(146, 23);
            this.button1.TabIndex = 23;
            this.button1.Text = "View Results of this Filter";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // toolStrip3
            // 
            this.toolStrip3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip3.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip3.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.blacklistAddButton,
            this.blacklistRemoveButton});
            this.toolStrip3.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.toolStrip3.Location = new System.Drawing.Point(226, 16);
            this.toolStrip3.Name = "toolStrip3";
            this.toolStrip3.Size = new System.Drawing.Size(24, 48);
            this.toolStrip3.TabIndex = 26;
            this.toolStrip3.Text = "toolStrip3";
            // 
            // blacklistAddButton
            // 
            this.blacklistAddButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.blacklistAddButton.Image = global::Cornerstone.Properties.Resources.list_add;
            this.blacklistAddButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.blacklistAddButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.blacklistAddButton.Name = "blacklistAddButton";
            this.blacklistAddButton.Size = new System.Drawing.Size(22, 20);
            this.blacklistAddButton.Text = "toolStripSplitButton1";
            this.blacklistAddButton.ToolTipText = "Add Item";
            this.blacklistAddButton.Click += new System.EventHandler(this.blacklistAddButton_Click);
            // 
            // blacklistRemoveButton
            // 
            this.blacklistRemoveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.blacklistRemoveButton.Image = global::Cornerstone.Properties.Resources.list_remove;
            this.blacklistRemoveButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.blacklistRemoveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.blacklistRemoveButton.Name = "blacklistRemoveButton";
            this.blacklistRemoveButton.Size = new System.Drawing.Size(22, 20);
            this.blacklistRemoveButton.Text = "toolStripButton1";
            this.blacklistRemoveButton.ToolTipText = "Remove Item";
            this.blacklistRemoveButton.Click += new System.EventHandler(this.blacklistRemoveButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 2143F));
            this.tableLayoutPanel1.Location = new System.Drawing.Point(41, 151);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(2143, 0);
            this.tableLayoutPanel1.TabIndex = 27;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.filterGroupingCombo);
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(163, 39);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(259, 27);
            this.flowLayoutPanel1.TabIndex = 28;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.panel1, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(166, 198);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(518, 120);
            this.tableLayoutPanel2.TabIndex = 29;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.blackList);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.toolStrip3);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(259, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(259, 120);
            this.panel1.TabIndex = 30;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.whiteList);
            this.panel2.Controls.Add(this.toolStrip2);
            this.panel2.Controls.Add(this.label8);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(259, 120);
            this.panel2.TabIndex = 31;
            // 
            // toolStrip2
            // 
            this.toolStrip2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip2.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.whitelistAddButton,
            this.whitelistRemoveButton});
            this.toolStrip2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.toolStrip2.Location = new System.Drawing.Point(226, 16);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(24, 48);
            this.toolStrip2.TabIndex = 26;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // whitelistAddButton
            // 
            this.whitelistAddButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.whitelistAddButton.Image = global::Cornerstone.Properties.Resources.list_add;
            this.whitelistAddButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.whitelistAddButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.whitelistAddButton.Name = "whitelistAddButton";
            this.whitelistAddButton.Size = new System.Drawing.Size(22, 20);
            this.whitelistAddButton.Text = "toolStripSplitButton1";
            this.whitelistAddButton.ToolTipText = "Add Item";
            this.whitelistAddButton.Click += new System.EventHandler(this.whitelistAddButton_Click);
            // 
            // whitelistRemoveButton
            // 
            this.whitelistRemoveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.whitelistRemoveButton.Image = global::Cornerstone.Properties.Resources.list_remove;
            this.whitelistRemoveButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.whitelistRemoveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.whitelistRemoveButton.Name = "whitelistRemoveButton";
            this.whitelistRemoveButton.Size = new System.Drawing.Size(22, 20);
            this.whitelistRemoveButton.Text = "toolStripButton1";
            this.whitelistRemoveButton.ToolTipText = "Remove Item";
            this.whitelistRemoveButton.Click += new System.EventHandler(this.whitelistRemoveButton_Click);
            // 
            // blackList
            // 
            this.blackList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            fieldDisplaySettings1.Table = null;
            this.blackList.FieldDisplaySettings = fieldDisplaySettings1;
            this.blackList.FullRowSelect = true;
            this.blackList.Location = new System.Drawing.Point(0, 16);
            this.blackList.Name = "blackList";
            this.blackList.Size = new System.Drawing.Size(222, 104);
            this.blackList.TabIndex = 28;
            this.blackList.View = System.Windows.Forms.View.Details;
            // 
            // whiteList
            // 
            this.whiteList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            fieldDisplaySettings2.Table = null;
            this.whiteList.FieldDisplaySettings = fieldDisplaySettings2;
            this.whiteList.FullRowSelect = true;
            this.whiteList.Location = new System.Drawing.Point(0, 16);
            this.whiteList.Name = "whiteList";
            this.whiteList.Size = new System.Drawing.Size(222, 104);
            this.whiteList.TabIndex = 27;
            this.whiteList.View = System.Windows.Forms.View.Details;
            // 
            // criteriaListPanel1
            // 
            this.criteriaListPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.criteriaListPanel1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.criteriaListPanel1.DBManager = null;
            this.criteriaListPanel1.Location = new System.Drawing.Point(166, 66);
            this.criteriaListPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.criteriaListPanel1.MinimumSize = new System.Drawing.Size(483, 120);
            this.criteriaListPanel1.Name = "criteriaListPanel1";
            this.criteriaListPanel1.Size = new System.Drawing.Size(483, 120);
            this.criteriaListPanel1.TabIndex = 5;
            this.criteriaListPanel1.Table = null;
            // 
            // GenericFilterEditorPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.filterNameTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.criteriaListPanel1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(690, 370);
            this.Name = "GenericFilterEditorPane";
            this.Size = new System.Drawing.Size(690, 370);
            this.Load += new System.EventHandler(this.FilterEditorPane_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.toolStrip3.ResumeLayout(false);
            this.toolStrip3.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox filterGroupingCombo;
        private Cornerstone.GUI.Filtering.CriteriaListPanel criteriaListPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton1;
        private System.Windows.Forms.ToolStripMenuItem addNewRuleToolStripMenuItem;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox filterNameTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem addNewSubFilterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addExistingSubFilterToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip3;
        private System.Windows.Forms.ToolStripButton blacklistAddButton;
        private System.Windows.Forms.ToolStripButton blacklistRemoveButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton whitelistAddButton;
        private System.Windows.Forms.ToolStripButton whitelistRemoveButton;
        private Cornerstone.GUI.Controls.DBObjectListEditor whiteList;
        private Cornerstone.GUI.Controls.DBObjectListEditor blackList;

    }
}
