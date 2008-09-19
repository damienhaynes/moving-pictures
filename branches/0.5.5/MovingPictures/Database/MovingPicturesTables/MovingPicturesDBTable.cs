using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database.Tables;

namespace MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables {
    public abstract class MovingPicturesDBTable: DatabaseTable {

        public MovingPicturesDBTable()
            : base() {
        }

        public override void Commit() {
            if (DBManager == null)
                DBManager = MovingPicturesCore.DatabaseManager;

            base.Commit();
        }

        public override void Delete() {
            if (DBManager == null)
                DBManager = MovingPicturesCore.DatabaseManager;

            base.Delete();
        }
    }
}
