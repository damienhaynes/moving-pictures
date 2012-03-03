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

            //initialize datagrid for movieMediaFileNames
            movieMediaFileNames.Rows.Add(new object[] { "File(s)", "" });
            movieMediaFileNames.Columns[0].DefaultCellStyle.BackColor = System.Drawing.SystemColors.Control;
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

                movieMediaFileNames.Enabled = false;
                // reset row and tooltip when no movie is selected
                movieMediaFileNames.Rows[0].Cells[1].Value = "";
                movieMediaFileNames.Rows[0].Cells[1].ToolTipText = "";

                return;
            }

            movieDetailsList.DatabaseObject = movie;
            movieDetailsList.Enabled = true;

            userMovieDetailsList.DatabaseObject = movie.UserSettings[0];
            userMovieDetailsList.Enabled = true;

            //populate movieMediaFilename datagrid
            movieMediaFilesPopulate();
        }

        // populate filenames datagrid with details about the selected movie's files
        private void movieMediaFilesPopulate() {
            String mediaFileNames = "";
            String mediaFileFullNames = "";
            
            // populate datagrid with comma delimited list of files with a tooltip with full paths 
            foreach (DBLocalMedia mediaFile in movie.LocalMedia) {
                if (mediaFileNames.Length > 0) mediaFileNames += ", ";
                mediaFileNames += mediaFile.File.Name;
                if (mediaFileFullNames.Length > 0) mediaFileFullNames += "\n";
                mediaFileFullNames += mediaFile.File.FullName;
            }
            
            // add values to datagrid
            movieMediaFileNames.Rows[0].Cells[1].Value = mediaFileNames;
            movieMediaFileNames.Rows[0].Cells[1].ToolTipText = mediaFileFullNames;

            // enable that sucker
            movieMediaFileNames.Enabled = true;
        }
    }
}
