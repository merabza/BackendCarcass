using BackendCarcassApi.CommandRequests.UserRights;
using CarcassMasterDataDom.Models;
using MediatR;
using MessagingAbstractions;
using Microsoft.AspNetCore.Identity;
using OneOf;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassContracts.Errors;
using SystemToolsShared.Errors;

namespace BackendCarcassApi.Handlers.UserRights;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommandRequest>
{
    private readonly UserManager<AppUser> _userMgr;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ChangePasswordCommandHandler(UserManager<AppUser> userMgr)
    {
        _userMgr = userMgr;
    }

    public async Task<OneOf<Unit, IEnumerable<Err>>> Handle(ChangePasswordCommandRequest request,
        CancellationToken cancellationToken)
    {
        //მოვძებნოთ მომხმარებელი მოწოდებული მომხმარებლის სახელით
        var user = await _userMgr.FindByNameAsync(request.UserName!);
        //თუ არ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user == null)
            return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        var currentUserName = request.HttpRequest.HttpContext.User.Identity!.Name!;
        if (user.Id != request.Userid || currentUserName != user.UserName || currentUserName != request.UserName)
            return new[] { UserRightsErrors.UserAuthenticationFailedThePasswordHasNotBeenChanged };

        var result = await _userMgr.ChangePasswordAsync(user, request.OldPassword!, request.NewPassword!);
        //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
        return !result.Succeeded ? new[] { UserRightsErrors.FailedToChangePassword } : new Unit();
    }
}