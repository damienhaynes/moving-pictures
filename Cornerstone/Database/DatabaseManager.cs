using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using SQLite.NET;
using System.Windows.Forms;
using System.Reflection;
using NLog;
using Cornerstone.Database.Tables;
using Cornerstone.Database.CustomTypes;
using System.Globalization;

namespace Cornerstone.Database {
    public class DatabaseManager {

        #region Private

        private string dbFilename;
        private DatabaseCache cache;
        private Dictionary<Type, bool> isVerified;
        private Dictionary<Type, bool> doneFullRetrieve;

        private Dictionary<Type, IDynamicFilterHelper> filterHelperLookup;

        private HashSet<Type> preloading;

        private bool transactionInProgress = false;
        
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        private readonly object lockObject = new object();

        #endregion

        #region Events

        public delegate void ObjectAffectedDelegate(DatabaseTable obj);
        public event ObjectAffectedDelegate ObjectInserted;
        public event ObjectAffectedDelegate ObjectDeleted;
        public event ObjectAffectedDelegate ObjectUpdated;

        #endregion

        #region Public Methods

        private bool _connected = false;

        public bool IsClosed()
        {         
          return _connected == false;
        }

        private SQLiteClient dbClient {
            get {
                lock (lockObject) {
                    if (!_connected) {
                        try {
                            sqliteClient = new SQLiteClient(dbFilename);
                            sqliteClient.Execute("PRAGMA synchronous=OFF");
                            logger.Info("Successfully Opened Database: " + dbFilename);
                            _connected = true;
                        }
                        catch (Exception e) {
                            logger.FatalException("Could Not Open Database: " + dbFilename, e);
                            sqliteClient = null;
                        }
                    }
                }

                return sqliteClient;
            }
        } private SQLiteClient sqliteClient;

        public void Close() {
            lock (lockObject) {
                if (!_connected)
                    return;

                logger.Info("Closing database connection...");
                try {
                    sqliteClient.Close();
                    sqliteClient.Dispose();
                    logger.Info("Successfully closed Database: {0}", dbFilename);
                    _connected = false;
                }
                catch (Exception e) {
                    logger.ErrorException("Failed closing Database: " + dbFilename, e);
                }
            }
        }

        // Creates a new DatabaseManager based on the given filename.
        public DatabaseManager(string dbFilename) {
            this.dbFilename = dbFilename;

            isVerified = new Dictionary<Type, bool>();
            doneFullRetrieve = new Dictionary<Type, bool>();
            preloading = new HashSet<Type>();
            filterHelperLookup = new Dictionary<Type, IDynamicFilterHelper>();

            cache = new DatabaseCache();
        }

        /// <summary>
        /// Quickly retrieves all items in the database of the specified type and loads them into
        /// memory. This will provide much faster access time for some configurations.
        /// </summary>
        /// <param name="tableType"></param>
        public void PreLoad(Type tableType) {
            preloading.Add(tableType);
            List<DatabaseTable> items = Get(tableType, null);
            preloading.Remove(tableType);

            foreach (DatabaseTable currItem in items) 
                getAllRelationData(currItem);
        }

        // Returns a list of objects of the specified type, based on the specified criteria.
        public List<T> Get<T>(ICriteria criteria) where T : DatabaseTable {
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

            // if this is a request for all object of this type, if we already have done this 
            // type of request, just return the cached objects. This assumes no one else is changing
            // the DB.
            if (criteria == null) {
                if (doneFullRetrieve.ContainsKey(tableType))
                    return new List<DatabaseTable>(cache.GetAll(tableType));

                doneFullRetrieve[tableType] = true;
            }

            lock (lockObject) {
                List<DatabaseTable> rtn = new List<DatabaseTable>();

                try {
                    // build and execute the query
                    string query = getSelectQuery(tableType);
                    if (criteria != null)
                        query += criteria.GetWhereClause();

                    SQLiteResultSet resultSet = dbClient.Execute(query);

                    // store each one
                    foreach (SQLiteResultSet.Row row in resultSet.Rows) {
                        // create the new entry
                        DatabaseTable newRecord = (DatabaseTable)tableType.GetConstructor(System.Type.EmptyTypes).Invoke(null);
                        newRecord.DBManager = this; 
                        newRecord.LoadByRow(row);

                        // if it is already cached, just use the cached object
                        if (cache.Get(tableType, (int)newRecord.ID) != null)
                            rtn.Add(cache.Get(tableType, (int)newRecord.ID));

                        // otherwise use the new record and cache it
                        else {
                            newRecord = cache.Add(newRecord);
                            getAllRelationData(newRecord);
                            rtn.Add(newRecord);
                        }
                    }
                }
                catch (SQLiteException e) {
                    logger.ErrorException("Error retrieving with criteria from " + tableType.Name + " table.", e);
                }

                return rtn;
            }
        }

