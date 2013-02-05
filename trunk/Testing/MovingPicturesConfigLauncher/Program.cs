using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.MainUI;
using MediaPortal.Plugins.MovingPictures.Database;
using System.Reflection;
using Microsoft.Win32;
using MediaPortal.GUI.Library;
using System.IO;

namespace MovingPicturesConfigTester {
    class Program {
        private static string mepoDir = null;
        private static string pluginDir = null;

        private static bool InitAssemblyFinder() {
            mepoDir = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal", "InstallPath", null);
            if (mepoDir == null) {
                MessageBox.Show("MediaPortal must be installed to use this program.");
                return false;
            }

            pluginDir = Path.Combine(mepoDir, @"plugins\Windows\");

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromMepoFolders);
            
            return true;
        }

        private static Assembly LoadFromMepoFolders(object sender, ResolveEventArgs args) {
            string assemblyPath = Path.Combine(mepoDir, new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath)) {
                assemblyPath = Path.Combine(pluginDir, new AssemblyName(args.Name).Name + ".dll");
                if (!File.Exists(assemblyPath)) return null;
            }

            return Assembly.LoadFrom(assemblyPath);
        }

        private static void Launch() {
            ConfigConnector plugin = new ConfigConnector();
            plugin.ShowPlugin();
        }

        [STAThreadAttribute]
        static void Main(string[] args) {
            System.Windows.Forms.Application.EnableVisualStyles();

            bool good = InitAssemblyFinder();
            if (good) Launch();
        }
    }
}


