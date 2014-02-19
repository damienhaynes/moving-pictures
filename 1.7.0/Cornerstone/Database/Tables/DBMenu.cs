using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database.CustomTypes;

namespace Cornerstone.Database.Tables {
    [DBTable("menu")]
    public class DBMenu<T>: DatabaseTable, IDBMenu where T: DatabaseTable {

        public DBMenu() {
            RootNodes.Changed += new ChangedEventHandler(RootNodes_Changed);
        }

        void RootNodes_Changed(object sender, EventArgs e) {
            commitNeeded = true;
        }

        #region Database Fields

        [DBField]
        public string Name {
            get { return _name; }

            set {
                _name = value;
                commitNeeded = true;
            }
        }
        private string _name;

        [DBRelation(AutoRetrieve = true)]
        public RelationList<DBMenu<T>, DBNode<T>> RootNodes {
            get {
                if (_rootNodes == null) {
                    _rootNodes = new RelationList<DBMenu<T>, DBNode<T>>(this);
                }
                return _rootNodes;
            }
        }
        RelationList<DBMenu<T>, DBNode<T>> _rootNodes;
        
        #endregion

        #region Helper Methods

        public List<DBNode<T>> FindNode(string nodeName) {
            return FindNode(nodeName, RootNodes);
        }

        private List<DBNode<T>> FindNode(string nodeName, IList<DBNode<T>> nodes) {
            List<DBNode<T>> results = new List<DBNode<T>>();
            foreach (DBNode<T> currNode in nodes) {
                // check if this is the node we are looking for
                if (currNode.Name == nodeName) 
                    results.Add(currNode);
                
                // recursively search the children
                if (currNode.Children.Count > 0) 
                    results.AddRange(FindNode(nodeName, currNode.Children));
            }

            return results;
        }

        #endregion
    }

    public interface IDBMenu { }
}
