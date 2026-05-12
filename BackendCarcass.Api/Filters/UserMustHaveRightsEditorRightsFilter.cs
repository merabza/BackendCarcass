using BackendCarcass.Identity;
using BackendCarcass.Rights;
using Microsoft.Extensions.Logging;
using SystemTools.SystemToolsShared;

namespace BackendCarcass.Api.Filters;

public sealed class UserMustHaveRightsEditorRightsFilter : UserMenuRightsFilter
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public UserMustHaveRightsEditorRightsFilter(IUserRightsRepository repo, IDatabaseAbstraction databaseAbstraction,
        ILogger<UserMustHaveRightsEditorRightsFilter> logger, ICurrentUser currentUser) : base(["Rights"], repo, logger,
        currentUser, databaseAbstraction)
    {
    }
}
