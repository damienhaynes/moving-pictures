using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.Database;
using Cornerstone.Tools.Translate;
using NLog;

namespace MediaPortal.Plugins.MovingPictures {
    public class MovingPicturesSettings: SettingsManager {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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
            Default = @"(([\(\{\[]|\b)((576|720|1080)[pi]|dir(ectors )?cut|dvd([r59]|rip|scr(eener)?)|(avc)?hd|wmv|ntsc|pal|mpeg|dsr|r[1-5]|bd[59]|dts|ac3|blu(-)?ray|[hp]dtv|stv|hddvd|xvid|divx|x264|dxva|(?-i)FEST[Ii]VAL|L[iI]M[iI]TED|[WF]S|PROPER|REPACK|RER[Ii]P|REAL|RETA[Ii]L|EXTENDED|REMASTERED|UNRATED|CHRONO|THEATR[Ii]CAL|DC|SE|UNCUT|[Ii]NTERNAL|[DS]UBBED)([\]\)\}]|\b)(-[^\s]+$)?)")]
        public string NoiseFilter {
            get { return _noiseFilter; }
            set {
                _noiseFilter = value;
                OnSettingChanged("importer_filter");
            }
        }
        private string _noiseFilter;

        [CornerstoneSetting(
            Name = "Default User Agent",
            Description = "Default user agent Moving-Pictures uses for its web requests.",
            Groups = "|Movie Importer|Tweaks|",
            Identifier = "useragent",
            Default = @"Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0; SLCC2)")]
        public string UserAgent {
            get { return _useragent; }
            set {
                _useragent = value;
                OnSettingChanged("useragent");
            }
        }
        private string _useragent;

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

        [CornerstoneSetting(
            Name = "JPG Compression Quality",
            Description = "Determines the quality that will be used for JPG compression of covers and backdrops. Value should be between 1 and 100.",
            Groups = "|Movie Importer|Tweaks|",
            Identifier = "jpg_compress_quality",
            Default = 90)]
        public int JpgCompressionQuality {
            get { return _jpgCompressionQuality; }
            set {
                _jpgCompressionQuality = value;
                OnSettingChanged("jpg_compress_quality");
            }
        }
        private int _jpgCompressionQuality;

        #endregion

        #region Matching and Importing

        [CornerstoneSetting(
            Name = "Title Auto-Approve Threshold",
            Description = "This is the maximum value for the levenshtein distance that is used for triggering auto-approval on close matching titles.",
            Groups = "|Movie Importer|Matching and Importing|",
            Identifier = "importer_autoapprove",
            Default = 1,
            Hidden = true)]
        public int AutoApproveThreshold {
            get { return _autoApproveThreshold; }
            set {
                _autoApproveThreshold = value;
                OnSettingChanged("importer_autoapprove");
            }
        }
        private int _autoApproveThreshold;

        [CornerstoneSetting(
            Name = "Year Auto-Approve Distance",
            Description = "This is the maximum of years the release date may be differ before triggering auto-approval on close matches.",
            Groups = "|Movie Importer|Matching and Importing|",
            Identifier = "importer_autoapprove_year",
            Default = 1)]
        public int AutoApproveYearDifference {
            get { return _autoApproveYearDifference; }
            set {
                _autoApproveYearDifference = value;
                OnSettingChanged("importer_autoapprove_year");
            }
        }
        private int _autoApproveYearDifference;

        [CornerstoneSetting(
            Name = "Auto-approve on alternate titles",
            Description = "When enabled this option will auto-approve matches using alternate titles.",
            Groups = "|Movie Importer|Matching and Importing|",
            Identifier = "importer_autoapprove_alternate_titles",
            Default = true,
            Hidden = true)]
        public bool AutoApproveOnAlternateTitle {
            get { return _autoApproveOnAlternateTitle; }
            set {
                _autoApproveOnAlternateTitle = value;
                OnSettingChanged("importer_autoapprove_alternate_titles");
            }
        }
        private bool _autoApproveOnAlternateTitle;

        [CornerstoneSetting(
            Name = "Only Auto Approve from Primary Data Source",
            Description = "When enabled this option will auto-approve matches only from the primary data source. Other matches will be available, but not auto approved. This will be overridden if the Auto Approval Rate is set to 'High' or 'Reckless!'",
            Groups = "|Movie Importer|Matching and Importing|",
            Identifier = "importer_autoapprove_primary_source",
            Default = true,
            Hidden = false)]
        public bool AutoApproveOnlyPrimarySource {
            get { 
                if (AutoApproveThreshold > 1)
                    return false;
                
                return _autoApproveOnlyPrimarySource; 
            }
            set {
                _autoApproveOnlyPrimarySource = value;
                OnSettingChanged("importer_autoapprove_primary_source");
            }
        }
        private bool _autoApproveOnlyPrimarySource;

        [CornerstoneSetting(
            Name = "Always Group Files In The Same Folder",
            Description = "When enabled this option will ALWAYS group multiple files in one folder together (assuming a multi-part movie).",
            Groups = "|Movie Importer|Matching and Importing|",
            Identifier = "importer_groupfolder",
            Default = false,
            Hidden = true)]
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
            Default = true,
            Hidden = true)]
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
            Default = true,
            Hidden = true)]
        public bool AutomaticallyImportDVDs {
            get { return _automaticallyImportInsertedDVDs; }
            set {
                _automaticallyImportInsertedDVDs = value;
                OnSettingChanged("importer_disc_enabled");
            }
        }
        private bool _automaticallyImportInsertedDVDs;

        [CornerstoneSetting(
            Name = "Ignore Interactive Content on Video Disc",
            Description = "When this option is enabled the importer will ignore the so-called interactive folders on video disc which might contain video material to be played on your PC.",
            Groups = "|Movie Importer|Matching and Importing|",
            Identifier = "importer_disc_ignore_interactive_content",
            Default = true)]
        public bool IgnoreInteractiveContentOnVideoDisc {
            get { return _ignoreInteractiveContentOnVideoDisc; }
            set {
                _ignoreInteractiveContentOnVideoDisc = value;
                OnSettingChanged("importer_disc_ignore_interactive_content");
            }
        }
        private bool _ignoreInteractiveContentOnVideoDisc;

        [CornerstoneSetting(
            Name = "Minimum Possible Match Threshold",
            Description = "The minimum number of possible matches that must be found before Moving Pictures will stop searching via additional data providers. Enter 0 to retrieve search results from all active data providers.",
            Groups = "|Movie Importer|Matching and Importing|",
            Identifier = "importer_dataprovider_movie_limit",
            Default = 3)]
        public int MinimumMatches {
            get { return _minimumMatches; }
            set {
                _minimumMatches = value;
                OnSettingChanged("importer_dataprovider_movie_limit");
            }
        }
        private int _minimumMatches;

        [CornerstoneSetting(
            Name = "Data Provider Request Limit",
            Description = "The maximum number of data providers to use when updating missing movie details. Enter 0 to use all active data providers.",
            Groups = "|Movie Importer|Matching and Importing|",
            Identifier = "importer_dataprovider_request_limit",
            Default = 0)]
        public int DataProviderRequestLimit {
            get { return _dataProviderRequestLimit; }
            set {
                _dataProviderRequestLimit = value;
                OnSettingChanged("importer_dataprovider_request_limit");
            }
        }
        private int _dataProviderRequestLimit;

        [CornerstoneSetting(
            Name = "Automatically Aquire MediaInfo Details From Movies",
            Description = "If set to true, Moving Pictures will automatically scan files for various statistics including video file resolution and audio settings. If this option is turned off, this information will not be available to the skin unless manually retrieved by the user. This can improve the speed of the import process.",
            Groups = "|Movie Importer|Matching and Importing|",
            Identifier = "importer_use_mediainfo",
            Default = true)]
        public bool AutoRetrieveMediaInfo {
            get { return _useMediaInfo; }
            set {
                _useMediaInfo = value;
                OnSettingChanged("importer_use_mediainfo");
            }
        }
        private bool _useMediaInfo;

        [CornerstoneSetting(
            Name = "\"Date Added\" Population Method",
            Description = "Determines which date to use for the 'Date Added' field when importing and adding a movie to the database. To update values for existing movies you must refresh your Date Added values from the Movie Manager.    Options are: created, modified, or current.",
            Groups = "|Movie Importer|Matching and Importing|",
            Identifier = "date_import_option",
            Default = "created")]
        public string DateImportOption {
            get { return _dateImportOption; }
            set {
                _dateImportOption = value;
                OnSettingChanged("date_import_option");
            }
        }
        private string _dateImportOption;

        [CornerstoneSetting(
            Name = "Automatically Rescan Network Paths",
            Description = "If set to true Moving Pictures will automatically rescan network import paths at the interval specified.",
            Groups = "|Movie Importer|Matching and Importing|",
            Identifier = "importer_rescan_enabled",
            Default = false,
            Hidden = true)]
        public bool RescanNetworkPaths {
            get { return _rescanNetworkPaths; }
            set {
                _rescanNetworkPaths = value;
                OnSettingChanged("importer_rescan_enabled");
            }
        }
        private bool _rescanNetworkPaths;

        [CornerstoneSetting(
            Name = "Rescan Interval",
            Description = "If the feature is enabled Moving Pictures will automatically rescan network shares at this interval (specified in minutes). This includes shares mounted as a local drive. Setting this option can be useful if the importer fails to pick up changes on network shares. Keep in mind that this will effectively keep the actual hard drives behind the share from spinning down.",
            Groups = "|Movie Importer|Matching and Importing|",
            Identifier = "importer_rescan_interval",
            Default = 90,
            Hidden = true)]
        public int RescanNetworkPathsInterval
        {
            get { return _rescanNetworkPathsInterval; }
            set
            {
                _rescanNetworkPathsInterval = value;
                OnSettingChanged("importer_rescan_interval");
            }
        }
        private int _rescanNetworkPathsInterval;

        [CornerstoneSetting(
            Name = "Delay Import of Write-Locked Files",
            Description = "If a file is locked for writing this generally means it is in the process of being copied to the watch folder. If this setting is true, the import will wait until the lock is released.",
            Groups = "|Movie Importer|Tweaks|",
            Identifier = "importer_delay_locked",
            Default = false)]
        public bool DelayLockedFiles {
            get { return _delayLockedFiles; }
            set {
                _delayLockedFiles = value;
                OnSettingChanged("importer_delay_locked");
            }
        }
        private bool _delayLockedFiles;

        #endregion

        #region Preprocessing

        [CornerstoneSetting(
            Name = "Enable NFO Scanner",
            Description = "Scan for NFO file and if available parse out the IMDB id.",
            Groups = "|Movie Importer|Preprocessing|",
            Identifier = "importer_nfoscan",
            Default = true,
            Hidden = true)]
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
            Default = "nfo;txt",
            Hidden = true)]
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
            Default = true,
            Hidden = true)]
        public bool AutoApproveOnIMDBMatch {
            get { return _autoApproveOnImdbMatch; }
            set {
                _autoApproveOnImdbMatch = value;
                OnSettingChanged("importer_autoimdb");
            }
        }
        private bool _autoApproveOnImdbMatch;

        [CornerstoneSetting(
            Name = "Enable DiscID Lookup for DVDs",
            Description = "Enables pre-search lookup for title by using the unique disc id of the DVD.",
            Groups = "|Movie Importer|Preprocessing|",
            Identifier = "importer_lookup_discid",
            Default = true)]
        public bool EnableDiscIdLookup {
            get { return _enableDiscIdLookup; }
            set {
                _enableDiscIdLookup = value;
                OnSettingChanged("importer_lookup_discid");
            }
        }
        private bool _enableDiscIdLookup;

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
            Name = "Enable TheMovieDb.org Hash Lookup",
            Description = "Enables pre-search lookup for title, year and imdbid by using the hash/movie match.",
            Groups = "|Movie Importer|Preprocessing|",
            Identifier = "importer_lookup_hash",
            Default = false)]
        public bool EnableHashLookup {
            get { return _enableHashLookup; }
            set {
                _enableHashLookup = value;
                OnSettingChanged("importer_lookup_hash");
            }
        }
        private bool _enableHashLookup;

        #endregion

        #region SampleFilter

        [CornerstoneSetting(
            Name = "Regular Expression Filter",
            Description = "a regular expression that matches keywords in the filename or it's parent folder indicating that the file is possible sample.",
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
            Name = "Include Parent Foldername When Matching",
            Description = "Include the parent foldername when checking for sample keywords.",
            Groups = "|Movie Importer|Sample Filter|",
            Identifier = "importer_sample_include_foldername",
            Default = false)]
        public bool SampleIncludeFolderName {
            get { return _sampleIncludeFolderName; }
            set {
                _sampleIncludeFolderName = value;
                OnSettingChanged("importer_sample_include_foldername");
            }
        }
        private bool _sampleIncludeFolderName;

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
            Default = 9,
            Hidden = true)]
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
            Default = false,
            Hidden = true)]
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
            Default = "folder.jpg|folder.png|folder.bmp",
            Hidden = true)]
        public string MovieFolderCoverArtworkFilenamePattern {
            get { return _movieFolderCoverArtworkFilenamePattern; }
            set {
                _movieFolderCoverArtworkFilenamePattern = value;
                OnSettingChanged("local_moviefolder_coverart_pattern");
            }
        }
        private string _movieFolderCoverArtworkFilenamePattern;

        #region Minimum Size

        [CornerstoneSetting(
            Name = "Width",
            Description = "The minimum width in pixels for any given cover. If a cover from any data provider is smaller than this value it will not be downloaded and saved.",
            Groups = "|Movie Importer|Cover Art|Minimum Size",
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
            Name = "Height",
            Description = "The minimum height in pixels for any given cover. If a cover from any data provider is smaller than this value it will not be downloaded and saved.",
            Groups = "|Movie Importer|Cover Art|Minimum Size",
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

        #endregion

        #region Maximum Size

        [CornerstoneSetting(
            Name = "Width",
            Description = "The maximum width in pixels for any given cover. If a cover from any data provider is larger than this value it will be resized.",
            Groups = "|Movie Importer|Cover Art|Maximum Size",
            Identifier = "max_cover_width",
            Default = 680)]
        public int MaximumCoverWidth {
            get { return _maximumCoverWidth; }
            set {
                _maximumCoverWidth = value;
                OnSettingChanged("max_cover_width");
            }
        }
        private int _maximumCoverWidth;


        [CornerstoneSetting(
            Name = "Height",
            Description = "The maximum height in pixels for any given cover. If a cover from any data provider is larger than this value it will be resized.",
            Groups = "|Movie Importer|Cover Art|Maximum Size",
            Identifier = "max_cover_height",
            Default = 960)]
        public int MaximumCoverHeight {
            get { return _maximumCoverHeight; }
            set {
                _maximumCoverHeight = value;
                OnSettingChanged("max_cover_height");
            }
        }
        private int _maximumCoverHeight;

        #endregion

        [CornerstoneSetting(
            Name = "Cover Thumbnail Width",
            Description = "The width that the thumbnail of cover art should be set. For this to affect previously imported movies you must manually delete your existing thumbnails.",
            Groups = "|Movie Importer|Cover Art|",
            Identifier = "gui_thumb_width",
            Default = 175)]
        public int ThumbnailWidth {
            get { return _thumbnailWidth; }
            set {
                _thumbnailWidth = value;
                OnSettingChanged("gui_thumb_width");
            }
        }
        private int _thumbnailWidth;

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
            Default = false,
            Hidden = true)]
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
            Default = "backdrop.jpg|backdrop.png|backdrop.bmp",
            Hidden = true)]
        public string MovieFolderBackdropFilenamePattern {
            get { return _movieFolderBackdropFilenamePattern; }
            set {
                _movieFolderBackdropFilenamePattern = value;
                OnSettingChanged("local_moviefolder_backdrop_pattern");
            }
        }
        private string _movieFolderBackdropFilenamePattern;

        #region Minimum Size

        [CornerstoneSetting(
            Name = "Width",
            Description = "The minimum width in pixels for any given movie backdrop. If a backdrop from any data provider is smaller than this value it will not automatically be downloaded and saved.",
            Groups = "|Movie Importer|Backdrops|Minimum Size|",
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
            Name = "Height",
            Description = "The minimum height in pixels for any given movie backdrop. If a cover from any data provider is smaller than this value it will not automatically be downloaded and saved.",
            Groups = "|Movie Importer|Backdrops|Minimum Size|",
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

        #endregion

        #region Maximum Size

        [CornerstoneSetting(
            Name = "Width",
            Description = "The maximum width in pixels for any given movie backdrop. If a backdrop from any data provider is larger than this value it will be resized.",
            Groups = "|Movie Importer|Backdrops|Maximum Size|",
            Identifier = "max_backdrop_width",
            Default = 1920)]
        public int MaximumBackdropWidth {
            get { return _maximumBackdropWidth; }
            set {
                _maximumBackdropWidth = value;
                OnSettingChanged("max_backdrop_width");
            }
        }
        private int _maximumBackdropWidth;


        [CornerstoneSetting(
            Name = "Height",
            Description = "The maximum height in pixels for any given movie backdrop. If a cover from any data provider is larger than this value it will be resized.",
            Groups = "|Movie Importer|Backdrops|Maximum Size|",
            Identifier = "max_backdrop_height",
            Default = 1080)]
        public int MaximumBackdropHeight {
            get { return _maximumBackdropHeight; }
            set {
                _maximumBackdropHeight = value;
                OnSettingChanged("max_backdrop_height");
            }
        }
        private int _maximumBackdropHeight;

        #endregion

        #endregion

        #region themoviedb.org

        [CornerstoneSetting(
            Name = "Max Timeouts",
            Description = "The maximum number of timeouts received from the server before a thread returns an error condition.",
            Groups = "|Movie Importer|Network Timeout Settings|",
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
            Groups = "|Movie Importer|Network Timeout Settings|",
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
            Groups = "|Movie Importer|Network Timeout Settings|",
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

        #region Importer Language Options

        [CornerstoneSetting(
            Name = "Data Provider Management",
            Description = "Method used to manage data providers. Valid options are 'auto', and 'manual'.",
            Groups = "|Movie Importer|Importer Language Options|",
            Identifier = "dataprovider_management",
            Default = "undefined",
            Hidden = true)]
        public string DataProviderManagementMethod {
            get { return _dataProviderManagementMethod; }
            set {
                _dataProviderManagementMethod = value;
                OnSettingChanged("dataprovider_management");
            }
        }
        private string _dataProviderManagementMethod;

        [CornerstoneSetting(
            Name = "Automatic Data Provider Language",
            Description = "The language that the automatic data provider management service will optimize for.",
            Groups = "|Movie Importer|Importer Language Options|",
            Identifier = "dataprovider_auto_language",
            Default = "en",
            Hidden = true)]
        public string DataProviderAutoLanguage {
            get { return _dataProviderAutoLanguage; }
            set {
                _dataProviderAutoLanguage = value;
                OnSettingChanged("dataprovider_auto_language");
            }
        }
        private string _dataProviderAutoLanguage;

        [CornerstoneSetting(
            Name = "Use Translator Service",
            Description = "Service that will translate scraped movie information to a specified language. This service translates the following movie detail fields: genres, tagline, summary.",
            Groups = "|Movie Importer|Importer Language Options|",
            Identifier = "use_translator",
            Default = false,
            Hidden = true)]
        public bool UseTranslator {
            get { return false; }
            set {
                _useTranslator = value;
                OnSettingChanged("use_translator");
            }
        }
        private bool _useTranslator;

        [CornerstoneSetting(
            Name = "Translator Service Configured",
            Description = "Service that will translate scraped movie information to a specified language. This service translates the following movie detail fields: genres, tagline, summary.",
            Groups = "|Movie Importer|Importer Language Options|",
            Identifier = "translator_configured",
            Default = false,
            Hidden = true)]
        public bool TranslatorConfigured {
            get { return _translatorConfigured; }
            set {
                _translatorConfigured = value;
                OnSettingChanged("translator_configured");
            }
        }
        private bool _translatorConfigured;

        [CornerstoneSetting(
            Name = "Translation Language",
            Description = "The language that the translator service will attempt to tranlate scraped movie details into.",
            Groups = "|Movie Importer|Importer Language Options|",
            Identifier = "translate_to",
            Default = "English",
            Hidden = true)]
        public string TranslationLanguageStr
        {
            get { return _translateTo; }
            set
            {
                _translateTo = value;
                OnSettingChanged("translate_to");
            }
        }
        private string _translateTo;

        public TranslatorLanguage TranslationLanguage {
            get {
                if (_translationLanguage == null) {
                    foreach (TranslatorLanguage currLang in LanguageUtility.TranslatableCollection) {
                        if (LanguageUtility.ToString(currLang).ToLower() == TranslationLanguageStr.ToLower()) {
                            _translationLanguage = currLang;
                            break;
                        }
                    }
                    
                    if (_translationLanguage == null) 
                        _translationLanguage = TranslatorLanguage.English;                   
                }

                return (TranslatorLanguage) _translationLanguage;
            }

            set {
                _translationLanguage = value;
                TranslationLanguageStr = LanguageUtility.ToString(value);
            }
        } private TranslatorLanguage? _translationLanguage = null;

        #endregion

        #endregion

        #region GUI Settings

        #region Interface Options

        [CornerstoneSetting(
            Name = "Default View",
            Description = "The default view used in the MediaPortal GUI when the plug-in is first opened. Valid options are \"lastused\", \"list\", \"thumbs\", \"largethumbs\", \"coverflow\" and \"filmstrip\".",
            Groups = "|MediaPortal GUI|Interface Options|",
            Identifier = "default_view",
            Default = "lastused",
            Hidden = true)]
        public string DefaultViewStr {
            get { return _defaultViewStr; }
            set {
                _defaultViewStr = value;
                OnSettingChanged("default_view");
            }
        }
        private string _defaultViewStr;

        /// <summary>
        /// The default view mode to start the plug-in in, as defined by the user.
        /// </summary>
        public BrowserViewMode DefaultView {
            get {
                string defaultView = DefaultViewStr.Trim().ToLower();
                if (defaultView.Equals("list"))
                    return BrowserViewMode.LIST;
                else if (defaultView.Equals("thumbs"))
                    return BrowserViewMode.SMALLICON;
                else if (defaultView.Equals("largethumbs"))
                    return BrowserViewMode.LARGEICON;
                else if (defaultView.Equals("filmstrip"))
                    return BrowserViewMode.FILMSTRIP;
                else if (defaultView.Equals("coverflow"))
                    return BrowserViewMode.COVERFLOW;
                else if (defaultView.Equals("lastused"))
                    return BrowserViewMode.LASTUSED;
                else {
                    logger.Warn("The DEFAULT_VIEW setting contains an invalid value. Defaulting to List View.");
                    return BrowserViewMode.LIST;
                }
            }
        }

        [CornerstoneSetting(
            Name = "Last Used View",
            Description = "The last view used in the GUI. This only applies when categories are disabled. When categories are enabled, this is stored on a node by node basis. Valid options are \"list\", \"thumbs\", \"largethumbs\", and \"filmstrip\".",
            Groups = "|MediaPortal GUI|Interface Options|",
            Identifier = "last_used_view",
            Default = "list",
            Hidden = true)]
        public string LastUsedViewStr {
            get { return _lastUsedView; }
            set {
                _lastUsedView = value;
                OnSettingChanged("last_used_view");
            }
        }
        private string _lastUsedView;

        public BrowserViewMode LastUsedView {
            get {
                if (LastUsedViewStr == "list")
                    return BrowserViewMode.LIST;
                if (LastUsedViewStr == "thumbs")
                    return BrowserViewMode.SMALLICON;
                if (LastUsedViewStr == "largethumbs")
                    return BrowserViewMode.LARGEICON;
                if (LastUsedViewStr == "filmstrip")
                    return BrowserViewMode.FILMSTRIP;
                if (LastUsedViewStr == "coverflow")
                    return BrowserViewMode.COVERFLOW;

                return BrowserViewMode.LIST;
            }

            set {
                switch (value) {
                    case BrowserViewMode.SMALLICON:
                        LastUsedViewStr = "thumbs";
                        break;
                    case BrowserViewMode.LARGEICON:
                        LastUsedViewStr = "largethumbs";
                        break;
                    case BrowserViewMode.FILMSTRIP:
                        LastUsedViewStr = "filmstrip";
                        break;
                    case BrowserViewMode.COVERFLOW:
                        LastUsedViewStr = "coverflow";
                        break;
                    case BrowserViewMode.LIST:
                    default:
                        LastUsedViewStr = "list";
                        break;

                }
            }
        }

        [CornerstoneSetting(
            Name = "Click Shows Details",
            Description = "Determines behavior when a movie in the movie browser is clicked. If true, the details view appears. If false the movie starts playback.",
            Groups = "|MediaPortal GUI|Interface Options|",
            Identifier = "click_to_details",
            Default = true,
            Hidden = true)]
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
            Default = "Moving Pictures",
            Hidden = true)]
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
            Default = "Title",
            Hidden = true)]
        public string DefaultSortField {
            get { return _defaultSortField; }
            set {
                _defaultSortField = value;
                OnSettingChanged("default_sort_field");
            }
        }
        private string _defaultSortField;

        [CornerstoneSetting(
            Name = "Allow user to delete files from the GUI context menu",
            Description = "Enables a delete menu item, which allows you to delete movies from your hard drive.",
            Groups = "|MediaPortal GUI|Interface Options|",
            Identifier = "enable_delete_movie",
            Default = false,
            Hidden = true)]
        public bool AllowDelete {
            get { return _allowDelete; }
            set {
                _allowDelete = value;
                OnSettingChanged("enable_delete_movie");
            }
        }
        private bool _allowDelete;

        [CornerstoneSetting(
            Name = "Display Rescan Menu Item in GUI",
            Description = "If enabled, a menu item in the GUI will be available to trigger a full rescan of all import paths.",
            Groups = "|MediaPortal GUI|Interface Options|",
            Identifier = "gui_show_rescan_menuitem",
            Default = false,
            Hidden = true)]
        public bool ShowRescanMenuItem {
            get { return _showRescanMenuItem; }
            set {
                _showRescanMenuItem = value;
                OnSettingChanged("gui_show_rescan_menuitem");
            }
        }
        private bool _showRescanMenuItem;

        [CornerstoneSetting(
            Name = "Auto-Prompt For User Rating",
            Description = "Moving Pictures will prompt you for your rating of a movie after the movie ends",
            Groups = "|MediaPortal GUI|Interface Options|",
            Identifier = "auto_prompt_for_rating",
            Default = false)]
        public bool AutoPromptForRating {
            get { return _autoPromptForRating; }
            set {
                _autoPromptForRating = value;
                OnSettingChanged("auto_prompt_for_rating");
            }
        }
        private bool _autoPromptForRating;

        [CornerstoneSetting(
            Name = "Allow Grouping",
            Description = "Show group headers when sorting the movies",
            Groups = "|MediaPortal GUI|Interface Options|",
            Identifier = "allow_grouping",
            Default = false)]
        public bool AllowGrouping {
            get { return _allow_grouping; }
            set {
                _allow_grouping = value;
                OnSettingChanged("allow_grouping");
            }
        }
        private bool _allow_grouping;

        [CornerstoneSetting(
            Name = "Display the actual runtime of a movie",
            Description = "If enabled this setting will display the actual runtime of the movie instead of the runtime imported from the data provider. If there's no actual runtime information available it will default to the imported runtime.",
            Groups = "|MediaPortal GUI|Interface Options|",
            Identifier = "gui_display_actual_runtime",
            Default = true)]
        public bool DisplayActualRuntime {
            get { return _displayActualRuntime; }
            set {
                _displayActualRuntime = value;
                OnSettingChanged("gui_display_actual_runtime");
            }
        }
        private bool _displayActualRuntime;

        [CornerstoneSetting(
            Name = "Reset the selected movie when switching categories",
            Description = "If enabled this setting will reset the selected movie when you switch between categories.",
            Groups = "|MediaPortal GUI|Interface Options|",
            Identifier = "gui_category_reset_selection",
            Default = true)]
        public bool ResetSelectedMovieWhenSwitchingCategories {
            get { return _resetSelectedMovieWhenSwitchingCategories; }
            set {
                _resetSelectedMovieWhenSwitchingCategories = value;
                OnSettingChanged("gui_category_reset_selection");
            }
        }
        private bool _resetSelectedMovieWhenSwitchingCategories;

        [CornerstoneSetting(
            Name = "Second Label on List View",
            Description = "Show a second label on listview. Possible values are none, year, runtime, genre, studio, language, certification or score.",
            Groups = "|MediaPortal GUI|Interface Options|",
            Identifier = "gui_listview_label2",
            Default = "year")]
        public string SecondLabel {
            get { return _secondLabel; }
            set {
                _secondLabel = value;
                OnSettingChanged("gui_listview_label2");
            }
        }
        private string _secondLabel;

        [CornerstoneSetting(
            Name = "Remove Articles From Display Title",
            Description = "If enabled, articles such as \"the\", \"a\", and \"an\" will be removed from the Display Title in the GUI. This compliments the \"Remove Title Articles\" setting.",
            Groups = "|MediaPortal GUI|Interface Options|",
            Identifier = "remove_articles_from_display_title",
            Default = false,
            Hidden = false)]
        public bool RemoveArticlesFromDisplayTitle {
            get { return _removeArticlesFromDisplayTitle; }
            set {
                _removeArticlesFromDisplayTitle = value;
                OnSettingChanged("remove_articles_from_display_title");
            }
        }
        private bool _removeArticlesFromDisplayTitle;

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
            Name = "Details Loading Delay",
            Description = "The number of milliseconds that Moving Pictures waits before it show the details about the current selection when traversing movies in the GUI. Increasing this value can improve performance if you are experiencing slow down with rapid movements in the GUI.",
            Groups = "|MediaPortal GUI|Tweaks|",
            Identifier = "gui_details_delay",
            Default = 250)]
        public int DetailsLoadingDelay {
            get { return _detailsLoadingDelay; }
            set {
                _detailsLoadingDelay = value;
                OnSettingChanged("gui_details_delay");
            }
        }
        private int _detailsLoadingDelay;

        [CornerstoneSetting(
            Name = "Category Random Artwork Refresh Interval",
            Description = "The number of seconds that Moving Pictures waits before renewing the backdrop when a category (using random backdrop) is selected.",
            Groups = "|MediaPortal GUI|Tweaks|",
            Identifier = "gui_artwork_random_refresh",
            Default = 120)]
        public int CategoryRandomArtworkRefreshInterval {
            get { return _categoryRandomArtworkRefreshInterval; }
            set {
                _categoryRandomArtworkRefreshInterval = value;
                OnSettingChanged("gui_artwork_random_refresh");
            }
        }
        private int _categoryRandomArtworkRefreshInterval;

        [CornerstoneSetting(
            Name = "Use Remote Control Filtering",
            Description = "Enables the Remote Controle Filter, set to false if you want to use the default mediaportal remote control functionality.",
            Groups = "|MediaPortal GUI|Tweaks|",
            Identifier = "enable_rc_filter",
            Default = true,
            Hidden = true)]
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
            Default = 90,
            Hidden = true)]
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
             Default = "DETAILS",
             Hidden = true)]
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
            Description = "Location of a custom intro that will play before each movie.  This should either be the full path to a video file or the full path to a directory containing several video files.",
            Groups = "|MediaPortal GUI|Playback Options|",
            Identifier = "custom_intro_location",
            Default = " ",
            MoreInfoLink = "http://moving-pictures.tv/wiki/Custom_Introduction_Videos")]
        public string CustomIntroLocation {
            get { return _customIntroLocation; }
            set {
                _customIntroLocation = value;
                OnSettingChanged("custom_intro_location");
            }
        }
        private string _customIntroLocation;

        [CornerstoneSetting(
            Name = "Custom Intro Count",
            Description = "The number of custom intros to play. This only applies if the Custom Intro Location is set to a directory rather than a file.",
            Groups = "|MediaPortal GUI|Playback Options|",
            Identifier = "custom_intro_count",
            Default = 1,
            MoreInfoLink = "http://moving-pictures.tv/wiki/Custom_Introduction_Videos")]
        public int CustomIntroCount {
            get { return _customIntroCount; }
            set {
                _customIntroCount = value;
                OnSettingChanged("custom_intro_count");
            }
        }
        private int _customIntroCount;


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
            Name = "Enable Dynamic Refresh Rate Changer Settings",
            Description = "Enable the Dynamic Refresh Rate Changer when using an external player, this setting will only take effect if you have setup the Dynamic Refresh Rate Changer settings in your MediaPortal configuration.",
            Groups = "|MediaPortal GUI|Bluray/HD-DVD Playback|",
            Identifier = "playback_hd_change_refresh_rate",
            Default = false)]
        public bool UseDynamicRefreshRateChangerWithExternalPlayer {
            get { return _useDynamicRefreshRateChangerWithExternalPlayer; }
            set {
                _useDynamicRefreshRateChangerWithExternalPlayer = value;
                OnSettingChanged("playback_hd_change_refresh_rate");
            }
        }
        private bool _useDynamicRefreshRateChangerWithExternalPlayer;


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
            Description = "The command-line arguments that should be appended when calling the executable. (available variables: %filename% will be replaced with the path to the movie, %fps% will be replaced with the framerate for the movie)",
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

        #region Sorting

        [CornerstoneSetting(
            Name = "Remove Title Articles",
            Description = "If enabled, articles such as \"the\", \"a\", and \"an\" will not be considered when sorting by title. This affects the Sort By field and for a change to take effect you must refresh your Sort By values from the Movie Manager.",
            Groups = "|MediaPortal GUI|Sorting|",
            Identifier = "remove_title_articles",
            Default = true)]
        public bool RemoveTitleArticles {
            get { return _removeTitleArticles; }
            set {
                _removeTitleArticles = value;
                OnSettingChanged("remove_title_articles");
            }
        }
        private bool _removeTitleArticles;

        [CornerstoneSetting(
            Name = "Articles for Removal",
            Description = "The articles that will be removed from a title when found at the beginning of a title for sorting purposes. Each language has its own set of articles, seperate articles with a pipe \"|\". See the \"Remove Title Articles\" setting.",
            Groups = "|MediaPortal GUI|Sorting|",
            Identifier = "articles_for_removal",
            Default = "en(the|a|an),de(ein|eine|der|die|das),pt(a|as|o|os|um|uma|uns|umas),nl(de|het),fr(le|la|les|un|une|des|l),es(el|la|lo|los|las|un|una|unos|unas)")]
        public string ArticlesForRemoval {
            get { return _articlesForRemoval; }
            set {
                _articlesForRemoval = value;
                OnSettingChanged("articles_for_removal");
            }
        }
        private string _articlesForRemoval;

        /// <summary>
        /// Preposition articles grouped by language.
        /// </summary>
        public Dictionary<string, string> ArticleGroups {
            get {
                var articleGroups = new Dictionary<string, string>();
                foreach (string articleGroup in MovingPicturesCore.Settings.ArticlesForRemoval.Split(',')) {
                    // language
                    int start = 0;
                    int end = articleGroup.IndexOf("(");
                    if (start < 0 || end < 0) continue;
                    string key = articleGroup.Substring(start, end);

                    // articles
                    start = end + 1;
                    end = articleGroup.IndexOf(")");
                    if (start < 0 || end < 0) continue;
                    string val = articleGroup.Substring(start, (end - start));

                    if (!articleGroups.ContainsKey(key)) articleGroups.Add(key, val);
                }

                return articleGroups;
            }
        }

        #endregion

        #region Parental Controls

        [CornerstoneSetting(
            Name = "Enable Parental Controls",
            Description = "Enables the Paretal Controls feature in the GUI.",
            Groups = "|MediaPortal GUI|Parental Controls|",
            Identifier = "enable_parental_controls",
            Default = false,
            Hidden = true)]
        public bool ParentalControlsEnabled {
            get { return _parentalControlsEnabled; }
            set {
                _parentalControlsEnabled = value;
                OnSettingChanged("enable_parental_controls");
            }
        }
        private bool _parentalControlsEnabled;

        [CornerstoneSetting(
            Name = "Parental Controls Filter ID",
            Description = "The filter attached to the Parental Controls functionality.",
            Groups = "|MediaPortal GUI|Parental Controls|",
            Identifier = "parental_controls_filter_id",
            Default = "null",
            Hidden = true)]
        public string ParentalContolsFilterID {
            get { return _parentalContolsFilterID; }
            set {
                _parentalContolsFilterID = value;
                OnSettingChanged("parental_controls_filter_id");
            }
        }
        private string _parentalContolsFilterID;

        public DBFilter<DBMovieInfo> ParentalControlsFilter {
            get {
                if (_parentalControlsFilter == null) {
                    // grab or create the filter object attached to the parental controls
                    string filterID = MovingPicturesCore.Settings.ParentalContolsFilterID;
                    if (filterID == "null") {
                        _parentalControlsFilter = new DBFilter<DBMovieInfo>();
                        _parentalControlsFilter.Name = "Children's Movies";
                        MovingPicturesCore.DatabaseManager.Commit(_parentalControlsFilter);
                        ParentalContolsFilterID = _parentalControlsFilter.ID.ToString();
                    }
                    else {
                        _parentalControlsFilter = MovingPicturesCore.DatabaseManager.Get<DBFilter<DBMovieInfo>>(int.Parse(filterID));
                    }
                }

                return _parentalControlsFilter;
            }
        } private DBFilter<DBMovieInfo> _parentalControlsFilter = null;

        [CornerstoneSetting(
            Name = "Parental Controls Password",
            Description = "The password required to access movies restricted by parental controls.",
            Groups = "|MediaPortal GUI|Parental Controls|",
            Identifier = "parental_controls_password",
            Default = "1111",
            Hidden = true,
            Sensitive = true)]
        public string ParentalContolsPassword {
            get { return _parentalContolsPassword; }
            set {
                _parentalContolsPassword = value;
                OnSettingChanged("parental_controls_password");
            }
        }
        private string _parentalContolsPassword;

        [CornerstoneSetting(
            Name = "Parental Controls Timeout",
            Description = "If set, this will reenable the parental filter after the system is idle for x minutes.  Use 0 to disable the timeout.",
            Groups = "|MediaPortal GUI|Parental Controls|",
            Identifier = "parental_controls_timeout",
            Default = 10,
            Hidden = false)]
        public int ParentalControlsTimeout {
            get { return _parentalControlsTimeout; }
            set {
                _parentalControlsTimeout = value;
                OnSettingChanged("parental_controls_timeout");
            }
        }
        private int _parentalControlsTimeout;
        #endregion

        #region Filtering

        [CornerstoneSetting(
            Name = "Filter Menu ID",
            Description = "The menu for the popup filtering menu.",
            Groups = "|MediaPortal GUI|Filtering|",
            Identifier = "filter_menu_id",
            Default = "null",
            Hidden = true)]
        public string FilterMenuID {
            get { return _filterMenuID; }
            set {
                _filterMenuID = value;
                OnSettingChanged("filter_menu_id");
            }
        }
        private string _filterMenuID;

        public DBMenu<DBMovieInfo> FilterMenu {
            get {
                if (_filterMenu == null) {
                    // grab or create the menu for the filtering popup
                    string menuID = FilterMenuID;
                    if (menuID == "null") {
                        _filterMenu = new DBMenu<DBMovieInfo>();
                        _filterMenu.Name = "Filtering Menu";
                        MovingPicturesCore.DatabaseManager.Commit(_filterMenu);
                        FilterMenuID = _filterMenu.ID.ToString();
                    }
                    else {
                        _filterMenu = MovingPicturesCore.DatabaseManager.Get<DBMenu<DBMovieInfo>>(int.Parse(menuID));
                    }
                }

                return _filterMenu;
            }
        } private DBMenu<DBMovieInfo> _filterMenu = null;

        [CornerstoneSetting(
            Name = "Use Default Filter",
            Description = "If enabled the default filter will be used on initial launch.",
            Groups = "|MediaPortal GUI|Filtering|",
            Identifier = "use_default_filter",
            Default = false,
            Hidden = true)]
        public bool DefaultFilterEnabled {
            get { return _useDefaultFilter; }
            set {
                _useDefaultFilter = value;
                OnSettingChanged("use_default_filter");
            }
        }
        private bool _useDefaultFilter;

        [CornerstoneSetting(
            Name = "Default Filter ID",
            Description = "The database ID for the default filter on startup.",
            Groups = "|MediaPortal GUI|Filtering|",
            Identifier = "default_filter_id",
            Default = "null",
            Hidden = true)]
        public string DefaultFilterID {
            get { return _defaultFilterID; }
            set {
                _defaultFilterID = value;
                OnSettingChanged("default_filter_id");
            }
        }
        private string _defaultFilterID;

        public DBNode<DBMovieInfo> DefaultFilter {
            get {
                if (_defaultFilter == null) {
                    // grab the default filter or assign the default
                    string filterID = DefaultFilterID;
                    if (filterID == "null") {
                        _defaultFilter = null;

                        List<DBNode<DBMovieInfo>> resultSet = FilterMenu.FindNode("${UnwatchedMovies}");                        
                        if (resultSet.Count > 0) {
                            _defaultFilter = resultSet[0];
                            DefaultFilterID = _defaultFilter.ID.ToString();
                        }
                    }
                    else {                        
                        try { _defaultFilter = MovingPicturesCore.DatabaseManager.Get<DBNode<DBMovieInfo>>(int.Parse(filterID)); }
                        catch (FormatException) {
                            _defaultFilter = null;
                            DefaultFilterID = "null";
                        }
                    }
                }

                return _defaultFilter;
            }

            set {
                if (value == null) DefaultFilterID = "null";
                else DefaultFilterID = value.ID.ToString();

                _defaultFilter = null;
            }
        } private DBNode<DBMovieInfo> _defaultFilter = null;

        [CornerstoneSetting(
            Name = "Category Menu ID",
            Description = "The menu for the categories functionality.",
            Groups = "|MediaPortal GUI|Filtering|",
            Identifier = "categories_menu_id",
            Default = "null",
            Hidden = true)]
        public string CategoriesMenuID {
            get { return _categoriesMenuID; }
            set {
                _categoriesMenuID = value;
                OnSettingChanged("categories_menu_id");
            }
        }
        private string _categoriesMenuID;

        public DBMenu<DBMovieInfo> CategoriesMenu {
            get {
                if (_categoriesMenu == null) {
                    // grab or create the menu for the filtering popup
                    string menuID = CategoriesMenuID;
                    if (menuID == "null") {
                        _categoriesMenu = new DBMenu<DBMovieInfo>();
                        _categoriesMenu.Name = "Categories Menu";
                        MovingPicturesCore.DatabaseManager.Commit(_categoriesMenu);
                        CategoriesMenuID = _categoriesMenu.ID.ToString();
                    }
                    else {
                        _categoriesMenu = MovingPicturesCore.DatabaseManager.Get<DBMenu<DBMovieInfo>>(int.Parse(menuID));
                    }
                }

                return _categoriesMenu;
            }
        } private DBMenu<DBMovieInfo> _categoriesMenu = null;

        [CornerstoneSetting(
            Name = "Movie Manager Filter Menu ID",
            Description = "The menu for the Movie Manager Filters functionality.",
            Groups = "|MediaPortal GUI|Filtering|",
            Identifier = "movie_manager_filter_menu_id",
            Default = "null",
            Hidden = true)]
        public string MovieManagerFilterMenuID {
            get { return _movieManagerFilterMenuID; }
            set {
                _movieManagerFilterMenuID = value;
                OnSettingChanged("movie_manager_filter_menu_id");
            }
        }
        private string _movieManagerFilterMenuID;

        public DBMenu<DBMovieInfo> MovieManagerFilterMenu {
            get {
                if (_movieManagerFilterMenu == null) {
                    // grab or create the menu for the filtering popup
                    string menuID = MovieManagerFilterMenuID;
                    if (menuID == "null") {
                        _movieManagerFilterMenu = new DBMenu<DBMovieInfo>();
                        _movieManagerFilterMenu.Name = "Movie Manager Filters Menu";
                        MovingPicturesCore.DatabaseManager.Commit(_movieManagerFilterMenu);
                        MovieManagerFilterMenuID = _movieManagerFilterMenu.ID.ToString();
                    }
                    else {
                        _movieManagerFilterMenu = MovingPicturesCore.DatabaseManager.Get<DBMenu<DBMovieInfo>>(int.Parse(menuID));
                    }
                }

                return _movieManagerFilterMenu;
            }
        } private DBMenu<DBMovieInfo> _movieManagerFilterMenu = null;

        [CornerstoneSetting(
            Name = "Enable Categories",
            Description = "Enables the Categories feature in the GUI.",
            Groups = "|MediaPortal GUI|Filtering|",
            Identifier = "enable_categories",
            Default = true,
            Hidden = true)]
        public bool CategoriesEnabled {
            get { return _categoriesEnabled; }
            set {
                _categoriesEnabled = value;
                OnSettingChanged("enable_categories");
            }
        }
        private bool _categoriesEnabled;

        [CornerstoneSetting(
            Name = "Dynamic Actor Category Limit",
            Description = "When an Actor based dynamic category is created, an actor must be in this number of movies you own to be included in the list.",
            Groups = "|MediaPortal GUI|Filtering|",
            Identifier = "actor_limit",
            Default = 2)]
        public int ActorLimit {
            get { return _actorLimit; }
            set {
                _actorLimit = value;
                OnSettingChanged("actor_limit");
            }
        }
        private int _actorLimit;

        #endregion

        #region Debugging

        [CornerstoneSetting(
            Name = "Skinners Debug Mode",
            Description = "Enabling this will log all changes to skin properties when logging is set to debug.",
            Groups = "|Internal|",
            Identifier = "enable_debug_allskinproperties",
            Default = false )]
        public bool LogAllSkinPropertyChanges {
            get { return _logAllSkinPropertyChanges; }
            set {
                _logAllSkinPropertyChanges = value;
                OnSettingChanged("enable_debug_allskinproperties");
            }
        }
        private bool _logAllSkinPropertyChanges;

        #endregion

        #endregion

        #region Extras
        #region File Renamer
        
        [CornerstoneSetting(
            Name = "File Rename Pattern",
            Description = "When using the file renamer functionality, movie files will be renamed based on this pattern. The multi-part pattern will replace ${moviepart} if the movie is made up of multipe files.",
            Groups = "|Extras|File Renamer|",
            Identifier = "file_rename_string",
            Default = @"${movie.title} (${movie.year})")]
        public string FileRenameString {
            get { return _fileRenameString; }
            set {
                _fileRenameString = value;
                OnSettingChanged("file_rename_string");
            }
        }
        private string _fileRenameString;

        [CornerstoneSetting(
            Name = "Directory Rename Pattern",
            Description = "When using the file renamer functionality, directories will be renamed based on this pattern.",
            Groups = "|Extras|File Renamer|",
            Identifier = "directory_rename_string",
            Default = @"${movie.title} (${movie.year})")]
        public string DirectoryRenameString {
            get { return _directoryRenameString; }
            set {
                _directoryRenameString = value;
                OnSettingChanged("directory_rename_string");
            }
        }
        private string _directoryRenameString;

        [CornerstoneSetting(
            Name = "Multi-Part Rename Pattern",
            Description = "When using the file renamer functionality, this filename pattern will be appended to the regular pattern for movies made up of multiple files.",
            Groups = "|Extras|File Renamer|",
            Identifier = "file_multipart",
            Default = @" Part #")]
        public string FileMultipartString {
            get { return _file_multipart; }
            set {
                _file_multipart = value;
                OnSettingChanged("file_multipart");
            }
        }
        private string _file_multipart;

        [CornerstoneSetting(
             Name = "Additional Files Type to Rename",
             Description = "When using the file renamer functionality, files starting with the original filename and with one of these extension will be renamed too.",
             Groups = "|Extras|File Renamer|",
             Identifier = "file_rename_other_filetypes",
             Default = @".srt|.idx|.sub|.ac3|.dts|.nfo|.txt")]
        public string Rename_SecondaryFileTypes
        {
            get { return _file_rename_other_filetypes; }
            set
            {
                _file_rename_other_filetypes = value;
                OnSettingChanged("file_rename_other_filetypes");
            }
        }
        private string _file_rename_other_filetypes;

        [CornerstoneSetting(
             Name = "Include Folders When Renaming",
             Description = "If true, when renaming files for a movie, the folder containing the files will be renamed as well.",
             Groups = "|Extras|File Renamer|",
             Identifier = "file_rename_folder",
             Default = true)]
        public bool RenameFolders
        {
            get { return _file_rename_folder; }
            set
            {
                _file_rename_folder = value;
                OnSettingChanged("file_rename_folder");
            }
        }
        private bool _file_rename_folder;

        [CornerstoneSetting(
          Name = "Include Files When Renaming",
          Description = "If true, when renaming, the movie files will be renamed.",
          Groups = "|Extras|File Renamer|",
          Identifier = "rename_files",
          Default = true)]
        public bool RenameFiles
        {
            get { return _renameFiles; }
            set
            {
                _renameFiles = value;
                OnSettingChanged("rename_files");
            }
        }
        private bool _renameFiles;

        [CornerstoneSetting(
            Name = "Include Secondary Files When Renaming",
            Description = "If true, when renaming files for a movie, thesecondary files will be renamed as well.",
            Groups = "|Extras|File Renamer|",
            Identifier = "rename_secondary_files",
            Default = true)]
        public bool RenameSecondaryFiles {
            get { return _renameSecondaryFiles; }
            set {
                _renameSecondaryFiles = value;
                OnSettingChanged("rename_secondary_files");
            }
        }
        private bool _renameSecondaryFiles;

        #endregion
        #endregion

        #region Internal Settings

        [CornerstoneSetting(
            Name = "Data Source Manager Enhanced Debug Mode",
            Description = "If set to true, additional logging will be written by the Scriptable Scraping Engine when the entire plug-in is in debug mode. Internal scripts stored in the DLL will also be reloaded on launch regardless of version number.",
            Groups = "|Internal|",
            Identifier = "source_manager_debug",
            Default = false,
            Hidden = true)]
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
            Default = "True",
            Hidden = true)]
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
            Default = "0.0.0.0",
            Hidden = true)]
        public string Version {
            get { return _versionNumber; }
            set {
                _versionNumber = value;
                OnSettingChanged("version");
            }
        }
        private string _versionNumber;

        [CornerstoneSetting(
            Name = "Allow Disk Monitor to Watch for Drive Changes",
            Description = "If disabled the disk monitor will not notify other aspects of the plug-in about disk events such as DVD insertions and newly connected network drives. Do not disable unless you are experiencing problems with the Disk Monitor.",
            Groups = "|Internal|",
            Identifier = "disk_monitor_enabled",
            Default = true)]
        public bool DeviceManagerEnabled
        {
            get { return _deviceManagerEnabled; }
            set
            {
                _deviceManagerEnabled = value;
                OnSettingChanged("disk_monitor_enabled");
            }
        }
        private bool _deviceManagerEnabled;

        #endregion

        #region follw.it

        [CornerstoneSetting(
            Name = "follw.it Enabled",
            Description = "Connect this instance of Moving Pictures to follw.it.",
            Groups = "|follw.it|",
            Identifier = "social_enabled",
            Default = false,
            Hidden = true)]
        public bool FollwitEnabled {
            get { return _follwitEnabled; }
            set {
                _follwitEnabled = value;
                OnSettingChanged("social_enabled");
            }
        }
        private bool _follwitEnabled;

        [CornerstoneSetting(
            Name = "Restrict Synchronized Movies",
            Description = "If true, when synchronizing with follw.it the movies synced will be filtered.",
            Groups = "|follw.it|",
            Identifier = "mps_restrict_synched_movies",
            Default = false,
            Hidden = true)]
        public bool RestrictSynchronizedMovies {
            get { return _restrictSynchronizedMovies; }
            set {
                _restrictSynchronizedMovies = value;
                OnSettingChanged("mps_restrict_synched_movies");
            }
        }
        private bool _restrictSynchronizedMovies;

        [CornerstoneSetting(
            Name = "follw.it URL",
            Description = "Base URL for follw.it.",
            Groups = "|follw.it|",
            Identifier = "socialurlbase",
            Default = "http://follw.it/",
            Hidden = false)]
        public string FollwitURLBase {
            get { return _follwitURLBase; }
            set {
                _follwitURLBase = value.Trim();
                if (!_follwitURLBase.EndsWith("/"))
                    _follwitURLBase += "/";

                OnSettingChanged("socialurlbase");
            }
        }
        private string _follwitURLBase;

        public string FollwitUrl {
            get {
                return FollwitURLBase + "api/2/";
            }
        }

        [CornerstoneSetting(
            Name = "Username",
            Description = "Username for follw.it.",
            Groups = "|follw.it|",
            Identifier = "socialusername",
            Default = "",
            Hidden = true)]
        public string FollwitUsername {
            get { return _follwitUsername; }
            set {
                _follwitUsername = value;
                OnSettingChanged("socialusername");
            }
        }
        private string _follwitUsername;

        [CornerstoneSetting(
            Name = "Password",
            Description = "Password for follw.it.",
            Groups = "|follw.it|",
            Identifier = "socialpassword",
            Default = "",
            Hidden = true,
            Sensitive = true)]
        public string FollwitHashedPassword {
            get { return _follwitPassword; }
            set {
                _follwitPassword = value;
                OnSettingChanged("socialpassword");
            }
        }
        private string _follwitPassword;

        [CornerstoneSetting(
            Name = "Task List Timer",
            Description = "The number of minutes between each check for follw.it tasks such as pulling down ratings from the webstie or uploading missing artwork. Enter 0 minutes to disable.",
            Groups = "|follw.it|",
            Identifier = "socialtasklisttimer",
            Default = 60,
            Hidden = false)]
        public int FollwitTaskListTimer {
            get { return _follwitTaskListTimer; }
            set {
                _follwitTaskListTimer = value;
                OnSettingChanged("socialtasklisttimer");
            }
        }
        private int _follwitTaskListTimer;

        [CornerstoneSetting(
            Name = "Follw.it Retry Time",
            Description = "Number of minutes to wait before reattempting to connect to follw.it when a connection error occured.",
            Groups = "|follw.it|",
            Identifier = "socialretrytime",
            Default = 1,
            Hidden = false)]
        public int FollwitRetryTime {
            get { return _follwitRetryTime; }
            set {
                _follwitRetryTime = value;
                OnSettingChanged("socialretrytime");
            }
        }
        private int _follwitRetryTime;


        [CornerstoneSetting(
            Name = "Last Synchronization Time",
            Description = "Date and time follw.it synchronization tasks were last processed.",
            Groups = "|follw.it|",
            Identifier = "sociallastretrieved",
            Default = "",
            Hidden = true)]
        public String LastSynchTime {
            get { return _follwitLastRetrieved; }
            set {
                _follwitLastRetrieved = value;
                OnSettingChanged("sociallastretrieved");
            }
        }
        private String _follwitLastRetrieved;

        
        [CornerstoneSetting(
            Name = "Enable Syncing of File Hashes to follw.it",
            Description = "Enable Syncing of File Hashes to follw.it",
            Groups = "|follw.it|",
            Identifier = "socialfilehashsync",
            Default = true)]
        public bool EnableFollwitFileHashSync {
            get { return _enableFollwitFileHashSync; }
            set {
                _enableFollwitFileHashSync = value;
                OnSettingChanged("socialfilehashsync");
            }
        }
        private bool _enableFollwitFileHashSync;

        [CornerstoneSetting(
            Name = "follw.it Movie Synch Filter ID",
            Description = "The filter used to restrict which movies are synchronized to follw.it.",
            Groups = "|follw.it|",
            Identifier = "social_sync_filter_id",
            Default = "null",
            Hidden = true)]
        public string FollwitSyncFilterID {
            get { return _follwitSyncFilterID; }
            set {
                _follwitSyncFilterID = value;
                OnSettingChanged("social_sync_filter_id");
            }
        }
        private string _follwitSyncFilterID;

        public DBFilter<DBMovieInfo> FollwitSyncFilter {
            get {
                if (_follwitSyncFilter == null) {
                    // grab or create the filter object attached to follw.it synching
                    string filterID = MovingPicturesCore.Settings.FollwitSyncFilterID;
                    if (filterID == "null") {
                        _follwitSyncFilter = new DBFilter<DBMovieInfo>();
                        _follwitSyncFilter.Name = "Synchronized Movies";
                        MovingPicturesCore.DatabaseManager.Commit(_follwitSyncFilter);
                        FollwitSyncFilterID = _follwitSyncFilter.ID.ToString();
                    }
                    else {
                        _follwitSyncFilter = MovingPicturesCore.DatabaseManager.Get<DBFilter<DBMovieInfo>>(int.Parse(filterID));
                    }
                }

                return _follwitSyncFilter;
            }
        } private DBFilter<DBMovieInfo> _follwitSyncFilter = null;



        [CornerstoneSetting(
            Name = "follw.it Sync Batch Size",
            Description = "The number of movies to sync to follw.it in each request",
            Groups = "|follw.it|",
            Identifier = "socialbatchsize",
            Default = 30)]
        public int FollwitBatchSize {
            get { return _follwitBatchSize; }
            set {
                _follwitBatchSize = value;
                OnSettingChanged("socialbatchsize");
            }
        }
        private int _follwitBatchSize;

        #endregion

    }
}
