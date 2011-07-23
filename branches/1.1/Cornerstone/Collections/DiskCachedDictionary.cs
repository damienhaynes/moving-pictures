using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Cornerstone.Collections {
    public class DiskCachedDictionary<TKey, TValue> : CachedDictionary<TKey, int> {
        BinaryFormatter serializer = new BinaryFormatter();

        private bool initialized = false;
        private string cacheLocation = null;

        public void Init() {
            if (initialized) return;
            initialized = true;

            cacheLocation = Path.GetTempPath() + @"cornerstone\" + GetHashCode() + @"\";
            Directory.CreateDirectory(cacheLocation);
        }

        public void DeInit() {
            if (!initialized) return;

            Directory.Delete(cacheLocation, true);
            initialized = false;            
        }

        ~DiskCachedDictionary() {
            DeInit();
        }

        private int Serialize(TKey key, TValue value) {
            Init();

            int lookup = key.GetHashCode();
            FileStream stream = File.Create(cacheLocation + lookup);

            serializer.Serialize(stream, value);
            stream.Close();

            return lookup;
        }

        private TValue Deserialize(int lookup) {
            Init();

            FileStream stream = File.OpenRead(cacheLocation + lookup);
            return (TValue)serializer.Deserialize(stream);
        }

        public void Add(TKey key, TValue value) {
            int lookup = Serialize(key, value);
            base.Add(key, lookup);
        }

        public override bool Remove(TKey key) {
            File.Delete(cacheLocation + key.GetHashCode());
            return base.Remove(key);
        }

        public new TValue this[TKey key] {
            get {
                int lookup = base[key];
                return Deserialize(lookup);
            }
            set {
                int lookup = Serialize(key, value);
                base[key] = lookup;
            }
        }

        public override void Clear() {
            DeInit();
            base.Clear();
        }

        public bool TryGetValue(TKey key, out TValue value) {
            int lookup;
            
            bool success = base.TryGetValue(key, out lookup);
            if (success)
                value = Deserialize(lookup);
            else
                value = default(TValue);

            return success;
        }
    }
}
