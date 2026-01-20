using System.Linq;
using LanguageExt;
using Microsoft.AspNetCore.Identity;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Repositories;

public /*open*/ class IdentityCrudBase
{
    protected static Option<Err[]> ConvertError(IdentityResult result)
    {
        return result.Succeeded
            ? null
            : result.Errors.Select(x => new Err { ErrorCode = x.Code, ErrorMessage = x.Description }).ToArray();
    }
}