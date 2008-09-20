using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database;
using Cornerstone.Database.Tables;

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

        // Friendly readable version number.
        public string Version {
            get { return versionMajor + "." + versionMinor + "." + versionPoint; }
        }

        // Major version number of script.
        [DBFieldAttribute(FieldName="version_major")]
        public int VersionMajor {
            get { return versionMajor; }
            set {
                versionMajor = value;
                commitNeeded = true;
            }
        } protected int versionMajor;

        // Minor version number of script.
        [DBFieldAttribute(FieldName = "version_minor")]
        public int VersionMinor {
            get { return versionMinor; }
            set {
                versionMinor = value;
                commitNeeded = true;
            }
        } protected int versionMinor;

        // Point version number of script.
        [DBFieldAttribute(FieldName = "version_point")]
        public int VersionPoint {
            get { return versionPoint; }
            set {
                versionPoint = value;
                commitNeeded = true;
            }
        } protected int versionPoint;

        #endregion

        public override bool Equals(object obj) {
            if (obj.GetType() != typeof(DBScriptInfo))
                return base.Equals(obj);

            return contents == ((DBScriptInfo)obj).contents;
        }

        public override int GetHashCode() {
            return contents.GetHashCode();
        }
    }
}
