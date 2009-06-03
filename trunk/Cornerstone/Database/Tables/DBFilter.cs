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

        public List<T> Filter(List<T> input) {
            if (!_active)
                return input;

            List<T> results = new List<T>();

            // if there is no criteria, just use the white list
            if (Criteria.Count == 0) {
                foreach (T currItem in WhiteList)
                    results.Add(currItem);
                return results;
            }

            // remove blacklist items
            foreach (T currItem in input) {
                if (!BlackList.Contains(currItem))
                    results.Add(currItem);
            }

            // handle AND type criteria
            if (CriteriaGrouping == CriteriaGroupingEnum.ALL)
                foreach (DBCriteria<T> currCriteria in Criteria) 
                    results = currCriteria.Filter(results);

            // handle OR type criteria
            if (CriteriaGrouping == CriteriaGroupingEnum.ONE) {
                List<T> okItems = new List<T>();
                foreach (DBCriteria<T> currCriteria in Criteria) {
                    List<T> tmp = currCriteria.Filter(results);
                    foreach (T currItem in tmp)
                        if (!okItems.Contains(currItem))
                            okItems.Add(currItem);
                }

                results = okItems;
            }

            // handle NONE type criteria
            if (CriteriaGrouping == CriteriaGroupingEnum.NONE) {
                List<T> excludeItems = new List<T>();
                excludeItems.AddRange(results);

                foreach (DBCriteria<T> currCriteria in Criteria)
                    excludeItems = currCriteria.Filter(excludeItems);

                foreach(T item in excludeItems)
                    results.Remove(item);   
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
