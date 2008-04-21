using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables {
    public class MoviesPluginDBTable: DatabaseTable {

        public MoviesPluginDBTable()
            : base() {
        }

        public void Commit() {
            MovingPicturesPlugin.DatabaseManager.Commit(this);
        }

        public void Delete() {
            MovingPicturesPlugin.DatabaseManager.Delete(this);
        }
    }
}
