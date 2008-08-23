using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables {
    [DBTableAttribute("users")]
    public class DBUser: MovingPicturesDBTable {

        public override void AfterDelete() {
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
            return MovingPicturesCore.DatabaseManager.Get<DBUser>(id);
        }

        public static List<DBUser> GetAll() {
            return MovingPicturesCore.DatabaseManager.Get<DBUser>(null);
        }

        #endregion

    }
}
