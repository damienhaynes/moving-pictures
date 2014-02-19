using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls {
    public class PotentialMatchColumn: DataGridViewComboBoxColumn {
        public PotentialMatchColumn() {
            this.CellTemplate = new PotentialMatchCell();
        }
    }
}
