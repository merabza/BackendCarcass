using CarcassRightsDom;
using Microsoft.Extensions.Logging;

namespace ServerCarcassMini.Filters;

public class UserMustHaveRightsEditorRightsFilter : UserMenuRightsFilter
{
    public UserMustHaveRightsEditorRightsFilter(IUserRightsRepository repo, ILogger<UserMenuRightsFilter> logger) :
        base(new[] { "Rights" }, repo, logger)
    {
    }
}