using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls {
    interface IDBFieldBackedControl: IDBBackedControl {

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
}
