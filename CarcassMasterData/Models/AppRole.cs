using Microsoft.AspNetCore.Identity;

namespace CarcassMasterData.Models;

public sealed class AppRole : IdentityRole<int>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public AppRole(string name, string roleName, int level) : base(name)
    {
        RoleName = roleName;
        Level = level;
    }

    public string RoleName { get; set; }
    public int Level { get; set; }
}
