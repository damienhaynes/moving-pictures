using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.GUI.Library;

namespace Cornerstone.MP.Extensions
{
    public static class GUIControlExtensions
    {
        /// <summary>
        /// Brings focus to this control
        /// </summary>
        /// <remarks>wrapper for GUIControl.FocusControl</remarks>
        /// <param name="self"></param>
        public static void Focus(this GUIControl self)
        {
            GUIControl.FocusControl(GUIWindowManager.ActiveWindow, self.GetID);
        }

        /// <summary>
        /// Brings focus to this control using the specified direction.
        /// </summary>
        /// <remarks>wrapper for GUIControl.FocusControl</remarks>
        /// <param name="self"></param>
        /// <param name="direction">The direction.</param>
        public static void Focus(this GUIControl self, GUIControl.Direction direction)
        {
            GUIControl.FocusControl(GUIWindowManager.ActiveWindow, self.GetID, direction);
        }

        /// <summary>
        /// Removes focus from this control
        /// </summary>
        /// <remarks>wrapper for GUIControl.UnfocusControl</remarks>
        /// <param name="self"></param>
        public static void Unfocus(this GUIControl self)
        {
            GUIControl.UnfocusControl(GUIWindowManager.ActiveWindow, self.GetID);
        }

    }
}
