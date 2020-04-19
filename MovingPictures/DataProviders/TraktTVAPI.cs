using Cornerstone.Extensions;
using Cornerstone.Tools;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using NLog;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

namespace MediaPortal.Plugins.MovingPictures.DataProviders.TraktTVAPI
{
    public static class TraktAPI
    {
        private static readonly Logger mLogger = LogManager.GetCurrentClassLogger();

        #region API variables

        private const string mClientId = "7fa4a2d61daadccbf6e7c85cf22635c86fb9392b4cf0b6a38d2cd9bf781bd7b9";
        private const string mApiUrl = "https://api.trakt.tv/";

        private static readonly string mApiMovieTitleSearchUrl = string.Concat(mApiUrl, "search/movie?query={0}&page=1&limit=10");
        private static readonly string mApiMovieTitleYearSearchUrl = string.Concat(mApiUrl, "search/movie?query={0}&years={1}&page=1&limit=10");
        private static readonly string mApiMovieGetMovieInfoUrl = string.Concat(mApiUrl, "movies/{0}?extended=full");
        private static readonly string mApiMovieGetCastInfoUrl = string.Concat(mApiUrl, "movies/{0}/people");
        private static readonly string mApiMovieGetAlternativeTitlesUrl = string.Concat(mApiUrl, "movies/{0}/aliases");
        private static readonly string mApiMovieGetGenresUrl = string.Concat(mApiUrl, "genres/movies");
        private static readonly string mApiMovieGetLanguagesUrl = string.Concat(mApiUrl, "languages/movies");

        #endregion

        #region Public Methods

        public static IEnumerable<MovieSearch> SearchMovies(string aTitle, int? aYear = null)
        {
            string lSearchTitle = HttpUtility.UrlEncode(aTitle);
            string lSearchUrl = aYear == null ? string.Format(mApiMovieTitleSearchUrl, lSearchTitle) : string.Format(mApiMovieTitleYearSearchUrl, lSearchTitle, aYear);

            string lResponse = GetJson(lSearchUrl);
            return lResponse.FromJsonArray<MovieSearch>();
        }

        /// <summary>
        /// Lookup movie info by trakt ID or IMDb ID
        /// </summary>
        public static MovieInfo GetMovieInfo(string aMovieId)
        {
            string lResponse = GetJson(string.Format(mApiMovieGetMovieInfoUrl, aMovieId));
            return lResponse.FromJson<MovieInfo>();
        }

        public static CastInfo GetCastInfo(int aMovieId)
        {
            string lResponse = GetJson(string.Format(mApiMovieGetCastInfoUrl, aMovieId));
            return lResponse.FromJson<CastInfo>();
        }

        public static IEnumerable<AlternativeTitle> GetAlternativeTitles(int aMovieId)
        {
            string lResponse = GetJson(string.Format(mApiMovieGetAlternativeTitlesUrl, aMovieId));
            return lResponse.FromJsonArray<AlternativeTitle>();
        }

        public static IEnumerable<Genre> GetGenres()
        {
            string lResponse = GetJson(mApiMovieGetGenresUrl);
            return lResponse.FromJsonArray<Genre>();
        }
        public static IEnumerable<Language> GetLanguages()
        {
            string lResponse = GetJson(mApiMovieGetLanguagesUrl);
            return lResponse.FromJsonArray<Language>();
        }

        #endregion

        #region Private Methods

        private static string GetJson(string url)
        {
            WebGrabber grabber = Utility.GetWebGrabberInstance(url);
            grabber.Encoding = Encoding.UTF8;
            grabber.Accept = "application/json";

            // add required headers for authorisation
            grabber.Request.Headers.Add("trakt-api-version", "2");
            grabber.Request.Headers.Add("trakt-api-key", mClientId);

            if (grabber.GetResponse())
            {
                return grabber.GetString();
            }
            else
                return null;
        }

        #endregion
    }

    #region Data Structures

    [DataContract]
    public class Genre
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "slug")]
        public string Slug { get; set; }
    }

    [DataContract]
    public class Language
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "code")]
        public string Code { get; set; }
    }

    [DataContract]
    public class TraktIds
    {
        [DataMember(Name = "trakt")]
        public int TraktId { get; set; }

        [DataMember(Name = "slug")]
        public string Slug { get; set; }

        [DataMember(Name = "imdb")]
        public string ImdbId { get; set; }

        [DataMember(Name = "tmdb")]
        public int TmdbId { get; set; }
    }

    [DataContract]
    public class TraktMovie
    {
        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "year")]
        public int Year { get; set; }

        [DataMember(Name = "ids")]
        public TraktIds Ids { get; set; }
    }

    [DataContract]
    public class MovieSearch
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "score")]
        public float Score { get; set; }

        [DataMember(Name = "movie")]
        public TraktMovie Movie { get; set; }
    }

    [DataContract]
    public class MovieInfo: TraktMovie
    {
        [DataMember(Name = "tagline")]
        public string Tagline { get; set; }

        [DataMember(Name = "overview")]
        public string Overview { get; set; }

        [DataMember(Name = "released")]
        public string Released { get; set; }

        [DataMember(Name = "runtime")]
        public int Runtime { get; set; }

        [DataMember(Name = "country")]
        public string Country { get; set; }

        [DataMember(Name = "updated_at")]
        public string UpdatedAt { get; set; }

        [DataMember(Name = "trailer")]
        public string Trailer { get; set; }

        [DataMember(Name = "homepage")]
        public string Homepage { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "rating")]
        public decimal Score { get; set; }

        [DataMember(Name = "votes")]
        public int Votes { get; set; }

        [DataMember(Name = "comment_count")]
        public int CommentCount { get; set; }

        [DataMember(Name = "language")]
        public string LanguageCode { get; set; }

        [DataMember(Name = "available_translations")]
        public List<string> AvailableTranslations { get; set; }

        [DataMember(Name = "genres")]
        public List<string> Genres { get; set; }

        [DataMember(Name = "certification")]
        public string Certification { get; set; }
    }

    [DataContract]
    public class AlternativeTitle
    {
        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "country")]
        public string CountryCode { get; set; }
    }

    [DataContract]
    public class TraktPerson
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "ids")]  
        public TraktIds Ids { get; set; }
    }

    [DataContract]
    public class MovieCast
    {
        [DataMember(Name = "characters")]
        public List<string> Characters { get; set; }

        [DataMember(Name = "person")]
        public TraktPerson Person { get; set; }
    }

    [DataContract]
    public class MovieCrew
    {
        [DataMember(Name = "directing")]
        public List<TraktCrew> Directors { get; set; }

        [DataMember(Name = "writing")]
        public List<TraktCrew> Writers { get; set; }
    }

    [DataContract]
    public class TraktCrew
    {
        [DataMember(Name = "jobs")]
        public List<string> Jobs { get; set; }

        [DataMember(Name = "person")]
        public TraktPerson Person { get; set; }
    }

    [DataContract]
    public class CastInfo
    {
        [DataMember(Name = "cast")]
        public List<MovieCast> Cast { get; set; }

        [DataMember(Name = "crew")]
        public MovieCrew Crew { get; set; }

    }

    #endregion
}
