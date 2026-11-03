using BackendCarcass.Application.Authentication.ForgotPassword;
using BackendCarcassShared.Contracts.Errors;
using FluentValidation;

namespace BackendCarcass.Api.Validators;

// ReSharper disable once UnusedType.Global
public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordRequestCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.UserNameOrEmail).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyUserNameOrEmailErrMessage);
    }
}
