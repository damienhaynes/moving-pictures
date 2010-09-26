using System.Collections.Generic;

namespace Cornerstone.Collections {
    
    /// <summary>
    /// Generic comparer that supports grouping
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGroupableComparer<T> : IComparer<T> {

        /// <summary>
        /// Should return the label/description for the group this item will belong to when this comparer is used
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        string GetGroupLabel(T item);

    }
}
