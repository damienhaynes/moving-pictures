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

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.MovieManager {
    public partial class MovieDetailsSubPane : UserControl, IDBBackedControl {
        private DBMovieInfo movie = null;
        
        public MovieDetailsSubPane() {
            InitializeComponent();
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
                return movie;
            }
            set {
                if (value is DBMovieInfo || value == null)
                    movie = value as DBMovieInfo;

                updateControls();
            }
        }

        #endregion

        private void updateControls() {
            if (movie == null) {
                // if we have no movie selcted (or multiple movies selected) clear out details
                movieDetailsList.DatabaseObject = null;
                movieDetailsList.Enabled = false;

                userMovieDetailsList.DatabaseObject = null;
                userMovieDetailsList.Enabled = false;

                return;
            }

            movieDetailsList.DatabaseObject = movie;
            movieDetailsList.Enabled = true;

            userMovieDetailsList.DatabaseObject = movie.UserSettings[0];
            userMovieDetailsList.Enabled = true;
        }

    }
}
