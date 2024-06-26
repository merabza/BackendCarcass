﻿using SystemToolsShared.Errors;

namespace CarcassContracts.Errors;

public static class CarcassMasterDataDomErrors
{
    public static Err MustBeInteger(string fieldName, string? caption, string? defErrorCode, string? defErrorMessage)
    {
        return new Err
        {
            ErrorCode = defErrorCode ?? $"{fieldName}{nameof(MustBeInteger)}",
            ErrorMessage = defErrorMessage ?? $"{caption} მთელი უნდა იყოს"
        };
    }

    public static Err MustBePositive(string fieldName, string? caption, string? defErrorCode,
        string? defErrorMessage)
    {
        return new Err
        {
            ErrorCode = defErrorCode ?? $"{fieldName}{nameof(MustBePositive)}",
            ErrorMessage = defErrorMessage ?? $"{caption} უნდა იყოს დადებითი რიცხვი"
        };
    }

    public static Err Required(string fieldName, string? caption, string? defErrorCode, string? defErrorMessage)
    {
        return new Err
        {
            ErrorCode = defErrorCode ?? $"{fieldName}{nameof(Required)}",
            ErrorMessage = defErrorMessage ?? $"{caption} შევსებული უნდა იყოს"
        };
    }

    public static Err MustBeBoolean(string fieldName, string? caption, string typeName)
    {
        return new Err
        {
            ErrorCode = $"{fieldName}{nameof(MustBeBoolean)}",
            ErrorMessage = $"{caption} ველი უნდა იყოს {typeName} ტიპის"
        };
    }

    public static Err IsEmpty(string fieldName, string? caption)
    {
        return new Err { ErrorCode = $"{fieldName}{nameof(IsEmpty)}", ErrorMessage = $"{caption} შევსებული არ არის" };
    }

    public static Err IsTooLong(string fieldName, string? caption)
    {
        return new Err { ErrorCode = $"{fieldName}{nameof(IsTooLong)}", ErrorMessage = $"{caption} ძალიან გრძელია" };
    }
}