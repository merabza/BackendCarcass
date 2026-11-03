using System.Linq;
using Microsoft.AspNetCore.Identity;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Identity;

public static class IdentityResultExtensions
{
    //IdentityResult-ის შეცდომები Result-ის ვალიდაციის შეცდომად გადაიყვანება, რომ მომხმარებელმა ზუსტად ნახოს,
    //რა წესი დაირღვა. თუ Identity-მ შეცდომების სია არ დააბრუნა, fallback გამოიყენება
    public static Error ToError(this IdentityResult result, Error fallback)
    {
        Error[] errors = [.. result.Errors.Select(e => Error.Problem(e.Code, e.Description))];
        return errors.Length == 0 ? fallback : Result.CreateValidationError(errors);
    }
}
