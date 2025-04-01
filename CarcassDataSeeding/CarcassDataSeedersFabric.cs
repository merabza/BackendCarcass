using CarcassDataSeeding.Seeders;
using CarcassMasterDataDom.Models;
using DatabaseToolsShared;
using Microsoft.AspNetCore.Identity;

namespace CarcassDataSeeding;

public /*open*/ class CarcassDataSeedersFabric
{
    protected readonly ICarcassDataSeederRepository CarcassRepo;
    protected readonly string DataSeedFolder;
    private readonly RoleManager<AppRole> _myRoleManager;
    private readonly UserManager<AppUser> _myUserManager;
    private readonly IDataSeederRepository _repo;
    protected readonly string SecretDataFolder;

    protected CarcassDataSeedersFabric(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager,
        string secretDataFolder, string dataSeedFolder, ICarcassDataSeederRepository carcassRepo,
        IDataSeederRepository repo)
    {
        SecretDataFolder = secretDataFolder;
        DataSeedFolder = dataSeedFolder;
        CarcassRepo = carcassRepo;
        _repo = repo;
        _myUserManager = userManager;
        _myRoleManager = roleManager;
    }

    public virtual ITableDataSeeder CreateDataTypesSeeder()
    {
        return new DataTypesSeeder(DataSeedFolder, CarcassRepo, _repo);
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
        return new ManyToManyJoinsSeeder(SecretDataFolder, DataSeedFolder, CarcassRepo, _repo);
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
        return new RolesSeeder(_myRoleManager, SecretDataFolder, DataSeedFolder, _repo);
    }

    public virtual ITableDataSeeder CreateUsersSeeder()
    {
        return new UsersSeeder(_myUserManager, SecretDataFolder, DataSeedFolder, _repo);
    }
}