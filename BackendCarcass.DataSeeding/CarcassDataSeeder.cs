using Microsoft.Extensions.Logging;
using SystemTools.DatabaseToolsShared;

namespace BackendCarcass.DataSeeding;

public /*open*/ class CarcassDataSeeder : DataSeederBase
{
    protected readonly CarcassDataSeedersFactory DataSeedersFactory;
    protected readonly ILogger Logger;

    protected CarcassDataSeeder(ILogger logger, CarcassDataSeedersFactory dataSeedersFactory,
        bool checkOnly) : base(checkOnly)
    {
        Logger = logger;
        DataSeedersFactory = dataSeedersFactory;
    }

    public override bool SeedData()
    {
        return SeedCarcassData();
    }

    private bool SeedCarcassData()
    {
        Logger.LogInformation("Seed Carcass Data Started");

        Logger.LogInformation("Seeding DataTypes");

        if (!Use(DataSeedersFactory.CreateDataTypesSeeder()))
        {
            return false;
        }

        Logger.LogInformation("Seeding Users");

        if (!Use(DataSeedersFactory.CreateUsersSeeder()))
        {
            return false;
        }

        Logger.LogInformation("Seeding Roles");

        if (!Use(DataSeedersFactory.CreateRolesSeeder()))
        {
            return false;
        }

        Logger.LogInformation("Seeding MenuGroups");

        if (!Use(DataSeedersFactory.CreateMenuGroupsSeeder()))
        {
            return false;
        }

        Logger.LogInformation("Seeding Menu");

        if (!Use(DataSeedersFactory.CreateMenuSeeder()))
        {
            return false;
        }

        Logger.LogInformation("Seeding CrudRightTypes");

        if (!Use(DataSeedersFactory.CreateCrudRightTypesSeeder()))
        {
            return false;
        }

        Logger.LogInformation("Seeding AppClaims");

        if (!Use(DataSeedersFactory.CreateAppClaimsSeeder()))
        {
            return false;
        }

        Logger.LogInformation("Seeding ManyToManyJoin");

        if (!Use(DataSeedersFactory.CreateManyToManyJoinsSeeder()))
        {
            return false;
        }

        Logger.LogInformation("Seed Carcass Data Finished successful");

        return true;
    }
}
