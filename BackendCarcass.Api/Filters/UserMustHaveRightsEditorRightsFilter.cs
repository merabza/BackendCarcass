using BackendCarcass.Application.Identity;
using BackendCarcass.Application.Rights;
using Microsoft.Extensions.Logging;
using SystemTools.SystemToolsShared;

namespace BackendCarcass.Api.Filters;

public sealed class UserMustHaveRightsEditorRightsFilter : UserMenuRightsFilter
{
    public UserMustHaveRightsEditorRightsFilter(IUserRightsRepository repo, IDatabaseAbstraction databaseAbstraction,
        ILogger<UserMustHaveRightsEditorRightsFilter> logger, ICurrentUser currentUser) : base(["Rights"], repo, logger,
        currentUser, databaseAbstraction)
    {
    }
}
