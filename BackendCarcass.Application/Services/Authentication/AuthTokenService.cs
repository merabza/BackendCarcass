using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendCarcass.Application.Identity.Models;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcassShared.Contracts.Errors;
using BackendCarcassShared.Contracts.V1.Responses;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Services.Authentication;

//ქმნის წვდომის ტოკენებს და დროებით (pending) ტოკენებს.
//დროებით ტოკენს ცალკე audience აქვს ("…/pending"), ამიტომ ჩვეულებრივი JWT ავტორიზაცია მას არ მიიღებს:
//ის გამოდგება მხოლოდ ელფოსტის დადასტურების და სავალდებულო პაროლის ცვლილების endpoint-ებისთვის
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class AuthTokenService : IScopeServiceCarcassApplication
{
    public const string StepClaim = "carcass_step";
    public const string PasswordChangeRequiredClaim = "carcass_password_change_required";
    public const string CodeRequiredClaim = "carcass_code_required";
    private const string PendingAudienceSuffix = "/pending";
    private const int PendingTokenMinutes = 15;
    private const string TrueValue = "1";
    private const string FalseValue = "0";

    private readonly IdentitySettings _settings;

    public AuthTokenService(IOptions<IdentitySettings> identitySettings)
    {
        _settings = identitySettings.Value;
    }

    private string PendingAudience => _settings.JwtAudience + PendingAudienceSuffix;

    public Result<string> CreateAccessToken(AppUser user, IList<string> roles)
    {
        if (_settings.JwtSecret is null)
        {
            return Result.Failure<string>(CarcassApiErrors.ParametersAreInvalid);
        }

        string? token = user.CreateJwToken(_settings.JwtSecret, 0,
            DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes), roles, _settings.JwtIssuer,
            _settings.JwtAudience);
        return token is null
            ? Result.Failure<string>(AuthenticationApiErrors.UsernameOrPasswordIsIncorrect)
            : Result.Success(token);
    }

    public Result<string> CreatePendingToken(AppUser user, ELoginNextStep step, bool codeRequired,
        bool passwordChangeRequired)
    {
        if (_settings.JwtSecret is null)
        {
            return Result.Failure<string>(CarcassApiErrors.ParametersAreInvalid);
        }

        var claims = new List<Claim>
        {
            new(StepClaim, step.ToString()),
            new(PasswordChangeRequiredClaim, passwordChangeRequired ? TrueValue : FalseValue),
            new(CodeRequiredClaim, codeRequired ? TrueValue : FalseValue)
        };
        string? token = user.CreateJwToken(_settings.JwtSecret, 0, DateTime.UtcNow.AddMinutes(PendingTokenMinutes),
            null, _settings.JwtIssuer, PendingAudience, claims);
        return token is null
            ? Result.Failure<string>(AuthenticationApiErrors.InvalidPendingToken)
            : Result.Success(token);
    }

    public Result<PendingTokenData> ValidatePendingToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token) || _settings.JwtSecret is null)
        {
            return Result.Failure<PendingTokenData>(AuthenticationApiErrors.InvalidPendingToken);
        }

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_settings.JwtSecret)),
            ValidateIssuer = _settings.JwtIssuer is not null,
            ValidIssuer = _settings.JwtIssuer,
            ValidateAudience = true,
            ValidAudience = PendingAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        ClaimsPrincipal principal;
        try
        {
            principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
        }
        catch (SecurityTokenException)
        {
            return Result.Failure<PendingTokenData>(AuthenticationApiErrors.InvalidPendingToken);
        }
        catch (ArgumentException)
        {
            return Result.Failure<PendingTokenData>(AuthenticationApiErrors.InvalidPendingToken);
        }

        if (!int.TryParse(principal.FindFirstValue(ClaimTypes.Sid), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out int userId) ||
            !Enum.TryParse(principal.FindFirstValue(StepClaim), out ELoginNextStep step))
        {
            return Result.Failure<PendingTokenData>(AuthenticationApiErrors.InvalidPendingToken);
        }

        return Result.Success(new PendingTokenData(userId, step,
            principal.FindFirstValue(PasswordChangeRequiredClaim) == TrueValue,
            principal.FindFirstValue(CodeRequiredClaim) == TrueValue));
    }
}
