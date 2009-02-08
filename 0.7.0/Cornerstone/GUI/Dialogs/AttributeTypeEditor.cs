using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Cornerstone.Database.Tables;

namespace Cornerstone.GUI.Dialogs {
    public partial class AttributeTypeEditor : Form {
        public AttributeTypeEditor() {
            InitializeComponent();
        }

        // The object cotnaining the data to be displayed.
        public DBAttrDescription Attribute {
            get { return _attribute; }
            set { _attribute = value; }
        } private DBAttrDescription _attribute = null;

        private void button1_Click(object sender, EventArgs e) {
            if (_attribute != null)
                _attribute.Commit();

            Close();
        }

        private void AttributeTypeEditor_Load(object sender, EventArgs e) {
            attrDescrList.DatabaseObject = _attribute;
        }
    }
}
