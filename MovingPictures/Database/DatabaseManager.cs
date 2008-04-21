using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using SQLite.NET;
using System.Windows.Forms;
using System.Reflection;
using MediaPortal.Plugins.MovingPictures.Database.CustomTypes;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.Database {
    public class DatabaseManager {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        private static Dictionary<Type, List<DBField>> fieldLists;

        private Dictionary<Type, bool> isVerified;
        private DatabaseCache cache;
        private string dbFilename;
        private SQLiteClient dbClient;
        

        static DatabaseManager() {
            fieldLists = new Dictionary<Type, List<DBField>>();
        }

        public DatabaseManager(string dbFilename) {
            this.dbFilename = dbFilename;
            isVerified = new Dictionary<Type, bool>();
            initDB();
            cache = new DatabaseCache();
        }
        
        #region Private Methods
        
        // Attempts to initialize the connection to the database file
        private void initDB() {
            try {
                dbClient = new SQLiteClient(dbFilename);
                logger.Info("Successfully Opened Database: " + dbFilename);
            }
            catch (Exception e) {
                logger.FatalException("Could Not Open Database: " + dbFilename, e);
                dbClient = null;
            }
        }

        // Returns a select statement retrieving all fields ordered as defined by FieldList
        // for the given Table Type. A where clause can be appended
        private static string getSelectQuery(Type tableType) {
            string query = "select ";
            foreach (DBField currField in fieldLists[tableType]) {
                if (query != "select ")
                    query += ", ";

                query += currField.FieldName;
            }
            query += ", id from " + GetTableName(tableType) + " ";
            return query;
        }
        
        // Loads into memory metadata about a given table type. 
        private static void loadFieldList(Type tableType) {
            if (fieldLists.ContainsKey(tableType))
                return;

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

        // creates an escaped, quoted string representation of the given object
        public static string getSQLiteString(object value) {
            if (value == null)
                return "NULL";
            
            string strVal = "";

            // handle boolean types
            if (value.GetType() == typeof(bool) || value.GetType() == typeof(Boolean)) {
                if ((Boolean)value == true)
                    strVal = "1";
                else
                    strVal = "0";
            
            // handle database table types
            } else if (IsDatabaseTableType(value.GetType()))
                strVal = ((DatabaseTable)value).ID.ToString();

            // everythign else just uses ToString()
            else
                strVal = value.ToString();


            // if we ended up with an empty string, save a space. an empty string is interpreted
            // as null by SQLite, and thats not what we want.
            if (strVal == "")
                strVal = " ";

            // escape all quotes
            strVal = strVal.Replace("'", "''");

            return "'" + strVal + "'";
        }

        public static bool IsDatabaseTableType(Type t) {
            Type currType = t.BaseType;
            while (currType != null) {
                if (currType == typeof(DatabaseTable)) {
                    return true;
                }
                currType = currType.BaseType;
            }

            return false;
        }

        // inserts a new object to the database
        private void insert(DatabaseTable dbObject) {
            try {
                string queryFieldList = "";
                string queryValueList = "";

                // loop through the fields and build the strings for the query
                foreach (DBField currField in fieldLists[dbObject.GetType()]) {
                    if (queryFieldList != "") {
                        queryFieldList += ", ";
                        queryValueList += ", ";
                    }

                    queryFieldList += currField.FieldName;
                    queryValueList += getSQLiteString(currField.GetValue(dbObject));
                }

                string query = "insert into " + GetTableName(dbObject.GetType()) +
                               " (" + queryFieldList + ") values (" + queryValueList + ")";

                dbClient.Execute(query);
                dbObject.ID = dbClient.LastInsertID();
                dbObject.DBManager = this;
                cache.Add(dbObject);
            }
            catch (SQLiteException e) {
                logger.ErrorException("Could not commit to " + GetTableName(dbObject.GetType()) + " table.", e);
            }
        }

        // updates the given object in the database. assumes the object was previously retrieved 
        // via a Get call from this class.
        private void update(DatabaseTable dbObject) {
            try {
                string query = "update " + GetTableName(dbObject.GetType()) + " set ";

                // loop through the fields and build the strings for the query
                bool firstField = true;
                foreach (DBField currField in fieldLists[dbObject.GetType()]) {
                    if (!firstField) {
                        query += ", ";
                    }

                    firstField = false;

                    query += currField.FieldName + " = " + getSQLiteString(currField.GetValue(dbObject));
                    
                }

                // add the where clause
                query += " where id = " + dbObject.ID;

                // execute the query
                dbClient.Execute(query);

                dbObject.DBManager = this;
            }
            catch (SQLiteException e) {
                logger.ErrorException("Could not commit to " + GetTableName(dbObject.GetType()) + " table.", e);
            }
        }

        #endregion

        #region Public Static Methods

        // Returns the table attribute information for the given type.
        public static DBTableAttribute GetDBTableAttribute(Type tableType) {
            // loop through the custom attributes of the type, if one of them is the type
            // we want, return it.
            object[] customAttrArray = tableType.GetCustomAttributes(true);
            foreach (object currAttr in customAttrArray) {
                if (currAttr.GetType() == typeof(DBTableAttribute)) 
                    return (DBTableAttribute) currAttr;
            }

            return null;
        }

        // Returns the name of the table of the given type.
        public static string GetTableName(Type tableType) {
            return GetDBTableAttribute(tableType).TableName;
        }

        // Returns the name of the table of the given type.
        public static string GetTableName(DatabaseTable tableObject) {
            return GetTableName(tableObject.GetType());
        }

        // Returns the list of DBFields for the given type. Developer should normally
        // directly use the properties of the class, but this allows for iteration.
        public static List<DBField> GetFieldList(Type tableType) {
            loadFieldList(tableType);
            return fieldLists[tableType];
        }

        #endregion

        #region Public Methods

        // Checks that the table coorisponding to this type exists, and if it is missing, it creates it.
        // Also verifies all columns represented in the class are also present in the table, creating 
        // any missing. Needs to be enhanced to allow for changed defaults.
        public void verifyTable(Type tableType) {
            lock (this) {
                // check that we haven't already verified this table
                if (isVerified.ContainsKey(tableType))
                    return;

                // attempt to grab table info for the type. if none exists, it's not tagged to be a table
                DBTableAttribute tableAttr = GetDBTableAttribute(tableType);
                if (tableAttr == null)
                    return;

                try {
                    // check if the table exists in the database, if not, create it
                    SQLiteResultSet resultSet = dbClient.Execute("select * from sqlite_master where type='table' and name = '" + tableAttr.TableName + "'");
                    if (resultSet.Rows.Count == 0) {
                        resultSet = dbClient.Execute("create table " + tableAttr.TableName + " (id INTEGER primary key )");
                    }

                    // ensure our column list for this type has been created
                    loadFieldList(tableType);

                    // grab existing table info from the DB
                    resultSet = dbClient.Execute("PRAGMA table_info(" + tableAttr.TableName + ")");

                    // loop through the CLASS DEFINED fields, and verify each is contained in the result set
                    foreach (DBField currField in fieldLists[tableType]) {

                        // loop through all defined columns in DB to ensure this col exists 
                        bool exists = false;
                        foreach (SQLiteResultSet.Row currRow in resultSet.Rows) {
                            if (currField.FieldName == currRow.fields[1]) {
                                exists = true;
                                break;
                            }
                        }

                        // if we couldn't find the column create it
                        if (!exists) {
                            string defaultValue;
                            if (currField.Default == null)
                                defaultValue = "NULL";
                            else
                                defaultValue = getSQLiteString(currField.Default);

                            dbClient.Execute("alter table " + tableAttr.TableName + " add column " + currField.FieldName + " " +
                                             currField.DBType.ToString() + " default " + defaultValue);
                        }
                    }

                    isVerified[tableType] = true;
                }
                catch (Exception e) {
                    logger.ErrorException("Internal error verifying " + tableAttr.TableName + " (" + tableType.ToString() + ") table.", e);
                }
            }
        }

        // Returns a list of objects of the specified type, based on the specified criteria.
        public List<T> Get<T>(ICriteria criteria) where T : DatabaseTable{
            List<T> rtn = new List<T>();
            List<DatabaseTable> objList = Get(typeof(T), criteria);
            foreach (DatabaseTable currObj in objList) {
                rtn.Add((T)currObj);
            }

            return rtn;
        }

        // Returns a list of objects of the specified type, based on the specified criteria.
        public List<DatabaseTable> Get(Type tableType, ICriteria criteria) {
            verifyTable(tableType);

            List<DatabaseTable> rtn = new List<DatabaseTable>();

            try {
                // build and execute the query
                string query = getSelectQuery(tableType);
                if (criteria != null)
                    query += criteria.GetWhereClause();

                SQLiteResultSet resultSet = dbClient.Execute(query);

                // store each one
                foreach (SQLiteResultSet.Row row in resultSet.Rows) {
                    DatabaseTable newRecord = (DatabaseTable)tableType.GetConstructor(System.Type.EmptyTypes).Invoke(null);
                    newRecord.DBManager = this;
                    newRecord.LoadByRow(row);
                    rtn.Add(newRecord);
                }

                cache.Sync(rtn);
            }
            catch (SQLiteException e) {
                logger.ErrorException("Error retrieving with criteria from " + tableType.Name + " table.", e);
            }

            return rtn;
        }

        // Based on the given table type and id, returns the cooresponding record.
        public T Get<T>(int id) where T : DatabaseTable {
            return (T) Get(typeof(T), id);
        }

        public DatabaseTable Get(Type tableType, int id) {
            // if we have already pulled this record down, don't query the DB
            DatabaseTable cachedObj = cache.Get(tableType, id);
            if (cachedObj != null)
                return cachedObj;
            
            verifyTable(tableType);

            try {
                // build and execute the query
                string query = getSelectQuery(tableType);
                query += "where id = " + id;
                SQLiteResultSet resultSet = dbClient.Execute(query);

                // make new object
                DatabaseTable newRecord = (DatabaseTable)tableType.GetConstructor(System.Type.EmptyTypes).Invoke(null);

                // if the given id doesn't exist, create a new uncommited record 
                if (resultSet.Rows.Count == 0) {
                    newRecord.Clear();
                    return newRecord;
                }

                // otherwise load it into the object
                newRecord.DBManager = this;
                newRecord.LoadByRow(resultSet.Rows[0]);
                cache.Add(newRecord);
                return newRecord;
            }
            catch (SQLiteException e) {
                logger.ErrorException("Error getting by ID from " + GetTableName(tableType) + " table.", e);
                return null;
            }
        }

        // Writes the given object to the database.
        public void Commit(DatabaseTable dbObject) {
            if (dbObject == null)
                return;

            if (!dbObject.CommitNeeded)
                return;

            verifyTable(dbObject.GetType());

            if (dbObject.ID == null)
                insert(dbObject);
            else
                update(dbObject);

            dbObject.CommitNeeded = false;
        }

        // Deletes a given object from the database, object in memory persists and could be recommited.
        public void Delete (DatabaseTable dbObject) {
            try {
                if (dbObject.ID == null) {
                    return;
                }

                string query = "delete from " + GetTableName(dbObject) + " where ID = " + dbObject.ID;
                dbClient.Execute(query);
                cache.Remove(dbObject);
                dbObject.ID = null;
            }
            catch (SQLiteException e) {
                logger.ErrorException("Error deleting object from " + GetTableName(dbObject) + " table.", e);
                return;
            }
        }

        #endregion

    }

    // A very simple object representing a database field. 
    public class DBField {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public enum DBDataType { INTEGER, REAL, TEXT, STRING_OBJECT, BOOL, DB_OBJECT }

        #region Private Variables
        private PropertyInfo propertyInfo;
        private DBFieldAttribute attribute;
        private DBDataType type;
        #endregion

        #region Constructors
        public DBField(PropertyInfo propertyInfo, DBFieldAttribute attribute) {
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
        // Returns the value of this field for the given object.
        public object GetValue(DatabaseTable owner) {
            return propertyInfo.GetGetMethod().Invoke(owner, null);
        }

        // sets the default value based on the datatype.
        public void initValue(DatabaseTable owner) {
            SetValue(owner, Default);
        }

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
                        propertyInfo.GetSetMethod().Invoke(owner, new object[] { float.Parse(value.ToString()) });
                        break;
                    case DBDataType.BOOL:
                        propertyInfo.GetSetMethod().Invoke(owner, new object[] { (value.ToString() == "true" || value.ToString() == "1") });
                        break;
                    case DBDataType.STRING_OBJECT:
                        // create a new object and populate it
                        IStringSourcedObject newObj = (IStringSourcedObject) propertyInfo.PropertyType.GetConstructor(System.Type.EmptyTypes).Invoke(null);
                        newObj.LoadFromString(value.ToString());
                        propertyInfo.GetSetMethod().Invoke(owner, new object[] { newObj });
                        break;
                    case DBDataType.DB_OBJECT:
                        DatabaseTable newDBObj = owner.DBManager.Get(propertyInfo.PropertyType, int.Parse(value.ToString()));
                        propertyInfo.GetSetMethod().Invoke(owner, new object[] { newDBObj });
                        break;
                    default:
                        propertyInfo.GetSetMethod().Invoke(owner, new object[] { value.ToString() });
                        break;
                }
            }
            catch (Exception e) {
                logger.ErrorException("Error writing to " + owner.GetType().Name + "." + this.Name + 
                                " Property. Sometimes indicates an out of date DB.", e);
            }
        }
        #endregion
    }
    
    #region Table Definition Attributes
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DBFieldAttribute : System.Attribute {
        private string fieldName = string.Empty;
        private string description = string.Empty;
        private string defaultValue = string.Empty;
        private bool allowAutoUpdate = true;

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

        public DBFieldAttribute() {
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DBTableAttribute : System.Attribute {
        private string tableName;
        private string description = string.Empty;

        public string TableName {
            get { return tableName; }
            set { tableName = value; }
        }

        public string Description {
            get { return description; }
            set { description = value; }
        }

        public DBTableAttribute(string tableName) {
            this.tableName = tableName;
        }
    }
    #endregion

    #region Criteria Classes
    public interface ICriteria {
        string GetWhereClause();
        string GetClause();
    }

    public class GroupedCriteria: ICriteria {
        public enum Operator { AND, OR }

        private ICriteria critA;
        private ICriteria critB;
        private Operator op;

        public GroupedCriteria(ICriteria critA, Operator op, ICriteria critB) {
            this.critA = critA;
            this.critB = critB;
            this.op = op;
        }

        public string GetWhereClause() {
            return " where " + GetClause();
        }

        public string GetClause() {
            return " (" + critA.GetClause() + " " + op.ToString() + " " + critB.GetClause() + ") ";
        }

        public override string ToString() {
            return GetWhereClause();
        }

    }

    public class BaseCriteria: ICriteria {
        // this should be used later for automated gui stuff, but for general use, would just complicate the code
        public enum Operator { EQUALS, LESS_THAN, LESS_THAN_OR_EQUAL, GREATER_THAN, GREATER_THAN_OR_EQUAL }

        private DBField field;
        private object value;
        private string op;

        public BaseCriteria(DBField field, string op, object value) {
            this.field = field;
            this.op = op;
            this.value = value;
        }

        public string GetWhereClause() {
            return " where " + GetClause();
        }

        public string GetClause() {
            return " (" + field.FieldName + " " + op + " " + DatabaseManager.getSQLiteString(value) + ") ";
        }

        public override string ToString() {
            return GetWhereClause();
        }
    }
    #endregion
}
