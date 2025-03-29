using CarcassDataSeeding.Seeders;
using CarcassMasterDataDom.Models;
using DatabaseToolsShared;
using Microsoft.AspNetCore.Identity;

namespace CarcassDataSeeding;

public /*open*/ class CarcassDataSeedersFabric
{
    private readonly ICarcassDataSeederRepository _carcassRepo;
    private readonly string _dataSeedFolder;
    private readonly RoleManager<AppRole> _myRoleManager;
    private readonly UserManager<AppUser> _myUserManager;
    private readonly IDataSeederRepository _repo;
    private readonly string _secretDataFolder;

    protected CarcassDataSeedersFabric(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager,
        string secretDataFolder, string dataSeedFolder, ICarcassDataSeederRepository carcassRepo,
        IDataSeederRepository repo)
    {
        _secretDataFolder = secretDataFolder;
        _dataSeedFolder = dataSeedFolder;
        _carcassRepo = carcassRepo;
        _repo = repo;
        _myUserManager = userManager;
        _myRoleManager = roleManager;
    }

    public virtual ITableDataSeeder CreateDataTypesSeeder()
    {
        return new DataTypesSeeder(_dataSeedFolder, _carcassRepo, _repo);
    }

    public virtual ITableDataSeeder CreateAppClaimsSeeder()
    {
        return new AppClaimsSeeder(_dataSeedFolder, _repo);
    }

    public virtual ITableDataSeeder CreateCrudRightTypesSeeder()
    {
        return new CrudRightTypesSeeder(_dataSeedFolder, _repo);
    }

    public virtual ITableDataSeeder CreateManyToManyJoinsSeeder()
    {
        return new ManyToManyJoinsSeeder(_secretDataFolder, _dataSeedFolder, _carcassRepo, _repo);
    }

    public virtual ITableDataSeeder CreateMenuGroupsSeeder()
    {
        return new MenuGroupsSeeder(_dataSeedFolder, _repo);
    }

    public virtual ITableDataSeeder CreateMenuSeeder()
    {
        return new MenuSeeder(_dataSeedFolder, _repo);
    }

    public virtual ITableDataSeeder CreateRolesSeeder()
    {
        return new RolesSeeder(_myRoleManager, _secretDataFolder, _dataSeedFolder, _repo);
    }

    public virtual ITableDataSeeder CreateUsersSeeder()
    {
        return new UsersSeeder(_myUserManager, _secretDataFolder, _dataSeedFolder, _repo);
    }
}