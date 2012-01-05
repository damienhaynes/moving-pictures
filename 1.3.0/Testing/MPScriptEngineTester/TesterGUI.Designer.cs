namespace MPScriptEngineTester {
    partial class TesterGUI {
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
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.actionsPanel = new System.Windows.Forms.Panel();
            this.actionsListBox = new System.Windows.Forms.ListBox();
            this.actionsToolStrip = new System.Windows.Forms.ToolStrip();
            this.removeActionButton = new System.Windows.Forms.ToolStripButton();
            this.addActionButton = new System.Windows.Forms.ToolStripButton();
            this.actionsLabel = new System.Windows.Forms.ToolStripLabel();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.removeStepButton = new System.Windows.Forms.ToolStripButton();
            this.addStepButton = new System.Windows.Forms.ToolStripButton();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.inputTabControl = new System.Windows.Forms.TabControl();
            this.inputPlainTextTabPage = new System.Windows.Forms.TabPage();
            this.inputPlainTextBox = new System.Windows.Forms.TextBox();
            this.inputHtmlTabPage = new System.Windows.Forms.TabPage();
            this.inputWebBrowser = new System.Windows.Forms.WebBrowser();
            this.inputToolStrip = new System.Windows.Forms.ToolStrip();
            this.nodeInputLabel = new System.Windows.Forms.ToolStripLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.openScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveScriptAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.regexGuideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutScriptEngineTesterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.parsedTextTabPage = new System.Windows.Forms.TabPage();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.actionsPanel.SuspendLayout();
            this.actionsToolStrip.SuspendLayout();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.inputTabControl.SuspendLayout();
            this.inputPlainTextTabPage.SuspendLayout();
            this.inputHtmlTabPage.SuspendLayout();
            this.inputToolStrip.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.mainSplitContainer.Location = new System.Drawing.Point(0, 24);
            this.mainSplitContainer.Margin = new System.Windows.Forms.Padding(0);
            this.mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.Controls.Add(this.splitContainer3);
            this.mainSplitContainer.Panel1.Margin = new System.Windows.Forms.Padding(3);
            // 
            // mainSplitContainer.Panel2
            // 
            this.mainSplitContainer.Panel2.Controls.Add(this.splitContainer2);
            this.mainSplitContainer.Size = new System.Drawing.Size(882, 636);
            this.mainSplitContainer.SplitterDistance = 173;
            this.mainSplitContainer.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.actionsPanel);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer4);
            this.splitContainer3.Size = new System.Drawing.Size(173, 636);
            this.splitContainer3.SplitterDistance = 118;
            this.splitContainer3.TabIndex = 0;
            // 
            // actionsPanel
            // 
            this.actionsPanel.Controls.Add(this.actionsListBox);
            this.actionsPanel.Controls.Add(this.actionsToolStrip);
            this.actionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionsPanel.Location = new System.Drawing.Point(0, 0);
            this.actionsPanel.Name = "actionsPanel";
            this.actionsPanel.Size = new System.Drawing.Size(173, 118);
            this.actionsPanel.TabIndex = 0;
            // 
            // actionsListBox
            // 
            this.actionsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.actionsListBox.FormattingEnabled = true;
            this.actionsListBox.Location = new System.Drawing.Point(0, 28);
            this.actionsListBox.Name = "actionsListBox";
            this.actionsListBox.Size = new System.Drawing.Size(173, 82);
            this.actionsListBox.TabIndex = 2;
            // 
            // actionsToolStrip
            // 
            this.actionsToolStrip.AutoSize = false;
            this.actionsToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.actionsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeActionButton,
            this.addActionButton,
            this.actionsLabel});
            this.actionsToolStrip.Location = new System.Drawing.Point(0, 0);
            this.actionsToolStrip.Name = "actionsToolStrip";
            this.actionsToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.actionsToolStrip.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.actionsToolStrip.Size = new System.Drawing.Size(173, 25);
            this.actionsToolStrip.TabIndex = 4;
            this.actionsToolStrip.Text = "actionsToolStrip";
            // 
            // removeActionButton
            // 
            this.removeActionButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.removeActionButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.removeActionButton.Image = global::MPScriptEngineTester.Properties.Resources.list_remove;
            this.removeActionButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.removeActionButton.Name = "removeActionButton";
            this.removeActionButton.Size = new System.Drawing.Size(23, 22);
            this.removeActionButton.Text = "Remove Selected Action";
            // 
            // addActionButton
            // 
            this.addActionButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.addActionButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.addActionButton.Image = global::MPScriptEngineTester.Properties.Resources.list_add;
            this.addActionButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addActionButton.Name = "addActionButton";
            this.addActionButton.Size = new System.Drawing.Size(23, 22);
            this.addActionButton.Text = "Add New Action";
            // 
            // actionsLabel
            // 
            this.actionsLabel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.actionsLabel.Name = "actionsLabel";
            this.actionsLabel.Size = new System.Drawing.Size(61, 22);
            this.actionsLabel.Text = "Actions";
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.treeView1);
            this.splitContainer4.Panel1.Controls.Add(this.toolStrip1);
            this.splitContainer4.Size = new System.Drawing.Size(173, 514);
            this.splitContainer4.SplitterDistance = 319;
            this.splitContainer4.TabIndex = 0;
            // 
            // treeView1
            // 
            this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView1.Location = new System.Drawing.Point(0, 28);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(181, 296);
            this.treeView1.TabIndex = 1;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.removeStepButton,
            this.addStepButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(173, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "scriptToolStrip";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(49, 22);
            this.toolStripLabel1.Text = "Script";
            // 
            // removeStepButton
            // 
            this.removeStepButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.removeStepButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.removeStepButton.Image = global::MPScriptEngineTester.Properties.Resources.list_remove;
            this.removeStepButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.removeStepButton.Name = "removeStepButton";
            this.removeStepButton.Size = new System.Drawing.Size(23, 22);
            this.removeStepButton.Text = "Remove Selected Step";
            // 
            // addStepButton
            // 
            this.addStepButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.addStepButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.addStepButton.Image = global::MPScriptEngineTester.Properties.Resources.list_add;
            this.addStepButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addStepButton.Name = "addStepButton";
            this.addStepButton.Size = new System.Drawing.Size(23, 22);
            this.addStepButton.Text = "Insert New Step";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer1);
            this.splitContainer2.Size = new System.Drawing.Size(705, 636);
            this.splitContainer2.SplitterDistance = 510;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.inputTabControl);
            this.splitContainer1.Panel1.Controls.Add(this.inputToolStrip);
            this.splitContainer1.Size = new System.Drawing.Size(510, 636);
            this.splitContainer1.SplitterDistance = 181;
            this.splitContainer1.TabIndex = 0;
            // 
            // inputTabControl
            // 
            this.inputTabControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.inputTabControl.Controls.Add(this.inputPlainTextTabPage);
            this.inputTabControl.Controls.Add(this.parsedTextTabPage);
            this.inputTabControl.Controls.Add(this.inputHtmlTabPage);
            this.inputTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputTabControl.Location = new System.Drawing.Point(0, 25);
            this.inputTabControl.Name = "inputTabControl";
            this.inputTabControl.SelectedIndex = 0;
            this.inputTabControl.Size = new System.Drawing.Size(510, 156);
            this.inputTabControl.TabIndex = 1;
            // 
            // inputPlainTextTabPage
            // 
            this.inputPlainTextTabPage.Controls.Add(this.inputPlainTextBox);
            this.inputPlainTextTabPage.Location = new System.Drawing.Point(4, 25);
            this.inputPlainTextTabPage.Name = "inputPlainTextTabPage";
            this.inputPlainTextTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.inputPlainTextTabPage.Size = new System.Drawing.Size(502, 127);
            this.inputPlainTextTabPage.TabIndex = 0;
            this.inputPlainTextTabPage.Text = "Plain Text";
            this.inputPlainTextTabPage.UseVisualStyleBackColor = true;
            // 
            // inputPlainTextBox
            // 
            this.inputPlainTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputPlainTextBox.Location = new System.Drawing.Point(3, 3);
            this.inputPlainTextBox.Multiline = true;
            this.inputPlainTextBox.Name = "inputPlainTextBox";
            this.inputPlainTextBox.Size = new System.Drawing.Size(496, 121);
            this.inputPlainTextBox.TabIndex = 0;
            // 
            // inputHtmlTabPage
            // 
            this.inputHtmlTabPage.Controls.Add(this.inputWebBrowser);
            this.inputHtmlTabPage.Location = new System.Drawing.Point(4, 25);
            this.inputHtmlTabPage.Name = "inputHtmlTabPage";
            this.inputHtmlTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.inputHtmlTabPage.Size = new System.Drawing.Size(502, 127);
            this.inputHtmlTabPage.TabIndex = 1;
            this.inputHtmlTabPage.Text = "Rendered HTML";
            this.inputHtmlTabPage.UseVisualStyleBackColor = true;
            // 
            // inputWebBrowser
            // 
            this.inputWebBrowser.AllowNavigation = false;
            this.inputWebBrowser.AllowWebBrowserDrop = false;
            this.inputWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputWebBrowser.IsWebBrowserContextMenuEnabled = false;
            this.inputWebBrowser.Location = new System.Drawing.Point(3, 3);
            this.inputWebBrowser.Margin = new System.Windows.Forms.Padding(4);
            this.inputWebBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.inputWebBrowser.Name = "inputWebBrowser";
            this.inputWebBrowser.Size = new System.Drawing.Size(496, 121);
            this.inputWebBrowser.TabIndex = 0;
            this.inputWebBrowser.Url = new System.Uri("http://www.google.com", System.UriKind.Absolute);
            this.inputWebBrowser.WebBrowserShortcutsEnabled = false;
            // 
            // inputToolStrip
            // 
            this.inputToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.inputToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nodeInputLabel});
            this.inputToolStrip.Location = new System.Drawing.Point(0, 0);
            this.inputToolStrip.Name = "inputToolStrip";
            this.inputToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.inputToolStrip.Size = new System.Drawing.Size(510, 25);
            this.inputToolStrip.TabIndex = 0;
            this.inputToolStrip.Text = "toolStrip2";
            this.inputToolStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.inputToolStrip_ItemClicked);
            // 
            // nodeInputLabel
            // 
            this.nodeInputLabel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nodeInputLabel.Name = "nodeInputLabel";
            this.nodeInputLabel.Size = new System.Drawing.Size(89, 22);
            this.nodeInputLabel.Text = "Node Input";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuStrip1.Size = new System.Drawing.Size(882, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.openScriptToolStripMenuItem,
            this.toolStripMenuItem2,
            this.toolStripSeparator1,
            this.saveScriptToolStripMenuItem,
            this.saveScriptAsToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(166, 22);
            this.toolStripMenuItem1.Text = "New Script";
            // 
            // openScriptToolStripMenuItem
            // 
            this.openScriptToolStripMenuItem.Name = "openScriptToolStripMenuItem";
            this.openScriptToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.openScriptToolStripMenuItem.Text = "Open Script";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(166, 22);
            this.toolStripMenuItem2.Text = "Close Script";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(163, 6);
            // 
            // saveScriptToolStripMenuItem
            // 
            this.saveScriptToolStripMenuItem.Name = "saveScriptToolStripMenuItem";
            this.saveScriptToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.saveScriptToolStripMenuItem.Text = "Save Script";
            // 
            // saveScriptAsToolStripMenuItem
            // 
            this.saveScriptAsToolStripMenuItem.Name = "saveScriptAsToolStripMenuItem";
            this.saveScriptAsToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.saveScriptAsToolStripMenuItem.Text = "Save Script As...";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(163, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.regexGuideToolStripMenuItem,
            this.aboutScriptEngineTesterToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // regexGuideToolStripMenuItem
            // 
            this.regexGuideToolStripMenuItem.Name = "regexGuideToolStripMenuItem";
            this.regexGuideToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.regexGuideToolStripMenuItem.Text = "Regex Guide";
            // 
            // aboutScriptEngineTesterToolStripMenuItem
            // 
            this.aboutScriptEngineTesterToolStripMenuItem.Name = "aboutScriptEngineTesterToolStripMenuItem";
            this.aboutScriptEngineTesterToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.aboutScriptEngineTesterToolStripMenuItem.Text = "About ScriptEngineTester";
            // 
            // parsedTextTabPage
            // 
            this.parsedTextTabPage.Location = new System.Drawing.Point(4, 25);
            this.parsedTextTabPage.Name = "parsedTextTabPage";
            this.parsedTextTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.parsedTextTabPage.Size = new System.Drawing.Size(502, 127);
            this.parsedTextTabPage.TabIndex = 2;
            this.parsedTextTabPage.Text = "Parsed Text";
            this.parsedTextTabPage.UseVisualStyleBackColor = true;
            // 
            // TesterGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(882, 660);
            this.Controls.Add(this.mainSplitContainer);
            this.Controls.Add(this.menuStrip1);
            this.Name = "TesterGUI";
            this.Text = "TesterGUI";
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            this.mainSplitContainer.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            this.actionsPanel.ResumeLayout(false);
            this.actionsToolStrip.ResumeLayout(false);
            this.actionsToolStrip.PerformLayout();
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel1.PerformLayout();
            this.splitContainer4.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.inputTabControl.ResumeLayout(false);
            this.inputPlainTextTabPage.ResumeLayout(false);
            this.inputPlainTextTabPage.PerformLayout();
            this.inputHtmlTabPage.ResumeLayout(false);
            this.inputToolStrip.ResumeLayout(false);
            this.inputToolStrip.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveScriptAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem regexGuideToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutScriptEngineTesterToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.ListBox actionsListBox;
        private System.Windows.Forms.ToolStrip actionsToolStrip;
        private System.Windows.Forms.ToolStripButton removeActionButton;
        private System.Windows.Forms.ToolStripButton addActionButton;
        private System.Windows.Forms.Panel actionsPanel;
        private System.Windows.Forms.ToolStripLabel actionsLabel;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ToolStripButton removeStepButton;
        private System.Windows.Forms.ToolStripButton addStepButton;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl inputTabControl;
        private System.Windows.Forms.TabPage inputPlainTextTabPage;
        private System.Windows.Forms.TabPage inputHtmlTabPage;
        private System.Windows.Forms.ToolStrip inputToolStrip;
        private System.Windows.Forms.ToolStripLabel nodeInputLabel;
        private System.Windows.Forms.TextBox inputPlainTextBox;
        private System.Windows.Forms.WebBrowser inputWebBrowser;
        private System.Windows.Forms.TabPage parsedTextTabPage;
    }
}