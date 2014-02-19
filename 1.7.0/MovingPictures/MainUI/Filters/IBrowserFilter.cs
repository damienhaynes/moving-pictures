using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;

namespace MediaPortal.Plugins.MovingPictures.MainUI.Filters {
    public interface IBrowserFilter {
        event FilterUpdatedDelegate Updated;
        List<DBMovieInfo> Filter(List<DBMovieInfo> input);
        bool Active { get; }
    }

    public delegate void FilterUpdatedDelegate(IBrowserFilter obj);
}
