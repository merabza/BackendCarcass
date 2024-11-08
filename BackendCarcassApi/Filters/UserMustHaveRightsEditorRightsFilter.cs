using CarcassDom;
using Microsoft.Extensions.Logging;

namespace BackendCarcassApi.Filters;

public class UserMustHaveRightsEditorRightsFilter : UserMenuRightsFilter
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public UserMustHaveRightsEditorRightsFilter(IUserRightsRepository repo, ILogger<UserMenuRightsFilter> logger) :
        base(["Rights"], repo, logger)
    {
    }
}