using System;
using System.Collections.Generic;

namespace CarcassDataSeeding
{
    public sealed class DataSeederTempData
    {
        private static DataSeederTempData _instance;
        private static readonly object SyncRoot = new();

        public static DataSeederTempData Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                lock (SyncRoot) //thread safe singleton
                {
                    _instance ??= new DataSeederTempData();
                }

                return _instance;
            }
        }

        private DataSeederTempData()
        {
        }

        private readonly Dictionary<Type, Dictionary<int, int>> _keyIntIdIntDictionary = new();

        private readonly Dictionary<Type, Dictionary<string, int>> _keyIdIntDictionary = new();

        private readonly Dictionary<Type, Dictionary<int, int>> _oldIntIdsDictToIntIds = new();

        private readonly Dictionary<Type, Dictionary<Tuple<int, int>, int>> _keyIntIntIdIntDictionary = new();

        private readonly Dictionary<Type, Dictionary<Tuple<string, short>, int>> _keyStringShortIdIntDictionary = new();

        private readonly Dictionary<Type, Dictionary<Tuple<string, int>, int>> _keyStringIntIdIntDictionary = new();

        private readonly Dictionary<Type, Dictionary<Tuple<int, string>, int>> _keyIntStringIdIntDictionary = new();

        private readonly Dictionary<Type, Dictionary<Tuple<int, int, int>, int>> _keyInt3IdIntDictionary = new();

        private readonly Dictionary<Type, Dictionary<Tuple<int, int, short>, int>> _keyInt2ShortIdIntDictionary = new();

        private readonly Dictionary<Type, Dictionary<Tuple<int, int, int, int>, int>> _keyInt4IdIntDictionary = new();

        private readonly Dictionary<Type, Dictionary<Tuple<int, int, int, int, int>, int>> _keyInt5IdIntDictionary =
            new();


        public void SaveIntIdKeys<T>(Dictionary<Tuple<int, int, int, int, int>, int> dict)
        {
            if (_keyInt5IdIntDictionary.ContainsKey(typeof(T)))
                _keyInt5IdIntDictionary[typeof(T)] = dict;
            else
                _keyInt5IdIntDictionary.Add(typeof(T), dict);
        }

        public void SaveIntIdKeys<T>(Dictionary<Tuple<int, int, int, int>, int> dict)
        {
            if (_keyInt4IdIntDictionary.ContainsKey(typeof(T)))
                _keyInt4IdIntDictionary[typeof(T)] = dict;
            else
                _keyInt4IdIntDictionary.Add(typeof(T), dict);
        }


        public int GetIntIdByKey<T>(int key1, int key2, int key3, int key4)
        {
            Tuple<int, int, int, int> key = new Tuple<int, int, int, int>(key1, key2, key3, key4);
            if (!_keyInt4IdIntDictionary.ContainsKey(typeof(T)))
                throw new Exception($"Cannot get Keys for key {typeof(T)}");
            if (_keyInt4IdIntDictionary[typeof(T)].ContainsKey(key))
                return _keyInt4IdIntDictionary[typeof(T)][key];
            throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
        }

        public int GetIntIdByKey<T>(int key1, int key2, int key3, int key4, int key5)
        {
            Tuple<int, int, int, int, int> key = new Tuple<int, int, int, int, int>(key1, key2, key3, key4, key5);
            if (!_keyInt5IdIntDictionary.ContainsKey(typeof(T)))
                throw new Exception($"Cannot get Keys for key {typeof(T)}");
            if (_keyInt5IdIntDictionary[typeof(T)].ContainsKey(key))
                return _keyInt5IdIntDictionary[typeof(T)][key];
            throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
        }


        public void SaveIntIdKeys<T>(Dictionary<Tuple<int, int, short>, int> dict)
        {
            if (_keyInt3IdIntDictionary.ContainsKey(typeof(T)))
                _keyInt2ShortIdIntDictionary[typeof(T)] = dict;
            else
                _keyInt2ShortIdIntDictionary.Add(typeof(T), dict);
        }

        public void SaveIntIdKeys<T>(Dictionary<Tuple<int, int, int>, int> dict)
        {
            if (_keyInt3IdIntDictionary.ContainsKey(typeof(T)))
                _keyInt3IdIntDictionary[typeof(T)] = dict;
            else
                _keyInt3IdIntDictionary.Add(typeof(T), dict);
        }


        public int GetIntIdByKey<T>(int key1, int key2, int key3)
        {
            Tuple<int, int, int> key = new Tuple<int, int, int>(key1, key2, key3);
            if (!_keyInt3IdIntDictionary.ContainsKey(typeof(T)))
                throw new Exception($"Cannot get Keys for key {typeof(T)}");
            if (_keyInt3IdIntDictionary[typeof(T)].ContainsKey(key))
                return _keyInt3IdIntDictionary[typeof(T)][key];
            throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
        }

        public int GetIntIdByKey<T>(int key1, int key2, short key3)
        {
            Tuple<int, int, short> key = new Tuple<int, int, short>(key1, key2, key3);
            if (!_keyInt2ShortIdIntDictionary.ContainsKey(typeof(T)))
                throw new Exception($"Cannot get Keys for key {typeof(T)}");
            if (_keyInt2ShortIdIntDictionary[typeof(T)].ContainsKey(key))
                return _keyInt2ShortIdIntDictionary[typeof(T)][key];
            throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
        }


        public void SaveIntIdKeys<T>(Dictionary<Tuple<string, short>, int> dict)
        {
            if (_keyStringShortIdIntDictionary.ContainsKey(typeof(T)))
                _keyStringShortIdIntDictionary[typeof(T)] = dict;
            else
                _keyStringShortIdIntDictionary.Add(typeof(T), dict);
        }

        public int GetIntIdByKey<T>(string key1, short key2)
        {
            Tuple<string, short> key = new Tuple<string, short>(key1, key2);
            if (!_keyStringShortIdIntDictionary.ContainsKey(typeof(T)))
                throw new Exception($"Cannot get Keys for key {typeof(T)}");
            if (_keyStringShortIdIntDictionary[typeof(T)].ContainsKey(key))
                return _keyStringShortIdIntDictionary[typeof(T)][key];
            throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
        }


        public void SaveIntIdKeys<T>(Dictionary<Tuple<string, int>, int> dict)
        {
            if (_keyStringIntIdIntDictionary.ContainsKey(typeof(T)))
                _keyStringIntIdIntDictionary[typeof(T)] = dict;
            else
                _keyStringIntIdIntDictionary.Add(typeof(T), dict);
        }


        public int GetIntIdByKey<T>(string key1, int key2)
        {
            Tuple<string, int> key = new Tuple<string, int>(key1, key2);
            if (!_keyStringIntIdIntDictionary.ContainsKey(typeof(T)))
                throw new Exception($"Cannot get Keys for key {typeof(T)}");
            if (_keyStringIntIdIntDictionary[typeof(T)].ContainsKey(key))
                return _keyStringIntIdIntDictionary[typeof(T)][key];
            throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
        }


        public void SaveIntIdKeys<T>(Dictionary<Tuple<int, string>, int> dict)
        {
            if (_keyIntStringIdIntDictionary.ContainsKey(typeof(T)))
                _keyIntStringIdIntDictionary[typeof(T)] = dict;
            else
                _keyIntStringIdIntDictionary.Add(typeof(T), dict);
        }


        public int GetIntIdByKey<T>(int key1, string key2)
        {
            Tuple<int, string> key = new Tuple<int, string>(key1, key2);
            if (!_keyIntStringIdIntDictionary.ContainsKey(typeof(T)))
                throw new Exception($"Cannot get Keys for key {typeof(T)}");
            if (_keyIntStringIdIntDictionary[typeof(T)].ContainsKey(key))
                return _keyIntStringIdIntDictionary[typeof(T)][key];
            throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
        }


        public void SaveIntIdKeys<T>(Dictionary<Tuple<int, int>, int> dict)
        {
            if (_keyIntIntIdIntDictionary.ContainsKey(typeof(T)))
                _keyIntIntIdIntDictionary[typeof(T)] = dict;
            else
                _keyIntIntIdIntDictionary.Add(typeof(T), dict);
        }


        public int GetIntIdByKey<T>(int key1, int key2)
        {
            Tuple<int, int> key = new Tuple<int, int>(key1, key2);
            if (!_keyIntIntIdIntDictionary.ContainsKey(typeof(T)))
                throw new Exception($"Cannot get Keys for key {typeof(T)}");
            if (_keyIntIntIdIntDictionary[typeof(T)].ContainsKey(key))
                return _keyIntIntIdIntDictionary[typeof(T)][key];
            throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
        }


        public int? GetIntNullableIdByKey<T>(string key1, int? key2)
        {
            if (key1 == null || key2 == null)
                return null;
            return GetIntIdByKey<T>(key1, key2.Value);
        }

        public int? GetIntNullableIdByKey<T>(string key1, short? key2)
        {
            if (key1 == null || key2 == null)
                return null;
            return GetIntIdByKey<T>(key1, key2.Value);
        }

        public int? GetIntNullableIdByKey<T>(int? key1, int? key2)
        {
            if (key1 == null || key2 == null)
                return null;
            return GetIntIdByKey<T>(key1.Value, key2.Value);
        }

        public int? GetIntNullableIdByKey<T>(string key)
        {
            if (key == null)
                return null;
            return GetIntIdByKey<T>(key);
        }


        public void SaveIntIdKeys<T>(Dictionary<int, int> dict)
        {
            if (_keyIntIdIntDictionary.ContainsKey(typeof(T)))
                _keyIntIdIntDictionary[typeof(T)] = dict;
            else
                _keyIntIdIntDictionary.Add(typeof(T), dict);
        }


        public int GetIntIdByKey<T>(int key)
        {
            if (!_keyIntIdIntDictionary.ContainsKey(typeof(T)))
                throw new Exception($"Cannot get Keys for key {typeof(T)}");
            if (_keyIntIdIntDictionary[typeof(T)].ContainsKey(key))
                return _keyIntIdIntDictionary[typeof(T)][key];
            throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
        }

        public void SaveIntIdKeys<T>(Dictionary<string, int> dict)
        {
            if (_keyIdIntDictionary.ContainsKey(typeof(T)))
                _keyIdIntDictionary[typeof(T)] = dict;
            else
                _keyIdIntDictionary.Add(typeof(T), dict);
        }

        public int GetIntIdByKey<T>(string key)
        {
            if (!_keyIdIntDictionary.ContainsKey(typeof(T)))
                throw new Exception($"Cannot get Keys for key {typeof(T)}");
            if (_keyIdIntDictionary[typeof(T)].ContainsKey(key))
                return _keyIdIntDictionary[typeof(T)][key];
            throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
        }

        public void SaveOldIntIdsDictToIntIds<T>(Dictionary<int, int> dict)
        {
            if (_oldIntIdsDictToIntIds.ContainsKey(typeof(T)))
                _oldIntIdsDictToIntIds[typeof(T)] = dict;
            else
                _oldIntIdsDictToIntIds.Add(typeof(T), dict);
        }

        public int GetIntIdByOldId<T>(int oldId)
        {
            if (!_oldIntIdsDictToIntIds.ContainsKey(typeof(T)))
                throw new Exception($"Cannot get Keys for key {typeof(T)}");
            if (_oldIntIdsDictToIntIds[typeof(T)].ContainsKey(oldId))
                return _oldIntIdsDictToIntIds[typeof(T)][oldId];
            throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {oldId}");
        }

        public int? GetIntNullableIdByOldId<T>(int? oldId)
        {
            if (oldId == null)
                return null;
            return GetIntIdByOldId<T>(oldId.Value);
        }
    }
}