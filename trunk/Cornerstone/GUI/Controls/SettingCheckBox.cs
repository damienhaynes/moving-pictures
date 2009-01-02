using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Cornerstone.Database.Tables;

namespace Cornerstone.GUI.Controls {
    public class SettingCheckBox: CheckBox {
        public DBSetting Setting {
            get { return _setting; }
            set {
                if (value == null)
                    return;

                if (!value.Type.Equals("BOOL"))
                    throw new Exception("Invalid Setting Type, SettingCheckBox requires a bool setting.");

                _setting = value;

                // Update the control to reflect the setting
                Text = _setting.Name;
                Checked = (bool)_setting.Value;
           }
        }
        private DBSetting _setting;

        public SettingCheckBox() {
            CheckedChanged += new EventHandler(SettingCheckBox_CheckedChanged);
        }

        void SettingCheckBox_CheckedChanged(object sender, EventArgs e) {
            _setting.Value = Checked;
            _setting.Commit();
        }

        
    }
}
