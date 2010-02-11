using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.MainUI;
using Cornerstone.GUI.Filtering;
using Cornerstone.GUI;
using System.Diagnostics;
using MediaPortal.Plugins.MovingPictures.Properties;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    public partial class MenuEditorPopup : Form {

        public bool ShowMovieNodeSettings {
            get { return movieNodeSettingsPanel.Visible; }
            set { movieNodeSettingsPanel.Visible = value; }
        }

        public MenuEditorPopup() {
            InitializeComponent();

            MenuTree.SelectedNodeChanged += new DBNodeEventHandler(MenuTree_SelectedNodeChanged);
            nodeSettingsPanel.DBManager = MovingPicturesCore.DatabaseManager;
            MenuTree.DBManager = MovingPicturesCore.DatabaseManager;

            nodeSettingsPanel.ShowFilterHelpButton = true;
            nodeSettingsPanel.FilterHelpAction = new HelpActionDelegate(delegate {
                ProcessStartInfo processInfo = new ProcessStartInfo(Resources.FilterEditorURL);
                Process.Start(processInfo);
            });
            
            TranslationParserDelegate parserDelegate = new TranslationParserDelegate(Translation.ParseString);
            MenuTree.TranslationParser = parserDelegate;
            nodeSettingsPanel.TranslationParser = parserDelegate;
        }

        void MenuTree_SelectedNodeChanged(IDBNode node, Type type) {
            DBNode<DBMovieInfo> movieNode = (DBNode<DBMovieInfo>)node;

            nodeSettingsPanel.Node = movieNode;
            if (movieNodeSettingsPanel.Visible)
                movieNodeSettingsPanel.Node = movieNode;

        }

        private void MenuEditorPopup_FormClosing(object sender, FormClosingEventArgs e) {
            MenuTree.Dispose();
            nodeSettingsPanel.Dispose();
        }

        private void okButton_Click(object sender, EventArgs e) {
            Close();
        }

        private void helpButton_Click(object sender, EventArgs e) {
            ProcessStartInfo processInfo = new ProcessStartInfo(Resources.MenuEditorURL);
            Process.Start(processInfo);
        }
    }
}
