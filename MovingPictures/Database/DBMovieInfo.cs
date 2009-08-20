using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using NLog;
using System.Web;
using System.Net;
using System.Threading;
using System.Collections;
using Cornerstone.Database;
using Cornerstone.Database.CustomTypes;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using System.Text.RegularExpressions;
using Cornerstone.Tools.Translate;

namespace MediaPortal.Plugins.MovingPictures.Database {
    public enum ArtworkLoadStatus {
        SUCCESS,
        ALREADY_LOADED,
        FAILED,
        FAILED_RES_REQUIREMENTS
    }

    [DBTableAttribute("movie_info")]
    public class DBMovieInfo: MovingPicturesDBTable, IComparable, IAttributeOwner {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        public DBMovieInfo()
            : base() {
            this.LocalMedia.Changed += new ChangedEventHandler(LocalMedia_Changed);
        }

        void LocalMedia_Changed(object sender, EventArgs e) {
            this.LocalMedia.Sort(new DBLocalMediaComparer());
        }

        public override void AfterDelete() {
            if (ID == null) {
                while (AlternateCovers.Count > 0)
                    this.DeleteCurrentCover();
                foreach (var wh in this.WatchedHistory) {
                    wh.Delete();
                }
            }
        }

        #region Database Fields

        [DBField(AllowDynamicFiltering=false)]
        public string Title {
            get { return _title; }
            set { 
                _title = value;
                PopulateSortBy();
                commitNeeded = true;
            }
        } private string _title;

        [DBField(FieldName = "alternate_titles", AllowDynamicFiltering = false)]
        public StringList AlternateTitles {
          get { return _alternateTitles; }

          set {
            _alternateTitles = value;
            commitNeeded = true;
          }
        } private StringList _alternateTitles;

        [DBField(Filterable=false)]
        public string SortBy {
            get {
                if (_sortBy.Trim().Length == 0)
                    PopulateSortBy();

                return _sortBy; 
            }

            set {
                _sortBy = value;
                commitNeeded = true;
            }
        } private string _sortBy;
        

        [DBField]
        public StringList Directors {
            
            get { return _directors; }
            set { 
                _directors = value;
                commitNeeded = true;
            }

        } private StringList _directors;


        [DBField]
        public StringList Writers {
            get { return _writers; }

            set {
                _writers = value;
                commitNeeded = true;
            }
        } private StringList _writers;


        [DBField]
        public StringList Actors {
            get { return _actors; }

            set {
                _actors = value;
                commitNeeded = true;
            }
        } private StringList _actors;


        [DBField]
        public int Year {
            get { return _year; }

            set {
                _year = value;
                commitNeeded = true;
            }
        } private int _year;


        [DBField]
        public StringList Genres {
            get { return _genres; }

            set {
                _genres = value;
                commitNeeded = true;
            }
        } private StringList _genres;


        [DBField]
        public string Certification {
            get { return _certification; }

            set {
                _certification = value;
                commitNeeded = true;
            }
        } private string _certification;
        
        
        [DBField(AllowManualFilterInput=false)]
        public string Language {
            get { return _language; }

            set {
                _language = value;
                commitNeeded = true;
            }
        } private string _language;


        [DBField(AllowDynamicFiltering = false)]
        public string Tagline {
            get { return _tagline; }

            set {
                _tagline = value;
                commitNeeded = true;
            }
        } private string _tagline;


        [DBField(AllowDynamicFiltering = false)]
        public string Summary {
            get { return _summary; }

            set {
                _summary = value;
                commitNeeded = true;
            }
        } private string _summary;


        [DBField(AllowDynamicFiltering = false)]
        public float Score {
            get { return _score; }

            set {
                _score = value;
                while (_score > 10)
                    _score /= 10;

                _score = (float) Math.Round(_score, 1);

                commitNeeded = true;
            }
        } private float _score;

        [DBField(Filterable=false)]
        public int Popularity {
            get { return _popularity; }

            set {
                _popularity = value;
                commitNeeded = true;
            }
        } private int _popularity;

