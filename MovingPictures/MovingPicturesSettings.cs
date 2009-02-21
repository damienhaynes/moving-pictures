using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database;
using Cornerstone.Database.Tables;

namespace MediaPortal.Plugins.MovingPictures {
    public class MovingPicturesSettings: SettingsManager {

        public MovingPicturesSettings(DatabaseManager dbManager)
            : base(dbManager) {
        }

        #region Importer Settings

        #region Tweaks

        [CornerstoneSetting(
            Name = "Thread Count",
            Description = "The number of threads retrieving move details for local media. A higher number uses more system resources, but can help with slow data providers. Do not set this value higher than 10 threads.",
            Groups = "|Movie Importer|Tweaks|",
            Identifier = "importer_thread_count",
            Default = 5)]
        public int ThreadCount {
            get { return _threadCount; }
            set {
                _threadCount = value;
                OnSettingChanged("importer_thread_count");
            }
        }
        private int _threadCount;

        [CornerstoneSetting(
            Name = "Regular Expression Noise Filter",
            Description = "A regular expression that removes common used keywords from the folder/filename.",
            Groups = "|Movie Importer|Tweaks|",
            Identifier = "importer_filter",
            Default = "((720p|1080p|1080i|DirCut|DVDRip|DVDScreener|DVDScr|AVCHD|WMV|NTSC|MPEG|DSR|R5|DVDR|DTS|AC3|Bluray|Blu-ray|HDTV|PDTV|HDDVD|XviD|DiVX|x264|dxva)[-]?.*?$)")]
        public string NoiseFilter {
            get { return _noiseFilter; }
            set {
                _noiseFilter = value;
                OnSettingChanged("importer_filter");
            }
        }
        private string _noiseFilter;


        [CornerstoneSetting(
            Name = "Enable Importer While In GUI",
            Description = "Enables the importer while in the GUI",
            Groups = "|Movie Importer|Tweaks|",
            Identifier = "importer_gui_enabled",
            Default = true)]
        public bool EnableImporterInGUI {
            get { return _enableImporterWhileInGUI; }
            set {
                _enableImporterWhileInGUI = value;
                OnSettingChanged("importer_gui_enabled");
            }
        }
        private bool _enableImporterWhileInGUI;

        #endregion

        #region Matching and Importing

        [CornerstoneSetting(
            Name = "Ignore Possible Matches with Incorrect Year",
            Description = "If a year keyword is detected and no exact match is found only show the results from the same year or blank year. (experimental)",
            Groups = "|Movie Importer|Matching and Importing|",
            Identifier = "importer_strict_year",
            Default = false)]
        public bool StrictYear {
            get { return _strictYear; }
            set {
                _strictYear = value;
                OnSettingChanged("importer_strict_year");
            }
        }
        private bool _strictYear;


        [CornerstoneSetting(
            Name = "Title Auto-Approve Threshold",
            Description = "This is the maximum value for the levenshtein distance that is used for triggering auto-approval on close matching titles.",
            Groups = "|Movie Importer|Matching and Importing|",
            Identifier = "importer_autoapprove",
            Default = 1)]
        public int AutoApproveThreshold {
            get { return _autoApproveThreshold; }
            set {
                _autoApproveThreshold = value;
                OnSettingChanged("importer_autoapprove");
            }
        }
        private int _autoApproveThreshold;


        [CornerstoneSetting(
            Name = "Always Group Files In The Same Folder",
            Description = "When enabled this option will ALWAYS group multiple files in one folder together (assuming a multi-part movie).",
            Groups = "|Movie Importer|Matching and Importing|",
            Identifier = "importer_groupfolder",
            Default = false)]
        public bool AlwaysGroupByFolder {
            get { return _alwaysGroupByFolder; }
            set {
                _alwaysGroupByFolder = value;
                OnSettingChanged("importer_groupfolder");
            }
        }
        private bool _alwaysGroupByFolder;


        [CornerstoneSetting(
            Name = "Prefer Folder Name for Movie Matching",
            Description = "If a folder contains just one movie file it will use the folder name for matching. If you are sure that the filenames are more accurate than the folder name disable this setting.",
            Groups = "|Movie Importer|Matching and Importing|",
            Identifier = "importer_prefer_foldername",
            Default = true)]
        public bool PreferFolderName {
            get { return _preferFolderName; }
            set {
                _preferFolderName = value;
                OnSettingChanged("importer_prefer_foldername");
            }
        }
        private bool _preferFolderName;