        // Based on the given table type and id, returns the cooresponding record.
        public T Get<T>(int id) where T : DatabaseTable {
            return (T)Get(typeof(T), id);
        }

        public DatabaseTable Get(Type tableType, int id) {
            // if we have already pulled this record down, don't query the DB
            DatabaseTable cachedObj = cache.Get(tableType, id);
            if (cachedObj != null)
                return cachedObj;

            verifyTable(tableType);

            lock (lockObject) {
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
                        return null;
                    }

                    // otherwise load it into the object
                    newRecord.DBManager = this;
                    newRecord.LoadByRow(resultSet.Rows[0]);
                    cache.Add(newRecord);
                    getAllRelationData(newRecord);
                    return newRecord;
                }
                catch (SQLiteException e) {
                    logger.ErrorException("Error getting by ID from " + GetTableName(tableType) + " table.", e);
                    return null;
                }
            }
        }

        public void Populate(IRelationList relationList) {
            getRelationData(relationList.Owner, relationList.MetaData);
        }

        public void BeginTransaction() {
            if (!transactionInProgress) {
                transactionInProgress = true;
                try {
                    lock(lockObject) dbClient.Execute("BEGIN");
                }
                catch (SQLiteException) {
                    logger.Error("Failed to BEGIN a SQLite Transaction.");
                }
            }
        }

        public void EndTransaction() {
            if (transactionInProgress) {
                transactionInProgress = false;
                try {
                    lock(lockObject) dbClient.Execute("COMMIT");
                }
                catch (SQLiteException) {
                    logger.Error("Failed to COMMIT a SQLite Transaction.");
                }
            }
        }

        public void Commit(IRelationList relationList) {
            updateRelationTable(relationList.Owner, relationList.MetaData);
        }

        // Writes the given object to the database.
        public void Commit(DatabaseTable dbObject) {
            if (dbObject == null)
                return;

            if (dbObject.CommitInProcess)
                return;

            if (dbObject.DBManager == null)
                dbObject.DBManager = this;

            dbObject.CommitInProcess = true;

            if (dbObject.CommitNeeded) {
                verifyTable(dbObject.GetType());

                dbObject.BeforeCommit();

                if (dbObject.ID == null) insert(dbObject);
                else update(dbObject);
            }

            CommitRelations(dbObject);

            dbObject.CommitInProcess = false;
            dbObject.CommitNeeded = false;
            dbObject.AfterCommit();
        }

        public void CommitRelations(DatabaseTable dbObject) {
            if (dbObject == null) return;

            foreach (DBRelation currRelation in DBRelation.GetRelations(dbObject.GetType())) {
                if (currRelation.AutoRetrieve) {
                    foreach (DatabaseTable subObj in currRelation.GetRelationList(dbObject))
                        Commit(subObj);

                    updateRelationTable(dbObject, currRelation);
                }
            }

            foreach (DBField currField in DBField.GetFieldList(dbObject.GetType())) {
                if (currField.DBType == DBField.DBDataType.DB_OBJECT) {
                    Commit((DatabaseTable)currField.GetValue(dbObject));
                }
            }

        }

        // Deletes a given object from the database, object in memory persists and could be recommited.
        public void Delete(DatabaseTable dbObject) {
            try {
                if (dbObject.ID == null) {
                    logger.Warn("Tried to delete an uncommited object...");
                    return;
                }

                dbObject.BeforeDelete();

                string query = "delete from " + GetTableName(dbObject) + " where ID = " + dbObject.ID;
                logger.Debug("DELETING: " + dbObject);
                logger.Debug(query);

                deleteAllRelationData(dbObject);
                lock(lockObject) dbClient.Execute(query);

                cache.Remove(dbObject);
                dbObject.ID = null;
                dbObject.AfterDelete();

                if (ObjectDeleted != null) {
                    logger.Debug("Calling listeners for " + dbObject.ToString());
                    ObjectDeleted(dbObject);
                }
            }
            catch (SQLiteException e) {
                logger.ErrorException("Error deleting object from " + GetTableName(dbObject) + " table.", e);
                return;
            }

        }

        public HashSet<string> GetAllValues(DBField field) {
            ICollection items = Get(field.OwnerType, null);

            // loop through all items in the DB and grab all existing values for this field
            HashSet<string> uniqueStrings = new HashSet<string>();
            foreach (DatabaseTable currItem in items) {
                List<string> values = getValues(field.GetValue(currItem));
                foreach (string currStr in values)
                    uniqueStrings.Add(currStr);
            }

            return uniqueStrings;
        }

        public HashSet<string> GetAllValues<T>(DBField field, DBRelation relation, ICollection<T> items) where T:DatabaseTable {
            // loop through all items in the DB and grab all existing values for this field
            HashSet<string> uniqueStrings = new HashSet<string>();
            foreach (T currItem in items) {
                if (relation == null) {
                    List<string> values = getValues(field.GetValue(currItem));
                    foreach (string currStr in values)
                        uniqueStrings.Add(currStr);
                }
                else {
                    foreach (DatabaseTable currSubItem in relation.GetRelationList(currItem)) {
                        List<string> values = getValues(field.GetValue(currSubItem));
                        foreach (string currStr in values)
                            uniqueStrings.Add(currStr);
                    }
                }
            }

            return uniqueStrings;
        }

        private static List<string> getValues(object obj) {
            List<string> results = new List<string>();

            if (obj == null)
                return results;

            if (obj is string) {
                if (((string)obj).Trim().Length != 0)
                    results.Add((string)obj);
            }
            else if (obj is StringList) {
                foreach (string currValue in (StringList)obj) {
                    if (currValue != null && currValue.Trim().Length != 0)
                        results.Add(currValue);
                }
            }
            else if (obj is bool || obj is bool?) {
                results.Add("true");
                results.Add("false");
            }
            else {
                results.Add(obj.ToString());
            }

            return results;
        }

        public DynamicFilterHelper<T> GetFilterHelper<T>() where T : DatabaseTable {
            if (filterHelperLookup.ContainsKey(typeof(T)))
                return (DynamicFilterHelper<T>) filterHelperLookup[typeof(T)];

            return null;
        }

        public void AddFilterHelper<T>(DynamicFilterHelper<T> helper) where T: DatabaseTable {
            filterHelperLookup[typeof(T)] = helper;
        }

        #endregion

        #region Public Static Methods

        // Returns the name of the table of the given type.
        public static string GetTableName(Type tableType) {
            return getDBTableAttribute(tableType).TableName;
        }

        // Returns the name of the table of the given type.
        public static string GetTableName(DatabaseTable tableObject) {
            return GetTableName(tableObject.GetType());
        }

        public static bool IsDatabaseTableType(Type t) {
            Type currType = t;
            while (currType != null) {
                if (currType == typeof(DatabaseTable)) {
                    return true;
                }
                currType = currType.BaseType;
            }

            return false;
        }



        #endregion

        #region Private Methods

        // Checks that the table coorisponding to this type exists, and if it is missing, it creates it.
        // Also verifies all columns represented in the class are also present in the table, creating 
        // any missing. Needs to be enhanced to allow for changed defaults.
        private void verifyTable(Type tableType) {
            lock (lockObject) {
                // check that we haven't already verified this table
                if (isVerified.ContainsKey(tableType))
                    return;

                // attempt to grab table info for the type. if none exists, it's not tagged to be a table
                DBTableAttribute tableAttr = getDBTableAttribute(tableType);
                if (tableAttr == null)
                    return;

                try {
                    // check if the table exists in the database, if not, create it
                    SQLiteResultSet resultSet = dbClient.Execute("select * from sqlite_master where type='table' and name = '" + tableAttr.TableName + "'");
                    if (resultSet.Rows.Count == 0) {
                        resultSet = dbClient.Execute("create table " + tableAttr.TableName + " (id INTEGER primary key )");
                        logger.Info("Created " + tableAttr.TableName + " table.");
                    }

                    // grab existing table info from the DB
                    resultSet = dbClient.Execute("PRAGMA table_info(" + tableAttr.TableName + ")");

                    // loop through the CLASS DEFINED fields, and verify each is contained in the result set
                    foreach (DBField currField in DBField.GetFieldList(tableType)) {

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
                                defaultValue = getSQLiteString(currField, currField.Default);

                            dbClient.Execute("alter table " + tableAttr.TableName + " add column " + currField.FieldName + " " +
                                             currField.DBType.ToString() + " default " + defaultValue);
                            // logger.Debug("Added " + tableAttr.TableName + "." + currField.FieldName + " column.");
                        }
                    }

                    verifyRelationTables(tableType);
                    isVerified[tableType] = true;
                }
                catch (SQLiteException e) {
                    logger.ErrorException("Internal error verifying " + tableAttr.TableName + " (" + tableType.ToString() + ") table.", e);
                }
            }
        }

        private void verifyRelationTables(Type primaryType) {
            foreach (DBRelation currRelation in DBRelation.GetRelations(primaryType)) {
                try {
                    // check if the table exists in the database, if not, create it
                    SQLiteResultSet resultSet = dbClient.Execute("select * from sqlite_master where type='table' and name = '" + currRelation.TableName + "'");
                    if (resultSet.Rows.Count == 0) {
                        // create table
                        string createQuery =
                            "create table " + currRelation.TableName + " (id INTEGER primary key, " +
                            currRelation.PrimaryColumnName + " INTEGER, " +
                            currRelation.SecondaryColumnName + " INTEGER)";

                        resultSet = dbClient.Execute(createQuery);

                        // create index1
                        resultSet = dbClient.Execute("create index " + currRelation.TableName + "__index1 on " +
                            currRelation.TableName + " (" + currRelation.PrimaryColumnName + ")");

                        // create index2
                        resultSet = dbClient.Execute("create index " + currRelation.TableName + "__index2 on " +
                            currRelation.TableName + " (" + currRelation.SecondaryColumnName + ")");

                        logger.Debug("Created " + currRelation.TableName + " sub-table.");
                    }
                }
                catch (SQLiteException e) {
                    logger.FatalException("Error verifying " + currRelation.TableName + " subtable.", e);
                }
            }
        }

        // Returns the table attribute information for the given type.
        private static DBTableAttribute getDBTableAttribute(Type tableType) {
            // loop through the custom attributes of the type, if one of them is the type
            // we want, return it.
            object[] customAttrArray = tableType.GetCustomAttributes(true);
            foreach (object currAttr in customAttrArray) {
                if (currAttr.GetType() == typeof(DBTableAttribute))
                    return (DBTableAttribute)currAttr;
            }

            throw new Exception("Table class " + tableType.Name + " not tagged with DBTable attribute.");
        }

        // Returns a select statement retrieving all fields ordered as defined by FieldList
        // for the given Table Type. A where clause can be appended
        private static string getSelectQuery(Type tableType) {
            string query = "select ";
            foreach (DBField currField in DBField.GetFieldList(tableType)) {
                if (query != "select ")
                    query += ", ";

                query += currField.FieldName;
            }
            query += ", id from " + GetTableName(tableType) + " ";
            return query;
        }

        public static string getSQLiteString(object value) {
            return getSQLiteString(null, value);
        }

        // creates an escaped, quoted string representation of the given object
        public static string getSQLiteString(DBField ownerField, object value) {
            if (value == null)
                return "NULL";
            
            string strVal = "";

            // handle boolean types
            if (value.GetType() == typeof(bool) || value.GetType() == typeof(Boolean)) {
                if ((Boolean)value == true)
                    strVal = "1";
                else
                    strVal = "0";
            }
            // handle double types
            else if (value.GetType() == typeof(double) || value.GetType() == typeof(Double)) 
                strVal = ((double)value).ToString(new CultureInfo("en-US", false));

            // handle float types
            else if (value.GetType() == typeof(float) || value.GetType() == typeof(Single)) 
                strVal = ((float)value).ToString(new CultureInfo("en-US", false));

            // handle database table types
            else if (IsDatabaseTableType(value.GetType())) {
                if (ownerField != null && ownerField.Type != value.GetType())
                    strVal = ((DatabaseTable)value).ID.ToString() + "|||" + value.GetType().AssemblyQualifiedName;
                else
                    strVal = ((DatabaseTable)value).ID.ToString();

            }

            // if field represents metadata about another dbfield
            else if (value is DBField) {
                DBField field = (DBField)value;
                strVal = field.OwnerType.AssemblyQualifiedName + "|||" + field.FieldName;
            }

            // if field represents metadata about a relation (subtable)
            else if (value is DBRelation) {
                DBRelation relation = (DBRelation)value;
                strVal = relation.PrimaryType.AssemblyQualifiedName + "|||" +
                         relation.SecondaryType.AssemblyQualifiedName + "|||" +
                         relation.Identifier;
            }

            // handle C# Types, Need full qualified name to load types from other aseemblies
            else if (value is Type)
                strVal = ((Type)value).AssemblyQualifiedName;

            else if (value is DateTime) {
                strVal = ((DateTime)value).ToUniversalTime().ToString("u");
            }
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

        // inserts a new object to the database
        private void insert(DatabaseTable dbObject) {
            try {
                string queryFieldList = "";
                string queryValueList = "";

                // loop through the fields and build the strings for the query
                foreach (DBField currField in DBField.GetFieldList(dbObject.GetType())) {
                    if (queryFieldList != "") {
                        queryFieldList += ", ";
                        queryValueList += ", ";
                    }

                    // if we dont have an ID, commit as needed
                    if (currField.DBType == DBField.DBDataType.DB_OBJECT && currField.GetValue(dbObject) != null &&
                        ((DatabaseTable)currField.GetValue(dbObject)).ID == null)
                        Commit((DatabaseTable)currField.GetValue(dbObject));

                    queryFieldList += currField.FieldName;
                    queryValueList += getSQLiteString(currField, currField.GetValue(dbObject));
                }

                string query = "insert into " + GetTableName(dbObject.GetType()) +
                               " (" + queryFieldList + ") values (" + queryValueList + ")";

                logger.Debug("INSERTING: " + dbObject.ToString());
                
                lock(lockObject) {
                    dbClient.Execute(query);
                    dbObject.ID = dbClient.LastInsertID();
                }
                dbObject.DBManager = this;
                cache.Add(dbObject);

                // loop through the fields and commit attached objects as needed
                foreach (DBField currField in DBField.GetFieldList(dbObject.GetType())) {
                    if (currField.DBType == DBField.DBDataType.DB_OBJECT)
                        Commit((DatabaseTable)currField.GetValue(dbObject));
                }

                // notify any listeners of the status change
                if (ObjectInserted != null)
                    ObjectInserted(dbObject);
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
                foreach (DBField currField in DBField.GetFieldList(dbObject.GetType())) {
                    if (!firstField) {
                        query += ", ";
                    }

                    firstField = false;

                    // if this is a linked db object commit it as needed
                    if (currField.DBType == DBField.DBDataType.DB_OBJECT)
                        Commit((DatabaseTable)currField.GetValue(dbObject));

                    query += currField.FieldName + " = " + getSQLiteString(currField, currField.GetValue(dbObject));

                }

                // add the where clause
                query += " where id = " + dbObject.ID;

                // execute the query
                logger.Debug("UPDATING: " + dbObject.ToString());
                
                lock(lockObject) dbClient.Execute(query);
                dbObject.DBManager = this;

                updateRelationTables(dbObject);
                
                // notify any listeners of the status change
                if (ObjectUpdated != null)
                    ObjectUpdated(dbObject);
            }
            catch (SQLiteException e) {
                logger.ErrorException("Could not commit to " + GetTableName(dbObject.GetType()) + " table.", e);
            }

        }

        /// <summary>
        /// Inserts into the database all relation information. Dependent objects will be commited.
        /// </summary>
        /// <param name="dbObject">The primary object owning the RelationList to be populated.</param>
        /// <param name="forceRetrieval">Determines if ALL relations will be retrieved.</param>
        private void updateRelationTables(DatabaseTable dbObject) {
            foreach (DBRelation currRelation in DBRelation.GetRelations(dbObject.GetType())) {
                updateRelationTable(dbObject, currRelation);
            }            
        }

        private void updateRelationTable(DatabaseTable dbObject, DBRelation currRelation) {
            if (!currRelation.GetRelationList(dbObject).CommitNeeded)
                return;

            // clear out old values then insert the new
            deleteRelationData(dbObject, currRelation);

            // insert all relations to the database
            foreach (object currObj in (IList)currRelation.GetRelationList(dbObject)) {
                DatabaseTable currDBObj = (DatabaseTable)currObj;
                Commit(currDBObj);
                string insertQuery = "insert into " + currRelation.TableName + "(" +
                    currRelation.PrimaryColumnName + ", " +
                    currRelation.SecondaryColumnName + ") values (" +
                    dbObject.ID + ", " + currDBObj.ID + ")";

                lock(lockObject) dbClient.Execute(insertQuery);
            }

            currRelation.GetRelationList(dbObject).CommitNeeded = false;
        }

        // deletes all subtable data for the given object.
        private void deleteAllRelationData(DatabaseTable dbObject) {
            foreach (DBRelation currRelation in DBRelation.GetRelations(dbObject.GetType()))
                deleteRelationData(dbObject, currRelation);
        }

        private void deleteRelationData(DatabaseTable dbObject, DBRelation relation) {
            if (relation.PrimaryType != dbObject.GetType())
                return;

            string deleteQuery = "delete from " + relation.TableName + " where " + relation.PrimaryColumnName + "=" + dbObject.ID;

            lock (lockObject) dbClient.Execute(deleteQuery);
        }

        private void getAllRelationData(DatabaseTable dbObject) {
            if (preloading.Contains(dbObject.GetType()))
                return;

            foreach (DBRelation currRelation in DBRelation.GetRelations(dbObject.GetType())) {
                if (currRelation.AutoRetrieve)
                    getRelationData(dbObject, currRelation);
            }
        }

        private void getRelationData(DatabaseTable dbObject, DBRelation relation) {
            IRelationList list = relation.GetRelationList(dbObject);

            if (list.Populated)
                return;

            bool oldCommitNeededFlag = dbObject.CommitNeeded;
            list.Populated = true;

            // build query
            string selectQuery = "select " + relation.SecondaryColumnName + " from " +
                       relation.TableName + " where " + relation.PrimaryColumnName + "=" + dbObject.ID;

            // and retireve relations
            //logger.Debug("Getting Relation Data for " + dbObject.GetType().Name + "[" + dbObject.ID + "]" + relation.SecondaryType.Name + "::: " + selectQuery);
            SQLiteResultSet resultSet;
            lock (lockObject) resultSet = dbClient.Execute(selectQuery);

            // parse results and add them to the list
            list.Clear();
            foreach (SQLiteResultSet.Row currRow in resultSet.Rows) {
                int objID = int.Parse(currRow.fields[0]);
                DatabaseTable newObj = Get(relation.SecondaryType, objID);
                list.AddIgnoreSisterList(newObj);
            }

            // update flags as needed
            list.CommitNeeded = false;
            dbObject.CommitNeeded = oldCommitNeededFlag;
        }

        #endregion

    }

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
            return " (" + field.FieldName + " " + op + " " + DatabaseManager.getSQLiteString(field, value) + ") ";
        }

        public override string ToString() {
            return GetWhereClause();
        }
    }

    public class ListCriteria: ICriteria {

        List<DatabaseTable> list;
        private bool exclude;

        public ListCriteria(List<DatabaseTable> list, bool exclude) {
            this.list = list;
            this.exclude = exclude;
        }

        public string GetWhereClause() {
            return " where " + GetClause();
        }

        public string GetClause() {
            if (list == null) return "1=1";

            string rtn = " ID" + (exclude ? " not " : " ") +"in ( ";
            bool first = true;
            foreach (DatabaseTable currItem in list) {
                if (currItem.ID == null) continue;

                if (first) first = false;
                else rtn += ", ";

                rtn += currItem.ID;
            }

            rtn += ")";

            return rtn;
        }

        public override string ToString() {
            return GetWhereClause();
        }
    }
    
    #endregion
}
