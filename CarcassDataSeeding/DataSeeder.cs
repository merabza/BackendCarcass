using System.Collections.Generic;
using System.IO;
using SystemToolsShared;

namespace CarcassDataSeeding;

public /*open*/ class DataSeeder<TDst> : IDataSeeder where TDst : class
{
    protected readonly IDataSeederRepository Repo;
    protected readonly List<string> Messages = new();

    private readonly string _dataSeedFolder;
    private readonly string _tableName;

    protected DataSeeder(string dataSeedFolder, IDataSeederRepository repo)
    {
        _dataSeedFolder = dataSeedFolder;
        Repo = repo;
        _tableName = Repo.GetTableName<TDst>();
    }


    protected List<T> LoadFromJsonFile<T>(string folderName = null, string fileName = null)
    {
        string folName = folderName ?? _dataSeedFolder;
        if (folName == null)
            return new List<T>();
        string jsonFullFileName = Path.Combine(folName, fileName ?? $"{_tableName}.json");
        return File.Exists(jsonFullFileName)
            ? FileLoader.LoadDeserializeResolve<List<T>>(jsonFullFileName, true)
            : new List<T>();
    }

    private bool CheckRecordsExists()
    {
        //აქ true ბრუნდება იმ მიზნით, რომ თუ ეს მეთოდი კლასში არ გადააწერეს, ითვლებოდეს, რომ ჩანაწერები არსებობს
        //return true;
        return Repo.HaveAnyRecord<TDst>();
    }


    public (bool success, List<string> messages) Create(bool checkRecordsExists = true)
    {
        bool success;

        if (checkRecordsExists)
        {
            //ეს ის ვარიანტია, როცა არც არსებულ ჩანაწერებს ვამოწმებთ და არც Json-დან შემოგვაქვს
            if (CheckRecordsExists())
            {
                Messages.Add($"Records in {_tableName} is already exists");
                return (false, Messages);
            }

            //ლოგიკა ასეთია: ჯერ მოისინჯება Json ფაილის გამოყენება
            success = CreateByJsonFile();
            //თუ რამე შეცდომა მოხდა ამ დროს, აქედანაც დაბრუნდება შეცდომა
            if (!success)
                return (false, Messages);
        }

        //აქ დამატებით ვუშვებ მონაცემების შემოწმებას და თუ რომელიმე აუცილებელი ჩანაწერი აკლია, რაც ლოგიკით განისაზღვრება,
        //მაშინ ისინიც ჩაემატება. ან თუ არასწორად არის რომელიმე ჩანაწერი, შეიცვლება. ან თუ ზედმეტია წაიშლება
        success = AdditionalCheck();
        return (success, Messages);
    }

    protected virtual bool CreateByJsonFile()
    {
        return true;
    }

    protected virtual bool AdditionalCheck()
    {
        return true;
    }

    protected virtual List<TDst> CreateMustList()
    {
        return null;
    }


    protected virtual bool RemoveNeedlessRecords(List<TDst> needLessList)
    {
        return Repo.RemoveNeedlessRecords(needLessList);
    }
}