        [CornerstoneSetting(
            Name = "Automatically Import Inserted DVDs",
            Description = "Enables importation of media from all available optical drives. This can include CDs, DVDs, HD-DVDs, and Bluray disks. This also applies to \"loose video files\" on a data CD/DVD.",
            Groups = "|Movie Importer|Matching and Importing|",
            Identifier = "importer_disc_enabled",
            Default = true)]
        public bool AutomaticallyImportDVDs {
            get { return _automaticallyImportInsertedDVDs; }
            set {
                _automaticallyImportInsertedDVDs = value;
                OnSettingChanged("importer_disc_enabled");
            }
        }
        private bool _automaticallyImportInsertedDVDs;
        #endregion

        #region Preprocessing

        [CornerstoneSetting(
            Name = "Check Disc ID for Optical Media",
            Description = "Calculate Disc ID for DVD media. This enables better matching for DVD discs.",
            Groups = "|Movie Importer|Preprocessing|",
            Identifier = "importer_discid",
            Default = true)]
        public bool UseDiscID {
            get { return _useDiscID; }
            set {
                _useDiscID = value;
                OnSettingChanged("importer_discid");
            }
        }
        private bool _useDiscID;


        [CornerstoneSetting(
            Name = "Enable NFO Scanner",
            Description = "Scan for NFO file and if available parse out the IMDB id.",
            Groups = "|Movie Importer|Preprocessing|",
            Identifier = "importer_nfoscan",
            Default = true)]
        public bool NfoScannerEnabled {
            get { return _nfoScannerEnabled; }
            set {
                _nfoScannerEnabled = value;
                OnSettingChanged("importer_nfoscan");
            }
        }
        private bool _nfoScannerEnabled;


        [CornerstoneSetting(
            Name = "NFO Scanner File Extensions",
            Description = "The extensions that are used when scanning for nfo files. Seperate multiple extensions with , or ;",
            Groups = "|Movie Importer|Preprocessing|",
            Identifier = "importer_nfoext",
            Default = "nfo;txt")]
        public string NfoScannerFileExtensions {
            get { return _fileExtensions; }
            set {
                _fileExtensions = value;
                OnSettingChanged("importer_nfoext");
            }
        }
        private string _fileExtensions;


        [CornerstoneSetting(
            Name = "Auto-Approve on NFO File IMDB Match",
            Description = "If we found a match on IMDB id always auto-approve this match even if the other criteria doesn't match closely enough. ",
            Groups = "|Movie Importer|Preprocessing|",
            Identifier = "importer_autoimdb",
            Default = true)]
        public bool AutoApproveOnIMDBMatch {
            get { return _autoApproveOnImdbMatch; }
            set {
                _autoApproveOnImdbMatch = value;
                OnSettingChanged("importer_autoimdb");
            }
        }
        private bool _autoApproveOnImdbMatch;


        [CornerstoneSetting(
            Name = "Enable IMDB Lookup",
            Description = "Enables pre-search lookup for title and year from imdb.com when an imdbid is available. This generally improves results from data providers that don't support imdb id searches.",
            Groups = "|Movie Importer|Preprocessing|",
            Identifier = "importer_lookup_imdb",
            Default = true)]
        public bool EnableImdbPreSearch {
            get { return _enableIMDBLookup; }
            set {
                _enableIMDBLookup = value;
                OnSettingChanged("importer_lookup_imdb");
            }
        }
        private bool _enableIMDBLookup;


        [CornerstoneSetting(
            Name = "Enable OSDb Hash Lookup",
            Description = "Enables pre-search lookup for title, year and imdbid by using the hash/movie match.",
            Groups = "|Movie Importer|Preprocessing|",
            Identifier = "importer_lookup_hash",
            Default = true)]
        public bool EnableHashLookup {
            get { return _enableHashLookup; }
            set {
                _enableHashLookup = value;
                OnSettingChanged("importer_lookup_hash");
            }
        }
        private bool _enableHashLookup;


