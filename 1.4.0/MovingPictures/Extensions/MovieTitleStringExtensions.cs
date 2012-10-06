using System;
using System.Text.RegularExpressions;
using Cornerstone.Extensions;

namespace MediaPortal.Plugins.MovingPictures.Extensions {

    public static class MovieTitleStringExtensions {

        // Regular expression pattern that matches a selection of non-word characters
        private const string rxMatchNonWordCharacters = @"[^\w]";

        /// <summary>
        /// Returns the string converted to a sortable string (The String -> String, The)
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string ToSortable(this String self) {
            Regex expr = new Regex(@"^(" + MovingPicturesCore.Settings.ArticlesForRemoval + @")\s(.+)", RegexOptions.IgnoreCase);
            return expr.Replace(self, "$2, $1").Trim();
        }

        /// <summary>
        /// Returns the string as converted from a sortable string (String, The -> The String)
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string FromSortable(this String self) {
            Regex expr = new Regex(@"(.+?)(?:, (" + MovingPicturesCore.Settings.ArticlesForRemoval + @"))?\s*$", RegexOptions.IgnoreCase);
            return expr.Replace(self, "$2 $1").Trim();
        }

        /// <summary>
        /// Filters non descriptive words/characters from a title so that only keywords remain.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string ToKeywords(this String self) {

            // Remove articles and non-descriptive words
            string newTitle = Regex.Replace(self, @"\b(" + MovingPicturesCore.Settings.ArticlesForRemoval + @")\b", "", RegexOptions.IgnoreCase);
            newTitle = Regex.Replace(newTitle, @"\b(and|or|of|und|en|et|y)\b", "", RegexOptions.IgnoreCase);

            // Replace non-word characters with spaces
            newTitle = Regex.Replace(newTitle, rxMatchNonWordCharacters, " ");

            // Remove double spaces and return the keywords
            return newTitle.TrimWhiteSpace();
        }

        /// <summary>
        /// Returns and converts the string into a common format.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string Equalize(this String self) {
            if (self == null) return string.Empty;

            // Convert title to lowercase culture invariant
            string newTitle = self.ToLowerInvariant();

            // Swap article
            newTitle = newTitle.FromSortable();

            // Replace non-descriptive characters with spaces
            newTitle = Regex.Replace(newTitle, rxMatchNonWordCharacters, " ");

            // Equalize: Convert to base character string
            newTitle = newTitle.RemoveDiacritics();

            // Equalize: Common characters with words of the same meaning
            newTitle = Regex.Replace(newTitle, @"\b(and|und|en|et|y)\b", " & ");

            // Equalize: Roman Numbers To Numeric
            newTitle = Regex.Replace(newTitle, @"\si(\b)", @" 1$1");
            newTitle = Regex.Replace(newTitle, @"\sii(\b)", @" 2$1");
            newTitle = Regex.Replace(newTitle, @"\siii(\b)", @" 3$1");
            newTitle = Regex.Replace(newTitle, @"\siv(\b)", @" 4$1");
            newTitle = Regex.Replace(newTitle, @"\sv(\b)", @" 5$1");
            newTitle = Regex.Replace(newTitle, @"\svi(\b)", @" 6$1");
            newTitle = Regex.Replace(newTitle, @"\svii(\b)", @" 7$1");
            newTitle = Regex.Replace(newTitle, @"\sviii(\b)", @" 8$1");
            newTitle = Regex.Replace(newTitle, @"\six(\b)", @" 9$1");

            // Remove the number 1 from the end of a title string
            newTitle = Regex.Replace(newTitle, @"\s(1)$", "");


            // Remove double spaces and return the cleaned title
            return newTitle.TrimWhiteSpace();
        }

    }

}
