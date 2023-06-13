using CarcassRightsDom;
using Microsoft.Extensions.Logging;

namespace BackendCarcassApi.Filters;

public class UserMustHaveRightsEditorRightsFilter : UserMenuRightsFilter
{
    public UserMustHaveRightsEditorRightsFilter(IUserRightsRepository repo, ILogger<UserMenuRightsFilter> logger) :
        base(new[] { "Rights" }, repo, logger)
    {
    }
}