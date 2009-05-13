using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Configuration;
using System.Xml;
using System.Reflection;
using System.IO;
using NLog;
using System.Threading;
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
        public static string ProblemLoadingSkinFile = "Sorry, there was a problem loading skin file...";
        public static string NoImportPathsHeading = "No Import Paths!";
        public static string NoImportPathsBody = "It doesn't look like you have\ndefined any import paths. You\nshould close MediaPortal and\nlaunch the MediaPortal\nConfiguration Screen to\nconfigure Moving Pictures.";
        public static string ShowAllMovies = "Show All Movies";
        public static string ShowOnlyUnwatchedMovies = "Show Only Unwatched Movies";
        public static string SortBy = "Sort By";
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

        #endregion

        private static string path = string.Empty;
        

        static Translation() {
            string lang = GUILocalizeStrings.GetCultureName(GUILocalizeStrings.CurrentLanguage());
            logger.Info("Using language " + lang);

            path = Config.GetSubFolder(Config.Dir.Language, "MovingPictures");

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            loadTranslations(lang);
        }


        public static int loadTranslations(string lang) {
            XmlDocument doc = new XmlDocument();
            Dictionary<string, string> TranslatedStrings = new Dictionary<string, string>();

            try {
                string langPath = Path.Combine(path, lang + ".xml");
                doc.Load(langPath);
            }
            catch (Exception e) {
                if (lang == "en")
                    return 0; // othwerise we are in an endless loop!
                logger.ErrorException("Cannot find Translation File (or error in xml): " + lang, e);
                logger.Error("Falling back to English");
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
    }

}
