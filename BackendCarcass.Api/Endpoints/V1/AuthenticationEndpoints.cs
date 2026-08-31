using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Api.Mappers;
using BackendCarcass.Application.Authentication.Login;
using BackendCarcass.Application.Authentication.Registration;
using BackendCarcassShared.Contracts.Errors;
using BackendCarcassShared.Contracts.V1.Requests;
using BackendCarcassShared.Contracts.V1.Responses;
using BackendCarcassShared.Contracts.V1.Routes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Serilog;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;
using WebSystemTools.WebApi.Abstractions.Infrastructure;

namespace BackendCarcass.Api.Endpoints.V1;

// ReSharper disable once UnusedType.Global
public static class AuthenticationEndpoints
{
    public static bool UseAuthenticationEndpoints(this IEndpointRouteBuilder endpoints, string myAllowSpecificOrigins,
        ILogger? debugLogger)
    {
        debugLogger?.Information("{MethodName} Started", nameof(UseAuthenticationEndpoints));

        RouteGroupBuilder group = endpoints
            .MapGroup(CarcassApiRoutes.ApiBase + CarcassApiRoutes.Authentication.AuthenticationBase)
            .RequireCors(myAllowSpecificOrigins);

        group.MapPost(CarcassApiRoutes.Authentication.Registration, Registration);
        group.MapPost(CarcassApiRoutes.Authentication.Login, Login);

        debugLogger?.Information("{MethodName} Finished", nameof(UseAuthenticationEndpoints));

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
    private static async ValueTask<Results<Ok<LoginResponse>, BadRequest<Error>, ProblemHttpResult>> Registration(
        [FromBody] RegistrationRequest? request, ICommandHandler<RegistrationRequestCommand, LoginResponse> handler,
        CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(RegistrationCommandHandler)} from {nameof(Registration)}");
        if (request is null)
        {
            return TypedResults.BadRequest(CarcassApiErrors.RequestIsEmpty);
        }

        RegistrationRequestCommand command = request.AdaptTo();
        Result<LoginResponse> result = await handler.Handle(command, cancellationToken);
        return result.Match<LoginResponse, Results<Ok<LoginResponse>, BadRequest<Error>, ProblemHttpResult>>(
            res => TypedResults.Ok(res), errors => (ProblemHttpResult)CustomResults.Problem(errors));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> არსებული მომხმარებლის ავტორიზაცია პაროლის გამოყენებით
    //შემავალი ინფორმაცია -> LoginModel კლასის ობიექტი, რომელიც მოდის ვებიდან
    //მოქმედება -> სხვადასხვა შემოწმებების შემდეგ ცდილობს მომხმარებლის ავტორიზებას
    //   წარმატებული ავტორიზების შემთხვევაში იქმნება JwT, რომელიც მომხმარებლის ინფორმაციასთან ერთად გადაეწოდება გამომძახებელს
    // POST api/authentication/login
    private static async ValueTask<Results<Ok<LoginResponse>, BadRequest<Error>, ProblemHttpResult>> Login(
        [FromBody] LoginRequest? request, ICommandHandler<LoginRequestCommand, LoginResponse> handler,
        CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(LoginCommandHandler)} from {nameof(Login)}");
        if (request is null)
        {
            return TypedResults.BadRequest(CarcassApiErrors.RequestIsEmpty);
        }

        LoginRequestCommand command = request.AdaptTo();
        Result<LoginResponse> result = await handler.Handle(command, cancellationToken);

        return result.Match<LoginResponse, Results<Ok<LoginResponse>, BadRequest<Error>, ProblemHttpResult>>(
            res => TypedResults.Ok(res), errors => (ProblemHttpResult)CustomResults.Problem(errors));
    }
}
