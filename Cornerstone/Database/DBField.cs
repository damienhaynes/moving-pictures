using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NLog;
using System.Collections.ObjectModel;
using Cornerstone.Database.Tables;
using Cornerstone.Database.CustomTypes;
using System.Globalization;
using System.Threading;

namespace Cornerstone.Database {
    public class DBField {
        public enum DBDataType { INTEGER, REAL, TEXT, STRING_OBJECT, BOOL, TYPE, ENUM, DATE_TIME, DB_OBJECT, DB_FIELD, DB_RELATION }

        #region Private Variables
        
        private PropertyInfo propertyInfo;
        private DBFieldAttribute attribute;
        private DBDataType type;

        private static Dictionary<Type, List<DBField>> fieldLists;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors
        private DBField(PropertyInfo propertyInfo, DBFieldAttribute attribute) {
            this.propertyInfo = propertyInfo;
            this.attribute = attribute;

            // determine how this shoudl be stored in the DB
            type = DBDataType.TEXT;

            if (propertyInfo.PropertyType == typeof(string))
                type = DBDataType.TEXT;
            else if (propertyInfo.PropertyType == typeof(int))
                type = DBDataType.INTEGER;
            else if (propertyInfo.PropertyType == typeof(int?))
                type = DBDataType.INTEGER;
            else if (propertyInfo.PropertyType == typeof(float))
                type = DBDataType.REAL;
            else if (propertyInfo.PropertyType == typeof(float?))
                type = DBDataType.REAL;
            else if (propertyInfo.PropertyType == typeof(double))
                type = DBDataType.REAL;
            else if (propertyInfo.PropertyType == typeof(double?))
                type = DBDataType.REAL;
            else if (propertyInfo.PropertyType == typeof(bool))
                type = DBDataType.BOOL;
            else if (propertyInfo.PropertyType == typeof(bool?))
                type = DBDataType.BOOL;
            else if (propertyInfo.PropertyType == typeof(Boolean))
                type = DBDataType.BOOL;
            else if (propertyInfo.PropertyType == typeof(DateTime))
                type = DBDataType.DATE_TIME;
            else if (propertyInfo.PropertyType == typeof(DateTime?))
                type = DBDataType.DATE_TIME;
            else if (propertyInfo.PropertyType == typeof(Type))
                type = DBDataType.TYPE;
            else if (propertyInfo.PropertyType.IsEnum)
                type = DBDataType.ENUM;
            // nullable enum
            else if (Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null ? Nullable.GetUnderlyingType(propertyInfo.PropertyType).IsEnum : false)
                type = DBDataType.ENUM;
            else if (DatabaseManager.IsDatabaseTableType(propertyInfo.PropertyType))
                type = DBDataType.DB_OBJECT;
            else if (propertyInfo.PropertyType == typeof(DBField))
                type = DBDataType.DB_FIELD;
            else if (propertyInfo.PropertyType == typeof(DBRelation))
                type = DBDataType.DB_RELATION;
            else {
                // check for string object types
                foreach (Type currInterface in propertyInfo.PropertyType.GetInterfaces())
                    if (currInterface == typeof(IStringSourcedObject)) {
                        type = DBDataType.STRING_OBJECT;
                        return;
                    }
            }
        }

        static DBField() {
            fieldLists = new Dictionary<Type, List<DBField>>();
        }

        #endregion

        #region Public Properties
        // Returns the name of this attribute.
        public string Name {
            get { return propertyInfo.Name; }
        }

        public string FriendlyName {
            get {
                if (_friendlyName == null) 
                    _friendlyName = DBField.MakeFriendlyName(Name);

                return _friendlyName;
            }
        } private string _friendlyName = null;

        // Returns the name of this field in the database. Generally the same as Name,
        // but this is not gauranteed.
        public string FieldName {
            get {
                if (attribute.FieldName == string.Empty)
                    return Name.ToLower();
                else
                    return attribute.FieldName;
            }
        }

        // Returns the Type of database object this field belongs to.
        public Type OwnerType {
            get {
                return propertyInfo.DeclaringType;
            }
        }

        // Returns the type the field will be stored as in the database.
        public DBDataType DBType {
            get { return type; }
        }

        // Returns the C# type for the field.
        public Type Type {
            get { return propertyInfo.PropertyType; }
        }

