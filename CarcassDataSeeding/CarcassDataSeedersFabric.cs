using CarcassDataSeeding.Seeders;
using CarcassMasterDataDom.Models;
using DatabaseToolsShared;
using Microsoft.AspNetCore.Identity;

namespace CarcassDataSeeding;

public /*open*/ class CarcassDataSeedersFabric
{
    private readonly IDataSeederRepository _repo;
    protected readonly ICarcassDataSeederRepository CarcassRepo;
    protected readonly string DataSeedFolder;
    protected readonly RoleManager<AppRole> MyRoleManager;
    protected readonly UserManager<AppUser> MyUserManager;
    protected readonly string SecretDataFolder;

    protected CarcassDataSeedersFabric(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager,
        string secretDataFolder, string dataSeedFolder, ICarcassDataSeederRepository carcassRepo,
        IDataSeederRepository repo)
    {
        SecretDataFolder = secretDataFolder;
        DataSeedFolder = dataSeedFolder;
        CarcassRepo = carcassRepo;
        _repo = repo;
        MyUserManager = userManager;
        MyRoleManager = roleManager;
    }

    public virtual ITableDataSeeder CreateDataTypesSeeder()
    {
        return new DataTypesSeeder(CarcassRepo, DataSeedFolder, _repo);
    }

    public virtual ITableDataSeeder CreateAppClaimsSeeder()
    {
        return new AppClaimsSeeder(DataSeedFolder, _repo);
    }

    public virtual ITableDataSeeder CreateCrudRightTypesSeeder()
    {
        return new CrudRightTypesSeeder(DataSeedFolder, _repo);
    }

    public virtual ITableDataSeeder CreateManyToManyJoinsSeeder()
    {
        return new ManyToManyJoinsSeeder(SecretDataFolder, CarcassRepo, DataSeedFolder, _repo);
    }

    public virtual ITableDataSeeder CreateMenuGroupsSeeder()
    {
        return new MenuGroupsSeeder(DataSeedFolder, _repo);
    }

    public virtual ITableDataSeeder CreateMenuSeeder()
    {
        return new MenuSeeder(DataSeedFolder, _repo);
    }

    public virtual ITableDataSeeder CreateRolesSeeder()
    {
        return new RolesSeeder(MyRoleManager, SecretDataFolder, DataSeedFolder, _repo);
    }

    public virtual ITableDataSeeder CreateUsersSeeder()
    {
        return new UsersSeeder(MyUserManager, SecretDataFolder, DataSeedFolder, _repo);
    }
}