
using BackendCarcassContracts.V1.Responses;
using Carcass.Application.Services.Authentication;
using MediatRMessagingAbstractions;
using SystemToolsShared.Errors;
using OneOf;

namespace Carcass.Application.Authentication.Login;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class LoginCommandHandler : LoginCommandHandlerBase, ICommandHandler<LoginRequestCommand, LoginResponse>
{
    private readonly LoginService _loginService;

    // ReSharper disable once ConvertToPrimaryConstructor
    public LoginCommandHandler(LoginService loginService)
    {
        _loginService = loginService;
    }

    public async Task<OneOf<LoginResponse, Err[]>> Handle(LoginRequestCommand request,
        CancellationToken cancellationToken = default)
    {
        var tryLoginResult = await _loginService.TryToLogin(request.UserName!, request.Password!, cancellationToken);
        if (tryLoginResult.IsT1)
            return tryLoginResult.AsT1;

        var user = tryLoginResult.AsT0.User;
        var appUserModel = new LoginResponse(user.Id, LastSequentialNumber, user.UserName!, user.Email!,
            tryLoginResult.AsT0.Token,
            tryLoginResult.AsT0.Roles.Aggregate(string.Empty,
                (cur, next) => cur + (cur == string.Empty ? string.Empty : ", ") + next), user.FirstName, user.LastName,
            tryLoginResult.AsT0.AppClaims);
        return appUserModel;

        ////მოწოდებული მომხმარებლის სახელით ხომ არ არსებობს უკვე რომელიმე მომხმარებელი
        //var user = await _userMgr.FindByNameAsync(request.UserName!);
        ////თუ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        //if (user == null)
        //    return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        ////აქ მოვალთ მხოლოდ იმ შემთხვევაში, თუ მომხმარებელი წარმატებით შეიქმნა,
        ////მოხდეს მისი ავტომატური ავტორიზაცია
        //user = await DoLogin(_signinMgr, user, request.Password!);
        //if (user == null)
        //    return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        //if (user.UserName == null)
        //    return new[] { AuthenticationApiErrors.InvalidUsername };

        //if (user.Email == null)
        //    return new[] { AuthenticationApiErrors.InvalidEmail };

        //var roles = await _userMgr.GetRolesAsync(user);

        ////ახლადშექმნილ მომხმარებელს როლები არ აქვს, ამიტომ შემდეგი ბრძანება დაკომენტარებულია
        ////თუ მომავალში საჭირო გახდა, რომ ახლადშექმნილ მომხმარებელს ავტომატურად მიეცეს როლი, მაშინ შემდეგი ბრძანება უნდა აღსდგეს
        ////IList<string> roles = await _userMgr.GetRolesAsync(user);

        //if (_identitySettings.Value.JwtSecret is null)
        //    return new[] { CarcassApiErrors.ParametersAreInvalid };

        //LastSequentialNumber++;
        //var token = user.CreateJwToken(_identitySettings.Value.JwtSecret, LastSequentialNumber, roles);
        //if (token is null)
        //    return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        //var appUserModel = new LoginResponse(user.Id, LastSequentialNumber, user.UserName, user.Email, token,
        //    roles.Aggregate(string.Empty, (cur, next) => cur + (cur == string.Empty ? string.Empty : ", ") + next),
        //    user.FirstName, user.LastName, await _mdRepo.UserAppClaims(user.UserName, cancellationToken));
        //return appUserModel;
    }
}