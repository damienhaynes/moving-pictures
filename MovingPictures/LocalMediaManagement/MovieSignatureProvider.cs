using System.Collections.Generic;
using MediaPortal.Plugins.MovingPictures.SignatureBuilders;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {
  /// <summary>
  /// THIS CLASS IS A CANDIDATE FOR REFACTORING
  /// </summary>
  public static class MovieSignatureProvider {
      private static Logger logger = LogManager.GetCurrentClassLogger();

      private static List<ISignatureBuilder> signatureBuilders;
      private static object loadingLock = new object();

      public static MovieSignature parseMediaMatch(MovieMatch movieMatch) {
          lock (loadingLock) {
              if (signatureBuilders == null) {
                  signatureBuilders = new List<ISignatureBuilder>();
                  signatureBuilders.Add(new HashBuilder());
                  signatureBuilders.Add(new LocalBuilder());
                  signatureBuilders.Add(new MetaServicesBuilder());
                  signatureBuilders.Add(new NfoBuilder());
                  signatureBuilders.Add(new ImdbBuilder());
              }
          }

          MovieSignature movieSignature = new MovieSignature(movieMatch.LocalMedia);
          foreach (ISignatureBuilder builder in signatureBuilders) {
              SignatureBuilderResult result = builder.UpdateSignature(movieSignature);
              // if a builder returns CONCLUSIVE it has updated the signature with
              // what is believed to be accurate data and we can exit the loop
              // Currently only the Hash and Imdb builder can return this status
              if (result == SignatureBuilderResult.CONCLUSIVE)
                  break;
          }

          return movieSignature;
      }

  }
}
