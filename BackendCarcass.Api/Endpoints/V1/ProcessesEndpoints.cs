using BackendCarcassContracts.V1.Responses;
using BackendCarcassContracts.V1.Routes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Serilog;

namespace BackendCarcass.Api.Endpoints.V1;

//კონტროლერი -> გამოიყენება გაშვებული პროცესების მიმდინარეობის გასაკონტროლებლად.
//რადგან ჯერჯერობით გაშვებული პროცესები არ გვაქვს, ეს კონტროლერი ჯერ-ჯერობით საჭირო არ არის.
//თუმცა მომავალში, როცა სერვისებთან ურთიერთობა იქნება საჭიროა, შეიძლება ეს კონტროლერი აღვადგინო.
//[Authorize]
//[ApiController]
//[Route("api/[controller]")]

// ReSharper disable once UnusedType.Global
public static class ProcessesEndpoints
{
    public static bool UseProcessesEndpoints(this IEndpointRouteBuilder endpoints, ILogger? debugLogger)
    {
        debugLogger?.Information("{MethodName} Started", nameof(UseProcessesEndpoints));

        endpoints.MapGet(
            CarcassApiRoutes.ApiBase + CarcassApiRoutes.Processes.ProcessesBase + CarcassApiRoutes.Processes.Status,
            Status).RequireAuthorization();

        debugLogger?.Information("{MethodName} Finished", nameof(UseProcessesEndpoints));

        return true;
    }

    private static Ok<CommandRunningStatusResponse> Status()
    {
        var commandRunningStatus = new CommandRunningStatusResponse();
        return TypedResults.Ok(commandRunningStatus);
    }
}
