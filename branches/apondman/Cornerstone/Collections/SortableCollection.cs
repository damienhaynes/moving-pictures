using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Cornerstone.Collections {

    /// <summary>
    /// Generic collection with advanced sorting options
    /// </summary>
    /// <typeparam name="T">the type of object this collection holds</typeparam>
    public class SortableCollection<T> : SortableObservableCollection<T> {

        /// <summary>
        /// Collection of sorting methods that will be applied in order when sorting the list
        /// </summary>
        public virtual SortOperation<T> SortBy {
            get {
                if (sortByOperation == null)
                    sortByOperation = new SortOperation<T>();

                return sortByOperation;
            }
        } private SortOperation<T> sortByOperation;

        /// <summary>
        /// Sort the list using the SortBy collection
        /// </summary>
        public override void Sort() {
            this.Sort(0, this.Count);
        }

        /// <summary>
        /// Sort the given range of the list using the sort methods supplied by the SortBy property
        /// </summary>
        /// <param name="index">index to start at</param>
        /// <param name="count">length of the range</param>
        public void Sort(int index, int count) {
            base.Sort(index, count, SortBy.GetComparer());
        }
    
    }

}
