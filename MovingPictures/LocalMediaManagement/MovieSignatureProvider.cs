using System.Collections.Generic;
using MediaPortal.Plugins.MovingPictures.SignatureBuilders;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {
  /// <summary>
  /// THIS CLASS IS A CANDIDATE FOR REFACTORING
  /// </summary>
  public static class MovieSignatureProvider {
      private static Logger logger = LogManager.GetCurrentClassLogger();

      private static List<ISignatureBuilder> signatureProviders;
      private static object loadingLock = new object();

      public static MovieSignature parseMediaMatch(MovieMatch movieMatch) {
          lock (loadingLock) {
              if (signatureProviders == null) {
                  signatureProviders = new List<ISignatureBuilder>();
                  signatureProviders.Add(new LocalBuilder());
                  signatureProviders.Add(new MetaServicesBuilder());
                  signatureProviders.Add(new NfoBuilder());
                  signatureProviders.Add(new ImdbBuilder());
              }
          }

          MovieSignature movieSignature = new MovieSignature(movieMatch.LocalMedia);
          foreach (ISignatureBuilder provider in signatureProviders) {
              movieSignature = provider.UpdateSignature(movieSignature);
          }

          return movieSignature;
      }

  }
}
