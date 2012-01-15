using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Cornerstone.GUI.Controls;
using MediaPortal.Plugins.MovingPictures.Database;
using Cornerstone.Database.Tables;
using Cornerstone.Database;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.MovieManager {
    public partial class MovieDetailsSubPane : UserControl, IDBBackedControl {

        public event FieldChangedListener FieldChanged;
        
        public MovieDetailsSubPane() {
            InitializeComponent();

            movieDetailsList.FieldChanged += new FieldChangedListener(movieDetailsList_FieldChanged);
        }

        void movieDetailsList_FieldChanged(DatabaseTable obj, DBField field, object value) {
            movieDetailsList.RepopulateValues();
            if (FieldChanged != null) FieldChanged(obj, field, value);
        }

        #region IDBBackedControl Members

        public Type Table {
            get {
                return typeof(DBMovieInfo);
            }
            set {
                return;
            }
        }

        public DatabaseTable DatabaseObject {
            get {
                return _movie;
            }
            set {
                if (value is DBMovieInfo || value == null)
                    _movie = value as DBMovieInfo;

                updateControls();
            }
        } private DBMovieInfo _movie = null;

        #endregion

        private void updateControls() {
            if (_movie == null) {
                // if we have no movie selcted (or multiple movies selected) clear out details
                movieDetailsList.DatabaseObject = null;
                movieDetailsList.Enabled = false;

                userMovieDetailsList.DatabaseObject = null;
                userMovieDetailsList.Enabled = false;

                return;
            }

            movieDetailsList.DatabaseObject = _movie;
            movieDetailsList.Enabled = true;

            userMovieDetailsList.DatabaseObject = _movie.UserSettings[0];
            userMovieDetailsList.Enabled = true;
        }

    }
}
