using System.Collections.Generic;
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

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommandRequest>
{
    private readonly ICurrentUser _currentUser;
    private readonly UserManager<AppUser> _userMgr;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ChangePasswordCommandHandler(UserManager<AppUser> userMgr, ICurrentUser currentUser)
    {
        _userMgr = userMgr;
        _currentUser = currentUser;
    }

    public async Task<OneOf<Unit, IEnumerable<Err>>> Handle(ChangePasswordCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        //მოვძებნოთ მომხმარებელი მოწოდებული მომხმარებლის სახელით
        var user = await _userMgr.FindByNameAsync(request.UserName!);
        //თუ არ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user == null)
            return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        if (user.Id != request.Userid || _currentUser.Name != user.UserName)
            return new[] { UserRightsErrors.UserAuthenticationFailedThePasswordHasNotBeenChanged };

        var result = await _userMgr.ChangePasswordAsync(user, request.OldPassword!, request.NewPassword!);
        //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
        return !result.Succeeded ? new[] { UserRightsErrors.FailedToChangePassword } : new Unit();
    }
}