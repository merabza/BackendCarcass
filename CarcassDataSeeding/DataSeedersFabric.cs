using CarcassDataSeeding.Seeders;
using CarcassMasterDataDom.Models;
using Microsoft.AspNetCore.Identity;

namespace CarcassDataSeeding;

public /*open*/ class DataSeedersFabric
{
    private readonly UserManager<AppUser> _myUserManager;
    private readonly IDataSeederRepository _repo;
    protected readonly string DataSeedFolder;
    protected readonly RoleManager<AppRole> MyRoleManager;
    protected readonly string SecretDataFolder;

    protected DataSeedersFabric(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager,
        string secretDataFolder, string dataSeedFolder, IDataSeederRepository repo)
    {
        SecretDataFolder = secretDataFolder;
        DataSeedFolder = dataSeedFolder;
        _repo = repo;
        _myUserManager = userManager;
        MyRoleManager = roleManager;
    }

    public virtual IDataSeeder CreateDataTypesSeeder()
    {
        return new DataTypesSeeder(DataSeedFolder, _repo);
    }

    public virtual IDataSeeder CreateAppClaimsSeeder()
    {
        return new AppClaimsSeeder(DataSeedFolder, _repo);
    }

    public virtual IDataSeeder CreateCrudRightTypesSeeder()
    {
        return new CrudRightTypesSeeder(DataSeedFolder, _repo);
    }

    public virtual IDataSeeder CreateManyToManyJoinsSeeder()
    {
        return new ManyToManyJoinsSeeder(SecretDataFolder, DataSeedFolder, _repo);
    }

    public virtual IDataSeeder CreateMenuGroupsSeeder()
    {
        return new MenuGroupsSeeder(DataSeedFolder, _repo);
    }

    public virtual IDataSeeder CreateMenuSeeder()
    {
        return new MenuSeeder(DataSeedFolder, _repo);
    }

    public virtual IDataSeeder CreateRolesSeeder()
    {
        return new RolesSeeder(MyRoleManager, SecretDataFolder, DataSeedFolder, _repo);
    }

    public virtual IDataSeeder CreateUsersSeeder()
    {
        return new UsersSeeder(_myUserManager, SecretDataFolder, DataSeedFolder, _repo);
    }
}