using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using Cornerstone.GUI.DesignMode;

namespace Cornerstone.GUI.Controls {
    [Designer(typeof(DBObjectListDesigner))]
    public class DBObjectEditor: UserControl, IDBBackedControl {

        #region Private Variables

        DataGridView grid;
        DataGridViewTextBoxColumn fieldColumn;
        DataGridViewTextBoxColumn valueColumn;

        #endregion

        #region Properties

        // The database object type that this object displays data about.
        [Category("Display Contents")]
        [Description("The datatype that this control displays. All classes using the DBTableAttribute attribute will be available in the drop down.")]
        [TypeConverter(typeof(DatabaseTableTypeConverter))]
        public Type Table {
            get {
                return _table;
            }
            set {
                if (_table != value) {
                    // if we are SWITCHING table types, clear out the field properties
                    if (_table != null)
                        FieldProperties.Clear();

                    _table = value;

                    // populates the field list based on the new table
                    _fields.Clear();
                    foreach (DBField currField in DBField.GetFieldList(Table))
                        _fields.Add(currField);

                    // if we are in the designer make sure we have all the properties we need
                    if (DesignMode)
                        generateFieldProperties();

                    // populate the rows of the grid view
                    GenerateRows();
                }
            }
        } private Type _table;

        // properties for configuring various properties of each field in the Table
        [Category("Display Contents")]
        [Description("Display properties for all fields in the database table represented by this control.")]
        [Editor(typeof(DBObjectListFieldEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<FieldProperty> FieldProperties {
            get {
                if (_fieldProperties == null)
                    _fieldProperties = new List<FieldProperty>();
                return _fieldProperties;
            }
            set {
                _fieldProperties = value;
                GenerateRows();
            }
        } List<FieldProperty> _fieldProperties;

        // List of fields from the Table object being displayed.
        [Browsable(false)]
        public ReadOnlyCollection<DBField> Fields {
            get {
                if (_fields == null)
                    _fields = new List<DBField>();
                return _fields.AsReadOnly();
            }
        } private List<DBField> _fields;

        // the object containing values for display
        [Browsable(false)]
        public DatabaseTable DatabaseObject {
            get {
                return _dbObject;
            }
            set {
                _dbObject = value;
                repopulateValues();
            }
        } private DatabaseTable _dbObject = null;

        #endregion

        public DBObjectEditor()
            : base() {
            InitializeComponent();

            _fields = new List<DBField>();

            grid.Columns[0].DefaultCellStyle.BackColor = System.Drawing.SystemColors.Control;
            grid.Columns[1].DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
        }

        #region Private Methods

        private void generateFieldProperties() {
            foreach (DBField currField in Fields) {
                if (getProperties(currField.Name) == null) {
                    FieldProperty metaData = new FieldProperty();
                    metaData.FieldName = currField.Name;
                    metaData.Visible = true;
                    FieldProperties.Add(metaData);
                }
            }
        }

        public void GenerateRows() {
            if (grid.Columns.Count == 0)
                return;

            grid.Rows.Clear();

            foreach (DBField currField in Fields) {
                FieldProperty properties = getProperties(currField.Name);
                if (properties == null) {
                    properties = new FieldProperty();
                    properties.FieldName = currField.Name;
                    properties.Visible = true;
                }

                if (properties.Visible == false)
                    continue;

                int rowNum = grid.Rows.Add();
                DataGridViewRow currRow = grid.Rows[rowNum];
                currRow.Tag = currField;
                currRow.ReadOnly = properties.ReadOnly;
                
                // build the custom value cell
                IDBFieldBackedControl valueCell;
                if (currField.DBType == DBField.DBDataType.ENUM)
                    valueCell = new DBComboBoxCell();
                else 
                    valueCell = new DBTextBoxCell();

                valueCell.Table = Table;
                valueCell.DatabaseFieldName = properties.FieldName;
                valueCell.DatabaseObject = DatabaseObject;
                currRow.Cells["valueColumn"] = (DataGridViewCell) valueCell;

                currRow.Cells["fieldColumn"].Value = properties.DisplayName;
            }
        }

        private void repopulateValues() {
            foreach (DataGridViewRow currRow in grid.Rows) {
                DBField currField = (DBField)currRow.Tag;
                ((IDBBackedControl)currRow.Cells["valueColumn"]).DatabaseObject = DatabaseObject;
            }

            grid.AutoResizeRows();
        }

        private FieldProperty getProperties(string field) {
            foreach (FieldProperty currProperty in FieldProperties) {
                if (currProperty.FieldName != null && currProperty.FieldName.Equals(field))
                    return currProperty;
            }
            
            return null;
        }
        
        #endregion

        private void InitializeComponent() {
            this.grid = new System.Windows.Forms.DataGridView();
            this.fieldColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.valueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            // 
            // grid
            // 
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.ColumnHeadersVisible = false;
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.fieldColumn,
            this.valueColumn});
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.Location = new System.Drawing.Point(0, 0);
            this.grid.Name = "grid";
            this.grid.RowHeadersVisible = false;
            this.grid.Size = new System.Drawing.Size(375, 552);
            this.grid.TabIndex = 0;
            // 
            // fieldColumn
            // 
            this.fieldColumn.HeaderText = "Field";
            this.fieldColumn.Name = "fieldColumn";
            this.fieldColumn.ReadOnly = true;
            // 
            // valueColumn
            // 
            this.valueColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.valueColumn.HeaderText = "Value";
            this.valueColumn.Name = "valueColumn";
            // 
            // DBObjectList
            // 
            this.Controls.Add(this.grid);
            this.Name = "DBObjectList";
            this.Size = new System.Drawing.Size(375, 552);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);

        }
    }

    // A property object representing display information for a DBField on a DBObject list.
    // These objects should normally be created and populated by the Visual Studio Designer.
    public class FieldProperty {
        #region Properties

        [Browsable(false)]
        [Description("The name of the database field this property represents.")]
        public string FieldName {
            get {
                return _fieldName;
            }
            set {
                _fieldName = value;
            }
        } private string _fieldName;


        [Category("Display Properties")]
        [Description("The label for this database field to be displayed on the control.")]
        public string DisplayName {
            get {
                if (_displayName == null)
                    _displayName = DefaultDisplayName;
                
                return _displayName;
            }
            set {
                _displayName = value;
            }
        } private string _displayName = null;


        [Category("Display Properties")]
        [Description("Determines if this database field should be displayed on the control.")]
        [DefaultValue(true)]
        public bool Visible {
            get {
                return _visible;
            }
            set {
                _visible = value;
            }
        } private bool _visible = true;


        [Category("Display Properties")]
        [Description("Determines if field is editable by the user.")]
        [DefaultValue(false)]
        public bool ReadOnly {
            get {
                return _readOnly;
            }
            set {
                _readOnly = value;
            }
        } private bool _readOnly = false;

        [Browsable(false)]
        [ReadOnly(true)]
        [Description("The default label for this database field to be displayed on the control.")]
        public string DefaultDisplayName {
            get {
                if (defaultDisplayName == null) {
                    defaultDisplayName = "";

                    char prevChar = char.MinValue;
                    foreach (char currChar in FieldName) {
                        if (prevChar != char.MinValue && char.IsLower(prevChar) && char.IsUpper(currChar))
                            defaultDisplayName += " ";

                        defaultDisplayName += currChar;
                        prevChar = currChar;
                    }
                }

                return defaultDisplayName;
            }
        } private string defaultDisplayName = null;

        #endregion

        #region Methods

        public override string ToString() {
            return _fieldName;
        }



        #endregion
    }
}
