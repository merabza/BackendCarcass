using System;
using System.Collections.Generic;
using System.Linq;
using BackendCarcass.Database.Models;
using BackendCarcass.DataSeeding.Models;
using BackendCarcass.MasterData.Models;
using Microsoft.AspNetCore.Identity;
using SystemTools.DatabaseToolsShared;
using SystemTools.DomainShared.Repositories;

namespace BackendCarcass.DataSeeding.Seeders;

public /*open*/
    class RolesSeeder : DataSeeder<Role, RoleSeederModel>
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly string _secretDataFolder;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RolesSeeder(RoleManager<AppRole> roleManager, string secretDataFolder, string dataSeedFolder,
        IDataSeederRepository repo, IUnitOfWork unitOfWork, ESeedDataType seedDataType = ESeedDataType.OnlyJson,
        List<string>? keyFieldNamesList = null) : base(dataSeedFolder, repo, unitOfWork, seedDataType,
        keyFieldNamesList)
    {
        _roleManager = roleManager;
        _secretDataFolder = secretDataFolder;
    }

    public override bool AdditionalCheck(List<RoleSeederModel> jsonData, List<Role> savedData)
    {
        DataSeederTempData.Instance.SaveIntIdKeys<Role>(savedData.ToDictionary(k => k.RolKey, v => v.RolId));

        List<Role> existingRoles = DataSeederRepo.GetAll<Role>();

        List<RoleModel> rolesToCreate = GetRoleModels()
            .Select(roleModel => new
            {
                roleModel, existingRole = existingRoles.SingleOrDefault(sd => sd.RolKey == roleModel.RoleKey)
            }).Where(w => w.existingRole is null).Select(s => s.roleModel).ToList();

        //თუ არ არსებობს ადმინის როლი, შევქმნათ იგი
        if (!(rolesToCreate.Any(x => x.Level <= 10) || rolesToCreate.Any(x =>
                x.RoleKey.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
                x.RoleKey.Equals("administrator", StringComparison.OrdinalIgnoreCase))))
        {
            rolesToCreate.Add(new RoleModel { Level = 10, RoleKey = "admin", RoleName = "ადმინისტრატორი" });
        }

        if (rolesToCreate.Any(roleModel => !CreateRole(roleModel)))
        {
            return false;
        }

        DataSeederTempData.Instance.SaveIntIdKeys<Role>(DataSeederRepo.GetAll<Role>()
            .ToDictionary(k => k.Key, v => v.Id));
        return true;
    }

    public override List<Role> Adapt(List<RoleSeederModel> rolesSeedData)
    {
        return rolesSeedData.Select(s => new Role
        {
            RolKey = s.RolKey, RolName = s.RolName, RolLevel = s.RolLevel, RolNormalizedKey = s.RolNormalizedKey
        }).ToList();
    }

    private bool CreateRole(RoleModel roleModel)
    {
        //შევქმნათ როლი
        IdentityResult result = _roleManager
            .CreateAsync(new AppRole(roleModel.RoleKey, roleModel.RoleName, roleModel.Level)).Result;
        return result.Succeeded ? true : throw new Exception($"Role {roleModel.RoleName} can not be created.");
    }

    private List<RoleModel> GetRoleModels()
    {
        return LoadFromJsonFile<RoleModel>(_secretDataFolder, "Roles.json").ToList();
    }
}
