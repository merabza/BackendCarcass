using BackendCarcass.Application.Identity.Models;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Services.Authentication;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcassShared.Contracts.Errors;
using BackendCarcassShared.Contracts.V1.Responses;
using Microsoft.Extensions.Options;
using SystemTools.SharedKernel;
using Xunit;

namespace BackendCarcass.Application.Tests.Services.Authentication;

// Pending tokens carry the login step and its flags, are signed with the JWT secret and use a separate audience,
// so a full access token can never be presented as a pending token and vice versa
public sealed class AuthTokenServiceTests
{
    private const string Secret = "0123456789abcdef0123456789abcdef0123456789abcdef";

    private readonly AuthTokenService _service = new(Options.Create(new IdentitySettings
    {
        JwtSecret = Secret, JwtIssuer = "https://issuer.test", JwtAudience = "https://audience.test"
    }));

    private readonly AppUser _user = new("merab", "Merab", "Test") { Id = 42, Email = "merab@example.com" };

    [Fact]
    public void Pending_token_round_trips_user_step_and_flags()
    {
        Result<string> token = _service.CreatePendingToken(_user, ELoginNextStep.ConfirmEmail, true, true);
        Assert.True(token.IsSuccess);

        Result<PendingTokenData> data = _service.ValidatePendingToken(token.Value);

        Assert.True(data.IsSuccess);
        Assert.Equal(new PendingTokenData(42, ELoginNextStep.ConfirmEmail, true, true), data.Value);
    }

    [Fact]
    public void Pending_token_flags_are_read_back_as_false_when_not_set()
    {
        Result<string> token = _service.CreatePendingToken(_user, ELoginNextStep.ChangePassword, false, false);

        Result<PendingTokenData> data = _service.ValidatePendingToken(token.Value);

        Assert.Equal(new PendingTokenData(42, ELoginNextStep.ChangePassword, false, false), data.Value);
    }

    [Fact]
    public void An_access_token_is_not_accepted_as_a_pending_token()
    {
        Result<string> accessToken = _service.CreateAccessToken(_user, []);
        Assert.True(accessToken.IsSuccess);

        Result<PendingTokenData> data = _service.ValidatePendingToken(accessToken.Value);

        Assert.True(data.IsFailure);
        Assert.Equal(AuthenticationApiErrors.InvalidPendingToken, data.Error);
    }

    [Fact]
    public void A_pending_token_signed_with_another_secret_is_rejected()
    {
        var otherService = new AuthTokenService(Options.Create(new IdentitySettings
        {
            JwtSecret = "another-secret-another-secret-another-secret-1234",
            JwtIssuer = "https://issuer.test",
            JwtAudience = "https://audience.test"
        }));
        Result<string> token = otherService.CreatePendingToken(_user, ELoginNextStep.ConfirmEmail, true, false);

        Result<PendingTokenData> data = _service.ValidatePendingToken(token.Value);

        Assert.True(data.IsFailure);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-jwt")]
    public void Garbage_is_rejected(string? token)
    {
        Result<PendingTokenData> data = _service.ValidatePendingToken(token);

        Assert.True(data.IsFailure);
        Assert.Equal(AuthenticationApiErrors.InvalidPendingToken, data.Error);
    }

    [Fact]
    public void Tokens_cannot_be_created_without_a_secret()
    {
        var service = new AuthTokenService(Options.Create(new IdentitySettings()));

        Assert.True(service.CreateAccessToken(_user, []).IsFailure);
        Assert.True(service.CreatePendingToken(_user, ELoginNextStep.ConfirmEmail, true, false).IsFailure);
    }
}
