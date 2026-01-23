using BackendCarcass.DataSeeding.Seeders;
using BackendCarcass.MasterData.Models;
using Microsoft.AspNetCore.Identity;
using SystemTools.DatabaseToolsShared;
using SystemTools.DomainShared.Repositories;

namespace BackendCarcass.DataSeeding;

public /*open*/ class CarcassDataSeedersFactory
{
    private readonly IDataSeederRepository _repo;
    private readonly IUnitOfWork _unitOfWork;
    protected readonly ICarcassDataSeederRepository CarcassRepo;
    protected readonly string DataSeedFolder;
    protected readonly RoleManager<AppRole> MyRoleManager;
    protected readonly UserManager<AppUser> MyUserManager;
    protected readonly string SecretDataFolder;

    protected CarcassDataSeedersFactory(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager,
        string secretDataFolder, string dataSeedFolder, ICarcassDataSeederRepository carcassRepo,
        IDataSeederRepository repo, IUnitOfWork unitOfWork)
    {
        SecretDataFolder = secretDataFolder;
        DataSeedFolder = dataSeedFolder;
        CarcassRepo = carcassRepo;
        _repo = repo;
        _unitOfWork = unitOfWork;
        MyUserManager = userManager;
        MyRoleManager = roleManager;
    }

    public virtual ITableDataSeeder CreateDataTypesSeeder()
    {
        return new DataTypesSeeder(CarcassRepo, DataSeedFolder, _repo, _unitOfWork);
    }

    public virtual ITableDataSeeder CreateAppClaimsSeeder()
    {
        return new AppClaimsSeeder(DataSeedFolder, _repo, _unitOfWork);
    }

    public virtual ITableDataSeeder CreateCrudRightTypesSeeder()
    {
        return new CrudRightTypesSeeder(DataSeedFolder, _repo, _unitOfWork);
    }

    public virtual ITableDataSeeder CreateManyToManyJoinsSeeder()
    {
        return new ManyToManyJoinsSeeder(SecretDataFolder, CarcassRepo, DataSeedFolder, _repo, _unitOfWork);
    }

    public virtual ITableDataSeeder CreateMenuGroupsSeeder()
    {
        return new MenuGroupsSeeder(DataSeedFolder, _repo, _unitOfWork);
    }

    public virtual ITableDataSeeder CreateMenuSeeder()
    {
        return new MenuSeeder(DataSeedFolder, _repo, _unitOfWork);
    }

    public virtual ITableDataSeeder CreateRolesSeeder()
    {
        return new RolesSeeder(MyRoleManager, SecretDataFolder, DataSeedFolder, _repo, _unitOfWork);
    }

    public virtual ITableDataSeeder CreateUsersSeeder()
    {
        return new UsersSeeder(MyUserManager, SecretDataFolder, DataSeedFolder, _repo, _unitOfWork);
    }
}
