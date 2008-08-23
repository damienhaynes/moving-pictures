using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using MediaPortal.Plugins.MovingPictures.Database.CustomTypes;

namespace MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables {
    [DBTable("attribute_values")]
    public class DBAttribute: MovingPicturesDBTable {
        [DBField]
        public DBAttrDescription Description {
            get { return _description; }
            set { 
                _description = value;
                commitNeeded = true;
            }
        } private DBAttrDescription _description;

        [DBField]
        public string Value {
            get { return _value; }
            set { 
                _value = value;
                commitNeeded = true;
            }
        } private string _value;
    }

    [DBTable("attributes")]
    public class DBAttrDescription : MovingPicturesDBTable {
        public enum ValueTypeEnum { INT, FLOAT, BOOL, STRING }
        
        // Denotes the methods that an attribute's value can be modified.
        public enum SelectionModeEnum { 
            Manual,    // The user manually enters the value, which should then be type-checked.
            
            Selection, // Upon creation of the attribute, the user specifies the list of possible 
                       // options.
            
            Dynamic    // Same as SELECTION but each choice has a criteria attached and the selection
                       // is picked and updated automatically.
        }

        #region Database Fields

        [DBField]
        // The user defined name of the attribute. 
        public string Name {
            get { return _name; }
            set { 
                _name = value;
                commitNeeded = true;
            }
        } private string _name;

        
        [DBField(FieldName="table_type")]
        // The table this attribute is attached to.
        public Type Table {
            get { return _tableType; }
            set {
                // make sure the type set is an IAttributeOwner
                bool valid = false;
                if (value != null) {
                    Type[] interfaces = value.GetInterfaces();
                    foreach (Type currType in interfaces) {
                        if (currType == typeof(IAttributeOwner)) {
                            valid = true;
                        }
                    }
                }

                if (valid || value == null) {
                    _tableType = value;
                    commitNeeded = true;
                } 

                // if it's not an IAttributeOwner, this is invalid.
                else throw new InvalidOperationException("Owning object must implement IAttributeOwner interface.");
            }
        } Type _tableType;


        [DBField(FieldName="value_type", Default="INT")]
        // The type of data stored by this attribute.
        public ValueTypeEnum? ValueType {
            get { return _valueType; }
            set {
                if (_valueType != null) {
                    _valueType = value;
                    createDefaultPossibleValues();
                }
                else
                    _valueType = value;

                commitNeeded = true;
            }
        } private ValueTypeEnum? _valueType = null;


        [DBField(FieldName = "default_value")]
        // The table this attribute is attached to.
        public string Default {
            get { return _default; }
            set { _default = value; }
        } string _default;


        [DBField(FieldName="selection_mode")]
        // The method the user assigns values to this attribute.
        public SelectionModeEnum SelectionMode {
            get { return _selectionMode; }
            set { 
                _selectionMode = value;
                commitNeeded = true;
            }
        } private SelectionModeEnum _selectionMode;


        [DBRelation(AutoRetrieve=true)]
        // List of possible values if this attribute is in SELECTION or DYNAMIC mode.
        public RelationList<DBAttrDescription, DBAttrPossibleValues> PossibleValues {
            get {
                if (_possibleValues == null)
                    _possibleValues = new RelationList<DBAttrDescription, DBAttrPossibleValues>(this);
                return _possibleValues; 
            }
            set { 
                _possibleValues = value;
                commitNeeded = true;
            }
        } RelationList<DBAttrDescription, DBAttrPossibleValues> _possibleValues;

        #endregion

        #region DatabaseTable Overrides

        // Add this attribute to any movies that don't yet have it.
        public override void AfterCommit() {
            base.AfterCommit();

            List<DatabaseTable> dbObjs = DBManager.Get(Table, null);
            foreach (DatabaseTable currObj in dbObjs) {
                IAttributeOwner currOwner = (IAttributeOwner)currObj;

                bool needsThisAttr = true;
                foreach (DBAttribute currAttr in currOwner.Attributes)
                    if (currAttr.Description != null && currAttr.Description.ID == this.ID) {
                        needsThisAttr = false;
                        break;
                    }

                if (needsThisAttr) {
                    DBAttribute newAttr = new DBAttribute();
                    newAttr.Description = this;

                    currOwner.Attributes.Add(newAttr);
                    currOwner.Attributes.Commit();
                }
            }
        }
        
        #endregion

        #region Private

        private void createDefaultPossibleValues() {
            if (ValueType == ValueTypeEnum.BOOL) {
                // clear out any existing possible values
                foreach (DBAttrPossibleValues currValue in PossibleValues)
                    currValue.Delete();
                PossibleValues.Clear();

                DBAttrPossibleValues newValue = new DBAttrPossibleValues();
                newValue.Value = "true";
                PossibleValues.Add(newValue);

                newValue = new DBAttrPossibleValues();
                newValue.Value = "false";
                PossibleValues.Add(newValue);
            }
        }

        #endregion
    }

    [DBTable("attribute_possible_values")]
    public class DBAttrPossibleValues : MovingPicturesDBTable {
        [DBField]
        public string Value {
            get { return _value; }
            set { 
                _value = value;
                commitNeeded = true;
            }
        } private string _value;
    }
}
