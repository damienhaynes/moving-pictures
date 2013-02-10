namespace SearchTester {
    partial class SearchForm {
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
            System.Windows.Forms.ColumnHeader movieHeader;
            System.Windows.Forms.Label label1;
            System.Windows.Forms.ColumnHeader columnHeader1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label4;
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.searchButton = new System.Windows.Forms.Button();
            this.resultsListView = new System.Windows.Forms.ListView();
            this.scoreHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.processingTimeLabel = new System.Windows.Forms.Label();
            this.luceneListView = new System.Windows.Forms.ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.luceneTimeLabel = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.searchModeComboBox = new System.Windows.Forms.ComboBox();
            this.altTitleCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            movieHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            label1 = new System.Windows.Forms.Label();
            columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // movieHeader
            // 
            movieHeader.Text = "Movie";
            movieHeader.Width = 303;
            // 
            // label1
            // 
            label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(257, 90);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(88, 13);
            label1.TabIndex = 3;
            label1.Text = "Processing Time:";
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Movie";
            columnHeader1.Width = 303;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(15, 90);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(149, 13);
            label2.TabIndex = 6;
            label2.Text = "Levenshtein Substring Search";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(15, 315);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(120, 13);
            label3.TabIndex = 9;
            label3.Text = "Apache Lucene Search";
            // 
            // label5
            // 
            label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(257, 315);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(88, 13);
            label5.TabIndex = 7;
            label5.Text = "Processing Time:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(15, 50);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(59, 13);
            label4.TabIndex = 13;
            label4.Text = "Search By:";
            // 
            // searchTextBox
            // 
            this.searchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.searchTextBox.Location = new System.Drawing.Point(13, 13);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(337, 20);
            this.searchTextBox.TabIndex = 0;
            // 
            // searchButton
            // 
            this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchButton.Location = new System.Drawing.Point(356, 10);
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(52, 23);
            this.searchButton.TabIndex = 1;
            this.searchButton.Text = "Search";
            this.searchButton.UseVisualStyleBackColor = true;
            this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
            // 
            // resultsListView
            // 
            this.resultsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            movieHeader,
            this.scoreHeader});
            this.resultsListView.Location = new System.Drawing.Point(15, 109);
            this.resultsListView.Name = "resultsListView";
            this.resultsListView.Size = new System.Drawing.Size(393, 185);
            this.resultsListView.TabIndex = 2;
            this.resultsListView.UseCompatibleStateImageBehavior = false;
            this.resultsListView.View = System.Windows.Forms.View.Details;
            this.resultsListView.DoubleClick += new System.EventHandler(this.resultsListView_DoubleClick);
            // 
            // scoreHeader
            // 
            this.scoreHeader.Text = "Score";
            // 
            // processingTimeLabel
            // 
            this.processingTimeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.processingTimeLabel.AutoSize = true;
            this.processingTimeLabel.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.processingTimeLabel.Location = new System.Drawing.Point(351, 90);
            this.processingTimeLabel.Name = "processingTimeLabel";
            this.processingTimeLabel.Size = new System.Drawing.Size(29, 13);
            this.processingTimeLabel.TabIndex = 4;
            this.processingTimeLabel.Text = "0 ms";
            // 
            // luceneListView
            // 
            this.luceneListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1,
            this.columnHeader2});
            this.luceneListView.Location = new System.Drawing.Point(15, 331);
            this.luceneListView.Name = "luceneListView";
            this.luceneListView.Size = new System.Drawing.Size(393, 185);
            this.luceneListView.TabIndex = 5;
            this.luceneListView.UseCompatibleStateImageBehavior = false;
            this.luceneListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Score";
            // 
            // luceneTimeLabel
            // 
            this.luceneTimeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.luceneTimeLabel.AutoSize = true;
            this.luceneTimeLabel.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.luceneTimeLabel.Location = new System.Drawing.Point(351, 315);
            this.luceneTimeLabel.Name = "luceneTimeLabel";
            this.luceneTimeLabel.Size = new System.Drawing.Size(29, 13);
            this.luceneTimeLabel.TabIndex = 8;
            this.luceneTimeLabel.Text = "0 ms";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Location = new System.Drawing.Point(13, 40);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(395, 3);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // searchModeComboBox
            // 
            this.searchModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.searchModeComboBox.FormattingEnabled = true;
            this.searchModeComboBox.Location = new System.Drawing.Point(80, 47);
            this.searchModeComboBox.Name = "searchModeComboBox";
            this.searchModeComboBox.Size = new System.Drawing.Size(102, 21);
            this.searchModeComboBox.TabIndex = 14;
            this.searchModeComboBox.SelectedIndexChanged += new System.EventHandler(this.searchModeComboBox_SelectedIndexChanged);
            // 
            // altTitleCheckBox
            // 
            this.altTitleCheckBox.AutoSize = true;
            this.altTitleCheckBox.Location = new System.Drawing.Point(277, 49);
            this.altTitleCheckBox.Name = "altTitleCheckBox";
            this.altTitleCheckBox.Size = new System.Drawing.Size(134, 17);
            this.altTitleCheckBox.TabIndex = 15;
            this.altTitleCheckBox.Text = "Include Alternate Titles";
            this.altTitleCheckBox.UseVisualStyleBackColor = true;
            this.altTitleCheckBox.CheckedChanged += new System.EventHandler(this.altTitleCheckBox_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Location = new System.Drawing.Point(13, 74);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(395, 3);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "groupBox2";
            // 
            // SearchForm
            // 
            this.AcceptButton = this.searchButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(418, 531);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.altTitleCheckBox);
            this.Controls.Add(this.searchModeComboBox);
            this.Controls.Add(label4);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(label3);
            this.Controls.Add(this.luceneTimeLabel);
            this.Controls.Add(label5);
            this.Controls.Add(label2);
            this.Controls.Add(this.luceneListView);
            this.Controls.Add(this.processingTimeLabel);
            this.Controls.Add(label1);
            this.Controls.Add(this.resultsListView);
            this.Controls.Add(this.searchButton);
            this.Controls.Add(this.searchTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SearchForm";
            this.ShowIcon = false;
            this.Text = "Search Tester";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.Button searchButton;
        private System.Windows.Forms.ListView resultsListView;
        private System.Windows.Forms.ColumnHeader scoreHeader;
        private System.Windows.Forms.Label processingTimeLabel;
        private System.Windows.Forms.ListView luceneListView;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Label luceneTimeLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox searchModeComboBox;
        private System.Windows.Forms.CheckBox altTitleCheckBox;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}

