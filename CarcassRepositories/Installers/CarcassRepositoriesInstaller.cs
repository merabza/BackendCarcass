//Created by CarcassRepositoriesInstallerClassCreator at 8/1/2022 9:35:56 PM

using System.Collections.Generic;
using CarcassDom;
using CarcassIdentity;
using CarcassMasterDataDom;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CarcassRepositories.Installers;

// ReSharper disable once UnusedType.Global
public sealed class CarcassRepositoriesInstaller : IInstaller
{
    public int InstallPriority => 30;
    public int ServiceUsePriority => 30;

    public void InstallServices(WebApplicationBuilder builder, string[] args, Dictionary<string, string> parameters)
    {
        //Console.WriteLine("CarcassRepositoriesInstaller.InstallServices Started");

        builder.Services.AddScoped<IIdentityRepository, IdentityRepository>();
        builder.Services.AddScoped<IMenuRightsRepository, MenuRightsRepository>();
        builder.Services.AddScoped<IDataTypesRepository, DataTypesRepository>();
        builder.Services.AddScoped<IReturnValuesRepository, SqlReturnValuesRepository>();
        builder.Services.AddScoped<IRightsRepository, RightsRepository>();


        //Console.WriteLine("CarcassRepositoriesInstaller.InstallServices Finished");
    }

    public void UseServices(WebApplication app)
    {
    }
}