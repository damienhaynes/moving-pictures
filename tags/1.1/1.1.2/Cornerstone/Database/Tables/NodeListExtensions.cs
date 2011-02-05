using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornerstone.Database.Tables {
    public static class NodeListExtensions {

        public static void Normalize<T>(this IList<DBNode<T>> list, bool commit) where T : DatabaseTable {
            int index = 0;
            foreach (DBNode<T> currNode in list) {
                if (currNode.SortPosition != index) {
                    currNode.SortPosition = index;
                    if (commit) currNode.Commit();
                }

                index++;
            }
        }
        
        public static bool MoveUp<T>(this List<DBNode<T>> list, DBNode<T> item, bool commit) where T : DatabaseTable {
            int index = list.IndexOf(item);
            if (index <= 0)
                return false;

            list.Reverse(index - 1, 2);
            list.Normalize(commit);

            return true;
        }

        public static bool MoveDown<T>(this List<DBNode<T>> list, DBNode<T> item, bool commit) where T : DatabaseTable {
            int index = list.IndexOf(item);
            if (index >= list.Count - 1 || index < 0)
                return false;

            list.Reverse(index, 2);
            list.Normalize(commit);

            return true;
        }

        public static void Normalize<T>(this IList<DBNode<T>> list) where T : DatabaseTable {
            list.Normalize(false);
        }

        public static bool MoveUp<T>(this List<DBNode<T>> list, DBNode<T> item) where T : DatabaseTable {
            return list.MoveUp(item, false);
        }

        public static bool MoveDown<T>(this List<DBNode<T>> list, DBNode<T> item) where T : DatabaseTable {
            return list.MoveDown(item, false);
        }


    }
}
