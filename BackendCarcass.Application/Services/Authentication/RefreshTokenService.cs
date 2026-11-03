using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.Identity.Models;
using BackendCarcass.Domain.RefreshTokens;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Services.Authentication;

//განახლების ტოკენები ბაზაში ჰეშირებული ინახება. ყოველი გამოყენებისას ძველი უქმდება და ახალი იქმნება (rotation)
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class RefreshTokenService : IScopeServiceCarcassApplication
{
    private readonly ICarcassApplicationDbContext _ctx;
    private readonly IdentitySettings _settings;

    public RefreshTokenService(ICarcassApplicationDbContext ctx, IOptions<IdentitySettings> identitySettings)
    {
        _ctx = ctx;
        _settings = identitySettings.Value;
    }

    public async Task<string> IssueAsync(int userId, CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;

        //ვადაგასული ტოკენები აღარ გვჭირდება
        await _ctx.RefreshTokens.Where(t => t.UserId == userId && t.ExpiresAt < now)
            .ExecuteDeleteAsync(cancellationToken);

        string rawToken = TokenHashing.CreateRandomToken();
        _ctx.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            TokenHash = TokenHashing.Sha256Hex(rawToken),
            CreatedAt = now,
            ExpiresAt = now.AddDays(_settings.RefreshTokenDays)
        });
        await _ctx.SaveChangesAsync(cancellationToken);
        return rawToken;
    }

    //ამოწმებს ტოკენს, აუქმებს მას და აბრუნებს მომხმარებლის იდენტიფიკატორს. ახალი ტოკენი IssueAsync-ით უნდა შეიქმნას
    public async Task<Result<int>> ConsumeAsync(string? rawToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return Result.Failure<int>(AuthenticationApiErrors.InvalidRefreshToken);
        }

        string tokenHash = TokenHashing.Sha256Hex(rawToken);
        RefreshToken? token =
            await _ctx.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);
        if (token is null)
        {
            return Result.Failure<int>(AuthenticationApiErrors.InvalidRefreshToken);
        }

        DateTime now = DateTime.UtcNow;
        if (token.RevokedAt is not null)
        {
            //უკვე გაუქმებული ტოკენის ხელახლა გამოყენება ნიშნავს, რომ ის სხვის ხელშია:
            //უსაფრთხოებისთვის მომხმარებლის ყველა სესია უქმდება
            await RevokeAllAsync(token.UserId, cancellationToken);
            return Result.Failure<int>(AuthenticationApiErrors.InvalidRefreshToken);
        }

        token.RevokedAt = now;
        await _ctx.SaveChangesAsync(cancellationToken);

        return token.ExpiresAt <= now
            ? Result.Failure<int>(AuthenticationApiErrors.InvalidRefreshToken)
            : Result.Success(token.UserId);
    }

    public async Task RevokeAsync(string? rawToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return;
        }

        string tokenHash = TokenHashing.Sha256Hex(rawToken);
        RefreshToken? token = await _ctx.RefreshTokens.SingleOrDefaultAsync(
            t => t.TokenHash == tokenHash && t.RevokedAt == null, cancellationToken);
        if (token is null)
        {
            return;
        }

        token.RevokedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(cancellationToken);
    }

    //მომხმარებლის ყველა მოქმედი სესიის გაუქმება (პაროლის ცვლილებისას, ტოკენის მოპარვის ეჭვისას)
    public async Task RevokeAllAsync(int userId, CancellationToken cancellationToken = default)
    {
        List<RefreshToken> activeTokens = await _ctx.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null).ToListAsync(cancellationToken);
        if (activeTokens.Count == 0)
        {
            return;
        }

        DateTime now = DateTime.UtcNow;
        foreach (RefreshToken token in activeTokens)
        {
            token.RevokedAt = now;
        }

        await _ctx.SaveChangesAsync(cancellationToken);
    }
}
