using BackendCarcass.Identity;
using BackendCarcass.Rights;
using Microsoft.Extensions.Logging;
using SystemTools.DomainShared.Repositories;

namespace BackendCarcass.Api.Filters;

public sealed class UserMustHaveRightsEditorRightsFilter : UserMenuRightsFilter
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public UserMustHaveRightsEditorRightsFilter(IUserRightsRepository repo, IUnitOfWork unitOfWork,
        ILogger<UserMustHaveRightsEditorRightsFilter> logger, ICurrentUser currentUser) : base(["Rights"], repo,
        unitOfWork, logger, currentUser)
    {
    }
}
