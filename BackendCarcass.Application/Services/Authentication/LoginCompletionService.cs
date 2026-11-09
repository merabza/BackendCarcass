using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcass.Domain.EmailVerifications;
using BackendCarcassShared.Contracts.Errors;
using BackendCarcassShared.Contracts.V1.Responses;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Services.Authentication;

//პაროლის შემოწმების შემდეგ წყვეტს, რა მიიღოს მომხმარებელმა:
//  - სრული წვდომა (ILoginTokenIssuer), თუ ელფოსტა დადასტურებულია და პაროლი წესებს აკმაყოფილებს
//  - დაუსრულებელი ნაბიჯი ელფოსტის დასადასტურებლად, თუ ელფოსტა ჯერ არ დადასტურებულა
//  - დაუსრულებელი ნაბიჯი პაროლის შესაცვლელად, თუ პაროლი მოქმედ წესებს ვეღარ აკმაყოფილებს
//დაუსრულებელი ნაბიჯის მდგომარეობას IPendingLoginStore ინახავს. სანამ რამე გაიცემა, ეშვება შესვლის
//დამატებითი შემოწმებები (ILoginCheck)
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class LoginCompletionService : IScopeServiceCarcassApplication
{
    private readonly IEnumerable<ILoginCheck> _checks;
    private readonly EmailVerificationService _emailVerification;
    private readonly IPendingLoginStore _pendingStore;
    private readonly ILoginTokenIssuer _tokenIssuer;

    public LoginCompletionService(ILoginTokenIssuer tokenIssuer, IPendingLoginStore pendingStore,
        EmailVerificationService emailVerification, IEnumerable<ILoginCheck> checks)
    {
        _tokenIssuer = tokenIssuer;
        _pendingStore = pendingStore;
        _emailVerification = emailVerification;
        _checks = checks;
    }

    public async Task<Result<LoginResult>> CompleteAsync(AppUser user, bool passwordChangeRequired,
        CancellationToken cancellationToken = default)
    {
        Result check = await RunChecksAsync(user, cancellationToken);
        if (check.IsFailure)
        {
            return Result.Failure<LoginResult>(check.Error);
        }

        if (!user.EmailConfirmed)
        {
            return await CreatePendingAsync(user, ELoginNextStep.ConfirmEmail,
                EEmailVerificationPurpose.EmailConfirmation, true, passwordChangeRequired, cancellationToken);
        }

        return passwordChangeRequired
            ? await CreatePendingAsync(user, ELoginNextStep.ChangePassword, EEmailVerificationPurpose.PasswordChange,
                true, false, cancellationToken)
            : await _tokenIssuer.IssueFullAsync(user, cancellationToken);
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
        Result check = await RunChecksAsync(user, cancellationToken);
        return check.IsFailure
            ? Result.Failure<LoginResult>(check.Error)
            : await _tokenIssuer.IssueFullAsync(user, cancellationToken);
    }

    private async Task<Result> RunChecksAsync(AppUser user, CancellationToken cancellationToken)
    {
        foreach (ILoginCheck loginCheck in _checks)
        {
            Result result = await loginCheck.CheckAsync(user, cancellationToken);
            if (result.IsFailure)
            {
                return result;
            }
        }

        return Result.Success();
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

        Result<string> token = _pendingStore.Save(user,
            new PendingTokenData(user.Id, step, passwordChangeRequired, codeRequired));
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
