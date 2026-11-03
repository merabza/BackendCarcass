using System.Globalization;
using Microsoft.AspNetCore.Identity;

namespace BackendCarcass.Application.Identity;

//ASP.NET Identity-ს შეცდომების ქართული ტექსტები. კოდები უცვლელი რჩება
public sealed class GeorgianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError()
    {
        return new IdentityError { Code = nameof(DefaultError), Description = "უცნობი შეცდომა" };
    }

    public override IdentityError PasswordMismatch()
    {
        return new IdentityError { Code = nameof(PasswordMismatch), Description = "პაროლი არასწორია" };
    }

    public override IdentityError InvalidUserName(string? userName)
    {
        return new IdentityError
        {
            Code = nameof(InvalidUserName),
            Description = "მომხმარებლის სახელი შეიძლება შეიცავდეს მხოლოდ ასოებს, ციფრებს და სიმბოლოებს -._@+"
        };
    }

    public override IdentityError InvalidEmail(string? email)
    {
        return new IdentityError { Code = nameof(InvalidEmail), Description = "ელფოსტის მისამართი არასწორია" };
    }

    public override IdentityError DuplicateUserName(string userName)
    {
        return new IdentityError
        {
            Code = nameof(DuplicateUserName), Description = "მომხმარებელი ასეთი სახელით უკვე არსებობს"
        };
    }

    public override IdentityError DuplicateEmail(string email)
    {
        return new IdentityError
        {
            Code = nameof(DuplicateEmail), Description = "მომხმარებელი ასეთი ელფოსტის მისამართით უკვე არსებობს"
        };
    }

    public override IdentityError PasswordTooShort(int length)
    {
        return new IdentityError
        {
            Code = nameof(PasswordTooShort),
            Description = "პაროლი უნდა შედგებოდეს მინიმუმ " + length.ToString(CultureInfo.InvariantCulture) +
                          " სიმბოლოსგან"
        };
    }

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresUniqueChars),
            Description = "პაროლი უნდა შეიცავდეს მინიმუმ " + uniqueChars.ToString(CultureInfo.InvariantCulture) +
                          " განსხვავებულ სიმბოლოს"
        };
    }

    public override IdentityError PasswordRequiresNonAlphanumeric()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresNonAlphanumeric),
            Description = "პაროლი უნდა შეიცავდეს მინიმუმ ერთ სპეციალურ სიმბოლოს"
        };
    }

    public override IdentityError PasswordRequiresDigit()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresDigit), Description = "პაროლი უნდა შეიცავდეს მინიმუმ ერთ ციფრს (0-9)"
        };
    }

    public override IdentityError PasswordRequiresLower()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresLower), Description = "პაროლი უნდა შეიცავდეს მინიმუმ ერთ პატარა ასოს"
        };
    }

    public override IdentityError PasswordRequiresUpper()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresUpper), Description = "პაროლი უნდა შეიცავდეს მინიმუმ ერთ დიდ ასოს"
        };
    }
}
