using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database;
using System.Windows.Forms;
using Cornerstone.Database.Tables;

namespace Cornerstone.GUI.Filtering {
    interface IFilterEditorPane {
        string DisplayName {
            get;
            set;
        }

        DatabaseManager DBManager {
            get;
            set;
        }

        IDBFilter AttachedFilter {
            get;
            set;
        }
    }
}
