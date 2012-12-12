namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups
{
    partial class IgnoredFilesManager
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.unignoreSelectedButton = new System.Windows.Forms.Button();
            this.ignoredMovieGrid = new System.Windows.Forms.DataGridView();
            this.ignoredMediaColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.ignoredMovieGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // unignoreSelectedButton
            // 
            this.unignoreSelectedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.unignoreSelectedButton.Location = new System.Drawing.Point(168, 291);
            this.unignoreSelectedButton.Name = "unignoreSelectedButton";
            this.unignoreSelectedButton.Size = new System.Drawing.Size(134, 23);
            this.unignoreSelectedButton.TabIndex = 5;
            this.unignoreSelectedButton.Text = "Restore Selected Files";
            this.unignoreSelectedButton.UseVisualStyleBackColor = true;
            this.unignoreSelectedButton.Click += new System.EventHandler(this.unignoreSelectedButton_Click);
            // 
            // ignoredMovieGrid
            // 
            this.ignoredMovieGrid.AllowUserToAddRows = false;
            this.ignoredMovieGrid.AllowUserToDeleteRows = false;
            this.ignoredMovieGrid.AllowUserToResizeRows = false;
            this.ignoredMovieGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ignoredMovieGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.ignoredMovieGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.ignoredMovieGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ignoredMovieGrid.ColumnHeadersVisible = false;
            this.ignoredMovieGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ignoredMediaColumn});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.ignoredMovieGrid.DefaultCellStyle = dataGridViewCellStyle2;
            this.ignoredMovieGrid.Location = new System.Drawing.Point(12, 12);
            this.ignoredMovieGrid.Name = "ignoredMovieGrid";
            this.ignoredMovieGrid.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.ignoredMovieGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.ignoredMovieGrid.RowHeadersVisible = false;
            this.ignoredMovieGrid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ignoredMovieGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ignoredMovieGrid.Size = new System.Drawing.Size(290, 219);
            this.ignoredMovieGrid.TabIndex = 7;
            // 
            // ignoredMediaColumn
            // 
            this.ignoredMediaColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ignoredMediaColumn.HeaderText = "Ignored File(s)";
            this.ignoredMediaColumn.Name = "ignoredMediaColumn";
            this.ignoredMediaColumn.ReadOnly = true;
            this.ignoredMediaColumn.ToolTipText = "List of ignored movies.";
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn1.HeaderText = "Ignored File(s)";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.ToolTipText = "List of ignored movies.";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(13, 238);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(289, 50);
            this.label1.TabIndex = 8;
            this.label1.Text = "Any files you restore will be sent to the importer. This action will cause the im" +
                "porter to restart so be sure you do not have any pending matches to approve.";
            // 
            // IgnoredFilesManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(314, 326);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ignoredMovieGrid);
            this.Controls.Add(this.unignoreSelectedButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "IgnoredFilesManager";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Ignored Files";
            ((System.ComponentModel.ISupportInitialize)(this.ignoredMovieGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button unignoreSelectedButton;
        private System.Windows.Forms.DataGridView ignoredMovieGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn ignoredMediaColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.Label label1;

    }
}
