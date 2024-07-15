using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.UserRights;
using BackendCarcassContracts.Errors;
using CarcassMasterDataDom.Models;
using MediatR;
using MessagingAbstractions;
using Microsoft.AspNetCore.Identity;
using OneOf;
using SystemToolsShared.Errors;

namespace BackendCarcassApi.Handlers.UserRights;

// ReSharper disable once UnusedType.Global
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ChangeProfileCommandHandler : ICommandHandler<ChangeProfileCommandRequest>
{
    private readonly UserManager<AppUser> _userMgr;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ChangeProfileCommandHandler(UserManager<AppUser> userMgr)
    {
        _userMgr = userMgr;
    }

    public async Task<OneOf<Unit, IEnumerable<Err>>> Handle(ChangeProfileCommandRequest request,
        CancellationToken cancellationToken)
    {
        //var userName = request.HttpContext.User.Identity?.Name;
        var currentUserName = request.HttpRequest.HttpContext.User.Identity!.Name!;
        //მოვძებნოთ მომხმარებელი მოწოდებული მომხმარებლის სახელით
        var user = await _userMgr.FindByNameAsync(currentUserName);

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