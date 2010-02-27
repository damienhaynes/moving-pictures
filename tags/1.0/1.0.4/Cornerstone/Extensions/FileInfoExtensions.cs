using System.IO;

namespace Cornerstone.Extensions {

    public static class FileInfoExtensions {

        public static bool IsLocked(this FileInfo self) {
            FileStream stream = null;
            try {
                stream = self.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException) {
                return true;
            }
            finally {
                if (stream != null) stream.Close();
            }

            return false;
        }

    }
}
