using MediaPortal.Plugins.MovingPictures.MainUI;
using System;

namespace ConfigTester {
    public class MovingPicturesLauncher : PluginConfigLauncher {

        public override string FriendlyPluginName {
            get { return "Moving Pictures"; }
        }

        public override void Launch() {
            ConfigConnector plugin = new ConfigConnector();
            plugin.ShowPlugin();
        }
    }
}


