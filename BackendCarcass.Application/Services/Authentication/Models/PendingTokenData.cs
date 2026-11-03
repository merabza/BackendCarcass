using BackendCarcassShared.Contracts.V1.Responses;

namespace BackendCarcass.Application.Services.Authentication.Models;

//დროებითი (pending) ტოკენიდან წაკითხული ინფორმაცია
public sealed record PendingTokenData(
    int UserId,
    ELoginNextStep Step,
    bool PasswordChangeRequired,
    bool CodeRequired);
