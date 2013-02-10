using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Database.Tables;
using Cornerstone.Database;
using Cornerstone.Database.CustomTypes;
using System.Text.RegularExpressions;

namespace Cornerstone.Tools.Search {
    public class LevenshteinSubstringSearcher<T>: AbstractSearcher<T> where T: DatabaseTable {

        private struct IndexEntry {

            public IndexEntry(T item) {
                this.Item = item;
                Content = new List<string>();
            }

            public T Item;
            public List<string> Content;

            public override int GetHashCode() {
                return Item.GetHashCode();
            }
        }

        private HashSet<IndexEntry> index = new HashSet<IndexEntry>();
        private static Regex cleaner = new Regex(@"[^\w\s]", RegexOptions.IgnoreCase);

        public LevenshteinSubstringSearcher(DatabaseManager db, ICollection<DBField> fields): 
            base(db, fields) {
        }

        public LevenshteinSubstringSearcher(DatabaseManager db, string[] fieldNames) :
            base(db, fieldNames) {
        }

        public override List<SearchResult> Search(string searchStr) {
            List<SearchResult> results = new List<SearchResult>();
            string cleanSearchStr = cleaner.Replace(searchStr, "").ToLower().Trim();
            
            foreach (IndexEntry currEntry in index) {
                SearchResult result = new SearchResult();
                result.Item = currEntry.Item;

                result.Score = int.MaxValue;
                foreach (string currContent in currEntry.Content) {
                    int newScore = Score(cleanSearchStr, currContent);
                    if (newScore < result.Score)
                        result.Score = newScore;
                }

                results.Add(result);
            }

            results.RemoveAll(r => r.Score > 2);
            return results.OrderBy(r => r.Score).ToList();
        }

        private int Score(string searchStr, string target) {
            int bestScore = int.MaxValue;
            int position = 0;
            int lastWhiteSpace = -1;

            string cleanTarget = cleaner.Replace(target, "").ToLower().Trim();

            // start off with a full string compare
            bestScore = AdvancedStringComparer.Levenshtein(searchStr, cleanTarget);

            // step through the movie title and try to match substrings of the same length as the search string
            while (position + searchStr.Length <= cleanTarget.Length) {
                string targetSubStr = cleanTarget.Substring(position, searchStr.Length);

                // base score
                int currScore = AdvancedStringComparer.Levenshtein(searchStr, targetSubStr);

                // penalty if the match starts mid word
                if (position - lastWhiteSpace > 1) currScore += 1;

                // penalty if the match ends mid word
                int trailingPos = position + searchStr.Length;
                if (trailingPos < cleanTarget.Length) {
                    char trailingChar = cleanTarget[trailingPos];
                    if (!char.IsWhiteSpace(trailingChar) && !char.IsPunctuation(trailingChar))
                        currScore++;
                }

                // penalty if it is a substring match
                currScore++;

                // store our new score as needed, upate state variables and move on
                if (bestScore > currScore) bestScore = currScore;
                if (targetSubStr.Length > 0 && (char.IsWhiteSpace(targetSubStr[0]) || char.IsPunctuation(targetSubStr[0]))) lastWhiteSpace = position;
                position++;
            }

            return bestScore;
        }

        public override void Add(T item) {
            IndexEntry newEntry = new IndexEntry(item);

            foreach (DBField currField in SearchFields) {
                object value = currField.GetValue(item);
                if (value == null) continue;

                if (value is StringList) {
                    foreach (string currStr in (StringList)value)
                        newEntry.Content.Add(currStr);
                }
                else {
                    newEntry.Content.Add(value.ToString());
                }
            }

            index.Add(newEntry);
        }

        public override void Remove(T item) {
            index.Remove(new IndexEntry(item));
        }

        public override void Clear() {
            base.Clear();

            index.Clear();
        }
    }
}
