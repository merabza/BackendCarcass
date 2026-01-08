using CarcassIdentity;
using CarcassRights;
using Microsoft.Extensions.Logging;

namespace BackendCarcassApi.Filters;

public sealed class UserMustHaveRightsEditorRightsFilter : UserMenuRightsFilter
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public UserMustHaveRightsEditorRightsFilter(IUserRightsRepository repo, ILogger<UserMenuRightsFilter> logger,
        ICurrentUser currentUser) : base(["Rights"], repo, logger, currentUser)
    {
    }
}