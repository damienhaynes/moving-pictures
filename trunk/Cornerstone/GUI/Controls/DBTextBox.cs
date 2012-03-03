using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using Cornerstone.GUI.DesignMode;
using System.Threading;
using System.Globalization;

namespace Cornerstone.GUI.Controls {
    public class DBTextBox : TextBox, IDBFieldBackedControl, IDataGridViewEditingControl {
        #region Properties

        // The database object type that this object displays data about.
        [Category("Display Properties")]
        [Description("The datatype that this control displays. All classes using the DBTableAttribute attribute will be available in the drop down.")]
        [TypeConverter(typeof(DatabaseTableTypeConverter))]
        public Type Table {
            get {
                return _table;
            }
            set {
                _table = value;
                _databaseField = DBField.GetField(_table, _databaseFieldName);
            }
        } private Type _table;

        [Category("Display Properties")]
        [Description("The database field that this control displays.")]
        [TypeConverter(typeof(DBFieldTypeConverter))]
        public String DatabaseFieldName {
            get {
                return _databaseFieldName;
            }
            set {
                _databaseFieldName = value;
                _databaseField = DBField.GetField(_table, _databaseFieldName);
                
                if (_databaseField != null)
                    _dbType = _databaseField.DBType;
                
                RevertText();

                if (DesignMode)
                    Text = _databaseFieldName;
            }
        } private String _databaseFieldName = null;

        [Browsable(false)]
        public DBField DatabaseField {
            get {
                return _databaseField;
            }
        } private DBField _databaseField = null;

        [Browsable(false)]
        public DatabaseTable DatabaseObject {
            get {
                return _databaseObject;
            }
            set {
                _databaseObject = value;
                RevertText();
            }
        } private DatabaseTable _databaseObject = null;

        [Category("Display Properties")]
        [Description("The datatype that this TextBox displays. Can be overridden if the real type is a String")]
        public DBField.DBDataType DBTypeOverride {
            get {
                return _dbType;
            }
            set {
                _dbType = value;
                UpdateValidationColor();
            }
        } private DBField.DBDataType _dbType;


        [Category("Display Properties")]
        [Description("Determines if this textbox should look like a label until clicked.")]
        [DefaultValue(false)]
        public bool EmulateLabel {
            get {
                return _emulateLabel;
            }
            set {
                _emulateLabel = value;

                if (_emulateLabel)
                    DrawAsLabel();
                else
                    DrawAsTextBox();

            }
        } private bool _emulateLabel = false;

        #endregion

        #region Public Methods

        public DBTextBox() {
            TextChanged += new System.EventHandler(DBTextBox_TextChanged);
            Leave += new System.EventHandler(DBTextBox_Leave);
            Enter += new System.EventHandler(DBTextBox_Enter);
        }

        ~DBTextBox() {
            //Sync();
        }

        // If current value in the control is valid for the linked DBField, returns true.
        public bool IsValid() {
            if (DatabaseField == null || DatabaseObject == null)
                return false;

            try {
                switch (DBTypeOverride) {
                    case DBField.DBDataType.BOOL:
                        bool.Parse(Text);
                        break;
                    case DBField.DBDataType.INTEGER:
                        int.Parse(Text);
                        break;
                    case DBField.DBDataType.REAL:
                        float.Parse(Text, new CultureInfo("en-US", false));
                        break;
                }

                return true;
            }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;
                return false;
            }
        }

        // If current value in control is valid, writes to the object, 
        // otherwise reverts to the stored value.
        public void Sync() {
            if (!SaveText())
                RevertText();
        }

        // If valid, writes the Text in the control to the database object. This does
        // NOT commit the object to the disk. Returns false if unsuccessful.
        public bool SaveText() {
            if (IsValid()) {
                DatabaseField.SetValue(DatabaseObject, Text);
                return true;
            }

            return false;
        }

        // Reverts the text in the control to what is stored in the database object.
        public void RevertText() {
            if (DatabaseField == null || DatabaseObject == null) {
                Text = "";
                return;
            }

            if (DatabaseField.GetValue(DatabaseObject) == null)
                Text = "";
            else 
                Text = DatabaseField.GetValue(DatabaseObject).ToString();

            UpdateValidationColor();
        }

        #endregion

        #region Private Methods

