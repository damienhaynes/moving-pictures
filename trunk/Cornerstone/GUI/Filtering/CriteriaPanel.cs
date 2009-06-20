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


        private bool fieldChanging = false;
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
            valueInputField.ComboBox.SelectedIndexChanged += new EventHandler(valueComboBox_SelectedIndexChanged);
            valueInputField.ComboBox.TextChanged += new EventHandler(valueComboBox_SelectedIndexChanged);

        }

        private void CriteriaPanel_Enter(object sender, EventArgs e) {
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            if (Selected != null) Selected(this);
        }

        private void CriteriaPanel_Leave(object sender, EventArgs e) {
            this.BackColor = Color.White;
        }

        private void populateFieldCombo() {
            fieldChanging = true;

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

            fieldChanging = false;
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
            if (!Criteria.Field.AllowManualFilterInput ||
                Criteria.Field.Type == typeof(string) ||
                Criteria.Field.Type == typeof(StringList) ||
                Criteria.Field.Type == typeof(bool)) {
             
                HashSet<string> values = DBManager.GetAllValues(Criteria.Field);

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
            } else
                valueInputField.InputType = CriteriaInputType.STRING;

            // set value
            if (Criteria.Value == null) {
                valueInputField.ComboBox.Text = "";
                valueInputField.TextBox.Text = "";
            }
            else {
                valueInputField.ComboBox.Text = Criteria.Value.ToString();
                valueInputField.TextBox.Text = Criteria.Value.ToString();
            }
        }

        private void fieldComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (Criteria != null) {
                Criteria.Field = ((ComboFieldWrapper)fieldComboBox.SelectedItem).Field;
                Criteria.Relation = ((ComboFieldWrapper)fieldComboBox.SelectedItem).Relation;
            }

            if (!fieldChanging)
                Criteria.Value = "";

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

        void valueComboBox_SelectedIndexChanged(object sender, EventArgs e) {
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
