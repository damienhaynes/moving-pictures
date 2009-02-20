using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Plugins.MovingPictures.Database;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    public partial class SearchStringPopup : Form {
        
        private MovieMatch movieMatch;

        public SearchStringPopup() {
            InitializeComponent();
        }

        public SearchStringPopup(MovieMatch match) {
            InitializeComponent();
            foreach (DBLocalMedia currFile in match.LocalMedia) {
                fileListBox.Items.Add(currFile.File);
            }
            movieMatch = match;
            uxTitle.Text = movieMatch.Signature.Title;
            uxYear.Text = movieMatch.Signature.Year.ToString();
            uxImdbId.Text = movieMatch.Signature.ImdbId;
        }

        private void okButton_Click(object sender, EventArgs e) {
            movieMatch.Signature.Title = uxTitle.Text;
            int year = 0;
            if (int.TryParse(uxYear.Text, out year))
                movieMatch.Signature.Year = year;
            movieMatch.Signature.ImdbId = uxImdbId.Text;
        }
      
    }
}
