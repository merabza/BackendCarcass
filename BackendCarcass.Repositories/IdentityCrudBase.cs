using System.Linq;
using LanguageExt;
using Microsoft.AspNetCore.Identity;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Repositories;

public /*open*/ class IdentityCrudBase
{
    protected static Option<Error[]> ConvertError(IdentityResult result)
    {
        return result.Succeeded
            ? null
            : result.Errors.Select(x => new Error { Code = x.Code, Name = x.Description }).ToArray();
    }
}
