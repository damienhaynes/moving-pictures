using System.Collections.Generic;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.SignatureBuilders;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    abstract class InternalProvider {

        public virtual DBSourceInfo SourceInfo {
            get {
                if (_sourceInfo == null)
                    _sourceInfo = DBSourceInfo.GetFromProviderObject( (IMovieProvider) this);

                return _sourceInfo;
            }
        } private DBSourceInfo _sourceInfo;        

        public virtual string Author { 
            get { 
                return "Moving Pictures Team";
            }
        }

        public virtual string Version {
            get {
                return "Internal";
            }
        }

    }
}
