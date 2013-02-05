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
        private const String errorDialogTitle = "Something Went Wrong";
        private const String missingDLL = "Moving Pictures must be installed to use this configuration launcher. The following component could not be found:\n\n{0}";
        private const string unexpectedError = "There is a problem and the Moving Pictures doesn't know how to recover:\n\n{0}";
        private const string noMepo = "MediaPortal must be installed to use this configuration launcher. A reference to the MediaPortal installation directory could not be found in the registry.";

        private static string mepoDir = null;
        private static string pluginDir = null;

        private static bool InitAssemblyFinder() {
            mepoDir = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal", "InstallPath", null) as string;
            if (mepoDir == null) {
                MessageBox.Show(noMepo, errorDialogTitle);
                return false;
            }

            pluginDir = Path.Combine(mepoDir, @"plugins\Windows\");

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromMepoFolders);
            
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

            try {
                bool pathsExists = InitAssemblyFinder();
                if (pathsExists) Launch();
            }

            catch (Exception e) {
                MessageBox.Show(String.Format(unexpectedError, e.Message), errorDialogTitle);
            }
        }
    }
}