        [CornerstoneSetting(
            Name = "OSDb Hash Confirmed Limit",
            Description = "Only accepts the matched movie if it's confirmed X times or more. This is to prevent false positives.",
            Groups = "|Movie Importer|Preprocessing|",
            Identifier = "importer_lookup_hash_seencount",
            Default = 2)]
        public int HashConfirmedLimit {
            get { return _hashConfirmedLimit; }
            set {
                _hashConfirmedLimit = value;
                OnSettingChanged("importer_lookup_hash_seencount");
            }
        }
        private int _hashConfirmedLimit;


        #endregion

        #region SampleFilter

        [CornerstoneSetting(
            Name = "Regular Expression Filter",
            Description = "a regular expression that matches keywords in the filename indicating that the file is possible sample.",
            Groups = "|Movie Importer|Sample Filter|",
            Identifier = "importer_sample_keyword",
            Default = "sample")]
        public string SampleRegExFilter {
            get { return _sampleRegExFilter; }
            set {
                _sampleRegExFilter = value;
                OnSettingChanged("importer_sample_keyword");
            }
        }
        private string _sampleRegExFilter;


        [CornerstoneSetting(
            Name = "Max Filesize (MB)",
            Description = "If the filesize of the potential sample file is below this value it will be skipped.",
            Groups = "|Movie Importer|Sample Filter|",
            Identifier = "importer_sample_maxsize",
            Default = 150)]
        public int MaxSampleFilesize {
            get { return _maxSampleFilesize; }
            set {
                _maxSampleFilesize = value;
                OnSettingChanged("importer_sample_maxsize");
            }
        }
        private int _maxSampleFilesize;

        #endregion

        #region Cover Art

        [CornerstoneSetting(
            Name = "Cover Artwork Folder",
            Description = "The folder in which cover art should be saved to disk.",
            Groups = "|Movie Importer|Cover Art|",
            Identifier = "cover_art_folder",
            Default = "")]
        public string CoverArtFolder {
            get { return _coverArtworkFolder; }
            set {
                _coverArtworkFolder = value;
                OnSettingChanged("cover_art_folder");
            }
        }
        private string _coverArtworkFolder;


        [CornerstoneSetting(
            Name = "Cover Artwork Thumbnails Folder",
            Description = "The folder in which cover art thumbnails should be saved to disk.",
            Groups = "|Movie Importer|Cover Art|",
            Identifier = "cover_thumbs_folder",
            Default = "")]
        public string CoverArtThumbsFolder {
            get { return _coverArtworkThumbnailsFolder; }
            set {
                _coverArtworkThumbnailsFolder = value;
                OnSettingChanged("cover_thumbs_folder");
            }
        }
        private string _coverArtworkThumbnailsFolder;


        [CornerstoneSetting(
            Name = "Redownload Cover Artwork on Rescan",
            Description = "When a full rescan is performed this setting determines if coverart that has already been downloaded will be reretrieved and the local copy updated.",
            Groups = "|Movie Importer|Cover Art|",
            Identifier = "redownload_coverart",
            Default = false)]
        public bool RedownloadCoverArtwork {
            get { return _redownloadCoverArtworkonRescan; }
            set {
                _redownloadCoverArtworkonRescan = value;
                OnSettingChanged("redownload_coverart");
            }
        }
        private bool _redownloadCoverArtworkonRescan;


        [CornerstoneSetting(
            Name = "Max Covers per Movie",
            Description = "When the movie importer automatically downloads cover art, it will not retrieve more than the given number of covers for a movie.",
            Groups = "|Movie Importer|Cover Art|",
            Identifier = "max_covers_per_movie",
            Default = 9)]
        public int MaxCoversPerMovie {
            get { return _maxCoversperMovie; }
            set {
                _maxCoversperMovie = value;
                OnSettingChanged("max_covers_per_movie");
            }
        }
        private int _maxCoversperMovie;


        [CornerstoneSetting(
            Name = "Max Covers per Movie per Session",
            Description = "When the movie importer automatically downloads cover art it will not retrieve more than the given number of covers for a movie in a single update / import session. Next time a full update is done, if there are additional covers to download, it will grab those as well.",
            Groups = "|Movie Importer|Cover Art|",
            Identifier = "max_covers_per_session",
            Default = 3)]
        public int MaxCoversPerSession {
            get { return _maxCoversperMovieperSession; }
            set {
                _maxCoversperMovieperSession = value;
                OnSettingChanged("max_covers_per_session");
            }
        }
        private int _maxCoversperMovieperSession;


