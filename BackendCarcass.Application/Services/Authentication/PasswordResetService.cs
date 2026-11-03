using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcass.Domain.EmailVerifications;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Services.Authentication;

//დავიწყებული პაროლის აღდგენა ელფოსტით
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PasswordResetService : IScopeServiceCarcassApplication
{
    private readonly LoginCompletionService _completion;
    private readonly EmailVerificationService _emailVerification;
    private readonly ILogger<PasswordResetService> _logger;
    private readonly PasswordService _passwords;
    private readonly UserManager<AppUser> _userMgr;

    public PasswordResetService(UserManager<AppUser> userMgr, EmailVerificationService emailVerification,
        PasswordService passwords, LoginCompletionService completion, ILogger<PasswordResetService> logger)
    {
        _userMgr = userMgr;
        _emailVerification = emailVerification;
        _passwords = passwords;
        _completion = completion;
        _logger = logger;
    }

    //პასუხი ყოველთვის წარმატებულია, რომ ამ მოთხოვნით ვერ დადგინდეს, არსებობს თუ არა მომხმარებელი
    public async Task<Result> ForgotPasswordAsync(string? userNameOrEmail,
        CancellationToken cancellationToken = default)
    {
        AppUser? user = await FindUserAsync(userNameOrEmail);
        if (user is null)
        {
            return Result.Success();
        }

        Result send = await _emailVerification.SendAsync(user, EEmailVerificationPurpose.PasswordReset,
            cancellationToken);
        if (send.IsFailure)
        {
            _logger.LogWarning("Password reset code was not sent to user {UserId}: {ErrorCode}", user.Id,
                send.Error.Code);
        }

        return Result.Success();
    }

    public async Task<Result<LoginResult>> ResetPasswordAsync(ResetPasswordParameters parameters,
        CancellationToken cancellationToken = default)
    {
        bool byLink = parameters.UserId is not null;
        AppUser? user = byLink
            ? await _userMgr.FindByIdAsync(parameters.UserId!.Value.ToString(CultureInfo.InvariantCulture))
            : await FindUserAsync(parameters.UserNameOrEmail);
        if (user is null)
        {
            return Result.Failure<LoginResult>(byLink
                ? AuthenticationApiErrors.InvalidConfirmationLink
                : AuthenticationApiErrors.InvalidOrExpiredCode);
        }

        //ჯერ პაროლის წესები მოწმდება, რომ სუსტი პაროლის შეყვანამ კოდი ან ბმული არ დახარჯოს
        Result validation = await _passwords.ValidateAsync(user, parameters.NewPassword);
        if (validation.IsFailure)
        {
            return Result.Failure<LoginResult>(validation.Error);
        }

        Result verify = byLink
            ? await _emailVerification.VerifyLinkAsync(user.Id, EEmailVerificationPurpose.PasswordReset,
                parameters.Token, cancellationToken)
            : await _emailVerification.VerifyCodeAsync(user.Id, EEmailVerificationPurpose.PasswordReset,
                parameters.Code, cancellationToken);
        if (verify.IsFailure)
        {
            return Result.Failure<LoginResult>(verify.Error);
        }

        //ელფოსტაზე წვდომა დადასტურდა, ამიტომ ელფოსტაც დადასტურებულად ითვლება
        user.EmailConfirmed = true;
        Result set = await _passwords.SetPasswordAsync(user, parameters.NewPassword, cancellationToken);
        if (set.IsFailure)
        {
            return Result.Failure<LoginResult>(set.Error);
        }

        return await _completion.IssueFullAsync(user, cancellationToken);
    }

    private async Task<AppUser?> FindUserAsync(string? userNameOrEmail)
    {
        if (string.IsNullOrWhiteSpace(userNameOrEmail))
        {
            return null;
        }

        string value = userNameOrEmail.Trim();
        return await _userMgr.FindByNameAsync(value) ?? await _userMgr.FindByEmailAsync(value);
    }
}
