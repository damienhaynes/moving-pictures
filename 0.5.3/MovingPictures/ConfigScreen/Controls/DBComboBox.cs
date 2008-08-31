using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.DesignMode;
using System.Collections;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls {
    public class DBComboBox : ComboBox, IDBFieldBackedControl, IDataGridViewEditingControl {
        #region Private Variables
        private IEnumerable customChoices = null;
        #endregion

        #region IDBBackedControl Implementation

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

                Revert();

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
        public DBField.DBDataType DBTypeOverride {
            get {
                return _dbType;
            }
            set {
                _dbType = value;
            }
        } private DBField.DBDataType _dbType;

        [Browsable(false)]
        public DatabaseTable DatabaseObject {
            get {
                return _databaseObject;
            }
            set {
                _databaseObject = value;
                Revert();
            }
        } private DatabaseTable _databaseObject = null;

        #endregion

        #region IDataGridViewEditingControl Implementation
        
        #region Private Variables

        private DataGridView dataGridView;
        private bool valueChanged = false;
        private int rowIndex;
        private bool reverting = false;

        #endregion

        #region Properties

        [Browsable(false)]
        [ReadOnly(true)]
        public object EditingControlFormattedValue {
            get { return this.SelectedItem; }
            set {
                this.SelectedItem = value;
            }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public int EditingControlRowIndex {
            get { return rowIndex; }
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
            get { return dataGridView; }
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

        protected override void OnSelectionChangeCommitted(EventArgs e) {
            base.OnSelectionChangeCommitted(e);
            valueChanged = true;

            if (EditingControlDataGridView != null) {
                EditingControlDataGridView.CurrentCell.Value = Text;
            }
        }

        #endregion
        
        #endregion

        #region Public Methods

        public DBComboBox() {
            DropDownStyle = ComboBoxStyle.DropDownList;
            this.SelectedIndexChanged += new System.EventHandler(DBComboBox_SelectedIndexChanged);
        }

        // If current value in the control is valid for the linked DBField, returns true.
        public bool IsValid() {
            if (DatabaseField == null || DatabaseObject == null)
                return false;

            return true;
        }

        // If current value in control is valid, writes to the object, 
        // otherwise reverts to the stored value.
        public void Sync() {
            if (!Save())
                Revert();
        }

        // If valid, writes the Text in the control to the database object. This does
        // NOT commit the object to the disk. Returns false if unsuccessful.
        public bool Save() {
            if (IsValid()) {
                DatabaseField.SetValue(DatabaseObject, SelectedItem);
                return true;
            }

            return false;
        }

        // Reverts the text in the control to what is stored in the database object.
        public void Revert() {
            reverting = true;
            this.Items.Clear();

            if (DatabaseField == null || DatabaseObject == null) {
                Text = "";
                return;
            }

            // if we are using custom choices (most likely the attribute dialog) set the drop down 
            // options up
            if (customChoices != null) {
                foreach (object currChoice in customChoices) {
                    if (currChoice is DBAttrPossibleValues)
                        this.Items.Add(((DBAttrPossibleValues)currChoice).Value);
                    else
                        this.Items.Add(currChoice);
                }

                if (Items.Contains(DatabaseField.GetValue(DatabaseObject)))
                    SelectedItem = DatabaseField.GetValue(DatabaseObject);
            }

            // if we are representing an enum, set that up, otherwise combo support is not implemented.
            else if (DatabaseField.DBType == DBField.DBDataType.ENUM) {
                Enum selectedValue = (Enum) DatabaseField.GetValue(DatabaseObject);
                foreach (object currValue in Enum.GetValues(selectedValue.GetType())) {
                    this.Items.Add(currValue);
                }
                this.SelectedItem = selectedValue;                
            }
            
            reverting = false;
        }

        public void SetCustomChoices(IEnumerable choices) {
            customChoices = choices;
            Revert();
        }

        private void DBComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (!reverting)
                Sync();
        }
        
        #endregion
    }

    public class DBComboBoxCell : DataGridViewTextBoxCell, IDBFieldBackedControl {

        #region Private Variables
        IEnumerable customChoices = null;
        #endregion

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
            DBComboBox control = DataGridView.EditingControl as DBComboBox;

            control.Table = Table;
            control.DatabaseFieldName = DatabaseFieldName;
            control.DBTypeOverride = DBTypeOverride;
            control.SetCustomChoices(customChoices);
            control.DatabaseObject = DatabaseObject;
        }

        public override Type EditType {
            get { return typeof(DBComboBox); }
        }

        public override Type ValueType {
            get { 
                if (DBTypeOverride == DBField.DBDataType.BOOL)
                    return typeof(bool);
                if (DBTypeOverride == DBField.DBDataType.ENUM)
                    return DatabaseField.GetValue(DatabaseObject).GetType();
                return typeof(string);
            }
        }

        public void SetCustomChoices(IEnumerable choices) {
            customChoices = choices;
        }

        #endregion
    }
}
