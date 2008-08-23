using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database.CustomTypes;

namespace MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables {
    public interface IAttributeOwner {
        RelationList<DBMovieInfo, DBAttribute> Attributes {
            get;
        }
    }
}
