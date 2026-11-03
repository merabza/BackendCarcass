using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Repositories;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcass.Domain.EmailVerifications;
using BackendCarcassShared.Contracts.Errors;
using BackendCarcassShared.Contracts.V1.Responses;
using Microsoft.AspNetCore.Identity;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Services.Authentication;

//პაროლის შემოწმების შემდეგ წყვეტს, რა მიიღოს მომხმარებელმა:
//  - სრული წვდომა (წვდომის და განახლების ტოკენები), თუ ელფოსტა დადასტურებულია და პაროლი წესებს აკმაყოფილებს
//  - დროებითი ტოკენი ელფოსტის დასადასტურებლად, თუ ელფოსტა ჯერ არ დადასტურებულა
//  - დროებითი ტოკენი პაროლის შესაცვლელად, თუ პაროლი მოქმედ წესებს ვეღარ აკმაყოფილებს
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class LoginCompletionService : IScopeServiceCarcassApplication
{
    private readonly EmailVerificationService _emailVerification;
    private readonly RefreshTokenService _refreshTokens;
    private readonly AuthTokenService _tokens;
    private readonly IUserClaimsRepository _userClaimsRepository;
    private readonly UserManager<AppUser> _userMgr;

    public LoginCompletionService(UserManager<AppUser> userMgr, AuthTokenService tokens,
        RefreshTokenService refreshTokens, EmailVerificationService emailVerification,
        IUserClaimsRepository userClaimsRepository)
    {
        _userMgr = userMgr;
        _tokens = tokens;
        _refreshTokens = refreshTokens;
        _emailVerification = emailVerification;
        _userClaimsRepository = userClaimsRepository;
    }

    public Task<Result<LoginResult>> CompleteAsync(AppUser user, bool passwordChangeRequired,
        CancellationToken cancellationToken = default)
    {
        if (!user.EmailConfirmed)
        {
            return CreatePendingAsync(user, ELoginNextStep.ConfirmEmail, EEmailVerificationPurpose.EmailConfirmation,
                true, passwordChangeRequired, cancellationToken);
        }

        return passwordChangeRequired
            ? CreatePendingAsync(user, ELoginNextStep.ChangePassword, EEmailVerificationPurpose.PasswordChange, true,
                false, cancellationToken)
            : IssueFullAsync(user, cancellationToken);
    }

    //ელფოსტა ახლახან დადასტურდა ამავე სესიაში. თუ პაროლის ცვლილებაც გვჭირდება, მეორე კოდი აღარ იგზავნება
    public Task<Result<LoginResult>> AfterEmailConfirmedAsync(AppUser user, bool passwordChangeRequired,
        CancellationToken cancellationToken = default)
    {
        return passwordChangeRequired
            ? CreatePendingAsync(user, ELoginNextStep.ChangePassword, null, false, false, cancellationToken)
            : IssueFullAsync(user, cancellationToken);
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

    private async Task<Result<LoginResult>> CreatePendingAsync(AppUser user, ELoginNextStep step,
        EEmailVerificationPurpose? purpose, bool codeRequired, bool passwordChangeRequired,
        CancellationToken cancellationToken)
    {
        if (purpose is not null)
        {
            Result send = await _emailVerification.SendAsync(user, purpose.Value, cancellationToken);
            //თუ ბოლო კოდი წუთზე ნაკლები ხნის წინ გაიგზავნა, ისევ ის მოქმედებს და ახალს არ ვაგზავნით
            if (send.IsFailure && send.Error != AuthenticationApiErrors.CodeResendTooSoon)
            {
                return Result.Failure<LoginResult>(send.Error);
            }
        }

        Result<string> token = _tokens.CreatePendingToken(user, step, codeRequired, passwordChangeRequired);
        if (token.IsFailure)
        {
            return Result.Failure<LoginResult>(token.Error);
        }

        return new LoginResult
        {
            User = user, Token = token.Value, NextStep = step, AppClaims = [], Roles = []
        };
    }
}
