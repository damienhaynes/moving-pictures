using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NLog;
using System.Collections.ObjectModel;
using Cornerstone.Database.Tables;
using Cornerstone.Database.CustomTypes;
using System.Globalization;

namespace Cornerstone.Database {
    public class DBField {
        public enum DBDataType { INTEGER, REAL, TEXT, STRING_OBJECT, BOOL, TYPE, ENUM, DB_OBJECT }

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
            else if (propertyInfo.PropertyType == typeof(Type))
                type = DBDataType.TYPE;
            else if (propertyInfo.PropertyType.IsEnum)
                type = DBDataType.ENUM;
            // nullable enum
            else if (Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null ? Nullable.GetUnderlyingType(propertyInfo.PropertyType).IsEnum : false)
                type = DBDataType.ENUM;
            else if (DatabaseManager.IsDatabaseTableType(propertyInfo.PropertyType))
                type = DBDataType.DB_OBJECT;
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

        // Returns the type the field will be stored as in the database.
        public DBDataType DBType {
            get { return type; }
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
                            return 0.0;
                        else
                            return float.Parse(attribute.Default);
                    case DBDataType.BOOL:
                        if (attribute.Default == "")
                            return false;
                        else
                            return attribute.Default == "true" || attribute.Default.ToString() == "1";
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

                switch (DBType) {
                    case DBDataType.INTEGER:
                        propertyInfo.GetSetMethod().Invoke(owner, new object[] { int.Parse(value.ToString()) });
                        break;
                    case DBDataType.REAL:
                        propertyInfo.GetSetMethod().Invoke(owner, new object[] { float.Parse(value.ToString(), new CultureInfo("en-US", false)) });
                        break;
                    case DBDataType.BOOL:
                        propertyInfo.GetSetMethod().Invoke(owner, new object[] { (value.ToString() == "true" || value.ToString() == "1") });
                        break;
                    case DBDataType.STRING_OBJECT:
                        // create a new object and populate it
                        IStringSourcedObject newObj = (IStringSourcedObject)propertyInfo.PropertyType.GetConstructor(System.Type.EmptyTypes).Invoke(null);
                        newObj.LoadFromString(value.ToString());
                        propertyInfo.GetSetMethod().Invoke(owner, new object[] { newObj });
                        break;
                    case DBDataType.TYPE:
                        Type newTypeObj = Type.GetType(value.ToString());
                        propertyInfo.GetSetMethod().Invoke(owner, new object[] { newTypeObj });
                        break;
                    case DBDataType.ENUM:
                        if (value.ToString().Trim().Length != 0) {
                            Type enumType = propertyInfo.PropertyType;
                            if (Nullable.GetUnderlyingType(enumType) != null)
                                enumType = Nullable.GetUnderlyingType(enumType);

                            object enumVal = Enum.Parse(enumType, value.ToString());
                            propertyInfo.GetSetMethod().Invoke(owner, new object[] { enumVal });
                        }
                        break;
                    case DBDataType.DB_OBJECT:
                        DatabaseTable newDBObj;
                        if (value.ToString().Trim().Length == 0)
                            newDBObj = null;
                        else newDBObj = owner.DBManager.Get(propertyInfo.PropertyType, int.Parse(value.ToString()));

                        propertyInfo.GetSetMethod().Invoke(owner, new object[] { newDBObj });
                        break;
                    default:
                        propertyInfo.GetSetMethod().Invoke(owner, new object[] { value.ToString() });
                        break;
                }
            }
            catch (Exception e) {
                logger.ErrorException("Error writing to " + owner.GetType().Name + "." + this.Name +
                                " Property.", e);
            }
        }

        // Returns the value of this field for the given object.
        public object GetValue(DatabaseTable owner) {
            return propertyInfo.GetGetMethod().Invoke(owner, null);
        }

        // sets the default value based on the datatype.
        public void InitializeValue(DatabaseTable owner) {
            SetValue(owner, Default);
        }


        #endregion

        #region Public Static Methods

        // Returns the list of DBFields for the given type. Developer should normally
        // directly use the properties of the class, but this allows for iteration.
        public static ReadOnlyCollection<DBField> GetFieldList(Type tableType) {
            if (tableType == null || !DatabaseManager.IsDatabaseTableType(tableType))
                return null;

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

        #endregion
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DBFieldAttribute : System.Attribute {
        #region Private
        private string fieldName = string.Empty;
        private string description = string.Empty;
        private string defaultValue = string.Empty;
        private bool allowAutoUpdate = true;
        #endregion

        #region Properties
        // if unassigned, the name of the parameter should be used for the field name
        public string FieldName {
            get { return fieldName; }
            set { fieldName = value; }
        }

        public string Description {
            get { return description; }
            set { description = value; }
        }

        public string Default {
            get { return defaultValue; }
            set { defaultValue = value; }
        }

        public bool AllowAutoUpdate {
            get { return allowAutoUpdate; }
            set { allowAutoUpdate = value; }
        }
        #endregion

        public DBFieldAttribute() {
        }
    }
}
