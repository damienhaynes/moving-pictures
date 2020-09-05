using Cornerstone.Tools;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.DataProviders.TraktTVAPI;
using MediaPortal.Plugins.MovingPictures.SignatureBuilders;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MediaPortal.Plugins.MovingPictures.DataProviders
{
    class TraktTVProvider : InternalProvider, IMovieProvider
    {
        private static readonly Logger mLogger = LogManager.GetCurrentClassLogger();
        
        private static List<Genre> mGenres = new List<Genre>();
        private static List<Language> mLanguages = new List<Language>();
        private static ConcurrentDictionary<string, DBMovieInfo> mMovieCache = new ConcurrentDictionary<string, DBMovieInfo>();

        #region IMovieProvider

        public string Name
        {
            get
            {
                return "trakt.tv";
            }
        }

        public string Description
        {
            get
            {
                return "Returns movie details in English from trakt.tv.";
            }
        }

        public string Language
        {
            get { return "English"; }
        }

        public string LanguageCode
        {
            get { return "en"; }
        }

        public bool ProvidesMoviesDetails
        {
            get { return true; }
        }

        public bool ProvidesCoverArt
        {
            get { return false; }
        }

        public bool ProvidesBackdrops
        {
            get { return false; }
        }

        public bool GetBackdrop(DBMovieInfo aMovie)
        {
            // not supported
            return false;
        }

        public bool GetArtwork(DBMovieInfo aMovie)
        {
            // not supported
            return false;
        }

        public List<DBMovieInfo> Get(MovieSignature aMovieSignature)
        {
            var lResults = new List<DBMovieInfo>();
            if (aMovieSignature == null)
                return lResults;

            if (aMovieSignature.ImdbId != null && aMovieSignature.ImdbId.Trim().Length >= 9)
            {
                // get movie information by IMDb ID
                DBMovieInfo lMovie = GetMovieInformation(aMovieSignature.ImdbId.Trim());
                if (lMovie != null)
                {
                    lResults.Add(lMovie);
                    return lResults;
                }
            }

            return Search(aMovieSignature.Title, aMovieSignature.Year);
        }

        public UpdateResults Update(DBMovieInfo aMovie)
        {
            if (aMovie == null)
                return UpdateResults.FAILED;

            string lTraktId = GetTraktId(aMovie, false);
            
            // check if Trakt id is still null, if so request id.
            if (string.IsNullOrEmpty(lTraktId))
                return UpdateResults.FAILED_NEED_ID;

            // Grab the movie using the trakt ID
            // First get from cache, this saves re-requesting information during initial import but will return new data when mp is restarted
            DBMovieInfo lMovie;
            if (!mMovieCache.TryGetValue(lTraktId, out lMovie))
            {
                lMovie = GetMovieInformation(lTraktId);
            }

            if (lMovie != null)
            {
                aMovie.GetSourceMovieInfo(SourceInfo).Identifier = lTraktId;
                aMovie.CopyUpdatableValues(lMovie);
                return UpdateResults.SUCCESS;
            }
            else
            {
                return UpdateResults.FAILED;
            }
        }

        #endregion

        #region Private Methods

        private List<DBMovieInfo> Search(string aTitle, int? aYear = null)
        {
            var lResults = new List<DBMovieInfo>();

            var lSearchResults = TraktAPI.SearchMovies(aTitle, aYear);
            if (lSearchResults == null || lSearchResults.Count() == 0)
                return lResults;

            foreach (var result in lSearchResults)
            {
                var lMovie = GetMovieInformation(result.Movie.Ids.TraktId.ToString());
                if (lMovie != null)
                    lResults.Add(lMovie);
            }

            return lResults;
        }

        private DBMovieInfo GetMovieInformation(string aMovieId)
        {
            // request the movie details by trakt id or imdb id
            var lMovieDetails = TraktAPI.GetMovieInfo(aMovieId);
            if (lMovieDetails == null)
                return null;

            var lMovie = new DBMovieInfo();

            // get the trakt id 
            lMovie.GetSourceMovieInfo(SourceInfo).Identifier = lMovieDetails.Ids.TraktId.ToString();

            // get the title
            lMovie.Title = lMovieDetails.Title;
            
            // get alternative titles
            var lAliases = TraktAPI.GetAlternativeTitles(lMovieDetails.Ids.TraktId);
            if (lAliases != null && lAliases.Count() != 0)
            {
                lMovie.AlternateTitles.AddRange(lAliases.Select(a => a.Title));
            }

            // get language (trakt only returns a single language code)
            if (!string.IsNullOrEmpty(lMovieDetails.LanguageCode))
            {
                lMovie.Language = GetLanguageFromCode(lMovieDetails.LanguageCode);
            }

            // get tagline
            lMovie.Tagline = lMovieDetails.Tagline;

            // get imdb id
            lMovie.ImdbID = lMovieDetails.Ids.ImdbId;

            // get homepage
            lMovie.DetailsURL = lMovieDetails.Homepage;

            // get movie overview
            lMovie.Summary = lMovieDetails.Overview;

            // get movie score (2 decimal places out of 10)
            lMovie.Score = Convert.ToSingle(Math.Round(lMovieDetails.Score, 2));

            // get movie vote count
            lMovie.Popularity = lMovieDetails.Votes;

            // get runtime (mins)
            lMovie.Runtime = lMovieDetails.Runtime;

            // get movie cast
            var lCastInfo = TraktAPI.GetCastInfo(lMovieDetails.Ids.TraktId);
            if (lCastInfo != null)
            {
                // add actors
                if (lCastInfo.Cast != null)
                    lMovie.Actors.AddRange(lCastInfo.Cast.Select(c => c.Person.Name));

                // add directors
                if (lCastInfo.Crew != null && lCastInfo.Crew.Directors != null)
                {
                    lMovie.Directors.AddRange(lCastInfo.Crew.Directors.Select(d => d.Person.Name).Distinct());
                }

                // add writers
                if (lCastInfo.Crew != null && lCastInfo.Crew.Writers != null)
                {
                    lMovie.Writers.AddRange(lCastInfo.Crew.Writers.Select(d => d.Person.Name).Distinct());
                }
            }

            // add genres
            if (lMovieDetails.Genres != null)
            {
                // convert genres to pretty names
                lMovieDetails.Genres.ForEach(g => lMovie.Genres.Add(GetGenreFromSlug(g)));
            }

            // add release date and year
            DateTime lDate;
            if (DateTime.TryParse(lMovieDetails.Released, out lDate))
            {
                lMovie.Year = lDate.Year;
                lMovie.ReleaseDate = lDate;
            }
            else
            {
                lMovie.Year = lMovieDetails.Year;
            }

            // add certification (US MPAA rating)
            lMovie.Certification = lMovieDetails.Certification;

            // update our movie cache
            mMovieCache.TryAdd(lMovieDetails.Ids.TraktId.ToString(), lMovie);

            return lMovie;
        }

        private string GetTraktId(DBMovieInfo aMovie, bool aFuzzyMatch)
        {
            // check for internally stored Trakt ID
            DBSourceMovieInfo lIdObj = aMovie.GetSourceMovieInfo(SourceInfo);
            if (lIdObj != null && lIdObj.Identifier != null)
            {
                return lIdObj.Identifier;
            }

            // if not available, lookup based on IMDb ID
            else if (aMovie.ImdbID != null && aMovie.ImdbID.Trim().Length >= 9)
            {
                string lImdbId = aMovie.ImdbID.Trim();
                var lMovieInfo = TraktAPI.GetMovieInfo(lImdbId);
                if (lMovieInfo != null)
                {
                    return lMovieInfo.Ids.TraktId.ToString();
                }
            }

            // if asked for, do a fuzzy match based on title and year
            else if (aFuzzyMatch)
            {
                // grab possible matches by main title + year
                List<DBMovieInfo> lResults = Search(aMovie.Title, aMovie.Year);
                if (lResults.Count == 0) lResults = Search(aMovie.Title);

                // grab possible matches by alt titles
                foreach (string currAltTitle in aMovie.AlternateTitles)
                {
                    List<DBMovieInfo> lTempResults = Search(currAltTitle, aMovie.Year);
                    if (lTempResults.Count == 0) lTempResults = Search(currAltTitle);

                    lResults.AddRange(lTempResults);
                }

                // pick a possible match if one meets our requirements
                foreach (DBMovieInfo currMatch in lResults)
                {
                    if (CloseEnough(currMatch, aMovie))
                        return currMatch.GetSourceMovieInfo(SourceInfo).Identifier;
                }
            }

            return null;
        }

        private string GetGenreFromSlug(string aGenreSlug)
        {
            if (mGenres == null || mGenres.Count == 0)
            {
                mGenres = TraktAPI.GetGenres()?.ToList();
            }

            return mGenres?.FirstOrDefault(g => g.Slug == aGenreSlug)?.Name;
        }

        private string GetLanguageFromCode(string aLanguageCode)
        {
            if (mLanguages == null || mLanguages.Count == 0)
            {
                mLanguages = TraktAPI.GetLanguages()?.ToList();
            }

            return mLanguages?.FirstOrDefault(l => l.Code == aLanguageCode)?.Name;
        }

        private bool CloseEnough(DBMovieInfo aMovieA, DBMovieInfo aMovieB)
        {
            if (CloseEnough(aMovieA.Title, aMovieB)) return true;

            foreach (string currAltTitle in aMovieA.AlternateTitles)
                if (CloseEnough(currAltTitle, aMovieB)) return true;

            return false;
        }

        private bool CloseEnough(string aTitle, DBMovieInfo aMovie)
        {
            int distance;

            distance = AdvancedStringComparer.Levenshtein(aTitle, aMovie.Title);
            if (distance <= MovingPicturesCore.Settings.AutoApproveThreshold)
                return true;

            foreach (string currAltTitle in aMovie.AlternateTitles)
            {
                distance = AdvancedStringComparer.Levenshtein(aTitle, currAltTitle);
                if (distance <= MovingPicturesCore.Settings.AutoApproveThreshold)
                    return true;
            }

            return false;
        }

        #endregion
    }
}
