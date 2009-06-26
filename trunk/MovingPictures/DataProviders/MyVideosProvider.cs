using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Plugins.MovingPictures.SignatureBuilders;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Configuration;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using SQLite.NET;
using System.Threading;


namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    public class MyVideosProvider: IMovieProvider {
        #region IMovieProvider Members

        public string Name {
            get { return "MyVideos (Local)"; }
        }

        public string Version {
            get { return "Internal"; }
        }

        public string Author {
            get { return "Moving Pictures Team"; }
        }

        public string Description {
            get { return "Retrieves movie cover artwork previously downloaded via MyVideos."; }
        }

        public string Language {
            get { return ""; }
        }

        public bool ProvidesMoviesDetails {
            get { return true; }
        }

        public bool ProvidesCoverArt {
            get { return true; }
        }

        public bool ProvidesBackdrops {
            get { return false; }
        }

        public List<DBMovieInfo> Get(MovieSignature movieSignature) {
            List<DBMovieInfo> results = new List<DBMovieInfo>();
            if (movieSignature == null)
                return results;

            string idMovie = getMovieID(movieSignature.File);
            if (idMovie == String.Empty)
                return results;

            DBMovieInfo movie = this.getMovieInfo(idMovie);
            if (movie == null)
                return results;
            
            results.Add(movie);
            return results;
        }

        public UpdateResults Update(DBMovieInfo movie) {
            if (movie == null)
                return UpdateResults.FAILED;

            if (movie != null)
            {
                movie.CopyUpdatableValues(movie);
                return UpdateResults.SUCCESS;
            }
            return UpdateResults.FAILED;
        }

        public bool GetArtwork(DBMovieInfo movie) {
            string myVideoCoversFolder = Config.GetFolder(Config.Dir.Thumbs) + "\\Videos\\Title";
            
            Regex cleaner = new Regex("[\\\\/:*?\"<>|]");
            string cleanTitle = cleaner.Replace(movie.Title, "_");

            string filename = myVideoCoversFolder + "\\" + cleanTitle + "L.jpg";

            if (System.IO.File.Exists(filename)) 
                return movie.AddCoverFromFile(filename);
            
            return false;
        }

        public bool GetBackdrop(DBMovieInfo movie) {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Method to find the file's unique id in the MyVideo's database
        /// </summary>
        /// <param name="fileName">Filename to look for in the MyVideo's database</param>
        /// <returns>unique id as string</returns>
        private string getMovieID(string fileName)
        {
            string idMovie = String.Empty;
            fileName = fileName.Replace("'", "''");
            try
            {
                SQLiteClient mp_db = new SQLiteClient(Config.GetFile(Config.Dir.Database, @"VideoDatabaseV5.db3"));
                SQLiteResultSet results = mp_db.Execute("SELECT idMovie FROM files WHERE strFilename LIKE '\\" + fileName + "'");
                idMovie = results.GetField(0, 0);
                mp_db.Close();
            }
            catch
            {
            }
            return idMovie;
        }

        private DBMovieInfo getMovieInfo(string idMovie)
        {
            DBMovieInfo movieRes = new DBMovieInfo();
            try
            {
                SQLiteClient mp_db = new SQLiteClient(Config.GetFile(Config.Dir.Database, @"VideoDatabaseV5.db3"));
                SQLiteResultSet sqlResults = mp_db.Execute("SELECT * FROM movieinfo WHERE idMovie LIKE '" + idMovie + "'");

                SQLiteResultSet.Row sqlRow = sqlResults.GetRow(0);
                System.Collections.Hashtable columns = sqlResults.ColumnIndices;

                movieRes.Popularity = int.Parse(sqlResults.GetField(0, int.Parse(columns["strVotes"].ToString())));
                movieRes.Runtime = int.Parse(sqlResults.GetField(0, int.Parse(columns["runtime"].ToString())));
                movieRes.Score = float.Parse(sqlResults.GetField(0, int.Parse(columns["fRating"].ToString())));
                movieRes.Year = int.Parse(sqlResults.GetField(0, int.Parse(columns["iYear"].ToString())));

                string Title = sqlResults.GetField(0, int.Parse(columns["strTitle"].ToString()));
                if (!Title.Contains("unknown"))
                    movieRes.Title = Title;

                string Certification = sqlResults.GetField(0, int.Parse(columns["mpaa"].ToString()));
                if (!Certification.Contains("unknown"))
                {
                    try {
                        Regex certParse = new Regex(@"Rated\s(?<cert>.+)\sfor");
                        Match match = certParse.Match(Certification);
                        movieRes.Certification = match.Groups["cert"].Value;
                    }
                    catch (Exception e) {
                        // if an error happens in the try we will not set a value for the Certification
                        if (e is ThreadAbortException)
                            throw e;
                    }
                }

                string Tagline = sqlResults.GetField(0, int.Parse(columns["strTagLine"].ToString()));
                if (!Tagline.Contains("unknown"))
                    movieRes.Tagline = Tagline;

                string Summary = sqlResults.GetField(0, int.Parse(columns["strPlotOutline"].ToString()));
                if (!Summary.Contains("unknown"))
                    movieRes.Summary = Summary;

                string imdb_id = sqlResults.GetField(0, int.Parse(columns["IMDBID"].ToString()));
                if (!imdb_id.Contains("unknown"))
                    movieRes.ImdbID = imdb_id;

                string genreMain = sqlResults.GetField(0, int.Parse(columns["strGenre"].ToString()));
                if (!genreMain.Contains("unknown"))
                {
                    string[] genreSplit = genreMain.Split('/');
                    foreach (string genre in genreSplit)
                    {
                        movieRes.Genres.Add(genre.Trim());
                    }
                }

                string castMain = sqlResults.GetField(0, int.Parse(columns["strCast"].ToString()));
                if (!castMain.Contains("unknown"))
                {
                    string[] castSplit = castMain.Split('\n');
                    foreach (string cast in castSplit)
                    {
                        string castFinal = cast;
                        if (cast.Contains(" as "))
                            castFinal = cast.Remove(cast.IndexOf(" as "));
                        movieRes.Actors.Add(castFinal.Trim());
                    }
                }

                string idDirector = sqlResults.GetField(0, int.Parse(columns["idDirector"].ToString()));
                if (!castMain.Contains("unknown"))
                {
                    SQLiteResultSet sqlDirector = mp_db.Execute("SELECT strActor FROM actors WHERE idActor LIKE '" + idDirector + "'");
                    movieRes.Directors.Add(sqlDirector.GetField(0, 0));
                }

                mp_db.Close();
            }
            catch
            {
                return null;
            }

            return movieRes;
        }

    }
}
