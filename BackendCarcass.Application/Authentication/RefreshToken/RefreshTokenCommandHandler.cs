using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Services.Authentication;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcassShared.Contracts.Errors;
using BackendCarcassShared.Contracts.V1.Responses;
using Microsoft.AspNetCore.Identity;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Authentication.RefreshToken;

//განახლების ტოკენით ახალი წვდომის და განახლების ტოკენების მიღება.
//როლები და უფლებები ბაზიდან თავიდან იკითხება, ამიტომ მათი ცვლილება წვდომის ტოკენის ვადის გასვლისთანავე მოქმედებს
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class RefreshTokenCommandHandler(
    RefreshTokenService refreshTokenService,
    LoginCompletionService loginCompletionService,
    UserManager<AppUser> userManager) : ICommandHandler<RefreshTokenRequestCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(RefreshTokenRequestCommand request,
        CancellationToken cancellationToken)
    {
        Result<int> userId = await refreshTokenService.ConsumeAsync(request.RefreshToken, cancellationToken);
        if (userId.IsFailure)
        {
            return Result.Failure<LoginResponse>(userId.Error);
        }

        AppUser? user = await userManager.FindByIdAsync(userId.Value.ToString(CultureInfo.InvariantCulture));
        if (user is null)
        {
            return Result.Failure<LoginResponse>(AuthenticationApiErrors.InvalidRefreshToken);
        }

        Result<LoginResult> result = await loginCompletionService.IssueFullAsync(user, cancellationToken);
        return result.IsFailure ? Result.Failure<LoginResponse>(result.Error) : result.Value.ToLoginResponse();
    }
}
