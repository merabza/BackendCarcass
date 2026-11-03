using BackendCarcass.Application.Authentication.ResendCode;
using BackendCarcassShared.Contracts.Errors;
using FluentValidation;

namespace BackendCarcass.Api.Validators;

// ReSharper disable once UnusedType.Global
public sealed class ResendCodeCommandValidator : AbstractValidator<ResendCodeRequestCommand>
{
    public ResendCodeCommandValidator()
    {
        RuleFor(x => x.PendingToken).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyPendingTokenErrMessage);
    }
}
