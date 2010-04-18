using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovingPicturesSocialAPI {
    public class MovieDTO {
        /// <summary>
        /// The name of the third party site used to gather data (ex: imdb)
        /// </summary>
        public string SourceName { get; set; }
        /// <summary>
        /// The id of the movie at the third party site (ex: imdbid)
        /// </summary>
        public string SourceId { get; set; }
        public string Title { get; set; }
        public string Year { get; set; }
        public string Certification { get; set; }
        public string Language { get; set; }
        public string Tagline { get; set; }
        public string Summary { get; set; }
        public string Score { get; set; }
        public string Popularity { get; set; }
        public string Runtime { get; set; }
        /// <summary>
        /// Pipe delimited list of genres
        /// </summary>
        public string Genres { get; set; }
        /// <summary>
        /// Pipe delimited list of directors
        /// </summary>
        public string Directors { get; set; }
        /// <summary>
        /// Pipe delimited list of actors
        /// </summary>
        public string Cast { get; set; }
        public string TranslatedTitle { get; set; }
        public string Locale { get; set; }
    }
}
