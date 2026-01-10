using System;
using System.ComponentModel.DataAnnotations.Schema;
using CarcassDb;
using CarcassMasterData;

namespace Carcass.Database.Models;

//მენიუს ელემენტი
public sealed class MenuItm : IDataType, IMyEquatable
{
    public int MenId { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string MenKey { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string MenName { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? MenValue { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string MenLinkKey { get; set; }
    public int MenGroupId { get; set; }
    public int SortId { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? MenIconName { get; set; }

    public MenuGroup MenGroupNavigation
    {
        get => field ?? throw new InvalidOperationException("Uninitialized property: " + nameof(MenGroupNavigation));
        set;
    }

    //public static string DtKeyKey => nameof(MenId).CountDtKey();

    [NotMapped]
    public int Id
    {
        get => MenId;
        set => MenId = value;
    }

    [NotMapped] public string Key => MenKey;

    [NotMapped] public string Name => MenName;

    [NotMapped] public int? ParentId => MenGroupId;

    public bool UpdateTo(IDataType data)
    {
        if (data is not MenuItm newData)
            return false;
        MenKey = newData.MenKey;
        MenLinkKey = newData.MenLinkKey;
        MenName = newData.MenName;
        MenValue = newData.MenValue;
        MenGroupId = newData.MenGroupId;
        SortId = newData.SortId;
        MenIconName = newData.MenIconName;
        return true;
    }

    public dynamic EditFields()
    {
        return new
        {
            MenId,
            MenKey,
            MenLinkKey,
            MenName,
            MenValue,
            MenGroupId,
            SortId,
            MenIconName
        };
    }

    public bool EqualsTo(IDataType data)
    {
        if (data is not MenuItm other)
            return false;

        return MenKey == other.MenKey && MenName == other.MenName && Equals(MenValue, other.MenValue) &&
               MenLinkKey == other.MenLinkKey && MenGroupId == other.MenGroupId && SortId == other.SortId &&
               Equals(MenIconName, other.MenIconName);
    }
}