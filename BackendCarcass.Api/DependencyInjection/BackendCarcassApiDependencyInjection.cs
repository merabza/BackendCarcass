using BackendCarcass.Api.Endpoints.V1;
using BackendCarcass.Application.Identity.Sso;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace BackendCarcass.Api.DependencyInjection;

// ReSharper disable once UnusedType.Global
public static class BackendCarcassApiDependencyInjection
{
    public static bool UseBackendCarcassApiEndpoints(this IEndpointRouteBuilder endpoints,
        string myAllowSpecificOrigins, ILogger? debugLogger)
    {
        debugLogger?.Information("{MethodName} Started", nameof(UseBackendCarcassApiEndpoints));

        //SSO რეჟიმში შესვლა sso.ltg.ge-ზეა: ლოკალური /authentication/* endpoint-ების ნაცვლად /bff/*
        if (endpoints.IsCarcassSsoMode())
        {
            endpoints.UseBffEndpoints(debugLogger);
        }
        else
        {
            endpoints.UseAuthenticationEndpoints(myAllowSpecificOrigins, debugLogger);
        }

        endpoints.UseDataTypesEndpoints(debugLogger);
        endpoints.UseMasterDataEndpoints(debugLogger);
        endpoints.UseProcessesEndpoints(debugLogger);
        endpoints.UseRightsEndpoints(debugLogger);
        endpoints.UseUserRightsEndpoints(debugLogger);

        debugLogger?.Information("{MethodName} Finished", nameof(UseBackendCarcassApiEndpoints));

        return true;
    }

    internal static bool IsCarcassSsoMode(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.ServiceProvider.GetService<IOptions<CarcassAuthenticationSettings>>()?.Value.Mode ==
               EAuthenticationMode.Sso;
    }
}
