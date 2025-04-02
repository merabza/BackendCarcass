using System.ComponentModel.DataAnnotations.Schema;
using CarcassMasterDataDom;

namespace CarcassDb.Models;

//მომხმარებელი
public sealed class User : IDataType
{
    public int UsrId { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string UserName { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string NormalizedUserName { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string Email { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string NormalizedEmail { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string PasswordHash { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string FullName { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string FirstName { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string LastName { get; set; }

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