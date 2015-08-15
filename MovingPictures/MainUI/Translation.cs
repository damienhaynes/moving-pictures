using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.MainUI {
    public static class Translation {

        #region Private variables

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static Dictionary<string, string> translations;
        private static Regex translateExpr = new Regex(@"\$\{([^\}]+)\}");
        private static string path = string.Empty;

        #endregion

        #region Constructor

        static Translation() {
            string lang;
            
            try {
                lang = GUILocalizeStrings.GetCultureName(GUILocalizeStrings.CurrentLanguage());
            }
            catch (Exception) {
                // when running MovingPicturesConfigTester outside of the MediaPortal directory this happens unfortunately
                // so we grab the active culture name from the system            
                lang = CultureInfo.CurrentUICulture.Name;
            }
                        
            logger.Info("Using language " + lang);
            
            path = Config.GetSubFolder(Config.Dir.Language, "MovingPictures");
            
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            LoadTranslations(lang);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the translated strings collection in the active language
        /// </summary>
        public static Dictionary<string, string> Strings {
            get {
                if (translations == null) {
                    translations = new Dictionary<string, string>();
                    Type transType = typeof(Translation);
                    FieldInfo[] fields = transType.GetFields(BindingFlags.Public | BindingFlags.Static);
                    foreach (FieldInfo field in fields) {
                        translations.Add(field.Name, field.GetValue(transType).ToString());
                    }
                }
                return translations;
            }
        }

        #endregion

        #region Public Methods

        public static int LoadTranslations(string lang) {
            XmlDocument doc = new XmlDocument();
            Dictionary<string, string> TranslatedStrings = new Dictionary<string, string>();
            string langPath = "";
            try {
                langPath = Path.Combine(path, lang + ".xml");
                doc.Load(langPath);
            }
            catch (Exception e) {
                if (lang == "en")
                    return 0; // otherwise we are in an endless loop!

                if (e.GetType() == typeof(FileNotFoundException))
                    logger.Warn("Cannot find translation file {0}.  Failing back to English", langPath);
                else
                    logger.ErrorException(String.Format("Error in translation xml file: {0}. Failing back to English", lang), e);

                return LoadTranslations("en");
            }
            foreach (XmlNode stringEntry in doc.DocumentElement.ChildNodes) {
                if (stringEntry.NodeType == XmlNodeType.Element)
                    try {
                        TranslatedStrings.Add(stringEntry.Attributes.GetNamedItem("name").Value, Regex.Unescape(stringEntry.InnerText));
                    }
                    catch (Exception ex) {
                        logger.ErrorException("Error in Translation Engine", ex);
                    }
            }

            Type TransType = typeof(Translation);
            FieldInfo[] fieldInfos = TransType.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (FieldInfo fi in fieldInfos) {
                if (TranslatedStrings != null && TranslatedStrings.ContainsKey(fi.Name))
                    TransType.InvokeMember(fi.Name, BindingFlags.SetField, null, TransType, new object[] { TranslatedStrings[fi.Name] });
                else
                    logger.Warn("Translation not found for name: {0}.  Using hard-coded English default.", fi.Name);
            }
            return TranslatedStrings.Count;
        }

        public static string GetByName(string name) {
            if (!Strings.ContainsKey(name))
                return name;

            return Strings[name];
        }

        public static string GetByName(string name, params object[] args) {
            return String.Format(GetByName(name), args);
        }

        /// <summary>
        /// Takes an input string and replaces all ${named} variables with the proper translation if available
        /// </summary>
        /// <param name="input">a string containing ${named} variables that represent the translation keys</param>
        /// <returns>translated input string</returns>
        public static string ParseString(string input) {
            MatchCollection matches = translateExpr.Matches(input);
            foreach(Match match in matches) {
                input = input.Replace(match.Value, GetByName(match.Groups[1].Value));
            }
            return input;
        }

        #endregion

        #region Translations / Strings

        /// <summary>
        /// These will be loaded with the language files content
        /// if the selected lang file is not found, it will first try to load en(us).xml as a backup
        /// if that also fails it will use the hardcoded strings as a last resort.
        /// </summary>

        // A
        public static string Actor = "Actor";
        public static string Actors = "Actors";
        public static string AllMovies = "All Movies";
        public static string AllFiles = "All Files";
        public static string AudioChannels = "Audio Channels";
        public static string AudioFormat = "Audio Format";
        public static string AspectRatio = "Aspect Ratio";

        // B
        public static string BadCategory = "An external plugin or skin tried\nto open a nonexistent Moving\nPictures category!";
        public static string BadMovie = "An external plugin or skin tried\nto open a nonexistent movie in\nMoving Pictures!";
        public static string BadParam = "An external plugin or skin passed\nMoving Pictures an invalid paramater!";
        public static string Backdrop = "Backdrop";
        public static string BeginsWith = "Begins With";
        public static string BelongsToCollection = "Belongs To Collection";


        // C
        public static string Cancel = "Cancel";
        public static string CannotDeleteOffline = "Not able to delete {0}\n because the file is offline";
        public static string CannotDeleteReadOnly = "Cannot delete a read-only movie.\nWould you like Moving Pictures to ignore this movie?";
        public static string CastAndCrew = "Cast and Crew";
        public static string CategoryEmptyDescription = "There are currently no movies\nlisted in this category.";
        public static string CategoryEmptyHeader = "Category is empty";
        public static string Certification = "Certification";
        public static string ChangeLayout = "Change Layout";
        public static string ChangeView = "Change View";
        public static string CheckForMissingArtwork = "Check for Missing Artwork Online";
        public static string Collection = "Collection";
        public static string Collections = "Collections";
        public static string Completed = "Completed";
        public static string CompletedFiles = "Completed Files";
        public static string ContinueToNextPartBody = "Do you wish to continue with part {0}?";
        public static string ContinueToNextPartHeader = "Continue to next part?";
        public static string CycleCoverArt = "Cycle Cover-Art";
        public static string CycleLayout = "Cycle Layout";
        public static string CycleView = "Cycle View";
        public static string Cover = "Cover";
        public static string CoverFlowLayout = "Coverflow Layout";
        public static string CoverFlowView = "Coverflow View";

        // D
        public static string DateAdded = "Date Added";
        public static string DateDay = "day";
        public static string DateDays = "days";
        public static string DateMonth = "month";
        public static string DateMonths = "months";
        public static string DatePartAgo = "{0} {1} ago";
        public static string DatePartWithinDays = "Last {0} days";
        public static string DatePartWithinMonths = "Last {0} months";
        public static string DatePartWithinWeeks = "Last {0} weeks";
        public static string DatePartWithinYears = "Last {0} years";
        public static string DateWeek = "week";
        public static string DateWeeks = "weeks";
        public static string DateYear = "year";
        public static string DateYears = "years";
        public static string DecadeShort = "{0}0s";
        public static string DeleteFailed = "Delete Failed";
        public static string DeleteMovie = "Delete Movie";
        public static string Director = "Director";
        public static string Directors = "Directors";
        public static string DownAbbreviation = "(dn)";
        public static string DoYouWantToDelete = "Do you want to permanently delete\n{0}\nfrom your hard drive?";

        // E
        public static string EarlierThisYear = "Earlier This Year";
        public static string Error = "Error";

        // F
        public static string FailedMountingImage = "Sorry, failed mounting DVD Image";
        public static string FilePath = "File Path";
        public static string FileSize = "File Size";
        public static string FilesNeedingAttention = "Files Needing Attention";
        public static string FilmstripLayout = "Filmstrip Layout";
        public static string FilmstripView = "Filmstrip View";
        public static string FilterBy = "Filter By";
        public static string FrameRate = "Frame Rate";
        public static string Friday = "Friday";
        public static string Future = "Future";

        // G
        public static string Genre = "Genre";
        public static string Genres = "Genres";
        public static string GlobalActions = "Global Actions";

        // I
        public static string IgnoreMovie = "Ignore Movie";
        public static string ImdbId = "IMDb ID";
        public static string Importer = "Movie Importer";
        public static string ImporterDisabled = "Importer Disabled";
        public static string ImporterDisabledMessage = "The importer has been disabled in the\nMediaPortal GUI. Would you like to\nreenable it?";
        public static string ImporterPending = "Importer ({0} Need Attention)";
        public static string InfoUrl = "Info URL";
        public static string InvalidPin = "Invalid PIN code!";
        public static string InvalidVideoDiscFormat = "Either the image file does not contain\na valid video disc format, or your Daemon\nTools MediaPortal configuration is incorrect.";

        // L
        public static string Language = "Language";
        public static string LargeThumbnailLayout = "Large Thumbnail Layout";
        public static string LargeThumbnailView = "Large Thumbnail View";
        public static string LastMonth = "Last Month";
        public static string LastWeek = "Last Week";
        public static string LastYear = "Last Year";
        public static string ListLayout = "List Layout";
        public static string ListView = "List View";
        public static string LockRestrictedMovies = "Lock Restricted Movies";

        // M
        public static string MarkAsUnwatched = "Mark as Unwatched";
        public static string MarkAsWatched = "Mark as Watched";
        public static string MediaInfo = "MediaInfo";
        public static string MediaIsMissing = "The media for the Movie you have selected is missing!\nVery sorry but something has gone wrong...";
        public static string MediaNotAvailableBody = "The media for the movie you have selected is not\ncurrently available. Please insert or connect media\nlabeled: {0}";
        public static string MediaNotAvailableHeader = "Media Not Available";
        public static string MediaType = "Media Type";
        public static string Missing = "Missing";
        public static string MissingExternalPlayerExe = "The executable for HD playback is missing.\nPlease correct the path to the executable.";
        public static string Monday = "Monday";
        public static string MonthName1 = "January";
        public static string MonthName2 = "February";
        public static string MonthName3 = "March";
        public static string MonthName4 = "April";
        public static string MonthName5 = "May";
        public static string MonthName6 = "June";
        public static string MonthName7 = "July";
        public static string MonthName8 = "August";
        public static string MonthName9 = "September";
        public static string MonthName10 = "October";
        public static string MonthName11 = "November";
        public static string MonthName12 = "December";
        public static string Movie = "Movie";
        public static string MovieDetails = "Movie Details";
        public static string MovieOptions = "Movie Options";
        public static string Movies = "Movies";

        // N
        public static string NoImportPathsBody = "You do not have any import paths\ndefined. Please close MediaPortal\nand launch MP Configuration.";
        public static string NoImportPathsHeading = "No Import Paths!";

        // O
        public static string Older = "Older";
        public static string Overview = "Overview";

        // P
        public static string ParentalControlsDisabled = "Parental controls are not enabled.";
        public static string PinCodeHeader = "Unlocking Restricted Content";
        public static string PinCodePrompt = "Please enter PIN code to continue:";
        public static string PlaybackFailed = "Playback is not possible because the '{0}'\nextension is not listed in your mediaportal configuration.\nPlease add this extension or setup an external player\nand try again.";
        public static string PlaybackFailedHeader = "Playback Failed";
        public static string PlayCount = "Play Count";
        public static string PlayMovie = "Play Movie";
        public static string PossibleMatches = "Possible Matches";
        public static string ProblemLoadingSkinFile = "Sorry, there was a problem loading the skin file";

        // R
        public static string Rate = "Rate";
        public static string Rated = "Rated";
        public static string RateFiveStarFive = "Perfect";
        public static string RateFiveStarFour = "Superb";
        public static string RateFiveStarOne = "Terrible";
        public static string RateFiveStarThree = "Good";
        public static string RateFiveStarTwo = "Mediocre";
        public static string RateHeading = "Rate Movie";
        public static string RecentlyAddedMovies = "Recently Added Movies";
        public static string ReleaseDate = "Release Date";
        public static string RemoteNumericAlphabet0 = "";
        public static string RemoteNumericAlphabet1 = "";
        public static string RemoteNumericAlphabet2 = "abc";
        public static string RemoteNumericAlphabet3 = "def";
        public static string RemoteNumericAlphabet4 = "ghi";
        public static string RemoteNumericAlphabet5 = "jkl";
        public static string RemoteNumericAlphabet6 = "mno";
        public static string RemoteNumericAlphabet7 = "pqrs";
        public static string RemoteNumericAlphabet8 = "tuv";
        public static string RemoteNumericAlphabet9 = "wxyz";
        public static string RestoreIgnoredFiles = "Restore Ignored Files";
        public static string ResumeFrom = "Resume from:";
        public static string ResumeFromLast = "Resume movie from last time?";
        public static string Resolution = "Resolution";
        public static string Retry = "Retry";
        public static string Runtime = "Runtime";
        public static string RuntimeHour = "{0} hour";
        public static string RuntimeHours = "{0} hours";
        public static string RuntimeLong = "{0} and {1}";
        public static string RuntimeLongExtended = "{0}, {1} and {2}";
        public static string RuntimeMinute = "{0} minute";
        public static string RuntimeMinutes = "{0} minutes";
        public static string RuntimeSecond = "{0} second";
        public static string RuntimeSeconds = "{0} seconds";
        public static string RuntimeShort = "{0}:{1:00}";
        public static string RuntimeShortExtended = "{0}h{1:00}m{2:00}s";

        // S
        public static string Saturday = "Saturday";
        public static string SendToImporter = "Send to Importer";
        public static string ScanForNewMovies = "Scan for New Movies";
        public static string Score = "Score";
        public static string Search = "Search";
        public static string SearchBy = "Search By";
        public static string SearchForMore = "Search for More Matches";
        public static string SearchNoResults = "There are no search results.";
        public static string SelectCorrectMovie = "Select Correct Movie";
        public static string SelectYourRating = "Select your rating for {0}";
        public static string ShowOnlyUnwatchedMovies = "Show Only Unwatched Movies";
        public static string ShowWatchedAndUnwatchedMovies = "Show Watched and Unwatched Movies";
        public static string SkinDoesNotSupportCategories = "This skin does not support Categories.";
        public static string SkinDoesNotSupportImporter = "This skin does not support the Importer.";
        public static string SkinDoesNotSupportPinDialog = "This skin does not support the Pin Code Dialog.";
        public static string SkinDoesNotSupportRatingDialog = "This skin does not support the Rating Dialog.";
        public static string Source = "Source";
        public static string SortBy = "Sort By";
        public static string SortDirection = "Sort Direction";
        public static string StarRating = "Star Rating";
        public static string Start = "Start";
        public static string Sunday = "Sunday";

        // T
        public static string TechnicalInfo = "Technical Info";
        public static string Theme = "Theme";
        public static string ThisMonth = "This Month";
        public static string ThisWeek = "This Week";
        public static string ThisYear = "This Year";
        public static string ThreeMonthsAgo = "Three Months Ago";
        public static string ThreeWeeksAgo = "Three Weeks Ago";
        public static string ThumbnailLayout = "Thumbnail Layout";
        public static string ThumbnailView = "Thumbnail View";
        public static string Thursday = "Thursday";
        public static string Title = "Title";
        public static string Today = "Today";
        public static string ToggleParentalLock = "Toggle Parental Lock";
        public static string Tuesday = "Tuesday";
        public static string TwoMonthsAgo = "Two Months Ago";
        public static string TwoWeeksAgo = "Two Weeks Ago";

        // U
        public static string Unknown = "Unknown";
        public static string UnlockRestrictedMovies = "Unlock Restricted Movies";
        public static string UnwatchedMovies = "Unwatched Movies";
        public static string UpAbbreviation = "(up)";
        public static string UpdateDetailsFromOnline = "Update Details from Online";
        public static string UpdateMovieDetailsBody = "You are about to refresh all movie metadata, overwriting\nany custom modifications to this film. Do you want\nto continue?";
        public static string UpdateMovieDetailsHeader = "Update Movie Details";

        // V
        public static string VideoFormat = "Video Format";
        public static string ViewMovieDetails = "View Movie Details";
        public static string VirtualDriveHeader = "Virtual drive not ready";
        public static string VirtualDriveMessage = "The virtual drive wasn't ready in time.\nPlease try again or cancel playback.";
        public static string Votes = "votes";

        // W
        public static string WatchedCount = "Watched Count";
        public static string Wednesday = "Wednesday";
        public static string Writer = "Writer";
        public static string Writers = "Writers";

        // Y
        public static string Year = "Year";
        public static string Yesterday = "Yesterday";

        #endregion

    }

}