using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Plugins.MovingPictures.Database;

namespace MediaPortal.Plugins.MovingPictures.SignatureBuilders {
    public interface ISignatureBuilder {

        MovieSignature UpdateSignature(MovieSignature signature);

    }
}
