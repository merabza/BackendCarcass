using System;
using LibCrud;

namespace CarcassMasterDataDom.Models;

public sealed class RoleCrudData : ICrudData, IDataType
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public RoleCrudData(string rolKey, string rolName, int rolLevel)
    {
        RolKey = rolKey;
        RolName = rolName;
        RolLevel = rolLevel;
    }

    public string RolKey { get; init; }
    public string RolName { get; init; }
    public int RolLevel { get; init; }

    public int Id { get; set; }

    public string Key => RolKey;
    public string Name => RolName;
    public int? ParentId => null;

    public bool UpdateTo(IDataType data)
    {
        throw new NotImplementedException();
    }

    public dynamic EditFields()
    {
        return new { Id, RolKey, RolName, RolLevel };
    }
}