        [CornerstoneSetting(
            Name = "Minimum Cover Width",
            Description = "The minimum width in pixels for any given cover. If a cover from any data provider is smaller than this value it will not be downloaded and saved.",
            Groups = "|Movie Importer|Cover Art|",
            Identifier = "min_cover_width",
            Default = 175)]
        public int MinimumCoverWidth {
            get { return _minimumCoverWidth; }
            set {
                _minimumCoverWidth = value;
                OnSettingChanged("min_cover_width");
            }
        }
        private int _minimumCoverWidth;


        [CornerstoneSetting(
            Name = "Minimum Cover Height",
            Description = "The minimum height in pixels for any given cover. If a cover from any data provider is smaller than this value it will not be downloaded and saved.",
            Groups = "|Movie Importer|Cover Art|",
            Identifier = "min_cover_height",
            Default = 260)]
        public int MinimumCoverHeight {
            get { return _minimumCoverHeight; }
            set {
                _minimumCoverHeight = value;
                OnSettingChanged("min_cover_height");
            }
        }
        private int _minimumCoverHeight;


        [CornerstoneSetting(
            Name = "Cover Artwork Filename Pattern",
            Description = "The importer will look in your cover art folder and try to find a file that matches this pattern. If one is found, it will be used as a cover. If none is found, an online data provider will be used to auto download artwork.",
            Groups = "|Movie Importer|Cover Art|",
            Identifier = "local_coverart_pattern",
            Default = "%imdb_id%.jpg|%imdb_id%.png|%imdb_id%.bmp")]
        public string CoverArtworkFilenamePattern {
            get { return _coverArtworkFilenamePattern; }
            set {
                _coverArtworkFilenamePattern = value;
                OnSettingChanged("local_coverart_pattern");
            }
        }
        private string _coverArtworkFilenamePattern;


        [CornerstoneSetting(
            Name = "Search Movie Folder for Cover Art",
            Description = "If set to true the local media data provider will use files matching a specified pattern for cover artwork. This setting should only be used if you have all movies in their own folders.",
            Groups = "|Movie Importer|Cover Art|",
            Identifier = "local_cover_from_movie_folder",
            Default = false)]
        public bool SearchMovieFolderForCoverArt {
            get { return _searchMovieFolderforCoverArt; }
            set {
                _searchMovieFolderforCoverArt = value;
                OnSettingChanged("local_cover_from_movie_folder");
            }
        }
        private bool _searchMovieFolderforCoverArt;


        [CornerstoneSetting(
            Name = "Movie Folder Cover Artwork Filename Pattern",
            Description = "The importer will look in the folder the given movie was found in, and try to find a file that matches this pattern. If one is found, it will be used as a cover. DB field names can be used, surrounded by % symbols. e.g. %imdb_id%.jpg",
            Groups = "|Movie Importer|Cover Art|",
            Identifier = "local_moviefolder_coverart_pattern",
            Default = "folder.jpg|folder.png|folder.bmp")]
        public string MovieFolderCoverArtworkFilenamePattern {
            get { return _movieFolderCoverArtworkFilenamePattern; }
            set {
                _movieFolderCoverArtworkFilenamePattern = value;
                OnSettingChanged("local_moviefolder_coverart_pattern");
            }
        }
        private string _movieFolderCoverArtworkFilenamePattern;

        #endregion

        #region Backdrops

        [CornerstoneSetting(
            Name = "Backdrop Folder",
            Description = "The folder in which movie backdrops should be saved to disk.",
            Groups = "|Movie Importer|Backdrops|",
            Identifier = "backdrop_folder",
            Default = "")]
        public string BackdropFolder {
            get { return _backdropFolder; }
            set {
                _backdropFolder = value;
                OnSettingChanged("backdrop_folder");
            }
        }
        private string _backdropFolder;


