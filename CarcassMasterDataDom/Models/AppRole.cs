using Microsoft.AspNetCore.Identity;

namespace CarcassMasterDataDom.Models;

public sealed class AppRole : IdentityRole<int>
{
    public AppRole(string name, string roleName, int level) : base(name)
    {
        RoleName = roleName;
        Level = level;
    }

    public string RoleName { get; set; }
    public int Level { get; set; }
}