using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Collections;
using Cornerstone.Database.Tables;

namespace Cornerstone.Database.CustomTypes {
    public class RelationList<T1, T2>: DynamicList<T2>, IRelationList 
        where T1: DatabaseTable 
        where T2: DatabaseTable {

        public RelationList(T1 owner) {
            this._owner = owner;
        }

        #region IRelationList Implementation
      
        #region Properties
        public DBRelation MetaData {
            get {
                ReadOnlyCollection<DBRelation> metadataList = DBRelation.GetRelations(typeof(T1));
                foreach (DBRelation currData in metadataList)
                    if (currData.GetRelationList(_owner) == this) {
                        _metaData = currData;
                        break;
                    }
                return _metaData;
            }
        } private DBRelation _metaData = null;

        public bool Populated {
            get { return _populated; }
            set { _populated = value; }
        } private bool _populated = false;

        public bool CommitNeeded {
            get { return _commitNeeded; }
            set { _commitNeeded = value; }
        } bool _commitNeeded = false;

        public DatabaseTable Owner {
            get { return _owner; }
        } private T1 _owner;
        #endregion

        #region Methods

        public void Commit() {
            if (Owner.DBManager != null)
                Owner.DBManager.Commit(this);
        }

        public bool RemoveIgnoreSisterList(DatabaseTable item) {
            if (item == null || item.GetType() != typeof(T2))
                return false;

            return base.Remove((T2)item);
        }

        public void AddIgnoreSisterList(DatabaseTable item) {
            if (item == null || item.GetType() != typeof(T2))
                return;

            base.Add((T2)item);
        }

        #endregion

        #endregion

        #region Overrides
        
        protected override void OnChanged(EventArgs e) {
            base.OnChanged(e);
            _commitNeeded = true;
        }

        // Gets or sets the element at the specified index.
        public override T2 this[int index] {
            get {
                return base[index];
            }
            set {
                base[index] = value;
                addSelfToSisterList(value);
            }
        }

        // Adds an object to the end of the List.
        public override void Add(T2 item) {
            base.Add(item);
            addSelfToSisterList(item);
        }

        // Adds the elements of the specified collection to the end of the List.
        public override void AddRange(IEnumerable<T2> collection) {
            base.AddRange(collection);

            foreach (T2 currSister in collection)
                addSelfToSisterList(currSister);
        }

        // Removes all elements from the List.
        public override void Clear() {
            foreach (T2 currSister in this)
                removeSelfFromSister(currSister);

            base.Clear();
        }

        // Inserts an element into the List at the specified index.
        public override void Insert(int index, T2 item) {
            base.Insert(index, item);
            addSelfToSisterList(item);
        }

        // Inserts the elements of a collection into the List at the specified index.
        public override void InsertRange(int index, IEnumerable<T2> collection) {
            base.InsertRange(index, collection);

            foreach (T2 currSister in collection)
                addSelfToSisterList(currSister);
        }

        // Removes the first occurrence of a specific object from the List.
        public override bool Remove(T2 item) {
            if (base.Remove(item)) {
                removeSelfFromSister(item);
                return true;
            }

            return false;
        }

        // Removes the all the elements that match the conditions defined by the specified predicate.
        public override int RemoveAll(Predicate<T2> match) {
            throw new NotImplementedException();
        }

        // Removes the element at the specified index of the List.
        public override void RemoveAt(int index) {
            T2 sister = this[index];
            base.RemoveAt(index);
            removeSelfFromSister(sister);
        }

        // Removes a range of elements from the List.
        public override void RemoveRange(int index, int count) {
            throw new NotImplementedException();
        }

        #endregion

        #region Private

        private MethodInfo getRelationListMethod = null;

        private RelationList<T2, T1> getSisterList(T2 sister) {
            // if we have not retrieved it, use reflection to grab the method that
            // returns the sister list given a specific object of type T1
            if (getRelationListMethod == null) {
                foreach (PropertyInfo currProperty in typeof(T2).GetProperties())
                    foreach (object currAttr in currProperty.GetCustomAttributes(true))
                        // if we have come to a relation property, lets check it
                        if (currAttr.GetType() == typeof(DBRelationAttribute) && !((DBRelationAttribute)currAttr).OneWay) {
                            if (currProperty.PropertyType.GetGenericArguments()[0] == typeof(T2) &&
                                currProperty.PropertyType.GetGenericArguments()[1] == typeof(T1) &&
                                ((DBRelationAttribute)currAttr).Identifier == MetaData.Identifier)
                                getRelationListMethod = currProperty.GetGetMethod();
                        }
            }

            if (getRelationListMethod == null)
                return null;

            return (RelationList<T2, T1>) getRelationListMethod.Invoke(sister, null);
        }

        private void addSelfToSisterList(T2 sister) {
            // try to grab the sister list and if it doesn't exist there is no work to do
            RelationList<T2, T1> sisterList = getSisterList(sister);
            if (sisterList == null) return;

            sisterList.AddIgnoreSisterList(_owner);
        }

        private void removeSelfFromSister(T2 sister) {
            // try to grab the sister list and if it doesn't exist there is no work to do
            RelationList<T2, T1> sisterList = getSisterList(sister);
            if (sisterList == null) return;

            sisterList.RemoveIgnoreSisterList(_owner);
        }

        #endregion

    }

    // The public interface for the RelationList class. This allows use of the Relationlist class
    // without knowing it's generic types. Unfortunately C# wont allow a 
    // (RelatilonList<DatabaseTable, DatabaseTable>) style cast.
    public interface IRelationList: IList, IDynamic {
        DBRelation MetaData {
            get;
        }

        bool Populated {
            get;
            set;
        } 

        bool CommitNeeded {
            get;
            set;
        }

        DatabaseTable Owner {
            get;
        }

        bool RemoveIgnoreSisterList(DatabaseTable item);

        void AddIgnoreSisterList(DatabaseTable item);
    }
}
