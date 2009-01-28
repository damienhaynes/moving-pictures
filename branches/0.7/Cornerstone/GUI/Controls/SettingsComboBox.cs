using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Cornerstone.Database.Tables;

namespace Cornerstone.GUI.Controls {
    public class SettingsComboBox: ComboBox {
        public class FormatedComboBoxItem {
            public object Value {
                get { return _value; }
                set { _value = value; }
            } private object _value;

            public override string ToString() {
                return MakeHumanReadable(Value.ToString());
            }

            public string MakeHumanReadable(string input) {
                string rtn = "";

                char prevChar = char.MinValue;
                foreach (char currChar in input) {
                    if (prevChar != char.MinValue && char.IsLower(prevChar) && char.IsUpper(currChar))
                        rtn += " ";

                    rtn += currChar;
                    prevChar = currChar;
                }

                return rtn;
            }
        }

        #region Properties

        public DBSetting Setting {
            get { return _setting; }
            set {
                if (value == null)
                    return;

                _setting = value;

                populateCombo();
            }
        }
        private DBSetting _setting = null;

        public Type EnumType {
            get { return _enumType; }
            set {
                if (value == null)
                    return;
                
                if (!value.IsEnum)
                    throw new ArgumentException("SettingsComboBox.EnumType field must be set to an enum type.");

                _enumType = value;

                populateCombo();
            }
        } private Type _enumType = null;

        #endregion

        private Dictionary<Enum, FormatedComboBoxItem> comboItems;

        public SettingsComboBox() {
            comboItems = new Dictionary<Enum, FormatedComboBoxItem>();
            SelectedIndexChanged += new EventHandler(SettingsComboBox_SelectedIndexChanged);
            DropDownStyle = ComboBoxStyle.DropDownList;
        }

        void SettingsComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            _setting.Value = ((FormatedComboBoxItem)SelectedItem).Value;
            _setting.Commit();
        }

        private void populateCombo() {
            if (_enumType == null || _setting == null)
                return;

            // add all items
            foreach (Enum currValue in Enum.GetValues(_enumType)) {
                FormatedComboBoxItem newItem = new FormatedComboBoxItem();
                newItem.Value = currValue;
                comboItems[currValue] = newItem;
                Items.Add(newItem);
            }

            // set selected item
            try {
                Enum value = (Enum)Enum.Parse(_enumType, _setting.StringValue);
                SelectedItem = comboItems[value];
            }
            catch {
                SelectedIndex = 0;
            }
            
        }

    }
}
