﻿using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Configuration;
using System.Xml;
using System.Reflection;
using System.IO;
using NLog;
using System.Threading;
using System.Globalization;
using MediaPortal.GUI.Library;

namespace MediaPortal.Plugins.MovingPictures.MainUI {
    public static class Translation {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// These will be loaded with the language files content
        /// if the selected lang file is not found, it will first try to load en(us).xml as a backup
        /// if that also fails it will use the hardcoded strings as a last resort.
        /// </summary>
        #region Translatable Fields
        public static string ProblemLoadingSkinFile = "Sorry, there was a problem loading the skin file";
        public static string SkinDoesNotSupportRatingDialog = "This skin does not support the Rating Dialog.";
        public static string SkinDoesNotSupportPinDialog = "This skin does not support the Pin Code Dialog.";
        public static string NoImportPathsHeading = "No Import Paths!";
        public static string NoImportPathsBody = "It doesn't look like you have\ndefined any import paths. You\nshould close MediaPortal and\nlaunch the MediaPortal\nConfiguration Screen to\nconfigure Moving Pictures.";
        public static string ShowWatchedAndUnwatchedMovies = "Show Watched and Unwatched Movies";
        public static string ShowOnlyUnwatchedMovies = "Show Only Unwatched Movies";
        public static string SortBy = "Sort By";
        public static string FilterBy = "Filter By";
        public static string AllMovies = "All Movies";
        public static string ChangeView = "Change View";
        public static string ListView = "List View";
        public static string ThumbnailView = "Thumbnail View";
        public static string LargeThumbnailView = "Large Thumbnail View";
        public static string FilmstripView = "Filmstrip View";
        public static string UpdateDetailsFromOnline = "Update Details from Online";
        public static string CycleCoverArt = "Cycle Cover-Art";
        public static string CheckForMissingArtwork = "Check for Missing Artwork Online";
        public static string MarkAsUnwatched = "Mark as Unwatched";
        public static string MarkAsWatched = "Mark as Watched";
        public static string DeleteMovie = "Delete Movie";
        public static string MovieOptions = "Movie Options";
        public static string LockRestrictedMovies = "Lock Restricted Movies";
        public static string PinCodeHeader = "Unlocking Restricted Content";
        public static string PinCodePrompt = "Please enter PIN code to continue:";
        public static string InvalidPin = "Invalid PIN code!";
        public static string ParentalControlsDisabled = "Parental controls are not enabled.";
        public static string UnlockRestrictedMovies = "Unlock Restricted Movies";
        public static string CannotDeleteReadOnly = "Cannot delete a read-only movie.\nWould you like Moving Pictures to ignore this movie?";
        public static string CannotDeleteOffline = "Not able to delete {0}\n because the file is offline";
        public static string DoYouWantToDelete = "Do you want to permanently delete\n{0}\nfrom your hard drive?";
        public static string DeleteFailed = "Delete Failed";
        public static string UpdateMovieDetailsHeader = "Update Movie Details";
        public static string UpdateMovieDetailsBody = "You are about to refresh all movie metadata, overwriting\nany custom modifications to this film. Do you want\nto continue?";
        public static string MediaNotAvailableHeader = "Media Not Available";
        public static string MediaNotAvailableBody = "The media for the movie you have selected is not\ncurrently available. Please insert or connect media\nlabeled: {0}";
        public static string VirtualDriveHeader = "Virtual drive not ready";
        public static string VirtualDriveMessage = "The virtual drive wasn't ready in time.\nPlease try again or cancel playback.";
        public static string Retry = "Retry";
        public static string Cancel = "Cancel";
        public static string Error = "Error";
        public static string MediaIsMissing = "The media for the Movie you have selected is missing!\nVery sorry but something has gone wrong...";
        public static string FailedMountingImage = "Sorry, failed mounting DVD Image";
        public static string PlaybackFailedHeader = "Playback Failed";
        public static string PlaybackFailed = "Playback is not possible because the '{0}'\nextension is not listed in your mediaportal configuration.\nPlease add this extension or setup an external player\nand try again.";
        public static string MissingExternalPlayerExe = "The executable for HD playback is missing.\nPlease correct the path to the executable.";
        public static string ResumeFromLast = "Resume movie from last time?";
        public static string ResumeFrom = "Resume from:";
        public static string InvalidVideoDiscFormat = "Either the image file does not contain\na valid video disc format, or your Daemon\nTools MediaPortal configuration is incorrect.";
        public static string ContinueToNextPartHeader = "Continue to next part?";
        public static string ContinueToNextPartBody = "Do you wish to continue with part {0}?";
        public static string Rate = "Rate";
        public static string SelectYourRating = "Select your rating for {0}";
		public static string RateHeading = "Rate Movie";
        public static string DownAbbreviation = "(dn)";
        public static string UpAbbreviation = "(up)";


