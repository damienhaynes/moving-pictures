using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database.Tables;

namespace Cornerstone.GUI.Controls {
    public interface IDBBackedControl {
        // The database object type that the control displays data about.
        Type Table {
            get;
            set;
        }

        // The object cotnaining the data to be displayed.
        DatabaseTable DatabaseObject {
            get;
            set;
        }
    }
}
