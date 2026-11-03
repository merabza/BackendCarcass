using BackendCarcass.Application.Authentication.ResetPassword;
using BackendCarcassShared.Contracts.Errors;
using FluentValidation;

namespace BackendCarcass.Api.Validators;

// ReSharper disable once UnusedType.Global
public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordRequestCommand>
{
    public ResetPasswordCommandValidator()
    {
        //ან ბმულის მონაცემები (UserId + Token), ან კოდის მონაცემები (UserNameOrEmail + Code)
        RuleFor(x => x).Must(HasLinkData).When(x => !HasCodeData(x))
            .WithErrorCode(AuthenticationApiErrors.ResetPasswordIdentificationErrCode)
            .WithMessage(AuthenticationApiErrors.ResetPasswordIdentificationErrMessage);
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

    private static bool HasLinkData(ResetPasswordRequestCommand command)
    {
        return command.UserId > 0 && !string.IsNullOrWhiteSpace(command.Token);
    }

    private static bool HasCodeData(ResetPasswordRequestCommand command)
    {
        return !string.IsNullOrWhiteSpace(command.UserNameOrEmail) && !string.IsNullOrWhiteSpace(command.Code);
    }
}
