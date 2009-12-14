using System;
using System.Collections.Generic;

namespace Cornerstone.Collections {

    public class CachedDictionary<TKey, TValue> : Dictionary<TKey, TValue> {

        private Dictionary<TKey, DateTime> cache {
            get {
                if (_cache == null)
                    _cache = new Dictionary<TKey, DateTime>();

                return _cache;
            }
        } private Dictionary<TKey, DateTime> _cache;

        /// <summary>
        /// Get/set the value after which items should expire
        /// </summary>
        public TimeSpan ExpireAfter {
            get { return ttl; }
            set { 
                if (value == null) 
                    ttl = TimeSpan.Zero;
                else
                    ttl = value;  
                }
        } private TimeSpan ttl = TimeSpan.Zero;
        
        /// <summary>
        /// Gets a value indicating wether this item has expired
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasExpired(TKey key) {
            if (ttl != TimeSpan.Zero && cache.ContainsKey(key)) {
                return (cache[key].Add(ttl) <= DateTime.Now);
            }
            return true;
        }

        #region Dictionary methods

        public new void Add(TKey key, TValue value) {
            cache.Add(key, DateTime.Now);
            base.Add(key, value);
        }

        public new bool Remove(TKey key) {
            cache.Remove(key);
            return base.Remove(key);
        }

        public new TValue this[TKey key] {
            get {
                return base[key];
            }
            set {
                cache[key] = DateTime.Now;
                base[key] = value;
            }
        }

        public new void Clear() {
            cache.Clear();
            base.Clear();
        }

        #endregion

    }
}
