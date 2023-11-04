using System.ComponentModel.DataAnnotations.Schema;
using CarcassMasterDataDom;

namespace CarcassDb.Models;

public sealed class AppClaim : IDataType, IMyEquatable
{
    public int AclId { get; set; } //იდენტიფიკატორი
    public string AclKey { get; set; } = null!; //კოდი
    public string AclName { get; set; } = null!; //სახელი

    [NotMapped]
    public int Id
    {
        get => AclId;
        set => AclId = value;
    }

    [NotMapped] public string Key => AclKey;

    [NotMapped] public string Name => AclName;

    [NotMapped] public int? ParentId => null;

    public bool UpdateTo(IDataType data)
    {
        if (data is not AppClaim newData)
            return false;
        AclKey = newData.AclKey;
        AclName = newData.AclName;
        return true;
    }

    public dynamic EditFields()
    {
        return new { AclId, AclKey, AclName };
    }

    public bool EqualsTo(IDataType data)
    {
        if (data is not AppClaim other)
            return false;

        return AclKey == other.AclKey && AclName == other.AclName;
    }
}