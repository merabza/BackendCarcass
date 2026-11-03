using BackendCarcass.Application.Authentication.RefreshToken;
using BackendCarcassShared.Contracts.Errors;
using FluentValidation;

namespace BackendCarcass.Api.Validators;

// ReSharper disable once UnusedType.Global
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenRequestCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyRefreshTokenErrMessage);
    }
}
