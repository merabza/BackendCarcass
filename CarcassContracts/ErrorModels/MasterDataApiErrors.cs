using SystemToolsShared;

namespace CarcassContracts.ErrorModels;

public static class MasterDataApiErrors
{
    public static Err EntryNotFound = new()
        { ErrorCode = nameof(EntryNotFound), ErrorMessage = "ჩანაწერის პოვნა ვერ მოხერხდა" };

    public static Err NoRightsForCreate = new()
        { ErrorCode = nameof(NoRightsForCreate), ErrorMessage = "თქვენ არ გაქვთ უფლება შექმნათ ჩანაწერი ამ ცხრილში" };

    public static Err NoRightsForUpdate = new()
        { ErrorCode = nameof(NoRightsForUpdate), ErrorMessage = "თქვენ არ გაქვთ უფლება შეცვალოთ ჩანაწერი ამ ცხრილში" };

    public static Err NoRightsForDelete = new()
        { ErrorCode = nameof(NoRightsForDelete), ErrorMessage = "თქვენ არ გაქვთ უფლება წაშალოთ ჩანაწერი ამ ცხრილში" };

    public static Err CannotCreateNewRecord = new()
        { ErrorCode = nameof(CannotCreateNewRecord), ErrorMessage = "ახალი ჩანაწერის შექმნა ვერ მოხერხდა" };

    public static Err CannotLoad = new()
        { ErrorCode = nameof(CannotLoad), ErrorMessage = "მონაცემთა ბაზიდან ინფორმაციის ჩატვირთვა ვერ მოხერხდა" };

    public static Err CannotFindUser = new()
        { ErrorCode = nameof(CannotFindUser), ErrorMessage = "მომხმარებელი ვერ მოიძებნა" };

    public static Err CannotFindRole = new()
        { ErrorCode = nameof(CannotFindRole), ErrorMessage = "როლი ვერ მოიძებნა" };

    public static Err CannotUpdateNewRecord = new()
        { ErrorCode = nameof(CannotUpdateNewRecord), ErrorMessage = "ჩანაწერის შეცვლა ვერ მოხერხდა" };

    public static Err CannotDeleteNewRecord = new()
        { ErrorCode = nameof(CannotDeleteNewRecord), ErrorMessage = "ჩანაწერის წაშლა ვერ მოხერხდა" };

    public static Err TableNotFound(string tableName)
    {
        var err = new Err
        {
            ErrorCode = $"{tableName}{nameof(TableNotFound)}",
            ErrorMessage = $"ცხრილი სახელით {tableName} არ არსებობს"
        };
        return err;
    }

    public static Err TableHaveNotSingleKey(string tableName)
    {
        var err = new Err
        {
            ErrorCode = $"{tableName}{nameof(TableHaveNotSingleKey)}",
            ErrorMessage = $"ცხრილს სახელით {tableName} არ აქვს ერთადერთი გასაღები"
        };
        return err;
    }

    public static Err TableSingleKeyMustHaveOneProperty(string tableName)
    {
        var err = new Err
        {
            ErrorCode = $"{tableName}{nameof(TableSingleKeyMustHaveOneProperty)}",
            ErrorMessage = $"ცხრილს სახელით {tableName} ერთადერთ გასაღებში არ აქვს ზუსტად ერთი ველი"
        };
        return err;
    }

    public static Err SetMethodNotFoundForTable(string tableName)
    {
        return new Err
        {
            ErrorCode = $"{nameof(SetMethodNotFoundForTable)}{tableName}",
            ErrorMessage = $"ცხრილს სახელით {tableName} არ აქვს მეთოდი Set"
        };
    }

    public static Err SetMethodReturnsNullForTable(string tableName)
    {
        return new Err
        {
            ErrorCode = $"{nameof(SetMethodReturnsNullForTable)}{tableName}",
            ErrorMessage = $"{tableName} ცხრილის Set მეთოდი აბრუნებს null-ს"
        };
    }

    public static Err RecordDoesNotDeserialized(string tableName)
    {
        return new Err
        {
            ErrorCode = $"{tableName}{nameof(RecordDoesNotDeserialized)}",
            ErrorMessage = $"მიღებული ჩანაწერის  გაშიფვრა ვერ მოხერხდა {tableName} ცხრილის სტრუქტურის მიხედვით"
        };
    }

    public static Err WrongId(string tableName)
    {
        return new Err
        {
            ErrorCode = $"{tableName}{nameof(WrongId)}",
            ErrorMessage =
                $"{tableName} ცხრილისთვის მოწოდებული ინფორმაცია არასწორია, რადგან იდენტიფიკატორი არ ემთხვევა მოწოდებული ობიექტის იდენტიფიკატორს"
        };
    }

    public static Err LoaderForTableNotFound(string tableName)
    {
        return new Err
        {
            ErrorCode = "LoaderForTableNotFound",
            ErrorMessage = $"ჩამტვირთავი ცხრილისთვის სახელით {tableName} ვერ მოიძებნა"
        };
    }

    public static Err RecordNotFound(string tableName, int id)
    {
        return new Err
        {
            ErrorCode = $"{nameof(RecordNotFound)}{tableName}{id}",
            ErrorMessage = $"ბაზაში {tableName} ცხრილში {id} იდენტიფიკატორის შესაბამისი ჩანაწერი არ არის ნაპოვნი"
        };
    }


    ////ბაზაში ვერ ვიპოვეთ მოწოდებული იდენტიფიკატორის შესაბამისი ჩანაწერი.

    public static Err MasterDataTableNotFound(string tableName)
    {
        return new Err
            { ErrorCode = nameof(MasterDataTableNotFound), ErrorMessage = $"მონაცემთა ტიპი {tableName} ვერ მოიძებნა" };
    }

    public static Err MasterDataInvalidValidationRules(string tableName)
    {
        return new Err
        {
            ErrorCode = nameof(MasterDataTableNotFound),
            ErrorMessage = $"მონაცემთა ტიპი {tableName} შეიცავს ვალიდაციის არასწორ წესებს"
        };
    }

    public static Err MasterDataFieldNotFound(string tableName, string fieldName)
    {
        return new Err
        {
            ErrorCode = nameof(MasterDataFieldNotFound),
            ErrorMessage = $"მონაცემთა ტიპის {tableName} ველი {fieldName} შემოწმებისას ვერ მოიძებნა"
        };
    }
}