using System;
using System.Collections.Generic;
using System.Threading;

namespace CarcassDataSeeding;

public sealed class DataSeederTempData
{
    private static DataSeederTempData? _instance;
    private static readonly Lock SyncRoot = new();

    private readonly Dictionary<Type, Dictionary<string, int>> _keyIdIntDictionary = [];
    private readonly Dictionary<Type, Dictionary<string, short>> _keyIdShortDictionary = [];
    private readonly Dictionary<Type, Dictionary<Tuple<int, int, short>, int>> _keyInt2ShortIdIntDictionary = [];
    private readonly Dictionary<Type, Dictionary<Tuple<int, int, int>, int>> _keyInt3IdIntDictionary = [];
    private readonly Dictionary<Type, Dictionary<Tuple<int, int, int, int>, int>> _keyInt4IdIntDictionary = [];
    private readonly Dictionary<Type, Dictionary<Tuple<int, int, int, int, int>, int>> _keyInt5IdIntDictionary = [];

    private readonly Dictionary<Type, Dictionary<Tuple<int, int, int, int, int, int>, int>>
        _keyInt6IdIntDictionary = [];

    private readonly Dictionary<Type, Dictionary<Tuple<int, DateTime>, int>> _keyIntDatetimeIdIntDictionary = [];

    private readonly Dictionary<Type, Dictionary<int, int>> _keyIntIdIntDictionary = [];
    private readonly Dictionary<Type, Dictionary<Tuple<int, int>, int>> _keyIntIntIdIntDictionary = [];
    private readonly Dictionary<Type, Dictionary<Tuple<int, short>, int>> _keyIntShortIdIntDictionary = [];
    private readonly Dictionary<Type, Dictionary<Tuple<int, string>, int>> _keyIntStringIdIntDictionary = [];
    private readonly Dictionary<Type, Dictionary<Tuple<string, int>, int>> _keyStringIntIdIntDictionary = [];
    private readonly Dictionary<Type, Dictionary<Tuple<string, short>, int>> _keyStringShortIdIntDictionary = [];
    private readonly Dictionary<Type, Dictionary<DateTime, DateTime>> _oldDateTimeIdsDictToDateTimeIds = [];
    private readonly Dictionary<Type, Dictionary<int, int>> _oldIntIdsDictToIntIds = [];
    private readonly Dictionary<Type, Dictionary<short, short>> _oldShortIdsDictToShortIds = [];

    private DataSeederTempData()
    {
    }

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

