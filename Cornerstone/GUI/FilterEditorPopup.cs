using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cornerstone.GUI {
    public partial class FilterEditorPopup : Form {
        public FilterEditorPopup() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            Close();
        }
    }
}
