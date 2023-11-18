using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;
using CarcassMasterDataDom.Models;
using LanguageExt;
using Microsoft.AspNetCore.Identity;
using SystemToolsShared;

namespace CarcassDataSeeding.Seeders;

public /*open*/
    class RolesSeeder(RoleManager<AppRole> roleManager, string secretDataFolder, string dataSeedFolder,
        IDataSeederRepository repo) : AdvancedDataSeeder<Role>(dataSeedFolder, repo)
{
    protected override Option<Err[]> CreateByJsonFile()
    {
        if (!Repo.CreateEntities(CreateListBySeedData(LoadFromJsonFile<RoleSeederModel>())))
            return new Err[]
            {
                new() { ErrorCode = "RoleEntitiesCannotBeCreated", ErrorMessage = "Role entities cannot be created" }
            };
        DataSeederTempData.Instance.SaveIntIdKeys<Role>(Repo.GetAll<Role>().ToDictionary(k => k.RolKey, v => v.RolId));
        return null;
    }

    protected override Option<Err[]> AdditionalCheck()
    {
        var existingRoles = Repo.GetAll<Role>();

        var rolesToCreate = GetRoleModels()
            .Select(roleModel => new
                { roleModel, existingRole = existingRoles.SingleOrDefault(sd => sd.RolKey == roleModel.RoleKey) })
            .Where(w => w.existingRole is null && w.roleModel is not null).Select(s => s!.roleModel);

        var roleCreateErrors = new List<Err>();
        foreach (var roleModel in rolesToCreate)
        {
            var result = CreateRole(roleModel);
            if (result.IsSome)
            {
                roleCreateErrors.AddRange((Err[])result);
            }
        }

        if (roleCreateErrors.Count > 0)
            return roleCreateErrors.ToArray();

        DataSeederTempData.Instance.SaveIntIdKeys<Role>(Repo.GetAll<Role>().ToDictionary(k => k.Key, v => v.Id));
        return null;
    }

    private static List<Role> CreateListBySeedData(List<RoleSeederModel> rolesSeedData)
    {
        return rolesSeedData.Select(s => new Role
            {
                RolKey = s.RolKey, RolName = s.RolName, RolLevel = s.RolLevel, RolNormalizedKey = s.RolNormalizedKey
            })
            .ToList();
    }

    private Option<Err[]> CreateRole(RoleModel roleModel)
    {
        //შევქმნათ როლი
        var result = roleManager
            .CreateAsync(new AppRole(roleModel.RoleKey, roleModel.RoleName, roleModel.Level)).Result;
        if (result.Succeeded)
            return null;
        //თუ ახალი როლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
        var errors = result.Errors.Select(s => new Err { ErrorCode = s.Code, ErrorMessage = s.Description }).ToList();
        errors.Add(new Err
        {
            ErrorCode = "RoleCanNotBeCreated",
            ErrorMessage = $"Role {roleModel.RoleName} can not be created."
        });
        return errors.ToArray();
    }

    private List<RoleModel> GetRoleModels()
    {
        return LoadFromJsonFile<RoleModel>(secretDataFolder, "Roles.json").ToList();
    }
}