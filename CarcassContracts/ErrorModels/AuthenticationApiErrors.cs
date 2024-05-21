using SystemToolsShared;
using SystemToolsShared.ErrorModels;

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
    public static string IsEmptyEmailErrMessage => ApiErrors.IsEmptyErrMessage("ელექტრონული ფოსტის მისამართი");
    public static string IsEmptyFirstNameErrMessage => ApiErrors.IsEmptyErrMessage("სახელი");
    public static string IsEmptyLastNameErrMessage => ApiErrors.IsEmptyErrMessage("გვარი");
    public static string IsEmptyUserNameErrMessage => ApiErrors.IsEmptyErrMessage("მომხმარებლის სახელი");
    public static string IsEmptyPasswordErrMessage => ApiErrors.IsEmptyErrMessage("პაროლი");
    public static string IsEmptyOldPasswordErrMessage => ApiErrors.IsEmptyErrMessage("ძველი პაროლი");
    public static string IsEmptyNewPasswordErrMessage => ApiErrors.IsEmptyErrMessage("ახალი პაროლი");
    public static string InvalidEmailAddressErrCode => "InvalidEmailAddress";
    public static string InvalidEmailAddressErrMessage => "ელექტრონული ფოსტის მისამართი არასწორია";
    public static string NameIsLongerThenErrMessage => ApiErrors.IsEmptyErrMessage("სახელის");
    public static string LastNameIsLongerThenErrMessage => ApiErrors.IsEmptyErrMessage("გვარის");
    public static string PasswordsDoNotMatchErrCode => "PasswordsDoNotMatch";
    public static string PasswordsDoNotMatchErrMessage => "პაროლები ერთმანეთს არ ემთხვევა";
}