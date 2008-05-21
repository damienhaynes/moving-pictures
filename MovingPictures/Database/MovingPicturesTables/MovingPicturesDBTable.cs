using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables {
    public abstract class MoviesPluginDBTable: DatabaseTable {

        public MoviesPluginDBTable()
            : base() {
        }

        public void Commit() {
            if (DBManager == null)
                DBManager = MovingPicturesPlugin.DatabaseManager;

            base.Commit();
        }

        public void Delete() {
            if (DBManager == null)
                DBManager = MovingPicturesPlugin.DatabaseManager;

            base.Delete();
        }
    }
}
