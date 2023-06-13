using System.ComponentModel.DataAnnotations.Schema;
using CarcassMasterDataDom;

namespace CarcassDb.Models;

//მომხმარებელი
public sealed class User : IDataType
{
    //[NotMapped] public const string DKey = "usr";

    public int UsrId { get; set; }
    public string UserName { get; set; } = null!;
    public string NormalizedUserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string NormalizedEmail { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    [NotMapped]
    public int Id
    {
        get => UsrId;
        set => UsrId = value;
    }

    [NotMapped] public string Key => NormalizedUserName;

    [NotMapped] public string Name => FullName;

    [NotMapped] public int? ParentId => null;

    public bool UpdateTo(IDataType data)
    {
        if (data is not User newData)
            return false;
        UserName = newData.UserName;
        Email = newData.Email;
        FirstName = newData.FirstName;
        LastName = newData.LastName;
        return true;
    }

    public dynamic EditFields()
    {
        return new { UsrId, UserName, Email, FirstName, LastName };
    }
}