        // Returns the default value for the field. Currently always returns in type string.
        public object Default {
            get {
                if (attribute.Default == null)
                    return null;

                switch (DBType) {
                    case DBDataType.INTEGER:
                        if (attribute.Default == "")
                            return 0;
                        else
                            return int.Parse(attribute.Default);
                    case DBDataType.REAL:
                        if (attribute.Default == "")
                            return (float)0.0;
                        else
                            return float.Parse(attribute.Default);
                    case DBDataType.BOOL:
                        if (attribute.Default == "")
                            return false;
                        else
                            return attribute.Default == "true" || attribute.Default.ToString() == "1";
                    case DBDataType.DATE_TIME:
                        if (attribute.Default == "")
                            return DateTime.Now;
                        else {
                            try {
                                return DateTime.Parse(attribute.Default);
                            }
                            catch { }
                        }
                        return DateTime.Now;
                    case DBDataType.STRING_OBJECT:
                        IStringSourcedObject newObj = (IStringSourcedObject)propertyInfo.PropertyType.GetConstructor(System.Type.EmptyTypes).Invoke(null);
                        newObj.LoadFromString(attribute.Default);
                        return newObj;
                    case DBDataType.DB_OBJECT:
                        DatabaseTable newDBObj = (DatabaseTable)propertyInfo.PropertyType.GetConstructor(System.Type.EmptyTypes).Invoke(null);
                        return newDBObj;
                    default:
                        if (attribute.Default == "")
                            return " ";
                        else
                            return attribute.Default;
                }
            }
        }

        // Returns true if this field should be updated when pulling updated data in from
        // an external source. 
        public bool AutoUpdate {
            get { return attribute.AllowAutoUpdate; }
        }

        // Returns true if this field should be available to the user for filtering purposes.
        public bool Filterable {
            get { return attribute.Filterable; }
        }

        // returns true if the user should be able to manually enter a value for filtering
        public bool AllowManualFilterInput {
            get {
                if (this.Type == typeof(bool))
                    return false;

                return attribute.AllowManualFilterInput;
            }
        }

        #endregion

        #region Public Methods
        // Sets the value of this field for the given object.
        public void SetValue(DatabaseTable owner, object value) {
            try {

                // if we were passed a null value, try to set that. 
                if (value == null) {
                    propertyInfo.GetSetMethod().Invoke(owner, new object[] { null });
                    return;
                }

                // if we were passed a matching object, just set it
                if (value.GetType() == propertyInfo.PropertyType) {
                    propertyInfo.GetSetMethod().Invoke(owner, new object[] { value });
                    return;
                }

                if (value is string) 
                    propertyInfo.GetSetMethod().Invoke(owner, new object[] { ConvertString(owner.DBManager, (string)value) });

            }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;
                
                logger.Error("Error writing to " + owner.GetType().Name + "." + this.Name +
                                " Property: " + e.Message);
            }
        }

        // Returns the value of this field for the given object.
        public object GetValue(DatabaseTable owner) {
            return propertyInfo.GetGetMethod().Invoke(owner, null);
        }

        public object ConvertString(DatabaseManager dbManager, string strVal) {
            try {
                switch (DBType) {
                    case DBDataType.INTEGER:
                        string tmp = strVal.ToString();
                        while (tmp.Contains(","))
                            tmp = tmp.Remove(tmp.IndexOf(','), 1);

                        return int.Parse(tmp);
                    
                    case DBDataType.REAL:
                        if (propertyInfo.PropertyType == typeof(double))
                            return double.Parse(strVal, new CultureInfo("en-US", false));
                        else
                            return float.Parse(strVal, new CultureInfo("en-US", false));
                    
                    case DBDataType.BOOL:
                        return (strVal.ToString() == "true" || strVal.ToString() == "1");
                    
                    case DBDataType.STRING_OBJECT:
                        // create a new object and populate it
                        IStringSourcedObject newObj = (IStringSourcedObject)propertyInfo.PropertyType.GetConstructor(System.Type.EmptyTypes).Invoke(null);
                        newObj.LoadFromString(strVal);
                        return newObj;
                    
                    case DBDataType.TYPE:
                        return Type.GetType(strVal);
                    
                    case DBDataType.ENUM:
                        if (strVal.Trim().Length != 0) {
                            Type enumType = propertyInfo.PropertyType;
                            if (Nullable.GetUnderlyingType(enumType) != null)
                                enumType = Nullable.GetUnderlyingType(enumType);

                            return Enum.Parse(enumType, strVal);
                        }
                        break;
                    
                    case DBDataType.DATE_TIME:
                        DateTime newDateTimeObj = DateTime.Now;
                        if (strVal.Trim().Length != 0)
                            try {
                                newDateTimeObj = DateTime.Parse(strVal);
                            }
                            catch { }

                        return newDateTimeObj;
                    
                    case DBDataType.DB_OBJECT:
                        DatabaseTable newDBObj;
                        if (strVal.Trim().Length == 0)
                            newDBObj = null;
                        else newDBObj = dbManager.Get(propertyInfo.PropertyType, int.Parse(strVal));

                        return newDBObj;
                    
                    case DBDataType.DB_FIELD:
                        string[] fieldValues = strVal.Split(new string[] { "|||" }, StringSplitOptions.None);
                        if (fieldValues.Length != 2)
                            break;

                        return DBField.GetFieldByDBName(Type.GetType(fieldValues[0]), fieldValues[1]);

                    case DBDataType.DB_RELATION:
                        string[] relationValues = strVal.Split(new string[] { "|||" }, StringSplitOptions.None);
                        if (relationValues.Length != 3)
                            break;

                        return DBRelation.GetRelation(Type.GetType(relationValues[0]),
                                                      Type.GetType(relationValues[1]),
                                                      relationValues[2]);

                    default:
                        return strVal;
                }
            }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;