        [DBField(FieldName="date_added")]
        public DateTime DateAdded {
            get { return _dateAdded; }

            set {
                _dateAdded = value;
                commitNeeded = true;
            }
        } private DateTime _dateAdded;


        [DBField(AllowDynamicFiltering = false)]
        public int Runtime {
            get { return _runtime; }

            set {
                _runtime = value;
                commitNeeded = true;
            }
        } private int _runtime;


        [DBField(FieldName = "imdb_id", Filterable=false)]
        public string ImdbID {
            get { return _imdbID; }

            set {
                _imdbID = value;
                commitNeeded = true;
            }
        } private string _imdbID;


        [DBRelation(AutoRetrieve=true)]
        public RelationList<DBMovieInfo, DBLocalMedia> LocalMedia {
            get {
                if (_localMedia == null) {
                    _localMedia = new RelationList<DBMovieInfo, DBLocalMedia>(this);
                }
                return _localMedia; 
            }
        } RelationList<DBMovieInfo, DBLocalMedia> _localMedia;


        [DBRelation(AutoRetrieve = true, Filterable=false)]
        public RelationList<DatabaseTable, DBAttribute> Attributes {
            get {
                if (_attributes == null) {
                    _attributes = new RelationList<DatabaseTable, DBAttribute>(this);
                }
                return _attributes;
            }
        } RelationList<DatabaseTable, DBAttribute> _attributes;

        [DBRelation(AutoRetrieve = true)]
        public RelationList<DBMovieInfo, DBUserMovieSettings> UserSettings {
          get {
            if (_userSettings == null) {
              _userSettings = new RelationList<DBMovieInfo, DBUserMovieSettings>(this);
            }
            return _userSettings;
          }
        } RelationList<DBMovieInfo, DBUserMovieSettings> _userSettings;

        [DBRelation(AutoRetrieve = true, Filterable = false)]
        public RelationList<DBMovieInfo, DBSourceMovieInfo> SourceMovieInfo {
          get {
            if (_sourceIDs == null) {
                _sourceIDs = new RelationList<DBMovieInfo, DBSourceMovieInfo>(this);
            }
            return _sourceIDs;
          }
        } RelationList<DBMovieInfo, DBSourceMovieInfo> _sourceIDs;

        [DBRelation(AutoRetrieve = true, Filterable = false)]
        public RelationList<DBMovieInfo, DBWatchedHistory> WatchedHistory {
            get {
                if (_watchedHistory == null) {
                    _watchedHistory = new RelationList<DBMovieInfo, DBWatchedHistory>(this);
                }
                return _watchedHistory;
            }
        } RelationList<DBMovieInfo, DBWatchedHistory> _watchedHistory;

        public DBSourceMovieInfo GetSourceMovieInfo(int scriptID) {
            return DBSourceMovieInfo.GetOrCreate(this, scriptID);
        }

        public DBSourceMovieInfo GetSourceMovieInfo(DBSourceInfo source) {
            return DBSourceMovieInfo.GetOrCreate(this, source);
        }

        [DBField(AllowAutoUpdate = false, Filterable=false)]
        public StringList AlternateCovers {
            get { return _covers; }

            set {
                _covers = value;
                commitNeeded = true;
            }
        } private StringList _covers;


        [DBField(AllowAutoUpdate = false, Filterable=false)]
        public String CoverFullPath
        {
            get {
                if (_coverFullPath.Trim().Length == 0 && AlternateCovers.Count > 0)
                    _coverFullPath = AlternateCovers[0];
                return _coverFullPath; 
            }

            set {
                _coverFullPath = value;
                _coverThumbFullPath = "";
                commitNeeded = true;
            }
        } private String _coverFullPath;


        [DBField(AllowAutoUpdate = false, Filterable=false)]
        public String CoverThumbFullPath {
            get {
                if (_coverThumbFullPath.Trim().Length == 0)
                    GenerateThumbnail();
                return _coverThumbFullPath;
            }

            set {
                _coverThumbFullPath = value;
                commitNeeded = true;
            }
        } private String _coverThumbFullPath;

        [DBField(AllowAutoUpdate = false, Filterable=false)]
        public String BackdropFullPath {
            get {
                return _backdropFullPath;
            }

            set {
                _backdropFullPath = value;
                commitNeeded = true;
            }
        } private string _backdropFullPath;

