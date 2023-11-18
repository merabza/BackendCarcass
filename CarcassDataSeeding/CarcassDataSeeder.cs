using LanguageExt;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace CarcassDataSeeding;

public /*open*/ class CarcassDataSeeder
{
    protected readonly ILogger<CarcassDataSeeder> Logger;
    protected readonly DataSeedersFabric DataSeedersFabric;
    private readonly bool _checkOnly;

    protected CarcassDataSeeder(ILogger<CarcassDataSeeder> logger, DataSeedersFabric dataSeedersFabric,
        bool checkOnly)
    {
        Logger = logger;
        DataSeedersFabric = dataSeedersFabric;
        _checkOnly = checkOnly;
    }

    protected Option<Err[]> Use(IDataSeeder dataSeeder)
    {
        return dataSeeder.Create(_checkOnly);
    }


    public Option<Err[]> SeedData()
    {
        var result = SeedCarcassData();
        if (result.IsSome)
            return (Err[])result;

        return SeedProjectSpecificData();
    }


    protected virtual Option<Err[]> SeedProjectSpecificData()
    {
        return null;
    }

    private Option<Err[]> SeedCarcassData()
    {
        Logger.LogInformation("Seed Carcass Data Started");

        Logger.LogInformation("Seeding DataTypes");

        var result = Use(DataSeedersFabric.CreateDataTypesSeeder());
        if (result.IsSome)
            return (Err[])result;

        Logger.LogInformation("Seeding Users");

        result = Use(DataSeedersFabric.CreateUsersSeeder());
        if (result.IsSome)
            return (Err[])result;

        Logger.LogInformation("Seeding Roles");

        result = Use(DataSeedersFabric.CreateRolesSeeder());
        if (result.IsSome)
            return (Err[])result;

        Logger.LogInformation("Seeding MenuGroups");

        result = Use(DataSeedersFabric.CreateMenuGroupsSeeder());
        if (result.IsSome)
            return (Err[])result;

        Logger.LogInformation("Seeding Menu");

        result = Use(DataSeedersFabric.CreateMenuSeeder());
        if (result.IsSome)
            return (Err[])result;

        Logger.LogInformation("Seeding CrudRightTypes");

        result = Use(DataSeedersFabric.CreateCrudRightTypesSeeder());
        if (result.IsSome)
            return (Err[])result;

        Logger.LogInformation("Seeding AppClaims");

        result = Use(DataSeedersFabric.CreateAppClaimsSeeder());
        if (result.IsSome)
            return (Err[])result;

        Logger.LogInformation("Seeding ManyToManyJoin");

        result = Use(DataSeedersFabric.CreateManyToManyJoinsSeeder());
        if (result.IsSome)
            return (Err[])result;

        Logger.LogInformation("Seed Carcass Data Finished successful");

        return null;
    }
}