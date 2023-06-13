using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;
using CarcassMasterDataDom.Models;
using Microsoft.AspNetCore.Identity;

namespace CarcassDataSeeding.Seeders;

public /*open*/ class RolesSeeder : AdvancedDataSeeder<Role>
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly string _secretDataFolder;

    public RolesSeeder(RoleManager<AppRole> roleManager, string secretDataFolder, string dataSeedFolder,
        IDataSeederRepository repo) : base(dataSeedFolder, repo)
    {
        _secretDataFolder = secretDataFolder;
        _roleManager = roleManager;
    }

    protected override bool CreateByJsonFile()
    {
        if (!Repo.CreateEntities(CreateListBySeedData(LoadFromJsonFile<RoleSeederModel>())))
            return false;
        DataSeederTempData.Instance.SaveIntIdKeys<Role>(Repo.GetAll<Role>().ToDictionary(k => k.RolKey, v => v.RolId));
        return true;
    }

    protected override bool AdditionalCheck()
    {
        List<Role> existingRoles = Repo.GetAll<Role>();

        if (!GetRoleModels()
                .Select(roleModel => new
                    { roleModel, existingRole = existingRoles.SingleOrDefault(sd => sd.RolKey == roleModel.RoleKey) })
                .Where(w => w.existingRole is null && w.roleModel is not null).Select(s => s!.roleModel)
                .All(CreateRole))
            return false;
        DataSeederTempData.Instance.SaveIntIdKeys<Role>(Repo.GetAll<Role>().ToDictionary(k => k.Key, v => v.Id));
        return true;
    }

    private List<Role> CreateListBySeedData(List<RoleSeederModel> rolesSeedData)
    {
        return rolesSeedData.Select(s => new Role
            {
                RolKey = s.RolKey, RolName = s.RolName, RolLevel = s.RolLevel, RolNormalizedKey = s.RolNormalizedKey
            })
            .ToList();
    }

    private bool CreateRole(RoleModel roleModel)
    {
        //შევქმნათ როლი
        IdentityResult result = _roleManager
            .CreateAsync(new AppRole(roleModel.RoleKey, roleModel.RoleName, roleModel.Level)).Result;
        if (result.Succeeded)
            return true;
        //თუ ახალი როლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
        Messages.AddRange(result.Errors.Select(s => s.Description));
        Messages.Add($"Role {roleModel.RoleName} can not created.");
        return false;
    }

    private List<RoleModel> GetRoleModels()
    {
        return LoadFromJsonFile<RoleModel>(_secretDataFolder, "Roles.json").ToList();
    }
}