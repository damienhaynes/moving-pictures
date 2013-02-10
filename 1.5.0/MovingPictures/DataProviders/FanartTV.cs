using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MediaPortal.Plugins.MovingPictures.DataProviders.FanartTV {    
    
    public class MovieImages {       
        public string imdb_id { get; set; }
        public string tmdb_id { get; set; }
        public List<MovieBackdrop> moviebackground { get; set; }
    }

    public abstract class Image {
        public string id { get; set; }
        public string lang { get; set; }
        public string likes { get; set; }
        public string url { get; set; }
    }

    public class MovieBackdrop : Image { }
}
