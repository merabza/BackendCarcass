using BackendCarcassApi.CommandRequests.Authentication;
using BackendCarcassContracts.Errors;
using FluentValidation;

namespace BackendCarcassApi.Validators;

// ReSharper disable once UnusedType.Global
public sealed class RegistrationCommandValidator : AbstractValidator<RegistrationCommandRequest>
{
    private const int UserNameMaxLength = 255;

    public RegistrationCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyEmailErrMessage).EmailAddress()
            .WithErrorCode(AuthenticationApiErrors.InvalidEmailAddressErrCode)
            .WithMessage(AuthenticationApiErrors.InvalidEmailAddressErrMessage);
        RuleFor(x => x.FirstName).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyFirstNameErrMessage).MaximumLength(50)
            .WithErrorCode(CarcassApiErrors.IsLongerThenErrCode)
            .WithMessage(AuthenticationApiErrors.NameIsLongerThenErrMessage);
        RuleFor(x => x.LastName).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyLastNameErrMessage).MaximumLength(100)
            .WithErrorCode(CarcassApiErrors.IsLongerThenErrCode)
            .WithMessage(AuthenticationApiErrors.LastNameIsLongerThenErrMessage);
        RuleFor(x => x.UserName).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyUserNameErrMessage).MaximumLength(UserNameMaxLength)
            .WithErrorCode(CarcassApiErrors.IsLongerThenErrCode)
            .WithMessage(AuthenticationApiErrors.UserNameIsLongerThenErr(UserNameMaxLength).ErrorCode);
        RuleFor(x => x.Password).NotEmpty().WithErrorCode(CarcassApiErrors.IsEmptyErrCode)
            .WithMessage(AuthenticationApiErrors.IsEmptyPasswordErrMessage);
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password)
            .WithErrorCode(AuthenticationApiErrors.PasswordsDoNotMatchErrCode)
            .WithMessage(AuthenticationApiErrors.PasswordsDoNotMatchErrMessage);
    }
}