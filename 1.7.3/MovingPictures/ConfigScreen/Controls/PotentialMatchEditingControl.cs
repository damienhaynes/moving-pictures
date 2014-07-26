using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls {
    public class PotentialMatchEditingControl : DataGridViewComboBoxEditingControl {
        public PotentialMatchEditingControl() {
            this.DrawMode = DrawMode.OwnerDrawFixed;
        }
        
        protected override void OnDrawItem(DrawItemEventArgs e) {
            // draw the basic combo chrome (no text)
            base.OnDrawItem(e);
            e.DrawBackground();
            e.DrawFocusRectangle();

            // bail if we have nothing to paint
            if (e.Index < 0 || e.Index >= this.Items.Count) return;
            PossibleMatch match = (PossibleMatch)this.Items[e.Index];

            string yearText = "(" + match.Movie.Year + ")";
            string altTitleText = "as " + match.Result.AlternateTitle;

            // figure basic positioning
            SizeF providerSize = e.Graphics.MeasureString(match.Movie.PrimarySource.Provider.Name, e.Font);
            SizeF titleSize = e.Graphics.MeasureString(match.Movie.Title, e.Font);
            SizeF yearSize = e.Graphics.MeasureString(yearText, e.Font);

            RectangleF titleRect = new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - (providerSize.Width + 3), e.Bounds.Height);
            int altTitlePosition = (int)(e.Bounds.X + titleSize.Width + yearSize.Width + 5);
            int providerPosition = (int)(e.Bounds.X + e.Bounds.Width - providerSize.Width - 3);
            
            // paint title
            using (Brush titleBrush = new SolidBrush(e.ForeColor)) {
                e.Graphics.DrawString(match.Movie.Title, e.Font, titleBrush, titleRect);
            }

            // paint year
            RectangleF yearRect = new RectangleF(Bounds.X + titleSize.Width + 5, e.Bounds.Y, e.Bounds.Width - (providerSize.Width + 3), e.Bounds.Height);
            e.Graphics.DrawString("(" + match.Movie.Year + ")", e.Font, SystemBrushes.ControlDark, yearRect);

            // paint alternate title if needed
            if (match.Result.AlternateTitleUsed()) {
                e.Graphics.DrawString(altTitleText, e.Font, SystemBrushes.ControlDark, altTitlePosition, e.Bounds.Y);
            }

            // paint provider
            e.Graphics.FillRectangle(SystemBrushes.ControlLight, providerPosition + 1, e.Bounds.Y + 1, providerSize.Width, e.Bounds.Height - 3);
            e.Graphics.DrawRectangle(SystemPens.ControlDark, providerPosition + 1, e.Bounds.Y + 1, providerSize.Width, e.Bounds.Height - 3);
            e.Graphics.DrawString(match.Movie.PrimarySource.Provider.Name, e.Font, SystemBrushes.ControlDark, providerPosition + 1, e.Bounds.Y);
        }
        
    }
}
