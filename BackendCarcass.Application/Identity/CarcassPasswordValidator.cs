using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using Microsoft.AspNetCore.Identity;

namespace BackendCarcass.Application.Identity;

//პაროლის დამატებითი წესები, რომლებიც ASP.NET Identity-ს სტანდარტულ პარამეტრებში არ არის:
//  1. მინიმუმ ერთი ასო
//  2. არ უნდა შეიცავდეს მომხმარებლის სახელს, ან ელფოსტის მისამართის @-მდე ნაწილს
//  3. არ უნდა იყოს ყველაზე გავრცელებული პაროლების სიიდან
public sealed class CarcassPasswordValidator : IPasswordValidator<AppUser>
{
    //ამაზე მოკლე მომხმარებლის სახელი პაროლში ვერ ვერ გამოიძებნება სანდოდ (მაგალითად "ab")
    private const int MinIdentifierLength = 3;

    public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string? password)
    {
        string pwd = password ?? string.Empty;
        var errors = new List<IdentityError>();

        if (!pwd.Any(char.IsLetter))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordRequiresLetter", Description = "პაროლი უნდა შეიცავდეს მინიმუმ ერთ ასოს"
            });
        }

        if (ContainsIdentifier(pwd, user.UserName) || ContainsIdentifier(pwd, EmailLocalPart(user.Email)))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordContainsUserName",
                Description = "პაროლი არ უნდა შეიცავდეს მომხმარებლის სახელს, ან ელფოსტის მისამართს"
            });
        }

        if (CommonPasswords.Contains(pwd))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordIsTooCommon", Description = "ეს პაროლი ძალიან გავრცელებულია, აირჩიეთ სხვა"
            });
        }

        return Task.FromResult(errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed([.. errors]));
    }

    private static bool ContainsIdentifier(string password, string? identifier)
    {
        return identifier is not null && identifier.Length >= MinIdentifierLength &&
               password.Contains(identifier, StringComparison.OrdinalIgnoreCase);
    }

    private static string? EmailLocalPart(string? email)
    {
        if (email is null)
        {
            return null;
        }

        int atIndex = email.IndexOf('@', StringComparison.Ordinal);
        return atIndex > 0 ? email[..atIndex] : email;
    }
}
