using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CarcassMasterDataDom;

namespace CarcassDb.Models;

//მენიუს ჯგუფი
public sealed class MenuGroup : IDataType, IMyEquatable
{
    public int MengId { get; set; }
    [MaxLength(36)] public required string MengKey { get; set; } = null!;
    [MaxLength(100)] public required string MengName { get; set; } = null!;
    public short SortId { get; set; }
    [MaxLength(50)] public string? MengIconName { get; set; }
    public bool Hidden { get; set; }

    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<MenuItm> Menu { get; set; } = new HashSet<MenuItm>();

    [NotMapped]
    public int Id
    {
        get => MengId;
        set => MengId = value;
    }

    [NotMapped] public string Key => MengKey;

    [NotMapped] public string Name => MengName;

    [NotMapped] public int? ParentId => null;

    public bool UpdateTo(IDataType data)
    {
        if (data is not MenuGroup newData)
            return false;
        MengKey = newData.MengKey;
        MengName = newData.MengName;
        SortId = newData.SortId;
        MengIconName = newData.MengIconName;
        Hidden = newData.Hidden;
        return true;
    }

    public dynamic EditFields()
    {
        return new
        {
            MengId,
            MengKey,
            MengName,
            SortId,
            MengIconName,
            Hidden
        };
    }

    public bool EqualsTo(IDataType data)
    {
        if (data is not MenuGroup other)
            return false;

        return MengKey == other.MengKey && MengName == other.MengName && SortId == other.SortId &&
               Equals(MengIconName, other.MengIconName) && Hidden == other.Hidden;
    }
}