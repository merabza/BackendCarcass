using System.Collections.Generic;
using System.IO;
using LanguageExt;
using SystemToolsShared;
using SystemToolsShared.Errors;

namespace CarcassDataSeeding;

public /*open*/ class DataSeeder<TDst> : IDataSeeder where TDst : class
{
    //protected readonly List<string> Messages = new();

    private readonly string _dataSeedFolder;
    private readonly string _tableName;
    protected readonly IDataSeederRepository Repo;

    protected DataSeeder(string dataSeedFolder, IDataSeederRepository repo)
    {
        _dataSeedFolder = dataSeedFolder;
        Repo = repo;
        _tableName = Repo.GetTableName<TDst>();
    }


    public Option<IEnumerable<Err>> Create(bool checkOnly)
    {
        if (checkOnly)
            return AdditionalCheck();

        //ეს ის ვარიანტია, როცა არც არსებულ ჩანაწერებს ვამოწმებთ და არც Json-დან შემოგვაქვს
        if (CheckRecordsExists())
            //Messages.Add($"Records in {_tableName} is already exists");
            //return (false, Messages);
            return new Err[]
            {
                new()
                {
                    ErrorCode = "RecordsIsAlreadyExists",
                    ErrorMessage = $"Records in {_tableName} is already exists"
                }
            };

        //ლოგიკა ასეთია: ჯერ მოისინჯება Json ფაილის გამოყენება
        var result = CreateByJsonFile();
        //თუ რამე შეცდომა მოხდა ამ დროს, აქედანაც დაბრუნდება შეცდომა
        if (result.IsSome)
            return (Err[])result;

        //აქ დამატებით ვუშვებ მონაცემების შემოწმებას და თუ რომელიმე აუცილებელი ჩანაწერი აკლია, რაც ლოგიკით განისაზღვრება,
        //მაშინ ისინიც ჩაემატება. ან თუ არასწორად არის რომელიმე ჩანაწერი, შეიცვლება. ან თუ ზედმეტია წაიშლება
        return AdditionalCheck();
    }


    protected List<T> LoadFromJsonFile<T>(string folderName = null, string fileName = null)
    {
        var folName = folderName ?? _dataSeedFolder;
        if (folName == null)
            return [];
        var jsonFullFileName = Path.Combine(folName, fileName ?? $"{_tableName.Capitalize()}.json");
        return File.Exists(jsonFullFileName) ? FileLoader.LoadDeserializeResolve<List<T>>(jsonFullFileName, true) : [];
    }

    private bool CheckRecordsExists()
    {
        //აქ true ბრუნდება იმ მიზნით, რომ თუ ეს მეთოდი კლასში არ გადააწერეს, ითვლებოდეს, რომ ჩანაწერები არსებობს
        //return true;
        return Repo.HaveAnyRecord<TDst>();
    }

    protected virtual Option<IEnumerable<Err>> CreateByJsonFile()
    {
        return null;
    }

    protected virtual Option<IEnumerable<Err>> AdditionalCheck()
    {
        return null;
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