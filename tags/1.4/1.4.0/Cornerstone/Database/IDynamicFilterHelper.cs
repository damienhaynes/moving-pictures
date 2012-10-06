using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Database.Tables;

namespace Cornerstone.Database {
    public interface  IDynamicFilterHelper {
        Type FilteredObject {
            get;
        }    
    }

    public abstract class DynamicFilterHelper<T>: IDynamicFilterHelper where T: DatabaseTable {

        /// <summary>
        /// The type of object that this IDynamicFilterHelper can handle.
        /// </summary>
        public Type FilteredObject {
            get { return typeof(T); }
        }

        /// <summary>
        /// Adds new required subnodes and removes subnodes that are no longer needed.
        /// </summary>
        /// <param name="node">The node to be updated.</param>
        /// <returns>TRUE if updating was handled.</returns>
        public abstract bool UpdateDynamicNode(DBNode<T> node);
    }
}
