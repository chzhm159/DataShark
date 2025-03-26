using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization.Metadata;

namespace DataShark.Server.utils {
    internal class CacheByFile {
        private static readonly Lazy<CacheByFile> _instance = new Lazy<CacheByFile>(() => new CacheByFile());
        int writeTimes = 0;        
        // public int CacheToFlush = 10;
        private JsonObject _cache;

        private readonly string _filePath = "config.json";

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private CacheByFile() {
            
            // CacheToFlush = AppCfg.GetIntVaue("CacheToFlush", CacheToFlush);
            _cache = new JsonObject();
            // Load data from file if the file exists
            LoadFromFile();
        }

        public static CacheByFile Inst => _instance.Value;

        public void AddOrUpdate(string key, string value) {
            _lock.EnterWriteLock();
            try {
                if (_cache[key] != null) {
                    _cache[key] = value;
                } else {
                    _cache.Add(key, value);
                }
                SaveToFile();
            } finally {
                _lock.ExitWriteLock();
            }
        }
        public void AddOrUpdate(string key, int value) {
            _lock.EnterWriteLock();
            try {
                if (_cache[key] != null) {
                    _cache[key] = value;
                } else {
                    _cache.Add(key, value);
                }
                SaveToFile();
            } finally {
                _lock.ExitWriteLock();
            }
        }
        public void AddOrUpdate(string key, long value, bool flush = true) {
            _lock.EnterWriteLock();
            try {
                if (_cache[key] != null) {
                    _cache[key] = value;
                } else {
                    _cache.Add(key, value);
                }
                if (flush) {
                    SaveToFile();
                }
            } finally {
                _lock.ExitWriteLock();
            }
        }
        /// <summary>
        /// 向Key指向的string[]中添加一个不重复的string值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ArrayAdd(string key, string value) {
            if (string.IsNullOrEmpty(value)) return;
            _lock.EnterWriteLock();
            try {
                JsonArray? vl = _cache[key] as JsonArray;
                if (vl == null) {
                    vl = new JsonArray();
                    vl.Add(value);
                    _cache.Add(key, vl);
                } else {

                    JsonNode? exist = vl.ToList().Find(el => {
                        return string.Equals(value, el);
                    });
                    if (exist == null) {
                        vl.Add(value);
                    }
                }
                SaveToFile();
            } finally {
                _lock.ExitWriteLock();
            }
        }
        public void ArrayAdd<T>(string key, T v) {
            if (v == null) return;
            _lock.EnterWriteLock();
            try {
                JsonArray? vl = _cache[key] as JsonArray;
                if (vl == null) {
                    vl = new JsonArray();
                    vl.Add(v);
                    _cache.Add(key, vl);
                } else {
                    JsonNode? exist = vl.ToList().Find(el => { return v.Equals(el); });
                    if (exist == null) {
                        vl.Add(v);
                    }
                }
                SaveToFile();
            } finally {
                _lock.ExitWriteLock();
            }
        }
        public string FirstOfArray(string key) {
            _lock.EnterWriteLock();
            try {
                JsonArray? vl = _cache[key] as JsonArray;
                if (vl == null || vl.Count < 1) {
                    return "";
                } else {
                    JsonValue first = vl.First().AsValue();
                    string fv = first.ToString();
                    return fv;
                }
            } finally {
                SaveToFile();
                _lock.ExitWriteLock();
            }
        }
        public string RmFirstFromArray(string key) {
            _lock.EnterWriteLock();
            try {
                JsonArray? vl = _cache[key] as JsonArray;
                if (vl == null || vl.Count < 1) {
                    return "";
                } else {
                    JsonValue first = vl.First().AsValue();
                    string fv = first.ToString();
                    vl.RemoveAt(0);
                    return fv;
                }
            } finally {
                SaveToFile();
                _lock.ExitWriteLock();
            }
        }
        public void ClearArray(string key) {
            _lock.EnterWriteLock();
            try {
                _cache.Remove(key);
                //JsonArray? vl = _cache[key] as JsonArray;
                //if (vl == null || vl.Count < 1) {
                //    return ;
                //} else {
                //    JsonValue first = vl.First().AsValue();
                //    string fv = first.ToString();
                //    vl.RemoveAt(0);
                //    return fv;
                //}
            } finally {
                SaveToFile();
                _lock.ExitWriteLock();
            }
        }
        public void RmFromArray(string key, string v) {
            _lock.EnterWriteLock();
            try {
                // _cache.Remove(key);
                JsonArray? vl = _cache[key] as JsonArray;
                if (vl == null || vl.Count < 1) {
                    return;
                } else {
                    List<JsonNode> tmp = vl.ToList()!;
                    tmp.RemoveAll(ve => {
                        return string.Equals(v, ve.GetValue<string>());
                    });
                    JsonArray nArray = new JsonArray();
                    foreach (var jNode in tmp) {
                        nArray.Add(jNode);
                    }
                    _cache[key] = nArray;
                }
            } finally {
                SaveToFile();
                _lock.ExitWriteLock();
            }
        }
        public List<string> GetArray(string key) {
            _lock.EnterWriteLock();
            try {
                JsonArray? vl = _cache[key] as JsonArray;
                if (vl != null) {
                    return vl.GetValues<String>().ToList();
                } else {
                    return Array.Empty<string>().ToList();
                }
            } finally {
                _lock.ExitWriteLock();
            }
        }

