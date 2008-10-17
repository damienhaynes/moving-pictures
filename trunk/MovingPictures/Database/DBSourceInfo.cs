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
                    DetailsPriority = value;
                    break;
                case DataType.COVERS:
                    CoverPriority = value; 
                    break;
                case DataType.BACKDROPS:
                    BackdropPriority = value;
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

        public bool IsDisabled(DataType type) {
            return GetPriority(type) == -1;
        }

        public bool IsScriptable() {
            return !(SelectedScript == null || SelectedScript.Contents.Trim().Length == 0);
        }

        #endregion

        #region Properties

        public IMovieProvider Provider {
            get {
                if (SelectedScript != null && !(SelectedScript.Contents.Trim().Length == 0))
                    return SelectedScript.Provider;

                if (provider == null) 
                    provider = (IMovieProvider) Activator.CreateInstance(providerType);

                return provider;
            }
        } protected IMovieProvider provider = null;

        #endregion

        public override void Delete() {
            foreach (DBScriptInfo currScript in Scripts)
                currScript.Delete();

            base.Delete();
        }

        #region Static Methods

        public static List<DBSourceInfo> GetAll() {
            return MovingPicturesCore.DatabaseManager.Get<DBSourceInfo>(null);
        }

        #endregion

    }

    public class DBSourceInfoComparer : IComparer<DBSourceInfo> {
        private DataType sortType;
        
        public DBSourceInfoComparer(DataType sortType) {
            this.sortType = sortType;
        }

        public int Compare(DBSourceInfo x, DBSourceInfo y) {
            if (x.GetPriority(sortType) == -1 && y.GetPriority(sortType) == -1)
                return x.Provider.Name.CompareTo(y.Provider.Name);
            
            if (x.GetPriority(sortType) == -1)
                return 1;

            if (y.GetPriority(sortType) == -1)
                return -1;
            
            if (x.GetPriority(sortType) < y.GetPriority(sortType))
                return -1;

            if (x.GetPriority(sortType) > y.GetPriority(sortType))
                return 1;

            return 0;
        }
    }

}
