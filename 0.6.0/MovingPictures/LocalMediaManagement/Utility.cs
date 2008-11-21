using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {
    class Utility {
        
        public static string CreateFilename(string subject) {
            if( String.IsNullOrEmpty(subject))
                return string.Empty;

            string rtFilename = subject;

            char[] invalidFileChars = System.IO.Path.GetInvalidFileNameChars();
            foreach (char invalidFileChar in invalidFileChars)
                rtFilename = rtFilename.Replace(invalidFileChar, '_');

            return rtFilename;
        }

    }
}
