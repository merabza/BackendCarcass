using SystemToolsShared.Errors;

namespace CarcassContracts.Errors;

public class RightsApiErrors
{
    public static readonly Err NoSufficientRights =
        new() { ErrorCode = nameof(NoSufficientRights), ErrorMessage = "თქვენ არ გაქვთ საკმარისი უფლებები" };

    public static readonly Err ErrorWhenDeterminingCrudType =
        new()
        {
            ErrorCode = nameof(ErrorWhenDeterminingCrudType),
            ErrorMessage = "შეცდომა ბაზაში ცვლილების მეთოდის დადგენისას"
        };

    public static readonly Err ErrorWhenDeterminingRights =
        new() { ErrorCode = nameof(ErrorWhenDeterminingRights), ErrorMessage = "შეცდომა უფლებების დადგენისას" };

    public static readonly Err UserNotIdentified =
        new() { ErrorCode = nameof(UserNotIdentified), ErrorMessage = "მომხმარებლის იდენტიფიცირება ვერ მოხერხდა" };

    public static readonly Err TableNameNotIdentified =
        new()
        {
            ErrorCode = nameof(TableNameNotIdentified), ErrorMessage = "ცხრილის სახელის იდენტიფიცირება ვერ მოხერხდა"
        };

    public static readonly Err TableNamesListNotIdentified =
        new()
        {
            ErrorCode = nameof(TableNamesListNotIdentified),
            ErrorMessage = "ცხრილის სახელების სიის იდენტიფიცირება ვერ მოხერხდა"
        };

    public static readonly Err InsufficientRights =
        new() { ErrorCode = nameof(InsufficientRights), ErrorMessage = "არასაკმარისი უფლებები" };
}