        private void DBTextBox_TextChanged(object sender, EventArgs e) {
            UpdateValidationColor();
        }

        private void DBTextBox_Enter(object sender, EventArgs e) {
            if (EmulateLabel)
                DrawAsTextBox();
        }

        private void DBTextBox_Leave(object sender, EventArgs e) {
            if (EmulateLabel)
                DrawAsLabel();

            Sync();
        }

        private void DrawAsLabel() {
            BorderStyle = BorderStyle.None;
            BackColor = System.Drawing.SystemColors.Control;
        }

        private void DrawAsTextBox() {
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = System.Drawing.SystemColors.Window;
        }

        private void UpdateValidationColor() {
            if (IsValid() || DesignMode)
                ForeColor = DefaultForeColor;
            else
                ForeColor = Color.Red;
        }

        #endregion

        #region IDataGridViewEditingControl Implementation

        #region Private Variables
        
        private DataGridView dataGridView;
        private bool valueChanged = false;
        private int rowIndex;
        
        #endregion

        #region Properties

        [Browsable(false)]
        [ReadOnly(true)]
        public object EditingControlFormattedValue {
            get {
                return this.Text;
            }
            set {
                if (value is String) {
                    Text = (String) value;
                }
            }
        }
        
        [Browsable(false)]
        [ReadOnly(true)]
        public int EditingControlRowIndex {
            get { return rowIndex;  }
            set { rowIndex = value; }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public bool RepositionEditingControlOnValueChange {
            get { return false; }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public DataGridView EditingControlDataGridView {
            get { return dataGridView;  }
            set { dataGridView = value; }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public bool EditingControlValueChanged {
            get { return valueChanged; }
            set { valueChanged = value; }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public Cursor EditingPanelCursor {
            get { return base.Cursor; }
        }

        #endregion

        #region Public Methods

        public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context) {
            return EditingControlFormattedValue;
        }

        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle) {
            this.Font = dataGridViewCellStyle.Font;
            this.ForeColor = dataGridViewCellStyle.ForeColor;
            this.BackColor = dataGridViewCellStyle.BackColor;
        }
                
        public bool EditingControlWantsInputKey(Keys key, bool dataGridViewWantsInputKey) {
            return true;
        }

        public void PrepareEditingControlForEdit(bool selectAll) {
            valueChanged = false;
            Focus();
        }

        #endregion

        #region Listeners

        protected override void OnTextChanged(EventArgs e) {
            base.OnTextChanged(e);
            valueChanged = true;

            if (EditingControlDataGridView != null) {
                EditingControlDataGridView.CurrentCell.Value = Text;
            }
        }

        #endregion

        #endregion
    }

    public class DBTextBoxCell : DataGridViewTextBoxCell, IDBFieldBackedControl {

        #region DBBackedControl Implementation

        // The database object type that this object displays data about.
        public Type Table {
            get { return _table; }
            set { _table = value; }
        } private Type _table;

        public String DatabaseFieldName {
            get { return _databaseFieldName; }
            set {
                _databaseFieldName = value;
                _databaseField = DBField.GetField(_table, _databaseFieldName);
                _dbType = _databaseField.DBType;
            }
        } private String _databaseFieldName = null;

        public DBField DatabaseField {
            get { return _databaseField; }
        } private DBField _databaseField = null;

        public DBField.DBDataType DBTypeOverride {
            get {
                return _dbType;
            }
            set {
                _dbType = value;
            }
        } private DBField.DBDataType _dbType;

        public DatabaseTable DatabaseObject {
            get { return _databaseObject; }
            set {
                _databaseObject = value;
                if (DatabaseField != null && Table != null && _databaseObject != null)
                    Value = DatabaseField.GetValue(_databaseObject);
                else
                    Value = "";
            }
        } private DatabaseTable _databaseObject = null;

        #endregion

        #region Public Methods

        public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle) {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
            DBTextBox control = DataGridView.EditingControl as DBTextBox;

            control.Multiline = false;
            control.Table = Table;
            control.DatabaseFieldName = DatabaseFieldName;
            control.DBTypeOverride = DBTypeOverride;
            control.DatabaseObject = DatabaseObject;
        }

        public override Type EditType {
            get { return typeof(DBTextBox); }
        }

        public override Type ValueType {
            get { return typeof(String); }
        }

        #endregion
    }

}