        [CornerstoneSetting(
            Name = "Backdrop Thumbnails Folder",
            Description = "The folder in which movie backdrop thumbnails should be saved to disk.",
            Groups = "|Movie Importer|Backdrops|",
            Identifier = "backdrop_thumbs_folder",
            Default = "")]
        public string BackdropThumbsFolder {
            get { return _backdropThumbnailsFolder; }
            set {
                _backdropThumbnailsFolder = value;
                OnSettingChanged("backdrop_thumbs_folder");
            }
        }
        private string _backdropThumbnailsFolder;


        [CornerstoneSetting(
            Name = "Redownload Backdrop on Rescan",
            Description = "When a full rescan is performed this setting determines if backdrops that have already been downloaded will be reretrieved and the local copy updated.",
            Groups = "|Movie Importer|Backdrops|",
            Identifier = "redownload_backdrops",
            Default = false)]
        public bool RedownloadBackdrops {
            get { return _redownloadBackdroponRescan; }
            set {
                _redownloadBackdroponRescan = value;
                OnSettingChanged("redownload_backdrops");
            }
        }
        private bool _redownloadBackdroponRescan;


        [CornerstoneSetting(
            Name = "Minimum Backdrop Width",
            Description = "The minimum width in pixels for any given movie backdrop. If a backdrop from any data provider is smaller than this value it will not automatically be downloaded and saved.",
            Groups = "|Movie Importer|Backdrops|",
            Identifier = "min_backdrop_width",
            Default = 1280)]
        public int MinimumBackdropWidth {
            get { return _minimumBackdropWidth; }
            set {
                _minimumBackdropWidth = value;
                OnSettingChanged("min_backdrop_width");
            }
        }
        private int _minimumBackdropWidth;


        [CornerstoneSetting(
            Name = "Minimum Backdrop Height",
            Description = "The minimum height in pixels for any given movie backdrop. If a cover from any data provider is smaller than this value it will not automatically be downloaded and saved.",
            Groups = "|Movie Importer|Backdrops|",
            Identifier = "min_backdrop_height",
            Default = 720)]
        public int MinimumBackdropHeight {
            get { return _minimumBackdropHeight; }
            set {
                _minimumBackdropHeight = value;
                OnSettingChanged("min_backdrop_height");
            }
        }
        private int _minimumBackdropHeight;


        [CornerstoneSetting(
            Name = "Backdrop Filename Pattern",
            Description = "The importer will look in your backdrop folder and try to find a file that matches this pattern. If one is found, it will be used as a backdrop. If none is found, an online data provider will be used to auto download artwork.",
            Groups = "|Movie Importer|Backdrops|",
            Identifier = "local_backdrop_pattern",
            Default = "%imdb_id%.jpg|%imdb_id%.png|%imdb_id%.bmp")]
        public string BackdropFilenamePattern {
            get { return _backdropFilenamePattern; }
            set {
                _backdropFilenamePattern = value;
                OnSettingChanged("local_backdrop_pattern");
            }
        }
        private string _backdropFilenamePattern;


        [CornerstoneSetting(
            Name = "Search Movie Folder for Backdrops",
            Description = "If set to true the local media data provider will use files matching a specified pattern for backdrops. This setting should only be used if you have all movies in their own folders.",
            Groups = "|Movie Importer|Backdrops|",
            Identifier = "local_backdrop_from_movie_folder",
            Default = false)]
        public bool SearchMovieFolderForBackdrops {
            get { return _searchMovieFolderforBackdrops; }
            set {
                _searchMovieFolderforBackdrops = value;
                OnSettingChanged("local_backdrop_from_movie_folder");
            }
        }
        private bool _searchMovieFolderforBackdrops;


        [CornerstoneSetting(
            Name = "Movie Folder Backdrop Filename Pattern",
            Description = "The importer will look in the folder the given movie was found in, and try to find a file that matches this pattern. If one is found, it will be used as a cover. DB field names can be used, surrounded by % symbols. e.g. %imdb_id%.jpg",
            Groups = "|Movie Importer|Backdrops|",
            Identifier = "local_moviefolder_backdrop_pattern",
            Default = "backdrop.jpg|backdrop.png|backdrop.bmp")]
        public string MovieFolderBackdropFilenamePattern {
            get { return _movieFolderBackdropFilenamePattern; }
            set {
                _movieFolderBackdropFilenamePattern = value;
                OnSettingChanged("local_moviefolder_backdrop_pattern");
            }
        }
        private string _movieFolderBackdropFilenamePattern;


        #endregion

