namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    partial class AdvancedSettingsWarningPane {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdvancedSettingsWarningPane));
            this.SettingsPane = new Cornerstone.GUI.AdvancedSettingsPane();
            this.warningPanel = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.warningLabel = new System.Windows.Forms.Label();
            this.warningPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // SettingsPane
            // 
            this.SettingsPane.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SettingsPane.Location = new System.Drawing.Point(0, 0);
            this.SettingsPane.Name = "SettingsPane";
            this.SettingsPane.Size = new System.Drawing.Size(435, 564);
            this.SettingsPane.TabIndex = 0;
            this.SettingsPane.Tag = "";
            // 
            // warningPanel
            // 
            this.warningPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.warningPanel.Controls.Add(this.button1);
            this.warningPanel.Controls.Add(this.label1);
            this.warningPanel.Controls.Add(this.warningLabel);
            this.warningPanel.Location = new System.Drawing.Point(0, 0);
            this.warningPanel.Name = "warningPanel";
            this.warningPanel.Size = new System.Drawing.Size(435, 564);
            this.warningPanel.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.button1.Location = new System.Drawing.Point(141, 356);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(155, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Show Advanced Settings";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(34, 139);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(369, 214);
            this.label1.TabIndex = 1;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // warningLabel
            // 
            this.warningLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.warningLabel.AutoSize = true;
            this.warningLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.warningLabel.ForeColor = System.Drawing.Color.Red;
            this.warningLabel.Location = new System.Drawing.Point(142, 108);
            this.warningLabel.Name = "warningLabel";
            this.warningLabel.Size = new System.Drawing.Size(153, 31);
            this.warningLabel.TabIndex = 0;
            this.warningLabel.Text = "WARNING";
            // 
            // AdvancedSettingsWarningPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.warningPanel);
            this.Controls.Add(this.SettingsPane);
            this.Name = "AdvancedSettingsWarningPane";
            this.Size = new System.Drawing.Size(435, 564);
            this.Load += new System.EventHandler(this.AdvancedSettingsWarningPane_Load);
            this.warningPanel.ResumeLayout(false);
            this.warningPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public Cornerstone.GUI.AdvancedSettingsPane SettingsPane;
        private System.Windows.Forms.Panel warningPanel;
        private System.Windows.Forms.Label warningLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
    }
}
