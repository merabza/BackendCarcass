using System.Collections.Generic;
using CarcassContracts.V1.Responses;
using CarcassContracts.V1.Routes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using WebInstallers;

namespace BackendCarcassApi.Endpoints.V1;

//კონტროლერი -> გამოიყენება გაშვებული პროცესების მიმდინარეობის გასაკონტროლებლად.
//რადგან ჯერჯერობით გაშვებული პროცესები არ გვაქვს, ეს კონტროლერი ჯერ-ჯერობით საჭირო არ არის.
//თუმცა მომავალში, როცა სერვისებთან ურთიერთობა იქნება საჭიროა, შეიძლება ეს კონტროლერი აღვადგინო.
//[Authorize]
//[ApiController]
//[Route("api/[controller]")]

// ReSharper disable once UnusedType.Global
public sealed class ProcessesEndpoints : IInstaller
{
    public int InstallPriority => 70;
    public int ServiceUsePriority => 70;

    public void InstallServices(WebApplicationBuilder builder, string[] args, Dictionary<string, string> parameters)
    {
    }

    public void UseServices(WebApplication app)
    {
        //Console.WriteLine("ProcessesEndpoints.UseServices Started");
        app.MapGet(CarcassApiRoutes.Processes.Status, Status).RequireAuthorization();
        //Console.WriteLine("ProcessesEndpoints.UseServices Finished");
    }

    private static IResult Status(int userId, int viewStyle)
    {
        var commandRunningStatus = new CommandRunningStatusResponse();
        return Results.Ok(commandRunningStatus);
    }
}