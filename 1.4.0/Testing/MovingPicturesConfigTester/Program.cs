using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.MainUI;

namespace MovingPicturesConfigTester {
    class Program {

        [STAThreadAttribute]
        static void Main(string[] args) {
            System.Windows.Forms.Application.EnableVisualStyles();

            ConfigConnector plugin = new ConfigConnector();
            plugin.ShowPlugin();

        }
    }
}


