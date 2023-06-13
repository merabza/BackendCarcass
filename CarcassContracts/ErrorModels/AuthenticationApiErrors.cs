using SystemToolsShared;

namespace CarcassContracts.ErrorModels;

public static class AuthenticationApiErrors
{
    public static readonly Err UserAlreadyExists =
        new() { ErrorCode = nameof(UserAlreadyExists), ErrorMessage = "მომხმარებელი ასეთი სახელით უკვე არსებობს" };

    public static readonly Err UsernameOrPasswordIsIncorrect =
        new()
        {
            ErrorCode = nameof(UsernameOrPasswordIsIncorrect),
            ErrorMessage = "მომხმარებლის სახელი, ან პაროლი არასწორია "
        };

    public static readonly Err EmailAlreadyExists =
        new()
        {
            ErrorCode = nameof(EmailAlreadyExists), ErrorMessage = "მომხმარებელი ასეთი ელექტრონული ფოსტით უკვე არსებობს"
        };

    public static readonly Err MoreComplexPasswordIsRequired =
        new()
        {
            ErrorCode = nameof(MoreComplexPasswordIsRequired),
            ErrorMessage = "პაროლის გამოყენება ვერ მოხერხდა, საჭიროა უფრო რთული პაროლი"
        };

    public static readonly Err CouldNotCreateNewUser =
        new() { ErrorCode = nameof(CouldNotCreateNewUser), ErrorMessage = "ახალი მომხმარებლის შექმნა ვერ მოხერხდა" };

    public static readonly Err InvalidUsername =
        new() { ErrorCode = nameof(InvalidUsername), ErrorMessage = "არასწორი მომხმარებლის სახელი" };

    public static readonly Err InvalidEmail =
        new() { ErrorCode = nameof(InvalidEmail), ErrorMessage = "არასწორი ელექტრონული ფოსტის მისამართი" };


    public static readonly Err UserNameIsLongerThenErr = CarcassApiErrors.IsLongerThen("მომხმარებლის სახელის");
    public static string IsEmptyEmailErrMessage => CarcassApiErrors.IsEmptyErrMessage("ელექტრონული ფოსტის მისამართი");
    public static string IsEmptyFirstNameErrMessage => CarcassApiErrors.IsEmptyErrMessage("სახელი");
    public static string IsEmptyLastNameErrMessage => CarcassApiErrors.IsEmptyErrMessage("გვარი");
    public static string IsEmptyUserNameErrMessage => CarcassApiErrors.IsEmptyErrMessage("მომხმარებლის სახელი");
    public static string IsEmptyPasswordErrMessage => CarcassApiErrors.IsEmptyErrMessage("პაროლი");
    public static string IsEmptyOldPasswordErrMessage => CarcassApiErrors.IsEmptyErrMessage("ძველი პაროლი");
    public static string IsEmptyNewPasswordErrMessage => CarcassApiErrors.IsEmptyErrMessage("ახალი პაროლი");
    public static string InvalidEmailAddressErrCode => "InvalidEmailAddress";
    public static string InvalidEmailAddressErrMessage => "ელექტრონული ფოსტის მისამართი არასწორია";
    public static string NameIsLongerThenErrMessage => CarcassApiErrors.IsEmptyErrMessage("სახელის");
    public static string LastNameIsLongerThenErrMessage => CarcassApiErrors.IsEmptyErrMessage("გვარის");
    public static string PasswordsDoNotMatchErrCode => "PasswordsDoNotMatch";
    public static string PasswordsDoNotMatchErrMessage => "პაროლები ერთმანეთს არ ემთხვევა";
}