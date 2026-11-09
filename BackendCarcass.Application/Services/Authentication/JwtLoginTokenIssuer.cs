using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Repositories;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcassShared.Contracts.Errors;
using BackendCarcassShared.Contracts.V1.Responses;
using Microsoft.AspNetCore.Identity;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Services.Authentication;

//ნაგულისხმევი რეალიზაცია: სრული წვდომა წვდომის ტოკენით (JWT) და განახლების ტოკენით
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class JwtLoginTokenIssuer : ILoginTokenIssuer
{
    private readonly RefreshTokenService _refreshTokens;
    private readonly AuthTokenService _tokens;
    private readonly IUserClaimsRepository _userClaimsRepository;
    private readonly UserManager<AppUser> _userMgr;

    public JwtLoginTokenIssuer(UserManager<AppUser> userMgr, AuthTokenService tokens,
        RefreshTokenService refreshTokens, IUserClaimsRepository userClaimsRepository)
    {
        _userMgr = userMgr;
        _tokens = tokens;
        _refreshTokens = refreshTokens;
        _userClaimsRepository = userClaimsRepository;
    }

    public async Task<Result<LoginResult>> IssueFullAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        if (user.UserName is null)
        {
            return Result.Failure<LoginResult>(AuthenticationApiErrors.InvalidUsername);
        }

        IList<string> roles = await _userMgr.GetRolesAsync(user);
        Result<string> accessToken = _tokens.CreateAccessToken(user, roles);
        if (accessToken.IsFailure)
        {
            return Result.Failure<LoginResult>(accessToken.Error);
        }

        string refreshToken = await _refreshTokens.IssueAsync(user.Id, cancellationToken);
        List<string>? appClaims = await _userClaimsRepository.UserAppClaims(user.UserName, cancellationToken);

        return new LoginResult
        {
            User = user,
            LastSequentialNumber = 0,
            Token = accessToken.Value,
            RefreshToken = refreshToken,
            NextStep = ELoginNextStep.None,
            AppClaims = appClaims ?? [],
            Roles = roles
        };
    }
}
