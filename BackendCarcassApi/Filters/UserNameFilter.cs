//using System.Threading.Tasks;
//using BackendCarcassContracts.Errors;
//using CarcassIdentity;
//using Microsoft.AspNetCore.Http;

//namespace BackendCarcassApi.Filters;

//public sealed class UserNameFilter : IEndpointFilter
//{
//    private readonly ICurrentUser _currentUser;

//    // ReSharper disable once ConvertToPrimaryConstructor
//    public UserNameFilter(ICurrentUser currentUser)
//    {
//        _currentUser = currentUser;
//    }

//    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
//    {
//        var userName = _currentUser.Name;
//        return await next(context);
//    }
//}

