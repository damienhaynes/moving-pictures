using System.Collections.Generic;
using MediaPortal.Plugins.MovingPictures.SignatureBuilders;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {
  /// <summary>
  /// THIS CLASS IS A CANDIDATE FOR REFACTORING
  /// </summary>
  public static class LocalMediaParser
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();

    private static List<ISignatureBuilder> signatureProviders;
    
    public static MovieSignature parseMediaMatch(MovieMatch movieMatch) {
        MovieSignature movieSignature = new MovieSignature(movieMatch.LocalMedia);
        if (signatureProviders == null) {
            signatureProviders = new List<ISignatureBuilder>();
            signatureProviders.Add(new LocalBuilder());
            signatureProviders.Add(new MetaServicesBuilder());
            signatureProviders.Add(new NfoBuilder());
        }

        foreach (ISignatureBuilder provider in signatureProviders) {
            movieSignature = provider.UpdateSignature(movieSignature);
        }

        return movieSignature;
    }    

  }
}
