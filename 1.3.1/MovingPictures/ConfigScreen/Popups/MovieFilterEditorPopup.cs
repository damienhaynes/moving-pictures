using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures;
using Cornerstone.Database.Tables;
using Cornerstone.GUI.Filtering;
using Cornerstone.GUI;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    public partial class MovieFilterEditorPopup : Form {

        public bool ShowHelpButton {
            get { return helpButton.Visible; }
            set { helpButton.Visible = value; }
        }

        public HelpActionDelegate HelpAction {
            get;
            set;
        }

        public FilterEditorPane FilterPane {
            get {
                return filterEditorPane1;
            }
        }

        public MovieFilterEditorPopup() {
            InitializeComponent();
            filterEditorPane1.DBManager = MovingPicturesCore.DatabaseManager;
            helpButton.Visible = false;
        }

        private void TesterFrame_Load(object sender, EventArgs e) {
            if (!DesignMode) {
                //foreach (DBMovieInfo movie in DBMovieInfo.GetAll())
                //    this.dbObjectListEditor1.DatabaseObjects.Add(movie);
            }
        }

        private void okButton_Click(object sender, EventArgs e) {
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e) {
            Close();
        }

        private void helpButton_Click(object sender, EventArgs e) {
            if (HelpAction != null)
                HelpAction(this);
        }


    }
}