        [DBField(AllowAutoUpdate = false, FieldName="details_url", Filterable=false)]
        public String DetailsURL {
            get {
                return _detailsUrl;
            }

            set {
                _detailsUrl = value;
                commitNeeded = true;
            }
        } private string _detailsUrl;

        public DBUserMovieSettings ActiveUserSettings {
            get {
                return UserSettings[0];
            }
        }
        #endregion

        #region General Management Methods
        
        // deletes this movie from the database and sets all related DBLocalMedia to ignored
        public void DeleteAndIgnore() {
            foreach (DBLocalMedia currFile in LocalMedia) {
                currFile.Ignored = true;
                currFile.Commit();
            }

            Delete();
        }

        /// <summary>
        /// The total runtime of the the localmedia files in milliseconds. 
        /// Returns 0 if the data has not yet been set in all of the dblocalmedia objects
        /// </summary>
        public int ActualRuntime {
            get {
                if (actualRuntime == 0) {
                    foreach (DBLocalMedia currFile in LocalMedia) {
                        if (currFile.Duration == 0) {
                            actualRuntime = 0;
                            return 0;
                        }

                        actualRuntime += currFile.Duration;
                    }

                }
                return actualRuntime;
            }
        } 
        private int actualRuntime = 0;

        /// <summary>
        /// Returns percentage watched
        /// </summary>
        /// <param name="part">part number</param>
        /// <param name="time">time in seconds</param>
        /// <returns>the percentage 1 - 100 through the whole movie.</returns>
        public int GetPercentage(int part, int time) {
            if (part > LocalMedia.Count)
                return 0;
            
            // convert to milliseconds
            float tally = time * 1000;

            for (int i = 0; i < part-1; i++) {
                if (LocalMedia[i].Duration == 0)
                    return 0;

                tally += LocalMedia[i].Duration;
            }

            return (int) (100 * (tally / (float) ActualRuntime));
        }


        /// <summary>
        /// Deletes all of the video files associated with this movie from disk
        /// If the movie is stored in a dedicated folder, this will delete
        /// the folder.
        /// </summary>
        /// <returns>true for success, false for failure</returns>
        public bool DeleteFiles() {
            try {
                FileInfo fInfo = this.LocalMedia[0].File;
                bool isFolderDedicated = Utility.isFolderDedicated(fInfo.Directory, this.LocalMedia.Count);

                if (isFolderDedicated) {
                    Utility.GetMovieBaseDirectory(fInfo.Directory).Delete(true);
                }
                else {
                    foreach (DBLocalMedia item in this.LocalMedia) {
                        File.Delete(item.FullPath);
                    }
                }

                this.Delete();
                return true;
            }
            catch (Exception ex) {
                logger.LogException(LogLevel.Error, "Error when deleting file", ex);
                return false;
            }
        }

        #endregion

        #region Coverart Management Methods

        // rotates the selected cover art to the next available cover
        public void NextCover() {
            if (AlternateCovers.Count <= 1)
                return;

            int index = AlternateCovers.IndexOf(CoverFullPath) + 1;
            if (index >= AlternateCovers.Count)
                index = 0;

            CoverFullPath = AlternateCovers[index];
            commitNeeded = true;
        }

        // rotates the selected cover art to the previous available cover
        public void PreviousCover() {
            if (AlternateCovers.Count <= 1)
                return;

            int index = AlternateCovers.IndexOf(CoverFullPath) - 1;
            if (index < 0)
                index = AlternateCovers.Count - 1;

            CoverFullPath = AlternateCovers[index];
            commitNeeded = true;
        }

        // removes the current cover from the selection list and deletes it and it's thumbnail 
        // from disk
        public void DeleteCurrentCover() {
            if (AlternateCovers.Count == 0)
                return;
            
            try {
                FileInfo coverFile = new FileInfo(CoverFullPath);
                if (coverFile.Exists)
                    coverFile.Delete();

                FileInfo thumbFile = new FileInfo(CoverThumbFullPath);
                if (thumbFile.Exists)
                    thumbFile.Delete();
            }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;
            }

