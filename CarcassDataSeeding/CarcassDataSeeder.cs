using Microsoft.Extensions.Logging;

namespace CarcassDataSeeding;

public /*open*/ class CarcassDataSeeder
{
    private readonly bool _checkOnly;
    protected readonly DataSeedersFabric DataSeedersFabric;
    protected readonly ILogger<CarcassDataSeeder> Logger;

    protected CarcassDataSeeder(ILogger<CarcassDataSeeder> logger, DataSeedersFabric dataSeedersFabric, bool checkOnly)
    {
        Logger = logger;
        DataSeedersFabric = dataSeedersFabric;
        _checkOnly = checkOnly;
    }

    protected bool Use(IDataSeeder dataSeeder)
    {
        return dataSeeder.Create(_checkOnly);
    }

    public bool SeedData()
    {
        return SeedCarcassData() && SeedProjectSpecificData();
    }

    protected virtual bool SeedProjectSpecificData()
    {
        return true;
    }

    private bool SeedCarcassData()
    {
        Logger.LogInformation("Seed Carcass Data Started");

        Logger.LogInformation("Seeding DataTypes");

        if (!Use(DataSeedersFabric.CreateDataTypesSeeder()))
            return false;

        Logger.LogInformation("Seeding Users");

        if (!Use(DataSeedersFabric.CreateUsersSeeder()))
            return false;

        Logger.LogInformation("Seeding Roles");

        if (!Use(DataSeedersFabric.CreateRolesSeeder()))
            return false;

        Logger.LogInformation("Seeding MenuGroups");

        if (!Use(DataSeedersFabric.CreateMenuGroupsSeeder()))
            return false;

        Logger.LogInformation("Seeding Menu");

        if (!Use(DataSeedersFabric.CreateMenuSeeder()))
            return false;

        Logger.LogInformation("Seeding CrudRightTypes");

        if (!Use(DataSeedersFabric.CreateCrudRightTypesSeeder()))
            return false;

        Logger.LogInformation("Seeding AppClaims");

        if (!Use(DataSeedersFabric.CreateAppClaimsSeeder()))
            return false;

        Logger.LogInformation("Seeding ManyToManyJoin");

        if (!Use(DataSeedersFabric.CreateManyToManyJoinsSeeder()))
            return false;

        Logger.LogInformation("Seed Carcass Data Finished successful");

        return true;
    }
}