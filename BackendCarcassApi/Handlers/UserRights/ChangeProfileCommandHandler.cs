using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.UserRights;
using BackendCarcassContracts.Errors;
using CarcassIdentity;
using CarcassMasterDataDom.Models;
using MediatR;
using MediatRMessagingAbstractions;
using Microsoft.AspNetCore.Identity;
using OneOf;
using SystemToolsShared.Errors;

namespace BackendCarcassApi.Handlers.UserRights;

// ReSharper disable once UnusedType.Global
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ChangeProfileCommandHandler : ICommandHandler<ChangeProfileCommandRequest>
{
    private readonly ICurrentUser _currentUser;
    private readonly UserManager<AppUser> _userMgr;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ChangeProfileCommandHandler(UserManager<AppUser> userMgr, ICurrentUser currentUser)
    {
        _userMgr = userMgr;
        _currentUser = currentUser;
    }

    public async Task<OneOf<Unit, Err[]>> Handle(ChangeProfileCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        //მოვძებნოთ მომხმარებელი მოწოდებული მომხმარებლის სახელით
        var user = await _userMgr.FindByNameAsync(_currentUser.Name);

        //თუ არ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user == null)
            return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        if (user.Id != request.Userid || user.UserName != request.UserName || user.Email != request.Email)
            return new[] { UserRightsErrors.UserNotIdentifierSaveFiled };

        user.FirstName = request.FirstName!;
        user.LastName = request.LastName!;
        var result = await _userMgr.UpdateAsync(user);
        //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
        return !result.Succeeded ? new[] { UserRightsErrors.FailedToSaveUserInformation } : new Unit();
    }
}