using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cornerstone.GUI {
    public delegate void HelpActionDelegate(Control sourceControl);

    public partial class FilterEditorPopup : Form {
        public bool ShowHelpButton {
            get { return helpButton.Visible; }
            set { helpButton.Visible = value; }
        }

        public HelpActionDelegate HelpAction {
            get;
            set;
        }

        public FilterEditorPopup() {
            InitializeComponent();

            helpButton.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e) {
            Close();
        }

        private void helpButton_Click(object sender, EventArgs e) {
            if (HelpAction != null)
                HelpAction(this);
        }
    }
}
