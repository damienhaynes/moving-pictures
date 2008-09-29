using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.DataProviders;
using Cornerstone.Database;
using Cornerstone.Database.CustomTypes;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.Database {
    [DBTableAttribute("source_info")]
    public class DBSourceInfo: MovingPicturesDBTable {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Database Fields
        
        [DBFieldAttribute]
        public Type ProviderType {
            get { return providerType; }
            set {
                providerType = value;
                commitNeeded = true;
            }
        } protected Type providerType;


        [DBFieldAttribute(Default="null")]
        public DBScriptInfo SelectedScript {
            get { return selectedScript; }
            set {
                selectedScript = value;
                commitNeeded = true;
            }
        } protected DBScriptInfo selectedScript = null;
        
        
        [DBRelation(AutoRetrieve = true)]
        public RelationList<DBSourceInfo, DBScriptInfo> Scripts {
            get {
                if (scripts == null) {
                    scripts = new RelationList<DBSourceInfo, DBScriptInfo>(this);
                }
                return scripts;
            }
        } RelationList<DBSourceInfo, DBScriptInfo> scripts;

        [DBFieldAttribute]
        public int? DetailsPriority {
            get { return detailsPriority; }
            set {
                detailsPriority = value;
                commitNeeded = true;
            }
        } protected int? detailsPriority;

        [DBFieldAttribute]
        public int? CoverPriority {
            get { return coverPriority; }
            set {
                coverPriority = value;
                commitNeeded = true;
            }
        } protected int? coverPriority;

        [DBFieldAttribute]
        public int? BackdropPriority {
            get { return backdropPriority; }
            set {
                backdropPriority = value;
                commitNeeded = true;
            }
        } protected int? backdropPriority;

        public void SetPriority(DataType type, int value) {
            switch (type) {
                case DataType.DETAILS:
                    detailsPriority = value;
                    break;
                case DataType.COVERS:
                    coverPriority = value; 
                    break;
                case DataType.BACKDROPS:
                    backdropPriority = value;
                    break;
            }
        }

        public int? GetPriority(DataType type) {
            switch (type) {
                case DataType.DETAILS:
                    return detailsPriority;
                case DataType.COVERS:
                    return coverPriority;
                case DataType.BACKDROPS:
                    return backdropPriority;
                default:
                    return null;
            }
        }

        #endregion

        #region Properties

        // Friendly name for the script.
        public string Name {
            get { return Provider.Name; }
        } 

        public IMovieProvider Provider {
            get {
                if (provider == null) {
                    provider = (IMovieProvider) Activator.CreateInstance(providerType);
                    if (selectedScript != null && selectedScript.Contents.Trim().Length != 0)
                        try {
                            ((IScriptableMovieProvider)provider).Load(selectedScript.Contents);
                        }
                        catch (Exception) {
                            logger.Warn("DataProvider tried to load a script when it doesn't support scripts!");
                            return null;
                        }
                }

                return provider;
            }
        } protected IMovieProvider provider = null;

        #endregion

        #region Static Methods

        public static List<DBSourceInfo> GetAll() {
            return MovingPicturesCore.DatabaseManager.Get<DBSourceInfo>(null);
        }

        #endregion

    }
}
