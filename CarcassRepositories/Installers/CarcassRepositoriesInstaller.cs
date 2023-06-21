//Created by CarcassRepositoriesInstallerClassCreator at 8/1/2022 9:35:56 PM

using CarcassIdentity;
using CarcassMasterDataDom;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WebInstallers;

namespace CarcassRepositories.Installers;

// ReSharper disable once UnusedType.Global
public sealed class CarcassRepositoriesInstaller : IInstaller
{
    public int InstallPriority => 30;
    public int ServiceUsePriority => 30;

    public void InstallServices(WebApplicationBuilder builder, string[] args)
    {
        //Console.WriteLine("CarcassRepositoriesInstaller.InstallServices Started");

        //builder.Services.AddScoped<IMasterDataRepository, CarcassMasterDataRepository>();
        builder.Services.AddScoped<IIdentityRepository, IdentityRepository>();
        //builder.Services.AddSingleton<IDataObserversManager, DataObserversManager>();
        builder.Services.AddScoped<IMenuRightsRepository, MenuRightsRepository>();
        builder.Services.AddScoped<IDataTypesRepository, DataTypesRepository>();

        //builder.Services.AddScoped<IMasterDataRepository, RtMasterDataRepository>();
        //builder.Services.AddSingleton<RtMasterDataRepoManager>();
        //builder.Services.AddSingleton<IGmDbRepositoryCreatorFabric, GmDbRepositoryCreatorFabric>()

        //Console.WriteLine("CarcassRepositoriesInstaller.InstallServices Finished");
    }

    public void UseServices(WebApplication app)
    {
    }
}