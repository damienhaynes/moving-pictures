using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using MediaPortal.Plugins.MovingPictures.Database.CustomTypes;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables {
    [DBTableAttribute("movie_info")]
    public class DBMovieInfo: MoviesPluginDBTable {
        public DBMovieInfo()
            : base() {
        }

        #region Database Fields

        [DBFieldAttribute]
        public string Name {
            get { return _name; }

            set {
                _name = value;
                commitNeeded = true;
            }
        } private string _name;


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
                _cover = null;
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
                _coverThumb = null;
                commitNeeded = true;
            }
        } private String _coverThumbFullPath;

        #endregion'

        #region Coverart Management Methods
        public bool LoadArtwork() {
            return (Cover != null && CoverThumb != null);
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

            int width = 150;
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
    }
}
