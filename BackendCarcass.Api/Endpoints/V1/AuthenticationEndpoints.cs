using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Api.Mappers;
using BackendCarcass.Application.Authentication.Login;
using BackendCarcass.Application.Authentication.Registration;
using BackendCarcassContracts.Errors;
using BackendCarcassContracts.V1.Requests;
using BackendCarcassContracts.V1.Responses;
using BackendCarcassContracts.V1.Routes;
using CorsTools.DependencyInjection;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OneOf;
using Serilog;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Api.Endpoints.V1;

// ReSharper disable once UnusedType.Global
public static class AuthenticationEndpoints
{
    public static bool UseAuthenticationEndpoints(this IEndpointRouteBuilder endpoints, ILogger logger, bool debugMode)
    {
        if (debugMode)
        {
            logger.Information("{MethodName} Started", nameof(UseAuthenticationEndpoints));
        }

        RouteGroupBuilder group = endpoints
            .MapGroup(CarcassApiRoutes.ApiBase + CarcassApiRoutes.Authentication.AuthenticationBase)
            .RequireCors(CorsDependencyInjection.MyAllowSpecificOrigins);

        group.MapPost(CarcassApiRoutes.Authentication.Registration, Registration);
        group.MapPost(CarcassApiRoutes.Authentication.Login, Login);

        if (debugMode)
        {
            logger.Information("{MethodName} Finished", nameof(UseAuthenticationEndpoints));
        }

        return true;
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> დაარეგისტრიროს ახალი მომხმარებელი ბაზაში
    //შემავალი ინფორმაცია -> RegistrationModel კლასის ობიექტი, რომელიც მოდის ვებიდან
    //მოქმედება -> სხვადასხვა შემოწმებების შემდეგ ცდილობს ახალი მომხმარებლის დარეგისტრირებას
    //   და თუ რეგისტრაცია წარმატებით დასრულდა ავტომატურად ალოგინებს ახალ მომხმარებელს.
    //   გამოდის რომ ახალ მომხმარებელს ეგრევე შეუძლია მუშაობის დაწყება.
    //   მაგრამ სამწუხაროდ უფლებების არქონის გამო პრაქტიკულად შეეძლება მხოლოდ თავისი ინფორმაციის ცვლილება
    //   ან თავისივე რეგისტრაციის წაშლა
    // POST api/v1/authentication/registration
    private static async ValueTask<Results<Ok<LoginResponse>, BadRequest<Err[]>>> Registration(
        [FromBody] RegistrationRequest? request, IMediator mediator, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(RegistrationCommandHandler)} from {nameof(Registration)}");
        if (request is null)
        {
            return TypedResults.BadRequest(Err.Create(CarcassApiErrors.RequestIsEmpty));
        }

        RegistrationRequestCommand command = request.AdaptTo();
        OneOf<LoginResponse, Err[]> result = await mediator.Send(command, cancellationToken);
        return result.Match<Results<Ok<LoginResponse>, BadRequest<Err[]>>>(res => TypedResults.Ok(res),
            errors => TypedResults.BadRequest(errors));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> არსებული მომხმარებლის ავტორიზაცია პაროლის გამოყენებით
    //შემავალი ინფორმაცია -> LoginModel კლასის ობიექტი, რომელიც მოდის ვებიდან
    //მოქმედება -> სხვადასხვა შემოწმებების შემდეგ ცდილობს მომხმარებლის ავტორიზებას
    //   წარმატებული ავტორიზების შემთხვევაში იქმნება JwT, რომელიც მომხმარებლის ინფორმაციასთან ერთად გადაეწოდება გამომძახებელს
    // POST api/authentication/login
    private static async ValueTask<Results<Ok<LoginResponse>, BadRequest<Err[]>>> Login(
        [FromBody] LoginRequest? request, IMediator mediator, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(LoginCommandHandler)} from {nameof(Login)}");
        if (request is null)
        {
            return TypedResults.BadRequest(Err.Create(CarcassApiErrors.RequestIsEmpty));
        }

        LoginRequestCommand command = request.AdaptTo();
        OneOf<LoginResponse, Err[]> result = await mediator.Send(command, cancellationToken);
        return result.Match<Results<Ok<LoginResponse>, BadRequest<Err[]>>>(res => TypedResults.Ok(res),
            errors => TypedResults.BadRequest(errors));
    }
}
