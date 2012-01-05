using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Database.Tables;
using Cornerstone.Database;

namespace Cornerstone.Tools.Search {
    public abstract class AbstractSearcher<T> where T: DatabaseTable {

        public struct SearchResult {
            public T Item;
            public float Score;
        }

        /// <summary>
        /// The database that searches will be performed on.
        /// </summary>
        public DatabaseManager DatabaseManager {
            get { return _databaseManager; }
            set {
                _databaseManager = value;
                VerifyDatabase();
            }
        } protected DatabaseManager _databaseManager = null;
                
        /// <summary>
        /// The fields to include when performing a search. These fields must be owned by the type T.
        /// </summary>
        public List<DBField> SearchFields {
            get { return _searchFields; }
            set { 
                _searchFields = value;
                VerifyFields();
            }
        } protected List<DBField> _searchFields = new List<DBField>();

        /// <summary>
        /// Initialized the searcher with database information and the fields to search.
        /// </summary>
        public AbstractSearcher(DatabaseManager db, ICollection<DBField> fields) {
            init(db, fields);
        }

        /// <summary>
        /// Initialized the searcher with database information and the field names to search.
        /// </summary>
        public AbstractSearcher(DatabaseManager db, string[] fieldNames) {
            List<DBField> fields = new List<DBField>();
            foreach (string currName in fieldNames) {
                DBField currField = DBField.GetFieldByDBName(typeof(T), currName);
                if (currField == null) throw new SearchException(SearchException.ErrorTypeEnum.INVALID_FIELDS);
                
                fields.Add(currField);
            }
            
            init(db, fields);            
        }

        private void init(DatabaseManager db, ICollection<DBField> fields) {
            DatabaseManager = db;
            SearchFields.AddRange(fields);

            VerifyDatabase();
            VerifyFields();
        }

        /// <summary>
        /// Builds a search index from the specified list of items.
        /// </summary>
        public virtual void BuildIndex(List<T> items) {
            VerifyDatabase();
            VerifyFields();

            Clear();
            AddRange(items);
        }

        /// <summary>
        /// Builds a search index from all elements in the database. Items added to or removed from the 
        /// database will automatically be added to or removed from the Index.
        /// </summary>
        public virtual void BuildDynamicIndex() {
            VerifyDatabase();
            VerifyFields();

            Clear();
            List<T> allItems = DatabaseManager.Get<T>(null);
            lock (DatabaseManager) {
                DatabaseManager.ObjectInserted += new DatabaseManager.ObjectAffectedDelegate(DatabaseManager_ObjectInserted);
                DatabaseManager.ObjectDeleted += new DatabaseManager.ObjectAffectedDelegate(DatabaseManager_ObjectDeleted);
                DatabaseManager.ObjectUpdated += new Database.DatabaseManager.ObjectAffectedDelegate(DatabaseManager_ObjectUpdated);

                AddRange(allItems);
            }
        }

        /// <summary>
        /// Add a collection of items to the search index.
        /// </summary>
        public virtual void AddRange(ICollection<T> items) {
            foreach (T currItem in items)
                Add(currItem);
        }

        /// <summary>
        /// Add a single item to the search index.
        /// </summary>
        public abstract void Add(T item);

        /// <summary>
        /// Remove a single item from the search index.
        /// </summary>
        public abstract void Remove(T item);

        /// <summary>
        /// Completely clears the search index.
        /// </summary>
        public virtual void Clear() {
            DatabaseManager.ObjectInserted -= new DatabaseManager.ObjectAffectedDelegate(DatabaseManager_ObjectInserted);
            DatabaseManager.ObjectDeleted -= new DatabaseManager.ObjectAffectedDelegate(DatabaseManager_ObjectDeleted);
            DatabaseManager.ObjectUpdated -= new Database.DatabaseManager.ObjectAffectedDelegate(DatabaseManager_ObjectUpdated);
        }

        /// <summary>
        /// Perform a search based on the given search string.
        /// </summary>
        public abstract List<SearchResult> Search(string searchStr);

        protected void VerifyFields() {
            if (SearchFields == null || SearchFields.Count == 0)
                throw new SearchException(SearchException.ErrorTypeEnum.MISSING_FIELDS);

            foreach (DBField currField in SearchFields) {
                if (!(currField.OwnerType == typeof(T)))
                    throw new SearchException(SearchException.ErrorTypeEnum.INVALID_FIELDS);
            }
        }

        protected void VerifyDatabase() {
            if (DatabaseManager == null)
                throw new SearchException(SearchException.ErrorTypeEnum.MISSING_DATABASE);
        }

        private void DatabaseManager_ObjectDeleted(DatabaseTable obj) {
            lock (DatabaseManager) 
                if (obj is T) Add((T)obj);
        }

        private void DatabaseManager_ObjectInserted(DatabaseTable obj) {
            lock (DatabaseManager) 
                if (obj is T) Remove((T)obj);
        }

        private void DatabaseManager_ObjectUpdated(DatabaseTable obj) {
            lock (DatabaseManager) if (obj is T) {
                Remove((T)obj);
                Add((T)obj);
            }
        }

    }
}
