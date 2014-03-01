﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace Cornerstone.Extensions {
    public static class JsonExtensions {
        /// <summary>
        /// Creates a list based on a JSON Array
        /// </summary>
        public static IEnumerable<T> FromJsonArray<T>(this string jsonArray) {
            if (string.IsNullOrEmpty(jsonArray)) return new List<T>();

            try {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonArray))) {
                    var ser = new DataContractJsonSerializer(typeof(IEnumerable<T>));
                    var result = (IEnumerable<T>)ser.ReadObject(ms);

                    if (result == null) {
                        return new List<T>();
                    }
                    else {
                        return result;
                    }
                }
            }
            catch (Exception) {
                return new List<T>();
            }
        }

        /// <summary>
        /// Creates an object from JSON
        /// </summary>
        public static T FromJson<T>(this string json) {
            if (string.IsNullOrEmpty(json)) return default(T);

            try {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json.ToCharArray()))) {
                    var ser = new DataContractJsonSerializer(typeof(T));
                    return (T)ser.ReadObject(ms);
                }
            }
            catch (Exception) {
                return default(T);
            }
        }

        /// <summary>
        /// Creates an object from JSON (this works with Dictionary types)
        /// Note: this does not use DataContracts from T
        /// </summary>
        public static T FromJsonDictionary<T>(this string json) {
            if (string.IsNullOrEmpty(json)) return default(T);

            try {
                var ser = new JavaScriptSerializer();
                return (T)ser.Deserialize<T>(json);
            }
            catch (Exception) {
                return default(T);
            }
        }

        /// <summary>
        /// Turns an object into JSON
        /// </summary>
        public static string ToJson(this object obj) {
            if (obj == null) return string.Empty;
            using (var ms = new MemoryStream()) {
                var ser = new DataContractJsonSerializer(obj.GetType());
                ser.WriteObject(ms, obj);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
}