using BackendCarcass.Api.Endpoints.V1;
using Microsoft.AspNetCore.Routing;
using Serilog;

namespace BackendCarcass.Api.DependencyInjection;

// ReSharper disable once UnusedType.Global
public static class BackendCarcassApiDependencyInjection
{
    public static bool UseBackendCarcassApiEndpoints(this IEndpointRouteBuilder endpoints, ILogger? debugLogger)
    {
        debugLogger?.Information("{MethodName} Started", nameof(UseBackendCarcassApiEndpoints));

        endpoints.UseAuthenticationEndpoints(debugLogger);
        endpoints.UseDataTypesEndpoints(debugLogger);
        endpoints.UseMasterDataEndpoints(debugLogger);
        endpoints.UseProcessesEndpoints(debugLogger);
        endpoints.UseRightsEndpoints(debugLogger);
        endpoints.UseUserRightsEndpoints(debugLogger);

        debugLogger?.Information("{MethodName} Finished", nameof(UseBackendCarcassApiEndpoints));

        return true;
    }
}
