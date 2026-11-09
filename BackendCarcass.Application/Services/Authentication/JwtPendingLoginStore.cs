using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Services.Authentication.Models;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Services.Authentication;

//ნაგულისხმევი რეალიზაცია: მდგომარეობა დროებით (pending) JWT-შია, ცალკე audience-ით ("…/pending")
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class JwtPendingLoginStore : IPendingLoginStore
{
    private readonly AuthTokenService _tokens;

    public JwtPendingLoginStore(AuthTokenService tokens)
    {
        _tokens = tokens;
    }

    public Result<string> Save(AppUser user, PendingTokenData data)
    {
        return _tokens.CreatePendingToken(user, data.Step, data.CodeRequired, data.PasswordChangeRequired);
    }

    public Result<PendingTokenData> Read(string? token)
    {
        return _tokens.ValidatePendingToken(token);
    }
}
