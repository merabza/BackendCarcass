using CarcassDataSeeding.Seeders;
using CarcassMasterDataDom.Models;
using Microsoft.AspNetCore.Identity;

namespace CarcassDataSeeding;

public /*open*/ class DataSeedersFabric
{
    protected readonly string SecretDataFolder;
    protected readonly string DataSeedFolder;
    private readonly IDataSeederRepository _repo;
    private readonly UserManager<AppUser> _myUserManager;
    protected readonly RoleManager<AppRole> MyRoleManager;

    protected DataSeedersFabric(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager,
        string secretDataFolder, string dataSeedFolder, IDataSeederRepository repo)
    {
        SecretDataFolder = secretDataFolder;
        DataSeedFolder = dataSeedFolder;
        _repo = repo;
        _myUserManager = userManager;
        MyRoleManager = roleManager;
    }

    public virtual DataTypesSeeder CreateDataTypesSeeder()
    {
        return new DataTypesSeeder(DataSeedFolder, _repo);
    }

    public virtual AppClaimsSeeder CreateAppClaimsSeeder()
    {
        return new AppClaimsSeeder(DataSeedFolder, _repo);
    }

    public virtual CrudRightTypesSeeder CreateCrudRightTypesSeeder()
    {
        return new CrudRightTypesSeeder(DataSeedFolder, _repo);
    }

    public virtual ManyToManyJoinsSeeder CreateManyToManyJoinsSeeder()
    {
        return new ManyToManyJoinsSeeder(SecretDataFolder, DataSeedFolder, _repo);
    }

    public virtual MenuGroupsSeeder CreateMenuGroupsSeeder()
    {
        return new MenuGroupsSeeder(DataSeedFolder, _repo);
    }

    public virtual MenuSeeder CreateMenuSeeder()
    {
        return new MenuSeeder(DataSeedFolder, _repo);
    }

    public virtual RolesSeeder CreateRolesSeeder()
    {
        return new RolesSeeder(MyRoleManager, SecretDataFolder, DataSeedFolder, _repo);
    }

    public virtual UsersSeeder CreateUsersSeeder()
    {
        return new UsersSeeder(_myUserManager, SecretDataFolder, DataSeedFolder, _repo);
    }
}