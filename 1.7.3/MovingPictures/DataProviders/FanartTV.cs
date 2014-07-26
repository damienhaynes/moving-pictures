using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MediaPortal.Plugins.MovingPictures.DataProviders.FanartTV {

    public class Artwork {
        public string name { get; set; }
        public string imdb_id { get; set; }
        public string tmdb_id { get; set; }
        public List<Image> moviebackground { get; set; }
        public List<Image> movieposter { get; set; }
        public List<Image> moviebanner { get; set; }
        public List<Image> moviethumb { get; set; }
        public List<Image> hdmovieclearart { get; set; }
        public List<Image> movieart { get; set; }
        public List<Image> hdmovielogo { get; set; }
        public List<Image> movielogo { get; set; }
        public List<DiscImage> moviedisc { get; set; }
    }
    
    public class Image {
        public string id { get; set; }
        public string lang { get; set; }
        public string likes { get; set; }
        public string url { get; set; }
    }

    public class DiscImage : Image {
        public string disc { get; set; }
        public string disc_type { get; set; }
    }

}
