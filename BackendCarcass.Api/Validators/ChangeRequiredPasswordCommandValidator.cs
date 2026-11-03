using BackendCarcass.Application.Authentication.ChangeRequiredPassword;
using BackendCarcassShared.Contracts.Errors;
using FluentValidation;

namespace BackendCarcass.Api.Validators;

// ReSharper disable once UnusedType.Global
public sealed class ChangeRequiredPasswordCommandValidator : AbstractValidator<ChangeRequiredPasswordRequestCommand>
{
    public ChangeRequiredPasswordCommandValidator()
    {
        RuleFor(x => x.PendingToken).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyPendingTokenErrMessage);
        //კოდი საჭიროა მხოლოდ მაშინ, თუ ტოკენი მას მოითხოვს. ეს ჰენდლერში მოწმდება, აქ მხოლოდ ფორმატი
        RuleFor(x => x.Code).Matches(ConfirmEmailCommandValidator.CodePattern)
            .When(x => !string.IsNullOrEmpty(x.Code))
            .WithErrorCode(AuthenticationApiErrors.InvalidCodeFormatErrCode)
            .WithMessage(AuthenticationApiErrors.InvalidCodeFormatErrMessage);
        RuleFor(x => x.NewPassword).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyNewPasswordErrMessage);
        RuleFor(x => x.NewPasswordConfirm).Equal(x => x.NewPassword)
            .WithErrorCode(AuthenticationApiErrors.PasswordsDoNotMatchErrCode)
            .WithMessage(AuthenticationApiErrors.PasswordsDoNotMatchErrMessage);
    }
}
