using System.Linq;
using Microsoft.AspNetCore.Identity;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.Repositories;

public /*open*/ class IdentityCrudBase
{
    protected static Result ConvertError(IdentityResult result)
    {
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(result.Errors.Select(x => new ErrorOmd { Code = x.Code, Name = x.Description }).ToArray()
                .ToError());
    }
}
