using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using System.Text.RegularExpressions;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.MainUI.MovieBrowser {
    public class KeyboardFilter : IBrowserFilter {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        public event FilterUpdatedDelegate Updated;

        private string _listFilterString; // List Filter String

        public KeyboardFilter() {
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
                    return item.Title.Contains(_listFilterString);
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
            if ((key >= 'a' && key <= 'z') || key == '\b' || key == '§') {

                // reset the list filter string
                if (key == '\b') // = backspace 
                    _listFilterString = string.Empty;
                
                // add the letter to the list filter
                else 
                    _listFilterString += key.ToString();

                if (Updated != null)
                    Updated(this);

                logger.Debug("Active Filter: " + _listFilterString);
                return true;

            }
            return false;
        }    

    }
}
