using System;
using System.Collections.Generic;
using System.IO;
using Cornerstone.Extensions.IO;
using NLog;

namespace Cornerstone.Tools {
    public static class DriveInfoHelper {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly object syncRoot = new object();
        private static Dictionary<string, DriveInfo> driveInfoPool;
        
        /// <summary>
        /// Gets the DriveInfo object for a given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DriveInfo GetDriveInfoFromFilePath(string path) {
            string driveletter = path.PathToDriveletter();
            return GetDriveInfo(driveletter);
        }

        /// <summary>
        /// Gets the DriveInfo object for the given driveletter 
        /// When the object was created before it will be returned from cache.
        /// </summary>
        /// <param name="driveletter">ex. E:\ </param>
        /// <returns></returns>
        public static DriveInfo GetDriveInfo(string drive) {
            if (drive == null)
                return null;

            lock (syncRoot) {
                // if this is the first request create the driveinfo collection cache
                if (driveInfoPool == null)
                    driveInfoPool = new Dictionary<string, DriveInfo>();

                if (!driveInfoPool.ContainsKey(drive)) {
                    try {
                        driveInfoPool.Add(drive, new DriveInfo(drive));
                    }
                    catch (Exception e) {
                        logger.Error("Error retrieving driveinfo object for '{0}': {1}", drive, e.Message);
                        return null;
                    }
                }
            }
            return driveInfoPool[drive];
        }

    }
}
