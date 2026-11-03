using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.AspNetCore.Identity;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Services.Authentication;

//პაროლის შემოწმება მოქმედი წესებით და ახალი პაროლის დაყენება
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PasswordService : IScopeServiceCarcassApplication
{
    private readonly RefreshTokenService _refreshTokens;
    private readonly UserManager<AppUser> _userMgr;

    public PasswordService(UserManager<AppUser> userMgr, RefreshTokenService refreshTokens)
    {
        _userMgr = userMgr;
        _refreshTokens = refreshTokens;
    }

    //აკმაყოფილებს თუ არა პაროლი მოქმედ წესებს. შეცდომების სია ზუსტად ეუბნება მომხმარებელს, რა არ არის რიგზე
    public async Task<Result> ValidateAsync(AppUser user, string? password)
    {
        var errors = new List<Error>();
        foreach (IPasswordValidator<AppUser> validator in _userMgr.PasswordValidators)
        {
            IdentityResult result = await validator.ValidateAsync(_userMgr, user, password);
            errors.AddRange(result.Errors.Select(e => Error.Problem(e.Code, e.Description)));
        }

        return errors.Count == 0 ? Result.Success() : Result.Failure(Result.CreateValidationError([.. errors]));
    }

    public async Task<bool> MeetsPolicyAsync(AppUser user, string password)
    {
        Result result = await ValidateAsync(user, password);
        return result.IsSuccess;
    }

    //ახალი პაროლის დაყენება. ბლოკირება იხსნება და მომხმარებლის ყველა სესია უქმდება,
    //რომ ძველი პაროლით გახსნილი სესიები აღარ გაგრძელდეს
    public async Task<Result> SetPasswordAsync(AppUser user, string? newPassword,
        CancellationToken cancellationToken = default)
    {
        Result validation = await ValidateAsync(user, newPassword);
        if (validation.IsFailure)
        {
            return validation;
        }

        user.PasswordHash = _userMgr.PasswordHasher.HashPassword(user, newPassword!);
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        IdentityResult update = await _userMgr.UpdateAsync(user);
        if (!update.Succeeded)
        {
            return Result.Failure(update.ToError(UserRightsErrors.FailedToChangePassword));
        }

        await _refreshTokens.RevokeAllAsync(user.Id, cancellationToken);
        return Result.Success();
    }
}
