using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cornerstone.GUI.Controls;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    internal class PinCodeTextBox: SettingsTextBox {
        public PinCodeTextBox(): base() {
            PasswordChar = '●';
        }
    }
}
