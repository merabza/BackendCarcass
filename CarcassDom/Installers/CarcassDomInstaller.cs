using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WebInstallers;

namespace CarcassDom.Installers;

public class CarcassDomInstaller : IInstaller
{
    public int InstallPriority => 30;
    public int ServiceUsePriority => 30;

    public void InstallServices(WebApplicationBuilder builder, string[] args)
    {
        Console.WriteLine("CarcassDomInstaller.InstallServices Started");

        builder.Services.AddSingleton<FilterSortManager>();

        Console.WriteLine("CarcassDomInstaller.InstallServices Finished");
    }

    public void UseServices(WebApplication app)
    {
    }
}