        #region themoviedb.org

        [CornerstoneSetting(
            Name = "Max Timeouts",
            Description = "The maximum number of timeouts received from the server before a thread returns an error condition.",
            Groups = "|Movie Importer|themoviedb.org|",
            Identifier = "tmdb_max_timeouts",
            Default = 10)]
        public int MaxTimeouts {
            get { return _maxTimeouts; }
            set {
                _maxTimeouts = value;
                OnSettingChanged("tmdb_max_timeouts");
            }
        }
        private int _maxTimeouts;


        [CornerstoneSetting(
            Name = "Timeout Length",
            Description = "The base length of time (in milliseconds) for a timeout when connecting to themoviedb.org data service.",
            Groups = "|Movie Importer|themoviedb.org|",
            Identifier = "tmdb_timeout_length",
            Default = 5000)]
        public int TimeoutLength {
            get { return _timeoutLength; }
            set {
                _timeoutLength = value;
                OnSettingChanged("tmdb_timeout_length");
            }
        }
        private int _timeoutLength;


        [CornerstoneSetting(
            Name = "Timeout Increment",
            Description = "The amount of time (in milliseconds) added to the timeout limit  after each timeout failure. A non-zero value will help when the server is experience a large amount of congestion.",
            Groups = "|Movie Importer|themoviedb.org|",
            Identifier = "tmdb_timeout_increment",
            Default = 1000)]
        public int TimeoutIncrement {
            get { return _timeoutIncrement; }
            set {
                _timeoutIncrement = value;
                OnSettingChanged("tmdb_timeout_increment");
            }
        }
        private int _timeoutIncrement;

        #endregion
        
        #endregion

        #region GUI Settings

        #region Interface Options

        [CornerstoneSetting(
            Name = "Default View",
            Description = "The default view used in the MediaPortal GUI when the plug-in is first opened. Valid options are \"list\", \"thumbs\", \"largethumbs\", and \"filmstrip\".",
            Groups = "|MediaPortal GUI|Interface Options|",
            Identifier = "default_view",
            Default = "list")]
        public string DefaultView {
            get { return _defaultView; }
            set {
                _defaultView = value;
                OnSettingChanged("default_view");
            }
        }
        private string _defaultView;


        [CornerstoneSetting(
            Name = "Click Shows Details",
            Description = "Determines behavior when a movie in the movie browser is clicked. If true, the details view appears. If false the movie starts playback.",
            Groups = "|MediaPortal GUI|Interface Options|",
            Identifier = "click_to_details",
            Default = true)]
        public bool ClickShowsDetails {
            get { return _clickShowsDetails; }
            set {
                _clickShowsDetails = value;
                OnSettingChanged("click_to_details");
            }
        }
        private bool _clickShowsDetails;


        [CornerstoneSetting(
            Name = "Max Actors, Genres, etc. to Display",
            Description = "This determines the number of actors, genres, directors, etc to display on the GUI. This applies to all string based list fields.",
            Groups = "|MediaPortal GUI|Interface Options|",
            Identifier = "max_string_list_items",
            Default = 5)]
        public int MaxElementsToDisplay {
            get { return _maxElementsToDisplay; }
            set {
                _maxElementsToDisplay = value;
                OnSettingChanged("max_string_list_items");
            }
        }
        private int _maxElementsToDisplay;


        [CornerstoneSetting(
            Name = "Name for Home Screen",
            Description = "The name that appears on the home screen for the plugin.",
            Groups = "|MediaPortal GUI|Interface Options|",
            Identifier = "home_name",
            Default = "Moving Pictures")]
        public string HomeScreenName {
            get { return _homeScreenName; }
            set {
                _homeScreenName = value;
                OnSettingChanged("home_name");
            }
        }
        private string _homeScreenName;


        [CornerstoneSetting(
            Name = "Default Sort Field",
            Description = "The default sort field used in the MediaPortal GUI when the plug-in is first opened. Valid options are \"title\", \"dateadded\", \"year\", \"certification\", \"language\", \"score\", \"userscore\", \"popularity\", \"runtime\", \"filepath\".",
            Groups = "|MediaPortal GUI|Interface Options|",
            Identifier = "default_sort_field",
            Default = "Title")]
        public string DefaultSortField {
            get { return _defaultSortField; }
            set {
                _defaultSortField = value;
                OnSettingChanged("default_sort_field");
            }
        }
        private string _defaultSortField;