            AlternateCovers.Remove(CoverFullPath);
            CoverFullPath = "";
            commitNeeded = true;
        }

        public bool AddCoverFromFile(string filename) {
            int minWidth = MovingPicturesCore.Settings.MinimumCoverWidth;
            int minHeight = MovingPicturesCore.Settings.MinimumCoverHeight;
            string artFolder = MovingPicturesCore.Settings.CoverArtFolder;

            Image newCover = null;
            try {
                newCover = Image.FromFile(filename);
            }
            catch (OutOfMemoryException e) {
                logger.DebugException("Invalid image or image format not supported.", e);
            }
            catch (FileNotFoundException e) {
                logger.DebugException("File not found.", e);
            }

            if (newCover == null) {
                logger.Error("Failed loading cover artwork for '" + Title + "' [" + ID + "] from " + filename + ".");
                return false;
            }

            
            // check if the image file is already in the cover folder
            FileInfo newFile = new FileInfo(filename);
            bool alreadyInFolder = newFile.Directory.FullName.Equals(new DirectoryInfo(artFolder).FullName);

            // if the file isnt in the cover folder, generate a name and save it there
            if (!alreadyInFolder) {
                string safeName = Utility.CreateFilename(Title.Replace(' ', '.'));
                string newFileName = artFolder + "\\{" + safeName + "} [" + filename.GetHashCode() + "].jpg";
                if (!File.Exists(newFileName)) {
                    bool saved = false;
                    try {
                        newCover.Save(newFileName, ImageFormat.Png);
                        AlternateCovers.Add(newFileName);
                        saved = true;
                    }
                    catch (ArgumentNullException) {
                        logger.Debug("Error while trying to save cover: filename is missing.");
                    }
                    catch (System.Runtime.InteropServices.ExternalException e) {
                        logger.Error("Error while trying to save cover. ", e);
                    }

                    if (!saved) {
                        newCover.Dispose();
                        return false;
                    }
                }
                else return false;
            }

            // if it's already in the folder, just store the filename in the db
            else {
                if (!AlternateCovers.Contains(filename)) {
                    AlternateCovers.Add(filename);
                    commitNeeded = true;
                }
                else
                    return false;
            }

            // create a thumbnail for the cover
            logger.Debug("Added cover art for '" + Title + "' [" + ID + "] from " + filename + ".");
            newCover.Dispose();
            commitNeeded = true;
            GenerateThumbnail();
            
            return true;
        }

        // Attempts to load cover art for this movie from a given URL. Optionally
        // ignores minimum resolution restrictions
        public ArtworkLoadStatus AddCoverFromURL(string url, bool ignoreRestrictions) {
            int minWidth = MovingPicturesCore.Settings.MinimumCoverWidth;
            int minHeight = MovingPicturesCore.Settings.MinimumCoverHeight;
            string artFolder = MovingPicturesCore.Settings.CoverArtFolder;
            string thumbsFolder = MovingPicturesCore.Settings.CoverArtThumbsFolder;
            bool redownloadCovers = MovingPicturesCore.Settings.RedownloadCoverArtwork;

            // genrate a filename for a movie. should be unique based on the url hash
            string safeName = Utility.CreateFilename(Title.Replace(' ', '.'));
            string filename = artFolder + "\\{" + safeName + "} [" + url.GetHashCode() + "].jpg";
            
            // if we already have a file for this movie from this URL, move on
            if (File.Exists(filename)) {
                if (redownloadCovers) {
                    FileInfo file = new FileInfo(filename);
                    string thumbFileName = thumbsFolder + "\\" + file.Name;
                    FileInfo thumbFile = new FileInfo(thumbFileName);
                    try {
                        file.Delete();
                        thumbFile.Delete();
                    }
                    catch (Exception e) {
                        if (e.GetType() == typeof(ThreadAbortException))
                            throw e;
                    }
                }
                else {
                    if (!AlternateCovers.Contains(filename)) {
                        AlternateCovers.Add(filename);
                        commitNeeded = true;
                    }

                    logger.Debug("Cover art for '" + Title + "' [" + ID + "] already exists from " + url + ".");
                    GenerateThumbnail();
                    return ArtworkLoadStatus.ALREADY_LOADED;
                }
            }

            // try to grab the image if failed, exit
            Image currImage = getImageFromUrl(url);
            if (currImage == null) {
                logger.Error("Failed retrieving cover artwork for '" + Title + "' [" + ID + "] from " + url + ".");
                return ArtworkLoadStatus.FAILED;
            }

            // check resolution
            if (!ignoreRestrictions && (currImage.Width < minWidth || currImage.Height < minHeight)) {
                logger.Debug("Cover art for '" + Title + "' [" + ID + "] failed minimum resolution requirements: " + url);
                currImage.Dispose();
                return ArtworkLoadStatus.FAILED_RES_REQUIREMENTS;
            }

            // save the artwork
            bool saved = false;
            try {
                currImage.Save(filename, ImageFormat.Png);
                AlternateCovers.Add(filename);
                GenerateThumbnail();
                commitNeeded = true;
                saved = true;
                logger.Debug("Added cover art for '" + Title + "' [" + ID + "] from " + url + ".");
            }
            catch (ArgumentNullException) {
                logger.Debug("Error while trying to save cover: filename is missing.");
            }
            catch (System.Runtime.InteropServices.ExternalException e) {
                logger.Error("Error while trying to save cover. ", e);
            }
            finally {
                currImage.Dispose();
            }

            if (saved)
                return ArtworkLoadStatus.SUCCESS;
            else
                return ArtworkLoadStatus.FAILED;
        }

