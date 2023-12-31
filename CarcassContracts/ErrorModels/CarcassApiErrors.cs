﻿using SystemToolsShared;

namespace CarcassContracts.ErrorModels;

public static class CarcassApiErrors
{
    public static readonly Err RequestIsEmpty = new()
        { ErrorCode = nameof(RequestIsEmpty), ErrorMessage = "ატვირთული ინფორმაცია არასწორია" };

    public static readonly Err ParametersAreInvalid =
        new() { ErrorCode = nameof(ParametersAreInvalid), ErrorMessage = "პარამეტრები არასწორია" };

    public static readonly Err InvalidUser = new()
        { ErrorCode = nameof(InvalidUser), ErrorMessage = "მომხმარებელი არასწორია" };


    public static string IsEmptyErrCode => "{PropertyName}IsEmpty";

    //public static Err IsEmpty(string propertyNameLocalized) => new()
    //    { ErrorCode = IsEmptyErrCode, ErrorMessage = IsEmptyErrMessage(propertyNameLocalized) };


    public static string IsLongerThenErrCode => "{PropertyName}IsLongerThen{MaxLength}";

    public static string IsEmptyErrMessage(string propertyNameLocalized)
    {
        return $"{propertyNameLocalized} შევსებული უნდა იყოს";
    }

    public static string IsLongerThenErrMessage(string propertyNameLocalized)
    {
        return propertyNameLocalized + " სიგრძე არ შეიძლება იყოს {MaxLength} სიმბოლოზე მეტი";
    }

    public static Err IsLongerThen(string propertyNameLocalized)
    {
        return new Err
            { ErrorCode = IsLongerThenErrCode, ErrorMessage = IsLongerThenErrMessage(propertyNameLocalized) };
    }
}