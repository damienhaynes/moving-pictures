using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Generic;
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

            List<DBMovieInfo> movie = TheMovieDbProvider.GetMoviesByHashLookup(signature.MovieHash);
            if (movie.Count == 1) {
                signature.Title = movie[0].Title;
                signature.Year = movie[0].Year;
                signature.ImdbId = movie[0].ImdbID;
                logger.Debug("Lookup Hash={0}: Title='{2}', Year={3}, ImdbID={1}", signature.MovieHash, signature.ImdbId, signature.Title, signature.Year);
                return SignatureBuilderResult.CONCLUSIVE;
            }
            else {
                logger.Debug("Lookup Hash={0}: No exact match found. ({1} results)", signature.MovieHash, movie.Count);
                return SignatureBuilderResult.INCONCLUSIVE;
            }
            
        }

        #endregion

    }
}
