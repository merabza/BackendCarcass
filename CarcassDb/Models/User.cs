using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CarcassMasterDataDom;

namespace CarcassDb.Models;

//მომხმარებელი
public sealed class User : IDataType
{
    public int UsrId { get; set; }

    [MaxLength(256)]
    public required string UserName { get; set; }
    [MaxLength(256)]
    public required string NormalizedUserName { get; set; }
    [MaxLength(256)]
    public required string Email { get; set; }
    [MaxLength(256)]
    public required string NormalizedEmail { get; set; }
    [MaxLength(256)]
    public required string PasswordHash { get; set; }
    [MaxLength(100)]
    public required string FullName { get; set; }
    [MaxLength(50)]
    public required string FirstName { get; set; }
    [MaxLength(100)]
    public required string LastName { get; set; }
    [NotMapped]
    public int Id { get => UsrId; set => UsrId = value; }
    [NotMapped] 
    public string Key => NormalizedUserName;
    [NotMapped] 
    public string Name => FullName;
    [NotMapped] 
    public int? ParentId => null;

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