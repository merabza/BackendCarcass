using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Api.Mappers;
using BackendCarcass.Application.Authentication.ChangeRequiredPassword;
using BackendCarcass.Application.Authentication.ConfirmEmail;
using BackendCarcass.Application.Authentication.ConfirmEmailLink;
using BackendCarcass.Application.Authentication.ForgotPassword;
using BackendCarcass.Application.Authentication.Login;
using BackendCarcass.Application.Authentication.Logout;
using BackendCarcass.Application.Authentication.RefreshToken;
using BackendCarcass.Application.Authentication.Registration;
using BackendCarcass.Application.Authentication.ResendCode;
using BackendCarcass.Application.Authentication.ResetPassword;
using BackendCarcass.Application.Identity.DependencyInjection;
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

        //ყველა ეს endpoint-ი ანონიმურია, ამიტომ ერთი IP-დან მოთხოვნების სიხშირე შეზღუდულია
        RouteGroupBuilder group = endpoints
            .MapGroup(CarcassApiRoutes.ApiBase + CarcassApiRoutes.Authentication.AuthenticationBase)
            .RequireCors(myAllowSpecificOrigins)
            .RequireRateLimiting(CarcassIdentityDependencyInjection.AuthenticationRateLimitPolicy);

        group.MapPost(CarcassApiRoutes.Authentication.Registration, Registration);
        group.MapPost(CarcassApiRoutes.Authentication.Login, Login);
        group.MapPost(CarcassApiRoutes.Authentication.ConfirmEmail, ConfirmEmail);
        group.MapPost(CarcassApiRoutes.Authentication.ConfirmEmailLink, ConfirmEmailLink);
        group.MapPost(CarcassApiRoutes.Authentication.ResendCode, ResendCode);
        group.MapPost(CarcassApiRoutes.Authentication.ChangeRequiredPassword, ChangeRequiredPassword);
        group.MapPost(CarcassApiRoutes.Authentication.Refresh, Refresh);
        group.MapPost(CarcassApiRoutes.Authentication.Logout, Logout);
        group.MapPost(CarcassApiRoutes.Authentication.ForgotPassword, ForgotPassword);
        group.MapPost(CarcassApiRoutes.Authentication.ResetPassword, ResetPassword);

        debugLogger?.Information("{MethodName} Finished", nameof(UseAuthenticationEndpoints));

        return true;
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> დაარეგისტრიროს ახალი მომხმარებელი ბაზაში
    //შემავალი ინფორმაცია -> RegistrationModel კლასის ობიექტი, რომელიც მოდის ვებიდან
    //მოქმედება -> სხვადასხვა შემოწმებების შემდეგ ცდილობს ახალი მომხმარებლის დარეგისტრირებას.
    //   წარმატებული რეგისტრაციის შემდეგ მომხმარებელს ელფოსტაზე ეგზავნება დადასტურების კოდი და ბმული,
    //   პასუხში კი NextStep = ConfirmEmail და დროებითი ტოკენი. სრულ წვდომას მომხმარებელი ელფოსტის დადასტურების შემდეგ იღებს.
    //   უფლებების არქონის გამო ახალ მომხმარებელს პრაქტიკულად შეეძლება მხოლოდ თავისი ინფორმაციის ცვლილება
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
        return ToLoginResult(await handler.Handle(command, cancellationToken));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> არსებული მომხმარებლის ავტორიზაცია პაროლის გამოყენებით
    //შემავალი ინფორმაცია -> LoginModel კლასის ობიექტი, რომელიც მოდის ვებიდან
    //მოქმედება -> სხვადასხვა შემოწმებების შემდეგ ცდილობს მომხმარებლის ავტორიზებას.
    //   წარმატებული ავტორიზების შემთხვევაში იქმნება JwT და განახლების ტოკენი, რომლებიც მომხმარებლის ინფორმაციასთან ერთად
    //   გადაეწოდება გამომძახებელს. თუ ელფოსტა დაუდასტურებელია, ან პაროლი მოქმედ წესებს არ აკმაყოფილებს,
    //   პასუხში NextStep მიუთითებს მომდევნო ნაბიჯს და Token დროებითი ტოკენია მხოლოდ ამ ნაბიჯისთვის
    // POST api/v1/authentication/login
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
        return ToLoginResult(await handler.Handle(command, cancellationToken));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> ელფოსტის დადასტურება ელფოსტაზე გაგზავნილი კოდით
    //შემავალი ინფორმაცია -> დროებითი ტოკენი და კოდი
    //მოქმედება -> სწორი კოდის შემთხვევაში ელფოსტა დადასტურებულად ინიშნება და მომხმარებელი იღებს სრულ წვდომას,
    //   ან, თუ პაროლის შეცვლაც სჭირდება, NextStep = ChangePassword-ს ახალი დროებითი ტოკენით
    // POST api/v1/authentication/confirmemail
    private static async ValueTask<Results<Ok<LoginResponse>, BadRequest<Error>, ProblemHttpResult>> ConfirmEmail(
        [FromBody] ConfirmEmailRequest? request, ICommandHandler<ConfirmEmailRequestCommand, LoginResponse> handler,
        CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(ConfirmEmailCommandHandler)} from {nameof(ConfirmEmail)}");
        if (request is null)
        {
            return TypedResults.BadRequest(CarcassApiErrors.RequestIsEmpty);
        }

        ConfirmEmailRequestCommand command = request.AdaptTo();
        return ToLoginResult(await handler.Handle(command, cancellationToken));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> ელფოსტის დადასტურება ელფოსტაზე გაგზავნილი ბმულით
    //შემავალი ინფორმაცია -> ბმულიდან წაკითხული მომხმარებლის იდენტიფიკატორი და ტოკენი, სურვილისამებრ დროებითი ტოკენი
    //მოქმედება -> ელფოსტა დადასტურებულად ინიშნება. თუ დროებითი ტოკენიც მოვიდა (ბმული იმავე ბრაუზერში გაიხსნა),
    //   ავტორიზაცია მაშინვე სრულდება, სხვა შემთხვევაში NextStep = EmailConfirmedLoginRequired
    // POST api/v1/authentication/confirmemaillink
    private static async ValueTask<Results<Ok<LoginResponse>, BadRequest<Error>, ProblemHttpResult>>
        ConfirmEmailLink([FromBody] ConfirmEmailLinkRequest? request,
            ICommandHandler<ConfirmEmailLinkRequestCommand, LoginResponse> handler,
            CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(ConfirmEmailLinkCommandHandler)} from {nameof(ConfirmEmailLink)}");
        if (request is null)
        {
            return TypedResults.BadRequest(CarcassApiErrors.RequestIsEmpty);
        }

        ConfirmEmailLinkRequestCommand command = request.AdaptTo();
        return ToLoginResult(await handler.Handle(command, cancellationToken));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> კოდის ხელახლა გაგზავნა ელფოსტაზე
    //შემავალი ინფორმაცია -> დროებითი ტოკენი
    //მოქმედება -> ახალი კოდი იგზავნება, ძველი აღარ მოქმედებს. წუთში ერთხელზე ხშირად არ შეიძლება
    // POST api/v1/authentication/resendcode
    private static async ValueTask<Results<Ok, BadRequest<Error>, ProblemHttpResult>> ResendCode(
        [FromBody] ResendCodeRequest? request, ICommandHandler<ResendCodeRequestCommand> handler,
        CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(ResendCodeCommandHandler)} from {nameof(ResendCode)}");
        if (request is null)
        {
            return TypedResults.BadRequest(CarcassApiErrors.RequestIsEmpty);
        }

        ResendCodeRequestCommand command = request.AdaptTo();
        return ToResult(await handler.Handle(command, cancellationToken));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> ავტორიზაციისას მოთხოვნილი სავალდებულო პაროლის ცვლილება
    //შემავალი ინფორმაცია -> დროებითი ტოკენი, ელფოსტაზე გაგზავნილი კოდი (თუ ტოკენი მას მოითხოვს) და ახალი პაროლი
    //მოქმედება -> ახალი პაროლი მოქმედი წესებით მოწმდება და ინახება, ძველი სესიები უქმდება, მომხმარებელი იღებს სრულ წვდომას
    // POST api/v1/authentication/changerequiredpassword
    private static async ValueTask<Results<Ok<LoginResponse>, BadRequest<Error>, ProblemHttpResult>>
        ChangeRequiredPassword([FromBody] ChangeRequiredPasswordRequest? request,
            ICommandHandler<ChangeRequiredPasswordRequestCommand, LoginResponse> handler,
            CancellationToken cancellationToken = default)
    {
        Debug.WriteLine(
            $"Call {nameof(ChangeRequiredPasswordCommandHandler)} from {nameof(ChangeRequiredPassword)}");
        if (request is null)
        {
            return TypedResults.BadRequest(CarcassApiErrors.RequestIsEmpty);
        }

        ChangeRequiredPasswordRequestCommand command = request.AdaptTo();
        return ToLoginResult(await handler.Handle(command, cancellationToken));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> ვადაგასული წვდომის ტოკენის განახლება
    //შემავალი ინფორმაცია -> განახლების ტოკენი
    //მოქმედება -> ძველი განახლების ტოკენი უქმდება, იქმნება ახალი წვდომის და განახლების ტოკენები
    // POST api/v1/authentication/refresh
    private static async ValueTask<Results<Ok<LoginResponse>, BadRequest<Error>, ProblemHttpResult>> Refresh(
        [FromBody] RefreshTokenRequest? request, ICommandHandler<RefreshTokenRequestCommand, LoginResponse> handler,
        CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(RefreshTokenCommandHandler)} from {nameof(Refresh)}");
        if (request is null)
        {
            return TypedResults.BadRequest(CarcassApiErrors.RequestIsEmpty);
        }

        RefreshTokenRequestCommand command = request.AdaptTo();
        return ToLoginResult(await handler.Handle(command, cancellationToken));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> სისტემიდან გასვლა
    //შემავალი ინფორმაცია -> განახლების ტოკენი
    //მოქმედება -> განახლების ტოკენი უქმდება
    // POST api/v1/authentication/logout
    private static async ValueTask<Results<Ok, BadRequest<Error>, ProblemHttpResult>> Logout(
        [FromBody] LogoutRequest? request, ICommandHandler<LogoutRequestCommand> handler,
        CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(LogoutCommandHandler)} from {nameof(Logout)}");
        if (request is null)
        {
            return TypedResults.BadRequest(CarcassApiErrors.RequestIsEmpty);
        }

        LogoutRequestCommand command = request.AdaptTo();
        return ToResult(await handler.Handle(command, cancellationToken));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> დავიწყებული პაროლის აღდგენის დაწყება
    //შემავალი ინფორმაცია -> მომხმარებლის სახელი, ან ელფოსტის მისამართი
    //მოქმედება -> თუ ასეთი მომხმარებელი არსებობს, ელფოსტაზე ეგზავნება კოდი და ბმული.
    //   პასუხი ყოველთვის წარმატებულია, რომ მომხმარებლის არსებობა ვერ დადგინდეს
    // POST api/v1/authentication/forgotpassword
    private static async ValueTask<Results<Ok, BadRequest<Error>, ProblemHttpResult>> ForgotPassword(
        [FromBody] ForgotPasswordRequest? request, ICommandHandler<ForgotPasswordRequestCommand> handler,
        CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(ForgotPasswordCommandHandler)} from {nameof(ForgotPassword)}");
        if (request is null)
        {
            return TypedResults.BadRequest(CarcassApiErrors.RequestIsEmpty);
        }

        ForgotPasswordRequestCommand command = request.AdaptTo();
        return ToResult(await handler.Handle(command, cancellationToken));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> დავიწყებული პაროლის აღდგენა ელფოსტაზე მისული კოდით, ან ბმულით
    //შემავალი ინფორმაცია -> (მომხმარებლის სახელი/ელფოსტა + კოდი) ან (მომხმარებლის იდენტიფიკატორი + ბმულის ტოკენი), ახალი პაროლი
    //მოქმედება -> ახალი პაროლი მოქმედი წესებით მოწმდება და ინახება, ძველი სესიები უქმდება, მომხმარებელი იღებს სრულ წვდომას
    // POST api/v1/authentication/resetpassword
    private static async ValueTask<Results<Ok<LoginResponse>, BadRequest<Error>, ProblemHttpResult>> ResetPassword(
        [FromBody] ResetPasswordRequest? request, ICommandHandler<ResetPasswordRequestCommand, LoginResponse> handler,
        CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(ResetPasswordCommandHandler)} from {nameof(ResetPassword)}");
        if (request is null)
        {
            return TypedResults.BadRequest(CarcassApiErrors.RequestIsEmpty);
        }

        ResetPasswordRequestCommand command = request.AdaptTo();
        return ToLoginResult(await handler.Handle(command, cancellationToken));
    }

    private static Results<Ok<LoginResponse>, BadRequest<Error>, ProblemHttpResult> ToLoginResult(
        Result<LoginResponse> result)
    {
        return result.Match<LoginResponse, Results<Ok<LoginResponse>, BadRequest<Error>, ProblemHttpResult>>(
            res => TypedResults.Ok(res), errors => (ProblemHttpResult)CustomResults.Problem(errors));
    }

    private static Results<Ok, BadRequest<Error>, ProblemHttpResult> ToResult(Result result)
    {
        return result.Match<Results<Ok, BadRequest<Error>, ProblemHttpResult>>(() => TypedResults.Ok(),
            errors => (ProblemHttpResult)CustomResults.Problem(errors));
    }
}
