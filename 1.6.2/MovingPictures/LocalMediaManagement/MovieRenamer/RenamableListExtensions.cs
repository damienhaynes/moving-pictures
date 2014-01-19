using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieRenamer {
    public static class RenamableListExtensions {
        public static bool RenameApprovedItems(this IList<Renamable> list) {
            bool success = true;
            
            // rename files first to prevent changed paths after a directory rename
            foreach (Renamable currItem in list)
                if (currItem is RenamableFile)
                    if (!currItem.Rename()) success = false;

            // wait a sec for directory changes to propagate.
            Thread.Sleep(100);

            // rename directories
            foreach (Renamable currItem in list)
                if (currItem is RenamableDirectory)
                    if (!currItem.Rename()) success = false;

            return success;
        }

        // update the final filenames based on directory name changes and approval. this 
        // is used purely for display purposes. druing the actual rename process, files are 
        // renamed first to avoid any directory conflicts and unapproved files are skipped
        public static void UpdateFinalFilenames(this IList<Renamable> list) {
            // initialize final names
            foreach (Renamable currItem in list)
                currItem.FinalNewName = currItem.NewName;


            // update all final filenames based on pending directory name changes
            foreach (Renamable currItem in list) {
                // check if we have found an approved directory rename
                RenamableDirectory currDir = currItem as RenamableDirectory;
                if (currDir == null) continue;
                if (!currDir.Approved) {
                    currItem.FinalNewName = currItem.OriginalName;
                    continue;
                }

                // and if so, replace the directory path in the finalfilename of all files
                foreach (Renamable currSubItem in list) {
                    RenamableFile currFile = currSubItem as RenamableFile;
                    if (currFile == null) continue;

                    currFile.FinalNewName = currFile.NewName.Replace(currDir.OriginalName, currDir.NewName);
                }
            }

            // reset the final name for any unapproved file rename operations
            foreach (Renamable currItem in list) {
                RenamableFile currFile = currItem as RenamableFile;
                if (currFile == null || currFile.Approved) continue;

                // remove the new filename for this movie and add the old one
                int lastDirSeperator = currFile.FinalNewName.LastIndexOf(Path.DirectorySeparatorChar);
                currFile.FinalNewName = currFile.FinalNewName.Remove(lastDirSeperator + 1) + currFile.OriginalFile.Name;   
            }
        }
    }
}
