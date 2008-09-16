using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;
using MediaPortal.Plugins.MovingPictures.DataProviders;
using System.Threading;
using System.Collections;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;
using System.Text.RegularExpressions;
using NLog;
using Cornerstone.Database.Tables;
using Cornerstone.Database;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {
    public enum MovieImporterAction { 
        ADDED,
        ADDED_FROM_SPLIT,
        ADDED_FROM_JOIN,
        PENDING,
        GETTING_MATCHES,
        GETTING_DETAILS,
        NEED_INPUT,
        APPROVED,
        COMMITED,
        IGNORED,
        REMOVED_FROM_SPLIT,
        REMOVED_FROM_JOIN,
        STARTED,
        STOPPED
    }

    public class MovieImporter {
        #region Private Variables

        private static Logger logger = LogManager.GetCurrentClassLogger();

        // threads that do actual processing
        private List<Thread> mediaScannerThreads;
        private Thread pathScannerThread;

        private int percentDone;
        private bool fullScanNeeded;

        public const string rxYearScan = @"(^.+)[\[\(]?([0-9]{4})[\]\)]?($|.+)";
        public const string rxMultiPartScan = @"((cd|disk|disc|part)[\s\-]*([a-c0-9]|[i]+))|[\(\[]\dof\d[\)\]]$|[^\s\d]([a-c0-9])$";
        public const string rxMultiPartClean = @"((cd|disk|disc|part)[\s\-]*([a-c0-9]|[i]+))|[\(\[]\dof\d[\)\]]$";
        public const string rxPunctuationToSpace = @"[\.\:\;\""\+\*]";
        public const string rxPunctuationFilter = @"[\'\`\,]";
        public const string rxCompressSpaces = @"\s{2,}";
                
        // a list of all files currently in the system
        private Dictionary<DBLocalMedia, MediaMatch> matchesInSystem;

        // Matches that have not yet been scanned.
        public ArrayList PendingMatches {
            get { return ArrayList.ReadOnly(pendingMatches); }
        } private ArrayList pendingMatches;

        // Same as PendingMatches, but this list gets priority. Used for user based interaction.
        public ArrayList PriorityPendingMatches {
            get { return ArrayList.ReadOnly(priorityPendingMatches); }
        } private ArrayList priorityPendingMatches;

        // Matches that are not close enough for auto approval and require user input.
        public ArrayList MatchesNeedingInput {
            get { return ArrayList.ReadOnly(matchesNeedingInput); }
        } private ArrayList matchesNeedingInput;

        // Matches that the importer is currently pulling details for
        public ArrayList RetrievingDetailsMatches {
            get { return ArrayList.ReadOnly(retrievingDetailsMatches); }
        } private ArrayList retrievingDetailsMatches;

        // Matches that are accepted and are awaiting details retrieval and commital. 
        public ArrayList ApprovedMatches {
            get { return ArrayList.ReadOnly(approvedMatches); }
        } private ArrayList approvedMatches;

        // Same as ApprovedMatches but this list get's priority. Used for user based interaction.
        public ArrayList PriorityApprovedMatches {
            get { return ArrayList.ReadOnly(priorityApprovedMatches); }
        } private ArrayList priorityApprovedMatches;

        // Matches that have been committed and saved to the database. 
        public ArrayList CommitedMatches {
            get { return ArrayList.ReadOnly(commitedMatches); }
        } private ArrayList commitedMatches;


        // Files that have recently been added to the filesystem, and need to be processed.
        public ArrayList FilesAdded {
            get { return ArrayList.ReadOnly(filesAdded); }
        } private ArrayList filesAdded;

        // Files that have recently been removed from the filesystem.
        public ArrayList FilesDeleted {
            get { return ArrayList.ReadOnly(filesDeleted); }
        } private ArrayList filesDeleted;

        // list of watcher objects that monitor the filesystem for changes
        List<FileSystemWatcher> fileSystemWatchers;
        Dictionary<FileSystemWatcher, DBImportPath> pathLookup;

        #endregion
        
        // sends progress update events to any available listeners
        public delegate void ImportProgressHandler(int percentDone, int taskCount, int taskTotal, string taskDescription);
        public event ImportProgressHandler Progress;

        // updates listeners of changes to the match lists. This will not provide info on when
        // pending matches are externally approved.
        public delegate void MovieStatusChangedHandler(MediaMatch obj, MovieImporterAction action);
        public event MovieStatusChangedHandler MovieStatusChanged;

        // Creates a MovieImporter object which will scan ImportPaths and import new media.
        public MovieImporter() {
            mediaScannerThreads = new List<Thread>();

            pendingMatches = ArrayList.Synchronized(new ArrayList());
            priorityPendingMatches = ArrayList.Synchronized(new ArrayList());
            matchesNeedingInput = ArrayList.Synchronized(new ArrayList());
            approvedMatches = ArrayList.Synchronized(new ArrayList());
            priorityApprovedMatches = ArrayList.Synchronized(new ArrayList());
            retrievingDetailsMatches = ArrayList.Synchronized(new ArrayList());
            commitedMatches = ArrayList.Synchronized(new ArrayList());

            filesAdded = ArrayList.Synchronized(new ArrayList());
            filesDeleted = ArrayList.Synchronized(new ArrayList());

            matchesInSystem = new Dictionary<DBLocalMedia, MediaMatch>();

            fileSystemWatchers = new List<FileSystemWatcher>();
            pathLookup = new Dictionary<FileSystemWatcher, DBImportPath>();

            MovingPicturesCore.DatabaseManager.ObjectDeleted += new DatabaseManager.ObjectAffectedDelegate(DatabaseManager_ObjectDeleted);

            percentDone = 0;
        }

        ~MovieImporter() {
            Stop();
        }

        #region Public Methods

        public void Start() {
            int maxThreadCount = (int)MovingPicturesCore.SettingsManager["importer_thread_count"].Value;

            if (mediaScannerThreads.Count == 0) {
                for (int i = 0; i < maxThreadCount; i++) {
                    Thread newThread = new Thread(new ThreadStart(ScanMedia));
                    newThread.Start();
                    newThread.Name = "MediaScanner";

                    mediaScannerThreads.Add(newThread);
                }
                
                logger.Info("Started MovieImporter");
            }

            if (pathScannerThread == null) {
                pathScannerThread = new Thread(new ThreadStart(ScanAndMonitorPaths));
                pathScannerThread.Start();
                pathScannerThread.Name = "PathScanner";
            }
        }

        public void Stop() {
            Thread newThread = new Thread(new ThreadStart(StopWorker));
            newThread.Start();
        }
        
        // This is spawned off in a seperate thread to give better responsiveness when shutting down
        // the config screen. It can take a few seconds sometimes to kill HTTP processes in the
        // media scanner threads.
        private void StopWorker() {
            lock (mediaScannerThreads) {
                if (mediaScannerThreads.Count > 0) {
                    foreach (Thread currThread in mediaScannerThreads)
                        currThread.Abort();

                    mediaScannerThreads.Clear();
                    logger.Info("Stopped MovieImporter");
                }

                if (pathScannerThread != null) {
                    pathScannerThread.Abort();
                    pathScannerThread = null;
                }

                if (Progress != null)
                    Progress(100, 0, 0, "Stopped");
            }
        }

        public bool IsScanning() {
            return (mediaScannerThreads.Count != 0);
        }

        public void StartFullScan() {
            fullScanNeeded = true;
        }

        // This method is written weird and needs to be clarified. But I think it works, 
        // reloading specified files.
        public void Reprocess(List<DBLocalMedia> fileList) {
            List<DBLocalMedia> fileSet = new List<DBLocalMedia>();
            foreach (DBLocalMedia currFile in new List<DBLocalMedia>(fileList)) {
                // if file is already in importer, reload if requested
                if (matchesInSystem.ContainsKey(currFile)) {
                    Reprocess(matchesInSystem[currFile]);
                    ScanFiles(fileSet, true);
                    fileSet = new List<DBLocalMedia>();
                    continue;
                }

                // if file is already commited but not in importer, remove all relations
                // and remove the file from the DB, then queue up for scanning
                if (currFile.ID != null ) {
                    RemoveCommitedRelations(currFile);
                    currFile.Delete();
                    fileSet.Add(currFile);
                }
            }

            ScanFiles(fileSet, true);
        }

        // Approves the MediaMatch for detail processing and commit. THis shold be
        // used in conjunction with the MatchListChanged event when a NEED_INPUT action 
        // is received. 
        public void Approve(MediaMatch match) {
            if (match.Selected == null)
                return;

            RemoveFromMatchLists(match);

            // clear the ignored flag in case these files were previously on the ignore list
            foreach (DBLocalMedia currFile in match.LocalMedia) {
                currFile.Ignored = false;
            }

            // select the list to add this match to based on priority
            ArrayList approveList;
            if (match.HighPriority) approveList = priorityApprovedMatches;
            else approveList = approvedMatches;

            lock (approveList.SyncRoot)
                approveList.Insert(0, match);
  
            // notify any listeners of the status change
            logger.Info("User approved " + match.LocalMediaString + "as " + match.Selected.Movie.Title);
            if (MovieStatusChanged != null)
                MovieStatusChanged(match, MovieImporterAction.APPROVED);

        }

        // removes any association with the file(s) in the MediaMatch and flags the files
        // to be ignored in the future.
        public void Ignore(MediaMatch match) {
            RemoveFromMatchLists(match);
            RemoveCommitedRelations(match.LocalMedia);

            foreach (DBLocalMedia currFile in match.LocalMedia) {
                currFile.Ignored = true;
                currFile.Commit();
            }

            // notify any listeners of the status change
            logger.Info("User ignored " + match.LocalMediaString);
            if (MovieStatusChanged != null)
                MovieStatusChanged(match, MovieImporterAction.IGNORED);
        }

        // rescans for possible movie matches using the specified search string
        public void Reprocess(MediaMatch match) {
            RemoveFromMatchLists(match);
            RemoveCommitedRelations(match.LocalMedia);

            // clear the ignored flag in case these files were previously on the ignore list
            foreach (DBLocalMedia currFile in match.LocalMedia) {
                currFile.Ignored = false;
            }
            
            lock (priorityPendingMatches) {
                match.HighPriority = true;
                priorityPendingMatches.Add(match);
            }

            // notify any listeners of the status change
            logger.Info("User reprocessing " + match.LocalMediaString);
            if (MovieStatusChanged != null)
                MovieStatusChanged(match, MovieImporterAction.PENDING);
        }

        // takes the given match containing multiple files and splits it up into
        // individual matches for each file
        public void Split(MediaMatch match) {
            if (match == null || match.LocalMedia.Count < 2)
                return;

            RemoveFromMatchLists(match);
            RemoveCommitedRelations(match.LocalMedia);
            match.Deleted = true;

            // notify any listeners of the status change
            logger.Info("User split pair " + match.LocalMediaString);
            if (MovieStatusChanged != null)
                MovieStatusChanged(match, MovieImporterAction.REMOVED_FROM_SPLIT);

            foreach (DBLocalMedia currFile in match.LocalMedia) {
                // clear the ignored flag in case these files were previously on the ignore list
                currFile.Ignored = false;

                MediaMatch newMatch = new MediaMatch();
                newMatch.LocalMedia.Add(currFile);
                lock (priorityPendingMatches.SyncRoot) {
                    newMatch.HighPriority = true;
                    priorityPendingMatches.Insert(0, newMatch);
                }

                if (MovieStatusChanged != null)
                    MovieStatusChanged(newMatch, MovieImporterAction.ADDED_FROM_SPLIT);
            }
        }

        // given multiple matches, a new match is created that is the sum of the previous 
        // parts. In practice this means that two parts of the same movie were joined together
        // to be treated as one.
        public void Join(List<MediaMatch> matchList) {
            if (matchList == null || matchList.Count < 2)
                return;

            List<DBLocalMedia> fileList = new List<DBLocalMedia>();

            // build the file list and clear out old matches
            foreach (MediaMatch currMatch in matchList) {
                RemoveFromMatchLists(currMatch);
                RemoveCommitedRelations(currMatch.LocalMedia);
                currMatch.Deleted = true;
                fileList.AddRange(currMatch.LocalMedia);

                // notify any listeners of the status change
                if (MovieStatusChanged != null)
                    MovieStatusChanged(currMatch, MovieImporterAction.REMOVED_FROM_JOIN);
            }

            // build the new match and add it for processing
            MediaMatch newMatch = new MediaMatch();
            newMatch.LocalMedia = fileList;
            lock (priorityPendingMatches.SyncRoot) {
                newMatch.HighPriority = true;
                priorityPendingMatches.Insert(0, newMatch);
            }

            // notify any listeners of the status change
            logger.Info("User joined " + newMatch.LocalMediaString);
            if (MovieStatusChanged != null)
                MovieStatusChanged(newMatch, MovieImporterAction.ADDED_FROM_JOIN);
        }

        // updates the artwork for any movies that are missing artwork
        public void LookForMissingArtwork() {
            Thread newThread = new Thread(new ThreadStart(LookForMissingArtworkWorker));
            newThread.Start();
        }

        private void LookForMissingArtworkWorker() {
            foreach (DBMovieInfo currMovie in DBMovieInfo.GetAll()) {
                if (currMovie.CoverFullPath.Trim().Length == 0) {
                    MovingPicturesCore.CoverProvider.GetArtwork(currMovie);
                    currMovie.UnloadArtwork();
                    currMovie.Commit();
                }

                if (currMovie.BackdropFullPath.Trim().Length == 0) {
                    new LocalProvider().GetBackdrop(currMovie);
                    //MovingPicturesCore.BackdropProvider.GetBackdrop(currMovie);
                    currMovie.Commit();

                }
            }
        }

        #endregion
        
        #region File System Scanner

        // Does an initial scan of all DBImportPath objects then monitors for any folder changes
        // or newly added import paths. This thread does not modify the percentDone progress indicator,
        // as generally file system scans are very very fast.
        private void ScanAndMonitorPaths() {
            try {
                while (true) {
                    logger.Info("Initiating full scan on watch folders.");
                    SetupFileSystemWatchers();
                    RemoveOrphanFiles();
                    RemoveMissingFiles();
                    RemoveOrphanArtwork();
                    LookForMissingArtwork();

                    // do an initial scan on all paths
                    // grab all the files in our import paths
                    int count = 0;
                    List<DBImportPath> paths = DBImportPath.GetAll();
                    foreach (DBImportPath currPath in paths) {
                        count++;
                        if (Progress != null)
                            Progress(percentDone, count, paths.Count, "Scanning local media sources...");

                        ScanPath(currPath);
                    }

                    fullScanNeeded = false;

                    // monitor existing paths for change
                    while (!fullScanNeeded) {
                        Thread.Sleep(1000);
                        
                        // if the filesystem scanner found any files, add them
                        lock (filesAdded.SyncRoot) {
                            if (filesAdded.Count > 0) {
                                List<DBLocalMedia> fileList = new List<DBLocalMedia>();
                                foreach (object currFile in filesAdded) 
                                    fileList.Add((DBLocalMedia)currFile);
                                
                                ScanFiles(fileList, false);
                                filesAdded.Clear();
                            }
                        }
                    }

                }
            }
            catch (ThreadAbortException) {
            }
        }

        // Sets up the objects that will watch the file system for changes, specifically
        // new files added to the import path, or old files removed.
        private void SetupFileSystemWatchers() {
            List<DBImportPath> paths = DBImportPath.GetAll();

            // clear out old watchers, if any
            foreach (FileSystemWatcher currWatcher in fileSystemWatchers) 
                currWatcher.EnableRaisingEvents = false;
            fileSystemWatchers.Clear();

            // setup new file systems watchers
            foreach (DBImportPath currPath in paths) {
                try {
                    FileSystemWatcher currWatcher = new FileSystemWatcher(currPath.FullPath);
                    currWatcher.IncludeSubdirectories = true;
                    currWatcher.Created += OnFileAdded;
                    currWatcher.Deleted += OnFileDeleted;
                    currWatcher.EnableRaisingEvents = true;
                    fileSystemWatchers.Add(currWatcher);
                    pathLookup[currWatcher] = currPath;
                }
                catch (ArgumentException) {
                    if (currPath.Removable)
                        logger.Info("Removable import path " + currPath.Removable + " is offline.");
                    else
                        logger.Error("Failed accessing import path " + currPath.FullPath + ". Should be set to 'Removable'?");
                }
            }

        }

        // When a FileSystemWatcher detects a new file, this method queues it up for processing.
        private void OnFileAdded(Object source, FileSystemEventArgs e) {
            DBLocalMedia newFile = DBLocalMedia.Get(e.FullPath);

            DBImportPath importPath = pathLookup[((FileSystemWatcher)source)];

            // if this file is already in the system, ignore (this happens if it's a removable source)
            if (newFile.ID != null) {
                if (!importPath.Removable) 
                    logger.Warn("FileSystemWatcher tried to add a pre-existing file: " + newFile.File.Name);
                else
                    logger.Info("Removable file " + newFile.File.Name + " brought online.");
                return;
            }

            // if the extension is proper, add the file
            foreach (string currExt in MediaPortal.Util.Utils.VideoExtensions)
                if (newFile.File.Extension == currExt) {
                    newFile.ImportPath = importPath;
                    lock (filesAdded.SyncRoot) filesAdded.Add(newFile);
                    logger.Info("FileSystemWatcher queued " + newFile.File.Name + " for processing.");
                    break;
                }
        }

        // When a FileSystemWatcher detects a file has been removed, delete it.
        private void OnFileDeleted(Object source, FileSystemEventArgs e) {
            DBLocalMedia removedFile = DBLocalMedia.Get(e.FullPath);

            logger.Info("FileSystemWatcher flagged " + removedFile.File.Name + " for removal from the database.");

            // if the file is not in our system there's nothing to do
            if (removedFile.ID == null) {
                logger.Warn("FileSystemWatcher tried to remove a file from the database that is not in our system.");
                return;
            }

            // if this file is from a removable source, there is no action
            if (removedFile.ImportPath.Removable) {
                logger.Info("Removable file " + removedFile.File.Name + " taken offline.");
                return;
            }

            // remove file, it's movie object it's attached to, and all other files
            // owned by that movie if it's a multi-part movie.
            foreach (DBMovieInfo currMovie in removedFile.AttachedMovies) {
                foreach (DBLocalMedia currFile in currMovie.LocalMedia)
                    currFile.Delete();
                currMovie.Delete();
            }
        }

        // loops through all local files in the system and removes anything that does not actually exist
        // and is not a part of a removable import path
        private void RemoveMissingFiles() {
            logger.Info("Removing missing files from database.");
            // take care of cover art
            foreach (DBLocalMedia currFile in DBLocalMedia.GetAll()) {
                if (!currFile.ImportPath.Removable && !currFile.File.Exists) {
                    // remove file, it's movie object it's attached to, and all other files
                    // owned by that movie if it's a multi-part movie.
                    logger.Info("Removing " + currFile.FullPath + " and associated movie from database because file is missing and not flagged as removable.");
                    foreach (DBMovieInfo currMovie in currFile.AttachedMovies) {
                        foreach (DBLocalMedia otherFile in currMovie.LocalMedia)
                            otherFile.Delete();
                        currMovie.Delete();
                    }

                    // should have already been deleted, but if we for some reason have a
                    // file with no associated movie then delete it too.
                    currFile.Delete();
                }
            }
        }

        private void RemoveOrphanArtwork() {
            logger.Info("Removing missing artwork from database attached to existing movies.");
            foreach (DBMovieInfo currMovie in DBMovieInfo.GetAll()) {
                // get the list of elements to remove
                List<string> toRemove = new List<string>();
                foreach (string currCoverPath in currMovie.AlternateCovers) {
                    if (!new FileInfo(currCoverPath).Exists)
                        toRemove.Add(currCoverPath);
                }

                // remove them
                foreach (string currItem in toRemove) {
                    currMovie.AlternateCovers.Remove(currItem);
                }

                // reset default cover is needed
                if (!currMovie.AlternateCovers.Contains(currMovie.CoverFullPath))
                    if (currMovie.AlternateCovers.Count == 0)
                        currMovie.CoverFullPath = " ";
                    else
                        currMovie.CoverFullPath = currMovie.AlternateCovers[0];

                // get rid of the backdrop link if it doesnt exist
                if (currMovie.BackdropFullPath.Trim().Length > 0 && 
                    !new FileInfo(currMovie.BackdropFullPath).Exists)
                    currMovie.BackdropFullPath = " ";
                
            }
        }

        // Removes all files not belonging to an import path.
        private void RemoveOrphanFiles() {
            logger.Info("Removing orphan files from database.");
            foreach (DBLocalMedia currFile in DBLocalMedia.GetAll()) {
                if (currFile.ImportPath == null || currFile.ImportPath.ID == null) {
                    // remove file, it's movie object it's attached to, and all other files
                    // owned by that movie if it's a multi-part movie.
                    logger.Info("Removing " + currFile.FullPath + " and associated movie because ImportPath has been removed.");
                    foreach (DBMovieInfo currMovie in currFile.AttachedMovies) {
                        foreach (DBLocalMedia otherFile in currMovie.LocalMedia)
                            otherFile.Delete();
                        currMovie.Delete();
                    }

                    // should have already been deleted, but if we for some reason have a
                    // file with no associated movie then delete it too.
                    currFile.Delete();
                }
            }
        }

        // Grabs the files from the DBImportPath and add them to the queue for use
        // by the ScanMedia thread.
        private void ScanPath(DBImportPath importPath) {
            ScanFiles(importPath.GetNewLocalMedia(), false);
        }
        
        // Adds the files to the importer for processing. If a file has recently been commited 
        // and it's readded, it will be reprocessed.
        private void ScanFiles(List<DBLocalMedia> importFileList, bool highPriority) {
            List<DBLocalMedia> currFileSet = new List<DBLocalMedia>();
            // Create the Multi-Part regular expression
            Regex rxMultiPart = new Regex(rxMultiPartScan, RegexOptions.IgnoreCase);   

            foreach (DBLocalMedia currFile in importFileList) {
              //string currFolder = currFile.File.DirectoryName;
                // if we have already loaded this file, move to the next
              if (matchesInSystem.ContainsKey(currFile) || currFile.ID != null)
                continue;

              // only grab the video_ts.ifo when extension is ifo
              // to prevent unnecessary stacking
              if (currFile.File.Extension.ToLower() == ".ifo")
              {
                if (currFile.File.Name.ToLower() != "video_ts.ifo")
                  continue;  
              }


                // exclude samplefiles
                if (isSampleFile(currFile.File)) {
                  logger.Info("Sample detected. Skipping {0} ({1} bytes)", currFile.File.Name, currFile.File.Length);
                  continue;
                }
                
                // if we have no previous files, move on so we can check if the next file
                // is a pair to this one.
                if (currFileSet.Count == 0) {
                    currFileSet.Add(currFile);
                    continue;
                }

                // check if the currFile is a part of the same movie as the previous
                // file(s)
                bool isAdditionalMatch = true;
                foreach (DBLocalMedia otherFile in currFileSet) {
                    // if files are not in the same folder we assume they are not a pair
                    if (!currFile.File.DirectoryName.Equals(otherFile.File.DirectoryName)) {
                        isAdditionalMatch = false;
                        break;
                    }

                    // if the filename differ by more than one character
                    // assume they are not a pair
                    if (AdvancedStringComparer.Levenshtein(currFile.File.Name, otherFile.File.Name) > 1) {
                        isAdditionalMatch = false;
                        break;
                    }
                    
                    // if the multi-part naming convention doesn't match up
                    // assume they are not a pair
                    if (!rxMultiPart.Match(RemoveFileExtension(currFile.File)).Success)
                    {
                      isAdditionalMatch = false;
                      break;
                    }

                }

                // if it's a match store it and move onto the next file to see if
                // it is part of the set too
                if (isAdditionalMatch) {
                    currFileSet.Add(currFile);
                    continue;
                }

                // if it's not a match, add the previous file set and then start a new one
                // with the current file.
                if (!isAdditionalMatch) {
                    MediaMatch newMatch = new MediaMatch();
                    newMatch.LocalMedia = currFileSet;
                    newMatch.FolderHint = (currFileSet.Count == folderCheck(currFileSet[0].File.Directory));
                                  
                    lock (pendingMatches.SyncRoot) {
                        pendingMatches.Add(newMatch);
                    }
                    
                    foreach (DBLocalMedia subFile in currFileSet) 
                        matchesInSystem.Add(subFile, newMatch);

                    if (MovieStatusChanged != null)
                        MovieStatusChanged(newMatch, MovieImporterAction.ADDED);

                    if (highPriority)
                        Reprocess(newMatch);

                    currFileSet = new List<DBLocalMedia>();
                    currFileSet.Add(currFile);
                }

            }
            // queue up the last set of files
            if (currFileSet.Count > 0) {
                MediaMatch newMatch = new MediaMatch();
                newMatch.LocalMedia = currFileSet;
                newMatch.FolderHint = (currFileSet.Count == folderCheck(currFileSet[0].File.Directory));
                lock (pendingMatches.SyncRoot) {
                    pendingMatches.Add(newMatch);
                }

                foreach (DBLocalMedia subFile in currFileSet)
                    matchesInSystem.Add(subFile, newMatch);

                if (MovieStatusChanged != null)
                    MovieStatusChanged(newMatch, MovieImporterAction.ADDED);

                if (highPriority)
                    Reprocess(newMatch);
            }

        }

        // When a process has removed a local file from the database, we should remove it from the matching system
        private void DatabaseManager_ObjectDeleted(DatabaseTable obj) {
            if (obj is DBLocalMedia)
                if (matchesInSystem.ContainsKey((DBLocalMedia)obj))
                    RemoveFromMatchLists(matchesInSystem[(DBLocalMedia)obj]);
        }


        // Check if the supplied file is a Sample file
        private bool isSampleFile(FileInfo file)
        {
          // Create the sample filter regular expression
          Regex rxSampleFilter = new Regex(MovingPicturesCore.SettingsManager["importer_sample_keyword"].Value.ToString(), RegexOptions.IgnoreCase);  
          // Set sample max size in bytes
          long sampleMaxSize = long.Parse(MovingPicturesCore.SettingsManager["importer_sample_maxsize"].Value.ToString()) * 1024 * 1024;
          return ((file.Length < sampleMaxSize) && rxSampleFilter.Match(file.Name).Success);
        }

        // Returns a movie count on the folder (excluding samples)
        private Dictionary<string, int> fileCount;
        private int folderCheck(DirectoryInfo folder) {
            if (fileCount == null)
                fileCount = new Dictionary<string, int>();

            // if we have already scanned this folder move on
            if (fileCount.ContainsKey(folder.FullName))
                return fileCount[folder.FullName];

            // count the number of non-sample video files in the folder
            int rtn = 0;
            FileInfo[] fileList = folder.GetFiles("*");
            foreach (FileInfo currFile in fileList) {
                foreach (string currExt in MediaPortal.Util.Utils.VideoExtensions) {
                    if (currFile.Extension == currExt) {
                        if (!isSampleFile(currFile))
                            rtn++;
                    }
                }
            }

            fileCount[folder.FullName] = rtn;
            return rtn;
        }

        // Removes the file extension from a filename
        public static string RemoveFileExtension(FileInfo file)
        {
          // get rid of the file extension
          int extIndex = file.Name.IndexOf(file.Extension);
          //int extLength = file.Extension.Length;
          return file.Name.Remove(extIndex);
        }

        #endregion

        #region Media Matcher

        // Monitors the mediaQueue for files imported from the ScanAndMonitorPaths thread. When elements
        // exist, possble matches will be imported and subsequently written to the database.
        private void ScanMedia() {
            try {
                while (true) {
                    // if there is nothing to process, then sleep
                    while (pendingMatches.Count == 0 &&
                           approvedMatches.Count == 0 &&
                           priorityPendingMatches.Count == 0 &&
                           priorityApprovedMatches.Count == 0)
                        Thread.Sleep(1000);

                    // so long as there is media to scan, we don't start processing the approved
                    // matches. The goal is to get as much for the user to approve, as fast as
                    // possible.
                    if (priorityPendingMatches.Count > 0)
                        ProcessNextPendingMatch();
                    else if (priorityApprovedMatches.Count > 0)
                        ProcessNextApprovedMatches();
                    else if (pendingMatches.Count > 0)
                        ProcessNextPendingMatch();
                    else
                        ProcessNextApprovedMatches();

                    UpdatePercentDone();

                    // if we are now just waiting on the user, say so
                    if (pendingMatches.Count == 0 && approvedMatches.Count == 0 &&
                        priorityPendingMatches.Count == 0 && priorityApprovedMatches.Count == 0 &&
                        matchesNeedingInput.Count > 0) {
                        if (Progress != null)
                            Progress(percentDone, 0, matchesNeedingInput.Count, "Waiting for Close Match Approvals...");
                    }

                    // if we are now just waiting on the user, say so
                    if (pendingMatches.Count == 0 && approvedMatches.Count == 0 &&
                        priorityPendingMatches.Count == 0 && priorityApprovedMatches.Count == 0 &&
                        matchesNeedingInput.Count == 0) {

                        //currentlyProccessing.Clear();
                        percentDone = 0;

                        if (Progress != null)
                            Progress(100, 0, 0, "Complete!");
                    }


                }

            } catch (ThreadAbortException) {
                // expected when threads shutdown. ignore.
            } catch (Exception e) {
                logger.FatalException("Unhandled error in MediaScanner.", e);
            }
        }

        // updates the local variables of the current progress
        private void UpdatePercentDone() {
            double mediaScanPercent; // value 0-50
            double commitApprovedPercent; // value 0-50

            mediaScanPercent = ((double)(matchesInSystem.Count - pendingMatches.Count)) / (matchesInSystem.Count + 0.001);
            mediaScanPercent *= 50.0;

            commitApprovedPercent = ((double)commitedMatches.Count) /
                                    ((double)matchesNeedingInput.Count + approvedMatches.Count + commitedMatches.Count + 0.001);

            commitApprovedPercent *= 50;
            
            percentDone = (int) (mediaScanPercent + commitApprovedPercent);

            if (percentDone > 100)
                percentDone = 100;
        }

        // gets details for and commits the next item in the ApprovedMatches list
        private void ProcessNextApprovedMatches() {
            ArrayList matchList;

            if (priorityApprovedMatches.Count > 0)
                matchList = priorityApprovedMatches;
            else
                matchList = approvedMatches;
            
            // grab the next match
            MediaMatch currMatch;
            lock (matchList.SyncRoot) {
                if (matchList.Count == 0)
                    return;

                currMatch = (MediaMatch)matchList[0];
                matchList.Remove(currMatch);
                retrievingDetailsMatches.Add(currMatch);
            }

            // notify the user we are processing
            logger.Info("Retrieving details for \"" + currMatch.Selected.Movie.Title + "\"");
            if (Progress != null) {
                int processed = commitedMatches.Count;
                int total = commitedMatches.Count + approvedMatches.Count + matchesNeedingInput.Count;
                Progress(percentDone, processed, total, "Retrieving details for: " + currMatch.Selected.Movie.Title);
            }

            // notify any listeners of the status change
            if (MovieStatusChanged != null)
                MovieStatusChanged(currMatch, MovieImporterAction.GETTING_DETAILS);

            // commit the match and move it to the commited array
            lock (currMatch) AssignFileToMovie(currMatch.LocalMedia, currMatch.Selected.Movie);
            retrievingDetailsMatches.Remove(currMatch);
            commitedMatches.Add(currMatch);

            // if the set has been ignored by the user since we started processing, 
            // reignore it properly and return
            if (currMatch.LocalMedia[0].Ignored) {
                Ignore(currMatch);
                return;
            }

            // if match has been deleted while processing (usually from a split or merge)
            // kick back out.
            if (currMatch.Deleted)
                return;
            
            // notify any listeners of the status change
            logger.Info("Added \"" + currMatch.Selected.Movie.Title + "\".");
            if (MovieStatusChanged != null) 
                MovieStatusChanged(currMatch, MovieImporterAction.COMMITED);
        }

        // retrieves possible matches for the next item (or group of items) in the mediaQueue
        private void ProcessNextPendingMatch() {
            MediaMatch mediaMatch = null;

            // check for a match needing reprocessing
            lock (priorityPendingMatches.SyncRoot) {
                if (priorityPendingMatches.Count != 0) {
                    // grab match
                    mediaMatch = (MediaMatch) priorityPendingMatches[0];
                    priorityPendingMatches.Remove(mediaMatch);
                }
            }

            // if no reprocessing matches available, get next pending match
            if (mediaMatch == null)
                lock (pendingMatches.SyncRoot) {
                    if (pendingMatches.Count == 0)
                        return;

                    mediaMatch = (MediaMatch) pendingMatches[0];
                    pendingMatches.Remove(mediaMatch);
                }


            // if we have any listeners, notify them of our status
            if (Progress != null) {
                int processed = matchesInSystem.Count - pendingMatches.Count;
                int total = matchesInSystem.Count;

                if (mediaMatch.LocalMedia.Count == 1)
                    Progress(percentDone, processed, total, "Retrieving possible matches: " + mediaMatch.LocalMedia[0].File.Name);
                else
                    Progress(percentDone, processed, total, "Retrieving possible matches: " + mediaMatch.LocalMedia[0].File.Directory.Name + "\\");
            }

            // get possible matches for this set of media files
            GetMatches(mediaMatch);

            // if the match has been set to ignore by the user while searching, cancel out
            if (mediaMatch.LocalMedia[0].Ignored) {
                Ignore(mediaMatch);
                return;
            }

            // if match has been deleted while processing (usually from a split or merge)
            // kick back out.
            if (mediaMatch.Deleted)
                return;

            // if the best match is exact or very close, place it in the accepted queue
            // otherwise place it in the pending queue for approval
            int threshold = (int)MovingPicturesCore.SettingsManager["importer_autoapprove"].Value;
            if (mediaMatch.Selected != null && mediaMatch.Selected.MatchValue <= threshold)
            {
                if (mediaMatch.HighPriority) priorityApprovedMatches.Add(mediaMatch);
                else approvedMatches.Add(mediaMatch);

                // notify any listeners
                logger.Info("Auto-approved " + mediaMatch.LocalMediaString + " as " + mediaMatch.Selected.Movie.Title);
                if (MovieStatusChanged != null)
                    MovieStatusChanged(mediaMatch, MovieImporterAction.APPROVED);
            } else {
                matchesNeedingInput.Add(mediaMatch);
                logger.Info("No exact match for " + mediaMatch.LocalMediaString);
                if (MovieStatusChanged != null)
                    MovieStatusChanged(mediaMatch, MovieImporterAction.NEED_INPUT);
            }
        }

        // Associates the given file(s) to the given movie object. Also creates all
        // relevent user related data.
        private void AssignFileToMovie(IList<DBLocalMedia> localMedia, DBMovieInfo movie) {
            if (localMedia == null || movie == null || localMedia.Count == 0)
                return;

            // loop through the local media files and clear out any movie assignments
            foreach (DBLocalMedia currFile in localMedia)
                RemoveCommitedRelations(currFile);

            // write the file(s) to the DB
            int count = 1;
            foreach (DBLocalMedia currFile in localMedia) {
                currFile.Part = count;
                currFile.Commit();

                count++;
            }

            // update, associate, and commit the movie
            MovingPicturesCore.MovieProvider.Update(movie);
            MovingPicturesCore.CoverProvider.GetArtwork(movie);

            new LocalProvider().GetBackdrop(movie);
            //MovingPicturesCore.BackdropProvider.GetBackdrop(movie);
            
            movie.LocalMedia.Clear();
            movie.LocalMedia.AddRange(localMedia);
            movie.UnloadArtwork(); 
            movie.Commit();
            
            // create user related data object for each user
            foreach (DBUser currUser in DBUser.GetAll()) {
                DBUserMovieSettings userSettings = new DBUserMovieSettings();
                userSettings.Movie = movie;
                userSettings.User = currUser;
                userSettings.Commit();
            }
        }

        // Associates the given file(s) to the given movie object. Also creates all
        // relevent user related data.
        private void AssignFileToMovie(DBLocalMedia file, DBMovieInfo movie) {
            AssignFileToMovie(new DBLocalMedia[] { file }, movie);
        }

        // removes the given match from all pending process lists
        private void RemoveFromMatchLists(MediaMatch match) {
            lock (pendingMatches.SyncRoot) {
                if (pendingMatches.Contains(match))
                    pendingMatches.Remove(match);
            }

            lock (priorityPendingMatches.SyncRoot) {
                if (priorityPendingMatches.Contains(match)) {
                    priorityPendingMatches.Remove(match);
                }
            }

            lock (matchesNeedingInput.SyncRoot) {
                if (matchesNeedingInput.Contains(match))
                    matchesNeedingInput.Remove(match);
            }

            lock (approvedMatches.SyncRoot) {
                if (approvedMatches.Contains(match))
                    approvedMatches.Remove(match);
            }

            lock (priorityApprovedMatches.SyncRoot) {
                if (priorityApprovedMatches.Contains(match))
                    priorityApprovedMatches.Remove(match);
            }

            lock (commitedMatches.SyncRoot) {
                if (commitedMatches.Contains(match)) {
                    commitedMatches.Remove(match);
                }
            }

            lock (retrievingDetailsMatches.SyncRoot) {
                if (retrievingDetailsMatches.Contains(match)) {
                    retrievingDetailsMatches.Remove(match);
                }
            }

            foreach (DBLocalMedia currFile in match.LocalMedia) {
                if (matchesInSystem.ContainsKey(currFile))
                    matchesInSystem.Remove(currFile);
            }
        }

        // removes any movies assigned to the files in the list
        private void RemoveCommitedRelations(List<DBLocalMedia> fileList) {
            foreach (DBLocalMedia currFile in fileList) {
                RemoveCommitedRelations(currFile);
            }
        }

        private void RemoveCommitedRelations(DBLocalMedia file) {
            foreach (DBMovieInfo currMovie in file.AttachedMovies)
                currMovie.Delete();

            file.AttachedMovies.Clear();
        }


        // Returns a possible match set for the given media file(s) 
        // using the given custom search string
        private void GetMatches(MediaMatch mediaMatch) {
            List<DBMovieInfo> movieList;
            List<PossibleMatch> rankedMovieList = new List<PossibleMatch>();
            string searchStr;

            bool searchCustom = mediaMatch.Custom;        
            if (searchCustom)
            {
              searchStr = mediaMatch.SearchString;
              // Reset the custom search flag 
              // (probably not needed because this object is recreated)
              mediaMatch.Custom = false; 
            }
            else 
            {
              // We are using a system generated search string 
              // The title is alrady parser so let's use that for the search.
              searchStr = mediaMatch.Signature.Title;
            }
            
            string searchIMDB = mediaMatch.Signature.ImdbId;
            int searchYear = mediaMatch.Signature.Year;      

            // notify any listeners we are checking for matches
            if (MovieStatusChanged != null)
                MovieStatusChanged(mediaMatch, MovieImporterAction.GETTING_MATCHES);

            // grab a list of movies from our dataProvider and rank each returned movie on 
            // how close a match it is

            bool imdbFastMatch = (bool)MovingPicturesCore.SettingsManager["importer_fastmatch"].Value;
            bool imdbHint = (!searchCustom && searchIMDB != string.Empty);
            if (imdbHint) {
              // if it's an automated search and we have an IMDB id 
              // do a search for it.
              movieList = MovingPicturesCore.MovieProvider.Get(searchIMDB);
              foreach (DBMovieInfo currMovie in movieList)
              {
                PossibleMatch imdbMatch = new PossibleMatch();
                imdbMatch.Movie = currMovie;
                // if we have a match, and we will probably have it
                // this should be an exact match in most cases so assign the lowest MatchValue
                imdbMatch.MatchValue = 0;
                // add it to the ranked movie list
                rankedMovieList.Add(imdbMatch);
                
                if (imdbFastMatch)
                {
                  // If Fast Match is turned on quit the movie matcher
                  // we probably have the exact match anyway
                  mediaMatch.PossibleMatches = rankedMovieList;
                  return;
                }
              }
            }
            
            // Continue finding matches for a (parsed) title or custom search string.
            movieList = MovingPicturesCore.MovieProvider.Get(searchStr);
            
            bool strictYear = (bool)MovingPicturesCore.SettingsManager["importer_strict_year"].Value;
            Regex rxReplacePunctuation = new Regex(rxPunctuationToSpace, RegexOptions.IgnoreCase);
            Regex rxCleanPunctuation = new Regex(rxPunctuationFilter, RegexOptions.IgnoreCase);
            Regex rxReplaceDoubleSpace = new Regex(rxCompressSpaces, RegexOptions.IgnoreCase);

            // if we have an imdb id add another point to the following results 
            // (maybe not necessary but this is to lower the 
            // priority of the second list of matches
            int matchValue = (imdbHint) ? 1 : 0;
            
            foreach (DBMovieInfo currMovie in movieList) {
                PossibleMatch currMatch = new PossibleMatch();
                currMatch.Movie = currMovie;
                
                // if we have 'year' data to compare add it to the string compare
                // this should give the match a higher priority
                
                string sMovie = currMovie.Title.ToLower().Trim();
                string sSearch = searchStr.ToLower().Trim();
                
                //replace punctuation with spaces
                sMovie = rxReplacePunctuation.Replace(sMovie, " ");
                //filter other punctuation characters completely
                sMovie = rxCleanPunctuation.Replace(sMovie, "");
                //replace multiple spaces with just one space
                sMovie = rxReplaceDoubleSpace.Replace(sMovie, " ");
                                             
                // if both have a year then add the year to the comparison variables
                if (searchYear > 0 && currMovie.Year > 0) {
                    sMovie +=  ' ' + currMovie.Year.ToString();
                    sSearch += ' ' + searchYear.ToString();
                }

                // get the Levenshtein distance between the two string and use them for the match value
                currMatch.MatchValue = matchValue + AdvancedStringComparer.Levenshtein(sMovie, sSearch);
                if ((searchYear > 0) && strictYear)
                {
                    // Strict year match (don't show results that don't match on year)
                    if (currMovie.Year == searchYear || currMovie.Year == 0)
                        rankedMovieList.Add(currMatch);
                }
                else
                {
                    rankedMovieList.Add(currMatch);
                }
              
            }

            mediaMatch.PossibleMatches = rankedMovieList;
        }

        #endregion

    }

    public class MediaMatch {
        
        public bool FolderHint
        {
          get { return _folder; }
          set { _folder = value; }
        } private bool _folder = false;

        public bool Custom
        {
          get { return _custom; }
          set { _custom = value; }
        } private bool _custom = false;
      
        public bool Deleted {
            get { return _deleted; }
            set { _deleted = value; }
        } private bool _deleted = false;

        public bool HighPriority {
            get { return _highPriority; }
            set { _highPriority = value; }
        } private bool _highPriority = false;

        public List<DBLocalMedia> LocalMedia {
            get {
                if (_localMedia == null)
                    _localMedia = new List<DBLocalMedia>();
                return _localMedia; 
            }

            set { _localMedia = value; }
        } private List<DBLocalMedia> _localMedia;

        public string LocalMediaString {
            get {
                if (_localMediaString == string.Empty) {
                    _localMediaString = "";
                    foreach (DBLocalMedia currFile in LocalMedia) {
                        if (_localMediaString.Length > 0)
                            _localMediaString += ", ";

                        _localMediaString += currFile.File.Name;
                    }
                }

                return _localMediaString;
            }
        } private string _localMediaString = string.Empty;

        public string LongLocalMediaString {
            get {
                if (_longLocalMediaString == string.Empty) {
                    _longLocalMediaString = "";
                    foreach (DBLocalMedia currFile in LocalMedia) {
                        if (_longLocalMediaString.Length > 0)
                            _longLocalMediaString += "\n";

                        _longLocalMediaString += currFile.File.FullName;
                    }
                }

                return _longLocalMediaString;
            }
        } private string _longLocalMediaString = string.Empty;

        public List<PossibleMatch> PossibleMatches {
            get {
                if (_possibleMatches == null)
                    _possibleMatches = new List<PossibleMatch>();

                return _possibleMatches; 
            }
            set { 
                _possibleMatches = value;
                if (_possibleMatches != null && _possibleMatches.Count != 0) {
                    _possibleMatches.Sort();
                    Selected = _possibleMatches[0];
                }
            }
        } private List<PossibleMatch> _possibleMatches;

        public PossibleMatch Selected {
            get { return _selected; }
            set { _selected = value; }
        } private PossibleMatch _selected;

        public string SearchString {        
            get {
              if (_searchString.Equals(string.Empty))
              {
                _searchString = Signature.Title + ((Signature.Year > 0) ? " " + Signature.Year : "") + ((Signature.ImdbId != null) ? " " + Signature.ImdbId : "");
              }                
              return _searchString;
            }

            set {
                _searchString = value;
            }
        } private string _searchString = string.Empty;

        public MediaSignature Signature
        {
          get
          {
            if (_signature.Title == null)
              _signature = LocalMediaParser.parseMediaMatch(this);
            return _signature;
          }
          set
          {
            _signature = value;
          }
        }
        private MediaSignature _signature;

    }

    public class PossibleMatch: IComparable {
        private DBMovieInfo movie;
        private int matchValue = int.MaxValue;

        public DBMovieInfo Movie {
            get { return movie; }
            set { movie = value; }
        }

        public int MatchValue {
            get { return matchValue; }
            set { matchValue = value; }
        }

        // This is silly, but required for how the DataGridView ComboBox Cell handles data.
        public PossibleMatch ValueMember {
            get { return this; }
        }

        // see previous comment
        public String DisplayMember {
            get {
              if (this.movie.Year > 0)
              {
                // if we have a year value for the possible match include it in the display member
                return ToString() + " (" + this.movie.Year.ToString() + ")";
              }
              else
              {
                return ToString();
              }
            }
        }

        public int CompareTo(object o) {
            if (o.GetType() != typeof(PossibleMatch))
                return 0;

            if (this.matchValue < ((PossibleMatch)o).matchValue)
                return -1;
            if (this.matchValue == ((PossibleMatch)o).matchValue)
                return ((PossibleMatch)o).movie.Popularity.CompareTo(this.movie.Popularity);
            else
                return 1;
        }

        public override string ToString() {
            if (movie != null)
              return movie.Title;
            return "";
        }
    }

}
