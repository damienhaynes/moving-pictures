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

    }
}
