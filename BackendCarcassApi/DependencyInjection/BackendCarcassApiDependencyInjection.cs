using System;
using BackendCarcassApi.Endpoints.V1;
using Microsoft.AspNetCore.Routing;

namespace BackendCarcassApi.DependencyInjection;

// ReSharper disable once UnusedType.Global
public static class BackendCarcassApiDependencyInjection
{
    public static bool UseBackendCarcassApiEndpoints(this IEndpointRouteBuilder endpoints, bool debugMode)
    {
        if (debugMode)
            Console.WriteLine($"{nameof(UseBackendCarcassApiEndpoints)} Started");

        endpoints.UseAuthenticationEndpoints(debugMode);
        endpoints.UseDataTypesEndpoints(debugMode);
        endpoints.UseMasterDataEndpoints(debugMode);
        endpoints.UseProcessesEndpoints(debugMode);
        endpoints.UseRightsEndpoints(debugMode);
        endpoints.UseUserRightsEndpoints(debugMode);

        if (debugMode)
            Console.WriteLine($"{nameof(UseBackendCarcassApiEndpoints)} Finished");

        return true;
    }
}