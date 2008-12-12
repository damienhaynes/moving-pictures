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
    public partial class ManualAssignPopup : Form {
        public ManualAssignPopup() {
            InitializeComponent();
        }

        public ManualAssignPopup(MovieMatch match) {
            InitializeComponent();
            foreach (DBLocalMedia currFile in match.LocalMedia) {
                fileListBox.Items.Add(currFile.File);
            }
            uxTitle.Text = match.Signature.Title;
            uxYear.Text = match.Signature.Year.ToString(); 
        }

        public string Title { get { return uxTitle.Text; } }
        public int? Year { get { return Int32.Parse(uxYear.Text); } }      

    }
}
