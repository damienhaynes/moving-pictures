using System;
using System.Collections.Generic;
using System.Text;
using SQLite.NET;
using System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;
using MediaPortal.Plugins.MovingPictures.Database.CustomTypes;

namespace MediaPortal.Plugins.MovingPictures.Database {
    public class DatabaseTable {
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

        // Loads data into this object based on the given database record.
        public void LoadByRow(SQLiteResultSet.Row row) {
            List<DBField> fieldList = DatabaseManager.GetFieldList(this.GetType());

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

        // Updates the current object with all fields in the newData object that are
        // not set to default.
        public void CopyUpdatableValues(DatabaseTable newData) {
            if (newData == null) return;
            List<DBField> fieldList = DatabaseManager.GetFieldList(newData.GetType());

            foreach (DBField currField in fieldList) {
                object newValue = currField.GetValue(newData);
                object oldValue = currField.GetValue(this);
                if (currField.AutoUpdate) {
                    // if the updated value is just the default, don't update. 
                    // something is better than nothing
                    if (newValue.Equals(currField.Default))
                        continue;

                    // if we have a string try to compare trimmed strings
                    if (newValue.GetType() == typeof(string) && ((string)newValue).Trim() == ((string)oldValue).Trim())
                        continue;

                    // finally if the value just hasn't changed, dont update
                    if (newValue.Equals(oldValue))
                        continue;

                    currField.SetValue(this, newValue);
                }
            }
        }

        // initialize all values of the given object. essentially makes the object a new refernece
        // and a new row will be created if commited
        public void Clear() {
            id = null;
            commitNeeded = true;

            List<DBField> fieldList = DatabaseManager.GetFieldList(this.GetType());
            foreach (DBField currField in fieldList) {
                object defaultVal = currField.Default;
                currField.SetValue(this, defaultVal);

                // if this is a dynamic (internally changable) object, setup a listener
                if (defaultVal != null && defaultVal.GetType() == typeof(IDynamic))
                    ((IDynamic) defaultVal).Changed += new ChangedEventHandler(commitNeededEventHandler);
            }
        }

        #endregion
    }
}
