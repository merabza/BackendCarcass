using System;
using LibCrud;

namespace CarcassMasterDataDom.Models;

public sealed class UserCrudData : ICrudData, IDataType
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public UserCrudData(string userName, string firstName, string lastName, string email)
    {
        UserName = userName;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    public string UserName { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }

    public int Id { get; set; }

    public string Key => UserName;
    public string Name => UserName;
    public int? ParentId => null;

    public bool UpdateTo(IDataType data)
    {
        throw new NotImplementedException();
    }

    public dynamic EditFields()
    {
        return new { UserName, FirstName, LastName, Email };
    }
}