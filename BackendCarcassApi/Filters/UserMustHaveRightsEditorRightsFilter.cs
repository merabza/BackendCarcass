using CarcassIdentity;
using CarcassRights;
using DomainShared.Repositories;
using Microsoft.Extensions.Logging;

namespace BackendCarcassApi.Filters;

public sealed class UserMustHaveRightsEditorRightsFilter : UserMenuRightsFilter
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public UserMustHaveRightsEditorRightsFilter(IUserRightsRepository repo, IUnitOfWork unitOfWork,
        ILogger<UserMenuRightsFilter> logger, ICurrentUser currentUser) : base(["Rights"], repo, unitOfWork, logger,
        currentUser)
    {
    }
}