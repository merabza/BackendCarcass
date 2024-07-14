using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using BackendCarcassContracts.Errors;

namespace BackendCarcassApi.Filters;

public class UserNameFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var userName = context.HttpContext.User.Identity?.Name;
        if (userName == null)
            return Results.BadRequest(new[] { RightsApiErrors.UserNotIdentified });
        //context.Arguments.Add(userName);
        return await next(context);
    }
}