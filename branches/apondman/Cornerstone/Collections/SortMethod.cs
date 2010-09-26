using System.Collections.Generic;

namespace Cornerstone.Collections {

    public class SortMethod<T> {

        /// <summary>
        /// Get/set the IComparer instance that should be used when sorting
        /// </summary>
        public IComparer<T> Comparer { get; set; }

        /// <summary>
        /// Get/set the direction of sorting
        /// </summary>
        public SortDirection Direction { get; set; }
    
    }

    /// <summary>
    /// Enumeration for the sorting direction
    /// </summary>
    public enum SortDirection {
        Ascending,
        Descending
    }
}
