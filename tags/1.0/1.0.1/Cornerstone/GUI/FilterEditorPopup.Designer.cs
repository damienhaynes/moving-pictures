namespace Cornerstone.GUI {
    partial class FilterEditorPopup {
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
            Cornerstone.GUI.Controls.FieldDisplaySettings fieldDisplaySettings2 = new Cornerstone.GUI.Controls.FieldDisplaySettings();
            this.button1 = new System.Windows.Forms.Button();
            this.helpButton = new System.Windows.Forms.Button();
            this.FilterEditorPanel = new Cornerstone.GUI.Filtering.FilterEditorPane();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(603, 332);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // helpButton
            // 
            this.helpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.helpButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.helpButton.Image = global::Cornerstone.Properties.Resources.help;
            this.helpButton.Location = new System.Drawing.Point(655, 4);
            this.helpButton.Margin = new System.Windows.Forms.Padding(0);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(23, 23);
            this.helpButton.TabIndex = 17;
            this.helpButton.UseVisualStyleBackColor = true;
            this.helpButton.Click += new System.EventHandler(this.helpButton_Click);
            // 
            // FilterEditorPanel
            // 
            this.FilterEditorPanel.DBManager = null;
            this.FilterEditorPanel.DisplayName = "items";
            this.FilterEditorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            fieldDisplaySettings2.Table = null;
            this.FilterEditorPanel.FieldDisplaySettings = fieldDisplaySettings2;
            this.FilterEditorPanel.Location = new System.Drawing.Point(0, 0);
            this.FilterEditorPanel.Name = "FilterEditorPanel";
            this.FilterEditorPanel.Size = new System.Drawing.Size(690, 367);
            this.FilterEditorPanel.TabIndex = 0;
            // 
            // FilterEditorPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(690, 367);
            this.ControlBox = false;
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.FilterEditorPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FilterEditorPopup";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Filter Editor";
            this.ResumeLayout(false);

        }

        #endregion

        public Cornerstone.GUI.Filtering.FilterEditorPane FilterEditorPanel;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button helpButton;



    }
}