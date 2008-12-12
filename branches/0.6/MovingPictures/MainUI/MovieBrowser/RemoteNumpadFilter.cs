using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using System.Text.RegularExpressions;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.MainUI.MovieBrowser {
    public class RemoteNumpadFilter : IBrowserFilter {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        public event FilterUpdatedDelegate Updated;

        private string _listFilterString; // List Filter String

        public RemoteNumpadFilter() {
            _listFilterString = string.Empty;
        }

        public List<DBMovieInfo> Filter(List<DBMovieInfo> input) {
            // If the listFilterString contains characters
            // filter the list using the current filter string
            if (Active) {
                logger.Debug("List Filter Active: '{0}'", _listFilterString);

                // This is the "contains" predicate
                // @todo: StartsWith, EndsWith etc.. ?
                Predicate<DBMovieInfo> predicate = delegate(DBMovieInfo item) {
                    return (NumPadEncode(item.Title).Contains(_listFilterString));
                };
                
                // Filter the list with the specified critera
                List<DBMovieInfo> filteredList = input.FindAll(predicate);
                return filteredList;
            }
            else {
                // fill facade (facade) with all items.
                return input;
            }
        }

        public bool Active {
            get { return !String.IsNullOrEmpty(_listFilterString); }
        }

        public void Clear() {
            _listFilterString = string.Empty;
            
            if (Updated != null)
                Updated(this);
        }

        public bool KeyPress(Char key) {
            if ((key >= '0' && key <= '9') || 
                key == '*' || key == '(' || key == '#' || key == '§') {

                // reset the list filter string
                if (key == '*') 
                    _listFilterString = string.Empty;
                
                // or add the numeric code to the list filter string
                else 
                    _listFilterString += NumPadEncode(key.ToString());

                if (Updated != null)
                    Updated(this);


                logger.Debug("Active Filter: " + _listFilterString);
                return true;

            }
            return false;
        }
    

        public static string NumPadEncode(string input) {
            string rtn = input.Trim();
            rtn = Regex.Replace(rtn, @"[abc]", "2", RegexOptions.IgnoreCase);
            rtn = Regex.Replace(rtn, @"[def]", "3", RegexOptions.IgnoreCase);
            rtn = Regex.Replace(rtn, @"[ghi]", "4", RegexOptions.IgnoreCase);
            rtn = Regex.Replace(rtn, @"[jkl]", "5", RegexOptions.IgnoreCase);
            rtn = Regex.Replace(rtn, @"[mno]", "6", RegexOptions.IgnoreCase);
            rtn = Regex.Replace(rtn, @"[pqrs]", "7", RegexOptions.IgnoreCase);
            rtn = Regex.Replace(rtn, @"[tuv]", "8", RegexOptions.IgnoreCase);
            rtn = Regex.Replace(rtn, @"[wxyz]", "9", RegexOptions.IgnoreCase);
            rtn = Regex.Replace(rtn, @"[\s]", "0", RegexOptions.IgnoreCase);
            return rtn;
        }

    }
}
