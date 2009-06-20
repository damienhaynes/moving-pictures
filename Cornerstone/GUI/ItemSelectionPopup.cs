using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Cornerstone.GUI.Controls;
using Cornerstone.Database.Tables;
using Cornerstone.Database.CustomTypes;

namespace Cornerstone.GUI {
    public partial class ItemSelectionPopup : Form, IFieldDisplaySettingsOwner {

        public enum ExitStatusEnum { OK, CANCEL }

        public ItemSelectionPopup() {
            InitializeComponent();
        }

        public bool ShowCancelButton {
            get { return _showCancelButton; }
            
            set { 
                _showCancelButton = value;
                cancelButton.Visible = value;
            }
        } private bool _showCancelButton = true;

        [Category("Cornerstone Settings")]
        [Description("Manage the type of database table this control connects to and which fields should be displayed.")]
        public FieldDisplaySettings FieldDisplaySettings {
            get {
                if (_fieldSettings == null) {
                    _fieldSettings = new FieldDisplaySettings();
                    _fieldSettings.Owner = this;
                }

                return _fieldSettings;
            }

            set {
                _fieldSettings = value;
                _fieldSettings.Owner = this;
            }
        } private FieldDisplaySettings _fieldSettings = null;

        public DBObjectListEditor ItemList {
            get {
                return itemList;
            }
        }

        public ExitStatusEnum ExitStatus {
            get { return _exitStatus; }
        } private ExitStatusEnum _exitStatus;

        public void OnFieldPropertiesChanged() {
            itemList.FieldDisplaySettings.Table = FieldDisplaySettings.Table;
            itemList.FieldDisplaySettings.FieldProperties = FieldDisplaySettings.FieldProperties;
        }

        private void okButton_Click(object sender, EventArgs e) {
            _exitStatus = ExitStatusEnum.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e) {
            _exitStatus = ExitStatusEnum.CANCEL;
            Close();
        }

    }
}
