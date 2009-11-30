using System;
using System.Collections.Generic;

namespace Cornerstone.Extensions {

    public static class IListExtensions {

        static Random randomizer;

        /// <summary>
        /// Returns a random object from the IList collection or null if it is empty
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static T Random<T>(this IList<T> list) where T : class {
            if (list.Count == 0)
                return null;

            if (randomizer == null)
                randomizer = new Random();
            
            if (list.Count > 1)
                return list[randomizer.Next(list.Count)];
            else
                return list[0];
        }

    }
}
