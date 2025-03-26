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

    //virtual methods

    //ამ ვირტუალური მეთოდის დანიშნულებაა ბაზაში შენახვის მერე გადაამოწმოს შენახული ინფორმაცია,
    //ან თუ შენახვის მერე რაიმე დამატებით არის გასაკეთებელი, რომ გააკეთოს
    //List<TJMo> seedData საჭიროა შესადარებლად.
    //თუ ბაზიდან საჭიროა ინფორმაცია, გადატვირთულმა მეთოდმა თვითონ უნდა ჩატვირთოს
    protected virtual bool AdditionalCheck(List<TJMo> seedData)
    {
        return true;
    }

    //ამ ვირტუალური მეთოდის დანიშნულებაა შექმნას სპეციალური წესების მიხედვით. 
    //როცა ასეთი წესები გვაქვს.
    protected virtual List<TDst> CreateListByRules()
    {
        return [];
    }

    //ამ ვირტუალური მეთიდის დანიშნულებაა ჯეისონიდან ჩატვირთული ინფორმაცია აქციოს ბაზის ინფორმაციად
    //ეს რეალიზაცია გამოდგება მხოლოდ იმ შემთხვევებისთვის, როცა მოდელები ერთი ერთში გადადიან ერთმანეთში
    //დანარჩენ შემთხვევაში საჭიროა გადატვირთვა
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

    //ამ მეთოდის დანიშნულებაა ჯეისონიდან ჩატვირთოს ინფორმაცია და აქციოს მოდელების სიად
    protected static List<T> LoadFromJsonFile<T>(string folderName, string fileName)
    {
        var jsonFullFileName = Path.Combine(folderName, fileName);
        return File.Exists(jsonFullFileName)
            ? FileLoader.LoadDeserializeResolve<List<T>>(jsonFullFileName, true) ?? []
            : [];
    }

    //ეს არის ძირითადი მეთოდი, რომლის საშუალებითაც ხდება ერთი ცხრილის შესაბამისი ინფორმაციის ჩატვირთვა ბაზაში
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

    //ამ მეთოდის დანიშნულებაა ჯეისონიდან ჩატვირთოს ინფორმაცია კონკრეტული მოდელისათვის
    private List<TJMo> LoadFromJsonFile()
    {
        return LoadFromJsonFile<TJMo>(_dataSeedFolder, $"{_tableName.Capitalize()}.json");
    }

    //მეთოდი ამოწმებს ბაზაში უკვე არის თუ არა შეასაბამის ცხრილში ჩანაწერები
    private bool CheckRecordsExists()
    {
        return Repo.HaveAnyRecord<TDst>();
    }

    //ამ მეთოდის დანიშნულებაა ბაზაში ჩასაწერი სია მიიყვანოს უპირატესი სიის მიხედვით
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