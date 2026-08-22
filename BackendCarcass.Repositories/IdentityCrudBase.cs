using System.Linq;
using LanguageExt;
using Microsoft.AspNetCore.Identity;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Repositories;

public /*open*/ class IdentityCrudBase
{
    protected static Option<ErrorOmd[]> ConvertError(IdentityResult result)
    {
        return result.Succeeded
            ? null
            : result.Errors.Select(x => new ErrorOmd { Code = x.Code, Name = x.Description }).ToArray();
    }
}
