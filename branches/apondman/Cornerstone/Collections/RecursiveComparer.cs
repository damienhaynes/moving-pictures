using System;
using System.Collections.Generic;

namespace Cornerstone.Collections {
    
    /// <summary>
    /// Specialized IComparer that supports multiple levels of sorting
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RecursiveComparer<T> : IComparer<T> {

        #region Member variables

        protected SortMethod<T> method;
        protected RecursiveComparer<T> recursiveComparer;

        #endregion

        #region Constructors

        public RecursiveComparer(SortMethod<T> method) : this(method, null) {

        }

        public RecursiveComparer(SortMethod<T> method, RecursiveComparer<T> comparer) {
            if (method == null) {
                throw new ArgumentNullException("method");
            }

            this.method = method;
            this.recursiveComparer = comparer;
        }

        #endregion

        #region IComparer<T> Members

        public virtual int Compare(T x, T y) {

            // compare the objects
            int rtn = this.method.Comparer.Compare(x, y);

            // if the results are equal try the nested comparer if available
            if (rtn == 0 && this.recursiveComparer != null)
                return this.recursiveComparer.Compare(x, y);

            // inverse the outcome if the SortDirection is descending
            if (this.method.Direction == SortDirection.Descending)
                rtn = -rtn;

            return rtn;
        }

        #endregion

    }
}