                logger.Error("Error parsing " + propertyInfo.DeclaringType.Name + "." + this.Name +
                                " Property: " + e.Message);
            }

            return null;
        }

        // sets the default value based on the datatype.
        public void InitializeValue(DatabaseTable owner) {
            SetValue(owner, Default);
        }

        public override string ToString() {
            return FriendlyName;
        }

        #endregion

        #region Public Static Methods

        // Returns the list of DBFields for the given type. Developer should normally
        // directly use the properties of the class, but this allows for iteration.
        public static ReadOnlyCollection<DBField> GetFieldList(Type tableType) {
            if (tableType == null || !DatabaseManager.IsDatabaseTableType(tableType))
                return new List<DBField>().AsReadOnly();

            if (!fieldLists.ContainsKey(tableType)) {


                List<DBField> newFieldList = new List<DBField>();

                // loop through each property in the class
                PropertyInfo[] propertyArray = tableType.GetProperties();
                foreach (PropertyInfo currProperty in propertyArray) {
                    object[] customAttrArray = currProperty.GetCustomAttributes(true);
                    // for each property, loop through it's custom attributes
                    // if one of them is ours, store the property info for later use
                    foreach (object currAttr in customAttrArray) {
                        if (currAttr.GetType() == typeof(DBFieldAttribute)) {
                            DBField newField = new DBField(currProperty, (DBFieldAttribute)currAttr);
                            newFieldList.Add(newField);
                            break;
                        }
                    }
                }

                fieldLists[tableType] = newFieldList;
            }

            if (!fieldLists.ContainsKey(tableType)) 
                return null;

            return fieldLists[tableType].AsReadOnly();
        }

        // Returns the DBField with the specified name for the specified table.
        public static DBField GetField(Type tableType, string fieldName) {
            if (tableType == null) {
                return null;
            }

            ReadOnlyCollection<DBField> fieldList = GetFieldList(tableType);
            foreach (DBField currField in fieldList) {
                if (currField.Name.Equals(fieldName))
                    return currField;
            }

            return null;
        }

        // Returns the DBField with the specified name for the specified table.
        public static DBField GetFieldByDBName(Type tableType, string fieldName) {
            if (tableType == null) {
                return null;
            }

            ReadOnlyCollection<DBField> fieldList = GetFieldList(tableType);
            foreach (DBField currField in fieldList) {
                if (currField.FieldName.Equals(fieldName))
                    return currField;
            }

            return null;
        }

        public static string MakeFriendlyName(string input) {
            string friendlyName = "";
            
            char prevChar = char.MinValue;
            foreach (char currChar in input) {
                if (prevChar != char.MinValue && char.IsLower(prevChar) && char.IsUpper(currChar))
                    friendlyName += " ";

                friendlyName += currChar;
                prevChar = currChar;
            }

            return friendlyName;
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DBFieldAttribute : System.Attribute {
        #region Private
        private string fieldName = string.Empty;
        private string description = string.Empty;
        private string defaultValue = string.Empty;
        private bool allowAutoUpdate = true;
        private bool _filterable = true;
        private bool _allowManualFilterInput = true;
        #endregion

        #region Properties
        // if unassigned, the name of the parameter should be used for the field name
        public string FieldName {
            get { return fieldName; }
            set { fieldName = value; }
        }

        public string Default {
            get { return defaultValue; }
            set { defaultValue = value; }
        }

        public bool AllowAutoUpdate {
            get { return allowAutoUpdate; }
            set { allowAutoUpdate = value; }
        }

        public bool Filterable {
            get { return _filterable; }
            set { _filterable = value; }
        }

        public bool AllowManualFilterInput {
            get { return _allowManualFilterInput; }
            set { _allowManualFilterInput = value; }
        }

        #endregion

        public DBFieldAttribute() {
        }
    }
}