        // Attempts to load cover art for this movie from a given URL. Honors 
        // minimum resolution restrictions
        public ArtworkLoadStatus AddCoverFromURL(string url) {
            return AddCoverFromURL(url, false);
        }

        public ArtworkLoadStatus AddBackdropFromURL(string url, bool ignoreRestrictions) {
            int minWidth = MovingPicturesCore.Settings.MinimumBackdropWidth;
            int minHeight = MovingPicturesCore.Settings.MinimumBackdropHeight;
            string artFolder = MovingPicturesCore.Settings.BackdropFolder;
            string thumbsFolder = MovingPicturesCore.Settings.BackdropThumbsFolder;
            bool redownloadBackdrops = MovingPicturesCore.Settings.RedownloadBackdrops;
            
            // generate a filename for a movie. should be unique based on the url hash
            string safeName = Utility.CreateFilename(Title.Replace(' ', '.'));
            string filename = artFolder + "\\{" + safeName + "} [" + url.GetHashCode() + "].jpg";

            // if we already have a file for this movie from this URL, move on
            if (File.Exists(filename)) {
                if (redownloadBackdrops) {
                    FileInfo file = new FileInfo(filename);
                    string thumbFileName = thumbsFolder + "\\" + file.Name;
                    FileInfo thumbFile = new FileInfo(thumbFileName);
                    try {
                        file.Delete();
                        thumbFile.Delete();
                    }
                    catch (Exception e) {
                        if (e.GetType() == typeof(ThreadAbortException))
                            throw e;
                    }
                }
                else {
                    // TODO: Add filename to alrternate backdrops string list here
                    if (_backdropFullPath.Trim().Length == 0) {
                        BackdropFullPath = filename;
                        commitNeeded = true;
                    }
                    logger.Debug("Backdrop for '" + Title + "' [" + ID + "] already exists from " + url + ".");
                    return ArtworkLoadStatus.ALREADY_LOADED;
                }
            }

            // try to grab the image if failed, exit
            Image currImage = getImageFromUrl(url);
            if (currImage == null) {
                // needs to be uncommented when backdrop provider is fleshed out and doesnt ask
                // for 404 urls to be loaded
                //logger.Error("Failed retrieving backdrop for '" + Title + "' [" + ID + "] from " + url + ".");
                return ArtworkLoadStatus.FAILED;
            }

            // check resolution
            if (!ignoreRestrictions && (currImage.Width < minWidth || currImage.Height < minHeight)) {
                logger.Debug("Backdrop for '" + Title + "' [" + ID + "] failed minimum resolution requirements: " + url);
                currImage.Dispose();
                return ArtworkLoadStatus.FAILED_RES_REQUIREMENTS;
            }

            // save the backdrop
            bool saved = false;
            try {
                currImage.Save(filename, ImageFormat.Jpeg);
                _backdropFullPath = filename;
                commitNeeded = true;
                saved = true;
            }
            catch (ArgumentNullException) {
                logger.Debug("Error while trying to save backdrop: filename is missing.");
            }
            catch (System.Runtime.InteropServices.ExternalException e) {
                logger.Error("Error while trying to save backdrop. ", e);
            }
            finally {
                currImage.Dispose();
            }
            if (saved)
                return ArtworkLoadStatus.SUCCESS;
            else
                return ArtworkLoadStatus.FAILED;
            
        }

