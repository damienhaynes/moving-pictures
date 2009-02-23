using System;
using System.Text.RegularExpressions;
using System.Web;
using Cornerstone.Tools;
using MediaPortal.Plugins.MovingPictures.DataProviders;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.SignatureBuilders {
    class HashBuilder : ISignatureBuilder {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region ISignatureBuilder Members

        public SignatureBuilderResult UpdateSignature(MovieSignature signature) {
            bool hashLookup = MovingPicturesCore.Settings.EnableHashLookup;

            if (!hashLookup || String.IsNullOrEmpty(signature.MovieHash))
                return SignatureBuilderResult.INCONCLUSIVE;

            DBMovieInfo movie = OSDbProvider.GetMovieByHash(signature.MovieHash);
            if (movie != null) {
                signature.Title = movie.Title;
                signature.Year = movie.Year;
                signature.ImdbId = movie.ImdbID;
                logger.Debug("Lookup Hash={0}: Title='{2}', Year={3}, ImdbID={1}", signature.MovieHash, signature.ImdbId, signature.Title, signature.Year);
                return SignatureBuilderResult.CONCLUSIVE;
            }
            else {
                logger.Debug("Lookup Hash={0}: No proper match found.", signature.MovieHash);
                return SignatureBuilderResult.INCONCLUSIVE;
            }

            
        }

        #endregion

    }
}
