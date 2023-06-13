using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SystemToolsShared;

namespace BackendCarcassApi.Filters;

public class UserNameFilter : IEndpointFilter
{
    private const string UserNotIdentified = "მომხმარებლის იდენტიფიცირება ვერ მოხეხდა";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var userName = context.HttpContext.User.Identity?.Name;
        if (userName == null)
            return Results.BadRequest(new Err[]
                { new() { ErrorCode = "UserNotIdentified", ErrorMessage = UserNotIdentified } });
        //context.Arguments.Add(userName);
        return await next(context);
    }
}