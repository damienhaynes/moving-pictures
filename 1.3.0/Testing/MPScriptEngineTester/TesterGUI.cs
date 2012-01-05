using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MPScriptEngineTester {
    public partial class TesterGUI : Form {
        public TesterGUI() {
            InitializeComponent();
        }

        [STAThreadAttribute]
        static void Main(string[] args) {
            new TesterGUI().ShowDialog();
        }

        private void inputToolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {

        }


    }
}
