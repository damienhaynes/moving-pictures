using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database.Tables;
using Cornerstone.Database.CustomTypes;

namespace Cornerstone.Database {
    public interface IAttributeOwner {
        RelationList<DatabaseTable, DBAttribute> Attributes {
            get;
        }
    }
}
