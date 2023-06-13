using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace CarcassDataSeeding;

public /*open*/ class CarcassDataSeeder
{
    protected readonly ILogger<CarcassDataSeeder> Logger;
    protected readonly DataSeedersFabric DataSeedersFabric;

    public List<string> Messages { get; } = new();

    protected CarcassDataSeeder(ILogger<CarcassDataSeeder> logger, DataSeedersFabric dataSeedersFabric)
    {
        Logger = logger;
        DataSeedersFabric = dataSeedersFabric;
    }

    protected bool Use(IDataSeeder dataSeeder)
    {
        (bool success, List<string> messages) result = dataSeeder.Create();
        Messages.AddRange(result.messages);
        return result.success;
    }


    public bool SeedData()
    {
        return SeedCarcassData() && SeedProjectSpecificData(); // && SeedFinalCarcassData();
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