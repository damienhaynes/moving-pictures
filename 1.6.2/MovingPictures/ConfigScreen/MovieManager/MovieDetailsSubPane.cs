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

            //initialize datagrid for movieMediaFileNames
            movieMediaFileNames.Rows.Add(new object[] { "File(s)", "" });
            movieMediaFileNames.Columns[0].DefaultCellStyle.BackColor = System.Drawing.SystemColors.Control;
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

                movieMediaFileNames.Enabled = false;
                // reset row and tooltip when no movie is selected
                movieMediaFileNames.Rows[0].Cells[1].Value = "";
                movieMediaFileNames.Rows[0].Cells[1].ToolTipText = "";

                return;
            }

            movieDetailsList.DatabaseObject = _movie;
            movieDetailsList.Enabled = true;

            userMovieDetailsList.DatabaseObject = _movie.UserSettings[0];
            userMovieDetailsList.Enabled = true;

            //populate movieMediaFilename datagrid
            movieMediaFilesPopulate();
        }

        // populate filenames datagrid with details about the selected movie's files
        private void movieMediaFilesPopulate() {
            String mediaFileNames = "";
            String mediaFileFullNames = "";
            
            // populate datagrid with comma delimited list of files with a tooltip with full paths 
            foreach (DBLocalMedia mediaFile in _movie.LocalMedia) {
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
