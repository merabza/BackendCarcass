using System.Diagnostics;
using System.Threading.Tasks;
using CarcassContracts.ErrorModels;
using CarcassContracts.V1.Requests;
using CarcassContracts.V1.Routes;
using CorsTools;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerCarcassMini.Handlers.Authentication;
using ServerCarcassMini.Mappers;
using WebInstallers;

namespace ServerCarcassMini.Endpoints.V1;

// ReSharper disable once UnusedType.Global
public sealed class AuthenticationEndpoints : IInstaller
{
    public int InstallPriority => 70;
    public int ServiceUsePriority => 70;

    public void InstallServices(WebApplicationBuilder builder, string[] args)
    {
    }

    public void UseServices(WebApplication app)
    {
        //Console.WriteLine("AuthenticationEndpoints.UseServices Started");
        app.MapPost(CarcassApiRoutes.Authentication.Registration, Registration)
            .RequireCors(CorsInstaller.MyAllowSpecificOrigins);
        app.MapPost(CarcassApiRoutes.Authentication.Login, Login).RequireCors(CorsInstaller.MyAllowSpecificOrigins);
        //Console.WriteLine("AuthenticationEndpoints.UseServices Finished");
    }


    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> დაარეგისტრიროს ახალი მომხმარებელი ბაზაში
    //შემავალი ინფორმაცია -> RegistrationModel კლასის ობიექტი, რომელიც მოდის ვებიდან
    //მოქმედება -> სხვადასხვა შემოწმებების შემდეგ ცდილობს ახალი მომხმარებლის დარეგსიტრირებას
    //   და თუ რეგისტრაცია წარმატებით დასრულდა ავტომატურად ალოგინებს ახალ მომხმარებელს.
    //   გამოდის რომ ახალ მომხმარებელს ეგრევე შეუძლია მუშაობის დაწყება.
    //   მაგრამ სამწუხაროდ უფლებების არქონის გამო პრაქტიკულად შეეძლება მხოლოდ თავისი ინფორმაციის ცვლილება
    //   ან თავისივე რეგისტრაციის წაშლა
    // POST api/v1/authentication/registration
    private static async Task<IResult> Registration([FromBody] RegistrationRequest? request, IMediator mediator)
    {
        Debug.WriteLine($"Call {nameof(RegistrationCommandHandler)} from {nameof(Registration)}");
        if (request is null)
            return Results.BadRequest(CarcassApiErrors.RequestIsEmpty);
        var command = request.AdaptTo();
        var result = await mediator.Send(command);
        return result.Match(Results.Ok, Results.BadRequest);
    }


    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> არსებული მომხმარებლის ავტორიზაცია პაროლის გამოყენებით
    //შემავალი ინფორმაცია -> LoginModel კლასის ობიექტი, რომელიც მოდის ვებიდან
    //მოქმედება -> სხვადასხვა შემოწმებების შემდეგ ცდილობს მომხმარებლის ავტორიზებას
    //   წარმატებული ავტორიზების შემთხვევაში იქმნება JwT, რომელიც მომხმარებლის ინფორმაციასთან ერთად გადაეწოდება გამომძახებელს
    // POST api/authentication/login
    private static async Task<IResult> Login([FromBody] LoginRequest? request, IMediator mediator)
    {
        Debug.WriteLine($"Call {nameof(LoginCommandHandler)} from {nameof(Login)}");
        if (request is null)
            return Results.BadRequest(CarcassApiErrors.RequestIsEmpty);
        var command = request.AdaptTo();
        var result = await mediator.Send(command);
        return result.Match(Results.Ok, Results.BadRequest);
    }
}