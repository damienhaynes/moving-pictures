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

        public CriteriaInputType InputType {
            get { return _inputType; }
            set {
                _inputType = value;
                UpdateInputField();
            }
        } private CriteriaInputType _inputType = CriteriaInputType.STRING;

        public CriteriaInputField() {
            BorderStyle = BorderStyle.None;
            Name = "CriteriaInputField";
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            ComboBox = new ComboBox();
            ComboBox.Dock = DockStyle.Fill;
            ComboBox.AutoSize = true;
            ComboBox.AutoCompleteMode = AutoCompleteMode.Suggest;
            ComboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
            Controls.Add(ComboBox);

            TextBox = new TextBox(); 
            TextBox.Dock = DockStyle.Fill;
            TextBox.AutoSize = true;
            Controls.Add(TextBox);

            UpdateInputField();
        }

        private void UpdateInputField() {
            SuspendLayout();

            switch (InputType) {
                case CriteriaInputType.COMBO:
                    ComboBox.Visible = true;
                    TextBox.Visible = false;
                    break;

                case CriteriaInputType.INT:
                case CriteriaInputType.REAL:
                case CriteriaInputType.STRING:
                    ComboBox.Visible = false;
                    TextBox.Visible = true;
                    break;
            }

            ResumeLayout(true);
        }
    }
}
