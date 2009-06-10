using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database.CustomTypes;

namespace Cornerstone.Database.Tables {
    [DBTableAttribute("filters")]
    public class DBFilter<T> : GenericDatabaseTable<T>, IFilter<T>, IDBFilter, IGenericFilter
        where T : DatabaseTable {
        
        public enum CriteriaGroupingEnum {
            ALL,
            ONE,
            NONE
        }
        
        #region IFilter<T> Members

        public event FilterUpdatedDelegate<T> Updated;

        public HashSet<T> Filter(ICollection<T> input) {
            HashSet<T> results = new HashSet<T>();

            // if we are not active, just return the inputs.
            if (!_active) {
                if (input is HashSet<T>)
                    return (HashSet<T>) input;

                foreach (T currItem in input)
                    results.Add(currItem);
                return results;
            }


            // if there is no criteria, just use the white list
            if (Criteria.Count == 0) {
                foreach (T currItem in WhiteList)
                    results.Add(currItem);
                return results;
            }

            // handle AND type criteria
            bool first = true;
            if (CriteriaGrouping == CriteriaGroupingEnum.ALL)
                foreach (DBCriteria<T> currCriteria in Criteria) {
                    results = currCriteria.Filter(first ? input : results);
                    first = false;
                }

            // handle OR type criteria
            if (CriteriaGrouping == CriteriaGroupingEnum.ONE) {
                HashSet<T> okItems = new HashSet<T>();
                foreach (DBCriteria<T> currCriteria in Criteria) {
                    HashSet<T> tmp = currCriteria.Filter(input);
                    okItems.UnionWith(tmp);
                }

                results = okItems;
            }

            // handle NONE type criteria
            if (CriteriaGrouping == CriteriaGroupingEnum.NONE) {
                HashSet<T> excludeItems = new HashSet<T>();
                foreach (T currItem in input)
                    excludeItems.Add(currItem);

                foreach (DBCriteria<T> currCriteria in Criteria)
                    excludeItems = currCriteria.Filter(excludeItems);

                foreach(T item in excludeItems)
                    results.Remove(item);   
            }

            // remove blacklist items
            foreach (T currItem in BlackList) {
                if (BlackList.Contains(currItem))
                    results.Remove(currItem);
            }

            // make sure all whitelist items are in the result list
            foreach (T item in WhiteList)
                if (!results.Contains(item))
                    results.Add(item);

            return results;
        }

        public bool Active {
            get { return _active; }
            set {
                if (_active != value) {
                    _active = value;

                    if (Updated != null)
                        Updated(this);
                }
            }
        }
        private bool _active = true;

        #endregion

        #region Database Fields
        
        [DBField]
        public string Name {
            get { return _name; }
            set {
                _name = value;
                commitNeeded = true;
            }
        } private string _name;

        [DBField]
        public CriteriaGroupingEnum CriteriaGrouping {
            get { return _criteriaGrouping; }

            set {
                _criteriaGrouping = value;
                commitNeeded = true;
            }
        } private CriteriaGroupingEnum _criteriaGrouping;

        [DBRelation(AutoRetrieve = true)]
        public RelationList<DBFilter<T>, DBCriteria<T>> Criteria {
            get {
                if (_criteria == null) {
                    _criteria = new RelationList<DBFilter<T>,DBCriteria<T>>(this);
                }
                return _criteria;
            }
        } RelationList<DBFilter<T>, DBCriteria<T>> _criteria;

        [DBRelation(AutoRetrieve = true, Identifier="white_list")]
        public RelationList<DBFilter<T>, T> WhiteList {
            get {
                if (_whiteList == null) {
                    _whiteList = new RelationList<DBFilter<T>, T>(this);
                }
                return _whiteList;
            }
        } RelationList<DBFilter<T>, T> _whiteList;

        [DBRelation(AutoRetrieve = true, Identifier="black_list")]
        public RelationList<DBFilter<T>, T> BlackList {
            get {
                if (_blackList == null) {
                    _blackList = new RelationList<DBFilter<T>, T>(this);
                }
                return _blackList;
            }
        } RelationList<DBFilter<T>, T> _blackList;


        #endregion

        public override void Commit() {
            base.Commit();

            foreach (DBCriteria<T> currCriteria in Criteria) {
                DBManager.Commit(currCriteria);
            }
        }
    }

    // empty interface to handle DBFilters generically
    public interface IDBFilter {}

    // empty interface to handle DBFilters generically
    public interface IGenericFilter { }
}
