using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BackendCarcass.Application.MasterData.Validation;
using BackendCarcassShared.Contracts.Errors;
using Newtonsoft.Json;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.MasterData.CellModels;

public sealed class StringCell : MixedCell
{
    public StringCell(string fieldName, string? caption, bool visible = true, string? typeName = null) : base(fieldName,
        caption, visible, typeName ?? CellTypeNameForSave(nameof(StringCell)))
    {
    }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Def { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public IntRule? MaxLenRule { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public IntRule? MinLenRule { get; set; }

    //რეგულარული გამოსახულება (JavaScript-თანაც თავსებადი), რომელსაც მთელი მნიშვნელობა უნდა შეესაბამებოდეს
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public StringRule? PatternRule { get; set; }

    public static new StringCell Create(string fieldName, string? caption, bool visible = true, string? typeName = null)
    {
        return new StringCell(fieldName, caption, visible, typeName);
    }

    public StringCell Default(string defaultValue = "")
    {
        Def = defaultValue;
        return this;
    }

    public new StringCell Required(string? errorCode = null, string? errorMessage = null)
    {
        base.Required(errorCode, errorMessage);
        return this;
    }

    public StringCell Max(int maxLen, string? errorCode = null, string? errorMessage = null)
    {
        MaxLenRule = new IntRule(maxLen, errorCode ?? $"{FieldName}TooLong",
            errorMessage ?? $"{Caption} ძალიან გრძელია");
        return this;
    }

    public StringCell Min(int minLen, string? errorCode = null, string? errorMessage = null)
    {
        MinLenRule = new IntRule(minLen, errorCode ?? $"{FieldName}TooShort",
            errorMessage ?? $"{Caption} ძალიან მოკლეა");
        return this;
    }

    public StringCell Pattern(string pattern, string? errorCode = null, string? errorMessage = null)
    {
        PatternRule = new StringRule(pattern, errorCode ?? $"{FieldName}WrongFormat",
            errorMessage ?? $"{Caption} არასწორი ფორმატისაა");
        return this;
    }

    public new StringCell Nullable(bool isNullable = true)
    {
        base.Nullable(isNullable);
        return this;
    }

    public override List<Error> Validate(object? value)
    {
        List<Error> errMes = ValidateByType<string>(base.Validate(value), value, "სტრიქონის");

        if (value is not string strValue)
        {
            return errMes;
        }

        if (IsRequiredErr is not null && string.IsNullOrEmpty(strValue))
        {
            errMes.Add(CarcassMasterDataErrors.IsEmpty(FieldName, Caption));
        }

        if (MaxLenRule is not null && strValue.Length > MaxLenRule.Val)
        {
            errMes.Add(CarcassMasterDataErrors.IsTooLong(FieldName, Caption));
        }

        //ცარიელ მნიშვნელობას მინიმალური სიგრძე და ფორმატი არ ამოწმებს: მის დასაშვებობას Required წყვეტს
        if (strValue.Length == 0)
        {
            return errMes;
        }

        if (MinLenRule is not null && strValue.Length < MinLenRule.Val)
        {
            errMes.Add(MinLenRule.Error);
        }

        if (PatternRule is not null && !Regex.IsMatch(strValue, PatternRule.Val, RegexOptions.CultureInvariant,
                TimeSpan.FromSeconds(1)))
        {
            errMes.Add(PatternRule.Error);
        }

        return errMes;
    }
}
