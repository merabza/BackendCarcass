using BackendCarcass.Api.Endpoints.V1;
using Microsoft.AspNetCore.Routing;
using Serilog;

namespace BackendCarcass.Api.DependencyInjection;

// ReSharper disable once UnusedType.Global
public static class BackendCarcassApiDependencyInjection
{
    public static bool UseBackendCarcassApiEndpoints(this IEndpointRouteBuilder endpoints, ILogger logger,
        bool debugMode)
    {
        if (debugMode) logger.Information("{MethodName} Started", nameof(UseBackendCarcassApiEndpoints));

        endpoints.UseAuthenticationEndpoints(logger, debugMode);
        endpoints.UseDataTypesEndpoints(logger, debugMode);
        endpoints.UseMasterDataEndpoints(logger, debugMode);
        endpoints.UseProcessesEndpoints(logger, debugMode);
        endpoints.UseRightsEndpoints(logger, debugMode);
        endpoints.UseUserRightsEndpoints(logger, debugMode);

        if (debugMode) logger.Information("{MethodName} Finished", nameof(UseBackendCarcassApiEndpoints));

        return true;
    }
}