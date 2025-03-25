using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SystemToolsShared;

namespace CarcassDataSeeding;

public /*open*/ class DataSeeder<TDst, TJMo> : IDataSeeder where TDst : class where TJMo : class
{
    private readonly string _dataSeedFolder;
    private readonly ESeedDataType _seedDataType;
    private readonly string? _stringKeyFieldName;
    private readonly string _tableName;
    protected readonly IDataSeederRepository Repo;

    protected DataSeeder(string dataSeedFolder, IDataSeederRepository repo,
        ESeedDataType seedDataType = ESeedDataType.OnlyJson, string? stringKeyFieldName = null)
    {
        _dataSeedFolder = dataSeedFolder;
        Repo = repo;
        _seedDataType = seedDataType;
        _stringKeyFieldName = stringKeyFieldName;
        _tableName = Repo.GetTableName<TDst>();
    }

    public bool Create(bool checkOnly)
    {
        if (checkOnly)
            return AdditionalCheck(LoadFromJsonFile());

        //ეს ის ვარიანტია, როცა არც არსებულ ჩანაწერებს ვამოწმებთ და არც Json-დან შემოგვაქვს
        if (CheckRecordsExists())
            return false;

        if (_seedDataType == ESeedDataType.None)
            return true;

        var seedData = _seedDataType switch
        {
            ESeedDataType.OnlyRules => [],
            ESeedDataType.OnlyJson or ESeedDataType.RulesHasMorePriority or ESeedDataType.JsonHasMorePriority =>
                LoadFromJsonFile(),
            ESeedDataType.None => throw new ArgumentOutOfRangeException(),
            _ => throw new ArgumentOutOfRangeException()
        };

        var dataListByJson = _seedDataType switch
        {
            ESeedDataType.OnlyRules => [],
            ESeedDataType.OnlyJson or ESeedDataType.RulesHasMorePriority or ESeedDataType.JsonHasMorePriority =>
                Adapt(seedData),
            ESeedDataType.None => throw new ArgumentOutOfRangeException(),
            _ => throw new ArgumentOutOfRangeException()
        };

        var dataListByRules = _seedDataType switch
        {
            ESeedDataType.OnlyJson => [],
            ESeedDataType.OnlyRules or ESeedDataType.RulesHasMorePriority or ESeedDataType.JsonHasMorePriority =>
                CreateListByRules(),
            ESeedDataType.None => throw new ArgumentOutOfRangeException(),
            _ => throw new ArgumentOutOfRangeException()
        };

        if (_stringKeyFieldName is null)
            throw new Exception($"String key field name is not set for {_tableName}");

        var dataList = _seedDataType switch
        {
            ESeedDataType.OnlyJson => dataListByJson,
            ESeedDataType.OnlyRules => dataListByRules,
            ESeedDataType.RulesHasMorePriority => Adjust(dataListByRules, dataListByJson),
            ESeedDataType.JsonHasMorePriority => Adjust(dataListByJson, dataListByRules),
            ESeedDataType.None => throw new ArgumentOutOfRangeException(),
            _ => throw new ArgumentOutOfRangeException()
        };

        if (!Repo.CreateEntities(dataList))
            throw new Exception($"{_tableName} entities cannot be created");

        //აქ დამატებით ვუშვებ მონაცემების შემოწმებას და თუ რომელიმე აუცილებელი ჩანაწერი აკლია, რაც ლოგიკით განისაზღვრება,
        //მაშინ ისინიც ჩაემატება. ან თუ არასწორად არის რომელიმე ჩანაწერი, შეიცვლება. ან თუ ზედმეტია წაიშლება
        return AdditionalCheck(seedData);
    }

    protected virtual List<TDst> CreateListByRules()
    {
        return [];
    }

    protected virtual List<TDst> Adapt(List<TJMo> appClaimsSeedData)
    {
        var jsonModelType = typeof(TJMo);
        var jsonModelTypeProperties = jsonModelType.GetProperties();

        var tableDataType = typeof(TDst);
        var tableDataTypeProperties = tableDataType.GetProperties();

        // Find the intersection of property names between jsonModelTypeProperties and tableDataTypeProperties
        var commonProperties = jsonModelTypeProperties.Select(p => p.Name)
            .Intersect(tableDataTypeProperties.Select(p => p.Name)).ToList();

        return appClaimsSeedData.Select(s =>
        {
            var instance = Activator.CreateInstance<TDst>();
            foreach (var propName in commonProperties)
            {
                var jsonProp = jsonModelType.GetProperty(propName);
                var tableProp = tableDataType.GetProperty(propName);
                if (jsonProp == null || tableProp == null)
                    continue;
                var value = jsonProp.GetValue(s);
                tableProp.SetValue(instance, value);
            }

            return instance;
        }).ToList();
    }

    //private List<TDst> CreateListByJson()
    //{
    //    var seedData = LoadFromJsonFile();
    //    return Adapt(seedData);
    //}

    //public List<TJMo> LoadFromJsonFile(string? folderName = null, string? fileName = null)
    //{
    //    var folName = folderName ?? DataSeedFolder;
    //    var jsonFullFileName = Path.Combine(folName, fileName ?? $"{_tableName.Capitalize()}.json");
    //    return File.Exists(jsonFullFileName)
    //        ? FileLoader.LoadDeserializeResolve<List<TJMo>>(jsonFullFileName, true) ?? []
    //        : [];
    //}

    private List<TJMo> LoadFromJsonFile()
    {
        return LoadFromJsonFile<TJMo>(_dataSeedFolder, $"{_tableName.Capitalize()}.json");
    }

    protected static List<T> LoadFromJsonFile<T>(string folderName, string fileName)
    {
        var jsonFullFileName = Path.Combine(folderName, fileName);
        return File.Exists(jsonFullFileName)
            ? FileLoader.LoadDeserializeResolve<List<T>>(jsonFullFileName, true) ?? []
            : [];
    }

    private bool CheckRecordsExists()
    {
        //აქ true ბრუნდება იმ მიზნით, რომ თუ ეს მეთოდი კლასში არ გადააწერეს, ითვლებოდეს, რომ ჩანაწერები არსებობს
        //return true;
        return Repo.HaveAnyRecord<TDst>();
    }

    protected virtual bool AdditionalCheck(List<TJMo> jMos)
    {
        return true;
    }

    private List<TDst> Adjust(List<TDst> listWithMorePriority, List<TDst> listWithLessPriority)
    {
        if (_stringKeyFieldName is null)
            throw new Exception($"String key field name is not set for {_tableName}");

        var tableDataType = typeof(TDst);
        var keyProperty = tableDataType.GetProperty(_stringKeyFieldName);

        if (keyProperty is null)
            throw new Exception($"KeyProperty {_stringKeyFieldName} does not exists {_tableName}");

        var duplicatePriorityKeys = listWithMorePriority.GroupBy(KeySelector).Where(group => group.Count() > 1)
            .Select(group => group.Key).ToList();

        if (duplicatePriorityKeys.Count != 0)
        {
            var strKeyList = string.Join(", ", duplicatePriorityKeys);
            Console.WriteLine("Priority keys contains duplicate keys {0}", strKeyList);
            throw new Exception("Priority keys contains duplicate keys");
        }

        var duplicateSecondKeys = listWithLessPriority.GroupBy(KeySelector).Where(group => group.Count() > 1)
            .Select(group => group.Key).ToList();

        if (duplicateSecondKeys.Count != 0)
        {
            var strKeyList = string.Join(", ", duplicateSecondKeys);
            Console.WriteLine("Secondary keys contains duplicate keys {0}", strKeyList);
            throw new Exception("Secondary keys duplicate keys");
        }

        var priorDictionary = listWithMorePriority.ToDictionary(KeySelector, v => v);

        var secondDictionary = listWithLessPriority.ToDictionary(
            item => keyProperty.GetValue(item)?.ToString()?.ToLower() ??
                    throw new InvalidOperationException("Key property value cannot be null"), v => v);

        var retList = new List<TDst>();
        retList.AddRange(priorDictionary.Values);

        var extraKeys = priorDictionary.Keys.Except(secondDictionary.Keys).ToList();
        retList.AddRange(extraKeys.Select(key => priorDictionary[key]));

        return retList;

        string KeySelector(TDst item)
        {
            return keyProperty.GetValue(item)?.ToString()?.ToLower() ??
                   throw new InvalidOperationException("Key property value cannot be null");
        }
    }
}