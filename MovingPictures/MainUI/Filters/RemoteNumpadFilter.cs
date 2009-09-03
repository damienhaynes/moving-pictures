using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;
using Cornerstone.Database.Tables;

namespace MediaPortal.Plugins.MovingPictures.MainUI.Filters {
    public class RemoteNumpadFilter : IFilter<DBMovieInfo> {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public event FilterUpdatedDelegate<DBMovieInfo> Updated;

        private enum FilterAction {
            Contains,
            StartsWith
        }

        private string _listFilterString; // List Filter String
        private FilterAction _listFilterAction; // List Filter Action
        private static Dictionary<string, string> keyMap;

        public RemoteNumpadFilter() {
            _listFilterString = string.Empty;
            _listFilterAction = FilterAction.Contains;
            createKeyMap();
        }

        // todo: if a user initiated language setting change would become available this method 
        // should be listening and trigger on update
        public void createKeyMap() {
            keyMap = new Dictionary<string, string>();
            keyMap.Add("0", Translation.RemoteNumericAlphabet0);
            keyMap.Add("1", Translation.RemoteNumericAlphabet1);
            keyMap.Add("2", Translation.RemoteNumericAlphabet2);
            keyMap.Add("3", Translation.RemoteNumericAlphabet3);
            keyMap.Add("4", Translation.RemoteNumericAlphabet4);
            keyMap.Add("5", Translation.RemoteNumericAlphabet5);
            keyMap.Add("6", Translation.RemoteNumericAlphabet6);
            keyMap.Add("7", Translation.RemoteNumericAlphabet7);
            keyMap.Add("8", Translation.RemoteNumericAlphabet8);
            keyMap.Add("9", Translation.RemoteNumericAlphabet9);
        }

        public HashSet<DBMovieInfo> Filter(ICollection<DBMovieInfo> input) {
            HashSet<DBMovieInfo> results = new HashSet<DBMovieInfo>();

            // if we are not active, just return the inputs.
            if (!Active) {
                if (input is HashSet<DBMovieInfo>)
                    return (HashSet<DBMovieInfo>)input;

                foreach (DBMovieInfo currItem in input)
                    results.Add(currItem);

                return results;
            }

            // If the listFilterString contains characters
            // filter the list using the current filter string

            logger.Debug("List Filter Active: '{0}'", _listFilterString);

            // Filter the list with the specified critera
            foreach (DBMovieInfo currMovie in input) {
                if (_listFilterAction == FilterAction.StartsWith) {
                    if (currMovie.SortBy.ToLower().StartsWith(_listFilterString))
                        results.Add(currMovie);
                }
                else {
                    if (NumPadEncode(currMovie.Title).Contains(_listFilterString))
                        results.Add(currMovie);
                }
            }

            return results;
        }

        public bool Active {
            get { return !String.IsNullOrEmpty(_listFilterString); }
        }

        public string Text { get { return _text; } }
        private string _text = string.Empty;

        public void Clear() {
            _listFilterString = string.Empty;
            _listFilterAction = FilterAction.Contains;
            if (Updated != null)
                Updated(this);
        }

        public bool KeyPress(Char key) {
            if ((key >= '0' && key <= '9') || key == '*' || key == '(' || key == '#' || key == '§') {

                // reset the list filter string
                if (key == '*') {
                    _listFilterString = string.Empty;
                    _listFilterAction = FilterAction.Contains;
                }

                // activate "StartsWith" function
                else if ((_listFilterString == string.Empty) && (key == '0')) {
                    _listFilterAction = FilterAction.StartsWith;
                }

                // Use StartsWith Filter
                else if (_listFilterAction == FilterAction.StartsWith) {
                    _listFilterString = NumPadNext(_listFilterString, key.ToString());
                    _text = "Starting with: " + _listFilterString.ToUpper();
                }
                // Add the numeric code to the list filter string   
                else {
                    // Default
                    _listFilterAction = FilterAction.Contains;
                    _listFilterString += key.ToString();
                    _text = "Filtered";
                }

                if (Updated != null)
                    Updated(this);

                logger.Debug("Active Filter: " + _listFilterString);
                return true;

            }
            return false;
        }

        public static string NumPadNext(string current, string requested) {
            string newValue;

            if (keyMap.ContainsKey(requested))
                newValue = getNextFromRange(keyMap[requested] + requested, current);
            else
                newValue = requested;

            return newValue;
        }

        public static string getNextFromRange(string range, string current) {
            if (current == string.Empty)
                return range[0].ToString();

            int index = range.IndexOf(current) + 1;
            if (index > 0 && range.Length > index)
                return range[index].ToString();
            else
                return range[0].ToString();
        }

        public static string NumPadEncode(string input) {
            string rtn = input.Trim();
            foreach (string key in keyMap.Keys) {
                if (keyMap[key].Length > 0)
                    rtn = Regex.Replace(rtn, @"[" + Regex.Escape(keyMap[key]) + @"]", key, RegexOptions.IgnoreCase);
            }

            return Regex.Replace(rtn, @"\s", "0", RegexOptions.IgnoreCase);
        } 

    }
}
