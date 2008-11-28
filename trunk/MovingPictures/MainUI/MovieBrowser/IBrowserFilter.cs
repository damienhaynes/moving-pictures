﻿using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;

namespace MediaPortal.Plugins.MovingPictures.MainUI.MovieBrowser {
    public interface IBrowserFilter {
        event FilterUpdatedDelegate Updated;
        List<DBMovieInfo> Filter(List<DBMovieInfo> input);
        string Text { get; }
        bool Active { get; }
    }

    public delegate void FilterUpdatedDelegate(IBrowserFilter obj);
}
