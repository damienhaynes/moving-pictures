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
    }

    public interface IDBMenu { }
}
