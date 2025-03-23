using System;
using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;
using CarcassMasterDataDom.Models;
using Microsoft.AspNetCore.Identity;

namespace CarcassDataSeeding.Seeders;

public /*open*/
    class RolesSeeder(
        RoleManager<AppRole> roleManager,
        string secretDataFolder,
        string dataSeedFolder,
        IDataSeederRepository repo) : DataSeeder<Role, RoleSeederModel>(dataSeedFolder, repo, ESeedDataType.OnlyRules)
{
    protected override bool AdditionalCheck(List<RoleSeederModel> jMos)
    {
        var dataList = Repo.GetAll<Role>();
        DataSeederTempData.Instance.SaveIntIdKeys<Role>(dataList.ToDictionary(k => k.RolKey, v => v.RolId));

        var existingRoles = Repo.GetAll<Role>();

        var rolesToCreate = GetRoleModels()
            .Select(roleModel => new
            {
                roleModel, existingRole = existingRoles.SingleOrDefault(sd => sd.RolKey == roleModel.RoleKey)
            }).Where(w => w.existingRole is null).Select(s => s.roleModel);

        if (rolesToCreate.Any(roleModel => !CreateRole(roleModel)))
            return false;

        DataSeederTempData.Instance.SaveIntIdKeys<Role>(Repo.GetAll<Role>().ToDictionary(k => k.Key, v => v.Id));
        return true;
    }

    protected override List<Role> Adapt(List<RoleSeederModel> rolesSeedData)
    {
        return rolesSeedData.Select(s => new Role
        {
            RolKey = s.RolKey, RolName = s.RolName, RolLevel = s.RolLevel, RolNormalizedKey = s.RolNormalizedKey
        }).ToList();
    }

    private bool CreateRole(RoleModel roleModel)
    {
        //შევქმნათ როლი
        var result = roleManager.CreateAsync(new AppRole(roleModel.RoleKey, roleModel.RoleName, roleModel.Level))
            .Result;
        if (result.Succeeded)
            return true;

        throw new Exception($"Role {roleModel.RoleName} can not be created.");
    }

    private List<RoleModel> GetRoleModels()
    {
        return LoadFromJsonFile<RoleModel>(secretDataFolder, "Roles.json").ToList();
    }
}