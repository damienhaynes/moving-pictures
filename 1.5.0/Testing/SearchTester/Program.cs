using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using SearchTester.Properties;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SearchTester {
    static class Program {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string lpFileName);

        static void Extract() {
            // create temp folder for library
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string dirName = Path.Combine(Path.GetTempPath(), Assembly.GetExecutingAssembly().GetName().Name + @"\" + version);
            if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);

            // define the full paths to our binaries
            string sqlitePath = Path.Combine(dirName, "sqlite.dll");

            // Copy the library and try to load it
            try {
                if (!File.Exists(sqlitePath))
                    using (Stream outFile = File.Create(sqlitePath))
                        outFile.Write(Resources.sqlite, 0, Resources.sqlite.Length);

                IntPtr h = LoadLibrary(sqlitePath);
                Debug.Assert(h != IntPtr.Zero, "Unable to load library: " + sqlitePath);

            }
            catch (Exception e) {
                Console.Write(e);
                return;
            }
        }


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Extract();

            #if !DEBUG
            try {
            #endif
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SearchForm());
            
            #if !DEBUG
            }
            catch (Exception) {
                MessageBox.Show("Unexpected error. Is Moving Pictures installed on this machine?");
            }
            #endif
        }
    }
}
