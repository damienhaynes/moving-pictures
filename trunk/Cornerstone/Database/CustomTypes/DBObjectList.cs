using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database.Tables;
using System.Threading;

namespace Cornerstone.Database.CustomTypes {
    public class DBObjectList<T>: DynamicList<T>, IStringSourcedObject 
        where T: DatabaseTable {

        private DatabaseManager dbManager;

        public DBObjectList(DatabaseManager dbManager) {
            this.dbManager = dbManager;
        }

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

                // grab the corresponding DB object
                string token = strList.Substring(startIndex, len).Trim();
                if (startIndex < strList.Length && token.Length > 0) {
                    try {
                        Add(dbManager.Get<T>(int.Parse(token)));
                    }
                    catch (Exception e) {
                        if (e.GetType() == typeof(ThreadAbortException))
                            throw e;
                    }
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