    public void SaveIntIdKeys<T>(Dictionary<Tuple<int, int, int, int, int, int>, int> dict)
    {
        if (_keyInt6IdIntDictionary.ContainsKey(typeof(T)))
            _keyInt6IdIntDictionary[typeof(T)] = dict;
        else
            _keyInt6IdIntDictionary.Add(typeof(T), dict);
    }

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
        var key = new Tuple<int, int, int, int>(key1, key2, key3, key4);
        if (!_keyInt4IdIntDictionary.ContainsKey(typeof(T)))
            throw new Exception($"Cannot get Keys for key {typeof(T)}");
        if (_keyInt4IdIntDictionary[typeof(T)].TryGetValue(key, out var value))
            return value;
        throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
    }

    public int GetIntIdByKey<T>(int key1, int key2, int key3, int key4, int key5, int key6)
    {
        var key = new Tuple<int, int, int, int, int, int>(key1, key2, key3, key4, key5, key6);
        if (!_keyInt6IdIntDictionary.ContainsKey(typeof(T)))
            throw new Exception($"Cannot get Keys for key {typeof(T)}");
        if (_keyInt6IdIntDictionary[typeof(T)].TryGetValue(key, out var value))
            return value;
        throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
    }

    public int GetIntIdByKey<T>(int key1, int key2, int key3, int key4, int key5)
    {
        var key = new Tuple<int, int, int, int, int>(key1, key2, key3, key4, key5);
        if (!_keyInt5IdIntDictionary.ContainsKey(typeof(T)))
            throw new Exception($"Cannot get Keys for key {typeof(T)}");
        if (_keyInt5IdIntDictionary[typeof(T)].TryGetValue(key, out var value))
            return value;
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
        var key = new Tuple<int, int, int>(key1, key2, key3);
        if (!_keyInt3IdIntDictionary.ContainsKey(typeof(T)))
            throw new Exception($"Cannot get Keys for key {typeof(T)}");
        if (_keyInt3IdIntDictionary[typeof(T)].TryGetValue(key, out var value))
            return value;
        throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
    }

    public int GetIntIdByKey<T>(int key1, int key2, short key3)
    {
        var key = new Tuple<int, int, short>(key1, key2, key3);
        if (!_keyInt2ShortIdIntDictionary.ContainsKey(typeof(T)))
            throw new Exception($"Cannot get Keys for key {typeof(T)}");
        if (_keyInt2ShortIdIntDictionary[typeof(T)].TryGetValue(key, out var value))
            return value;
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
        var key = new Tuple<string, short>(key1, key2);
        if (!_keyStringShortIdIntDictionary.ContainsKey(typeof(T)))
            throw new Exception($"Cannot get Keys for key {typeof(T)}");
        if (_keyStringShortIdIntDictionary[typeof(T)].TryGetValue(key, out var value))
            return value;
        throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
    }

    public void SaveIntIdKeys<T>(Dictionary<Tuple<string, int>, int> dict)
    {
        if (_keyStringIntIdIntDictionary.ContainsKey(typeof(T)))
            _keyStringIntIdIntDictionary[typeof(T)] = dict;
        else
            _keyStringIntIdIntDictionary.Add(typeof(T), dict);
    }

    //public საჭიროა GanmartebaGe ბაზისათვის
    // ReSharper disable once MemberCanBePrivate.Global
    public int GetIntIdByKey<T>(string key1, int key2)
    {
        var key = new Tuple<string, int>(key1, key2);
        if (!_keyStringIntIdIntDictionary.ContainsKey(typeof(T)))
            throw new Exception($"Cannot get Keys for key {typeof(T)}");
        if (_keyStringIntIdIntDictionary[typeof(T)].TryGetValue(key, out var value))
            return value;
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
        var key = new Tuple<int, string>(key1, key2);
        if (!_keyIntStringIdIntDictionary.ContainsKey(typeof(T)))
            throw new Exception($"Cannot get Keys for key {typeof(T)}");
        if (_keyIntStringIdIntDictionary[typeof(T)].TryGetValue(key, out var value))
            return value;
        throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
    }

    public void SaveIntIdKeys<T>(Dictionary<Tuple<int, DateTime>, int> dict)
    {
        if (_keyIntDatetimeIdIntDictionary.ContainsKey(typeof(T)))
            _keyIntDatetimeIdIntDictionary[typeof(T)] = dict;
        else
            _keyIntDatetimeIdIntDictionary.Add(typeof(T), dict);
    }

    public int GetIntIdByKey<T>(int key1, DateTime key2)
    {
        var key = new Tuple<int, DateTime>(key1, key2);
        if (!_keyIntDatetimeIdIntDictionary.ContainsKey(typeof(T)))
            throw new Exception($"Cannot get Keys for key {typeof(T)}");
        if (_keyIntDatetimeIdIntDictionary[typeof(T)].TryGetValue(key, out var value))
            return value;
        throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
    }

    public void SaveIntIdKeys<T>(Dictionary<Tuple<int, int>, int> dict)
    {
        if (_keyIntIntIdIntDictionary.ContainsKey(typeof(T)))
            _keyIntIntIdIntDictionary[typeof(T)] = dict;
        else
            _keyIntIntIdIntDictionary.Add(typeof(T), dict);
    }

    public void SaveIntIdKeys<T>(Dictionary<Tuple<int, short>, int> dict)
    {
        if (_keyIntShortIdIntDictionary.ContainsKey(typeof(T)))
            _keyIntShortIdIntDictionary[typeof(T)] = dict;
        else
            _keyIntShortIdIntDictionary.Add(typeof(T), dict);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public int GetIntIdByKey<T>(int key1, int key2)
    {
        var key = new Tuple<int, int>(key1, key2);
        if (!_keyIntIntIdIntDictionary.ContainsKey(typeof(T)))
            throw new Exception($"Cannot get Keys for key {typeof(T)}");
        if (_keyIntIntIdIntDictionary[typeof(T)].TryGetValue(key, out var value))
            return value;
        throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
    }

    public int GetIntIdByKey<T>(int key1, short key2)
    {
        var key = new Tuple<int, short>(key1, key2);
        if (!_keyIntShortIdIntDictionary.ContainsKey(typeof(T)))
            throw new Exception($"Cannot get Keys for key {typeof(T)}");
        if (_keyIntShortIdIntDictionary[typeof(T)].TryGetValue(key, out var value))
            return value;
        throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
    }

    public int? GetIntNullableIdByKey<T>(string? key1, int? key2)
    {
        if (key1 == null || key2 == null)
            return null;
        return GetIntIdByKey<T>(key1, key2.Value);
    }

    public int? GetIntNullableIdByKey<T>(string? key1, short? key2)
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
        if (_keyIntIdIntDictionary[typeof(T)].TryGetValue(key, out var value))
            return value;
        throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
    }

    public void SaveShortIdKeys<T>(Dictionary<string, short> dict)
    {
        if (_keyIdShortDictionary.ContainsKey(typeof(T)))
            _keyIdShortDictionary[typeof(T)] = dict;
        else
            _keyIdShortDictionary.Add(typeof(T), dict);
    }

    public void SaveIntIdKeys<T>(Dictionary<string, int> dict)
    {
        if (_keyIdIntDictionary.ContainsKey(typeof(T)))
            _keyIdIntDictionary[typeof(T)] = dict;
        else
            _keyIdIntDictionary.Add(typeof(T), dict);
    }

    public short GetShortIdByKey<T>(string key)
    {
        if (!_keyIdShortDictionary.ContainsKey(typeof(T)))
            throw new Exception($"Cannot get Keys for key {typeof(T)}");
        if (_keyIdShortDictionary[typeof(T)].TryGetValue(key, out var value))
            return value;
        throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {key}");
    }

    public int GetIntIdByKey<T>(string key)
    {
        if (!_keyIdIntDictionary.ContainsKey(typeof(T)))
            throw new Exception($"Cannot get Keys for key {typeof(T)}");
        if (_keyIdIntDictionary[typeof(T)].TryGetValue(key, out var value))
            return value;
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
        if (_oldIntIdsDictToIntIds[typeof(T)].TryGetValue(oldId, out var value))
            return value;
        throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {oldId}");
    }

    public void SaveOldShortIdsDictToShortIds<T>(Dictionary<short, short> dict)
    {
        if (_oldShortIdsDictToShortIds.ContainsKey(typeof(T)))
            _oldShortIdsDictToShortIds[typeof(T)] = dict;
        else
            _oldShortIdsDictToShortIds.Add(typeof(T), dict);
    }

    public short GetShortIdByOldId<T>(short oldId)
    {
        if (!_oldShortIdsDictToShortIds.ContainsKey(typeof(T)))
            throw new Exception($"Cannot get Keys for key {typeof(T)}");
        if (_oldShortIdsDictToShortIds[typeof(T)].TryGetValue(oldId, out var value))
            return value;
        throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {oldId}");
    }

    public void SaveOldDatetimeIdsDictToDatetimeIds<T>(Dictionary<DateTime, DateTime> dict)
    {
        if (_oldDateTimeIdsDictToDateTimeIds.ContainsKey(typeof(T)))
            _oldDateTimeIdsDictToDateTimeIds[typeof(T)] = dict;
        else
            _oldDateTimeIdsDictToDateTimeIds.Add(typeof(T), dict);
    }

    public DateTime GetDatetimeIdByOldId<T>(DateTime oldId)
    {
        if (!_oldDateTimeIdsDictToDateTimeIds.ContainsKey(typeof(T)))
            throw new Exception($"Cannot get Keys for key {typeof(T)}");
        if (_oldDateTimeIdsDictToDateTimeIds[typeof(T)].TryGetValue(oldId, out var value))
            return value;
        throw new Exception($"Cannot get Id for key {typeof(T).Name} and key {oldId}");
    }

    public int? GetIntNullableIdByOldId<T>(int? oldId)
    {
        if (oldId == null)
            return null;
        return GetIntIdByOldId<T>(oldId.Value);
    }

    public short? GetShortNullableIdByOldId<T>(short? oldId)
    {
        if (oldId == null)
            return null;
        return GetShortIdByOldId<T>(oldId.Value);
    }

    public int? GetIntNullableIdByKey<T>(string? key)
    {
        if (key == null)
            return null;
        return GetIntIdByKey<T>(key);
    }

    public short? GetShortNullableIdByKey<T>(string? key)
    {
        if (key == null)
            return null;
        return GetShortIdByKey<T>(key);
    }

}