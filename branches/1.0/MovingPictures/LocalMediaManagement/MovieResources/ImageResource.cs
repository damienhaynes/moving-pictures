using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieResources {
    public class ImageResource: FileBasedResource {

        public string ThumbFilename {
            get;
            set;
        }

        protected void GenerateThumbnail() {
            throw new NotImplementedException();
        }
    }
}
