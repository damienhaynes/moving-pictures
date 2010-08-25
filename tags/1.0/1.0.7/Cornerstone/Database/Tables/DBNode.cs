﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Cornerstone.Database.CustomTypes;
using System.Windows.Forms;
using System.Threading;
using NLog;

namespace Cornerstone.Database.Tables {
    public delegate void DBNodeEventHandler(IDBNode node, Type type);

    [DBTable("node")]
    public class DBNode<T>: DatabaseTable, IDBNode where T: DatabaseTable {
        
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static Random random = new Random();

        public event DBNodeEventHandler Modified;
        private bool updating = false;

        #region Database Fields
        
        public DBNode() {
            //Children.Changed += new ChangedEventHandler(RelationListChanged);
        }

        void RelationListChanged(object sender, EventArgs e) {
            //commitNeeded = true;
            OnModified();
        }

        [DBField]
        public string Name {
            get { return _name; }
            
            set {
                _name = value;
                commitNeeded = true;

                OnModified();
            }
        }
        private string _name;

        [DBField(Default=null)]
        public DBNode<T> Parent {
            get { return _parent; }
            set {
                _parent = value;
                commitNeeded = true;
            }
        } private DBNode<T> _parent;

        [DBField]
        public DBField BasicFilteringField {
            get { return _basicFilteringField; }

            set {
                _basicFilteringField = value;
                commitNeeded = true;

                OnModified();
            }
        } 
        private DBField _basicFilteringField = null;

        [DBField]
        public DBRelation BasicFilteringRelation {
            get { return _basicFilteringRelation; }

            set {
                _basicFilteringRelation = value;
                commitNeeded = true;

                OnModified();
            }
        } 
        private DBRelation _basicFilteringRelation;

        [DBField]
        public bool AutoGenerated {
            get { return _autoGenerated; }
            
            set {
                _autoGenerated = value;
                commitNeeded = true;

                OnModified();
            }
        } 
        private bool _autoGenerated = false;

        [DBField]
        public bool DynamicNode {
            get { return _dynamicNode; }

            set {
                _dynamicNode = value;
                commitNeeded = true;

                OnModified();
            }
        }
        private bool _dynamicNode = false;

        [DBField(Default=null)]
        public DBFilter<T> Filter {
            get { return _filter; }
            
            set {
                _filter = value;
                commitNeeded = true;

                OnModified();
            }
        } private DBFilter<T> _filter = null;

        [DBRelation(AutoRetrieve = true, OneWay=true)]
        public RelationList<DBNode<T>, DBNode<T>> Children {
            get {
                if (_children == null) {
                    _children = new RelationList<DBNode<T>, DBNode<T>>(this);
                }
                return _children;
            }
        } 
        RelationList<DBNode<T>, DBNode<T>> _children;

        [DBField]
        public DatabaseTable AdditionalSettings {
            get { return _additionalSettings; }
            set {
                _additionalSettings = value;
                commitNeeded = true;
            }
        } private DatabaseTable _additionalSettings;

        [DBField]
        public int SortPosition {
            get { return _sortPosition; }
            set {
                _sortPosition = value;
                commitNeeded = true;
            }
        } private int _sortPosition;

        #endregion

        public void OnModified() {
            if (Modified != null && !updating)
                Modified(this, typeof(DBNode<T>));
        }

        public override void Delete() {
            if (DBManager == null)
                return;
            
            DBManager.BeginTransaction();
            
            base.Delete();

            if (Filter != null)
                Filter.Delete();

            foreach (DBNode<T> currSubNode in Children) {
                currSubNode.Delete();
            }

            DBManager.EndTransaction();
        }

        /// <summary>
        /// Returns all items that should be displayed if this node is selected.
        /// </summary>
        /// <returns></returns>
        public HashSet<T> GetFilteredItems() {
            
            // seed all items
            HashSet<T> results = new HashSet<T>(DBManager.Get<T>(null));

            // apply filters
            HashSet<IFilter<T>> filters = GetAllFilters();
            foreach (IFilter<T> filter in filters) {
                results = filter.Filter(results);
            }

            return results;
        }

