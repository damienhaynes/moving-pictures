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

        private Dictionary<DBField, ComboFieldWrapper> wrapperLookup = new Dictionary<DBField, ComboFieldWrapper>();

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
            // add fields from primary type
            fieldComboBox.Items.Clear();
            foreach(DBField currField in DBField.GetFieldList(typeof(T))) {
                if (currField.Filterable) {
                    ComboFieldWrapper wrapper = new ComboFieldWrapper(currField);
                    wrapperLookup[currField] = wrapper;
                    fieldComboBox.Items.Add(wrapper);
                }
            }

            // add fields from secondary types
            foreach (DBRelation currRelation in DBRelation.GetRelations(typeof(T))) {
                if (!currRelation.Filterable)
                    continue;

                foreach (DBField currField in DBField.GetFieldList(currRelation.SecondaryType)) {
                    if (currField.Filterable) {
                        ComboFieldWrapper wrapper = new ComboFieldWrapper(currField, currRelation);
                        wrapperLookup[currField] = wrapper;
                        fieldComboBox.Items.Add(wrapper);
                    }
                }
            }

            if (Criteria != null && Criteria.Field != null && wrapperLookup.ContainsKey(Criteria.Field))
                fieldComboBox.SelectedItem = wrapperLookup[Criteria.Field];
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

            // grab the list of possible values for this field
            List<string> values = getValues(Criteria.Field, Criteria.Relation);

            // if we have possible values, set displaymode to a combo box
            if (values.Count > 0) {
                valueInputField.InputType = CriteriaInputType.COMBO;
                if (Criteria.Field.AllowManualFilterInput)
                    valueInputField.ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                else
                    valueInputField.ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            else
                valueInputField.InputType = CriteriaInputType.STRING;

            // add all possible values to the drop down
            valueInputField.ComboBox.Items.Clear();
            foreach (string currValue in values) {
                valueInputField.ComboBox.Items.Add(currValue);
            }

            // set value
            if (Criteria.Value == null) valueInputField.ComboBox.Text = "";
            else valueInputField.ComboBox.Text = Criteria.Value.ToString();
        }

        private List<string> getValues(DBField field, DBRelation relation) {
            // loop through all items in the DB and grab all existing values for this field
            // use a dictionary because there is no Set class in .NET 2.0 :(
            Dictionary<string, bool> uniqueStrings = new Dictionary<string, bool>();
            List<T> items = DBManager.Get<T>(null);
            foreach (T currItem in items) {
                // if this is a field on a sub table, loop through all sub objects
                if (Criteria.Relation != null) {
                    foreach (object currSubItem in Criteria.Relation.GetRelationList(currItem)) {
                        List<string> values = getValues(Criteria.Field.GetValue((DatabaseTable)currSubItem), !field.AllowManualFilterInput);
                        foreach (string currStr in values) {
                            uniqueStrings[currStr] = true;
                        }
                    }
                }
                // normal field, just add the value(s)
                else {
                    List<string> values = getValues(Criteria.Field.GetValue(currItem), !field.AllowManualFilterInput);
                    foreach (string currStr in values) {
                        uniqueStrings[currStr] = true;
                    }
                }
            }

            // add all unique strings to the result set
            List<string> results = new List<string>();
            foreach (string currValue in uniqueStrings.Keys) 
                results.Add(currValue);

            return results;
        }

        private List<string> getValues(object obj, bool forcePopulation) {
            List<string> results = new List<string>();

            if (obj == null)
                return results;

            if (obj is string) {
                if (((string)obj).Trim().Length != 0)
                    results.Add((string)obj);
            }
            else if (obj is StringList) {
                foreach (string currValue in (StringList)obj) {
                    if (currValue != null && currValue.Trim().Length != 0)
                        results.Add(currValue);
                }
            }
            else if (obj is bool || obj is bool?) {
                results.Add("true");
                results.Add("false");
            }
            else if (forcePopulation) {
                results.Add(obj.ToString());
            }

            return results;
        }

        private void fieldComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (Criteria != null) {
                Criteria.Field = ((ComboFieldWrapper)fieldComboBox.SelectedItem).Field;
                Criteria.Relation = ((ComboFieldWrapper)fieldComboBox.SelectedItem).Relation;
            }

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

    class ComboFieldWrapper {
        public DBField Field {
            get { return _field; }
            set { _field = value; }
        } private DBField _field = null;

        public DBRelation Relation {
            get { return _relation; }
            set { _relation = value; }
        } private DBRelation _relation = null;

        public ComboFieldWrapper(DBField field) {
            _field = field;
        }

        public ComboFieldWrapper(DBField field, DBRelation relation) {
            _field = field;
            _relation = relation;
        }

        public override string ToString() {
            return _field.ToString();
        }
    }

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
