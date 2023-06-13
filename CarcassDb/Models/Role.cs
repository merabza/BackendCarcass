using System.ComponentModel.DataAnnotations.Schema;
using CarcassMasterDataDom;

namespace CarcassDb.Models;

//როლი
public sealed class Role : IDataType, IMyEquatable
{
    //[NotMapped] public const string DKey = "rol";

    //public Role(string rolKey, string rolName, int rolLevel, string rolNormalizedKey)
    //{
    //    RolKey = rolKey;
    //    RolName = rolName;
    //    RolLevel = rolLevel;
    //    RolNormalizedKey = rolNormalizedKey;
    //}

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

    //public bool Equals(Role other)
    //{
    //    if (ReferenceEquals(null, other)) return false;
    //    if (ReferenceEquals(this, other)) return true;
    //    return RolKey == other.RolKey && RolName == other.RolName && RolLevel == other.RolLevel && RolNormalizedKey == other.RolNormalizedKey;
    //}

    //public override bool Equals(object obj)
    //{
    //    if (ReferenceEquals(null, obj)) return false;
    //    if (ReferenceEquals(this, obj)) return true;
    //    if (obj.GetType() != this.GetType()) return false;
    //    return Equals((Role) obj);
    //}

    //public override int GetHashCode()
    //{
    //    unchecked
    //    {
    //        var hashCode = (RolKey != null ? RolKey.GetHashCode() : 0);
    //        hashCode = (hashCode * 397) ^ (RolName != null ? RolName.GetHashCode() : 0);
    //        hashCode = (hashCode * 397) ^ RolLevel;
    //        hashCode = (hashCode * 397) ^ (RolNormalizedKey != null ? RolNormalizedKey.GetHashCode() : 0);
    //        return hashCode;
    //    }
    //}
}