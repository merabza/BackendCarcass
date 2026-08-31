using System.Linq;
using Microsoft.AspNetCore.Identity;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Repositories;

public /*open*/ class IdentityCrudBase
{
    protected static Result ConvertError(IdentityResult result)
    {
        return Result.Failure(Result.CreateValidationError([
            .. result.Errors.Select(s => Error.Problem(s.Code, s.Description))
        ]));
    }
}
