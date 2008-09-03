using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Plugins.MovingPictures.Database {
    // This class is primarily intended to ensure that when I database object is selected
    // multiple times from two different places in the code, both places will be working
    // with the same physical c# object. It also reduces retrieval time for get(id) type
    // queries.
    class DatabaseCache {
        private Dictionary<Type, Dictionary<int, DatabaseTable>> cache;

        public DatabaseCache() {
            cache = new Dictionary<Type, Dictionary<int, DatabaseTable>>();
        }

        public DatabaseTable Get(Type type, int id) {
            try {
                return cache[type][id];
            }
            catch (Exception) {
                return null;
            }
        }


        public ICollection<DatabaseTable> GetAll(Type type) {
            if (cache.ContainsKey(type))
                return cache[type].Values;

            return new List<DatabaseTable>();
        }

        // Adds the given element to the cacheing system
        public DatabaseTable Add(DatabaseTable obj) {
            if (obj == null || obj.ID == null)
                return obj;

            if (!cache.ContainsKey(obj.GetType()))
                cache[obj.GetType()] = new Dictionary<int,DatabaseTable>();

            if (!cache[obj.GetType()].ContainsKey((int)obj.ID))
                cache[obj.GetType()][(int)obj.ID] = obj;

            return cache[obj.GetType()][(int)obj.ID];
        }

        // Goes through the list and if any elements reference an object already in
        // memory, it updates the reference in the list with the in memory version.
        public void Sync(IList<DatabaseTable> list) {
            for (int i = 0; i < list.Count; i++) {
                DatabaseTable currObj = list[i];

                if (currObj == null || currObj.ID == null)
                    continue;

                try {
                    list[i] = (DatabaseTable)cache[currObj.GetType()][(int)currObj.ID];
                }
                catch (Exception) {
                    Add(currObj);
                }
            }
        }

        // Shoudl really only be called if an item has been deleted from the database
        public void Remove(DatabaseTable obj) {
            if (obj == null || obj.ID == null)
                return;

            cache[obj.GetType()].Remove((int)obj.ID);
        }

        
    }
}
