using BackendCarcass.Application.Authentication.ConfirmEmail;
using BackendCarcassShared.Contracts.Errors;
using FluentValidation;

namespace BackendCarcass.Api.Validators;

// ReSharper disable once UnusedType.Global
public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailRequestCommand>
{
    public const string CodePattern = "^[0-9]{6}$";

    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.PendingToken).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyPendingTokenErrMessage);
        RuleFor(x => x.Code).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyCodeErrMessage).Matches(CodePattern)
            .WithErrorCode(AuthenticationApiErrors.InvalidCodeFormatErrCode)
            .WithMessage(AuthenticationApiErrors.InvalidCodeFormatErrMessage);
    }
}
