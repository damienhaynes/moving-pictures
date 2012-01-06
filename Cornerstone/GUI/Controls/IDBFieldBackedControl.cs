using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database;
using System.Windows.Forms;
using Cornerstone.Database.Tables;

namespace Cornerstone.GUI.Controls {
    public interface IDBFieldBackedControl: IDBBackedControl {
        event FieldChangedListener FieldChanged;

        String DatabaseFieldName {
            get;
            set;
        }

        DBField DatabaseField {
            get;
        }

        DBField.DBDataType DBTypeOverride {
            get;
            set;
        } 
    }

    public delegate void FieldChangedListener(DatabaseTable obj, DBField field, object value);
}
