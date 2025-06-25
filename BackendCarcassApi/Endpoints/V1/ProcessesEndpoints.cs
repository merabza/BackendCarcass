using System;
using System.Collections.Generic;
using BackendCarcassContracts.V1.Responses;
using BackendCarcassContracts.V1.Routes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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

    public bool InstallServices(WebApplicationBuilder builder, bool debugMode, string[] args,
        Dictionary<string, string> parameters)
    {
        return true;
    }

    public bool UseServices(WebApplication app, bool debugMode)
    {
        if (debugMode)
            Console.WriteLine($"{GetType().Name}.{nameof(UseServices)} Started");

        app.MapGet(
            CarcassApiRoutes.ApiBase + CarcassApiRoutes.Processes.ProcessesBase + CarcassApiRoutes.Processes.Status,
            Status).RequireAuthorization();

        if (debugMode)
            Console.WriteLine($"{GetType().Name}.{nameof(UseServices)} Finished");

        return true;
    }

    private static Ok<CommandRunningStatusResponse> Status(int userId, int viewStyle)
    {
        var commandRunningStatus = new CommandRunningStatusResponse();
        return TypedResults.Ok(commandRunningStatus);
    }
}