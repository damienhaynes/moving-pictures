using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cornerstone.Database;

namespace Cornerstone.GUI.Filtering {
    public partial class AddNodePopup : Form {
        public enum CloseStateEnum { ADD_DYNAMIC, ADD_BASIC, CANCEL }

        private Dictionary<DBField, ComboFieldWrapper> wrapperLookup = new Dictionary<DBField, ComboFieldWrapper>();

        public CloseStateEnum CloseState {
            get { return _closeState; }
        } private CloseStateEnum _closeState;


        public AddNodePopup() {
            InitializeComponent();
        }

        // The database object type that is to be filtered.
        public Type Table {
            get { return _table; }
            set { _table = value; }
        } private Type _table;

        public bool ForceTopLevel {
            get { return _forceTopLevel; }
            set { _forceTopLevel = value; }
        } private bool _forceTopLevel;

        public DBField DynamicFilteringField {
            get { return ((ComboFieldWrapper)fieldComboBox.SelectedItem).Field; }
        }

        public DBRelation DynamicFilteringRelation {
            get { return ((ComboFieldWrapper)fieldComboBox.SelectedItem).Relation; }
        }

        public bool AddToRoot {
            get { return rootCheckBox.Checked; }
        }

        private void generateFieldList() {
            // add fields from primary type
            fieldComboBox.Items.Clear();
            foreach (DBField currField in DBField.GetFieldList(Table)) {
                if (currField.Filterable && currField.AllowDynamicFiltering) {
                    ComboFieldWrapper wrapper = new ComboFieldWrapper(currField);
                    wrapperLookup[currField] = wrapper;
                    fieldComboBox.Items.Add(wrapper);
                }
            }

            // add fields from secondary types
            foreach (DBRelation currRelation in DBRelation.GetRelations(Table)) {
                if (!currRelation.Filterable)
                    continue;

                foreach (DBField currField in DBField.GetFieldList(currRelation.SecondaryType)) {
                    if (currField.Filterable && currField.AllowDynamicFiltering) {
                        ComboFieldWrapper wrapper = new ComboFieldWrapper(currField, currRelation);
                        wrapperLookup[currField] = wrapper;
                        fieldComboBox.Items.Add(wrapper);
                    }
                }
            }

            fieldComboBox.SelectedIndex = 0;
        }

        private void okButton_Click(object sender, EventArgs e) {
            if (dynamicNodeRadioButton.Checked)
                _closeState = CloseStateEnum.ADD_DYNAMIC;
            else if (emptyNodeRadioButton.Checked)
                _closeState = CloseStateEnum.ADD_BASIC;
            else _closeState = CloseStateEnum.CANCEL;

            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e) {
            _closeState = CloseStateEnum.CANCEL; 
            Close();
        }

        private void AddNodePopup_Load(object sender, EventArgs e) {
            generateFieldList();
            if (ForceTopLevel) {
                rootCheckBox.Checked = true;
                rootCheckBox.Enabled = false;
            }
        }

        private void dynamicNodeRadioButton_CheckedChanged(object sender, EventArgs e) {
            fieldComboBox.Enabled = dynamicNodeRadioButton.Checked;
        }

        private void emptyNodeRadioButton_CheckedChanged(object sender, EventArgs e) {
            fieldComboBox.Enabled = dynamicNodeRadioButton.Checked;
        }
    }
}
