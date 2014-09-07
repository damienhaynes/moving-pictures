using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Web;
using Cornerstone.Extensions;
using Cornerstone.Tools;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;

namespace MediaPortal.Plugins.MovingPictures.DataProviders.TMDbAPI {
    public static class TheMovieDbAPI {

        // NOTE: To other developers creating other applications, using this code as a base
        //       or as a reference. PLEASE get your own API key. Do not reuse the one listed here
        //       it is intended for Moving Pictures use ONLY. API keys are free and easy to apply
        //       for. Visit this url: http://help.themoviedb.org/kb/general/how-do-i-register-for-an-api-key

        #region API variables

        private const string apiKey = "cc25933c4094ca50635f94574491f320";
        private const string apiMovieUrl = "http://api.themoviedb.org/3/";

        private static string apiConfig = string.Concat(apiMovieUrl, "configuration?api_key=", apiKey);
        private static string apiMovieSearch = string.Concat(apiMovieUrl, "search/movie?api_key=", apiKey, "&query={0}&page={1}&language={2}&include_adult={3}&year={4}");
        private static string apiMovieGetMovieInfo = string.Concat(apiMovieUrl, "movie/{0}?api_key=", apiKey, "&language={1}");
        private static string apiMovieGetMovieImages = string.Concat(apiMovieUrl, "movie/{0}/images?api_key=", apiKey);
        private static string apiMovieGetReleaseInfo = string.Concat(apiMovieUrl, "movie/{0}/releases?api_key=", apiKey);
        private static string apiMovieGetCastInfo = string.Concat(apiMovieUrl, "movie/{0}/casts?api_key=", apiKey);
        private static string apiMovieGetAlternativeTitles = string.Concat(apiMovieUrl, "movie/{0}/alternative_titles?api_key=", apiKey);
        private static string apiMovieGetPlotKeywords = string.Concat(apiMovieUrl, "movie/{0}/keywords?api_key=", apiKey);

        #endregion

        #region Public Methods

        public static Configuration GetConfiguration() {
            string response = getJson(apiConfig);
            return response.FromJson<Configuration>();
        }

        public static MovieSearch SearchMovies(string searchStr, int page = 1, string language = "en", bool includeAdult = false, string year = null) {
            string response = getJson(string.Format(apiMovieSearch, HttpUtility.UrlEncode(searchStr), page.ToString(), language, includeAdult.ToString(), year ?? string.Empty));
            return response.FromJson<MovieSearch>();
        }

        public static MovieInfo GetMovieInfo(string movieId, string language = "en") {
            string response = getJson(string.Format(apiMovieGetMovieInfo, movieId, language));
            return response.FromJson<MovieInfo>();
        }

        public static MovieImages GetMovieImages(string movieId) {
            string response = getJson(string.Format(apiMovieGetMovieImages, movieId));
            return response.FromJson<MovieImages>();
        }

        public static ReleaseInfo GetReleaseInfo(string movieId) {
            string response = getJson(string.Format(apiMovieGetReleaseInfo, movieId));
            return response.FromJson<ReleaseInfo>();
        }

        public static CastInfo GetCastInfo(string movieId) {
            string response = getJson(string.Format(apiMovieGetCastInfo, movieId));
            return response.FromJson<CastInfo>();
        }

        public static AlternativeTitle GetAlternativeTitles(string movieId) {
            string response = getJson(string.Format(apiMovieGetAlternativeTitles, movieId));
            return response.FromJson<AlternativeTitle>();
        }

        public static PlotKeywords GetPlotKeywords(string movieId)
        {
            string response = getJson(string.Format(apiMovieGetPlotKeywords, movieId));
            return response.FromJson<PlotKeywords>();
        }

        #endregion

        #region Private Methods

        private static string getJson(string url) {

            WebGrabber grabber = Utility.GetWebGrabberInstance(url);
            grabber.Encoding = Encoding.UTF8;
            grabber.Accept = "application/json";
            
            if (grabber.GetResponse())
                return grabber.GetString();
            else
                return null;
        }

        #endregion
    }
    
    #region Data Structures

    [DataContract]
    public class Configuration {
        [DataMember(Name = "images")]
        public ImageConfiguration Images { get; set; }

        [DataContract]
        public class ImageConfiguration {
            [DataMember(Name = "backdrop_sizes")]
            public List<string> BackdropSizes { get; set; }

            [DataMember(Name = "base_url")]
            public string BaseUrl { get; set; }

            [DataMember(Name = "profile_sizes")]
            public List<string> ProfileSizes { get; set; }
        }
    }

    [DataContract]
    public class MovieSearch {
        [DataMember(Name = "page")]
        public int Page { get; set; }

        [DataMember(Name = "total_pages")]
        public int TotalPages { get; set; }

        [DataMember(Name = "total_results")]
        public int TotalResults{ get; set; }

        [DataMember(Name = "results")]
        public List<SearchResult> Results { get; set; }

        [DataContract]
        public class SearchResult {
            [DataMember(Name = "backdrop_path")]
            public string BackdropPath { get; set; }

            [DataMember(Name = "id")]
            public int Id { get; set; }

            [DataMember(Name = "original_title")]
            public string OriginalTitle { get; set; }

            [DataMember(Name = "popularity")]
            public decimal Popularity { get; set; }

            [DataMember(Name = "poster_path")]
            public string PosterPath { get; set; }

