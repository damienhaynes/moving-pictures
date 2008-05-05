using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables {
    [DBTableAttribute("users")]
    public class DBUser: MoviesPluginDBTable {

        public override void CleanUpForDeletion() {
        }

        #region Database Fields

        [DBFieldAttribute(Default="New User")]
        public string Name {
            get { return name; }
            set { 
                name = value;
                commitNeeded = true;
            }
        } private string name;

        #endregion

        #region Database Management Methods

        public static DBUser Get(int id) {
            return MovingPicturesPlugin.DatabaseManager.Get<DBUser>(id);
        }

        public static List<DBUser> GetAll() {
            return MovingPicturesPlugin.DatabaseManager.Get<DBUser>(null);
        }

        #endregion

    }
}
