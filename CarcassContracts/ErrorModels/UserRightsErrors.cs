using SystemToolsShared;

namespace CarcassContracts.ErrorModels;

public static class UserRightsErrors
{
    public static readonly Err UserNotIdentifierSaveFiled = new()
    {
        ErrorCode = nameof(UserNotIdentifierSaveFiled),
        ErrorMessage = "ვერ მოხერხდა მომხმარებლის იდენტიფიკაცია. მომხმარებლის ინფორმაციის შენახვა ვერ მოხერხდა"
    };

    public static readonly Err UserAuthenticationFailedThePasswordHasNotBeenChanged = new()
    {
        ErrorCode = nameof(UserAuthenticationFailedThePasswordHasNotBeenChanged),
        ErrorMessage = "ვერ მოხერხდა მომხმარებლის იდენტიფიკაცია. პაროლი არ შეიცვალა"
    };

    public static readonly Err FailedToSaveUserInformation = new()
    {
        ErrorCode = nameof(FailedToSaveUserInformation),
        ErrorMessage = "მომხმარებლის ინფორმაციის შენახვა ვერ მოხერხდა"
    };

    public static readonly Err FailedToChangePassword = new()
    {
        ErrorCode = nameof(FailedToChangePassword),
        ErrorMessage = "პაროლის შეცვლა ვერ მოხერხდა"
    };

    public static readonly Err BadRequestFailedToDeleteUser = new()
    {
        ErrorCode = nameof(BadRequestFailedToDeleteUser),
        ErrorMessage = "არასწორი მოთხოვნა, მომხმარებლის წაშლა ვერ მოხერხდა"
    };

    public static readonly Err NoUserFound = new()
    {
        ErrorCode = nameof(NoUserFound),
        ErrorMessage = "მომხმარებელი არ მოიძებნა"
    };

    public static readonly Err DeletionErrorUserCouldNotBeDeleted = new()
    {
        ErrorCode = nameof(DeletionErrorUserCouldNotBeDeleted),
        ErrorMessage = "წაშლისას მოხდა შეცდომა, მომხმარებლის წაშლა ვერ მოხერხდა"
    };

    public static readonly Err CouldNotLoadMenu = new()
    {
        ErrorCode = nameof(CouldNotLoadMenu),
        ErrorMessage = "მენიუს ჩატვირთვა ვერ მოხერხდა"
    };
}