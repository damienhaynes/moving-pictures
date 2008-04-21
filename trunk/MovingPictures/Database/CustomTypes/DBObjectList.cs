using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Plugins.MovingPictures.Database.CustomTypes {
    public class DBObjectList<T>: DynamicList<T>, IStringSourcedObject 
        where T: DatabaseTable {

        public void LoadFromString(string strList) {
            // Why isn't there a String Tokenizer in C# ??

            int startIndex = 0;
            while (startIndex < strList.Length) {
                // find the start index of this token
                while (startIndex < strList.Length && strList[startIndex] == '|')
                    startIndex++;

                // figure it's length
                int len = 0;
                while (startIndex + len < strList.Length && strList[startIndex + len] != '|')
                    len++;

                // grab the cooresponding DB object
                string token = strList.Substring(startIndex, len).Trim();
                if (startIndex < strList.Length && token.Length > 0) {
                    try {
                        Add(MovingPicturesPlugin.DatabaseManager.Get<T>(int.Parse(token)));
                    }
                    catch (Exception) { }
                }

                startIndex += len;
            }
        }

        public override string ToString() {
            string rtn = "";

            if (this.Count > 0)
                rtn += "|";

            foreach (T currObj in this) {
                rtn += currObj.ID;
                rtn += "|";
            }

            return rtn;
        }
    }
}
