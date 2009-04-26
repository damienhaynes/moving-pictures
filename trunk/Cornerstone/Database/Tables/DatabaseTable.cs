using System;
using System.Collections.Generic;
using System.Text;
using SQLite.NET;
using System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Cornerstone.Database.CustomTypes;

namespace Cornerstone.Database.Tables {
    public abstract class DatabaseTable {
        #region Properties

        public int? ID {
            get { return id; }
            set { id = value; }
        } private int? id;

        // The database manager that created this object. Only is valid if
        // this object is actually linked to a DB record.
        public DatabaseManager DBManager {
            get { return dbManager; }
            set { dbManager = value; }
        } private DatabaseManager dbManager = null;

        // Flag deterimining whether a database commit is needed. 
        public bool CommitNeeded {
            get { return commitNeeded; }
            set { commitNeeded = value; }
        } protected bool commitNeeded;

        protected void commitNeededEventHandler(object sender, EventArgs e) {
            commitNeeded = true;
        }

        #endregion

        #region Public Methods

        public DatabaseTable() {
            Clear();
        }

        public string GetTableName() {
            return DatabaseManager.GetTableName(this);
        }

        public virtual void BeforeCommit() {
        }

        public virtual void AfterCommit() {
        }

        public virtual void BeforeDelete() {
        }

        public virtual void AfterDelete() {
        }

        // Loads data into this object based on the given database record.
        public void LoadByRow(SQLiteResultSet.Row row) {
            ReadOnlyCollection<DBField> fieldList = DBField.GetFieldList(this.GetType());

            // load each field one at a time. they should have been retrieved in the
            // ordering in FieldList
            int i;
            for (i = 0; i < fieldList.Count; i++) {
                if (row.fields[i] == "")
                    fieldList[i].SetValue(this, null);
                else
                    fieldList[i].SetValue(this, row.fields[i]);
            }

            // id is always at the end, assign that too
            id = int.Parse(row.fields[i]);

            // all values are in sync with DB so no commit needed.
            commitNeeded = false;
        }


        // This method protects all non-default values from getting overwritten by the next call to CopyUpdatableValues
        // todo: overload this method to specify fields that should be protected
        public void ProtectExistingValuesFromCopy(bool protect) {
            protectExistingValuesFromCopy = protect;
        } private bool protectExistingValuesFromCopy = true;

        // Updates the current object with all fields in the newData object that are
        // not set to default.
        public void CopyUpdatableValues(DatabaseTable newData) {
            if (newData == null) return;
            ReadOnlyCollection<DBField> fieldList = DBField.GetFieldList(newData.GetType());

            foreach (DBField currField in fieldList) {
                object newValue = currField.GetValue(newData);
                object oldValue = currField.GetValue(this);
                if (currField.AutoUpdate) {

                    if (newValue == null) {
                        currField.SetValue(this, newValue);
                        continue;
                    }

                    // if the updated value is just the default, don't update. 
                    // something is better than nothing
                    if (newValue.Equals(currField.Default))
                        continue;

                    // if we have a string try to compare trimmed strings
                    if (newValue.GetType() == typeof(string) && ((string)newValue).Trim() == ((string)oldValue).Trim())
                        continue;

                    // if the value just hasn't changed, dont update
                    if (newValue.Equals(oldValue))
                        continue;

                    // finally check if we are protecting values from getting overwritten
                    if (protectExistingValuesFromCopy && !oldValue.Equals(currField.Default))
                        continue;

                    currField.SetValue(this, newValue);
                }
            }
            // reset the value protection to true again
            protectExistingValuesFromCopy = true;
        }

        // initialize all values of the given object. essentially makes the object a new refernece
        // and a new row will be created if commited
        public void Clear() {
            id = null;
            commitNeeded = true;

            ReadOnlyCollection<DBField> fieldList = DBField.GetFieldList(this.GetType());
            foreach (DBField currField in fieldList) {
                object defaultVal = currField.Default;
                currField.SetValue(this, defaultVal);

                // if this is a dynamic (internally changable) object, setup a listener
                if (defaultVal != null && defaultVal.GetType() == typeof(IDynamic))
                    ((IDynamic) defaultVal).Changed += new ChangedEventHandler(commitNeededEventHandler);
            }

            ReadOnlyCollection<DBRelation> relationList = DBRelation.GetRelationList(this.GetType());
            foreach (DBRelation currRelation in relationList) {
                try {
                    currRelation.GetRelationList(this).Changed += new ChangedEventHandler(commitNeededEventHandler);
                }
                catch (NullReferenceException) {
                    throw new System.InvalidOperationException("RelationLists must be initialized in the get{} property method.");
                }
            }
        }

        public virtual void Commit() {
            if (DBManager != null)
                DBManager.Commit(this);
        }

        public virtual void Delete() {
            if (DBManager != null)
                DBManager.Delete(this);
        }


        #endregion
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
}


