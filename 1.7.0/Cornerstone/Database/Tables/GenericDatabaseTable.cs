using System;
using System.Collections.Generic;
using System.Text;

namespace Cornerstone.Database.Tables {
    public class GenericDatabaseTable<T>: DatabaseTable {
        [DBField(FieldName="generic_type")]
        public Type GenericType {
            get { return typeof(T); }
            set { }
        }
    }
}
