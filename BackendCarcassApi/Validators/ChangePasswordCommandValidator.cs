using BackendCarcassApi.CommandRequests.UserRights;
using BackendCarcassContracts.Errors;
using FluentValidation;

namespace BackendCarcassApi.Validators;

// ReSharper disable once UnusedType.Global
public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordRequestCommand>
{
    private const int UserNameMaxLength = 255;

    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyUserNameErrMessage).MaximumLength(UserNameMaxLength)
            .WithErrorCode(CarcassApiErrors.IsLongerThenErrCode)
            .WithMessage(AuthenticationApiErrors.UserNameIsLongerThenErr(UserNameMaxLength).ErrorMessage);
        RuleFor(x => x.OldPassword).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyOldPasswordErrMessage);
        RuleFor(x => x.NewPassword).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyNewPasswordErrMessage);
        RuleFor(x => x.NewPasswordConfirm).Equal(x => x.NewPassword)
            .WithErrorCode(AuthenticationApiErrors.PasswordsDoNotMatchErrCode)
            .WithMessage(AuthenticationApiErrors.PasswordsDoNotMatchErrMessage);
    }
}