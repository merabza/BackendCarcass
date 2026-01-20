using System.ComponentModel.DataAnnotations.Schema;
using BackendCarcass.MasterData;

namespace BackendCarcass.Database.Models;

//მომხმარებელი
public sealed class User : IDataType
{
    public int UsrId { get; set; }

    public required string UserName { get; set; }
    public required string NormalizedUserName { get; set; }
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public required string PasswordHash { get; set; }
    public required string FullName { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    [NotMapped] public static string DtKeyKey => "user";

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
        if (data is not User newData) return false;

        UserName = newData.UserName;
        Email = newData.Email;
        FirstName = newData.FirstName;
        LastName = newData.LastName;
        return true;
    }

    public dynamic EditFields()
    {
        return new
        {
            UsrId,
            UserName,
            Email,
            FirstName,
            LastName
        };
    }
}