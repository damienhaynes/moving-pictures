using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Cornerstone.Database;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Cornerstone.GUI.DesignMode;
using System.Drawing.Design;

namespace Cornerstone.GUI.Controls {
    public interface IFieldDisplaySettingsOwner {
        FieldDisplaySettings FieldDisplaySettings {
            get;
            set;
        }

        void OnFieldPropertiesChanged();

    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FieldDisplaySettings {
        #region Properties

        [Browsable(false)]
        [ReadOnly(true)]
        public IFieldDisplaySettingsOwner Owner {
            get { return _owner; }
            set { _owner = value; }
        } private IFieldDisplaySettingsOwner _owner;


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
                    if (_fields == null) _fields = new List<DBField>();
                    _fields.Clear();
                    foreach (DBField currField in DBField.GetFieldList(Table))
                        _fields.Add(currField);

                    generateFieldProperties();

                    // notify owner of updated settings
                    if (Owner != null) Owner.OnFieldPropertiesChanged();
                }
            }
        } private Type _table;


        // properties for configuring various properties of each field in the Table
        [Category("Display Contents")]
        [Description("Display properties for all fields in the database table represented by this control.")]
        [Editor(typeof(IDBObjectFieldEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<FieldProperty> FieldProperties {
            get {
                if (_fieldProperties == null)
                    _fieldProperties = new List<FieldProperty>();
                return _fieldProperties;
            }
            set {
                _fieldProperties = value;
                Owner.OnFieldPropertiesChanged();
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

        #endregion

        #region Methods

        protected void generateFieldProperties() {
            foreach (DBField currField in Fields) {
                if (getProperties(currField.Name) == null) {
                    FieldProperty metaData = new FieldProperty();
                    metaData.FieldName = currField.Name;
                    metaData.Visible = true;
                    FieldProperties.Add(metaData);
                }
            }
        }

        public FieldProperty getProperties(string field) {
            foreach (FieldProperty currProperty in FieldProperties) {
                if (currProperty.FieldName != null && currProperty.FieldName.ToLower().Equals(field.ToLower()))
                    return currProperty;
            }

            return null;
        }

        #endregion
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
        [Description("The width of the column for this field, if used in a table. This does not apply to all controls.")]
        public int? ColumnWidth {
            get { return _columnWidth; }
            set { _columnWidth = value; }
        } private int? _columnWidth = null;


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
                if (defaultDisplayName == null)
                    defaultDisplayName = DBField.MakeFriendlyName(_fieldName);

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
