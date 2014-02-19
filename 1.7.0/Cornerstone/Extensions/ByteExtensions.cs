using System.Runtime.InteropServices;
using System.Text;

namespace Cornerstone.Extensions {

    public static class ByteExtensions {

        /// <summary>
        /// Converts a byte array to a hexadecimal string (hash)
        /// </summary>
        /// <param name="self"></param>
        /// <returns>hexadecimal string</returns>
        public static string ToHexString(this byte[] self) {
            StringBuilder hexBuilder = new StringBuilder();
            for (int i = 0; i < self.Length; i++) {
                hexBuilder.Append(self[i].ToString("x2"));
            }
            return hexBuilder.ToString();
        }

        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        static extern long StrFormatByteSize(long fileSize, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer, int bufferSize);

        /// <summary>
        /// returns localised pretty string for byte size
        /// </summary>
        /// <param name="fileSize">filesize in bytes</param>
        public static string ToFormattedByteString(this long fileSize)
        {
            var sbBuffer = new StringBuilder(20);
            StrFormatByteSize(fileSize, sbBuffer, 20);
            return sbBuffer.ToString();
        }
    }
}
