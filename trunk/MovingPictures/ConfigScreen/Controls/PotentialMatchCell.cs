using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls {
    public class PotentialMatchCell: DataGridViewComboBoxCell {

        public override Type EditType {
            get {
                return typeof(PotentialMatchEditingControl);
            }
        }

        protected override void Paint(System.Drawing.Graphics graphics, System.Drawing.Rectangle clipBounds, System.Drawing.Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts) {
            if (value == null) {
                base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
                return;
            }

            PossibleMatch match = (PossibleMatch)value;

            // draw the native combo first, we are just painting on top of it, not redrawing from scratch
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, match.Movie.Title, errorText, cellStyle, advancedBorderStyle, paintParts);
            
            string yearText = "(" + match.Movie.Year + ")";
            string altTitleText = "as " + match.Result.AlternateTitle;
            string providerName = match.Movie.PrimarySource == null ? string.Empty : match.Movie.PrimarySource.Provider.Name;

            // figure basic positioning
            SizeF providerSize = graphics.MeasureString(providerName, DataGridView.Font);
            SizeF titleSize = graphics.MeasureString(match.Movie.Title, DataGridView.Font);
            SizeF yearSize = graphics.MeasureString(yearText, DataGridView.Font);
            
            int providerPosition = (int)(cellBounds.X + cellBounds.Width - providerSize.Width - 25);
            int yearPosition = (int)(cellBounds.X + titleSize.Width);
            int altTitlePosition = (int)(cellBounds.X + titleSize.Width + yearSize.Width);

            // draw year
            RectangleF yearRect = new RectangleF(cellBounds.X + titleSize.Width, cellBounds.Y + 4, cellBounds.Width - providerSize.Width - 25, cellBounds.Height);
            graphics.DrawString(yearText, DataGridView.Font, SystemBrushes.ControlDark, yearRect);

            // draw alternate title if needed
            if (match.Result.AlternateTitleUsed()) {
                graphics.DrawString(altTitleText, DataGridView.Font, SystemBrushes.ControlDark, altTitlePosition, cellBounds.Y + 4);
            }

            // draw data provider if needed
            if (!string.IsNullOrEmpty(providerName)) {
                bool uncertainMatch = match.Result.TitleScore > 0 || match.Result.YearScore > 0;
                if (MovingPicturesCore.Settings.AlwaysDisplayProviderTags || !match.Result.FromTopSource || (uncertainMatch && match.HasMultipleSources)) {
                    graphics.FillRectangle(SystemBrushes.ControlLight, providerPosition + 1, cellBounds.Y + 4, providerSize.Width + 1, cellBounds.Height - 10);
                    graphics.DrawString(providerName, DataGridView.Font, SystemBrushes.ControlDark, providerPosition + 1, cellBounds.Y + 4);
                }
            }
        }
    }
}
