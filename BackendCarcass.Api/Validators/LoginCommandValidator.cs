using BackendCarcassContracts.Errors;
using Carcass.Application.Authentication.Login;
using FluentValidation;

namespace BackendCarcassApi.Validators;

// ReSharper disable once UnusedType.Global
public sealed class LoginCommandValidator : AbstractValidator<LoginRequestCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyUserNameErrMessage);
        RuleFor(x => x.Password).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyPasswordErrMessage);
    }
}