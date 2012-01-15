using System;
using System.Collections.Generic;
using System.Text;

namespace Cornerstone.Database.Tables {
    public interface IFilter<T> where T:DatabaseTable {
        event FilterUpdatedDelegate<T> Updated;
        HashSet<T> Filter(ICollection<T> input);
        HashSet<T> Filter(ICollection<T> input, bool forceActive);
        bool Active { get; }
    }

    public delegate void FilterUpdatedDelegate<T>(IFilter<T> obj) where T:DatabaseTable;
}
