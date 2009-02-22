using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using Cornerstone.Tools;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.DataProviders;
using MediaPortal.Plugins.MovingPictures.SignatureBuilders;
using MediaPortal.Profile;
using NLog;

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
        MANUAL,
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
        private Thread artworkUpdaterThread;

        private int percentDone;

        // a list of all files currently in the system
        private Dictionary<DBLocalMedia, MovieMatch> matchLookup;
        private ArrayList allMatches;

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

        // Matches that have been ignored/committed and saved to the database. 
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
        List<DBImportPath> watcherQueue;
        List<DBImportPath> rescanQueue;
        Dictionary<FileSystemWatcher, DBImportPath> pathLookup;
        int watcherInterval;

        #endregion

        // sends progress update events to any available listeners
        public delegate void ImportProgressHandler(int percentDone, int taskCount, int taskTotal, string taskDescription);
        public event ImportProgressHandler Progress;

        // updates listeners of changes to the match lists. This will not provide info on when
        // pending matches are externally approved.
        public delegate void MovieStatusChangedHandler(MovieMatch obj, MovieImporterAction action);
        public event MovieStatusChangedHandler MovieStatusChanged;

        // Creates a MovieImporter object which will scan ImportPaths and import new media.
        public MovieImporter() {
            initialize();
            MovingPicturesCore.DatabaseManager.ObjectDeleted += new DatabaseManager.ObjectAffectedDelegate(DatabaseManager_ObjectDeleted);
            DeviceManager.OnVolumeInserted += new DeviceManager.DeviceManagerEvent(OnVolumeInserted);
            percentDone = 0;
        }

        private void initialize() {
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

            matchLookup = new Dictionary<DBLocalMedia, MovieMatch>();
            allMatches = ArrayList.Synchronized(new ArrayList());

            watcherQueue = new List<DBImportPath>();
            rescanQueue = new List<DBImportPath>();
            watcherInterval = 30;
            fileSystemWatchers = new List<FileSystemWatcher>();
            pathLookup = new Dictionary<FileSystemWatcher, DBImportPath>();
        }

        ~MovieImporter() {
            Stop();
        }

        #region Public Methods

        public void Start() {
            int maxThreadCount = MovingPicturesCore.Settings.ThreadCount;

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

            if (artworkUpdaterThread == null) {
                artworkUpdaterThread = new Thread(new ThreadStart(LookForMissingArtworkWorker));
                artworkUpdaterThread.Name = "Artwork Updater";
                artworkUpdaterThread.Start();
            }

            if (MovieStatusChanged != null)
                MovieStatusChanged(null, MovieImporterAction.STARTED);
        }

        public void Stop() {
            lock (mediaScannerThreads) {
                bool stoppedSomething = false;

                if (mediaScannerThreads.Count > 0) {
                    logger.Info("Shutting Down Media Scanner Threads...");
                    foreach (Thread currThread in mediaScannerThreads)
                        currThread.Abort();

                    // wait for all threads to shut down
                    bool waiting = true;
                    while (waiting) {
                        waiting = false;
                        foreach (Thread currThread in mediaScannerThreads)
                            waiting = waiting || currThread.IsAlive;
                        Thread.Sleep(100);
                    }

                    mediaScannerThreads.Clear();
                    stoppedSomething = true;
                }

                if (fileSystemWatchers != null && fileSystemWatchers.Count > 0) {
                    foreach (FileSystemWatcher currWatcher in fileSystemWatchers) {
                        currWatcher.EnableRaisingEvents = false;
                        currWatcher.Created -= OnFileAdded;
                        currWatcher.Deleted -= OnFileDeleted;
                        currWatcher.Renamed -= OnFileRenamed;
                    }

                    fileSystemWatchers.Clear();
                    pathLookup.Clear();
                    watcherQueue.Clear();
                    rescanQueue.Clear();
                }

                if (pathScannerThread != null) {
                    logger.Info("Shutting Down Path Scanner Thread...");
                    pathScannerThread.Abort();

                    // wait for the path scanner to shut down
                    while (pathScannerThread.IsAlive)
                        Thread.Sleep(100);

                    pathScannerThread = null;
                    stoppedSomething = true;
                }

                if (artworkUpdaterThread != null) {
                    if (artworkUpdaterThread.IsAlive) {
                        logger.Info("Shutting Down Artwork Updater Thread...");
                        artworkUpdaterThread.Abort();

                        // wait for the path scanner to shut down
                        while (artworkUpdaterThread.IsAlive)
                            Thread.Sleep(100);

                        stoppedSomething = true;
                    }

                    artworkUpdaterThread = null;
                }

                if (stoppedSomething) {
                    if (Progress != null)
                        Progress(100, 0, 0, "Stopped");

                    if (MovieStatusChanged != null)
                        MovieStatusChanged(null, MovieImporterAction.STOPPED);

                    logger.Info("Stopped MovieImporter");
                }
            }
        }

        public bool IsScanning {
            get {
                return (mediaScannerThreads.Count != 0);
            }
        }

        public void RestartScanner() {
            this.Stop();
            this.initialize();
            this.Start();
        }

        // This method is written weird and needs to be clarified. But I think it works, 
        // reloading specified files.
        public void Reprocess(List<DBLocalMedia> fileList) {
            List<DBLocalMedia> fileSet = new List<DBLocalMedia>();
            foreach (DBLocalMedia currFile in new List<DBLocalMedia>(fileList)) {
                // if file is already in importer, reload if requested
                if (matchLookup.ContainsKey(currFile)) {
                    Reprocess(matchLookup[currFile]);
                    ScanFiles(fileSet, true);
                    fileSet = new List<DBLocalMedia>();
                    continue;
                }

                // if file is already commited but not in importer, remove all relations
                // and remove the file from the DB, then queue up for scanning
                if (currFile.ID != null) {
                    RemoveCommitedRelations(currFile);
                    currFile.Delete();
                    fileSet.Add(currFile);
                }
            }

            ScanFiles(fileSet, true);
        }

        // Approves the MovieMatch for detail processing and commit. THis shold be
        // used in conjunction with the MatchListChanged event when a NEED_INPUT action 
        // is received. 
        public void Approve(MovieMatch match) {
            if (match.Selected == null)
                return;

            RemoveFromMatchLists(match);

            // clear the ignored flag in case these files were previously on the disable list
            foreach (DBLocalMedia currFile in match.LocalMedia) {
                currFile.Ignored = false;
            }

            // select the list to add this match to based on priority
            ArrayList approveList;
            if (match.HighPriority) approveList = priorityApprovedMatches;
            else approveList = approvedMatches;

            lock (approveList.SyncRoot) {
                approveList.Insert(0, match);
                allMatches.Add(match);
                foreach (DBLocalMedia currFile in match.LocalMedia)
                    matchLookup[currFile] = match;
            }

            // notify any listeners of the status change
            logger.Info("User approved " + match.LocalMediaString + "as " + match.Selected.Movie.Title);
            if (MovieStatusChanged != null)
                MovieStatusChanged(match, MovieImporterAction.APPROVED);
        }

        // removes any association with the file(s) in the MovieMatch and flags the files
        // to be ignored in the future.
        public void Ignore(MovieMatch match) {
            RemoveFromMatchLists(match);
            RemoveCommitedRelations(match.LocalMedia);

            foreach (DBLocalMedia currFile in match.LocalMedia) {
                currFile.Ignored = true;
                currFile.Commit();
            }

            // add match to the committed list
            commitedMatches.Add(match);

            // notify any listeners of the status change
            logger.Info("User ignored " + match.LocalMediaString);
            if (MovieStatusChanged != null)
                MovieStatusChanged(match, MovieImporterAction.IGNORED);
        }

        // rescans for possible movie matches using the specified search string
        public void Reprocess(MovieMatch match) {
            RemoveFromMatchLists(match);

            if (match.ExistingMovieInfo == null)
                RemoveCommitedRelations(match.LocalMedia);

            // clear the ignored flag in case these files were previously on the disable list
            foreach (DBLocalMedia currFile in match.LocalMedia) {
                currFile.Ignored = false;
            }

            lock (priorityPendingMatches) {
                match.HighPriority = true;
                priorityPendingMatches.Add(match);
                allMatches.Add(match);
                foreach (DBLocalMedia currFile in match.LocalMedia)
                    matchLookup[currFile] = match;
            }

            // notify any listeners of the status change
            logger.Info("User reprocessing " + match.LocalMediaString);
            if (MovieStatusChanged != null)
                MovieStatusChanged(match, MovieImporterAction.PENDING);
        }

        // takes the given match containing multiple files and splits it up into
        // individual matches for each file
        public void Split(MovieMatch match) {
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
                // clear the ignored flag in case these files were previously on the disable list
                currFile.Ignored = false;

                MovieMatch newMatch = new MovieMatch();
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
        public void Join(List<MovieMatch> matchList) {
            if (matchList == null || matchList.Count < 2)
                return;

            List<DBLocalMedia> fileList = new List<DBLocalMedia>();

            // build the file list and clear out old matches
            foreach (MovieMatch currMatch in matchList) {
                RemoveFromMatchLists(currMatch);
                RemoveCommitedRelations(currMatch.LocalMedia);
                currMatch.Deleted = true;
                fileList.AddRange(currMatch.LocalMedia);

                // notify any listeners of the status change
                if (MovieStatusChanged != null)
                    MovieStatusChanged(currMatch, MovieImporterAction.REMOVED_FROM_JOIN);
            }

            // build the new match and add it for processing
            MovieMatch newMatch = new MovieMatch();
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

        public void ManualAssign(MovieMatch match) {
            if (match.Selected == null)
                return;

            // remove match from all lists
            RemoveFromMatchLists(match);

            // clear the ignored flag in case these files were previously on the disable list
            foreach (DBLocalMedia currFile in match.LocalMedia) {
                currFile.Ignored = false;
            }

            // assign files to movie
            AssignAndCommit(match, false);

            // add match to the committed list
            commitedMatches.Add(match);

            // notify any listeners of the status change
            logger.Info("User manually assigned " + match.LocalMediaString + "as " + match.Selected.Movie.Title);
            if (MovieStatusChanged != null)
                MovieStatusChanged(match, MovieImporterAction.MANUAL);
        }

        // This will add the specified movie to the importer for reprocessing, using the 
        // specified data source. 
        public void Update(DBMovieInfo movie, DBSourceInfo source) {
            MovieMatch newMatch = new MovieMatch();
            newMatch.ExistingMovieInfo = movie;
            newMatch.PreferedDataSource = source;
            newMatch.LocalMedia = movie.LocalMedia;

            if (matchLookup.ContainsKey(movie.LocalMedia[0]))
                RemoveFromMatchLists(matchLookup[movie.LocalMedia[0]]);

            lock (priorityPendingMatches.SyncRoot) {
                priorityPendingMatches.Add(newMatch);
            }

            allMatches.Add(newMatch);
            foreach (DBLocalMedia subFile in movie.LocalMedia)
                matchLookup.Add(subFile, newMatch);

            if (MovieStatusChanged != null)
                MovieStatusChanged(newMatch, MovieImporterAction.ADDED);
        }

        private void LookForMissingArtworkWorker() {
            try {
                foreach (DBMovieInfo currMovie in DBMovieInfo.GetAll()) {
                    try {
                        if (currMovie.ID == null)
                            continue;

                        if (currMovie.CoverFullPath.Trim().Length == 0) {
                            MovingPicturesCore.DataProviderManager.GetArtwork(currMovie);
                            currMovie.Commit();
                        }

                        if (currMovie.BackdropFullPath.Trim().Length == 0) {
                            new LocalProvider().GetBackdrop(currMovie);
                            MovingPicturesCore.DataProviderManager.GetBackdrop(currMovie);
                            currMovie.Commit();
                        }
                    }
                    catch (Exception e) {
                        if (e is ThreadAbortException)
                            throw e;

                        logger.ErrorException("Error retrieving artwork for " + currMovie.Title, e);
                    }

                }
            }
            catch (ThreadAbortException) {
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
                    int checkWatchers= 0;

                    // do an initial scan on all paths
                    // grab all the files in our import paths
                    int count = 0;
                    List<DBImportPath> paths = DBImportPath.GetAll();
                    foreach (DBImportPath currPath in paths) {
                        if (currPath.Active) {
                            count++;
                            if (Progress != null)
                                Progress((int)(count * 100.0 / paths.Count), count, paths.Count, "Scanning local media sources...");

                            ScanPath(currPath);
                        }
                    }
                    if (Progress != null) Progress(100, 0, 0, "Done!");


                    // monitor existing paths for change
                    while (true) {
                        Thread.Sleep(1000);


                        // if the filesystem scanner found any files, add them
                        lock (filesAdded.SyncRoot) {
                            if (filesAdded.Count > 0) {
                                List<DBLocalMedia> fileList = new List<DBLocalMedia>();
                                foreach (object currFile in filesAdded) {
                                    DBLocalMedia addedFile = (DBLocalMedia)currFile;
                                    if (!fileList.Contains(addedFile))
                                        fileList.Add((DBLocalMedia)currFile);
                                }

                                ScanFiles(fileList, false);
                                filesAdded.Clear();
                            }
                        }

                        checkWatchers++;
                        if (checkWatchers == watcherInterval) {
                            // check the watchers
                            UpdateFileSystemWatchers();
                            checkWatchers = 0;
                        }

                    }

                }
            }
            catch (ThreadAbortException) {
            }
        }     

        public void OnVolumeInserted(string volume, string serial) {
            // Check if this volume has an import path
            // if so rescan these import paths
            foreach (DBImportPath importPath in DBImportPath.GetAll()) {
                if (importPath.Active && importPath.GetDiskSerial() == serial)
                    ScanPath(importPath);
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
            
            // fill the watcher queue with import paths
            foreach (DBImportPath currPath in paths)
                watcherQueue.Add(currPath);

            // Actually add the file system watchers
            UpdateFileSystemWatchers();
        }

        private void UpdateFileSystemWatchers() {
            if (watcherQueue.Count > 0) {
                foreach (DBImportPath importPath in watcherQueue.ToArray()) {
                    FileSystemWatcher watcher = new FileSystemWatcher();
                    bool success = false;
                    try {
                        // nothing will change on CD/DVD so skip these
                        // else try to create a watcher
                        if (importPath.GetDriveType() != DriveType.CDRom)
                            WatchImportPath(watcher, importPath);

                        success = true;
                    }
                    catch (Exception e) {
                        // if the path is not available
                        if (!importPath.IsAvailable) {
                            if (importPath.IsRemovable) {
                                // if it's removable just leave a message
                                logger.Info("Failed watching: '{0}' ({1}) - Path is currently offline.", importPath.FullPath, importPath.GetDriveType().ToString());
                                if (!rescanQueue.Contains(importPath))
                                    rescanQueue.Add(importPath);
                            }
                            else {
                                // if it's not removable do not process it anymore
                                logger.Info("Cancelled watching: '{0}' ({1}) - Path does not exist (anymore).", importPath.FullPath, importPath.GetDriveType().ToString());
                                watcherQueue.Remove(importPath);
                                if (rescanQueue.Contains(importPath))
                                    rescanQueue.Remove(importPath);
                            }
                        }
                        else { // Other kind of error?
                            logger.Debug("FileSystemWatcher error: {0}", e.Message.ToString());
                        }
                    }
                    
                    // If starting the watcher was succesful 
                    // remove it from startup queue
                    if (success) {
                        watcherQueue.Remove(importPath);
                        if (rescanQueue.Contains(importPath)) {
                            // rescan the import path
                            ScanPath(importPath);
                            // remove it from the rescan queue
                            rescanQueue.Remove(importPath);
                        }
                    }
                }
            }
        }

        private void WatchImportPath(FileSystemWatcher watcher, DBImportPath importPath) {
            // todo: figure out if Renamed has to be in this list
            watcher.Path = importPath.FullPath;
            watcher.IncludeSubdirectories = true;
            watcher.Error += OnWatcherError;
            watcher.Created += OnFileAdded;
            watcher.Deleted += OnFileDeleted;
            watcher.Renamed += OnFileRenamed;
            watcher.EnableRaisingEvents = true;
            
            fileSystemWatchers.Add(watcher);
            pathLookup[watcher] = importPath;
            logger.Info("Started watching '{0}' ({1}) - Path is now being monitored for changes.", importPath.FullPath, importPath.GetDriveType().ToString());
        }

        // When a FileSystemWatcher gets corrupted this handler will add it to the queue again
        private void OnWatcherError(object source, ErrorEventArgs e) {
            Exception watchException = e.GetException();
            FileSystemWatcher watcher = (FileSystemWatcher)source;
            DBImportPath importPath = pathLookup[watcher];
            logger.Info("Stopped watching '{0}' ({1}) - Reason: {2}", importPath, importPath.GetDriveType(), watchException.Message);

            // remove the watcher from the lookups
            if (pathLookup.ContainsKey(watcher))
                pathLookup.Remove(watcher);
            if (fileSystemWatchers.Contains(watcher))
                fileSystemWatchers.Remove(watcher);

            // Clean the old watcher (?)
            watcher.Dispose();

            // Add the importPath to the watcher queue
            watcherQueue.Add(importPath);
            
            // Add the importPath to the rescan queue
            if (!rescanQueue.Contains(importPath))
                rescanQueue.Add(importPath);
        }

        // When a FileSystemWatcher detects a new file, this method queues it up for processing.
        private void OnFileAdded(Object source, FileSystemEventArgs e) {
            List<FileInfo> filesCreated = new List<FileInfo>();
            
            if (File.Exists(e.FullPath)) {
                // This is just one file so add it to the list if it's a video file
                FileInfo file = new FileInfo(e.FullPath);
                if (Utility.IsVideoFile(file))
                    filesCreated.Add(file);
            }
            else {
                // This is a directory so scan it and add create the (video) filelist
                filesCreated = Utility.GetVideoFilesRecursive(new DirectoryInfo(e.FullPath));
            }

            // If we have a list of video files process them
            if (filesCreated.Count > 0) {

                // Get the importPath attached to this event
                DBImportPath importPath = pathLookup[(FileSystemWatcher)source];

                // Process all files that were created
                foreach (FileInfo videoFile in filesCreated) {
                    // Check if the file already exists in our system
                    DBLocalMedia newFile = DBLocalMedia.Get(videoFile.FullName, DeviceManager.GetDiskSerial(videoFile.FullName));
                    if (newFile.ID != null) {
                        // if this file is already in the system, ignore and log a message.
                        if (importPath.IsRemovable)
                            logger.Info("Removable file " + newFile.File.Name + " brought online.");
                        else
                            logger.Warn("Watcher tried to add a pre-existing file: " + newFile.File.Name);
                        continue;
                    }
                    // We have a new file so add it to the filesAdded list
                    newFile.ImportPath = importPath;
                    newFile.UpdateDiskProperties();
                    lock (filesAdded.SyncRoot) filesAdded.Add(newFile);
                    logger.Info("Watcher queued " + newFile.File.Name + " for processing.");
                }
            }
        }

        // When a FileSystemWatcher detects a file has been removed, delete it.
        private void OnFileDeleted(Object source, FileSystemEventArgs e) {
            List<DBLocalMedia> localMediaRemoved = new List<DBLocalMedia>();

            if (File.Exists(e.FullPath)) {
                // This is just one file so add it to the list
                localMediaRemoved.Add(DBLocalMedia.Get(e.FullPath, DeviceManager.GetDiskSerial(e.FullPath)));
            }
            else {
                // This is a directory so we are going to get all localmedia that uses 
                // this directory we can do this by adding a % character to the end of 
                // the path as the sqlite query behind it uses LIKE as operator.
                localMediaRemoved = DBLocalMedia.GetAll(e.FullPath + '%', DeviceManager.GetDiskSerial(e.FullPath));
            }

            // Loop through the remove files list and process
            foreach (DBLocalMedia removedFile in localMediaRemoved) {
                logger.Info("Watcher flagged " + removedFile.File.Name + " for removal from the database.");
                
                // if the file is not in our system anymore there's nothing to do
                // todo: this is not entirely true because the file could be sitting in the match system
                // waiting for user input  we would have to  remove it there also.
                if (removedFile.ID == null)
                    continue;

                // Check if the file is really removed
                if (!removedFile.IsRemoved) {
                    logger.Info("Removable file " + removedFile.File.Name + " taken offline.");
                    continue;
                }

                // Remove the file
                removedFile.Delete();
            }
        }

        private void OnFileRenamed(object source, RenamedEventArgs e) {
            List<DBLocalMedia> localMediaRenamed = new List<DBLocalMedia>();

            if (File.Exists(e.FullPath)) {
                // This is just one file so add it to the list
                localMediaRenamed.Add(DBLocalMedia.Get(e.OldFullPath, DeviceManager.GetDiskSerial(e.FullPath)));
                logger.Info("Watched file '{0}' was renamed to '{1}'", e.OldFullPath, e.FullPath);
            }
            else {
                // This is a directory so we are going to get all localmedia that uses 
                // this directory we can do this by adding a % character to the end of 
                // the path as the sqlite query behind it uses LIKE as operator.
                localMediaRenamed = DBLocalMedia.GetAll(e.OldFullPath + '%', DeviceManager.GetDiskSerial(e.FullPath));
                logger.Info("Watched folder '{0}' was renamed to '{1}'", e.OldFullPath, e.FullPath);
            }

            // Loop through the renamed files list and process
            int renamed = 0;
            foreach (DBLocalMedia renamedFile in localMediaRenamed) {
                renamedFile.FullPath = renamedFile.FullPath.Replace(e.OldFullPath, e.FullPath);
                renamedFile.Commit();
                renamed++;
            }

            logger.Info("Watcher updated {0} local media records that were affected by a rename event.", renamed);
        }

        private void OnFileChanged(object source, FileSystemEventArgs e) {
            // Specify what is done when a file is changed, created, or deleted.
            logger.Info("File: " + e.FullPath + " " + e.ChangeType);
        }




        // Grabs the files from the DBImportPath and add them to the queue for use
        // by the ScanMedia thread.
        private void ScanPath(DBImportPath importPath) {
            ScanFiles(importPath.GetNewLocalMedia(), false);
        }

        // Adds the files to the importer for processing. If a file has recently been commited 
        // and it's readded, it will be reprocessed.
        private void ScanFiles(List<DBLocalMedia> importFileList, bool highPriority) {
            if (importFileList == null)
                return;

            List<DBLocalMedia> currFileSet = new List<DBLocalMedia>();
            bool alwaysGroup = MovingPicturesCore.Settings.AlwaysGroupByFolder;

            foreach (DBLocalMedia currFile in importFileList) {

                // if we have already loaded this file, move to the next
                if (currFile.ID != null) {
                    logger.Debug("Skipping " + currFile.File.Name + " because it is already in the system.");
                    continue;
                }

                // File is already in the matching system
                if (matchLookup.ContainsKey(currFile)) {
                    logger.Debug("Skipping " + currFile.File.Name + " because it's being matched.");
                    continue;
                }
                
                // exclude samplefiles
                if (Utility.isSampleFile(currFile.File)) {
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

                    DirectoryInfo currentDir = currFile.File.Directory;
                    DirectoryInfo otherDir = otherFile.File.Directory;

                    // if both files are located in folders marked as multi-part folders
                    if (Utility.isFolderMultipart(currentDir.Name) && Utility.isFolderMultipart(otherDir.Name)) {
                        // check if they share the same parent folder, if not then they are not a pair
                        if (!currentDir.Parent.FullName.Equals(otherDir.Parent.FullName)) {
                            isAdditionalMatch = false;
                            break;
                        }
                    }
                    else {
                        // if files are not in the same folder we assume they are not a pair
                        if (!currFile.File.DirectoryName.Equals(otherFile.File.DirectoryName)) {
                            isAdditionalMatch = false;
                            break;
                        }
                    }

                    // if the setting always group files in the same folder is used just group them 
                    // without checking differences at all.
                    // @todo: maybe place this below the character differences count?
                    if (alwaysGroup)
                        break;

                    // if the filename differ by more than two characters
                    // assume they are not a pair
                    if (AdvancedStringComparer.Levenshtein(currFile.File.Name, otherFile.File.Name) > 2) {
                        isAdditionalMatch = false;
                        break;
                    }

                    // if the multi-part naming convention doesn't match up
                    // assume they are not a pair
                    if (!Utility.isFileMultiPart(currFile.File)) {
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
                    MovieMatch newMatch = new MovieMatch();
                    newMatch.LocalMedia = currFileSet;

                    lock (pendingMatches.SyncRoot) {
                        pendingMatches.Add(newMatch);
                    }

                    allMatches.Add(newMatch);
                    foreach (DBLocalMedia subFile in currFileSet)
                        matchLookup.Add(subFile, newMatch);

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
                MovieMatch newMatch = new MovieMatch();
                newMatch.LocalMedia = currFileSet;
                lock (pendingMatches.SyncRoot) {
                    pendingMatches.Add(newMatch);
                }

                allMatches.Add(newMatch);
                foreach (DBLocalMedia subFile in currFileSet)
                    matchLookup.Add(subFile, newMatch);

                if (MovieStatusChanged != null)
                    MovieStatusChanged(newMatch, MovieImporterAction.ADDED);

                if (highPriority)
                    Reprocess(newMatch);
            }

        }

        // When a process has removed a local file from the database, we should remove it from the matching system
        private void DatabaseManager_ObjectDeleted(DatabaseTable obj) {
            if (obj is DBLocalMedia)
                if (matchLookup.ContainsKey((DBLocalMedia)obj))
                    RemoveFromMatchLists(matchLookup[(DBLocalMedia)obj]);
        }

        #endregion

        #region Media Matcher

        // Monitors the mediaQueue for files imported from the ScanAndMonitorPaths thread. When elements
        // exist, possble matches will be imported and subsequently written to the database.
        private void ScanMedia() {
            try {
                while (true) {
                    int previousCommittedCount = commitedMatches.Count;

                    // if there is nothing to process, then sleep
                    while (pendingMatches.Count == 0 &&
                           approvedMatches.Count == 0 &&
                           priorityPendingMatches.Count == 0 &&
                           priorityApprovedMatches.Count == 0 &&
                           commitedMatches.Count == previousCommittedCount)
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
                    else if (approvedMatches.Count > 0)
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

                        if (Progress != null) {
                            UpdatePercentDone();
                            if (percentDone == 100)
                                Progress(100, 0, 0, "Done!");
                        }
                    }


                }

            }
            catch (ThreadAbortException) {
                // expected when threads shutdown. disable.
            }
            catch (Exception e) {
                logger.FatalException("Unhandled error in MediaScanner.", e);
            }
        }

        // updates the local variables of the current progress
        private void UpdatePercentDone() {
            // calculate the total actions that need to be performed this session
            int mediaToScan = allMatches.Count;
            int mediaToCommit = allMatches.Count - matchesNeedingInput.Count;
            int totalActions = mediaToScan + mediaToCommit;
            
            // if there is nothing to do, set progress to 100%
            if (totalActions == 0) {
                percentDone = 100;
                return;
            }
            
            // calculate the number of actions completed so far
            int mediaScanned = allMatches.Count - pendingMatches.Count - priorityPendingMatches.Count;
            int mediaCommitted = commitedMatches.Count;

            percentDone = (int)Math.Round(((double)mediaScanned + mediaCommitted) * 100 / ((double)totalActions));

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
            MovieMatch currMatch;
            lock (matchList.SyncRoot) {
                if (matchList.Count == 0)
                    return;

                currMatch = (MovieMatch)matchList[0];
                matchList.Remove(currMatch);
                retrievingDetailsMatches.Add(currMatch);
            }

            // notify the user we are processing
            logger.Info("Retrieving details for \"" + currMatch.Selected.Movie.Title + "\"");
            if (Progress != null) {
                UpdatePercentDone();
                int processed = commitedMatches.Count;
                int total = commitedMatches.Count + approvedMatches.Count + matchesNeedingInput.Count;
                Progress(percentDone, processed, total, "Retrieving details for: " + currMatch.Selected.Movie.Title);
            }

            // notify any listeners of the status change
            if (MovieStatusChanged != null)
                MovieStatusChanged(currMatch, MovieImporterAction.GETTING_DETAILS);

            // commit the match and move it to the commited array
            AssignAndCommit(currMatch, true);
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
            MovieMatch mediaMatch = null;

            // check for a match needing reprocessing
            lock (priorityPendingMatches.SyncRoot) {
                if (priorityPendingMatches.Count != 0) {
                    // grab match
                    mediaMatch = (MovieMatch)priorityPendingMatches[0];
                    priorityPendingMatches.Remove(mediaMatch);
                }
            }

            // if no reprocessing matches available, get next pending match
            if (mediaMatch == null)
                lock (pendingMatches.SyncRoot) {
                    if (pendingMatches.Count == 0)
                        return;

                    mediaMatch = (MovieMatch)pendingMatches[0];
                    pendingMatches.Remove(mediaMatch);
                }


            // if we have any listeners, notify them of our status
            if (Progress != null) {
                UpdatePercentDone();
                int processed = allMatches.Count - pendingMatches.Count;
                int total = allMatches.Count;

                if (mediaMatch.LocalMedia.Count == 1)
                    Progress(percentDone, processed, total, "Retrieving possible matches: " + mediaMatch.LocalMedia[0].File.Name);
                else
                    Progress(percentDone, processed, total, "Retrieving possible matches: " + mediaMatch.LocalMedia[0].File.Directory.Name + "\\");
            }

            // get possible matches for this set of media files
            GetMatches(mediaMatch);

            // if the match has been set to disable by the user while searching, cancel out
            if (mediaMatch.LocalMedia[0].Ignored) {
                Ignore(mediaMatch);
                return;
            }

            // if match has been deleted while processing (usually from a split or merge)
            // kick back out.
            if (mediaMatch.Deleted)
                return;

            // Extra logging to debug movie matching
            logger.Debug("Built MediaSignature: {0}", mediaMatch.Signature.ToString());

            // if the best match is exact or very close, place it in the accepted queue
            // otherwise place it in the pending queue for approval
            int threshold = MovingPicturesCore.Settings.AutoApproveThreshold;
            if (mediaMatch.Selected != null && mediaMatch.Selected.MatchValue <= threshold) {
                if (mediaMatch.HighPriority) priorityApprovedMatches.Add(mediaMatch);
                else approvedMatches.Add(mediaMatch);

                // notify any listeners
                logger.Info("Auto-approved " + mediaMatch.LocalMediaString + " as " + mediaMatch.Selected.Movie.Title);
                if (MovieStatusChanged != null)
                    MovieStatusChanged(mediaMatch, MovieImporterAction.APPROVED);
            }
            else {
                matchesNeedingInput.Add(mediaMatch);
                logger.Info("No exact match for " + mediaMatch.LocalMediaString);
                if (MovieStatusChanged != null)
                    MovieStatusChanged(mediaMatch, MovieImporterAction.NEED_INPUT);
            }
        }

        // Associates the given file(s) to the given movie object. Also creates all
        // relevent user related data.
        private void AssignFileToMovie(IList<DBLocalMedia> localMedia, DBMovieInfo movie, bool update) {
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

            movie.LocalMedia.Clear();
            movie.LocalMedia.AddRange(localMedia);

            // update, associate, and commit the movie
            if (update) {
                MovingPicturesCore.DataProviderManager.Update(movie);
                MovingPicturesCore.DataProviderManager.GetArtwork(movie);
                MovingPicturesCore.DataProviderManager.GetBackdrop(movie);
            }

            foreach (DBLocalMedia currFile in localMedia)
                currFile.CommitNeeded = false;


            // create user related data object for each user
            movie.UserSettings.Clear();
            foreach (DBUser currUser in DBUser.GetAll()) {
                DBUserMovieSettings userSettings = new DBUserMovieSettings();
                userSettings.User = currUser;
                userSettings.Commit();
                movie.UserSettings.Add(userSettings);
                userSettings.CommitNeeded = false;
            }

            if (movie.LocalMedia[0].File.Extension.ToLower() == ".ifo")
                movie.DateAdded = DateTime.Now;
            else
                movie.DateAdded = movie.LocalMedia[0].File.CreationTime;
            
            movie.Commit();
        }

        private void AssignAndCommit(MovieMatch match, bool update) {
            lock (match) {
                // if we already have a movie object with assigned files, just update
                if (match.ExistingMovieInfo != null && update) {
                    DBMovieInfo movie = match.ExistingMovieInfo;

                    // Using IMovieProvider
                    string siteID = match.Selected.Movie.GetSourceMovieInfo(match.PreferedDataSource).Identifier;
                    movie.GetSourceMovieInfo(match.PreferedDataSource).Identifier = siteID;

                    // and update from that
                    match.PreferedDataSource.Provider.Update(movie);
                    movie.Commit();
                }

                // no movie object exists so go ahead and assign our retrieved details.
                else {
                    AssignFileToMovie(match.LocalMedia, match.Selected.Movie, update);
                }
            }
        }

        // removes the given match from all pending process lists
        private void RemoveFromMatchLists(MovieMatch match) {
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
                if (matchLookup.ContainsKey(currFile)) {
                    matchLookup.Remove(currFile);
                    allMatches.Remove(match);
                }
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
        private void GetMatches(MovieMatch mediaMatch) {
            List<DBMovieInfo> movieList;
            List<PossibleMatch> rankedMovieList = new List<PossibleMatch>();

            // Get the MovieSignature
            MovieSignature signature = mediaMatch.Signature;

            // notify any listeners we are checking for matches
            if (MovieStatusChanged != null)
                MovieStatusChanged(mediaMatch, MovieImporterAction.GETTING_MATCHES);

            // grab a list of movies from our dataProvider and rank each returned movie on 
            // how close a match it is
            
            if (mediaMatch.PreferedDataSource != null)
                movieList = mediaMatch.PreferedDataSource.Provider.Get(signature);
            else
                movieList = MovingPicturesCore.DataProviderManager.Get(signature);

            bool strictYear = MovingPicturesCore.Settings.StrictYear;

            foreach (DBMovieInfo currMovie in movieList) {

                // Skip this movie if the strict year setting is on and the years don't match
                if (strictYear) // todo: refactor / rethink / relocate
                    if ((currMovie.Year != signature.Year) && (currMovie.Year != 0) && (signature.Year != 0))
                        continue;

                // Create a Possible Match object
                PossibleMatch currMatch = new PossibleMatch();

                // Add the movie
                currMatch.Movie = currMovie;

                // Get the matching score for this movie
                currMatch.MatchValue = signature.MatchScore(currMovie);

                // Add the match to the ranked movie list
                rankedMovieList.Add(currMatch);
            }

            mediaMatch.PossibleMatches = rankedMovieList;
        }

        #endregion

    }

    public class MovieMatch {

        public DBMovieInfo ExistingMovieInfo {
            get { return _existingMovieInfo; }
            set { _existingMovieInfo = value; }
        } private DBMovieInfo _existingMovieInfo = null;

        public DBSourceInfo PreferedDataSource {
            get { return _preferedDataSource; }
            set { _preferedDataSource = value; }
        } private DBSourceInfo _preferedDataSource = null;

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

                        string displayname = currFile.File.Name;

                        // if this is a ripped video disc type show the base directory as display name
                        if (displayname.ToLower() == "video_ts.ifo" || displayname.ToLower() == "index.bdmv" || displayname.ToLower() == "discid.dat")
                            displayname = Utility.GetMovieBaseDirectory(currFile.File.Directory).Name;

                        // If read from optical drive
                        if (currFile.ImportPath.GetDriveType() == DriveType.CDRom && displayname.Length == 3) {
                            VideoDiscFormat videoDisc = Utility.GetVideoDiscFormat(currFile.FullPath);
                            // Add the video disc type and media label
                            if (videoDisc != VideoDiscFormat.Unknown)
                                displayname = String.Format("({0}) <{1}>", videoDisc.ToString(), currFile.MediaLabel);                                                      
                        }
                        _localMediaString += displayname;
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

        public MovieSignature Signature {
            get {
                if (_signature == null)
                    if (_existingMovieInfo != null)
                        _signature = new MovieSignature(_existingMovieInfo);
                    else
                        _signature = MovieSignatureProvider.parseMediaMatch(this);

                return _signature;
            }
            set {
                _signature = value;
            }
        }
        private MovieSignature _signature;

    }

    public class PossibleMatch : IComparable {
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
                if (this.movie.Year > 0) {
                    // if we have a year value for the possible match include it in the display member
                    return ToString() + " (" + this.movie.Year.ToString() + ")";
                }
                else {
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