using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures.DataProviders;

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
                if (provider == null) {
                    Reload();
                }

                return provider;
            }
        } ScriptableProvider provider = null;

        public void Reload() {
            provider = new ScriptableProvider();
            provider.Load(Contents);
        }

        public override bool Equals(object obj) {
            if (obj.GetType() != typeof(DBScriptInfo))
                return base.Equals(obj);

            return Provider.Equals(((DBScriptInfo)obj).Provider);
        }

        public override int GetHashCode() {
            return Provider.GetHashCode();
        }
    }
}
