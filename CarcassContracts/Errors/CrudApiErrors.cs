using SystemToolsShared;

namespace CarcassContracts.Errors;

public static class CrudApiErrors
{
    public static readonly Err WeCouldNotFindARecordToEditInTheDatabase = new()
    {
        ErrorCode = nameof(WeCouldNotFindARecordToEditInTheDatabase),
        ErrorMessage = "ბაზაში ვერ ვიპოვეთ დასარედაქტირებელი ჩანაწერი"
    };

    public static readonly Err CouldNotCreateNewRecord = new()
        { ErrorCode = nameof(CouldNotCreateNewRecord), ErrorMessage = "ახალი ჩანაწერის შექმნა ვერ მოხერხდა" };

    public static readonly Err NoRecordToDeleteFound = new()
    {
        ErrorCode = nameof(NoRecordToDeleteFound),
        ErrorMessage = "წასაშლელი ჩანაწერი ვერ მოიძებნა. წაშლა ვერ მოხერხდა"
    };

    public static readonly Err VirtualMethodDoesNotImplemented =
        new()
        {
            ErrorCode = nameof(VirtualMethodDoesNotImplemented),
            ErrorMessage = "იდენტიფიკატორის მიხედვით ინფორმაციის ჩატვირთვის მეთოდი არ არის იმპლემენტირებული"
        };


    public static readonly Err UploadedInformationCouldNotBeDecrypted = new()
    {
        ErrorCode = nameof(UploadedInformationCouldNotBeDecrypted),
        ErrorMessage = "ატვირთული ინფორმაციის გაშიფვრა ვერ მოხერხდა"
    };


    public static readonly Err WrongIdentifier = new()
    {
        ErrorCode = nameof(WrongIdentifier),
        ErrorMessage = "ატვირთული ინფორმაცია არასწორია. (არასწორი იდენტიფიკატორი.)"
    };
}