using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcass.Domain.EmailVerifications;
using BackendCarcassShared.Contracts.Errors;
using BackendCarcassShared.Contracts.V1.Responses;
using Microsoft.AspNetCore.Identity;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Services.Authentication;

//ავტორიზაციის დაუსრულებელი ნაბიჯები: ელფოსტის დადასტურება კოდით ან ბმულით, კოდის ხელახლა გაგზავნა,
//სავალდებულო პაროლის ცვლილება. ყველა მათგანი დროებით (pending) ტოკენს ეყრდნობა
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PendingLoginService : IScopeServiceCarcassApplication
{
    private readonly LoginCompletionService _completion;
    private readonly EmailVerificationService _emailVerification;
    private readonly PasswordService _passwords;
    private readonly AuthTokenService _tokens;
    private readonly UserManager<AppUser> _userMgr;

    public PendingLoginService(UserManager<AppUser> userMgr, AuthTokenService tokens,
        EmailVerificationService emailVerification, LoginCompletionService completion, PasswordService passwords)
    {
        _userMgr = userMgr;
        _tokens = tokens;
        _emailVerification = emailVerification;
        _completion = completion;
        _passwords = passwords;
    }

    public async Task<Result<LoginResult>> ConfirmEmailByCodeAsync(string? pendingToken, string? code,
        CancellationToken cancellationToken = default)
    {
        Result<PendingTokenData> tokenData = ValidateToken(pendingToken, ELoginNextStep.ConfirmEmail);
        if (tokenData.IsFailure)
        {
            return Result.Failure<LoginResult>(tokenData.Error);
        }

        AppUser? user = await FindUserAsync(tokenData.Value.UserId);
        if (user is null)
        {
            return Result.Failure<LoginResult>(AuthenticationApiErrors.InvalidPendingToken);
        }

        Result verify = await _emailVerification.VerifyCodeAsync(user.Id, EEmailVerificationPurpose.EmailConfirmation,
            code, cancellationToken);
        if (verify.IsFailure)
        {
            return Result.Failure<LoginResult>(verify.Error);
        }

        Result confirm = await MarkEmailConfirmedAsync(user);
        if (confirm.IsFailure)
        {
            return Result.Failure<LoginResult>(confirm.Error);
        }

        return await _completion.AfterEmailConfirmedAsync(user, tokenData.Value.PasswordChangeRequired,
            cancellationToken);
    }

    //ბმულით დადასტურება. თუ ბმული იმავე ბრაუზერში გაიხსნა და დროებითი ტოკენიც მოვიდა, ავტორიზაცია მაშინვე სრულდება,
    //სხვა შემთხვევაში მხოლოდ ელფოსტა დასტურდება და მომხმარებელმა ავტორიზაცია თავიდან უნდა გაიაროს
    public async Task<Result<LoginResult>> ConfirmEmailByLinkAsync(int userId, string? linkToken,
        string? pendingToken, CancellationToken cancellationToken = default)
    {
        AppUser? user = await FindUserAsync(userId);
        if (user is null)
        {
            return Result.Failure<LoginResult>(AuthenticationApiErrors.InvalidConfirmationLink);
        }

        Result verify = await _emailVerification.VerifyLinkAsync(user.Id, EEmailVerificationPurpose.EmailConfirmation,
            linkToken, cancellationToken);
        if (verify.IsFailure)
        {
            return Result.Failure<LoginResult>(verify.Error);
        }

        Result confirm = await MarkEmailConfirmedAsync(user);
        if (confirm.IsFailure)
        {
            return Result.Failure<LoginResult>(confirm.Error);
        }

        Result<PendingTokenData> tokenData = ValidateToken(pendingToken, ELoginNextStep.ConfirmEmail);
        if (tokenData.IsFailure || tokenData.Value.UserId != user.Id)
        {
            return new LoginResult
            {
                User = user,
                Token = string.Empty,
                NextStep = ELoginNextStep.EmailConfirmedLoginRequired,
                AppClaims = [],
                Roles = []
            };
        }

        return await _completion.AfterEmailConfirmedAsync(user, tokenData.Value.PasswordChangeRequired,
            cancellationToken);
    }

    public async Task<Result> ResendCodeAsync(string? pendingToken, CancellationToken cancellationToken = default)
    {
        Result<PendingTokenData> tokenData = _tokens.ValidatePendingToken(pendingToken);
        if (tokenData.IsFailure)
        {
            return Result.Failure(tokenData.Error);
        }

        EEmailVerificationPurpose purpose;
        switch (tokenData.Value.Step)
        {
            case ELoginNextStep.ConfirmEmail:
                purpose = EEmailVerificationPurpose.EmailConfirmation;
                break;
            case ELoginNextStep.ChangePassword when tokenData.Value.CodeRequired:
                purpose = EEmailVerificationPurpose.PasswordChange;
                break;
            default:
                return Result.Failure(AuthenticationApiErrors.InvalidPendingToken);
        }

        AppUser? user = await FindUserAsync(tokenData.Value.UserId);
        return user is null
            ? Result.Failure(AuthenticationApiErrors.InvalidPendingToken)
            : await _emailVerification.SendAsync(user, purpose, cancellationToken);
    }

    public async Task<Result<LoginResult>> ChangeRequiredPasswordAsync(string? pendingToken, string? code,
        string? newPassword, CancellationToken cancellationToken = default)
    {
        Result<PendingTokenData> tokenData = ValidateToken(pendingToken, ELoginNextStep.ChangePassword);
        if (tokenData.IsFailure)
        {
            return Result.Failure<LoginResult>(tokenData.Error);
        }

        AppUser? user = await FindUserAsync(tokenData.Value.UserId);
        if (user is null)
        {
            return Result.Failure<LoginResult>(AuthenticationApiErrors.InvalidPendingToken);
        }

        //ჯერ პაროლის წესები მოწმდება, რომ სუსტი პაროლის შეყვანამ კოდი არ დახარჯოს
        Result validation = await _passwords.ValidateAsync(user, newPassword);
        if (validation.IsFailure)
        {
            return Result.Failure<LoginResult>(validation.Error);
        }

        if (tokenData.Value.CodeRequired)
        {
            Result verify = await _emailVerification.VerifyCodeAsync(user.Id, EEmailVerificationPurpose.PasswordChange,
                code, cancellationToken);
            if (verify.IsFailure)
            {
                return Result.Failure<LoginResult>(verify.Error);
            }
        }

        Result set = await _passwords.SetPasswordAsync(user, newPassword, cancellationToken);
        if (set.IsFailure)
        {
            return Result.Failure<LoginResult>(set.Error);
        }

        return await _completion.IssueFullAsync(user, cancellationToken);
    }

    private Result<PendingTokenData> ValidateToken(string? pendingToken, ELoginNextStep expectedStep)
    {
        Result<PendingTokenData> tokenData = _tokens.ValidatePendingToken(pendingToken);
        if (tokenData.IsFailure)
        {
            return tokenData;
        }

        return tokenData.Value.Step == expectedStep
            ? tokenData
            : Result.Failure<PendingTokenData>(AuthenticationApiErrors.InvalidPendingToken);
    }

    private Task<AppUser?> FindUserAsync(int userId)
    {
        return _userMgr.FindByIdAsync(userId.ToString(CultureInfo.InvariantCulture));
    }

    private async Task<Result> MarkEmailConfirmedAsync(AppUser user)
    {
        if (user.EmailConfirmed)
        {
            return Result.Success();
        }

        user.EmailConfirmed = true;
        IdentityResult update = await _userMgr.UpdateAsync(user);
        return update.Succeeded
            ? Result.Success()
            : Result.Failure(update.ToError(UserRightsErrors.FailedToSaveUserInformation));
    }
}