        public ArtworkLoadStatus AddBackdropFromURL(string url) {
            return AddBackdropFromURL(url, false);
        }

        public bool AddBackdropFromFile(string filename) {
            int minWidth = MovingPicturesCore.Settings.MinimumBackdropWidth;
            int minHeight = MovingPicturesCore.Settings.MinimumBackdropHeight;
            string artFolder = MovingPicturesCore.Settings.BackdropFolder;

            Image newBackdrop = null;
            try {
                newBackdrop = Image.FromFile(filename);
            }
            catch (OutOfMemoryException e) {
                logger.DebugException("Invalid image or image format not supported.", e);
            }
            catch (FileNotFoundException e) {
                logger.DebugException("File not found.", e);
            }
            
            if (newBackdrop == null) {
                logger.Error("Failed loading backdrop for '" + Title + "' [" + ID + "] from " + filename + ".");
                return false;
            }


            // check if the image file is already in the backdrop folder
            FileInfo newFile = new FileInfo(filename);
            bool alreadyInFolder = newFile.Directory.FullName.Equals(new DirectoryInfo(artFolder).FullName);

            // if the file isnt in the backdrop folder, generate a name and save it there
            if (!alreadyInFolder) {
                string safeName = Utility.CreateFilename(Title.Replace(' ', '.'));
                string newFileName = artFolder + "\\{" + safeName + "} [" + filename.GetHashCode() + "].jpg";
                
                // save the backdrop
                bool saved = false;
                try {
                    newBackdrop.Save(newFileName, ImageFormat.Jpeg);
                    _backdropFullPath = filename;
                    commitNeeded = true;
                    saved = true;
                }
                catch (ArgumentNullException) {
                    logger.Debug("Error while trying to save backdrop: filename is NULL.");
                }
                catch (System.Runtime.InteropServices.ExternalException e) {
                    logger.Error("Error while trying to save backdrop. ", e);
                }
                finally {
                    newBackdrop.Dispose();
                }
                return saved;
            }

            // if it's already in the folder, just store the filename in the db
            else {
                newBackdrop.Dispose();
                _backdropFullPath = filename;
                commitNeeded = true;
                return true;

            }
        }

        // given a URL, returns an image stored at that URL. Returns null if not 
        // an image or connection error.
        private Image getImageFromUrl(string url) {
            Image rtn = null;

            // pull in timeout settings
            int tryCount = 0;
            int maxRetries = MovingPicturesCore.Settings.MaxTimeouts;
            int timeout = MovingPicturesCore.Settings.TimeoutLength;
            int timeoutIncrement = MovingPicturesCore.Settings.TimeoutIncrement;

            while (rtn == null && tryCount < maxRetries) {
                try {
                    // try to grab the image
                    tryCount++;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Timeout = timeout + (timeoutIncrement * tryCount);
                    request.ReadWriteTimeout = 20000;
                    request.UserAgent = "Mozilla/5.0 (Windows; U; MSIE 7.0; Windows NT 6.0; en-US)";
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    // parse the stream into an image file
                    rtn = Image.FromStream(response.GetResponseStream());
                }
                catch (WebException e) {
                    // file doesnt exist
                    if (e.Message.Contains("404")) {
                        // needs to be uncommented when backdrop provider is fleshed out and doesnt ask
                        // for 404 urls to be loaded
                        //logger.Warn("Failed retrieving artwork from " + url + ". File does not exist. (404)");
                        return null;
                    }
                    
                    // if we timed out past our try limit
                    if (tryCount == maxRetries) {
                        logger.ErrorException("Failed to retrieve artwork from " + url + ". Reached retry limit of " + maxRetries, e);
                        return null;
                    }
                }
                catch (UriFormatException) {
                    logger.Error("Bad URL format, failed loading image: " + url);
                    return null;
                }
                catch (ArgumentException) {
                    logger.Error("URL does not point to an image: " + url);
                    return null;
                }
            }

            if (rtn == null) {
                logger.Error("Failed loading image from url: " + url);
                return null;
            }

            return rtn;
        }

