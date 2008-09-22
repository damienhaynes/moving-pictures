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
        public SearchStringPopup() {
            InitializeComponent();
        }

        public SearchStringPopup(MovieMatch match) {
            InitializeComponent();
            foreach (DBLocalMedia currFile in match.LocalMedia) {
                fileListBox.Items.Add(currFile.File);
            }

          searchStrTextBox.Text = match.Signature.Title + ((match.Signature.Year > 0) ? " " + match.Signature.Year : "") + ((match.Signature.ImdbId != null) ? " " + match.Signature.ImdbId : "");  
        }

        public string GetSearchString() {
            return searchStrTextBox.Text;
        }
    }
}
