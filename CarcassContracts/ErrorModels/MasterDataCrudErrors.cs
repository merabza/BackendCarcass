using System;
using SystemToolsShared;

namespace CarcassContracts.ErrorModels;

public static class MasterDataCrudErrors
{
    public static Err GridModelIsNull(string tableName)
    {
        return new Err
            { ErrorCode = nameof(GridModelIsNull), ErrorMessage = $"gridModel is null for Table {tableName}" };
    }

    public static Err GenericMethodWasNotCreated(string methodName)
    {
        return new Err
        {
            ErrorCode = nameof(GenericMethodWasNotCreated),
            ErrorMessage = $"Generic Method {methodName} was Not Created"
        };
    }

    public static Err MethodResultIsNull(string methodName)
    {
        return new Err { ErrorCode = nameof(MethodResultIsNull), ErrorMessage = $"Method {methodName} Result Is Null" };
    }

    public static Err MethodResultTaskIsNull(string methodName)
    {
        return new Err
            { ErrorCode = nameof(MethodResultTaskIsNull), ErrorMessage = $"Method {methodName} Result Task Is Null" };
    }

    public static Err SortIdHelperWasNotCreatedForType(Type type)
    {
        return new Err
        {
            ErrorCode = nameof(SortIdHelperWasNotCreatedForType),
            ErrorMessage = $"SortIdHelper was not created for type {type.Name}"
        };
    }
}

/*
            return new Err[]
       { new() { ErrorCode = "ISortIdHelperIsNull", ErrorMessage = "ISortIdHelper Is Null" } };
 */