using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    public partial class SearchStringPopup : Form {
        public SearchStringPopup() {
            InitializeComponent();
        }

        public SearchStringPopup(MediaMatch match) {
            InitializeComponent();
            foreach (DBLocalMedia currFile in match.LocalMedia) {
                fileListBox.Items.Add(currFile.File);
            }

            searchStrTextBox.Text = match.SearchString;
        }

        public string GetSearchString() {
            return searchStrTextBox.Text;
        }
    }
}
