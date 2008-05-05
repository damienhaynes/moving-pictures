using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using MediaPortal.Plugins.MovingPictures.Database.CustomTypes;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using NLog;
using System.Web;
using System.Net;
using System.Threading;

namespace MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables {
    public enum CoverArtLoadStatus {
        SUCCESS,
        ALREADY_LOADED,
        FAILED,
        FAILED_RES_REQUIREMENTS
    }

    [DBTableAttribute("movie_info")]
    public class DBMovieInfo: MoviesPluginDBTable, IComparable {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        public DBMovieInfo()
            : base() {
        }

        public override void CleanUpForDeletion() {
            if (ID == null) {
                while (AlternateCovers.Count > 0)
                    this.DeleteCurrentCover();
            }
        }

        #region Database Fields

        [DBFieldAttribute]
        public string Name {
            get { return _name; }

            set {
                _name = value;
                populateSortName();

                commitNeeded = true;
            }
        } private string _name;


        [DBFieldAttribute]
        public string SortName {
            get {
                if (_sortName.Trim().Length == 0)
                    populateSortName();

                return _sortName; 
            }

            set {
                _sortName = value;
                commitNeeded = true;
            }
        } private string _sortName;


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


        [DBFieldAttribute(FieldName="trailer_link")]
        public string TrailerLink {
            get { return _trailerLink; }

            set {
                _trailerLink = value;
                commitNeeded = true;
            }
        } private string _trailerLink;


        [DBFieldAttribute]
        public string PosterUrl {
            get { return _posterUrl; }

            set {
                _posterUrl = value;
                commitNeeded = true;
            }
        } private string _posterUrl;


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


        [DBFieldAttribute]
        public DBObjectList<DBLocalMedia> LocalMedia {
            get { return _localMedia; }
            set {
                _localMedia = value;
                commitNeeded = true;
            }
        } private DBObjectList<DBLocalMedia> _localMedia;


        [DBFieldAttribute]
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


        [DBFieldAttribute]
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


        [DBFieldAttribute]
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
        }

        // rotates the selected cover art to the previous available cover
        public void PreviousCover() {
            if (AlternateCovers.Count <= 1)
                return;

            int index = AlternateCovers.IndexOf(CoverFullPath) - 1;
            if (index < 0)
                index = AlternateCovers.Count - 1;

            CoverFullPath = AlternateCovers[index];
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

                logger.Warn("Failed to delete cover art '" + CoverFullPath + "' and/or '" + CoverThumbFullPath + "'");
            }

            AlternateCovers.Remove(CoverFullPath);
            CoverFullPath = "";
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
            int minWidth = (int)MovingPicturesPlugin.SettingsManager["min_cover_width"].Value;
            int minHeight = (int)MovingPicturesPlugin.SettingsManager["min_cover_height"].Value;
            string artFolder = (string)MovingPicturesPlugin.SettingsManager["cover_art_folder"].Value;
            
            Image newCover = Image.FromFile(filename);

            if (newCover == null) {
                logger.Error("Failed loading cover artwork for '" + Name + "' [" + ID + "] from " + filename + ".");
                return false;
            }

            // genrate a filename for the new cover. should be unique based on the file path hash
            string safeName = HttpUtility.UrlEncode(Name.Replace(' ', '.'));
            string newFileName = artFolder + "\\" + safeName + " [" + filename.GetHashCode() + "].jpg";

