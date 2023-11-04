﻿using System.ComponentModel.DataAnnotations.Schema;
using CarcassMasterDataDom;

namespace CarcassDb.Models;

//როლი
public sealed class Role : IDataType, IMyEquatable
{
    public int RolId { get; set; }
    public string RolKey { get; set; } = null!;
    public string RolName { get; set; } = null!;
    public int RolLevel { get; set; }
    public string RolNormalizedKey { get; set; } = null!;

    [NotMapped]
    public int Id
    {
        get => RolId;
        set => RolId = value;
    }

    [NotMapped] public string Key => RolKey;

    [NotMapped] public string Name => RolName;

    [NotMapped] public int? ParentId => null;

    public bool UpdateTo(IDataType data)
    {
        if (data is not Role newData)
            return false;
        RolKey = newData.RolKey;
        RolName = newData.RolName;
        RolLevel = newData.RolLevel;
        return true;
    }

    public dynamic EditFields()
    {
        return new { RolId, RolKey, RolName, RolLevel };
    }

    public bool EqualsTo(IDataType data)
    {
        if (data is not Role other)
            return false;

        return RolKey == other.RolKey && RolName == other.RolName && RolLevel == other.RolLevel &&
               RolNormalizedKey == other.RolNormalizedKey;
    }

}