using System;
using System.ComponentModel.DataAnnotations.Schema;
using CarcassMasterDataDom;

namespace CarcassDb.Models;

//მენიუს ელემენტი
public sealed class MenuItm : IDataType, IMyEquatable
{
    private MenuGroup? _menGroupNavigation;

    public int MenId { get; set; }
    public string MenKey { get; set; } = null!;
    public string MenName { get; set; } = null!;
    public string? MenValue { get; set; }
    public string MenLinkKey { get; set; } = null!;
    public int MenGroupId { get; set; }
    public int SortId { get; set; }
    public string? MenIconName { get; set; }

    public MenuGroup MenGroupNavigation
    {
        get => _menGroupNavigation ??
               throw new InvalidOperationException("Uninitialized property: " + nameof(MenGroupNavigation));
        set => _menGroupNavigation = value;
    }

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
        return new { MenId, MenKey, MenLinkKey, MenName, MenValue, MenGroupId, SortId, MenIconName };
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