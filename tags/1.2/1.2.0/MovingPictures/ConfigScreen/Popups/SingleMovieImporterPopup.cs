using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Plugins.MovingPictures.SignatureBuilders;
using MediaPortal.Plugins.MovingPictures.Database;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    public partial class SingleMovieImporterPopup : Form {
        private List<DBLocalMedia> localMedia;
        private MovieMatch mediaMatch;
        private bool updating = false;

        public SingleMovieImporterPopup() {
            InitializeComponent();
        }

        public SingleMovieImporterPopup(DBMovieInfo movie) {
            InitializeComponent();
            localMedia = new List<DBLocalMedia>(movie.LocalMedia);
            movie.Delete();
        }

        public SingleMovieImporterPopup(List<DBLocalMedia> files) {
            InitializeComponent();
            localMedia = new List<DBLocalMedia>(files);
        }

        private void SingleMovieImporterPopup_Load(object sender, EventArgs e) {
            if (!DesignMode) {
                // setup listener and start Importer if it's not already running
                MovingPicturesCore.Importer.MovieStatusChanged += new MovieImporter.MovieStatusChangedHandler(movieStatusChangedListener);
                MovingPicturesCore.Importer.Start();

                // populate onscreen list box with files to be processed
                fileListBox.Items.Clear();
                foreach (DBLocalMedia currFile in localMedia) {
                    fileListBox.Items.Add(currFile.File);
                }

                MovingPicturesCore.Importer.Reprocess(localMedia);
            }
        }
        
        private void movieStatusChangedListener(MovieMatch obj, MovieImporterAction action) {
            // we dont care about any messages except those about media matches
            if (obj == null)
                return;
            
            // This ensures we are thread safe. Makes sure this method is run by
            // the thread that created this panel.
            if (InvokeRequired) {
                Delegate method = new MovieImporter.MovieStatusChangedHandler(movieStatusChangedListener);
                object[] parameters = new object[] { obj, action };
                this.Invoke(method, parameters);
                return;
            }

            // if this is not a message about our files ignore it
            if (!obj.LocalMedia.Contains(localMedia[0])) 
                return;

            // store the current match
            mediaMatch = obj;

            switch (action) {
                case MovieImporterAction.ADDED:
                case MovieImporterAction.PENDING:
                    statusLabel.Text = "Possible match retrieval pending...";
                    break;
                case MovieImporterAction.GETTING_MATCHES:
                    statusLabel.Text = "Retrieving possible matches...";
                    break;
                case MovieImporterAction.NEED_INPUT:
                    updateCombo();

                    // auto choose the default (mainly to pull in metadata. we can 
                    // easily undo if the user picks another movie.
                    mediaMatch.Selected = (PossibleMatch)possibleMatchesCombo.SelectedItem;
                    mediaMatch.HighPriority = true;
                    MovingPicturesCore.Importer.Approve(mediaMatch);

                    break;
                case MovieImporterAction.APPROVED:
                    statusLabel.Text = "Close match found...";
                    updateCombo();
                    break;
                case MovieImporterAction.GETTING_DETAILS:
                    statusLabel.Text = "Retrieving movie details...";
                    break;
                case MovieImporterAction.COMMITED:
                    statusLabel.Text = "Match commited...";
                    movieDetails.DatabaseObject = mediaMatch.Selected.Movie;
                    break;
            }
        }

        private void updateCombo() {
            updating = true;
            possibleMatchesCombo.Items.Clear();
            foreach (PossibleMatch currMatch in mediaMatch.PossibleMatches)
                possibleMatchesCombo.Items.Add(currMatch);

            // select the "Selected" possible match in the combo box
            possibleMatchesCombo.SelectedIndex = possibleMatchesCombo.Items.IndexOf(mediaMatch.Selected);
            updating = false;
        }

        private void rescanButton_Click(object sender, EventArgs e) {
            MovieSignature signature = new MovieSignature();
            signature.Title = possibleMatchesCombo.Text;
            mediaMatch.Signature = signature;
            MovingPicturesCore.Importer.Reprocess(mediaMatch);
        }

        private void okButton_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void possibleMatchesCombo_SelectedIndexChanged(object sender, EventArgs e) {
            if (!updating) {
                // get rid of the previously sleected movie
                mediaMatch.Selected.Movie.Delete();

                // set new selected movie and put it on the fast track for commital
                mediaMatch.Selected = (PossibleMatch)possibleMatchesCombo.SelectedItem;
                mediaMatch.HighPriority = true;
                MovingPicturesCore.Importer.Approve(mediaMatch);
            }
        }
    }
}
