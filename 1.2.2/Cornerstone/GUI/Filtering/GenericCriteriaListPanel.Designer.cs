namespace Cornerstone.GUI.Filtering {
    partial class GenericCriteriaListPanel<T> {
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
            this.listPanel = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();
            // 
            // listPanel
            // 
            this.listPanel.AutoSize = true;
            this.listPanel.BackColor = System.Drawing.SystemColors.Window;
            this.listPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.listPanel.ColumnCount = 1;
            this.listPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.listPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.listPanel.Location = new System.Drawing.Point(0, 0);
            this.listPanel.Name = "listPanel";
            this.listPanel.RowCount = 2;
            this.listPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.listPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.listPanel.Size = new System.Drawing.Size(406, 3);
            this.listPanel.TabIndex = 0;
            // 
            // GenericCriteriaListPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.listPanel);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(410, 40);
            this.Name = "GenericCriteriaListPanel";
            this.Size = new System.Drawing.Size(406, 152);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel listPanel;
    }
}
