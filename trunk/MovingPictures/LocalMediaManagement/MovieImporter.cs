using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;
using MediaPortal.Plugins.MovingPictures.DataProviders;
using System.Threading;
using System.Collections;
using MediaPortal.Plugins.MovingPictures.Database;
using System.Text.RegularExpressions;
using NLog;
using Cornerstone.Database.Tables;
using Cornerstone.Database;
using Cornerstone.Tools;
using MediaPortal.Plugins.MovingPictures.SignatureBuilders;

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
    private Dictionary<DBLocalMedia, MovieMatch> matchesInSystem;

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
    Dictionary<FileSystemWatcher, DBImportPath> pathLookup;

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

      matchesInSystem = new Dictionary<DBLocalMedia, MovieMatch>();

      fileSystemWatchers = new List<FileSystemWatcher>();
      pathLookup = new Dictionary<FileSystemWatcher, DBImportPath>();
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
                }

                fileSystemWatchers.Clear();
                pathLookup.Clear();
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

    public bool IsScanning() {
      return (mediaScannerThreads.Count != 0);
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
        if (matchesInSystem.ContainsKey(currFile)) {
          Reprocess(matchesInSystem[currFile]);
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

      lock (approveList.SyncRoot)
        approveList.Insert(0, match);

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

        if (matchesInSystem.ContainsKey(movie.LocalMedia[0]))
            RemoveFromMatchLists(matchesInSystem[movie.LocalMedia[0]]);

        lock (priorityPendingMatches.SyncRoot) {
            priorityPendingMatches.Add(newMatch);
        }

        foreach (DBLocalMedia subFile in movie.LocalMedia)
            matchesInSystem.Add(subFile, newMatch);

        if (MovieStatusChanged != null)
            MovieStatusChanged(newMatch, MovieImporterAction.ADDED);
    }

    private void LookForMissingArtworkWorker() {
        try {
            foreach (DBMovieInfo currMovie in DBMovieInfo.GetAll()) {
                if (currMovie.CoverFullPath.Trim().Length == 0) {
                    MovingPicturesCore.DataProviderManager.GetArtwork(currMovie);
                    currMovie.UnloadArtwork();
                    currMovie.Commit();
                }

                if (currMovie.BackdropFullPath.Trim().Length == 0) {
                    new LocalProvider().GetBackdrop(currMovie);
                    MovingPicturesCore.DataProviderManager.GetBackdrop(currMovie);
                    currMovie.Commit();

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
          
          // maintainence tasks
          RemoveOrphanFiles();
          RemoveMissingFiles();
          RemoveOrphanArtwork();

          SetupFileSystemWatchers();

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


          // monitor existing paths for change
          while (true) {
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

    // The DBUserMovieSettings system was revamped so we need to get 
    // rid of any obsolete data and populate new objects as neccisary
    public void VerifyUserMovieSettings() {
        List<DBUserMovieSettings> allUserSettings = DBUserMovieSettings.GetAll();
        foreach (DBUserMovieSettings currSetting in allUserSettings) {
            if (currSetting.AttachedMovies.Count == 0)
                currSetting.Delete();
        }

        List<DBMovieInfo> allMovies = DBMovieInfo.GetAll();
        foreach(DBMovieInfo currMovie in allMovies)
            if (currMovie.UserSettings.Count == 0) {
                logger.Info(currMovie.Title + " was missing UserMovingSettings, adding now.");
                foreach (DBUser currUser in DBUser.GetAll()) {
                    DBUserMovieSettings userSettings = new DBUserMovieSettings();
                    userSettings.User = currUser;
                    userSettings.Commit();
                    currMovie.UserSettings.Add(userSettings);
                    userSettings.CommitNeeded = false;
                }
                
                currMovie.Commit();
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
      DBImportPath importPath = pathLookup[(FileSystemWatcher)source];

      // if this file is already in the system, disable (this happens if it's a removable source)
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

      // if this file's root is unavailable, there is no action
      if (!removedFile.File.Directory.Root.Exists) {
          logger.Info("File " + removedFile.File.Name + " taken offline. (root unavailable)");
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
      logger.Info("Removing missing video files from database.");
      // take care of cover art
      foreach (DBLocalMedia currFile in DBLocalMedia.GetAll()) {
          if (!currFile.ImportPath.Removable && !currFile.File.Exists && currFile.File.Directory.Root.Exists) {         
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
        if (currMovie.BackdropFullPath.Trim().Length > 0 && !new FileInfo(currMovie.BackdropFullPath).Exists)
          currMovie.BackdropFullPath = " ";

        currMovie.Commit();
      }
    }

    // Removes all files not belonging to an import path.
    private void RemoveOrphanFiles() {
      logger.Info("Removing files from database not attached to movies.");
      foreach (DBLocalMedia currFile in DBLocalMedia.GetAll()) {
        if (currFile.AttachedMovies.Count == 0 && !currFile.Ignored)
          currFile.Delete();
      }

        
      logger.Info("Removing files from database belonging to deleted Import Paths.");
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
      bool alwaysGroup = (bool)MovingPicturesCore.SettingsManager["importer_groupfolder"].Value;
        
      foreach (DBLocalMedia currFile in importFileList) {
        //string currFolder = currFile.File.DirectoryName;
        // if we have already loaded this file, move to the next
        if (matchesInSystem.ContainsKey(currFile) || currFile.ID != null) {
          logger.Debug("Skipping " + currFile.File.Name + " because it is already in the system.");
          continue;
        }

        // only grab the video_ts.ifo when extension is ifo
        // to prevent unnecessary stacking
        if (currFile.File.Extension.ToLower() == ".ifo")
          if (currFile.File.Name.ToLower() != "video_ts.ifo")
            continue;
        
        // don't add vob files that are part of a DVD disc/folder
        if (currFile.File.Extension.ToLower() == ".vob")
            if (Utility.isDvdContainer(currFile.File.Directory))
                continue;

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
        MovieMatch newMatch = new MovieMatch();
        newMatch.LocalMedia = currFileSet;
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

            if (Progress != null)
              Progress(100, 0, 0, "Complete!");
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
      double mediaScanPercent; // value 0-50
      double commitApprovedPercent; // value 0-50

      mediaScanPercent = ((double)(matchesInSystem.Count - pendingMatches.Count)) / (matchesInSystem.Count + 0.001);
      mediaScanPercent *= 50.0;

      commitApprovedPercent = ((double)commitedMatches.Count) /
                              ((double)matchesNeedingInput.Count + approvedMatches.Count + commitedMatches.Count + 0.001);

      commitApprovedPercent *= 50;

      percentDone = (int)(mediaScanPercent + commitApprovedPercent);

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
          mediaMatch = (MovieMatch) priorityPendingMatches[0];
          priorityPendingMatches.Remove(mediaMatch);
        }
      }

      // if no reprocessing matches available, get next pending match
      if (mediaMatch == null)
        lock (pendingMatches.SyncRoot) {
          if (pendingMatches.Count == 0)
            return;

          mediaMatch = (MovieMatch) pendingMatches[0];
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
      int threshold = (int)MovingPicturesCore.SettingsManager["importer_autoapprove"].Value;
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

      movie.UnloadArtwork();

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

      movie.Commit();
    }

    private void AssignAndCommit(MovieMatch match, bool update) {
        lock (match) {
            // if we already have a movie object with assigned files, just update
            if (match.ExistingMovieInfo != null && update) {
                DBMovieInfo movie = match.ExistingMovieInfo;

                // pass on the site_id from the selected match
                int scriptID = match.PreferedDataSource.SelectedScript.Provider.ScriptID;
                string siteID = match.Selected.Movie.GetSourceMovieInfo(scriptID).Identifier;
                movie.GetSourceMovieInfo(scriptID).Identifier = siteID;

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

      bool strictYear = (bool)MovingPicturesCore.SettingsManager["importer_strict_year"].Value;
      // TODO: this boolean will probably be removed on improvement of the matching system
      bool imdbBoost = (bool)MovingPicturesCore.SettingsManager["importer_autoimdb"].Value;

      foreach (DBMovieInfo currMovie in movieList) {
        PossibleMatch currMatch = new PossibleMatch();
        currMatch.Movie = currMovie;

        // TODO: code below is the part of the matching system that should change

        MovieSignature currSignature = new MovieSignature(currMovie.Title);
        currSignature.Year = currMovie.Year;
        currSignature.ImdbId = currMovie.ImdbID;

        // If strict year matching is enabled exclude match when years don't match
        if (strictYear)
          if ((currSignature.Year != signature.Year) && (currSignature.Year != 0) && (signature.Year != 0))
            continue; // move to next match in the list               

        int bestMatch = calculateMatchValue(signature, currSignature, imdbBoost);
        foreach (string akaTitle in currMovie.AlternateTitles.ToArray()) {
          currSignature.Title = akaTitle;
          int matchValue = calculateMatchValue(signature, currSignature, imdbBoost);
          if (matchValue < bestMatch)
            bestMatch = matchValue;
        }
        // Set the MatchValue to the best match found.
        currMatch.MatchValue = bestMatch;

        // Add the match to the ranked movie list
        rankedMovieList.Add(currMatch);
      }

      mediaMatch.PossibleMatches = rankedMovieList;
    }

    private static int calculateMatchValue(MovieSignature sig1, MovieSignature sig2, bool imdbBoost) {
      // Clean titles to improve matching 
      string cleanSource = Utility.normalizeTitle(sig1.Title);
      string cleanMatch = Utility.normalizeTitle(sig2.Title);

      // Account for Year when criteria is met
      // this should give the match a higher priority  
      if (sig1.Year > 0 && sig2.Year > 0) {
        cleanMatch += ' ' + sig2.Year.ToString();
        cleanSource += ' ' + sig1.Year.ToString();
      }

      // Account for IMDB when criteria is met
      if (sig1.ImdbId != null && sig2.ImdbId != null) {

          string s1Imdb = sig1.ImdbId.Trim();
          string s2Imdb = sig2.ImdbId.Trim();

          // only proceed if both are not empty
          if (s1Imdb != string.Empty && s2Imdb != string.Empty) {
              if (imdbBoost && s2Imdb == s1Imdb) {
                  // If IMDB Auto-Approval is active
                  // and the we have an ImdbId match,
                  // cheat the current match system into
                  // an auto-match
                  cleanMatch = s2Imdb;
                  cleanSource = s1Imdb;
              }
              else {
                  // add the imdb id tot the complete matching string
                  // this should improve priority
                  cleanMatch += ' ' + s2Imdb;
                  cleanSource += ' ' + s1Imdb;
              }
          }
      }

      // get the Levenshtein distance between the two string and use them for the match value
      
      int dist = AdvancedStringComparer.Levenshtein(cleanMatch, cleanSource);
      logger.Debug("Compare: '{0}', With: '{1}, Result: {2}", cleanSource, cleanMatch, dist);
      return dist;
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

            // logic for DVD folder display
            if (displayname.ToLower() == "video_ts.ifo") {
              if (currFile.File.Directory.Name.ToLower() == "video_ts") {
                displayname = currFile.File.Directory.Parent.Name;
              }
              else {
                displayname = currFile.File.Directory.Name;
              }
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
          _signature = LocalMediaParser.parseMediaMatch(this);
        if (_existingMovieInfo != null) {
            _signature.Title = _existingMovieInfo.Title;
            _signature.Year = _existingMovieInfo.Year;
            _signature.ImdbId = _existingMovieInfo.ImdbID;
            if (_existingMovieInfo.LocalMedia != null && _existingMovieInfo.LocalMedia.Count > 0)
                _signature.DiscId = _existingMovieInfo.LocalMedia[0].DiscId;
        }
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
