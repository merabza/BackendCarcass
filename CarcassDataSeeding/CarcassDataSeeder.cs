using DatabaseToolsShared;
using Microsoft.Extensions.Logging;

namespace CarcassDataSeeding;

public /*open*/ class CarcassDataSeeder : DataSeeder
{
    protected readonly CarcassDataSeedersFabric DataSeedersFabric;
    protected readonly ILogger<CarcassDataSeeder> Logger;

    protected CarcassDataSeeder(ILogger<CarcassDataSeeder> logger, CarcassDataSeedersFabric dataSeedersFabric, bool checkOnly)
        : base(checkOnly)
    {
        Logger = logger;
        DataSeedersFabric = dataSeedersFabric;
    }

    public override bool SeedData()
    {
        return SeedCarcassData();
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