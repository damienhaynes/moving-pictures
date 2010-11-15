using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.DataProviders;
using System.Threading;

namespace MediaPortal.Plugins.MovingPictures.Database {
    [DBTableAttribute("scripts")]
    public class DBScriptInfo: MovingPicturesDBTable {
        #region Database Fields

        [DBFieldAttribute]
        public string Contents {
            get { return contents; }

            set {
                contents = value;
                commitNeeded = true;
            }
        } private string contents;

        #endregion

        public IScriptableMovieProvider Provider {
            get {
                if (provider == null && contents != null && contents.Trim().Length != 0) {
                    Reload();
                }

                return provider;
            }
        } ScriptableProvider provider = null;

        public void Reload() {
            provider = new ScriptableProvider();
            if (!provider.Load(Contents))
                provider = null;
        }

        public override bool Equals(object obj) {
            if (obj.GetType() != typeof(DBScriptInfo))
                return base.Equals(obj);

            return Provider.Equals(((DBScriptInfo)obj).Provider);
        }

        public override int GetHashCode() {
            if (Provider != null)
                return Provider.GetHashCode();

            return base.GetHashCode();
        }

        public override string ToString() {
            if (Provider != null)
                return "DBScriptInfo: " + Provider.Name + " (" + Provider.Version + ")";

            return base.ToString();
            
        }
    }
}
