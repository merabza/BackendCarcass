using SystemToolsShared.Errors;

namespace CarcassContracts.Errors;

public static class DataTypesApiErrors
{
    public static readonly Err NoGridNamesInUriQuery =
        new() { ErrorCode = nameof(NoGridNamesInUriQuery), ErrorMessage = "no grid names in uri query" };

    public static Err GridNotFound(string tableName)
    {
        return new Err { ErrorCode = nameof(GridNotFound), ErrorMessage = $"Grid with key {tableName} not found" };
    }

    //                return Results.BadRequest($"Grid with key {tableName} not found");
}