using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Cornerstone.Collections {

    /// <summary>
    /// ObservableCollection with added sorting
    /// <inheritdoc />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SortableObservableCollection<T> : ObservableCollection<T> {

        /// <inheritdoc />
        public virtual void Sort() {
            this.Sort(0, Count, null);
        }

        /// <inheritdoc />
        public virtual void Sort(IComparer<T> comparer) {
            this.Sort(0, Count, comparer);
        }

        /// <inheritdoc />
        public virtual void Sort(int index, int count, IComparer<T> comparer) {
            (this.Items as List<T>).Sort(index, count, comparer);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

    }
}
