using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Cornerstone.GUI.Controls;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.Database;
using System.Diagnostics;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.MovieManager {
    public partial class FileDetailsSubPane : UserControl, IDBBackedControl {
        private DBMovieInfo movie = null;
        
        public FileDetailsSubPane() {
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

        public void playSelected() {
            if (fileList.SelectedItem == null || !((DBLocalMedia)fileList.SelectedItem).IsAvailable)
                return;

            ProcessStartInfo processInfo = new ProcessStartInfo(((DBLocalMedia)fileList.SelectedItem).File.FullName);
            Process.Start(processInfo);
        }

        private void updateControls() {
            // if no object selected (or multiple objects selected) clear details
            if (movie == null) {
                fileList.Items.Clear();
                fileList.Enabled = false;
                fileDetailsList.DatabaseObject = null;
                return;
            }

            // populate file list combo
            fileList.Items.Clear();
            List<DBLocalMedia> files = new List<DBLocalMedia>();
            files.AddRange(movie.LocalMedia);
            files.Sort(new DBLocalMediaComparer());
            foreach (DBLocalMedia currFile in files)
                fileList.Items.Add(currFile);

            // select first file            
            if (fileList.Items.Count > 0) {
                fileList.SelectedIndex = 0;
            }

            fileList.Enabled = true;
        }

        private void fileList_SelectedIndexChanged(object sender, EventArgs e) {
            if (fileList.SelectedItem != null)
                fileDetailsList.DatabaseObject = (DBLocalMedia)fileList.SelectedItem;
        }

        private void fileUpButton_Click(object sender, EventArgs e) {
            int index = fileList.SelectedIndex;
            if (index == 0)
                return;

            // swap the part# with the file above
            int partA = ((DBLocalMedia)fileList.Items[index]).Part;
            int partB = ((DBLocalMedia)fileList.Items[index - 1]).Part;
            ((DBLocalMedia)fileList.Items[index]).Part = partB;
            ((DBLocalMedia)fileList.Items[index - 1]).Part = partA;

            movie.LocalMedia.Sort(new DBLocalMediaComparer());
            movie.Commit();

            updateControls();
            fileList.SelectedIndex = index - 1;
        }

        private void fileDownButton_Click(object sender, EventArgs e) {
            int index = fileList.SelectedIndex;
            if (index == fileList.Items.Count - 1)
                return;

            // swap the part# with the file above
            int partA = ((DBLocalMedia)fileList.Items[index]).Part;
            int partB = ((DBLocalMedia)fileList.Items[index + 1]).Part;
            ((DBLocalMedia)fileList.Items[index]).Part = partB;
            ((DBLocalMedia)fileList.Items[index + 1]).Part = partA;

            movie.LocalMedia.Sort(new DBLocalMediaComparer());
            movie.Commit();

            updateControls();
            fileList.SelectedIndex = index + 1;
        }

    }
}