        public HashSet<IFilter<T>> GetAllFilters() {
            HashSet<IFilter<T>> results = new HashSet<IFilter<T>>();

            // get the filters for all parent nodes
            DBNode<T> currNode = this;
            while (currNode != null) {
                if (currNode.Filter != null)
                    results.Add(currNode.Filter);
                currNode = currNode.Parent;
            }

            return results;
        }

        /// <summary>
        /// Returns a list of all items that could result from filtering from any 
        /// of the sub nodes of this node.
        /// </summary>
        /// <returns></returns>
        public HashSet<T> GetPossibleFilteredItems() {
            return GetPossibleFilteredItemsWorker(GetFilteredItems(), false);
        }

        private HashSet<T> GetPossibleFilteredItemsWorker(HashSet<T> existingItems, bool applyFilter) {
            if (applyFilter && Filter != null)
                existingItems = Filter.Filter(existingItems);

            if (Children.Count == 0)
                return existingItems;

            HashSet<T> results = new HashSet<T>();
            foreach (DBNode<T> currSubNode in Children) {
                foreach (T currItem in currSubNode.GetPossibleFilteredItemsWorker(existingItems, true))
                    results.Add(currItem);
            }

            return results;
        }

        public T GetRandomSubItem() {
            HashSet<T> possibleItems = GetPossibleFilteredItems();
            int index = random.Next(possibleItems.Count);
            
            HashSet<T>.Enumerator enumerator = possibleItems.GetEnumerator();
            for (int i = 0; i <= index; i++)
                enumerator.MoveNext();

            return enumerator.Current;
        }

        public void UpdateDynamicNode() {
            if (!DynamicNode)
                return;

            updating = true;

            // try using a filtering helper for dynamic node maintenance
            if (DBManager.GetFilterHelper<T>() != null) {
                bool success = DBManager.GetFilterHelper<T>().UpdateDynamicNode(this);

                if (success) {
                    Children.Sort();

                    updating = false;
                    OnModified();
                    return;
                }
            }


            UpdateDynamicNodeGeneric();
            Children.Sort();

            updating = false;
            OnModified();
        }

        public void UpdateDynamicNodeGeneric() {
            // grab list of possible values
            HashSet<string> possibleValues = DBManager.GetAllValues(BasicFilteringField, BasicFilteringRelation, GetFilteredItems());

            // build lookup for subnodes and build list of nodes to remove
            List<DBNode<T>> toRemove = new List<DBNode<T>>();
            Dictionary<string, DBNode<T>> nodeLookup = new Dictionary<string, DBNode<T>>();
            foreach (DBNode<T> currSubNode in Children) {
                if (!currSubNode.AutoGenerated)
                    continue;

                if (!possibleValues.Contains(currSubNode.Filter.Criteria[0].Value.ToString()))
                    toRemove.Add(currSubNode);
                else
                    nodeLookup[currSubNode.Filter.Criteria[0].Value.ToString()] = currSubNode;
            }

            // remove subnodes that are no longer valid
            foreach (DBNode<T> currSubNode in toRemove) {
                Children.Remove(currSubNode);
                currSubNode.Delete();
            }

            // add subnodes that are missing
            foreach (string currValue in possibleValues) {
                if (nodeLookup.ContainsKey(currValue))
                    continue;

                DBNode<T> newSubNode = new DBNode<T>();
                newSubNode.Name = currValue;
                newSubNode.AutoGenerated = true;

                DBFilter<T> newFilter = new DBFilter<T>();
                DBCriteria<T> newCriteria = new DBCriteria<T>();
                newCriteria.Field = BasicFilteringField;
                newCriteria.Relation = BasicFilteringRelation;
                newCriteria.Operator = DBCriteria<T>.OperatorEnum.EQUAL;
                newCriteria.Value = currValue;

                newFilter.Criteria.Add(newCriteria);
                newSubNode.Filter = newFilter;

                Children.Add(newSubNode);
                newSubNode.Parent = this;
            }
        }

        public override string ToString() {
            return "DBNode: " + Name + " (" + ID + ")";
        }

        public override int CompareTo(object obj) {
            int rt = this.SortPosition.CompareTo(((DBNode<T>)obj).SortPosition);
            if (rt == 0)
                return this.ToString().CompareTo(obj.ToString());
            else
                return rt;
        }

    }

    public interface IDBNode { }
}