        public void GenerateThumbnail() {
            if (CoverFullPath.Trim().Length == 0)
                return;

            string thumbsFolder = MovingPicturesCore.Settings.CoverArtThumbsFolder;
            string filename = new FileInfo(CoverFullPath).Name;
            string fullname = thumbsFolder + '\\' + filename;

            if (File.Exists(fullname)) {
                _coverThumbFullPath = fullname;
                return;
            }

            Image cover = Image.FromFile(CoverFullPath);

            int width = 175;
            int height = (int)(cover.Height * ((float)width / (float)cover.Width));

            Image coverThumb = cover.GetThumbnailImage(width, height, null, IntPtr.Zero);
            try {
                coverThumb.Save(fullname, ImageFormat.Png);
                _coverThumbFullPath = fullname;
                commitNeeded = true;
            }
            catch (ArgumentNullException) {
                logger.Debug("Error while trying to save thumbnail: filename is NULL.");
            }
            catch (System.Runtime.InteropServices.ExternalException e) {
                logger.Error("Error while trying to save thumbnail. ", e);
            }
            finally {
                cover.Dispose();
                coverThumb.Dispose();
            }
        }

        #endregion

        #region Database Management Methods

        public static DBMovieInfo Get(int id) {
            return MovingPicturesCore.DatabaseManager.Get<DBMovieInfo>(id);
        }

        public static List<DBMovieInfo> GetAll() {
            return MovingPicturesCore.DatabaseManager.Get<DBMovieInfo>(null);
        }

        // this should be changed to reflectively commit all sub objects and relation lists
        // and moved down to the DatabaseTable class.
        public override void Commit() {
            if (this.ID == null) {
                base.Commit();
                commitNeeded = true;
            }

            foreach (DBSourceMovieInfo currInfo in SourceMovieInfo) {
                currInfo.Commit();
            }
        
            base.Commit();
        }

        #endregion

        public override int CompareTo(object obj) {
            if (obj.GetType() == typeof(DBMovieInfo)) {
                return SortBy.CompareTo(((DBMovieInfo)obj).SortBy);
            }
            return 0;
        }

        public override string ToString() {
            return Title;
        }

        public void PopulateSortBy() {
            // remove all non-word characters and replace them with spaces
            SortBy = Regex.Replace(_title, @"[^\w\s]", "", RegexOptions.IgnoreCase).ToLower().Trim();

            // loop through and try to remove a preposition
            if (MovingPicturesCore.Settings.RemoveTitleArticles) {
                string[] prepositions = MovingPicturesCore.Settings.ArticlesForRemoval.Split('|');
                foreach (string currWord in prepositions) {
                    string word = currWord + " ";
                    if (_sortBy.ToLower().IndexOf(word) == 0) {
                        SortBy = _sortBy.Substring(word.Length) + " " + _sortBy.Substring(0, currWord.Length);
                        return;
                    }
                }
            }
        }

        public void Translate() {
            Translate(MovingPicturesCore.Settings.TranslationLanguage);
        }

        public void Translate(TranslatorLanguage language) {
            Translator tr = new Translator();
            tr.ToLanguage = language;

            Summary = tr.Translate(Summary);
            Tagline = tr.Translate(Tagline);

            string tmp = tr.Translate(Genres.ToString());
            Genres.Clear();
            foreach (string currGenre in tmp.Split('|'))
                if (currGenre.Trim().Length > 0)
                    Genres.Add(currGenre.Trim());
        }
    }
}