        // friendly names for sorting
        public static string Title = "Title";
        public static string DateAdded = "Date Added";
        public static string Year = "Year";
        public static string Certification = "Certification";
        public static string Language = "Language";
        public static string Score = "Score";
        public static string Runtime = "Runtime";
        public static string FilePath = "File Path";

        // group names for the date added sorting
        public static string Future = "Future";
        public static string Today = "Today";
        public static string Yesterday = "Yesterday";
        public static string Sunday = "Sunday";
        public static string Monday = "Monday";
        public static string Tuesday = "Tuesday";
        public static string Wednesday = "Wednesday";
        public static string Thursday = "Thursday";
        public static string Friday = "Friday";
        public static string Saturday = "Saturday";
        public static string LastWeek = "Last Week";
        public static string TwoWeeksAgo = "Two Weeks Ago";
        public static string ThreeWeeksAgo = "Three Weeks Ago";
        public static string LastMonth = "Last Month";
        public static string TwoMonthsAgo = "Two Months Ago";
        public static string ThreeMonthsAgo = "Three Months Ago";
        public static string EarlierThisYear = "Earlier This Year";
        public static string LastYear = "Last Year";
        public static string Older = "Older";
		
		// Rate Movie Descriptions - 5 Stars
		public static string RateFiveStarOne = "Terrible";
		public static string RateFiveStarTwo = "Mediocre";
		public static string RateFiveStarThree = "Good";
		public static string RateFiveStarFour = "Superb";
		public static string RateFiveStarFive = "Perfect";
                
        public static string RuntimeHour = "{0} hour";
        public static string RuntimeHours = "{0} hours";
        public static string RuntimeMinute = "{0} minute";
        public static string RuntimeMinutes = "{0} minutes";
        public static string RuntimeSecond = "{0} second";
        public static string RuntimeSeconds = "{0} seconds";
        
        public static string RuntimeShort = "{0}:{1:00}";
        public static string RuntimeShortExtended = "{0}h{1:00}m{2:00}s";
        public static string RuntimeLong = "{0} and {1}";
        public static string RuntimeLongExtended = "{0}, {1} and {2}";

        // Remote Control Numeric Alphabet

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

        // Year
        public static string DecadeShort = "{0}0s";
        public static string Unknown = "Unknown";

        // Date added
        public static string DateDay = "day";
        public static string DateDays = "days";
        public static string DateWeek = "week";
        public static string DateWeeks = "weeks";
        public static string DateMonth = "month";
        public static string DateMonths = "months";
        public static string DateYear = "year";
        public static string DateYears = "years";
        public static string DatePartThis = "This {0}";
        public static string DatePartLast = "Last {0}";
        public static string DatePartWithin = "Last {0} {1}";
        public static string DatePartAgo = "{0} {1} ago";

        // Categories
        public static string CategoryEmptyHeader = "Category is empty";
        public static string CategoryEmptyDescription = "There are currently no movies\nlisted in this category.";

        #endregion

        private static string path = string.Empty;
        

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

            loadTranslations(lang);
        }


        public static int loadTranslations(string lang) {
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

                return loadTranslations("en");
            }
            foreach (XmlNode stringEntry in doc.DocumentElement.ChildNodes) {
                if (stringEntry.NodeType == XmlNodeType.Element)
                    try {
                        TranslatedStrings.Add(stringEntry.Attributes.GetNamedItem("Field").Value, stringEntry.InnerText);
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
                    logger.Info("Translation not found for field: {0}.  Using hard-coded English default.", fi.Name);
            }
            return TranslatedStrings.Count;
        }

        public static string GetByName(string name)
        {
            Type TransType = typeof(Translation);
            FieldInfo fi = TransType.GetField(name, BindingFlags.Public | BindingFlags.Static);
            return fi.GetValue(TransType).ToString();
        }

        public static string GetByName(string name, params object[] args) {
            return String.Format(GetByName(name), args);
        }
    }

}