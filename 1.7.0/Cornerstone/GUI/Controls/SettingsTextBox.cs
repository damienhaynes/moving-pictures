using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Cornerstone.Database.Tables;
using System.Drawing;

namespace Cornerstone.GUI.Controls {
    public class SettingsTextBox: TextBox {
        public DBSetting Setting {
            get { return _setting; }
            set {
                if (value == null)
                    return;

                _setting = value;

                // Update the control to reflect the setting
                Text = _setting.StringValue;
            }
        }
        private DBSetting _setting;

        public SettingsTextBox() {
            this.LostFocus += new EventHandler(SettingsTextBox_LostFocus);
            this.TextChanged += new EventHandler(SettingsTextBox_TextChanged);
        }

        void SettingsTextBox_TextChanged(object sender, EventArgs e) {
            if (DesignMode)
                return;

            if (_setting.Validate(Text))
                ForeColor = DefaultForeColor;
            else
                ForeColor = Color.Red;
        }

        void SettingsTextBox_LostFocus(object sender, EventArgs e) {
            if (DesignMode)
                return;
            
            if (_setting.Validate(Text)) {
                _setting.Value = Text;
                _setting.Commit();
            }
            else {
                Text = _setting.StringValue;
            }

        }

    }
}
