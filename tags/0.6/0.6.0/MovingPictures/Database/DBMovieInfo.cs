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
        }

        public override void AfterDelete() {
            if (ID == null) {
                while (AlternateCovers.Count > 0)
                    this.DeleteCurrentCover();
            }
        }

        #region Database Fields

        [DBFieldAttribute]
        public string Title {
            get { return _title; }

            set {
                _title = value;
                populateSortBy();

                commitNeeded = true;
            }
        } private string _title;

        [DBFieldAttribute(FieldName = "alternate_titles")]
        public StringList AlternateTitles {
          get { return _alternateTitles; }

          set {
            _alternateTitles = value;
            commitNeeded = true;
          }
        } private StringList _alternateTitles;

        [DBFieldAttribute]
        public string SortBy {
            get {
                if (_sortBy.Trim().Length == 0)
                    populateSortBy();

                return _sortBy; 
            }

            set {
                _sortBy = value;
                commitNeeded = true;
            }
        } private string _sortBy;


        [DBFieldAttribute]
        public StringList Directors {
            
            get { return _directors; }
            set { 
                _directors = value;
                commitNeeded = true;
            }

        } private StringList _directors;


        [DBFieldAttribute]
        public StringList Writers {
            get { return _writers; }

            set {
                _writers = value;
                commitNeeded = true;
            }
        } private StringList _writers;


        [DBFieldAttribute]
        public StringList Actors {
            get { return _actors; }

            set {
                _actors = value;
                commitNeeded = true;
            }
        } private StringList _actors;


        [DBFieldAttribute]
        public int Year {
            get { return _year; }

            set {
                _year = value;
                commitNeeded = true;
            }
        } private int _year;


        [DBFieldAttribute]
        public StringList Genres {
            get { return _genres; }

            set {
                _genres = value;
                commitNeeded = true;
            }
        } private StringList _genres;


        [DBFieldAttribute]
        public string Certification {
            get { return _certification; }

            set {
                _certification = value;
                commitNeeded = true;
            }
        } private string _certification;
        
        
        [DBFieldAttribute]
        public string Language {
            get { return _language; }

            set {
                _language = value;
                commitNeeded = true;
            }
        } private string _language;


        [DBFieldAttribute]
        public string Tagline {
            get { return _tagline; }

            set {
                _tagline = value;
                commitNeeded = true;
            }
        } private string _tagline;


        [DBFieldAttribute]
        public string Summary {
            get { return _summary; }

            set {
                _summary = value;
                commitNeeded = true;
            }
        } private string _summary;


        [DBFieldAttribute]
        public float Score {
            get { return _score; }

            set {
                _score = value;
                commitNeeded = true;
            }
        } private float _score;

        [DBFieldAttribute(FieldName="user_score")]
        public float UserScore {
            get { return _userScore; }

            set {
                _userScore = value;
                commitNeeded = true;
            }
        } private float _userScore;

        [DBFieldAttribute]
        public int Popularity {
            get { return _popularity; }

            set {
                _popularity = value;
                commitNeeded = true;
            }
        } private int _popularity;


        [DBFieldAttribute]
        public int Runtime {
            get { return _runtime; }

            set {
                _runtime = value;
                commitNeeded = true;
            }
        } private int _runtime;


        [DBFieldAttribute(FieldName="movie_xml_id")]
        public int? MovieXmlID {
            get { return _movieXmlID; }

            set {
                _movieXmlID = value;
                commitNeeded = true;
            }
        } private int? _movieXmlID;


        [DBFieldAttribute(FieldName = "imdb_id")]
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


        [DBRelation(AutoRetrieve = true)]
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

        [DBRelation(AutoRetrieve = true)]
        public RelationList<DBMovieInfo, DBSourceMovieInfo> SourceMovieInfo {
          get {
            if (_sourceIDs == null) {
                _sourceIDs = new RelationList<DBMovieInfo, DBSourceMovieInfo>(this);
            }
            return _sourceIDs;
          }
        } RelationList<DBMovieInfo, DBSourceMovieInfo> _sourceIDs;

        public DBSourceMovieInfo GetSourceMovieInfo(int scriptID) {
            return DBSourceMovieInfo.GetOrCreate(this, scriptID);
        }

        public DBSourceMovieInfo GetSourceMovieInfo(DBSourceInfo source) {
            return DBSourceMovieInfo.GetOrCreate(this, source);
        }

        [DBFieldAttribute(AllowAutoUpdate = false)]
        public StringList AlternateCovers {
            get { return _covers; }

            set {
                _covers = value;
                commitNeeded = true;
            }
        } private StringList _covers;


        public Image Cover {
            get {
                if (_cover == null && File.Exists(CoverFullPath))
                    _cover = Image.FromFile(CoverFullPath);
                return _cover;
            }
        } private Image _cover = null;


        [DBFieldAttribute(AllowAutoUpdate = false)]
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
                UnloadArtwork();
                commitNeeded = true;
            }
        } private String _coverFullPath;


        public Image CoverThumb {
            get {
                if (_coverThumb == null && File.Exists(CoverThumbFullPath))
                    _coverThumb = Image.FromFile(CoverThumbFullPath);

                return _coverThumb;
            }
        } private Image _coverThumb = null;


        [DBFieldAttribute(AllowAutoUpdate = false)]
        public String CoverThumbFullPath {
            get {
                if (_coverThumbFullPath.Trim().Length == 0)
                    GenerateThumbnail();
                return _coverThumbFullPath;
            }

            set {
                _coverThumbFullPath = value;
                UnloadArtwork();
                commitNeeded = true;
            }
        } private String _coverThumbFullPath;

        [DBFieldAttribute(AllowAutoUpdate = false)]
        public String BackdropFullPath {
            get {
                return _backdropFullPath;
            }

            set {
                _backdropFullPath = value;
                commitNeeded = true;
            }
        } private string _backdropFullPath;

        [DBFieldAttribute(AllowAutoUpdate = false, FieldName="details_url")]
        public String DetailsURL {
            get {
                return _detailsUrl;
            }

            set {
                _detailsUrl = value;
                commitNeeded = true;
            }
        } private string _detailsUrl;

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

        // the total runtime of all the files. returns 0 if the data has not yet been
        // set in all of the dblocalmedia objects
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

        // given a part number and the number of seconds into that part,
        // returns the percentage 0.0-1.0 through the whole movie.
        public int GetPercentage(int part, int time) {
            if (part > LocalMedia.Count)
                return 0;

            float tally = time;
            for (int i = 0; i < part-1; i++) {
                if (LocalMedia[i].Duration == 0)
                    return 0;

                tally += LocalMedia[i].Duration;
            }

            return (int)(100 *(tally / (float) ActualRuntime));
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
            
            UnloadArtwork();
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

        // Loads existing cover art and thumbnail into memory. This only 
        // loads the currently selected cover.
        public bool LoadArtwork() {
            return (Cover != null && CoverThumb != null);
        }

        // Unloads coverart information from memory
        public void UnloadArtwork() {
            if (_cover != null)
                _cover.Dispose();

            if (_coverThumb != null)
                _coverThumb.Dispose();
            
            _cover = null;
            _coverThumb = null;
        }

        public bool AddCoverFromFile(string filename) {
            int minWidth = (int)MovingPicturesCore.SettingsManager["min_cover_width"].Value;
            int minHeight = (int)MovingPicturesCore.SettingsManager["min_cover_height"].Value;
            string artFolder = (string)MovingPicturesCore.SettingsManager["cover_art_folder"].Value;
            
            Image newCover = Image.FromFile(filename);

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
                if (!File.Exists(newFileName))
                    newCover.Save(newFileName);
                AlternateCovers.Add(newFileName);
            }

            // if it's already in the folder, just store the filename in the db
            else {
                if (!AlternateCovers.Contains(filename)) {
                    AlternateCovers.Add(filename);
                    commitNeeded = true;
                }
            }

            // create a thumbnail for the cover
            _cover = newCover;
            commitNeeded = true;
            GenerateThumbnail();
            
            return true;
        }

        // Attempts to load cover art for this movie from a given URL. Optionally
        // ignores minimum resolution restrictions
        public ArtworkLoadStatus AddCoverFromURL(string url, bool ignoreRestrictions) {
            int minWidth = (int)MovingPicturesCore.SettingsManager["min_cover_width"].Value;
            int minHeight = (int)MovingPicturesCore.SettingsManager["min_cover_height"].Value;
            string artFolder = (string)MovingPicturesCore.SettingsManager["cover_art_folder"].Value;
            string thumbsFolder = (String)MovingPicturesCore.SettingsManager["cover_thumbs_folder"].Value;
            bool redownloadCovers = (bool)MovingPicturesCore.SettingsManager["redownload_coverart"].Value;

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

                    logger.Info("Cover art for '" + Title + "' [" + ID + "] already exists from " + url + ".");
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
                logger.Info("Cover art for '" + Title + "' [" + ID + "] failed minimum resolution requirements: " + url);
                currImage.Dispose();
                return ArtworkLoadStatus.FAILED_RES_REQUIREMENTS;
            }

            // save the artwork
            logger.Info(filename);
            currImage.Save(filename);
            AlternateCovers.Add(filename);
            _cover = currImage;
            GenerateThumbnail();
            commitNeeded = true;
            return ArtworkLoadStatus.SUCCESS;
        }

        // Attempts to load cover art for this movie from a given URL. Honors 
        // minimum resolution restrictions
        public ArtworkLoadStatus AddCoverFromURL(string url) {
            return AddCoverFromURL(url, false);
        }

        public ArtworkLoadStatus AddBackdropFromURL(string url, bool ignoreRestrictions) {
            int minWidth = (int)MovingPicturesCore.SettingsManager["min_backdrop_width"].Value;
            int minHeight = (int)MovingPicturesCore.SettingsManager["min_backdrop_height"].Value;
            string artFolder = (string)MovingPicturesCore.SettingsManager["backdrop_folder"].Value;
            string thumbsFolder = (String)MovingPicturesCore.SettingsManager["backdrop_thumbs_folder"].Value;
            bool redownloadBackdrops = (bool)MovingPicturesCore.SettingsManager["redownload_backdrops"].Value;

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
                    logger.Info("Backdrop for '" + Title + "' [" + ID + "] already exists from " + url + ".");
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
                logger.Info("Backdrop for '" + Title + "' [" + ID + "] failed minimum resolution requirements: " + url);
                currImage.Dispose();
                return ArtworkLoadStatus.FAILED_RES_REQUIREMENTS;
            }

            // save the backdrop
            currImage.Save(filename);
            _backdropFullPath = filename;
            commitNeeded = true;
            return ArtworkLoadStatus.SUCCESS;
        }

        public ArtworkLoadStatus AddBackdropFromURL(string url) {
            return AddBackdropFromURL(url, false);
        }

        public bool AddBackdropFromFile(string filename) {
            int minWidth = (int)MovingPicturesCore.SettingsManager["min_backdrop_width"].Value;
            int minHeight = (int)MovingPicturesCore.SettingsManager["min_backdrop_height"].Value;
            string artFolder = (string)MovingPicturesCore.SettingsManager["backdrop_folder"].Value;

            Image newBackdrop = Image.FromFile(filename);

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
                newBackdrop.Save(newFileName);
                _backdropFullPath = filename;
                commitNeeded = true;
                return true;
            }

            // if it's already in the folder, just store the filename in the db
            else {
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
            int maxRetries = (int)MovingPicturesCore.SettingsManager["tmdb_max_timeouts"].Value;
            int timeout = (int)MovingPicturesCore.SettingsManager["tmdb_timeout_length"].Value;
            int timeoutIncrement = (int)MovingPicturesCore.SettingsManager["tmdb_timeout_increment"].Value;

            while (rtn == null && tryCount < maxRetries) {
                try {
                    // try to grab the image
                    tryCount++;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Timeout = timeout + (timeoutIncrement * tryCount);
                    request.ReadWriteTimeout = 20000;
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

            string thumbsFolder = (String)MovingPicturesCore.SettingsManager["cover_thumbs_folder"].Value;
            string filename = new FileInfo(CoverFullPath).Name;
            string fullname = thumbsFolder + '\\' + filename;

            if (File.Exists(fullname)) {
                _coverThumbFullPath = fullname;
                return;
            }

            if (Cover == null)
                return;

            int width = 175;
            int height = (int)(Cover.Height * ((float)width / (float)Cover.Width));

            Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
            _coverThumb = Cover.GetThumbnailImage(width, height, myCallback, IntPtr.Zero);
            _coverThumb.Save(fullname);
            _coverThumbFullPath = fullname;
            commitNeeded = true;
        }

        public bool ThumbnailCallback() {
            return false;
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

        public int CompareTo(object obj) {
            if (obj.GetType() == typeof(DBMovieInfo)) {
                return SortBy.CompareTo(((DBMovieInfo)obj).SortBy);
            }
            return 0;
        }

        public override string ToString() {
            return Title;
        }

        private void populateSortBy() {
            // loop through and try to remove a preposition
            string[] prepositions = { "the", "a", "an" };
            foreach (string currWord in prepositions) {
                string word = currWord + " ";
                if (_title.ToLower().IndexOf(word) == 0) {
                    SortBy = _title.Substring(word.Length) + ", " + _title.Substring(0, currWord.Length);
                    return;
                }
            }

            // if no preposition to remove, just use the name
            SortBy = _title;
        }
    }
}