            [DataMember(Name = "release_date")]
            public string ReleaseDate { get; set; }

            [DataMember(Name = "title")]
            public string Title { get; set; }
        }
    }

    [DataContract]
    public class MovieInfo {
        [DataMember(Name = "adult")]
        public bool Adult { get; set; }

        [DataMember(Name = "backdrop_path")]
        public string BackdropPath { get; set; }

        [DataMember(Name = "belongs_to_collection")]
        public MovieCollection BelongsToCollection { get; set; }

        [DataContract]
        public class MovieCollection {
            [DataMember(Name = "backdrop_path")]
            public string BackdropPath { get; set; }

            [DataMember(Name = "id")]
            public int Id { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "poster_path")]
            public string PosterPath { get; set; }
        }

        [DataMember(Name = "genres")]
        public List<Genre> Genres { get; set; }

        [DataContract]
        public class Genre {
            [DataMember(Name = "id")]
            public int Id { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }
        }

        [DataMember(Name = "homepage")]
        public string HomePage { get; set; }

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "imdb_id")]
        public string ImdbId { get; set; }

        [DataMember(Name = "original_title")]
        public string OriginalTitle { get; set; }

        [DataMember(Name = "overview")]
        public string Overview { get; set; }

        [DataMember(Name = "popularity")]
        public decimal Popularity { get; set; }

        [DataMember(Name = "poster_path")]
        public string PosterPath { get; set; }

        [DataMember(Name = "production_companies")]
        public List<Company> ProductionCompanies { get; set; }

        [DataContract]
        public class Company {
            [DataMember(Name = "id")]
            public int Id { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }
        }

        [DataMember(Name = "production_countries")]
        public List<Country> ProductionCountries { get; set; }

        [DataContract]
        public class Country {
            [DataMember(Name = "iso_3166_1")]
            public string CountryCode { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }
        }

        [DataMember(Name = "release_date")]
        public string ReleaseDate { get; set; }

        [DataMember(Name = "revenue")]
        public long Revenue { get; set; }

        [DataMember(Name = "runtime")]
        public int? Runtime { get; set; }

        [DataMember(Name = "spoken_languages")]
        public List<Language> SpokenLanguages { get; set; }

        [DataContract]
        public class Language {
            [DataMember(Name = "iso_639_1")]
            public string LanguageCode { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }
        }

        [DataMember(Name = "tagline")]
        public string Tagline { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "vote_average")]
        public float Score { get; set; }

        [DataMember(Name = "vote_count")]
        public int Votes { get; set; }
    }

    [DataContract]
    public class AlternativeTitle {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "titles")]
        public List<AltTitle> Titles { get; set; }

        [DataContract]
        public class AltTitle {
            [DataMember(Name = "iso_3166_1")]
            public string CountryCode { get; set; }

            [DataMember(Name = "title")]
            public string Title { get; set; }
        }
    }

    [DataContract]
    public class CastInfo {
        [DataMember(Name = "cast")]
        public List<MovieCast> Cast { get; set; }

        [DataContract]
        public class MovieCast {
            [DataMember(Name = "character")]
            public string Character { get; set; }

            [DataMember(Name = "id")]
            public int Id { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "order")]
            public int Order { get; set; }

            [DataMember(Name = "profile_path")]
            public string ProfilePath { get; set; }
        }

        [DataMember(Name = "crew")]
        public List<MovieCrew> Crew { get; set; }

        [DataContract]
        public class MovieCrew {
            [DataMember(Name = "department")]
            public string Department { get; set; }

            [DataMember(Name = "id")]
            public int Id { get; set; }

            [DataMember(Name = "job")]
            public string Job { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "profile_path")]
            public string ProfilePath { get; set; }
        }

        [DataMember(Name = "id")]
        public int Id { get; set; }
    }

    [DataContract]
    public class MovieImages {
        [DataMember(Name = "backdrops")]
        public List<MovieImage> Backdrops { get; set; }

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "posters")]
        public List<MovieImage> Posters { get; set; }

        [DataContract]
        public class MovieImage {
            [DataMember(Name = "aspect_ratio")]
            public double AspectRatio { get; set; }

            [DataMember(Name = "file_path")]
            public string FilePath { get; set; }

            [DataMember(Name = "height")]
            public int Height { get; set; }

            [DataMember(Name = "iso_639_1")]
            public string LanguageCode { get; set; }

            [DataMember(Name = "vote_average")]
            public decimal Score { get; set; }

            [DataMember(Name = "vote_count")]
            public int Votes { get; set; }

            [DataMember(Name = "width")]
            public int Width { get; set; }
        }
    }

    [DataContract]
    public class ReleaseInfo {
        [DataMember(Name = "countries")]
        public List<Country> Countries { get; set; }

        [DataContract]
        public class Country {
            [DataMember(Name = "certification")]
            public string Certification { get; set; }

            [DataMember(Name = "iso_3166_1")]
            public string CountryCode { get; set; }

            [DataMember(Name = "release_date")]
            public string ReleaseDate { get; set; }
        }

        [DataMember(Name = "id")]
        public int Id { get; set; }
    }

    [DataContract]
    public class PlotKeywords
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "keywords")]
        public List<Keyword> Keywords { get; set; }

        [DataContract]
        public class Keyword
        {
            [DataMember(Name = "id")]
            public int Id { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }
        }
    }

    #endregion
}
