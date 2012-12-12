using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Plugins.MovingPictures.SignatureBuilders {

    public enum SignatureBuilderResult {
        INCONCLUSIVE,
        CONCLUSIVE        
    }
    
    public interface ISignatureBuilder {

        SignatureBuilderResult UpdateSignature(MovieSignature signature);

    }
}