        public string Get(string key, string defaultValue = "") {
            _lock.EnterReadLock();
            try {
                JsonNode? vobj = _cache[key];
                if (vobj == null) {
                    return defaultValue;
                }
                string? v = vobj.GetValue<string>();
                if (string.IsNullOrEmpty(v)) {
                    return defaultValue;
                }
                return v;
            } finally {
                _lock.ExitReadLock();
            }
        }
        public int Get(string key, int defaultValue = -1) {
            try {
                _lock.EnterReadLock();
                JsonNode? vobj = _cache[key];
                if (vobj == null) {
                    return defaultValue;
                }
                int v = vobj.GetValue<int>();
                return v;
            } finally {
                _lock.ExitReadLock();
            }
        }
        public T Get<T>(string key, T? t = default(T)) {
            _lock.EnterReadLock();
            try {
                JsonNode? vobj = _cache[key];
                if (vobj == null) {
                    return t;
                }
                T v = vobj.GetValue<T>();
                return v;
            } finally {
                _lock.ExitReadLock();
            }
        }
        public void Remove(string key) {
            _lock.EnterWriteLock();
            try {
                _cache.Remove(key);
                SaveToFile();
            } finally {
                _lock.ExitWriteLock();
            }
        }
        public JsonObject GetObject(string key) {
            _lock.EnterWriteLock();
            try {
                JsonNode? vobj = _cache[key];
                if (vobj != null) {
                    return vobj.AsObject();
                } else {
                    return null;
                }
            } finally {
                _lock.ExitWriteLock();
            }
        }
        public JsonValue GetValueFromObject(string key, string itemKey, bool del = false) {
            _lock.EnterWriteLock();
            try {
                JsonNode? vobj = _cache[key];
                if (vobj != null) {
                    JsonObject item = vobj.AsObject();
                    JsonNode? itemV = item[itemKey];
                    JsonValue? itemValue = itemV?.AsValue();
                    if (del) {
                        item.Remove(itemKey);
                        SaveToFile();
                    }
                    return itemValue;
                } else {
                    return null;
                }
            } finally {
                _lock.ExitWriteLock();
            }
        }
        public void Add2Object<T>(string key, string itemKey, T itemValue) {
            try {
                _lock.EnterReadLock();
                JsonNode? vobj = _cache[key];
                if (vobj == null) {
                    // JsonNode.Parse(@"{}");
                    JsonObject jo = new JsonObject();
                    _cache[key] = jo;
                    vobj = jo;
                }
                // int v = vobj.GetValue<int>();
                // itemValue as JsonToken;
                JsonValue? v = JsonValue.Create(itemValue);
                JsonObject obj = vobj.AsObject();
                if (!obj.ContainsKey(itemKey)) {
                    obj.Add(itemKey, v);
                } else {
                    obj[itemKey] = v;
                }

                SaveToFile();
            } finally {
                _lock.ExitReadLock();
            }
        }

        public void RemoveItemFromObject(string key, string itemKey) {
            try {
                _lock.EnterReadLock();
                JsonNode? vobj = _cache[key];
                if (vobj == null) {
                    // JsonNode.Parse(@"{}");
                    JsonObject jo = new JsonObject();
                    vobj = jo;
                }
                vobj?.AsObject().Remove(itemKey);
                SaveToFile();
            } finally {
                _lock.ExitReadLock();
            }
        }
        private void LoadFromFile() {
            if (File.Exists(_filePath)) {
                string jsonData;
                using (var stream = File.OpenRead(_filePath))
                using (var reader = new StreamReader(stream)) {
                    jsonData = reader.ReadToEnd();
                }
                if (!string.IsNullOrEmpty(jsonData)) {
                    JsonNode? jnode = JsonObject.Parse(jsonData);
                    if (jnode != null) {
                        _cache = jnode.AsObject();
                    }
                }
            }
        }
        JsonSerializerOptions options = new(JsonSerializerDefaults.Web) {
            WriteIndented = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        
        public void SaveToFile() {
            //if (flush) {

            //}
            //writeTimes++;
            //if (writeTimes < CacheToFlush) return;
            //writeTimes = 0;

            string jsonData = _cache.ToJsonString(options: options);

            using (FileStream fs = new FileStream(
                   path: _filePath,
                   mode: FileMode.OpenOrCreate,
                   access: FileAccess.ReadWrite,
                   share: FileShare.None,
                   bufferSize: 4096,
                   useAsync: false)) {

                var bits = Encoding.UTF8.GetBytes(jsonData);
                fs.Seek(0, SeekOrigin.Begin);
                fs.SetLength(0);
                fs.Write(bits, 0, bits.Length);
                fs.Flush();
            }
        }
    
}
}