            // save the artwork
            newCover.Save(newFileName);
            AlternateCovers.Add(newFileName);
            GenerateThumbnail();
            return true;
        }

        // Attempts to load cover art for this movie from a given URL. Optionally
        // ignores minimum resolution restrictions
        public CoverArtLoadStatus AddCoverFromURL(string url, bool ignoreRestrictions) {
            int minWidth = (int)MovingPicturesPlugin.SettingsManager["min_cover_width"].Value;
            int minHeight = (int)MovingPicturesPlugin.SettingsManager["min_cover_height"].Value;
            string artFolder = (string)MovingPicturesPlugin.SettingsManager["cover_art_folder"].Value;
            string thumbsFolder = (String)MovingPicturesPlugin.SettingsManager["cover_thumbs_folder"].Value;
            bool redownloadCovers = (bool)MovingPicturesPlugin.SettingsManager["redownload_coverart"].Value;

            // genrate a filename for a movie. should be unique based on the url hash
            string safeName = HttpUtility.UrlEncode(Name.Replace(' ', '.'));
            string filename = artFolder + "\\" + safeName + " [" + url.GetHashCode() + "].jpg";
            
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

                        logger.Warn("Problem reloading artwork for '" + Name + "' [" + ID + "]. Failed old file deletion.");
                    }
                }
                else {
                    if (!AlternateCovers.Contains(filename))
                        AlternateCovers.Add(filename);

                    logger.Info("Cover art for '" + Name + "' [" + ID + "] already exists from " + url + ".");
                    return CoverArtLoadStatus.ALREADY_LOADED;
                }
            }

            // try to grab the image if failed, exit
            Image currImage = getImageFromUrl(url);
            if (currImage == null) {
                logger.Error("Failed retrieving cover artwork for '" + Name + "' [" + ID + "] from " + url + ".");
                return CoverArtLoadStatus.FAILED;
            }

            // check resolution
            if (!ignoreRestrictions && (currImage.Width < minWidth || currImage.Height < minHeight)) {
                logger.Info("Cover art for '" + Name + "' [" + ID + "] failed minimum resolution requirements: " + url);
                return CoverArtLoadStatus.FAILED_RES_REQUIREMENTS;
            }

            // save the artwork
            currImage.Save(filename);
            AlternateCovers.Add(filename);
            GenerateThumbnail();
            return CoverArtLoadStatus.SUCCESS;
        }

        // Attempts to load cover art for this movie from a given URL. Honors 
        // minimum resolution restrictions
        public CoverArtLoadStatus AddCoverFromURL(string url) {
            return AddCoverFromURL(url, false);
        }

        // given a URL, returns an image stored at that URL. Returns null if not 
        // an image or connection error.
        private Image getImageFromUrl(string url) {
            Image rtn = null;

            // pull in timeout settings
            int tryCount = 0;
            int maxRetries = (int)MovingPicturesPlugin.SettingsManager["xml_max_timeouts"].Value;
            int timeout = (int)MovingPicturesPlugin.SettingsManager["xml_timeout_length"].Value;
            int timeoutIncrement = (int)MovingPicturesPlugin.SettingsManager["xml_timeout_increment"].Value;

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
            if (Cover == null)
                return;

            string thumbsFolder = (String)MovingPicturesPlugin.SettingsManager["cover_thumbs_folder"].Value;
            string filename = new FileInfo(CoverFullPath).Name;
            string fullname = thumbsFolder + '\\' + filename;

            if (File.Exists(fullname)) {
                _coverThumbFullPath = fullname;
                return;
            }

            int width = 175;
            int height = (int)(Cover.Height * ((float)width / (float)Cover.Width));

            Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
            _coverThumb = Cover.GetThumbnailImage(width, height, myCallback, IntPtr.Zero);
            _coverThumb.Save(fullname);
            _coverThumbFullPath = fullname;
        }

        public bool ThumbnailCallback() {
            return false;
        }

        #endregion

        #region Database Management Methods

        public static DBMovieInfo Get(int id) {
            return MovingPicturesPlugin.DatabaseManager.Get<DBMovieInfo>(id);
        }

        public static List<DBMovieInfo> GetAll() {
            return MovingPicturesPlugin.DatabaseManager.Get<DBMovieInfo>(null);
        }

        public static DBField GetField(string fieldName) {
            List<DBField> fieldList = DatabaseManager.GetFieldList(typeof(DBMovieInfo));
            foreach (DBField currField in fieldList) {
                if (currField.Name.ToLower() == fieldName.ToLower())
                    return currField;
            }
            return null;
        }

        #endregion

        public int CompareTo(object obj) {
            if (obj.GetType() == typeof(DBMovieInfo)) {
                return SortName.CompareTo(((DBMovieInfo)obj).SortName);
            }
            return 0;
        }

        private void populateSortName() {
            // loop through and try to remove a preposition
            string[] prepositions = { "the", "a", "an" };
            foreach (string currWord in prepositions) {
                string word = currWord + " ";
                if (_name.ToLower().IndexOf(word) == 0) {
                    SortName = _name.Substring(word.Length);
                    return;
                }
            }

            // if no preposition to remove, just use the name
            SortName = _name;
        }
    }
}
