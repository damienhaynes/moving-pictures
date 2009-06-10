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
            Default,
            StartsWith,
            ByYear,
            ByDecade
        }

        private string _listFilterString; // List Filter String
        private FilterAction _listFilterAction; // List Filter Action

        private Dictionary<string, string> _keyMap;

        public RemoteNumpadFilter() {
            _listFilterString = string.Empty;
            _listFilterAction = FilterAction.Default;
            loadKeyMap();            
        }

        private void loadKeyMap() {
            _keyMap = new Dictionary<string, string>();
            _keyMap.Add("2","abc");
            _keyMap.Add("3","def");
            _keyMap.Add("4","ghi");
            _keyMap.Add("5","jkl");
            _keyMap.Add("6","mno");
            _keyMap.Add("7","pqrs");
            _keyMap.Add("8","tuv");
            _keyMap.Add("9","wxyz");
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
                switch (_listFilterAction) {
                    case FilterAction.StartsWith:
                        if (currMovie.SortBy.ToLower().StartsWith(_listFilterString))
                            results.Add(currMovie);
                        break;
                    case FilterAction.ByYear:
                        if (currMovie.Year.ToString() == _listFilterString)
                            results.Add(currMovie);
                        break;
                    case FilterAction.ByDecade:
                        int start = int.Parse(_listFilterString + "0");
                        if ((currMovie.Year >= start) && (currMovie.Year < (start + 10)))
                            results.Add(currMovie);
                        break;
                    default:
                        if (NumPadEncode(currMovie.Title).Contains(_listFilterString))
                            results.Add(currMovie);
                        break;
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
            _listFilterAction = FilterAction.Default;
            if (Updated != null)
                Updated(this);
        }

        public bool KeyPress(Char key) {
            if ((key >= '0' && key <= '9') || key == '*' || key == '(' || key == '#' || key == '§') {

                // reset the list filter string
                if (key == '*') {
                    _listFilterString = string.Empty;
                    _listFilterAction = FilterAction.Default;
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
                    _listFilterAction = FilterAction.Default;
                    _listFilterString += key.ToString();
                    _text = "Filtered";

                    // If this looks like (3 digit part) of a year, try year filters.
                    int year;
                    if ((_listFilterString.Length > 2) && int.TryParse(_listFilterString, out year)) {
                        // exact year
                        if (_listFilterString.Length == 4 && year < DateTime.Now.Year + 2) {
                            _listFilterAction = FilterAction.ByYear;
                            _text = "Year: " + _listFilterString;
                        } // decade
                        else if (_listFilterString.Length == 3 && year > 190 && (year < int.Parse((DateTime.Now.Year + 10).ToString().Substring(0, 3)))) {
                            _listFilterAction = FilterAction.ByDecade;
                            _text = _listFilterString + "0" + "-" + (int.Parse(_listFilterString + "0") + 9).ToString();
                        }
                    }
                }

                if (Updated != null)
                    Updated(this);

                logger.Debug("Active Filter: " + _listFilterString);
                return true;

            }
            return false;
        }

        public string NumPadNext(string current, string requested) {
            string newValue;

            if (_keyMap.ContainsKey(requested))
                newValue = getNextFromRange(_keyMap[requested] + requested, current);
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

        public string NumPadEncode(string input) {
            string rtn = input.Trim();
            foreach (string key in _keyMap.Keys)
                rtn = Regex.Replace(rtn, "[" + _keyMap[key] + "]", key, RegexOptions.IgnoreCase);

            rtn = Regex.Replace(rtn, @"[\s]", "0", RegexOptions.IgnoreCase);
            return rtn;
        }

    }
}
