using SystemToolsShared;

namespace CarcassContracts.ErrorModels;

public static class CommonErrors
{
    public static readonly Err IncorrectData =
        new() { ErrorCode = nameof(IncorrectData), ErrorMessage = "არასწორი მონაცემები" };

    public static Err CannotFindMethod(string methodName)
    {
        return new Err { ErrorCode = nameof(CannotFindMethod), ErrorMessage = $"cannot find method {methodName}" };
    }
}