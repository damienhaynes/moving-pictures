using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Tools;
using NLog;
using MediaPortal.Plugins.MovingPictures.Database;
using System.Threading;

namespace MediaPortal.Plugins.MovingPictures.BackgroundProcesses {
    internal class FileSyncProcess: AbstractBackgroundProcess {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public override string Name {
            get { return "Removed File Check"; }
        }

        public override string Description {
            get {
                return "This process checks for any files that have been removed from the disk and " +
                       "removes them from the database as needed.";
            }
        }

        public override void Work() {
            logger.Info("File maintenance started in the background.");
            DateTime start = DateTime.Now;
            DatabaseMaintenanceManager.RemoveInvalidFiles();
            TimeSpan duration = DateTime.Now - start;
            logger.Info("File maintenance finished. ({0} seconds)", Math.Round(duration.TotalSeconds,1).ToString());
        }
    }
}