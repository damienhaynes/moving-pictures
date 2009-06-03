using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Cornerstone.GUI.Filtering {
    public enum CriteriaInputType {
        INT, REAL, STRING, COMBO
    }

    public class CriteriaInputField: UserControl {
        public TextBox TextBox;
        public ComboBox ComboBox;
        
        private Panel wrapper;

        public CriteriaInputType InputType {
            get { return _inputType; }
            set {
                _inputType = value;
                InitializeComponent();
            }
        } private CriteriaInputType _inputType = CriteriaInputType.STRING;

        public CriteriaInputField() {
            BorderStyle = BorderStyle.None;
            Name = "CriteriaInputField";
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            wrapper = new Panel();
            wrapper.Dock = DockStyle.Fill;
            wrapper.Margin = new Padding(0);
            wrapper.AutoSize = true;
            wrapper.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(wrapper);

            ComboBox = new ComboBox();
            TextBox = new TextBox();

            InitializeComponent();
        }

        private void InitializeComponent() {
            SuspendLayout();
            switch (InputType) {
                case CriteriaInputType.COMBO:
                    ComboBox.Dock = DockStyle.Fill;
                    ComboBox.AutoSize = true;

                    wrapper.Controls.Clear();
                    wrapper.Controls.Add(ComboBox);
                    break;

                case CriteriaInputType.INT:
                case CriteriaInputType.REAL:
                case CriteriaInputType.STRING:
                    TextBox.Dock = DockStyle.Fill;
                    TextBox.AutoSize = true;

                    wrapper.Controls.Clear();
                    wrapper.Controls.Add(TextBox);
                    break;

            }

            ResumeLayout(false);
        }
    }
}