        [CornerstoneSetting(
            Name = "Show Only Unwatched Movies on Startup",
            Description = "When Moving Pictures starts up, if this option is set to true only Unwatched movies will be displayed until the user changes the filtering options.",
            Groups = "|MediaPortal GUI|Interface Options|",
            Identifier = "start_watched_filter_on",
            Default = false)]
        public bool ShowUnwatchedOnStartup {
            get { return _showOnlyUnwatchedMoviesonStartup; }
            set {
                _showOnlyUnwatchedMoviesonStartup = value;
                OnSettingChanged("start_watched_filter_on");
            }
        }
        private bool _showOnlyUnwatchedMoviesonStartup;


        [CornerstoneSetting(
            Name = "Allow user to delete files from the GUI context menu",
            Description = "Enables a delete menu item, which allows you to delete movies from your hard drive.",
            Groups = "|MediaPortal GUI|Interface Options|",
            Identifier = "enable_delete_movie",
            Default = false)]
        public bool AllowDelete {
            get { return _allowDelete; }
            set {
                _allowDelete = value;
                OnSettingChanged("enable_delete_movie");
            }
        }
        private bool _allowDelete;

        #endregion

        #region Tweaks

        [CornerstoneSetting(
            Name = "Artwork Loading Delay",
            Description = "The number of milliseconds that Moving Pictures waits before it loads new artwork (backdrops and covers) when traversing movies in the GUI. Increasing this value can improve performance if you are experiencing slow down with rapid movements in the GUI.",
            Groups = "|MediaPortal GUI|Tweaks|",
            Identifier = "gui_artwork_delay",
            Default = 250)]
        public int ArtworkLoadingDelay {
            get { return _artworkLoadingDelay; }
            set {
                _artworkLoadingDelay = value;
                OnSettingChanged("gui_artwork_delay");
            }
        }
        private int _artworkLoadingDelay;


        [CornerstoneSetting(
            Name = "Use Remote Control Filtering",
            Description = "Enables the Remote Controle Filter, set to false if you want to use the default mediaportal remote control functionality.",
            Groups = "|MediaPortal GUI|Tweaks|",
            Identifier = "enable_rc_filter",
            Default = true)]
        public bool UseRemoteControlFiltering {
            get { return _useRemoteControlFiltering; }
            set {
                _useRemoteControlFiltering = value;
                OnSettingChanged("enable_rc_filter");
            }
        }
        private bool _useRemoteControlFiltering;

        #endregion

        #region Playback Options

        [CornerstoneSetting(
            Name = "GUI Watch Percentage",
            Description = "The percentage of a movie that must be watched before it will be flagged as watched. This also affects whether resume data is stored.",
            Groups = "|MediaPortal GUI|Playback Options|",
            Identifier = "gui_watch_percentage",
            Default = 90)]
        public int MinimumWatchPercentage {
            get { return _minimumWatchPercentage; }
            set {
                _minimumWatchPercentage = value;
                OnSettingChanged("gui_watch_percentage");
            }
        }
        private int _minimumWatchPercentage;


        [CornerstoneSetting(
             Name = "Disk Insertion Behavior",
             Description = "Action to take when a DVD, Bluray, or HDDVD disk is inserted. (\"DETAILS\": Goto the details page for the DVD. \"PLAY\": Start playback immediately. \"NOTHING\": Take no action).",
             Groups = "|MediaPortal GUI|Playback Options|",
             Identifier = "on_disc_loaded",
             Default = "DETAILS")]
        public string DiskInsertionBehavior {
            get { return _diskInsertionBehavior; }
            set {
                _diskInsertionBehavior = value;
                OnSettingChanged("on_disc_loaded");
            }
        }
        private string _diskInsertionBehavior;


        [CornerstoneSetting(
            Name = "Custom Intro Location",
            Description = "Location of a custom intro that will play before each movie.  This should be the full path including the movie. For example: c:\\custom_intro\\into.mpg",
            Groups = "|MediaPortal GUI|Playback Options|",
            Identifier = "custom_intro_location",
            Default = " ")]
        public string CustomIntroLocation {
            get { return _customIntroLocation; }
            set {
                _customIntroLocation = value;
                OnSettingChanged("custom_intro_location");
            }
        }
        private string _customIntroLocation;

