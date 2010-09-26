using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Cornerstone.Collections {
    
    /// <summary>
    /// This collection represents an ordered list of comparers and their sorting direction
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SortOperation<T> : KeyedCollection<IComparer<T>, SortMethod<T>> {

        /// <summary>
        /// Adds a new sort method to the operation
        /// </summary>
        /// <param name="sorter">the sorter to use</param>
        public virtual void Add(IComparer<T> comparer) {
            this.Add(comparer, SortDirection.Ascending);
        }

        /// <summary>
        /// Adds a new sort method to the operation
        /// </summary>
        /// <param name="sorter">the sorter to use</param>
        /// <param name="sortDirection">direction to sort</param>
        public virtual void Add(IComparer<T> comparer, SortDirection direction) {
            if (this.Contains(comparer))
                return;

            SortMethod<T> method = new SortMethod<T> {
                Comparer = comparer,
                Direction = direction
            };

            this.Add(method);
        }

        /// <summary>
        /// Returns the recursive comparer as result of the SortMethod collection
        /// </summary>
        /// <returns></returns>
        public virtual RecursiveComparer<T> GetComparer() {
            RecursiveComparer<T> comparer = null;
            
            // we reverse the order of the collection so we build the recursive comparer 
            // from last to first comparer
            IEnumerable<SortMethod<T>> methods = this.Reverse();
            foreach (SortMethod<T> method in methods) {
                comparer = new RecursiveComparer<T>(method, comparer);
            }
            return comparer;
        }

        protected override IComparer<T> GetKeyForItem(SortMethod<T> item) {
            return item.Comparer;
        }
    }
}
