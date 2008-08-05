using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database;
using System.IO;
using MediaPortal.Plugins.MovingPictures.DataProviders;
using System.Threading;
using System.Collections;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;
using System.Text.RegularExpressions;
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

            matchesInSystem = new Dictionary<DBLocalMedia, MediaMatch>();

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
            if (mediaScannerThreads.Count > 0) {
                foreach(Thread currThread in mediaScannerThreads)
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

        public bool IsScanning() {
            return (mediaScannerThreads.Count != 0);
        }

        public void StartFullScan() {
            fullScanNeeded = true;
        }

        // will add all files to 
        public void Import(List<DBLocalMedia> fileList, bool reloadIfExists) {
            List<DBLocalMedia> fileSet = new List<DBLocalMedia>();
            foreach (DBLocalMedia currFile in new List<DBLocalMedia>(fileList)) {
                // if file is already in importer, reload if requested
                if (matchesInSystem.ContainsKey(currFile)) {
                    if (reloadIfExists)
                        Reprocess(matchesInSystem[currFile]);

                    ScanFiles(fileSet, true);
                    fileSet = new List<DBLocalMedia>();
                    continue;
                }

                // if file is already commited but not in importer, remove all relations
                // and remove the file from the DB, then queue up for scanning
                if (currFile.ID != null && reloadIfExists) {
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
            if (MovieStatusChanged != null)
                MovieStatusChanged(newMatch, MovieImporterAction.ADDED_FROM_JOIN);
        }

        #endregion

        #region File System Scanner

        // Does an initial scan of all DBImportPath objects then monitors for any folder changes
        // or newly added import paths. This thread does not modify the percentDone progress indicator,
        // as generally file system scans are very very fast.
        private void ScanAndMonitorPaths() {
            try {
                while (true) {
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
                    }
                }
            }
            catch (ThreadAbortException) {
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
            foreach (DBLocalMedia currFile in importFileList) {
                // if we have already loaded this file, move to the next
                if (matchesInSystem.ContainsKey(currFile) || currFile.ID != null) 
                    continue;
                

                // if we have no previous files, move on so we can check if the next file
                // is a pair to this one.
                if (currFileSet.Count == 0) {
                    currFileSet.Add(currFile);
                    continue;
                }

                // check if the currFile is a part of the same movie as the previous
                // file(s)
                bool isAdditionalMatch = true;
                string currSearchStr = GetSearchString(currFile.File);
                foreach (DBLocalMedia otherFile in currFileSet) {
                    // if files are not in the same folder we assume they are not a pair
                    if (!currFile.File.DirectoryName.Equals(otherFile.File.DirectoryName)) {
                        isAdditionalMatch = false;
                        break;
                    }

                    // if the file search strings differ by more than one character
                    // assume they are not a pair
                    string otherSearchStr = GetSearchString(otherFile.File);
                    if (AdvancedStringComparer.Levenshtein(currSearchStr, otherSearchStr) > 1) {
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

        #endregion

        #region Media Matcher

        // Monitors the mediaQueue for files imported from the ScanAndMonitorPaths thread. When elements
        // exist, possble matches will be imported and subsequently written to the database.
        private void ScanMedia() {
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
            int processed = commitedMatches.Count;
            int total = commitedMatches.Count + approvedMatches.Count + matchesNeedingInput.Count;
            Progress(percentDone, processed, total, "Retrieving details for: " + currMatch.Selected.Movie.Name);


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
            if (mediaMatch.Selected != null && mediaMatch.Selected.MatchValue <= 3) {
                if (mediaMatch.HighPriority) priorityApprovedMatches.Add(mediaMatch);
                else approvedMatches.Add(mediaMatch);
                
                if (MovieStatusChanged != null)
                    MovieStatusChanged(mediaMatch, MovieImporterAction.APPROVED);
            } else {
                matchesNeedingInput.Add(mediaMatch);
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

            string[] searchArray = mediaMatch.SearchString.Split('|');
            string searchStr = searchArray[0];
            int searchYear = (searchArray.Length > 1) ? int.Parse(searchArray[1]) : 0;      

            // notify any listeners we are checking for matches
            if (MovieStatusChanged != null)
                MovieStatusChanged(mediaMatch, MovieImporterAction.GETTING_MATCHES);

            // grab a list of movies from our dataProvider and rank each returned movie on 
            // how close a match it is
            movieList = MovingPicturesCore.MovieProvider.Get(searchStr);

            bool strictYear = (bool)MovingPicturesCore.SettingsManager["importer_strict_year"].Value;

            foreach (DBMovieInfo currMovie in movieList) {
                PossibleMatch currMatch = new PossibleMatch();
                currMatch.Movie = currMovie;
                
                // if we have 'year' data to compare add it to the string compare
                // this should give the match a higher priority
                
                string sMovie = currMovie.Name.ToLower().Trim();
                string sSearch = searchStr.ToLower().Trim();
                
                // if both have a year then add the year to the comparison variables
                if (searchYear > 0 && currMovie.Year > 0) {
                    sMovie +=  ' ' + currMovie.Year.ToString();
                    sSearch += ' ' + searchYear.ToString();
                }

                currMatch.MatchValue = AdvancedStringComparer.Levenshtein(sMovie,sSearch);

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

        #region Search String Processing

        // Cleans up a filename for movie name matching. Removes extension, converts '.' to  ' ', etc.
        public static string GetSearchString(FileInfo file) {
            string str;

            // get rid of the file extension
            int extIndex = file.Name.IndexOf(file.Extension);
            int extLength = file.Extension.Length;
            str = file.Name.Remove(extIndex);

            return getSearchString(str);
        }

        // Cleans up a directory name for movie name matching. Converts '.' to ' ', etc.
        public static string getSearchString(DirectoryInfo dir) {
            return getSearchString(dir.Name);
        }

        // cleans a string up for movie name matching.
        private static string getSearchString(string inputStr) {
            string rtn = inputStr;
            
            // @todo do not limit to search string only - we should probably make 
            // this return a key/value pair with 'title' being the default key
            // we could then include 'year', 'edition' etc as extra keys
            // if we have a data provider that accepts extra parameters it could use
            // the extra keys for better search.
            
            // if there are no spaces, but a period, assume the period is replacement for spaces.
            // lets clean that up.
            if (!rtn.Contains(" "))
                rtn = rtn.Replace('.', ' ');
            
            // a lot of keywords that could poison the result so let's clean them according to our given exp.
            // regexParser = new Regex(@"((720p|1080p|DVDRip|DTS|AC3|Bluray|HDDVD|XviD|DiVX|x264)[-]?.*?$)", RegexOptions.IgnoreCase);
            Regex regexParser = new Regex(MovingPicturesCore.SettingsManager["importer_filter"].Value.ToString(), RegexOptions.IgnoreCase);
            rtn = regexParser.Replace(rtn, "");

            // if there is a four digit number that looks like a year, parse it out
            // Regex regexParser = new Regex(@"(^.*?)[\[\(]?([0-9]{4})[\]\)]?(.+)");
            regexParser = new Regex(@"(^.+)[\[\(]?([0-9]{4})[\]\)]?(.+)");
            Match match = regexParser.Match(rtn);
            if (match.Success)
            {
                int year = int.Parse(match.Groups[2].Value);
                if (year > 1900 && year < DateTime.Now.Year + 2)
                    // Seperate year from the title and pass it along (using the pipe as marker)
                    rtn = match.Groups[1].Value + "|" + match.Groups[2].Value;
            }

            return rtn;
        }

        #endregion
    }

    public class MediaMatch {
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
                if (_searchString.Equals(string.Empty)) {
                    bool preferFolder = (bool)MovingPicturesCore.SettingsManager["importer_prefer_foldername"].Value;

                    if (LocalMedia == null || LocalMedia.Count == 0)
                        _searchString = "";
                    else if (LocalMedia.Count == 1 && !preferFolder)
                        _searchString = MovieImporter.GetSearchString(LocalMedia[0].File);
                    else
                        _searchString = MovieImporter.getSearchString(LocalMedia[0].File.Directory);
                }

                return _searchString;
            }
            

            set {
                _searchString = value;
            }
        } private string _searchString = string.Empty;
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
            get { return ToString() + " (" + this.movie.Year.ToString() + ")"; }
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
                return movie.Name;
            return "";
        }
    }

}
