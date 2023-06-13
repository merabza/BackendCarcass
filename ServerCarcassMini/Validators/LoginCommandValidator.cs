using CarcassContracts.ErrorModels;
using FluentValidation;
using ServerCarcassMini.CommandRequests.Authentication;

namespace ServerCarcassMini.Validators;

// ReSharper disable once UnusedType.Global
public sealed class LoginCommandValidator : AbstractValidator<LoginCommandRequest>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyUserNameErrMessage);
        RuleFor(x => x.Password).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyPasswordErrMessage);
    }
}