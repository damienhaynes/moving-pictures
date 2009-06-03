using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Cornerstone.Database.Tables;
using Cornerstone.Database;
using System.Reflection;
using Cornerstone.Database.CustomTypes;

namespace Cornerstone.GUI.Filtering {
    public partial class CriteriaPanel<T> : UserControl where T: DatabaseTable {
        public event SelectedDelegate<T> Selected;

        [Browsable(false)]
        public DatabaseManager DBManager {
            get {
                return _dbManager;
            }
            set {
                _dbManager = value;
            }
        } private DatabaseManager _dbManager;

        public DBCriteria<T> Criteria {
            get { return _criteria; }
            
            set { 
                _criteria = value;
                populateFieldCombo();
                populateValue();
            }
        } private DBCriteria<T> _criteria;

        public CriteriaPanel() {
            InitializeComponent();

            Margin = new Padding(0);
            Dock = DockStyle.Fill;

            valueInputField.TextBox.TextChanged += new EventHandler(valueTextBox_TextChanged);
            valueInputField.ComboBox.SelectedIndexChanged += new EventHandler(ComboBox_SelectedIndexChanged);
            valueInputField.ComboBox.TextChanged += new EventHandler(ComboBox_SelectedIndexChanged);

        }

        private void CriteriaPanel_Enter(object sender, EventArgs e) {
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            if (Selected != null) Selected(this);
        }

        private void CriteriaPanel_Leave(object sender, EventArgs e) {
            this.BackColor = Color.White;
        }

        private void populateFieldCombo() {
            fieldComboBox.Items.Clear();
            foreach(DBField currField in DBField.GetFieldList(typeof(T))) {
                //if (_criteria.GetOperators(currField).Count != 0)
                    fieldComboBox.Items.Add(currField);
            }

            if (Criteria != null && Criteria.Field != null)
                fieldComboBox.SelectedItem = Criteria.Field;
            else
                fieldComboBox.SelectedIndex = 0;

            populateOperatorCombo();
        }

        private void populateOperatorCombo() {
            if (_criteria == null) return;

            operatorComboBox.Items.Clear();
            foreach (DBCriteria<T>.OperatorEnum currOp in _criteria.GetOperators()) {
                ComboEnumWrapper currItem = new ComboEnumWrapper(currOp);
                operatorComboBox.Items.Add(currItem);

                if (currOp == _criteria.Operator)
                    operatorComboBox.SelectedItem = currItem;
            }

            if (operatorComboBox.SelectedItem == null && operatorComboBox.Items.Count > 0)
                operatorComboBox.SelectedIndex = 0;
        }

        private void populateValue() {
            if (Criteria == null || DBManager == null) 
                return;

            if (Criteria.Field.Type == typeof(StringList)) {
                valueInputField.InputType = CriteriaInputType.COMBO;

                // loop through all items in the DB and grab all existing values for this field
                // use a dictionary because there is no Set class in .NET 2.0 :(
                Dictionary<string, bool> uniqueStrings = new Dictionary<string, bool>();
                List<T> items = DBManager.Get<T>(null);
                foreach (T currItem in items) {
                    StringList values = (StringList)Criteria.Field.GetValue(currItem);
                    foreach (string currValue in values) {
                        uniqueStrings[currValue] = true;
                    }
                }

                // add all unique strings to the drop down
                valueInputField.ComboBox.Items.Clear();
                foreach (string currValue in uniqueStrings.Keys) {
                    valueInputField.ComboBox.Items.Add(currValue);
                }

                // set value
                if (Criteria.Value == null) valueInputField.ComboBox.Text = "";
                else valueInputField.ComboBox.Text = Criteria.Value.ToString();

            }
            else if (Criteria.Field.Type == typeof(string)) {
                valueInputField.InputType = CriteriaInputType.COMBO;

                // loop through all items in the DB and grab all existing values for this field
                // use a dictionary because there is no Set class in .NET 2.0 :(
                Dictionary<string, bool> uniqueStrings = new Dictionary<string, bool>();
                List<T> items = DBManager.Get<T>(null);
                foreach (T currItem in items) {
                    string value = (string)Criteria.Field.GetValue(currItem);
                    uniqueStrings[value] = true;
                }

                // add all unique strings to the drop down
                valueInputField.ComboBox.Items.Clear();
                foreach (string currValue in uniqueStrings.Keys) {
                    valueInputField.ComboBox.Items.Add(currValue);
                }

                // set value
                if (Criteria.Value == null) valueInputField.ComboBox.Text = "";
                else valueInputField.ComboBox.Text = Criteria.Value.ToString();
            }
            else {
                valueInputField.InputType = CriteriaInputType.STRING;
                if (Criteria.Value == null)
                    valueInputField.TextBox.Text = "";
                else
                    valueInputField.TextBox.Text = Criteria.Value.ToString();
            }
        }

        private void fieldComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (Criteria != null) 
                Criteria.Field = (DBField)fieldComboBox.SelectedItem;

            populateOperatorCombo();
            populateValue();
        }

        private void operatorComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (Criteria != null)
                Criteria.Operator = (DBCriteria<T>.OperatorEnum)((ComboEnumWrapper)operatorComboBox.SelectedItem).Value;
        }

        private void valueTextBox_TextChanged(object sender, EventArgs e) {
            if (Criteria != null)
                Criteria.Value = valueInputField.TextBox.Text;
        }

        void ComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (Criteria != null)
                Criteria.Value = valueInputField.ComboBox.Text;
        }

    }

    public delegate void SelectedDelegate<T>(CriteriaPanel<T> obj) where T: DatabaseTable;

    class ComboEnumWrapper {
        public string Name {
            get {
                if (_name == null) {
                    Type type = Value.GetType();
                    MemberInfo[] memInfo = type.GetMember(Value.ToString());

                    if (memInfo != null && memInfo.Length > 0) {
                        object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                        if (attrs != null && attrs.Length > 0)
                            _name = ((DescriptionAttribute)attrs[0]).Description;
                    } 
                    else
                        _name = Value.ToString();
                }
                
                return _name; 
            }
        } private string _name = null;
        
        public Enum Value {
            get { return _value; }
        } private Enum _value;

        public ComboEnumWrapper(Enum value) {
            this._value = value;
        }

        public override string ToString() {
            return Name;
        }
    }
}
