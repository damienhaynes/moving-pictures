﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using System.Threading;

namespace MediaPortal.Plugins.MovingPictures.Database {
    internal class FilterHelperDBMovieInfo: DynamicFilterHelper<DBMovieInfo> {

        public override bool UpdateDynamicNode(DBNode<DBMovieInfo> node) {
            if (node.BasicFilteringField == DBField.GetFieldByDBName(typeof(DBMovieInfo), "year")) {
                UpdateYear(node);
                return true;
            }

            if (node.BasicFilteringField == DBField.GetFieldByDBName(typeof(DBMovieInfo), "date_added")) {
                UpdateDateAdded(node);
                return true;
            }


            return false;
        }

        private void UpdateYear(DBNode<DBMovieInfo> node) {
            // grab list of possible years
            HashSet<string> allYears = node.DBManager.GetAllValues(node.BasicFilteringField, 
                                                                   node.BasicFilteringRelation, 
                                                                   node.GetFilteredItems());

            // build list of decades, each will coorespond to one subnode
            HashSet<int> decades = new HashSet<int>();
            foreach (string year in allYears) {
                int iYear;
                if (int.TryParse(year, out iYear))
                    decades.Add(iYear / 10);
            }

            // build lookup for subnodes and build list of nodes to remove
            List<DBNode<DBMovieInfo>> toRemove = new List<DBNode<DBMovieInfo>>();
            Dictionary<int, DBNode<DBMovieInfo>> nodeLookup = new Dictionary<int, DBNode<DBMovieInfo>>();
            foreach (DBNode<DBMovieInfo> currSubNode in node.Children) {
                if (!currSubNode.AutoGenerated)
                    continue;

                try {
                    int decade = (int.Parse(currSubNode.Filter.Criteria[1].Value.ToString()) / 10) - 10;
                    if (!decades.Contains(decade))
                        toRemove.Add(currSubNode);
                    else
                        nodeLookup[decade] = currSubNode;
                }
                catch (Exception e) {
                    if (e is ThreadAbortException)
                        throw e;

                    toRemove.Add(currSubNode);
                    continue;
                }
            }

            // remove subnodes that are no longer valid
            foreach (DBNode<DBMovieInfo> currSubNode in toRemove) {
                node.Children.Remove(currSubNode);
                currSubNode.Delete();
            }

            // add subnodes that are missing
            foreach (int currDecade in decades) {
                if (nodeLookup.ContainsKey(currDecade))
                    continue;

                DBNode<DBMovieInfo> newSubNode = new DBNode<DBMovieInfo>();
                newSubNode.Name = currDecade + "0s";
                newSubNode.AutoGenerated = true;

                DBFilter<DBMovieInfo> newFilter = new DBFilter<DBMovieInfo>();
                newFilter.CriteriaGrouping = DBFilter<DBMovieInfo>.CriteriaGroupingEnum.ALL;
                
                DBCriteria<DBMovieInfo> lowerCriteria = new DBCriteria<DBMovieInfo>();
                lowerCriteria.Field = node.BasicFilteringField;
                lowerCriteria.Relation = node.BasicFilteringRelation;
                lowerCriteria.Operator = DBCriteria<DBMovieInfo>.OperatorEnum.GREATER_THAN;
                lowerCriteria.Value = (currDecade * 10) - 1;
                newFilter.Criteria.Add(lowerCriteria);

                DBCriteria<DBMovieInfo> uppperCriteria = new DBCriteria<DBMovieInfo>();
                uppperCriteria.Field = node.BasicFilteringField;
                uppperCriteria.Relation = node.BasicFilteringRelation;
                uppperCriteria.Operator = DBCriteria<DBMovieInfo>.OperatorEnum.LESS_THAN;
                uppperCriteria.Value = (currDecade * 10) + 10;
                newFilter.Criteria.Add(uppperCriteria);

                newSubNode.Filter = newFilter;

                node.Children.Add(newSubNode);
                newSubNode.Parent = node;
            }

            node.Children.Sort();
        }

        private void UpdateDateAdded(DBNode<DBMovieInfo> node) {

        }

    }
}