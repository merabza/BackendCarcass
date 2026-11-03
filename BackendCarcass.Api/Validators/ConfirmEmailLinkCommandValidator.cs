using BackendCarcass.Application.Authentication.ConfirmEmailLink;
using BackendCarcassShared.Contracts.Errors;
using FluentValidation;

namespace BackendCarcass.Api.Validators;

// ReSharper disable once UnusedType.Global
public sealed class ConfirmEmailLinkCommandValidator : AbstractValidator<ConfirmEmailLinkRequestCommand>
{
    public ConfirmEmailLinkCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithErrorCode(AuthenticationApiErrors.InvalidUserIdErrCode)
            .WithMessage(AuthenticationApiErrors.InvalidUserIdErrMessage);
        RuleFor(x => x.Token).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyLinkTokenErrMessage);
    }
}
