using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Plugins.MovingPictures.SignatureBuilders {
    public interface ISignatureBuilder {

        MovieSignature UpdateSignature(MovieSignature signature);

    }
}