        #endregion

        #region Bluray/HD-DVD Playback

        [CornerstoneSetting(
            Name = "Use External Player",
            Description = "Enable playback for Bluray/HD-DVD using an external player, When set to false the main video stream will be played in the internal player (no menu).",
            Groups = "|MediaPortal GUI|Bluray/HD-DVD Playback|",
            Identifier = "playback_hd_external",
            Default = false)]
        public bool UseExternalPlayer {
            get { return _useExternalPlayer; }
            set {
                _useExternalPlayer = value;
                OnSettingChanged("playback_hd_external");
            }
        }
        private bool _useExternalPlayer;


        [CornerstoneSetting(
            Name = "External Player Path",
            Description = "The path to the executable of the external player that you want to use for playing back BR/HD-DVD.",
            Groups = "|MediaPortal GUI|Bluray/HD-DVD Playback|",
            Identifier = "playback_hd_executable",
            Default = "C:\\MyExternalPlayer\\MyExternalPlayer.exe")]
        public string ExternalPlayerExecutable {
            get { return _externalPlayerExecutable; }
            set {
                _externalPlayerExecutable = value;
                OnSettingChanged("playback_hd_executable");
            }
        }
        private string _externalPlayerExecutable;


        [CornerstoneSetting(
            Name = "External Player Arguements",
            Description = "The command-line arguments that should be appended when calling the executable. (the variable %filename% will be replaced with the path to the movie)",
            Groups = "|MediaPortal GUI|Bluray/HD-DVD Playback|",
            Identifier = "playback_hd_arguments",
            Default = "%filename%")]
        public string ExternalPlayerArguements {
            get { return _externalPlayerArguements; }
            set {
                _externalPlayerArguements = value;
                OnSettingChanged("playback_hd_arguments");
            }
        }
        private string _externalPlayerArguements;

        #endregion

        #endregion

        #region Internal Settings

        [CornerstoneSetting(
            Name = "Data Source Manager Enhanced Debug Mode",
            Description = "If set to true, additional logging will be written by the Scriptable Scraping Engine when the entire plug-in is in debug mode. Internal scripts stored in the DLL will also be reloaded on launch regardless of version number.",
            Groups = "|Internal|",
            Identifier = "source_manager_debug",
            Default = false)]
        public bool DataSourceDebugActive {
            get { return _dataSourceManagerEnhancedDebugMode; }
            set {
                _dataSourceManagerEnhancedDebugMode = value;
                OnSettingChanged("source_manager_debug");
            }
        }
        private bool _dataSourceManagerEnhancedDebugMode;


        [CornerstoneSetting(
            Name = "Data Provider Manager Initialized",
            Description = "An internal flag to determine if an initial load of the Data Source Manager has been preformed.",
            Groups = "|Internal|",
            Identifier = "source_manager_init_done",
            Default = "True")]
        public bool DataProvidersInitialized {
            get { return _dataProviderManagerInitialized; }
            set {
                _dataProviderManagerInitialized = value;
                OnSettingChanged("source_manager_init_done");
            }
        }
        private bool _dataProviderManagerInitialized;


        [CornerstoneSetting(
            Name = "Show Advanced Settings Warning",
            Description = "If set to false, the Advanced Settings warning screen will no longer be displayed when first clicking on the Advanced Settings tab.",
            Groups = "|Internal|",
            Identifier = "config_advanced_nag",
            Default = true)]
        public bool ShowAdvancedSettingsWarning {
            get { return _showAdvancedSettingsWarning; }
            set {
                _showAdvancedSettingsWarning = value;
                OnSettingChanged("config_advanced_nag");
            }
        }
        private bool _showAdvancedSettingsWarning;


        [CornerstoneSetting(
            Name = "Version Number",
            Description = "Version number of Moving Pictures. Used for database upgrade purposes, do not change.",
            Groups = "|Internal|",
            Identifier = "version",
            Default = "0.0.0.0")]
        public string Version {
            get { return _versionNumber; }
            set {
                _versionNumber = value;
                OnSettingChanged("version");
            }
        }
        private string _versionNumber;

        #endregion
    }
}
