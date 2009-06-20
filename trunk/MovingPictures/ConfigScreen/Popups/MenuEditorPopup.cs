using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.Database;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    public partial class MenuEditorPopup : Form {
        public MenuEditorPopup() {
            InitializeComponent();

            MenuTree.SelectedNodeChanged += new DBNodeEventHandler(MenuTree_SelectedNodeChanged);
            nodeSettingsPanel.DBManager = MovingPicturesCore.DatabaseManager;
            MenuTree.DBManager = MovingPicturesCore.DatabaseManager;
        }

        void MenuTree_SelectedNodeChanged(IDBNode node, Type type) {
            DBNode<DBMovieInfo> movieNode = (DBNode<DBMovieInfo>)node;

            nodeSettingsPanel.Node = movieNode;
            movieNodeSettingsPanel.Node = movieNode;

        }

        private void MenuEditorPopup_FormClosing(object sender, FormClosingEventArgs e) {

        }

        private void okButton_Click(object sender, EventArgs e) {
            Close();
        }
    }
}
