using System;
using System.Linq;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity.Models;
using BackendCarcass.Application.Services.Authentication;
using BackendCarcass.Application.Tests.TestInfrastructure;
using BackendCarcass.Domain.RefreshTokens;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.Extensions.Options;
using SystemTools.SharedKernel;
using Xunit;

namespace BackendCarcass.Application.Tests.Services.Authentication;

// A refresh token is single use: consuming it revokes it, presenting a revoked one again revokes every session of
// the user, expired and unknown tokens fail, and logging out revokes the presented token only
public sealed class RefreshTokenServiceTests : IDisposable
{
    private readonly CarcassSqliteDatabase _db = new();
    private readonly RefreshTokenService _service;
    private readonly int _userId;

    public RefreshTokenServiceTests()
    {
        _userId = _db.AddUser().UsrId;
        _service = new RefreshTokenService(_db.Context, Options.Create(new IdentitySettings { RefreshTokenDays = 7 }));
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task Issue_stores_only_a_hash_and_consume_returns_the_user_and_revokes_the_token()
    {
        string raw = await _service.IssueAsync(_userId);

        RefreshToken row = Assert.Single(_db.Context.RefreshTokens);
        Assert.NotEqual(raw, row.TokenHash);
        Assert.Null(row.RevokedAt);
        Assert.True(row.ExpiresAt > DateTime.UtcNow.AddDays(6));

        Result<int> consumed = await _service.ConsumeAsync(raw);

        Assert.True(consumed.IsSuccess);
        Assert.Equal(_userId, consumed.Value);
        Assert.NotNull(_db.Context.RefreshTokens.Single().RevokedAt);
    }

    [Fact]
    public async Task Reusing_a_consumed_token_fails_and_revokes_all_sessions_of_the_user()
    {
        string first = await _service.IssueAsync(_userId);
        string second = await _service.IssueAsync(_userId);
        await _service.ConsumeAsync(first);

        Result<int> reused = await _service.ConsumeAsync(first);
        Result<int> secondResult = await _service.ConsumeAsync(second);

        Assert.Equal(AuthenticationApiErrors.InvalidRefreshToken, reused.Error);
        Assert.Equal(AuthenticationApiErrors.InvalidRefreshToken, secondResult.Error);
        Assert.All(_db.Context.RefreshTokens, t => Assert.NotNull(t.RevokedAt));
    }

    [Fact]
    public async Task An_expired_token_is_rejected()
    {
        string raw = await _service.IssueAsync(_userId);
        RefreshToken row = _db.Context.RefreshTokens.Single();
        row.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await _db.Context.SaveChangesAsync();

        Result<int> result = await _service.ConsumeAsync(raw);

        Assert.Equal(AuthenticationApiErrors.InvalidRefreshToken, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unknown-token")]
    public async Task Unknown_tokens_are_rejected(string? raw)
    {
        Result<int> result = await _service.ConsumeAsync(raw);

        Assert.Equal(AuthenticationApiErrors.InvalidRefreshToken, result.Error);
    }

    [Fact]
    public async Task Revoke_disables_the_presented_token_only()
    {
        string first = await _service.IssueAsync(_userId);
        string second = await _service.IssueAsync(_userId);

        await _service.RevokeAsync(first);

        Assert.True((await _service.ConsumeAsync(first)).IsFailure);
        Assert.True((await _service.ConsumeAsync(second)).IsFailure);
    }

    [Fact]
    public async Task Revoke_all_disables_every_token_of_the_user_but_not_of_others()
    {
        int otherUserId = _db.AddUser("other", "other@example.com").UsrId;
        string mine = await _service.IssueAsync(_userId);
        string theirs = await _service.IssueAsync(otherUserId);

        await _service.RevokeAllAsync(_userId);

        Assert.True((await _service.ConsumeAsync(mine)).IsFailure);
        Assert.True((await _service.ConsumeAsync(theirs)).IsSuccess);
    }

    [Fact]
    public async Task Issuing_cleans_up_the_expired_tokens_of_the_user()
    {
        await _service.IssueAsync(_userId);
        RefreshToken old = _db.Context.RefreshTokens.Single();
        old.ExpiresAt = DateTime.UtcNow.AddDays(-1);
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        await _service.IssueAsync(_userId);

        Assert.Single(_db.Context.RefreshTokens);
    }
}
